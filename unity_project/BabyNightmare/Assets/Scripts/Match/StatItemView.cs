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
        [SerializeField] private Animator _animator;
        [SerializeField] private Image _iconIMG;
        [SerializeField] private TextMeshProUGUI _valueTMP;
        [SerializeField] private TextMeshProUGUI _addTMP;
        [SerializeField] private TextMeshProUGUI _subTMP;
        [SerializeField] private AnimationCurve _bounceCurve;

        private const string PATH_ICON = "Match/Stat/ICN_";
        private readonly int ANI_HASH_ADD = Animator.StringToHash("Add");
        private readonly int ANI_HASH_SUB = Animator.StringToHash("Sub");

        private int _value = 0;

        public void Init(EStatType type)
        {
            _iconIMG.sprite = Resources.Load<Sprite>($"{PATH_ICON}{type}");
            _valueTMP.text = $"0/s";
        }

        public void AddValue(int amount)
        {
            if (amount < 0)
            {
                _subTMP.text = $"{amount}";
                _animator.Play(ANI_HASH_SUB);
            }
            else
            {
                _addTMP.text = $"+{amount}";
                _animator.Play(ANI_HASH_ADD);
            }

            _value += amount;
            _valueTMP.text = $"{_value}/s";

            StartCoroutine(SimpleLerp.Co_BounceScale(_rtf, Vector3.one * 1.2f, _bounceCurve, 0.1f));
        }
    }
}
