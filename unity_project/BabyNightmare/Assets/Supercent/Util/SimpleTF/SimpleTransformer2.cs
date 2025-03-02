using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supercent.Util.SimpleTF
{
    public class SimpleTransformer2 : MonoBehaviour
    {
        static readonly WaitForFixedUpdate WFFU = new WaitForFixedUpdate();


        [Flags] public enum EOption
        {
            Position = 1,
            Rotation = 2,
            Scale    = 4,
        }

        [System.Serializable] public class CurveInfo
        {
            public string Key;
            public AnimationCurve Curve;
        }

        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private List<SimpleTransformInfo> _infos;
        [SerializeField] private List<CurveInfo> _curves;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private bool _initedSet = false;

        private Dictionary<int, SimpleTransformInfo> _infoSet  = null;
        private Dictionary<int, CurveInfo>           _curveSet = null;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void Start() 
        {
            InitSet();
        }

        private void InitSet()
        {
            if (_initedSet)
                return;

            _initedSet = true;

            _infoSet?.Clear();
            _infoSet = new Dictionary<int, SimpleTransformInfo>();
            for (int i = 0, size = _infos.Count; i < size; ++i)
            {
                var c = _infos[i];
                if (null == c)
                    continue;

                _infoSet[c.Key.GetHashCode()] = c;
            }

            _curveSet?.Clear();
            _curveSet = new Dictionary<int, CurveInfo>();
            for (int i = 0, size = _curves.Count; i < size; ++i)
            {
                var c = _curves[i];
                if (null == c)
                    continue;

                _curveSet[c.Key.GetHashCode()] = c;
            }
        }
        
        //------------------------------------------------------------------------------
        // get
        //------------------------------------------------------------------------------
        public Transform GetTransform(string key)
        {
            if (null == _infoSet)
                InitSet();

            if (string.IsNullOrEmpty(key))
                return null;

            if (_infoSet.TryGetValue(key.GetHashCode(), out var info))
                return info.transform;

            return null;
        }

        public Vector3 GetPosition(string key)
        {
            if (null == _infoSet)
                InitSet();

            if (string.IsNullOrEmpty(key))
                return Vector3.zero;

            if (_infoSet.TryGetValue(key.GetHashCode(), out var info))
                return info.transform.position;

            return Vector3.zero;
        }

        public Vector3 GetRotation(string key)
        {
            if (null == _infoSet)
                InitSet();

            if (string.IsNullOrEmpty(key))
                return Vector3.zero;

            if (_infoSet.TryGetValue(key.GetHashCode(), out var info))
                return info.transform.eulerAngles;

            return Vector3.zero;
        }

        public Vector3 GetScale(string key)
        {
            if (null == _infoSet)
                InitSet();

            if (string.IsNullOrEmpty(key))
                return Vector3.zero;

            if (_infoSet.TryGetValue(key.GetHashCode(), out var info))
                return info.transform.localScale;

            return Vector3.zero;
        }

        public AnimationCurve GetCurve(string key)
        {
            if (null == _curveSet)
                InitSet();

            if (string.IsNullOrEmpty(key))
                return null;

            if (_curveSet.TryGetValue(key.GetHashCode(), out var info))
                return info.Curve;

            return null;
        }

        //------------------------------------------------------------------------------
        // shake
        //------------------------------------------------------------------------------
        public void Shake(Transform target, float duration, int shakingCount, float maxRange, float rangeRatio = 0.75f, bool toSmall = true)
        {
            if (null == _infoSet)
                InitSet();

            if (null == target || 0 == shakingCount)
                return;

            StartCoroutine(Co_Shake(target, duration, shakingCount, maxRange, rangeRatio, toSmall));
        }

        private IEnumerator Co_Shake(Transform target, float duration, int shakingCount, float maxRange, float rangeRatio, bool toSmall)
        {
            var interval = duration / (float)shakingCount;
            var range    = toSmall ? maxRange : CalcShakeToLargeRange(maxRange, rangeRatio, shakingCount);
            var orgPos   = target.position;

            for (int i = 0, size = shakingCount + 1; i < size; ++i)
            {
                var timer = 0f;
                var begin = target.position;
                var end   = i == shakingCount
                          ? orgPos
                          : Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up) * Vector3.right * range + orgPos;

                while (timer < interval)
                {
                    target.position = Vector3.Lerp(begin, end, timer / interval);

                    yield return null;
                    timer += Time.deltaTime;
                }

                if (toSmall)
                    range *= rangeRatio;
                else
                    range /= rangeRatio;
            }
        }

        private float CalcShakeToLargeRange(float range, float ratio, int count)
        {
            for (int i = 0; i < count; ++i)
                range *= ratio;

            return range;
        }

        //------------------------------------------------------------------------------
        // Change
        //------------------------------------------------------------------------------
        public void ChangeImmediate(Transform target, EOption option, string endKey)
        {
            if (null == _infoSet)
                InitSet();

            if (null == target)
            {
                Debug.LogError("[SimpleTransformer2.ChangeImmediate] 타겟을 찾을 수 없습니다.");
                return;
            }

            if (string.IsNullOrEmpty(endKey))
            {
                Debug.LogError("[SimpleTransformer2.ChangeImmediate] 변경할 정보의 키가 없습니다.");
                return;
            }

            if (!_infoSet.TryGetValue(endKey.GetHashCode(), out var info))
            {
                Debug.LogError($"[SimpleTransformer2.ChangeImmediate] 해당 키의 정보를 찾을 수 없습니다. key: {endKey}"); 
                return;
            }

            if (option.HasFlag(EOption.Position))
                target.transform.position = info.transform.position;

            if (option.HasFlag(EOption.Rotation))
                target.transform.rotation = info.transform.rotation;

            if (option.HasFlag(EOption.Scale))
                target.transform.localScale = info.transform.localScale;
        }

        public Coroutine Change(Transform target, EOption option, float duration, string endKey, string curveKey, Action doneCallback = null, bool isClamped = true)
        {
            if (null == _infoSet)
                InitSet();

            if (null == target)
            {
                Debug.LogError("[SimpleTransformer2.Change] 타겟을 찾을 수 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (string.IsNullOrEmpty(endKey))
            {
                Debug.LogError("[SimpleTransformer2.Change] 끝 정보의 키가 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (string.IsNullOrEmpty(curveKey))
            {
                Debug.LogError("[SimpleTransformer2.Change] 커브 정보의 키가 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (!_infoSet.TryGetValue(endKey.GetHashCode(), out var endInfo))
            {
                Debug.LogError($"[SimpleTransformer2.Change] 끝 정보를 찾을 수 없습니다. key: {endKey}");
                doneCallback?.Invoke();
                return null;
            }

            if (!_curveSet.TryGetValue(curveKey.GetHashCode(), out var curveInfo))
            {
                Debug.LogError($"[SimpleTransformer2.Change] 커브 정보를 찾을 수 없습니다. key: {curveKey}");
                doneCallback?.Invoke();
                return null;
            }

            // all
            if (option.HasFlag(EOption.Position) && option.HasFlag(EOption.Rotation) && option.HasFlag(EOption.Scale))
                return StartCoroutine(Co_ChangeAll(target, duration, target.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));
            
            // position && rotation
            if (option.HasFlag(EOption.Position) && option.HasFlag(EOption.Rotation))
                return StartCoroutine(Co_ChangePositionAndRotation(target, duration, target.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            // position && scale
            if (option.HasFlag(EOption.Position) && option.HasFlag(EOption.Scale))
                return StartCoroutine(Co_ChangePositionAndScale(target, duration, target.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            // rotation && scale
            if (option.HasFlag(EOption.Rotation) && option.HasFlag(EOption.Scale))
                return StartCoroutine(Co_ChangeRotationAndScale(target, duration, target.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            // position
            if (option.HasFlag(EOption.Position))
                return StartCoroutine(Co_ChangePosition(target, duration, target.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            // rotation
            if (option.HasFlag(EOption.Rotation))
                return StartCoroutine(Co_ChangeRotation(target, duration, target.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            // scale
            if (option.HasFlag(EOption.Scale))
                return StartCoroutine(Co_ChangeScale(target, duration, target.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            Debug.LogError($"[SimpleTransformer2.Change] 해당 옵션을 실행할 수 없습니다. option: {(int)option}");
            doneCallback?.Invoke();
            return null;
        }

        public Coroutine Change(Transform target, EOption option, float duration, string beginKey, string endKey, string curveKey, Action doneCallback = null, bool isClamped = true)
        {
            if (null == _infoSet)
                InitSet();

            if (null == target)
            {
                Debug.LogError("[SimpleTransformer2.Change] 타겟을 찾을 수 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (string.IsNullOrEmpty(beginKey))
            {
                Debug.LogError("[SimpleTransformer2.Change] 시작 정보의 키가 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (string.IsNullOrEmpty(endKey))
            {
                Debug.LogError("[SimpleTransformer2.Change] 끝 정보의 키가 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (string.IsNullOrEmpty(curveKey))
            {
                Debug.LogError("[SimpleTransformer2.Change] 커브 정보의 키가 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (!_infoSet.TryGetValue(beginKey.GetHashCode(), out var beginInfo))
            {
                Debug.LogError($"[SimpleTransformer2.Change] 시작 정보를 찾을 수 없습니다. key: {beginKey}");
                doneCallback?.Invoke();
                return null;
            }

            if (!_infoSet.TryGetValue(endKey.GetHashCode(), out var endInfo))
            {
                Debug.LogError($"[SimpleTransformer2.Change] 끝 정보를 찾을 수 없습니다. key: {endKey}");
                doneCallback?.Invoke();
                return null;
            }

            if (!_curveSet.TryGetValue(curveKey.GetHashCode(), out var curveInfo))
            {
                Debug.LogError($"[SimpleTransformer2.Change] 커브 정보를 찾을 수 없습니다. key: {curveKey}");
                doneCallback?.Invoke();
                return null;
            }

            // all
            if (option.HasFlag(EOption.Position) && option.HasFlag(EOption.Rotation) && option.HasFlag(EOption.Scale))
                return StartCoroutine(Co_ChangeAll(target, duration, beginInfo.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));
            
            // position && rotation
            if (option.HasFlag(EOption.Position) && option.HasFlag(EOption.Rotation))
                return StartCoroutine(Co_ChangePositionAndRotation(target, duration, beginInfo.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            // position && scale
            if (option.HasFlag(EOption.Position) && option.HasFlag(EOption.Scale))
                return StartCoroutine(Co_ChangePositionAndScale(target, duration, beginInfo.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            // rotation && scale
            if (option.HasFlag(EOption.Rotation) && option.HasFlag(EOption.Scale))
                return StartCoroutine(Co_ChangeRotationAndScale(target, duration, beginInfo.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            // position
            if (option.HasFlag(EOption.Position))
                return StartCoroutine(Co_ChangePosition(target, duration, beginInfo.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            // rotation
            if (option.HasFlag(EOption.Rotation))
                return StartCoroutine(Co_ChangeRotation(target, duration, beginInfo.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            // scale
            if (option.HasFlag(EOption.Scale))
                return StartCoroutine(Co_ChangeScale(target, duration, beginInfo.transform, endInfo.transform, curveInfo.Curve, doneCallback, isClamped));

            Debug.LogError($"[SimpleTransformer2.Change] 해당 옵션을 실행할 수 없습니다. option: {(int)option}");
            doneCallback?.Invoke();
            return null;
        }

        private IEnumerator Co_ChangeAll(Transform target, float duration, Transform begin, Transform end, AnimationCurve curve, Action doneCallback, bool isClamped = true)
        {
            var timer    = 0f;
            var targetTf = target.transform;
            var normal   = 0f;
            
            var beginPosition = begin.position;
            var beginRotation = begin.rotation;
            var beginScale = begin.localScale;

            while (timer < duration)
            {
                normal = curve.Evaluate(timer / duration);

                if (true == isClamped)
                {
                    targetTf.position = Vector3.Lerp(beginPosition, end.position, normal);
                    targetTf.rotation = Quaternion.Lerp(beginRotation, end.rotation, normal);
                    targetTf.localScale = Vector3.Lerp(beginScale, end.localScale, normal);
                }
                else
                {
                    targetTf.position = Vector3.LerpUnclamped(beginPosition, end.position, normal);
                    targetTf.rotation = Quaternion.LerpUnclamped(beginRotation, end.rotation, normal);
                    targetTf.localScale = Vector3.LerpUnclamped(beginScale, end.localScale, normal);
                }

                yield return null;
                timer += Time.deltaTime;
            }

            targetTf.position   = end.position;
            targetTf.rotation   = end.rotation;
            targetTf.localScale = end.localScale;

            doneCallback?.Invoke();
        }

        private IEnumerator Co_ChangePositionAndRotation(Transform target, float duration, Transform begin, Transform end, AnimationCurve curve, Action doneCallback, bool isClamped = true)
        {
            var timer    = 0f;
            var targetTf = target.transform;
            var normal   = 0f;

            var beginPosition = begin.position;
            var beginRotation = begin.rotation;

            while (timer < duration)
            {
                normal = curve.Evaluate(timer / duration);

                if (true == isClamped)
                {
                    targetTf.position = Vector3.Lerp(beginPosition, end.position, normal);
                    targetTf.rotation = Quaternion.Lerp(beginRotation, end.rotation, normal);
                }
                else
                {
                    targetTf.position = Vector3.LerpUnclamped(beginPosition, end.position, normal);
                    targetTf.rotation = Quaternion.LerpUnclamped(beginRotation, end.rotation, normal);
                }

                yield return null;
                timer += Time.deltaTime;
            }

            targetTf.position = end.position;
            targetTf.rotation = end.rotation;

            doneCallback?.Invoke();
        }

        private IEnumerator Co_ChangePositionAndScale(Transform target, float duration, Transform begin, Transform end, AnimationCurve curve, Action doneCallback, bool isClamped = true)
        {
            var timer    = 0f;
            var targetTf = target.transform;
            var normal   = 0f;

            var beginPosition = begin.position;
            var beginScale = begin.localScale;

            while (timer < duration)
            {
                normal = curve.Evaluate(timer / duration);

                if (true == isClamped)
                {
                    targetTf.position = Vector3.Lerp(beginPosition, end.position, normal);
                    targetTf.localScale = Vector3.Lerp(beginScale, end.localScale, normal);
                }
                else
                {
                    targetTf.position = Vector3.LerpUnclamped(beginPosition, end.position, normal);
                    targetTf.localScale = Vector3.LerpUnclamped(beginScale, end.localScale, normal);
                }

                yield return null;
                timer += Time.deltaTime;
            }

            targetTf.position   = end.position;
            targetTf.localScale = end.localScale;

            doneCallback?.Invoke();
        }

        private IEnumerator Co_ChangeRotationAndScale(Transform target, float duration, Transform begin, Transform end, AnimationCurve curve, Action doneCallback, bool isClamped = true)
        {
            var timer    = 0f;
            var targetTf = target.transform;
            var normal   = 0f;

            var beginRotation = begin.rotation;
            var beginScale = begin.localScale;

            while (timer < duration)
            {
                normal = curve.Evaluate(timer / duration);

                if (true == isClamped)
                {
                    targetTf.rotation = Quaternion.Lerp(beginRotation, end.rotation, normal);
                    targetTf.localScale = Vector3.Lerp(beginScale, end.localScale, normal);
                }
                else
                {
                    targetTf.rotation = Quaternion.LerpUnclamped(beginRotation, end.rotation, normal);
                    targetTf.localScale = Vector3.LerpUnclamped(beginScale, end.localScale, normal);
                }

                yield return null;
                timer += Time.deltaTime;
            }

            targetTf.rotation   = end.rotation;
            targetTf.localScale = end.localScale;

            doneCallback?.Invoke();
        }

        private IEnumerator Co_ChangePosition(Transform target, float duration, Transform begin, Transform end, AnimationCurve curve, Action doneCallback, bool isClamped = true)
        {
            var timer    = 0f;
            var targetTf = target.transform;
            var normal   = 0f;

            var beginPosition = begin.position;

            while (timer < duration)
            {
                normal = curve.Evaluate(timer / duration);

                if (true == isClamped)
                {
                    targetTf.position = Vector3.Lerp(beginPosition, end.position, normal);
                }
                else
                {
                    targetTf.position = Vector3.LerpUnclamped(beginPosition, end.position, normal);
                }

                yield return null;
                timer += Time.deltaTime;
            }

            targetTf.position = end.position;

            doneCallback?.Invoke();
        }

        private IEnumerator Co_ChangeRotation(Transform target, float duration, Transform begin, Transform end, AnimationCurve curve, Action doneCallback, bool isClamped = true)
        {
            var timer    = 0f;
            var targetTf = target.transform;
            var normal   = 0f;

            var beginRotation = begin.rotation;

            while (timer < duration)
            {
                normal = curve.Evaluate(timer / duration);

                if (true == isClamped)
                {
                    targetTf.rotation = Quaternion.Lerp(beginRotation, end.rotation, normal);
                }
                else
                {
                    targetTf.rotation = Quaternion.LerpUnclamped(beginRotation, end.rotation, normal);
                }

                yield return null;
                timer += Time.deltaTime;
            }

            targetTf.rotation   = end.rotation;

            doneCallback?.Invoke();
        }

        private IEnumerator Co_ChangeScale(Transform target, float duration, Transform begin, Transform end, AnimationCurve curve, Action doneCallback, bool isClamped = true)
        {
            var timer    = 0f;
            var targetTf = target.transform;
            var normal   = 0f;

            var beginScale = begin.localScale;

            while (timer < duration)
            {
                normal = curve.Evaluate(timer / duration);

                if (true == isClamped)
                {
                    targetTf.localScale = Vector3.Lerp(beginScale, end.localScale, normal);
                }
                else
                {
                    targetTf.localScale = Vector3.LerpUnclamped(beginScale, end.localScale, normal);
                }

                yield return null;
                timer += Time.deltaTime;
            }

            targetTf.localScale = end.localScale;

            doneCallback?.Invoke();
        }



        //------------------------------------------------------------------------------
        // bezier move
        //------------------------------------------------------------------------------
        public enum EUpdateOption
        {
            Update,
            FixedUpdate,
        }

        public Coroutine ChangePositionBezier(Transform target, EUpdateOption updateOption, float duration, string middleKey, string endKey, string curveKey, Action doneCallback = null)
        {
            if (null == _infoSet)
                InitSet();

            if (null == target)
            {
                Debug.LogError("[SimpleTransformer2.ChangeBezier] 타겟을 찾을 수 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (string.IsNullOrEmpty(middleKey))
            {
                Debug.LogError("[SimpleTransformer2.ChangeBezier] 중간 정보의 키가 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (string.IsNullOrEmpty(endKey))
            {
                Debug.LogError("[SimpleTransformer2.ChangeBezier] 끝 정보의 키가 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (string.IsNullOrEmpty(curveKey))
            {
                Debug.LogError("[SimpleTransformer2.ChangeBezier] 커브 정보의 키가 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (!_infoSet.TryGetValue(middleKey.GetHashCode(), out var middleInfo))
            {
                Debug.LogError($"[SimpleTransformer2.ChangeBezier] 중간 정보를 찾을 수 없습니다. key: {middleKey}");
                doneCallback?.Invoke();
                return null;
            }

            if (!_infoSet.TryGetValue(endKey.GetHashCode(), out var endInfo))
            {
                Debug.LogError($"[SimpleTransformer2.ChangeBezier] 끝 정보를 찾을 수 없습니다. key: {endKey}");
                doneCallback?.Invoke();
                return null;
            }

            if (!_curveSet.TryGetValue(curveKey.GetHashCode(), out var curveInfo))
            {
                Debug.LogError($"[SimpleTransformer2.ChangeBezier] 커브 정보를 찾을 수 없습니다. key: {curveKey}");
                doneCallback?.Invoke();
                return null;
            }

            return StartCoroutine(Co_ChangePositionBezier(target, duration, updateOption, target.transform, Vector3.zero, middleInfo.transform, Vector3.zero, endInfo.transform, Vector3.zero, curveInfo.Curve, doneCallback));
        }

        public Coroutine ChangeRangePositionBezier(Transform target, EUpdateOption updateOption, float duration, string middleKey, Vector3 middleRange, string endKey, Vector3 endRange, string curveKey, Action doneCallback = null)
        {
            if (null == _infoSet)
                InitSet();

            if (null == target)
            {
                Debug.LogError("[SimpleTransformer2.ChangeBezier] 타겟을 찾을 수 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (string.IsNullOrEmpty(middleKey))
            {
                Debug.LogError("[SimpleTransformer2.ChangeBezier] 중간 정보의 키가 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (string.IsNullOrEmpty(endKey))
            {
                Debug.LogError("[SimpleTransformer2.ChangeBezier] 끝 정보의 키가 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (string.IsNullOrEmpty(curveKey))
            {
                Debug.LogError("[SimpleTransformer2.ChangeBezier] 커브 정보의 키가 없습니다.");
                doneCallback?.Invoke();
                return null;
            }

            if (!_infoSet.TryGetValue(middleKey.GetHashCode(), out var middleInfo))
            {
                Debug.LogError($"[SimpleTransformer2.ChangeBezier] 중간 정보를 찾을 수 없습니다. key: {middleKey}");
                doneCallback?.Invoke();
                return null;
            }

            if (!_infoSet.TryGetValue(endKey.GetHashCode(), out var endInfo))
            {
                Debug.LogError($"[SimpleTransformer2.ChangeBezier] 끝 정보를 찾을 수 없습니다. key: {endKey}");
                doneCallback?.Invoke();
                return null;
            }

            if (!_curveSet.TryGetValue(curveKey.GetHashCode(), out var curveInfo))
            {
                Debug.LogError($"[SimpleTransformer2.ChangeBezier] 커브 정보를 찾을 수 없습니다. key: {curveKey}");
                doneCallback?.Invoke();
                return null;
            }

            return StartCoroutine(Co_ChangePositionBezier(target, duration, updateOption, target.transform, Vector3.zero, middleInfo.transform, middleRange, endInfo.transform, endRange, curveInfo.Curve, doneCallback));
        }

        private IEnumerator Co_ChangePositionBezier(Transform target, float duration, EUpdateOption updateOption, Transform begin, Vector3 beginRange, Transform middle, Vector3 middleRange, Transform end, Vector3 endRange, AnimationCurve curve, Action doneCallback)
        {
            var timer = 0f;
            var targetTf = target.transform;
            var normal = 0f;

            var startPosition = begin.position + new Vector3(UnityEngine.Random.Range(-beginRange.x * 0.5f, beginRange.x * 0.5f),
                                                             UnityEngine.Random.Range(-beginRange.y * 0.5f, beginRange.y * 0.5f),
                                                             UnityEngine.Random.Range(-beginRange.z * 0.5f, beginRange.z * 0.5f));

            var middlePosition = middle.position + new Vector3(UnityEngine.Random.Range(-middleRange.x * 0.5f, middleRange.x * 0.5f),
                                                               UnityEngine.Random.Range(-middleRange.y * 0.5f, middleRange.y * 0.5f),
                                                               UnityEngine.Random.Range(-middleRange.z * 0.5f, middleRange.z * 0.5f));


            var endPosition = end.position + new Vector3(UnityEngine.Random.Range(-endRange.x * 0.5f, endRange.x * 0.5f),
                                                         UnityEngine.Random.Range(-endRange.y * 0.5f, endRange.y * 0.5f),
                                                         UnityEngine.Random.Range(-endRange.z * 0.5f, endRange.z * 0.5f));


            while(timer < duration)
            {
                normal = curve.Evaluate(timer / duration);

                targetTf.position = LerpBezier(startPosition, middlePosition, endPosition, normal);

                if (updateOption == EUpdateOption.Update)
                {
                    yield return null;
                    timer += Time.deltaTime;
                }
                else
                {
                    yield return WFFU;
                    timer += Time.fixedDeltaTime;
                }
            }

            targetTf.position = endPosition;

            doneCallback?.Invoke();
        }

        private Vector3 LerpBezier(Vector3 start, Vector3 middle, Vector3 end, float delta)
        {
            return (1f - delta) * (1f - delta) * start +
                   2f * (1 - delta) * delta * middle +
                   delta * delta * end;
        }


        #if UNITY_EDITOR
        [ContextMenu("Get Info From Childs")]
        private void EDITOR_GetInfoFromChilds()
        {
            var infoComponents = this.GetComponentsInChildren<SimpleTransformInfo>(false);

            _infos?.Clear();
            _infos = new List<SimpleTransformInfo>();
            
            for (int i = 0; i < infoComponents.Length; ++i)
                _infos.Add(infoComponents[i]);
        }
        #endif
    }
}