using System;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.Util.Cheat
{
    public class CheatItemInputField : BehaviourBase
    {
        [SerializeField] private Text _textName;
        [SerializeField] private GameObject _objDescription;
        [SerializeField] private Text _textDescription;
        [SerializeField] private Button _btnDescription;
        [SerializeField] private Button _buttonInfo;

        [SerializeField] private InputField _inputField;
        [SerializeField] private Text _placeholder;
        [SerializeField] private Button _buttonExecute;

        public void Init(string itemName, string description, Action<string> onClick, string status)
        {
            _textName.text = itemName;
            _textDescription.text = description;
            _objDescription.SetActive(false);

            _placeholder.text = status;
            
            _btnDescription.onClick.AddListener(() => { _objDescription.SetActive(!_objDescription.activeSelf); });
            _buttonExecute.onClick.RemoveAllListeners();
            _buttonExecute.onClick.AddListener(() =>
            {
                onClick.Invoke(_inputField.text);
                
                _placeholder.text = _inputField.text;
                _inputField.text = string.Empty;
            });
            _buttonInfo.onClick.AddListener(() => { _objDescription.SetActive(!_objDescription.activeSelf); });
        }
        
#if UNITY_EDITOR
        protected override void OnBindSerializedField()
        {
            base.OnBindSerializedField();
            _textName = transform.Find("Label").GetComponent<Text>();
            _objDescription = transform.Find("InfoDescription").gameObject;
            _textDescription = _objDescription.transform.Find("Label").GetComponent<Text>();
            _btnDescription = _objDescription.GetComponent<Button>();
            _buttonInfo = _textName.transform.Find("BTN_Info").GetComponent<Button>();

            _inputField = transform.Find("InputField").GetComponent<InputField>();
            _placeholder = _inputField.placeholder.GetComponent<Text>();
            _buttonExecute = transform.Find("BTN_Click").GetComponent<Button>();
        }
#endif
    }
}