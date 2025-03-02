using System;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.Util.Cheat
{
    public class CheatItemCheckBox : BehaviourBase
    {
        [SerializeField] private Text _textName;
        [SerializeField] private GameObject _objDescription;
        [SerializeField] private Text _textDescription;
        [SerializeField] private Button _btnDescription;
        [SerializeField] private Button _buttonInfo;
        [SerializeField] private Button _buttonExecute;
        [SerializeField] private GameObject _iconCheck;

        public void Init(string itemName, string description, Action<string> onClick, string status)
        {
            _textName.text = itemName;
            _textDescription.text = description;
            _objDescription.SetActive(false);
            
            if (!bool.TryParse(status, out var enable))
                enable = false;

            _iconCheck.SetActive(enable);
            
            _btnDescription.onClick.AddListener(() => { _objDescription.SetActive(!_objDescription.activeSelf); });
            _buttonInfo.onClick.AddListener(() => { _objDescription.SetActive(!_objDescription.activeSelf); });
            _buttonExecute.onClick.RemoveAllListeners();
            _buttonExecute.onClick.AddListener(() =>
            {
                var nextActive = !_iconCheck.activeSelf;
                _iconCheck.SetActive(nextActive);
                var nextStatus = nextActive.ToString();
                onClick.Invoke(nextStatus);
            });
        }
        
#if UNITY_EDITOR
        protected override void OnBindSerializedField()
        {
            base.OnBindSerializedField();
            _textName = transform.Find("Label").GetComponent<Text>();
            _textDescription = _objDescription.transform.Find("Label").GetComponent<Text>();
            _objDescription = transform.Find("InfoDescription").gameObject;
            _btnDescription = _objDescription.GetComponent<Button>();
            _buttonInfo = _textName.transform.Find("BTN_Info").GetComponent<Button>();
            _buttonExecute = transform.Find("BTN_Click").GetComponent<Button>();

            _iconCheck = _buttonExecute.transform.Find("IMG_CheckIcon").gameObject;
        }
#endif
    }
}