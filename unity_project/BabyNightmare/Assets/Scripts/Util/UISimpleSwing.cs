using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using UnityEngine;

namespace BabyNightmare.Util
{
    public class UISimpleSwing : BehaviourBase
    {
        private RectTransform _rtf;
        [SerializeField] private Vector2 _startPos;
        [SerializeField] private Vector2 _endPos;
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private float _duration;

        private void OnEnable()
        {
            _rtf = GetComponent<RectTransform>();
            if (null == _rtf)
            {
                Debug.LogError($"{name}에 RectTransform 컴포넌트가 없습니다.");
                return;
            }

            if (true == gameObject.activeInHierarchy)
                StartCoroutine(SimpleLerp.Co_LerpAnchoredPosition_Loop(_rtf, _startPos, _endPos, _curve, _duration * 0.5f));
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}