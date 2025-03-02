using System;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.Util.Cheat
{
    public class CheatItemLabel : BehaviourBase
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        const float UPDATE_INTERVAL = 0.2f;
        [SerializeField] private Text _textName;
        [SerializeField] private GameObject _objDescription;
        [SerializeField] private Text _textDescription;
        [SerializeField] private Button _btnDescription;
        [SerializeField] private Button _buttonInfo;
        [SerializeField] private Text _label;
        float _nextLabelUpdateTime;
        Func<string, string> _onClick;


        public void Init(string itemName, string description, Func<string, string> onClick, string status)
        {
            _textName.text = itemName;
            _textDescription.text = description;
            _objDescription.SetActive(false);

            _label.text = onClick.Invoke(status);
            
            _btnDescription.onClick.AddListener(() => { _objDescription.SetActive(!_objDescription.activeSelf); });
            _buttonInfo.onClick.AddListener(() => { _objDescription.SetActive(!_objDescription.activeSelf); });
            _nextLabelUpdateTime = Time.time + UPDATE_INTERVAL;
            _onClick = onClick;
        }

        private void Update()
        {
            if (Time.time < _nextLabelUpdateTime)
                return;
            
            _nextLabelUpdateTime = Time.time + UPDATE_INTERVAL;
            _label.text = _onClick?.Invoke(string.Empty);
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
            
            _label = transform.Find("TMP_Label").GetComponent<Text>();
        }
#endif
    }
}