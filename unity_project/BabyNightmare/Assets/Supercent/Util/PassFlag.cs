using System.Collections;
using UnityEngine;

namespace Supercent.Util
{
    public class PassFlag
    {
        public enum EResult
        {
            None = 0,
            Timeout,
            Succeed,
            Failed,
        }

        private EResult _result;

        public EResult Result => _result;
        public bool IsPassed => _result == EResult.Succeed;


        public void Reset()
        {
            _result = EResult.None;
        }

        public void SetPassed(bool succeedOrFailed)
        {
            _result = succeedOrFailed ? EResult.Succeed : EResult.Failed;
        }

        public IEnumerator Wait(string log = null)
        {
            while (_result == EResult.None)
            {
                if (log != null)
                    Debug.Log(log);
                yield return null;
            }
        }

        public IEnumerator Wait(float timeoutSec, string log = null)
        {
            var started = Time.realtimeSinceStartup;
            while (_result == EResult.None && (Time.realtimeSinceStartup - started) < timeoutSec)
            {
                if (log != null)
                    Debug.Log(log);
                yield return null;
            }

            if (_result == EResult.None)
            {
                _result = EResult.Timeout;
            }
        }

    }
}