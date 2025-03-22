using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using SimpleJSON;
using BabyNightmare.StaticData;
using BabyNightmare.HUD;
using BabyNightmare.Util;
using BabyNightmare.Talent;
using Newtonsoft.Json;
using BabyNightmare.CustomShop;

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

        private const string PATH_MENU_BUTTON_CONFIG = "Lobby/LobbyMenuConfig";
        private const string PATH_LOBBY_BUTTON_SAVE_DATA = "lobby_button_save";
        private const string PATH_LOBBY_VIEW = "Lobby/LobbyView";
        private const string PATH_HOME_VIEW = "Lobby/HomeView";

        private LobbyMenuConfig _menuConfig = null;
        private LobbyView _lobbyView = null;
        private HomeView _homeView = null;
        private ELobbyButtonType _focusButtonType = ELobbyButtonType.Unknown;
        private Dictionary<ELobbyButtonType, ELobbyButtonState> _buttonStateDict = new Dictionary<ELobbyButtonType, ELobbyButtonState>();
        private bool _isEnterSequenceDone = false;
        private LogClassPrinter _printer = new LogClassPrinter("LobbyManager", "333331");

        public ELobbyButtonType FocusButtonType => _focusButtonType;

        public void Init(Action startGame)
        {
            Load();

            _menuConfig = Resources.Load<LobbyMenuConfig>(PATH_MENU_BUTTON_CONFIG);
            if (null == _menuConfig)
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

            var menuButtonDataList = _menuConfig.MenuButtonDataList;
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

            RefreshButtonState();

            GameLifeCycle.Start_Coroutine(Co_EnterSequence());
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

        private void RefreshButtonState()
        {
            foreach (ELobbyButtonType type in Enum.GetValues(typeof(ELobbyButtonType)))
            {
                if (type == ELobbyButtonType.Unknown)
                    continue;

                if (true == IsUnlocked(type))
                {
                    _buttonStateDict[type] = ELobbyButtonState.Unlocked;
                }

                if (_buttonStateDict[type] == ELobbyButtonState.Onboarding)
                {
                    OpenButton(type, true);
                    GuideMenuButton(type);
                    continue;
                }

                if (_buttonStateDict[type] == ELobbyButtonState.Unlocked)
                {
                    OpenButton(type, true);
                }
            }
        }

        private IEnumerator Co_EnterSequence()
        {
            _printer.Log("Co_EnterSequence", $"Co_EnterSequence - Begin");
            var waiter = new CoroutineWaiter();

            HUDManager.Instance.SetState(EHUDState.Show_Shortcut_Off, "LobbyManager");
            _lobbyView.ActiveInteract(false);

            var orderList = new List<LobbyButtonData>();
            foreach (ELobbyButtonType type in Enum.GetValues(typeof(ELobbyButtonType)))
            {
                if (type == ELobbyButtonType.Unknown)
                    continue;

                var state = _buttonStateDict[type];
                var data = StaticDataManager.Instance.GetLobbyButtonData(type);
                if (null == data)
                    continue;

                if (state != ELobbyButtonState.Locked)
                    continue;

                if (false == IsOpenable(data))
                    continue;

                orderList.Add(data);
            }

            orderList.Sort();

            for (var i = 0; i < orderList.Count; i++)
            {
                var data = orderList[i];
                OpenButton(data.ButtonType, !data.ShowOpenAni, waiter.Signal);
                yield return waiter.Wait();

                _buttonStateDict[data.ButtonType] = ELobbyButtonState.Onboarding;
                Save();
            }

            for (var i = 0; i < orderList.Count; i++)
            {
                var data = orderList[i];
                GuideMenuButton(data.ButtonType);
            }

            //HUDManager.Instance.RevertState("LobbyManager");
            _lobbyView.ActiveInteract(true);

            _isEnterSequenceDone = true;
            _printer.Log("Co_EnterSequence", $"Co_EnterSequence - Done");
        }


        public void SetMenu(ELobbyButtonType type, object identifier = null, Action doneCallback = null)
        {
            if (type == ELobbyButtonType.Unknown || _focusButtonType == type)
                return;

            if (_focusButtonType != ELobbyButtonType.Unknown)
                HideView(_focusButtonType);

            ShowView(type, identifier);

            _lobbyView.FocusButton(type);

            _focusButtonType = type;

            if (_buttonStateDict[type] != ELobbyButtonState.Locked)
            {
                _buttonStateDict[type] = ELobbyButtonState.Unlocked;
                Save();
            }

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
                case ELobbyButtonType.CustomShop:
                    {
                        CustomShopManager.Instance.Show(_lobbyView.ScreenRTF);
                        return;
                    }
                case ELobbyButtonType.Home:
                    {
                        var chapter = PlayerData.Instance.Chapter;
                        var chapterData = StaticDataManager.Instance.GetChapterData(chapter);

                        _homeView.gameObject.SetActive(true);
                        _homeView.Refresh(chapterData);
                        return;
                    }
                case ELobbyButtonType.Talent:
                    {
                        TalentManager.Instance.Show(_lobbyView.ScreenRTF);
                        return;
                    }
                case ELobbyButtonType.Mission:
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
                case ELobbyButtonType.Shop:
                    {
                        return;
                    }
                case ELobbyButtonType.CustomShop:
                    {
                        CustomShopManager.Instance.Hide();
                        return;
                    }
                case ELobbyButtonType.Home:
                    {
                        _homeView.gameObject.SetActive(false);
                        return;
                    }
                case ELobbyButtonType.Talent:
                    {
                        TalentManager.Instance.Hide();
                        return;
                    }
                case ELobbyButtonType.Mission:
                    {
                        return;
                    }

                default:
                    Debug.LogError($"{type}의 Hide가 없습니다.");
                    return;
            }
        }

        private bool CheckRedDot(ELobbyButtonType type)
        {
            switch (type)
            {
                case ELobbyButtonType.CustomShop: return false;
                case ELobbyButtonType.Shop: return false;
                case ELobbyButtonType.Home: return false;
                case ELobbyButtonType.Talent: return TalentManager.Instance.IsRedDot();
                case ELobbyButtonType.Mission: return false;
                default: return false;
            }
        }

        private void GuideMenuButton(ELobbyButtonType type)
        {
            if (false == TryGetMenuButtonData(type, out var lobbyButtonData))
                return;

            if (false == lobbyButtonData.ShowGuide)
                return;

            ShowGuide(type, lobbyButtonData.Guide_Force);
        }

        public void ShowGuide(ELobbyButtonType type, bool force) => _lobbyView.ShowGuide(type, force);
        public void ClearGuide() => _lobbyView.ClearGuide();

        private void OpenButton(ELobbyButtonType type, bool immediate, Action doneCallback = null)
        {
            //_printer.Log("OpenButton", $"{type} {immediate}");

            if (true == TryGetMenuButtonData(type, out var lobbyButtonData))
                _lobbyView.OpenButton(type, immediate, doneCallback);
            else
                _homeView.OpenButton(type, immediate, () => _isEnterSequenceDone, doneCallback);
        }

        private bool TryGetMenuButtonData(ELobbyButtonType type, out MenuButtonData lobbyButtonData)
        {
            lobbyButtonData = null;
            var lobbyButtonDataList = _menuConfig.MenuButtonDataList;
            for (var i = 0; i < lobbyButtonDataList.Count; i++)
            {
                lobbyButtonData = lobbyButtonDataList[i];
                if (lobbyButtonData.Type == type)
                    return true;
            }

            lobbyButtonData = null;
            return false;
        }

        private bool IsUnlocked(ELobbyButtonType buttonType)
        {
            switch (buttonType)
            {
                case ELobbyButtonType.Home: return true;
                case ELobbyButtonType.Shop: return false;
                case ELobbyButtonType.CustomShop: return CustomShopManager.Instance.ShowCount > 0;
                case ELobbyButtonType.Talent: return TalentManager.Instance.ShowCount > 0;
                case ELobbyButtonType.Mission: return false;

                default: return false;
            }
        }

        private bool IsOpenable(LobbyButtonData data)
        {
            if (data.ConditionType == EConditionType.Unknown)
            {
                Debug.LogError($"{data.ButtonType}의 컨디션이 Unknown입니다.");
                return false;
            }

            var conditionValue = ConditionHelper.GetValue(data.ConditionType);
            var comparison_type = data.ComparisonType;
            var value = 0;

            if (false == int.TryParse(data.ConditionValue, out value))
            {
                Debug.LogError($"{data.ConditionValue}을 int형으로 파싱할 수 없습니다.");
                return false;
            }

            var correct = ConditionHelper.IsCorrect(comparison_type, conditionValue, value);
            if (false == correct)
                return false;

            return true;
        }

        private void Save()
        {
            var binaryData = JsonConvert.SerializeObject(_buttonStateDict);
            FileSaveUtil.Save(PATH_LOBBY_BUTTON_SAVE_DATA, binaryData, false, false);
        }

        private void Load()
        {
            var buttonTypes = Enum.GetValues(typeof(ELobbyButtonType));

            var binaryData = FileSaveUtil.Load(PATH_LOBBY_BUTTON_SAVE_DATA, false, false);
            if (true == binaryData.IsNullOrEmpty())
            {
                InitStateDict(buttonTypes); //저장 데이터가 없을 시 초기화 후 리턴
                return;
            }

            var tempDict = JsonConvert.DeserializeObject<Dictionary<string, ELobbyButtonState>>(binaryData);
            foreach (var pair in tempDict)
            {
                var type_string = pair.Key;
                if (false == Enum.TryParse<ELobbyButtonType>(type_string, out var type))
                    continue;

                var state = pair.Value;

                _buttonStateDict.Add(type, state);
            }

            InitStateDict(buttonTypes); //저장된 데이터를 파싱한 이후에 초기화 하여 추가된 타입 대응

            void InitStateDict(Array buttonTypes)
            {
                foreach (ELobbyButtonType buttonType in buttonTypes)
                {
                    if (buttonType == ELobbyButtonType.Unknown)
                        continue;

                    if (true == _buttonStateDict.ContainsKey(buttonType))
                        continue;

                    var state = IsUnlocked(buttonType) ? ELobbyButtonState.Unlocked : ELobbyButtonState.Locked;

                    _buttonStateDict.Add(buttonType, state);
                }
            }
        }
    }
}