using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.StaticData;

namespace BabyNightmare.Lobby
{
    public class LobbyManager : SingletonBase<LobbyManager>
    {
        enum ELobbyButtonState
        {
            Locked,
            Onboarding,
            Unlocked,
        }

        private const string PATH_MENU_BUTTON_CONFIG = "Lobby/MenuButtonConfig";
        private const string PATH_LOBBY_VIEW = "Lobby/LobbyView";
        private const string PATH_HOME_VIEW = "Lobby/HomeView";

        private MenuButtonConfig _menuButtonConfig = null;
        private LobbyView _lobbyView = null;
        private HomeView _homeView = null;
        private ELobbyButtonType _focusButtonType = ELobbyButtonType.Unknown;
        private Dictionary<ELobbyButtonType, ELobbyButtonState> _buttonStateDict = new Dictionary<ELobbyButtonType, ELobbyButtonState>();
        private LogClassPrinter _printer = new LogClassPrinter("LobbyManager", "333331");
        private bool _isEnterSequenceDone = false;

        public ELobbyButtonType FocusButtonType => _focusButtonType;

        public void Init(Action startGame)
        {
            _menuButtonConfig = Resources.Load<MenuButtonConfig>(PATH_MENU_BUTTON_CONFIG);
            if (null == _menuButtonConfig)
            {
                _printer.Error("Init", $"{PATH_MENU_BUTTON_CONFIG}에 프리팹이 없습니다.");
                return;
            }

            _lobbyView = ObjectUtil.LoadAndInstantiate<LobbyView>(PATH_LOBBY_VIEW, null);
            if (null == _lobbyView)
            {
                _printer.Error("Init", $"{PATH_LOBBY_VIEW}에 프리팹이 없습니다.");
                return;
            }

            var menuButtonDataList = _menuButtonConfig.MenuButtonDataList;
            var menuButtonTypeList = new List<ELobbyButtonType>();
            for (var i = 0; i < menuButtonDataList.Count; i++)
                menuButtonTypeList.Add(menuButtonDataList[i].Type);

            _lobbyView.Init(menuButtonTypeList, (type) => SetMenu(type, null), CheckRedDot);
            GameObject.DontDestroyOnLoad(_lobbyView.gameObject);

            _homeView = ObjectUtil.LoadAndInstantiate<HomeView>(PATH_HOME_VIEW, _lobbyView.ScreenRTF);
            if (null == _homeView)
            {
                _printer.Error("Init", $"{PATH_HOME_VIEW}에 프리팹이 없습니다.");
                return;
            }

            _homeView.Init(startGame);

            (_homeView.transform as RectTransform).SetFullStretch();
        }

        public void Enter()
        {
            _focusButtonType = ELobbyButtonType.Unknown;
            _lobbyView.gameObject.SetActive(true);

            SetMenu(ELobbyButtonType.Home);
        }

        public void Exit()
        {
            if (_focusButtonType != ELobbyButtonType.Unknown)
            {
                HideView(_focusButtonType);
                _focusButtonType = ELobbyButtonType.Unknown;
            }

            _lobbyView.gameObject.SetActive(false);

            _isEnterSequenceDone = false;
        }

        public void SetMenu(ELobbyButtonType type, object identifier = null, Action doneCallback = null)
        {
            if (type == ELobbyButtonType.Unknown)
            {
                _printer.Log("SetMenu", $"Unknown 버튼 클릭 요청으로 갱신을 취소합니다.");
                return;
            }

            if (type == _focusButtonType)
            {
                switch (type)
                {
                    case ELobbyButtonType.Shop:

                        // if (null != identifier && Enum.TryParse(identifier.ToString(), out EShopScrollType scrollType))
                        // {
                        //     ShopManager.Instance.Show(_lobbyView.ScreenRTF, scrollType);
                        // }

                        return;
                    default:
                        _printer.Log("SetMenu", $"{type} 같은 타입의 버튼 클릭 요청으로 갱신을 취소합니다.");
                        return;
                }
            }

            if (_focusButtonType != ELobbyButtonType.Unknown)
                HideView(_focusButtonType);

            ShowView(type, identifier);

            _lobbyView.FocusButton(type);

            _focusButtonType = type;

            _buttonStateDict[type] = ELobbyButtonState.Unlocked;

            doneCallback?.Invoke();
        }

        private void ShowView(ELobbyButtonType type, object identifier)
        {
            switch (type)
            {
                case ELobbyButtonType.Shop:
                    {
                        return;

                    }
                case ELobbyButtonType.Home:
                    {
                        return;

                    }
                case ELobbyButtonType.Talent:
                    {
                        return;
                    }
            }

            throw new Exception($"{type}의 Show가 없습니다.");
        }

        private void HideView(ELobbyButtonType type)
        {
            switch (type)
            {
                default:
                    return;
            }
        }

        private bool CheckRedDot(ELobbyButtonType type)
        {
            switch (type)
            {
                default: return false;
            }
        }
    }
}