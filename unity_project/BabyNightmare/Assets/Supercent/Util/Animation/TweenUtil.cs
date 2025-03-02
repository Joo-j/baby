using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.Util
{
    public static class TweenUtil
    {
        public readonly struct Token
        {
            public readonly int Value;
            public readonly TimeType TypeTime;
            public readonly double SecBegin;


            public Token(int value, TimeType typeTime, double secBegin)
            {
                Value = value;
                TypeTime = typeTime;
                SecBegin = secBegin;
            }

            public bool IsValid() => UpdateService.ContainsJob(Value);
            public bool Stop() => UpdateService.RemoveJob(Value);
        }

        public struct Params
        {
            public bool isYoyo;
            public TimeType timeType;
            public double secBegin;
            public double secDuration;
            public Func<float, float> timeModular;

            public Params(in Token token) : this(token.TypeTime, TIME[(int)token.TypeTime]()) { }
            public Params(in Token token, float secOffset) : this(token.TypeTime, TIME[(int)token.TypeTime]() + secOffset) { }
            public Params(TimeType timeType) : this(timeType, TIME[(int)timeType]()) { }
            public Params(TimeType timeType, double secBegin)
            {
                isYoyo = default;
                this.timeType = timeType;
                this.secBegin = secBegin;
                secDuration = default;
                timeModular = default;
            }

            public Func<double> GetTimer() => TIME[(int)timeType];
            public Params SetBeginNow()
            {
                secBegin = TIME[(int)timeType]();
                return this;
            }
            public Params SetCurve(AnimationCurve curve)
            {
                secDuration = curve.length;
                timeModular = curve.Evaluate;
                return this;
            }
        }

        public enum TimeType
        {
            Scale = 0,
            Unscale,
            FixedScale,
            FixedUnscale,
            Real,
        }

        static readonly Func<double>[] TIME = new Func<double>[]
        {
            () => Time.timeAsDouble,
            () => Time.unscaledTimeAsDouble,
            () => Time.fixedTimeAsDouble,
            () => Time.fixedUnscaledTimeAsDouble,
            () => Time.realtimeSinceStartupAsDouble,
        };

        public interface ITweenJob
        {
            bool IsInvalid { get; }
            void Set(float ratio);
            void SetStart();
            void SetEnd();
        }

        static readonly Exception Except_JobNull = new Exception($"{nameof(TweenUtil)} : job is null");
        static readonly Exception Except_ToNull = new Exception($"{nameof(TweenUtil)} : to is null");
        static readonly Exception Except_FromNull = new Exception($"{nameof(TweenUtil)} : form is null");
        static readonly Exception Except_InterpolateNull = new Exception($"{nameof(TweenUtil)} : interpolate is null");


        static float Linear(float t) => t;
        static float Yoyo(float t) => 1f - Math.Abs((t * 2f) - 1f);
        static float Repeat(double t, double length) => (float)(t % length / length);

        static IEnumerator TweenJob(bool isLoop, ITweenJob job, Params args)
        {
            if (args.timeModular == null)
                args.timeModular = Linear;

            var getTime = args.GetTimer();
            if (isLoop)
            {
                for (var secCur = args.secBegin; ; secCur = getTime())
                {
                    if (job.IsInvalid)
                        yield break;

                    var ratio = Repeat(secCur - args.secBegin, args.secDuration);
                    if (args.isYoyo)
                        ratio = Yoyo(ratio);
                    job.Set(args.timeModular(ratio));
                    yield return null;
                }
            }
            else
            {
                var secDone = args.secBegin + args.secDuration;
                for (var secCur = args.secBegin; secCur < secDone; secCur = getTime())
                {
                    if (job.IsInvalid)
                        yield break;

                    var ratio = (float)((secCur - args.secBegin) / args.secDuration);
                    if (args.isYoyo)
                        ratio = Yoyo(ratio);
                    job.Set(args.timeModular(ratio));
                    yield return null;
                }

                if (job.IsInvalid)
                    yield break;

                if (args.isYoyo)
                    job.SetStart();
                else
                    job.SetEnd();
            }
        }


        #region Manual - Job
        public static Token TweenManual(this ITweenJob job, bool isYoyo, float secDuration, Action<int> doneCallback = null) => TweenManual(job, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenManual(this ITweenJob job, Params args, Action<int> doneCallback = null)
        {
            if (job == null) throw Except_JobNull;

            var token = UpdateService.AddJob(TweenJob(false, job, args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }

        public static Token LoopManual(this ITweenJob job, bool isYoyo, float secDuration, Action<int> doneCallback = null) => LoopManual(job, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token LoopManual(this ITweenJob job, Params args, Action<int> doneCallback = null)
        {
            if (job == null) throw Except_JobNull;

            var token = UpdateService.AddJob(TweenJob(true, job, args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        #endregion// Manual - Job


        #region Manual - Type
        public static Token TweenManual<T>(this T to, bool isYoyo, float secDuration, Action<T, float> interpolate, Action<int> doneCallback = null) => TweenManual(to, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, interpolate, doneCallback);
        public static Token TweenManual<T>(this T to, Params args, Action<T, float> interpolate, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;
            if (interpolate == null) throw Except_InterpolateNull;

            var token = UpdateService.AddJob(TweenJob(false, new ManualJob<T>(to, interpolate), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }

        public static Token LoopManual<T>(this T to, bool isYoyo, float secDuration, Action<T, float> interpolate, Action<int> doneCallback = null) => LoopManual(to, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, interpolate, doneCallback);
        public static Token LoopManual<T>(this T to, Params args, Action<T, float> interpolate, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;
            if (interpolate == null) throw Except_InterpolateNull;

            var token = UpdateService.AddJob(TweenJob(true, new ManualJob<T>(to, interpolate), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }

        struct ManualJob<T> : ITweenJob
        {
            T to;
            Action<T, float> interpolate;

            public ManualJob(T to, Action<T, float> interpolate)
            {
                this.to = to;
                this.interpolate = interpolate;
            }

            public bool IsInvalid => to == null;
            public void Set(float ratio) => interpolate(to, ratio);
            public void SetStart() => Set(0f);
            public void SetEnd() => Set(1f);
        }
        #endregion// Manual - Job


        #region Position
        public static Token TweenPosition(this Component to, Component from, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenPosition(to, from, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, } ,doneCallback);
        public static Token TweenPosition(this Component to, Component from, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;
            if (from == null) throw Except_FromNull;

            var token = UpdateService.AddJob(TweenJob(false, new WorldPositionJob(to.transform, to.transform.position, from.transform.position), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenPosition(this Component to, Vector3 end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenPosition(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenPosition(this Component to, Vector3 end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new WorldPositionJob(to.transform, to.transform.position, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenPosition(this Component to, Vector3 start, Vector3 end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenPosition(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenPosition(this Component to, Vector3 start, Vector3 end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new WorldPositionJob(to.transform, start, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }

        public static Token TweenLocalPosition(this Component to, Vector3 end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenLocalPosition(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenLocalPosition(this Component to, Vector3 end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new LocalPositionJob(to.transform, to.transform.localPosition, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenLocalPosition(this Component to, Vector3 start, Vector3 end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenLocalPosition(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenLocalPosition(this Component to, Vector3 start, Vector3 end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new LocalPositionJob(to.transform, start, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }



        public static Token LoopPosition(this Component to, Component from, bool isYoyo, double secDuration) => LoopPosition(to, from, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopPosition(this Component to, Component from, Params args)
        {
            if (to == null) throw Except_ToNull;
            if (from == null) throw Except_FromNull;

            var token = UpdateService.AddJob(TweenJob(true, new WorldPositionJob(to.transform, to.transform.position, from.transform.position), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopPosition(this Component to, Vector3 end, bool isYoyo, double secDuration) => LoopPosition(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopPosition(this Component to, Vector3 end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new WorldPositionJob(to.transform, to.transform.position, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopPosition(this Component to, Vector3 start, Vector3 end, bool isYoyo, double secDuration) => LoopPosition(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopPosition(this Component to, Vector3 start, Vector3 end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new WorldPositionJob(to.transform, start, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }

        public static Token LoopLocalPosition(this Component to, Vector3 end, bool isYoyo, double secDuration) => LoopLocalPosition(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopLocalPosition(this Component to, Vector3 end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new LocalPositionJob(to.transform, to.transform.localPosition, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopLocalPosition(this Component to, Vector3 start, Vector3 end, bool isYoyo, double secDuration) => LoopLocalPosition(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopLocalPosition(this Component to, Vector3 start, Vector3 end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new LocalPositionJob(to.transform, start, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }

        readonly struct WorldPositionJob : ITweenJob
        {
            readonly Transform to;
            readonly Vector3 start, end, gap;

            public WorldPositionJob(Transform to, Vector3 start, Vector3 end)
            {
                this.to = to;
                this.start = start;
                this.end = end;
                gap = end - start;
            }

            public bool IsInvalid => to == null;
            public void Set(float ratio) => to.position = start + gap * ratio;
            public void SetStart() => to.position = start;
            public void SetEnd() => to.position = end;
        }

        readonly struct LocalPositionJob : ITweenJob
        {
            readonly Transform to;
            readonly Vector3 start, end, gap;

            public LocalPositionJob(Transform to, Vector3 start, Vector3 end)
            {
                this.to = to;
                this.start = start;
                this.end = end;
                gap = end - start;
            }

            public bool IsInvalid => to == null;
            public void Set(float ratio) => to.localPosition = start + gap * ratio;
            public void SetStart() => to.localPosition = start;
            public void SetEnd() => to.localPosition = end;
        }
        #endregion// Position


        #region Rotation
        public static Token TweenLookAt(this Component to, Component from, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenLookAt(to, from, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenLookAt(this Component to, Component from, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;
            if (from == null) throw Except_FromNull;

            var token = UpdateService.AddJob(TweenJob(false, new WorldRotationJob(to.transform, to.transform.rotation, Quaternion.LookRotation(from.transform.position - to.transform.position)), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }

        public static Token TweenLookAt(this Component to, Vector3 from, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenLookAt(to, from, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenLookAt(this Component to, Vector3 from, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;
            if (from == null) throw Except_FromNull;

            var token = UpdateService.AddJob(TweenJob(false, new WorldRotationJob(to.transform, to.transform.rotation, Quaternion.LookRotation(from - to.transform.position)), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }

        public static Token TweenLocalLookAt(this Component to, Vector3 from, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenLocalLookAt(to, from, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenLocalLookAt(this Component to, Vector3 from, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;
            if (from == null) throw Except_FromNull;

            var token = UpdateService.AddJob(TweenJob(false, new LocalRotationJob(to.transform, to.transform.localRotation, Quaternion.LookRotation(from - to.transform.localPosition)), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }



        public static Token LoopLookAt(this Component to, Component from, bool isYoyo, double secDuration) => LoopLookAt(to, from, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopLookAt(this Component to, Component from, Params args)
        {
            if (to == null) throw Except_ToNull;
            if (from == null) throw Except_FromNull;

            var token = UpdateService.AddJob(TweenJob(true, new WorldRotationJob(to.transform, to.transform.rotation, Quaternion.LookRotation(from.transform.position - to.transform.position)), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopLookAt(this Component to, Vector3 from, bool isYoyo, double secDuration) => LoopLookAt(to, from, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopLookAt(this Component to, Vector3 from, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new WorldRotationJob(to.transform, to.transform.rotation, Quaternion.LookRotation(from - to.transform.position)), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }

        public static Token LoopLocalLookAt(this Component to, Vector3 from, bool isYoyo, double secDuration) => LoopLocalLookAt(to, from, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopLocalLookAt(this Component to, Vector3 from, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new LocalRotationJob(to.transform, to.transform.localRotation, Quaternion.LookRotation(from - to.transform.localPosition)), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }


        public static Token TweenRotation(this Component to, Vector3 end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenRotation(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenRotation(this Component to, Vector3 end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new WorldRotationJob(to.transform, to.transform.rotation.eulerAngles, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenRotation(this Component to, Vector3 start, Vector3 end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenRotation(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenRotation(this Component to, Vector3 start, Vector3 end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new WorldRotationJob(to.transform, start, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenRotation(this Component to, Quaternion end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenRotation(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenRotation(this Component to, Quaternion end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new WorldRotationJob(to.transform, to.transform.rotation, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenRotation(this Component to, Quaternion start, Quaternion end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenRotation(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenRotation(this Component to, Quaternion start, Quaternion end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new WorldRotationJob(to.transform, start, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }

        public static Token TweenLocalRotation(this Component to, Vector3 end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenLocalRotation(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenLocalRotation(this Component to, Vector3 end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new LocalRotationJob(to.transform, to.transform.localRotation.eulerAngles, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenLocalRotation(this Component to, Vector3 start, Vector3 end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenLocalRotation(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenLocalRotation(this Component to, Vector3 start, Vector3 end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new LocalRotationJob(to.transform, start, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenLocalRotation(this Component to, Quaternion end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenLocalRotation(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenLocalRotation(this Component to, Quaternion end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new LocalRotationJob(to.transform, to.transform.localRotation, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenLocalRotation(this Component to, Quaternion start, Quaternion end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenLocalRotation(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenLocalRotation(this Component to, Quaternion start, Quaternion end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new LocalRotationJob(to.transform, start, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }



        public static Token LoopRotation(this Component to, Vector3 end, bool isYoyo, double secDuration) => LoopRotation(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopRotation(this Component to, Vector3 end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new WorldRotationJob(to.transform, to.transform.rotation.eulerAngles, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopRotation(this Component to, Vector3 start, Vector3 end, bool isYoyo, double secDuration) => LoopRotation(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopRotation(this Component to, Vector3 start, Vector3 end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new WorldRotationJob(to.transform, start, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopRotation(this Component to, Quaternion end, bool isYoyo, double secDuration) => LoopRotation(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopRotation(this Component to, Quaternion end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new WorldRotationJob(to.transform, to.transform.rotation, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopRotation(this Component to, Quaternion start, Quaternion end, bool isYoyo, double secDuration) => LoopRotation(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopRotation(this Component to, Quaternion start, Quaternion end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new WorldRotationJob(to.transform, start, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }

        public static Token LoopLocalRotation(this Component to, Vector3 end, bool isYoyo, double secDuration) => LoopLocalRotation(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopLocalRotation(this Component to, Vector3 end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new LocalRotationJob(to.transform, to.transform.localRotation.eulerAngles, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopLocalRotation(this Component to, Vector3 start, Vector3 end, bool isYoyo, double secDuration) => LoopLocalRotation(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopLocalRotation(this Component to, Vector3 start, Vector3 end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new LocalRotationJob(to.transform, start, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopLocalRotation(this Component to, Quaternion end, bool isYoyo, double secDuration) => LoopLocalRotation(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopLocalRotation(this Component to, Quaternion end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new LocalRotationJob(to.transform, to.transform.localRotation, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopLocalRotation(this Component to, Quaternion start, Quaternion end, bool isYoyo, double secDuration) => LoopLocalRotation(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopLocalRotation(this Component to, Quaternion start, Quaternion end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new LocalRotationJob(to.transform, start, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }

        readonly struct WorldRotationJob : ITweenJob
        {
            readonly Transform to;
            readonly Quaternion start, end;
            readonly Vector3 gap;

            public WorldRotationJob(Transform to, Vector3 start, Vector3 end)
            {
                this.to = to;
                this.start = Quaternion.Euler(start);
                this.end = Quaternion.Euler(end);
                gap = end - start;
            }
            public WorldRotationJob(Transform to, Quaternion start, Quaternion end)
            {
                this.to = to;
                this.start = start;
                this.end = end;
                gap = end.eulerAngles - start.eulerAngles;
            }

            public bool IsInvalid => to == null;
            public void Set(float ratio) => to.rotation = start * Quaternion.Euler(gap * ratio);
            public void SetStart() => to.rotation = start;
            public void SetEnd() => to.rotation = end;
        }

        readonly struct LocalRotationJob : ITweenJob
        {
            readonly Transform to;
            readonly Quaternion start, end;
            readonly Vector3 gap;

            public LocalRotationJob(Transform to, Vector3 start, Vector3 end)
            {
                this.to = to;
                this.start = Quaternion.Euler(start);
                this.end = Quaternion.Euler(end);
                gap = end - start;
            }
            public LocalRotationJob(Transform to, Quaternion start, Quaternion end)
            {
                this.to = to;
                this.start = start;
                this.end = end;
                gap = end.eulerAngles - start.eulerAngles;
            }

            public bool IsInvalid => to == null;
            public void Set(float ratio) => to.localRotation = start * Quaternion.Euler(gap * ratio);
            public void SetStart() => to.localRotation = start;
            public void SetEnd() => to.localRotation = end;
        }
        #endregion// Rotation


        #region Scale
        public static Token TweenScale(this Component to, Vector3 end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenScale(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenScale(this Component to, Vector3 end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new ScaleJob(to.transform, to.transform.localScale, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenScale(this Component to, Vector3 start, Vector3 end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenScale(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenScale(this Component to, Vector3 start, Vector3 end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new ScaleJob(to.transform, start, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }



        public static Token LoopScale(this Component to, Vector3 end, bool isYoyo, double secDuration) => LoopScale(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopScale(this Component to, Vector3 end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new ScaleJob(to.transform, to.transform.localScale, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopScale(this Component to, Vector3 start, Vector3 end, bool isYoyo, double secDuration) => LoopScale(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopScale(this Component to, Vector3 start, Vector3 end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new ScaleJob(to.transform, start, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }

        readonly struct ScaleJob : ITweenJob
        {
            readonly Transform to;
            readonly Vector3 start, end, gap;

            public ScaleJob(Transform to, Vector3 start, Vector3 end)
            {
                this.to = to;
                this.start = start;
                this.end = end;
                gap = end - start;
            }

            public bool IsInvalid => to == null;
            public void Set(float ratio) => to.localScale = start + gap * ratio;
            public void SetStart() => to.localScale = start;
            public void SetEnd() => to.localScale = end;
        }
        #endregion// Scale


        #region Color - Material
        public static Token TweenColor(this Material to, Color end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenColor(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenColor(this Material to, Color end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new MaterialColorJob(to, to.color, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenColor(this Material to, Color start, Color end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenColor(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenColor(this Material to, Color start, Color end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new MaterialColorJob(to, start, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }



        public static Token LoopColor(this Material to, Color end, bool isYoyo, double secDuration) => LoopColor(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopColor(this Material to, Color end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new MaterialColorJob(to, to.color, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopColor(this Material to, Color start, Color end, bool isYoyo, double secDuration) => LoopColor(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopColor(this Material to, Color start, Color end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new MaterialColorJob(to, start, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }

        readonly struct MaterialColorJob : ITweenJob
        {
            readonly Material to;
            readonly Color start, end, gap;

            public MaterialColorJob(Material to, Color start, Color end)
            {
                this.to = to;
                this.start = start;
                this.end = end;
                gap = end - start;
            }

            public bool IsInvalid => to == null;
            public void Set(float ratio) => to.color = start + gap * ratio;
            public void SetStart() => to.color = start;
            public void SetEnd() => to.color = end;
        }
        #endregion// Color


        #region Color - SpriteRenderer
        public static Token TweenColor(this SpriteRenderer to, Color end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenColor(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenColor(this SpriteRenderer to, Color end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new SpriteColorJob(to, to.color, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenColor(this SpriteRenderer to, Color start, Color end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenColor(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenColor(this SpriteRenderer to, Color start, Color end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new SpriteColorJob(to, start, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }



        public static Token LoopColor(this SpriteRenderer to, Color end, bool isYoyo, double secDuration) => LoopColor(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopColor(this SpriteRenderer to, Color end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new SpriteColorJob(to, to.color, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopColor(this SpriteRenderer to, Color start, Color end, bool isYoyo, double secDuration) => LoopColor(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopColor(this SpriteRenderer to, Color start, Color end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new SpriteColorJob(to, start, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }

        readonly struct SpriteColorJob : ITweenJob
        {
            readonly SpriteRenderer to;
            readonly Color start, end, gap;

            public SpriteColorJob(SpriteRenderer to, Color start, Color end)
            {
                this.to = to;
                this.start = start;
                this.end = end;
                gap = end - start;
            }

            public bool IsInvalid => to == null;
            public void Set(float ratio) => to.color = start + gap * ratio;
            public void SetStart() => to.color = start;
            public void SetEnd() => to.color = end;
        }
        #endregion// Color - SpriteRenderer


        #region Color - Graphic
        public static Token TweenColor(this Graphic to, Color end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenColor(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenColor(this Graphic to, Color end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new GraphicColorJob(to, to.color, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token TweenColor(this Graphic to, Color start, Color end, bool isYoyo, double secDuration, Action<int> doneCallback = null) => TweenColor(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, }, doneCallback);
        public static Token TweenColor(this Graphic to, Color start, Color end, Params args, Action<int> doneCallback = null)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(false, new GraphicColorJob(to, start, end), args), doneCallback);
            return new Token(token, args.timeType, args.secBegin);
        }



        public static Token LoopColor(this Graphic to, Color end, bool isYoyo, double secDuration) => LoopColor(to, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopColor(this Graphic to, Color end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new GraphicColorJob(to, to.color, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }
        public static Token LoopColor(this Graphic to, Color start, Color end, bool isYoyo, double secDuration) => LoopColor(to, start, end, new Params(TimeType.Scale) { isYoyo = isYoyo, secDuration = secDuration, });
        public static Token LoopColor(this Graphic to, Color start, Color end, Params args)
        {
            if (to == null) throw Except_ToNull;

            var token = UpdateService.AddJob(TweenJob(true, new GraphicColorJob(to, start, end), args), null);
            return new Token(token, args.timeType, args.secBegin);
        }

        readonly struct GraphicColorJob : ITweenJob
        {
            readonly Graphic to;
            readonly Color start, end, gap;

            public GraphicColorJob(Graphic to, Color start, Color end)
            {
                this.to = to;
                this.start = start;
                this.end = end;
                gap = end - start;
            }

            public bool IsInvalid => to == null;
            public void Set(float ratio) => to.color = start + gap * ratio;
            public void SetStart() => to.color = start;
            public void SetEnd() => to.color = end;
        }
        #endregion// Color
    }
}
