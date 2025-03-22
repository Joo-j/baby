using System;
using System.Collections;
using System.Collections.Generic;
using BabyNightmare.StaticData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare.InventorySystem
{
    public class EquipmentInfoPopup : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameTMP;
        [SerializeField] private TextMeshProUGUI _levelTMP;
        [SerializeField] private TextMeshProUGUI _cooltimeTMP;
        [SerializeField] private List<GameObject> _statPanelList;
        [SerializeField] private List<TextMeshProUGUI> _statValueList;

        private const string PATH_EQUIPMENT_ICON_INFO = "Inventory/Equipment_Icon_Info/";

        public void Show(EquipmentData data)
        {
            gameObject.SetActive(true);

            var iconPath = $"{PATH_EQUIPMENT_ICON_INFO}{data.Name}";
            _icon.sprite = Resources.Load<Sprite>(iconPath);
            Debug.Assert(null != iconPath, $"{iconPath} no icon");

            _nameTMP.text = $"{data.Name}";
            _levelTMP.text = $"LV {data.Level}";
            _cooltimeTMP.text = $"Cool Down : {data.CoolTime:F1}s";

            var statDataList = data.StatDataList;

            for (var i = 0; i < _statValueList.Count; i++)
            {
                _statValueList[i].text = $"0";
            }

            for (var i = 0; i < statDataList.Count; i++)
            {
                _statValueList[(int)statDataList[i].Type].text = $"{statDataList[i].Value:F1}";
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
