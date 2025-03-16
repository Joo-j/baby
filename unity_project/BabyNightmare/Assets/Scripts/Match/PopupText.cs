using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using BabyNightmare.Util;

namespace BabyNightmare.Match
{
    public class PopupText : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationTrigger _trigger;
        [SerializeField] private TextMeshProUGUI _tmp;

        public void Refresh(string text, Color color, Action callback)
        {
            _tmp.text = $"{text}";
            _tmp.color = color;

            _trigger.Clear();
            _trigger.AddAction(1, callback);
        }
    }
}

