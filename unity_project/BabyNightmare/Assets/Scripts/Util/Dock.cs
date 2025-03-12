using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;

namespace BabyNightmare.Util
{
    public interface IMenuButton
    {
        public RectTransform RTF { get; }
        public int Index { get; set; }
        public void Focus(bool on);
    }

    public class Dock : BehaviourBase
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private float _onButtonSizeXRate = 1.3f;
        [SerializeField] private float _onButtonSizeYRate = 1.3f;
        [SerializeField] private float _bounceDuration = 0.1f;
        [SerializeField] private float _spacing = 0;
        [SerializeField] private float _leftPadding = 0;
        [SerializeField] private float _rightPadding = 0;
        [SerializeField] private AnimationCurve _bounceCurve;

        private Vector2 _onButtonSize;
        private Vector2 _offButtonSize;
        private List<IMenuButton> _buttonList = null;

        public RectTransform RTF => transform as RectTransform;

        public void Init(List<IMenuButton> buttonList)
        {
            _buttonList = buttonList;
            Calc_Size();
        }

        private void Calc_Size()
        {
            if (null == _buttonList || _buttonList.Count == 0)
            {
                Debug.LogError("버튼이 초기화되지 않았습니다.");
                return;
            }

            var totalWidth = _rtf.rect.width;
            if (totalWidth == 0)
            {
                Debug.LogError($"레이아웃 렉트의 크기가 0입니다.");
                return;
            }

            for (var i = 0; i < _buttonList.Count; i++)
            {
                var button = _buttonList[i];
                button.RTF.SetAnchor(TransformExtensions.AnchorType.LeftMiddle);
                button.RTF.SetPivot(TransformExtensions.PivotType.LeftMiddle);
                button.RTF.SetParent(transform);
                button.Index = i;
            }

            var buttonCount = _buttonList.Count;
            var spacingWidth = _spacing * (buttonCount - 1);
            var paddingWidth = _leftPadding + _rightPadding;
            var totalButtonWidth = totalWidth - spacingWidth - paddingWidth;

            var buttonWidth = totalButtonWidth / buttonCount;
            var onButtonWidth = buttonWidth * _onButtonSizeXRate;
            var onButtonHeight = _rtf.sizeDelta.y * _onButtonSizeYRate;
            _onButtonSize = new Vector2(onButtonWidth, onButtonHeight);
            var offButtonWidth = (totalButtonWidth - onButtonWidth) / (buttonCount - 1);
            _offButtonSize = new Vector2(offButtonWidth, _rtf.sizeDelta.y);
        }

        public void FocusButton(int index)
        {
            Calc_Size();

            if (index < 0 || index >= _buttonList.Count)
            {
                Debug.LogError($"{index} 인덱스가 가능 범위를 초과했습니다.");
                return;
            }

            var xPos = _leftPadding;

            for (var i = 0; i < _buttonList.Count; i++)
            {
                var button = _buttonList[i];
                var rtf = button.RTF;

                var focus = i == index;
                if (true == focus)
                {
                    rtf.sizeDelta = _onButtonSize;
                    button.Focus(true);

                    rtf.anchoredPosition = new Vector2(xPos, 0);
                    xPos += _onButtonSize.x;

                    if (true == gameObject.activeInHierarchy)
                        StartCoroutine(SimpleLerp.Co_LerpSize(rtf, rtf.sizeDelta, _onButtonSize, _bounceCurve, _bounceDuration));
                    else
                        rtf.sizeDelta = _onButtonSize;
                }
                else
                {
                    rtf.sizeDelta = _offButtonSize;
                    button.Focus(false);

                    rtf.anchoredPosition = new Vector2(xPos, 0);
                    xPos += _offButtonSize.x;
                }

                xPos += _spacing;
            }
        }


#if UNITY_EDITOR
        protected override void OnBindSerializedField()
        {
            base.OnBindSerializedField();

            _rtf = GetComponent<RectTransform>();
        }
#endif
    }
}