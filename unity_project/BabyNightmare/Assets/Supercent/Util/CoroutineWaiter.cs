using System.Collections;
using UnityEngine;

namespace Supercent.Util
{
    public class CoroutineWaiter
    {
        private bool _isWait = true;

        public IEnumerator Wait()
        {
            while (_isWait)
            {
                yield return null;
            }
            _isWait = true;
        }

        public void Signal()
        {
            _isWait = false;
        }

        public IEnumerator Wait(float timeoutSec)
        {
            var started = Time.realtimeSinceStartup;
            while (_isWait && (Time.realtimeSinceStartup - started) < timeoutSec)
            {
                yield return null;
            }
            _isWait = true;
        }
    }
}