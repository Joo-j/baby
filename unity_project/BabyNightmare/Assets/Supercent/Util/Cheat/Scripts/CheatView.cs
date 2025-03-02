using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.Util.Cheat
{
    public class CheatView : BehaviourBase
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        [SerializeField] CanvasGroup _panelGroup;
        [SerializeField] RectTransform _rtfGroup;
        [SerializeField] RectTransform _rtfItem;
        [SerializeField] Button _btnClose;
        private List<CheatGroupButton> _groupButtons = new List<CheatGroupButton>();
        //------------------------------------------------------------------------------
        // properties
        //------------------------------------------------------------------------------
        public Transform CheatItemViewParent => _rtfItem.transform;
        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public void RemoveCheatItemViews()
        {
            foreach (Transform item in _rtfItem)
            {
                if (item == _rtfItem)
                    continue;
                Destroy(item.gameObject);
            }
        }

        public void Init(string[] groupArr, Func<int, int> onClickGroupButton, int selectIndex)
        {
            _btnClose.onClick.AddListener(Hide);

            _groupButtons.Clear();
            for (int i = 0; i < groupArr.Length; i++)
            {
                var groupType = groupArr[i];
                var groupButton = ObjectUtil.LoadAndInstantiate<CheatGroupButton>("CheatGroupButton", _rtfGroup);
                var index = i;
                groupButton.Init(groupType.ToString(), () => 
                {
                    var selectIndex = onClickGroupButton.Invoke(index);
                    RefreshUI(selectIndex);
                });
                _groupButtons.Add(groupButton);
            }
            RefreshUI(selectIndex);

            _panelGroup.alpha = 0f;
            _panelGroup.interactable = false;
            _panelGroup.blocksRaycasts = false;
        }

        public void CreateCheatItemView(ICheatItem cheatItem, string status, E_CheatType cheatType)
        {
            if (cheatType == E_CheatType.Button)
            {
                var cheatItemView = ObjectUtil.LoadAndInstantiate<CheatItemButton>(nameof(CheatItemButton), CheatItemViewParent);
                cheatItemView.Init(cheatItem.Name, cheatItem.Description, () => { cheatItem.OnExecute?.DynamicInvoke(""); });
            }
            else if (cheatType == E_CheatType.CheckBox)
            {
                var cheatItemView = ObjectUtil.LoadAndInstantiate<CheatItemCheckBox>(nameof(CheatItemCheckBox), CheatItemViewParent);
                cheatItemView.Init(cheatItem.Name, cheatItem.Description, (param) => { cheatItem.OnExecute?.DynamicInvoke(param); }, status);
            }
            else if (cheatType == E_CheatType.InputField)
            {
                var cheatItemView = ObjectUtil.LoadAndInstantiate<CheatItemInputField>(nameof(CheatItemInputField), CheatItemViewParent);
                cheatItemView.Init(cheatItem.Name, cheatItem.Description, (param) => { cheatItem.OnExecute?.DynamicInvoke(param); }, status);
            }
            else if (cheatType == E_CheatType.Slider)
            {
                var cheatItemView = ObjectUtil.LoadAndInstantiate<CheatItemSlider>(nameof(CheatItemSlider), CheatItemViewParent);
                cheatItemView.Init(cheatItem.Name, cheatItem.Description, (param) => { cheatItem.OnExecute?.DynamicInvoke(param); }, status);
            }
            else if (cheatType == E_CheatType.Label)
            {
                var cheatItemView = ObjectUtil.LoadAndInstantiate<CheatItemLabel>(nameof(CheatItemLabel), CheatItemViewParent);
                cheatItemView.Init(cheatItem.Name, cheatItem.Description, (param) => {
                    var result = cheatItem.OnExecute.DynamicInvoke(param);
                    return result.ToString();
                }, status);
            }
        }

        public void Show()
        {
            _panelGroup.alpha = 1f;
            _panelGroup.interactable = true;
            _panelGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _panelGroup.alpha = 0f;
            _panelGroup.interactable = false;
            _panelGroup.blocksRaycasts = false;
            RemoveCheatItemViews();
        }

        private void RefreshUI(int selectIndex)
        {
            for (int i = 0; i < _groupButtons.Count; i++)
            {
                var groupButton = _groupButtons[i];
                groupButton.Select(i == selectIndex);
            }
        }
        
        void OnDestroy()
        {
            _btnClose.onClick.RemoveAllListeners();
            for (int i = 0; i < _groupButtons.Count; i++)
            {
                var groupButton = _groupButtons[i];
                groupButton.Release();
            }
        }

#if UNITY_EDITOR
        protected override void OnBindSerializedField()
        {
            base.OnBindSerializedField();
            _panelGroup = transform.Find("Panel").GetComponent<CanvasGroup>();
            _rtfGroup = _panelGroup.transform.Find("RTF_GroupScroll").GetComponent<ScrollRect>().content;
            _rtfItem = _panelGroup.transform.Find("RTF_ItemScroll").GetComponent<ScrollRect>().content;
            _btnClose = _panelGroup.transform.Find("RTF_Bottom/BTN_Close").GetComponent<Button>();
        }
#endif
    }
}