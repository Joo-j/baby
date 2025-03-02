using System.Collections;
using UnityEngine;

namespace Supercent.Util.SimpleFSM
{
    public abstract class SimpleState<TKey>
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private MonoBehaviour _coroutineOwner = null;
        private System.Action<TKey, object> _onChangeState = null;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public abstract TKey StateKey { get; }

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public void Init(MonoBehaviour coroutineOwner, System.Action<TKey, object> onChangeState, object initParam = null)
        {
            Release();

            _coroutineOwner = coroutineOwner;
            _onChangeState  = onChangeState;

            _Init(initParam);
        }

        public void Release()
        {
            _coroutineOwner = null;
            _onChangeState  = null;

            _Release();
        }

        public void Start(TKey preStateKey, object startParam = null)
        {
            _Start(preStateKey, startParam);
        }

        public void Finish(TKey nextStateKey)
        {
            _Finish(nextStateKey);
        }

        public void Update()
        {
            _Update();
        }

        protected virtual void _Init(object initParam) {}
        protected virtual void _Release() {}
        protected virtual void _Start(TKey preStateKey, object startParam) {}
        protected virtual void _Finish(TKey nextStateKey) {}
        protected virtual void _Update() {}

        protected Coroutine StartCoroutine(IEnumerator func)
        {
            if (null == _coroutineOwner)
            {
                Debug.LogError($"[SimpleFSM.SimpleState.{nameof(StartCoroutine)}] 코루틴을 사용할 수 있는 상태가 아닙니다.");
                return null;
            }

            return _coroutineOwner.StartCoroutine(func);
        }

        protected void SwapCoroutine(ref Coroutine target, IEnumerator func)
        {
            if (null == _coroutineOwner)
            {
                Debug.LogError($"[SimpleFSM.SimpleState.{nameof(SwapCoroutine)}] 코루틴을 사용할 수 있는 상태가 아닙니다.");
                return;
            }

            if (target != null)
                _coroutineOwner.StopCoroutine(target);
            target = func == null ? null : _coroutineOwner.StartCoroutine(func);
        }

        protected void StopCoroutine(Coroutine coroutine)
        {
            if (null == coroutine || null == _coroutineOwner)
                return;

            _coroutineOwner.StopCoroutine(coroutine);
        }

        protected void ReleaseCoroutine(ref Coroutine coroutine)
        {
            if (coroutine != null)
                _coroutineOwner.StopCoroutine(coroutine);
            coroutine = null;
        }

        protected void ChangeState(TKey key, object param = null)
        {
            _onChangeState?.Invoke(key, param);
        }
    }
}