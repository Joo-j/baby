using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Supercent.Util
{
    using JobContainer = Dictionary<int, (IEnumerator, Action<int>)>;

    public sealed class UpdateService : MonoBehaviour
    {
        const int ArrayLimit = int.MaxValue - 56;

        static UpdateService instance = null;
        static readonly JobContainer jobs = new JobContainer();
        static int[] keys = new int[4];

        static int GetOptimalCapacity(int count)
        {
            if (count < 4) return 4;
            if (ArrayLimit <= count) return ArrayLimit;

            int round = 0;
            uint number = (uint)count;
            while (1 < number)
            {
                ++round;
                number = number >> 1;
            }

            number = number << round;
            if (number < count)
                number = number << 1;
            return number < ArrayLimit ? (int)number : ArrayLimit;
        }



        void Awake()
        {
            if (instance == null)
            {
                DontDestroyOnLoad(gameObject);
                instance = this;
                return;
            }

            Debug.LogAssertion($"{nameof(UpdateService)} already exists");
            UnityObject.Destroy(this);
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        void Update() => UpdateJob();

        static void UpdateJob()
        {
            var cntJob = jobs.Count;
            if (0 < cntJob)
            {
                if (keys.Length < cntJob)
                    keys = new int[GetOptimalCapacity(cntJob)];

                jobs.Keys.CopyTo(keys, 0);
                _IterateJob(0);
            }


            void _IterateJob(int _index)
            {
                try
                {
                    for (int _cnt = Math.Min(keys.Length, cntJob); _index < _cnt; ++_index)
                    {
                        var _key = keys[_index];
                        if (jobs.TryGetValue(_key, out var _job)
                         && !_job.Item1.MoveNext())
                        {
                            jobs.Remove(_key);
                            _job.Item2?.Invoke(_key);
                        }
                    }
                    return;
                }
                catch (Exception _error)
                {
                    Debug.LogException(_error);
                    if (-1 < _index && _index < keys.Length)
                        jobs.Remove(keys[_index]);
                }

                _IterateJob(++_index);
            }
        }



        public static bool ContainsJob(int key) => jobs.ContainsKey(key);

        public static int AddJob(IEnumerator job, Action<int> callback)
        {
            if (job == null)
                throw new Exception($"{nameof(UpdateService)} : job is null");

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif// UNITY_EDITOR
            {
                if (instance == null)
                {
                    var obj = new GameObject(nameof(UpdateService));
                    DontDestroyOnLoad(obj);
                    instance = obj.AddComponent<UpdateService>();
                }
            }
#if UNITY_EDITOR
            else
            {
                UnityEditor.EditorApplication.update -= UpdateJob;
                UnityEditor.EditorApplication.update += UpdateJob;
            }
#endif// UNITY_EDITOR

            var key = job.GetHashCode();
            if (job.MoveNext())
                jobs[key] = (job, callback);
            else
                callback?.Invoke(key);
            return key;
        }

        public static bool RemoveJob(int key) => jobs.Remove(key);
    }
}