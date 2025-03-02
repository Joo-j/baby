using System;
using System.Diagnostics;
using UnityEngine;

namespace Supercent.Util
{
    public struct BenchmarkUtil
    {
        const string TAG = nameof(BenchmarkUtil);
        static ILogger unityLogger => UnityEngine.Debug.unityLogger;

        static void Log(string msg) => unityLogger.Log(LogType.Log, msg);
        static void LogAssert(string msg) => unityLogger.Log(LogType.Assert, msg);


        public static void Check(string tag, uint count, Action job)
        {
            if (job == null)
                LogAssert($"[{TAG}] {tag} : {nameof(job)} is null");
            else
            {
                var stamp = Stopwatch.GetTimestamp();
                for (uint index = 0; index < count; ++index)
                    job();
                Record(tag, count, stamp);
            }
        }
        public static void Check(string tag, uint count, Action<uint> job)
        {
            if (job == null)
                LogAssert($"[{TAG}] {tag} : {nameof(job)} is null");
            else
            {
                var stamp = Stopwatch.GetTimestamp();
                for (uint index = 0; index < count; ++index)
                    job(index);
                Record(tag, count, stamp);
            }
        }

        public static void Check<T>(string tag, uint count, T arg, Action<T> job)
        {
            if (job == null)
                LogAssert($"[{TAG}] {tag} : {nameof(job)} is null");
            else
            {
                var stamp = Stopwatch.GetTimestamp();
                for (uint index = 0; index < count; ++index)
                    job(arg);
                Record(tag, count, stamp);
            }
        }
        public static void Check<T>(string tag, uint count, T arg, Action<uint, T> job)
        {
            if (job == null)
                LogAssert($"[{TAG}] {tag} : {nameof(job)} is null");
            else
            {
                var stamp = Stopwatch.GetTimestamp();
                for (uint index = 0; index < count; ++index)
                    job(index, arg);
                Record(tag, count, stamp);
            }
        }

        static void Record(string tag, uint count, long stamp)
        {
            var ticks = Stopwatch.GetTimestamp() - stamp;
            var ms = (long)(ticks / (Stopwatch.Frequency * 0.001));
            Log($"[{TAG}] {tag} : {ms} ms, {ticks} ticks, {count} times, isHighResolution {Stopwatch.IsHighResolution}");
        }


        public static long GetStamp() => Stopwatch.GetTimestamp();
        public static long CheckStamp(string tag, long stamp)
        {
            var curStamp = Stopwatch.GetTimestamp();
            var ticks = curStamp - stamp;
            var ms = (long)(ticks / (Stopwatch.Frequency * 0.001));
            Log($"[{TAG}] {tag} : {ms} ms, {ticks} ticks, isHighResolution {Stopwatch.IsHighResolution}");
            return curStamp;
        }
    }
}