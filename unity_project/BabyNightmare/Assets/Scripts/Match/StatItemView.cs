using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using System;

namespace BabyNightmare.Match
{
    public class StatItemView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Image _iconIMG;
        [SerializeField] private TextMeshProUGUI _valueTMP;
        [SerializeField] private TextMeshProUGUI _addTMP;
        [SerializeField] private TextMeshProUGUI _subTMP;
        [SerializeField] private GameObject _addGO;
        [SerializeField] private GameObject _subGO;
        [SerializeField] private AnimationCurve _bounceCurve;

        private const string PATH_ICON = "Match/Stat/ICN_";


        public void Init(EStatType type)
        {
            _iconIMG.sprite = Resources.Load<Sprite>($"{PATH_ICON}{type}");
            RefreshValue(0);
            RefreshChangeValue(0);
        }

        public void RefreshChangeValue(float value)
        {
            _addGO.SetActive(false);
            _subGO.SetActive(false);

            if (value < 0)
            {
                _subTMP.text = $"{(int)value}";
                _subGO.SetActive(true);
            }
            else if (value > 0)
            {
                _addTMP.text = $"+{(int)value}";
                _addGO.SetActive(true);
            }
        }

        public void RefreshValue(float value)
        {
            _valueTMP.text = $"{(int)value}/s";

            if (value == 0)
                return;

            StartCoroutine(SimpleLerp.Co_BounceScale(_rtf, Vector3.one * 1.2f, _bounceCurve, 0.1f));
        }
    }
}
