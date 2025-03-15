using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using SimpleJSON;
using BabyNightmare.StaticData;
using BabyNightmare.HUD;
using BabyNightmare.Util;
using Newtonsoft.Json;

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
        private const string PATH_LOBBY_BUTTON_SAVE_DATA = "lobby_button_save";
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
            Load();

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
            if (type == ELobbyButtonType.Unknown)
            {
                _printer.Log("SetMenu", $"Unknown 버튼 클릭 요청으로 갱신을 취소합니다.");
                return;
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
                case ELobbyButtonType.CustomShop:
                    {
                        //HUDManager.Instance.SetState(EHUDState.Show_Shortcut_On, "LobbyManager");
                        //CustomShopManager.Instance.Show(_lobbyView.ScreenRTF);
                        return;
                    }

                case ELobbyButtonType.Shop:
                    {
                        //HUDManager.Instance.SetState(EHUDState.Show_Shortcut_On, "LobbyManager");

                        // if (null != identifier && Enum.TryParse(identifier.ToString(), out EShopScrollType scrollType))
                        // {
                        //     ShopManager.Instance.Show(_lobbyView.ScreenRTF, scrollType);
                        //     return;
                        // }

                        // ShopManager.Instance.Show(_lobbyView.ScreenRTF);
                        return;
                    }
                case ELobbyButtonType.Home:
                    {
                        //HUDManager.Instance.SetState(EHUDState.Show_Shortcut_On, "LobbyManager");
                        _homeView.gameObject.SetActive(true);
                        _homeView.Refresh();
                        return;
                    }
                case ELobbyButtonType.Talent:
                    {
                        //HUDManager.Instance.SetState(EHUDState.Show_Shortcut_On, "LobbyManager");
                        //TalentManager.Instance.Show(_lobbyView.ScreenRTF);
                        return;
                    }
                case ELobbyButtonType.Mission:
                    {
                        // HUDManager.Instance.SetState(EHUDState.Show_Shortcut_On, "LobbyManager");
                        //MissionManager.Instance.Show(_lobbyView.ScreenRTF);
                        return;
                    }
            }

            throw new Exception($"{type}의 Show가 없습니다.");
        }

        private void HideView(ELobbyButtonType type)
        {
            switch (type)
            {
                case ELobbyButtonType.CustomShop:
                    {
                        // HUDManager.Instance.RevertState("LobbyManager");
                        //CustomShopManager.Instance.Hide(); 
                        return;
                    }
                case ELobbyButtonType.Shop:
                    {
                        // HUDManager.Instance.RevertState("LobbyManager");
                        //ShopManager.Instance.Hide();
                        return;
                    }
                case ELobbyButtonType.Home:
                    {
                        // HUDManager.Instance.RevertState("LobbyManager");
                        // _homeView.gameObject.SetActive(false);
                        return;
                    }
                case ELobbyButtonType.Talent:
                    {
                        // HUDManager.Instance.RevertState("LobbyManager");
                        //TalentManager.Instance.Hide();
                        return;
                    }
                case ELobbyButtonType.Mission:
                    {
                        // HUDManager.Instance.RevertState("LobbyManager");
                        //MissionManager.Instance.Hide();
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
                // case ELobbyButtonType.CustomShop: return CustomShopManager.Instance.IsAnyNew();
                // case ELobbyButtonType.Shop: return ShopUserData.IsExistFreeGem;
                // case ELobbyButtonType.Home: return false;
                // case ELobbyButtonType.Talent: return TalentManager.Instance.IsRedDot();
                // case ELobbyButtonType.Mission: return MissionManager.Instance.IsClaimMission;
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
            var lobbyButtonDataList = _menuButtonConfig.MenuButtonDataList;
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
                case ELobbyButtonType.Home:
                case ELobbyButtonType.Shop:
                case ELobbyButtonType.CustomShop:
                    return true;

                // case ELobbyButtonType.Talent: return !TalentManager.Instance.NeedGuide;
                // case ELobbyButtonType.Mission: return MissionManager.Instance.CompletedMissionViewShowCount > 0;
                // case ELobbyButtonType.StepUp: return StepUpManager.Instance.GuideDone;
                // case ELobbyButtonType.Luckyspin: return LuckyspinModel.Instance.IsExistAniSaveData;
                // case ELobbyButtonType.LeaderBoard: return LeaderBoardManager.Instance.ResetCount > 0;
                // case ELobbyButtonType.Christmas: return ChristmasEventManager.Instance.IsLive;
                // case ELobbyButtonType.NoAds: //다른 컨텐츠는 매니저가 관리해서 버튼이 생성되지 않는데 NoAds는 따로 관리하는 주체가 없어 Remote Check
                //     {
                //         var isActive = DataBase.Get<RemoteConfig>(RemoteConfig.NOADS_ON).value.IntValue() == 1;
                //         return true == isActive && false == User.NoAds && PlayerData.TotalISWatchCount > 0;
                //     }
                // case ELobbyButtonType.CoinBoost: return ConditionHelper.GetValue(EConditionType.TotalAttemptCount) >= 1;
                // case ELobbyButtonType.Restaurant: return RestaurantEventManager.Instance.IsLive;

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