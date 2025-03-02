using System;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.Util.Cheat
{
    public class CheatGroupButton : BehaviourBase
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] Text _textName;
        [SerializeField] Image _imgBg;
        [SerializeField] Image _imgInner;
        [SerializeField] Button _groupBtn;


        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private static readonly Color COLOR_SELECTED = new Color(0f, 0.8627451f, 1f, 1f);
        private static readonly Color COLOR_DESELECTED = new Color(0f, 0.4588235f, 0.5372549f, 1f);


        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public void Init(string groupName, Action onClickGroupTab)
        {
            _textName.text = groupName;
            _groupBtn.onClick.AddListener(() => 
            {
                onClickGroupTab?.Invoke();
            });
        }

        public void Release()
        {
            _groupBtn.onClick.RemoveAllListeners();
        }

        public void Select(bool isSelect)
        {
            if (true == isSelect)
            {
                _imgBg.color = COLOR_SELECTED;
                _imgInner.color = COLOR_SELECTED;
            }
            else
            {
                _imgBg.color = COLOR_DESELECTED;
                _imgInner.color = COLOR_DESELECTED;
            }
        }
        
#if UNITY_EDITOR
        protected override void OnBindSerializedField()
        {
            base.OnBindSerializedField();
            _textName = transform.Find("GroupName").GetComponent<Text>();
            _imgBg = transform.Find("IMG_Bg").GetComponent<Image>();
            _imgInner = transform.Find("IMG_Bg/IMG_Inner").GetComponent<Image>();
            _groupBtn = GetComponent<Button>();
        }
#endif
    }
}