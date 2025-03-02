using System.Collections.Generic;
using UnityEngine;

namespace Supercent.Util.SimpleFSM
{
    public class SimpleStateMachine<TKey>
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        protected bool _isInited = false;

        protected MonoBehaviour _coroutineOwner = null;
        protected Dictionary<TKey, SimpleState<TKey>> _stateSet = null;
        protected SimpleState<TKey> _currentState = null;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public bool IsInited => _isInited;

        public virtual SimpleState<TKey> CurrentState => _currentState;

        public virtual TKey CurrentStateKey
        {
            get
            {
                if (null == _currentState)
                    return default;

                return _currentState.StateKey;
            }
        }

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public virtual void Init(Dictionary<TKey, SimpleState<TKey>> stateSet, 
                                 MonoBehaviour coroutineOwner, 
                                 object initParam)
        {
            Release();

            if (null == stateSet)
            {
                Debug.LogError("[SimpleFSM.SimpleStateMachine.Init] 사용할 상태가 하나도 없습니다.");
                return;
            }

            if (null == coroutineOwner)
            {
                Debug.LogError("[SimpleFSM.SimpleStateMachine.Init] coroutineOwner 가 없습니다. 상태 내부에서 코루틴을 사용하지 못할 수 있습니다.");
                return;
            }

            _stateSet = stateSet;

            var iter = _stateSet.GetEnumerator();
            while (iter.MoveNext())
            {
                if (null == iter.Current.Value)
                    continue;

                iter.Current.Value.Init(coroutineOwner, ChangeState, initParam);
            }
        }

        public virtual void Update()
        {
            _currentState?.Update();
        }

        public virtual void Release()
        {
            _isInited       = false;
            _currentState   = null;
            _coroutineOwner = null;

            if (null != _stateSet)
            {
                foreach (var c in _stateSet.Values)
                    c.Release();
                
                _stateSet.Clear();
                _stateSet = null;
            }
        }

        public virtual void ChangeState(TKey key, object param = null)
        {
            if (null == _stateSet)
            {
                Debug.LogError("[SimpleFSM.SimpleStateMachine.ChangeState] 사용할 상태가 하나도 없습니다.");
                return;
            }
            
            if (!_stateSet.TryGetValue(key, out var state))
            {
                Debug.LogError($"[SimpleFSM.SimpleStateMachine.ChangeState] 변경할 상태를 찾을 수 없습니다. 변경할 상태: {key.ToString()}");
                return;
            }

            if (null == state)
            {
                Debug.LogError($"[SimpleFSM.SimpleStateMachine.ChangeState] 변경할 상태가 정상이 아닙니다. 변경할 상태: {key.ToString()}");
                return;
            }

            var preStateKey = default(TKey);
            if (null != _currentState)
            {
                preStateKey = _currentState.StateKey;
                _currentState.Finish(state.StateKey);
            }

            _currentState = state;
            _currentState.Start(preStateKey, param);
        }
    }
}