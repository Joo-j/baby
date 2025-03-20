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
        [SerializeField] private TextMeshProUGUI _gradeTMP;
        [SerializeField] private TextMeshProUGUI _nameTMP;
        [SerializeField] private TextMeshProUGUI _descTMP;
        [SerializeField] private TextMeshProUGUI _levelTMP;
        [SerializeField] private TextMeshProUGUI _damageTMP;
        [SerializeField] private TextMeshProUGUI _cooltimeTMP;

        public void Show(EquipmentData data)
        {
            gameObject.SetActive(true);

            _icon.sprite = data.Sprite;
            _nameTMP.text = $"{data.Name}";
            _descTMP.text = $"LV {data.Desc}";
            _levelTMP.text = $"LV {data.Level}";
            _cooltimeTMP.text = $"{data.CoolTime:F1}s";

            var statDataList = data.StatDataList;
            for (var i = 0; i < statDataList.Count; i++)
            {
                var statData = statDataList[i];
                if (statData.Type == EStatType.ATK)
                {
                    _damageTMP.text = $"{statData.Value}";
                    break;
                }
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
