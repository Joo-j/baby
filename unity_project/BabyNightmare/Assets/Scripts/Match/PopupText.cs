using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using BabyNightmare.Util;

namespace BabyNightmare.Match
{
    public class PopupText : MonoBehaviour
    {
        [SerializeField] private EPopupTextType _type;
        [SerializeField] private AnimationTrigger _trigger;
        [SerializeField] private TextMeshProUGUI _tmp;

        public EPopupTextType Type => _type;

        public void Refresh(string text, Action callback)
        {
            _tmp.text = $"{text}";

            _trigger.Clear();
            _trigger.AddAction(1, callback);
        }
    }
}

