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
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshPro _gradeTMP;
        [SerializeField] private TextMeshPro _nameTMP;
        [SerializeField] private TextMeshPro _levelTMP;
        [SerializeField] private TextMeshPro _damageTMP;
        [SerializeField] private TextMeshPro _cooltimeTMP;

        public void Show(EquipmentData data)
        {
            _image.sprite = data.Sprite;
            _nameTMP.text = $"{data.Name}";
            _levelTMP.text = $"LV {data.Level}";
            _damageTMP.text = $"{data.Damage}";
            _cooltimeTMP.text = $"{data.CoolTime:F1}s";
        }
    }
}
