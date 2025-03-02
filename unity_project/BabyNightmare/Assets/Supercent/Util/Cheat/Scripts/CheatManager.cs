using System;
using System.Collections.Generic;
using System.Linq;
using Supercent.Util.Cheat.MetaData;
using UnityEngine;

namespace Supercent.Util.Cheat
{
    public class CheatManager<T_Group> where T_Group : Enum
    {
        //------------------------------------------------------------------------------
        // const
        //------------------------------------------------------------------------------
        private const string APP_INFO_NAME = "AppInfo";
        //------------------------------------------------------------------------------
        // singleton
        //------------------------------------------------------------------------------
        private static CheatManager<T_Group> _instance = null;
        private static bool _isInit = false;
        public static CheatManager<T_Group> Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                _instance = new CheatManager<T_Group>();
                _instance.Init();
                return _instance;
            }
        }

        private CheatManager() {}

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private CheatMetaData _metaData = null;
        private CheatView _cheatView;
        private Dictionary<int, ICheatItem> _cheatItems = new Dictionary<int, ICheatItem>();  // cheatItemKey(런타임 _cheatAutoIndex), CheatItem
        private Dictionary<string, HashSet<int>> _cheatGroups = new Dictionary<string, HashSet<int>>();  // groupName, 치트아이템 묶음
        private int _cheatAutoIndex = 1;
        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void Init()
        {
            if (_isInit)
            {
                Debug.LogError("CheatManager 싱글턴은 이미 Initialize 되었습니다.");
                return;
            }

            LoadMetaData();

            InitCheatGroup();

            InitCheatObject();

            InitAppInfo();

            _isInit = true;
        }

        private void LoadMetaData()
        {
            _metaData = CheatMetaData.CreateMetaData();
        }

        private void InitCheatGroup()
        {
            _cheatGroups.Add(APP_INFO_NAME, new HashSet<int>());
            foreach (T_Group groupType in Enum.GetValues(typeof(T_Group)))
            {
                _cheatGroups.Add(groupType.ToString(), new HashSet<int>());
            }
        }

        private void InitAppInfo()
        {
            Func<string, string> productNameDelegate = (param) => { return Application.productName; };
            RegisterLabel(APP_INFO_NAME, "Product Id", "게임 이름입니다.", productNameDelegate);

            Func<string, string> buildVersionDelegate = (param) => { return Application.version; };
            RegisterLabel(APP_INFO_NAME, "BuildVersion", "게임 빌드 버전 정보입니다.", buildVersionDelegate);
        }

        private int Register(string groupName, string name, string description, Delegate onCheatExecute, E_CheatType cheatType)
        {
            if (false == IsValidCheatItem(groupName, name))
                return -1;

            Func<string, string> onExecute = (status) =>
            {
                var returnValue = onCheatExecute.DynamicInvoke(status);
                SaveStatus(groupName, name, status);
                return returnValue.ToString();
            };

            var cheatItem = CreateCheatItem(cheatType, name, description, onExecute);
            if (null == cheatItem)
            {
                Debug.LogError("등록되지 않는 치트 타입입니다.");
                return -1;
            }
            _cheatAutoIndex++;
            
            _cheatItems.Add(cheatItem.Id, cheatItem);
            _cheatGroups[groupName].Add(cheatItem.Id);
            
            return cheatItem.Id;
        }

        public void UnRegister(int cheatItemId)
        {
            if (_cheatItems.ContainsKey(cheatItemId))
                _cheatItems.Remove(cheatItemId);
            foreach (var groupName in _cheatGroups.Keys)
            {
                if (_cheatGroups[groupName].Contains(cheatItemId))
                    _cheatGroups[groupName].Remove(cheatItemId);
            }
        }

        public void UnRegisterAll()
        {
            foreach (var groupName in _cheatGroups.Keys)
            {
                _cheatGroups[groupName].Clear();
            }
            _cheatGroups.Clear();
        }

        private bool IsValidCheatItem(string groupName, string cheatName)
        {
            if (false == _cheatGroups.TryGetValue(groupName, out var groupCheatItems))
            {
                Debug.LogError($"해당 그룹이 초기화되지 않았습니다.groupType({groupName})Name({cheatName})");
                return false;
            }

            foreach (var cheatItemId in groupCheatItems)
            {
                if (false == _cheatItems.ContainsKey(cheatItemId))
                    continue;

                var cheatItem = _cheatItems[cheatItemId];
                var name = cheatItem.Name;
                if (name == cheatName)
                {
                    Debug.LogError($"해당 그룹에 동일한 이름의 치트가 존재합니다.groupType({groupName})Name({cheatName})");
                    return false;
                }
            }

            return true;
        }

        private ICheatItem CreateCheatItem(E_CheatType cheatType, string name, string description, Func<string, string> onExecute)
        {
            ICheatItem cheatItem;
            switch (cheatType)
            {
                case E_CheatType.Button:
                    cheatItem = new CheatItem<string>();
                    cheatItem.Init(_cheatAutoIndex, name, description, cheatType, onExecute);
                    return cheatItem;
                case E_CheatType.CheckBox:
                    cheatItem = new CheatItem<bool>();
                    cheatItem.Init(_cheatAutoIndex, name, description, cheatType, onExecute);
                    return cheatItem;
                case E_CheatType.InputField:
                    cheatItem = new CheatItem<string>();
                    cheatItem.Init(_cheatAutoIndex, name, description, cheatType, onExecute);
                    return cheatItem;
                case E_CheatType.Slider:
                    cheatItem = new CheatItem<float>();
                    cheatItem.Init(_cheatAutoIndex, name, description, cheatType, onExecute);
                    return cheatItem;
                case E_CheatType.Label:
                    cheatItem = new CheatItem<string>();
                    cheatItem.Init(_cheatAutoIndex, name, description, cheatType, onExecute);
                    return cheatItem;
            }
            return null;
        }

        private string GetStatus(string groupName, string cheatName)
        {
            var status = _metaData.GetStatus(groupName, cheatName);
            if (!string.IsNullOrEmpty(status))
                return status;

            return string.Empty;
        }

        private void SaveStatus(string groupName, string name, string status)
        {
            _metaData.SetStatus(groupName, name, status);
        }

        private string GetSelectGroupName()
        {
            var groups = _cheatGroups.Keys.ToArray();
            var selectIndex = Mathf.Clamp(_metaData.LastSelectGroupIndex, 0, groups.Length - 1);
            return groups[selectIndex];
        }

        #region view
        private void InitCheatObject()
        {
            var obj = new GameObject("[CheatManager]");
            GameObject.DontDestroyOnLoad(obj);
            
            _cheatView = ObjectUtil.LoadAndInstantiate<CheatView>("CheatView", obj.transform);
            var groupTypeArr = _cheatGroups.Keys.ToArray();
            _cheatView.Init(groupTypeArr, OnClickGroupButton, _metaData.LastSelectGroupIndex);
        }

        private int OnClickGroupButton(int groupIndex)
        {
            _metaData.LastSelectGroupIndex = groupIndex;

            var curSelectGroupName = GetSelectGroupName();
            ShowGroupCheatItemList(curSelectGroupName);
            return _metaData.LastSelectGroupIndex;
        }

        private void ShowGroupCheatItemList(string groupName)
        {
            _cheatView.RemoveCheatItemViews();

            var cheatItems = _cheatGroups[groupName];
            foreach (var cheatItemId in cheatItems)
            {
                if (!_cheatItems.ContainsKey(cheatItemId))
                    continue;
                var cheatItem = _cheatItems[cheatItemId];
                var cheatType = cheatItem.CheatType;
                var status = GetStatus(groupName, cheatItem.Name);
                _cheatView.CreateCheatItemView(cheatItem, status, cheatType);
            }
        }
        #endregion view

        #region register methods
        public int RegisterButton(T_Group groupType, string name, string description, Action onCheatExecute)
        {
            Func<string,string> onExecute = (status) => 
            {
                onCheatExecute?.Invoke();
                return string.Empty;
            };
            return RegisterButton(groupType, name, description, onExecute);
        }

        public int RegisterButton(T_Group groupType, string name, string description, Delegate onCheatExecute)
        {
            return Register(groupType.ToString(), name, description, onCheatExecute, E_CheatType.Button);
        }

        public int RegisterCheckBox(T_Group groupType, string name, string description, Action<bool> onCheatExecute)
        {
            Func<string, string> onExecute = (status) =>
            {
                if (!bool.TryParse(status, out var statusBool))
                {
                    Debug.LogError($"bool으로 형변환에 실패했습니다.");
                    return string.Empty;
                }

                var resultValue = onCheatExecute.DynamicInvoke(statusBool);

                return resultValue != null ? resultValue.ToString() : string.Empty;
            };
            return RegisterCheckBox(groupType, name, description, onExecute);
        }

        public int RegisterCheckBox(T_Group groupType, string name, string description, Delegate onCheatExecute)
        {
            return Register(groupType.ToString(), name, description, onCheatExecute, E_CheatType.CheckBox);
        }

        public int RegisterInputField(T_Group groupType, string name, string description, Action<string> onCheatExecute)
        {
            Func<string,string> onExecute = (status) => 
            {
                onCheatExecute?.Invoke(status);
                return string.Empty;
            };
            return RegisterInputField(groupType, name, description, onExecute);
        }
        
        public int RegisterInputField(T_Group groupType, string name, string description, Delegate onCheatExecute)
        {
            return Register(groupType.ToString(), name, description, onCheatExecute, E_CheatType.InputField);
        }
        
        public int RegisterSlider(T_Group groupType, string name, string description, Action<float> onCheatExecute)
        {
            Func<string, string> onExecute = (status) =>
            {
                if (!float.TryParse(status, out var statusFloat))
                {
                    Debug.LogError($"float으로 형변환에 실패했습니다.");
                    return string.Empty;
                }

                onCheatExecute.DynamicInvoke(statusFloat);
                return string.Empty;
            };
            return RegisterSlider(groupType, name, description, onExecute);
        }

        public int RegisterSlider(T_Group groupType, string name, string description, Delegate onCheatExecute)
        {
            return Register(groupType.ToString(), name, description, onCheatExecute, E_CheatType.Slider);
        }

        public int RegisterLabel(T_Group groupType, string name, string description, Func<string> onCheatExecute)
        {
            Func<string,string> onExecute = (status) => 
            {
                var resultValue = onCheatExecute?.DynamicInvoke();
                return resultValue != null ? resultValue.ToString() : string.Empty;
            };
            return RegisterLabel(groupType.ToString(), name, description, onExecute);
        }

        private int RegisterLabel(string groupType, string name, string description, Delegate onCheatExecute)
        {
            return Register(groupType.ToString(), name, description, onCheatExecute, E_CheatType.Label);
        }
        #endregion register methods

        public void Show()
        {
            var groupList = _cheatGroups.Keys.ToList();
            if (groupList.Count == 0)
                return;

            _cheatView.Show();

            var curSelectGroupName = GetSelectGroupName();
            ShowGroupCheatItemList(curSelectGroupName);
        }
        
        public void Hide()
        {
            _cheatView.Hide();
        }
    }
}