using System;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.Util.Cheat
{
    public class CheatItemButton : BehaviourBase
    {
        [SerializeField] private Text _textName;
        [SerializeField] private GameObject _objDescription;
        [SerializeField] private Text _textDescription;
        [SerializeField] private Button _btnDescription;
        [SerializeField] private Button _buttonInfo;
        [SerializeField] private Button _buttonExecute;

        public void Init(string itemName, string description, Action onClick)
        {
            _textName.text = itemName;
            _textDescription.text = description;
            _objDescription.SetActive(false);
            
            _btnDescription.onClick.AddListener(() => { _objDescription.SetActive(!_objDescription.activeSelf); });
            _buttonInfo.onClick.AddListener(() => { _objDescription.SetActive(!_objDescription.activeSelf); });
            _buttonExecute.onClick.AddListener(() => { onClick?.Invoke(); });
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
            _buttonExecute = transform.Find("BTN_Click").GetComponent<Button>();
        }
#endif
    }
}