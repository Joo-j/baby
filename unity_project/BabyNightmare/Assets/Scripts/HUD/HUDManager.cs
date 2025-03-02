using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;

namespace BabyNightmare.HUD
{
    public enum EHUDType
    {
        Coin,
        Gem,
        EventCurrency,
    }

    public enum EHUDState
    {
        Show_Shortcut_Off,
        Show_Shortcut_On,
        Hide,
    }

    public class HUDManager
    {
        private static HUDManager _instance = null;
        public static HUDManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new HUDManager();
                }

                return _instance;
            }
        }

        private const string PATH_HUD_VIEW = "HUD/HUDView";

        private Stack<EHUDState> _stateStack = new Stack<EHUDState>();
        private EHUDState _currentState = EHUDState.Hide;
        private HUDView _hudView = null;

        public void Init()
        {
            _hudView = ObjectUtil.LoadAndInstantiate<HUDView>(PATH_HUD_VIEW, null);
            if (null == _hudView)
            {
                Debug.LogError($"HUDView {PATH_HUD_VIEW}에 프리팹이 없습니다.");
                return;
            }

            _hudView.Init();
            GameObject.DontDestroyOnLoad(_hudView.gameObject);

            Clear();
        }

        public void SetState(EHUDState state, string caller)
        {
            _stateStack.Push(_currentState);

            _currentState = state;
            Apply(state);
        }

        public void RevertState(string caller)
        {
            if (_stateStack.Count <= 0)
            {
                return;
            }

            var state = _stateStack.Pop();

            _currentState = state;
            Apply(state);
        }

        private void Apply(EHUDState state)
        {
            if (null == _hudView || null == _hudView.gameObject)
                return;

            switch (state)
            {
                case EHUDState.Show_Shortcut_Off:
                    _hudView.Show(false);
                    break;
                case EHUDState.Show_Shortcut_On:
                    _hudView.Show(true);
                    break;
                case EHUDState.Hide:
                    _hudView.Hide();
                    break;
            }
        }

        public void Clear()
        {
            _stateStack.Clear();
            _currentState = EHUDState.Hide;
            _hudView.Hide();
        }

        public void ActiveHUD(EHUDType type, bool active) => _hudView.ActiveHUD(type, active);
    }
}