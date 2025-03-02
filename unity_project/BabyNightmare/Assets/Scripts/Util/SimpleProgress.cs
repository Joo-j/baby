using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare.Util
{
    public class SimpleProgress : MonoBehaviour
    {
        [SerializeField] private Image _bgIMG;
        [SerializeField] private Image _progressIMG;
        [SerializeField] private TextMeshProUGUI _progressTMP;
        [SerializeField] private float _fillDuration = 1f;

        private Coroutine _coFill = null;
        private RectTransform _progressRTF = null;
        private Vector2 _bgSize = default;

        private void Awake()
        {
            _progressRTF = _progressIMG.rectTransform;
            _bgSize = _bgIMG.rectTransform.rect.size;
        }

        public void Refresh(float current, float max, bool immediate)
        {
            if (true == immediate)
            {
                _progressTMP.text = $"{current}/ {max}";
                _progressRTF.sizeDelta = new Vector2(Mathf.Lerp(0, _bgSize.x, current / max), _bgSize.y);
                return;
            }

            if (null != _coFill)
                StopCoroutine(_coFill);

            _coFill = StartCoroutine(Co_Fill(current, max));
        }

        private IEnumerator Co_Fill(float current, float max)
        {
            var elapsed = 0f;
            var startSize = _progressRTF.sizeDelta;
            var targetSize = new Vector2(Mathf.Lerp(0, _bgSize.x, current / max), _bgSize.y);
            while (elapsed < _fillDuration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = elapsed / _fillDuration;
                _progressRTF.sizeDelta = Vector2.Lerp(startSize, targetSize, factor);
            }

            _progressRTF.sizeDelta = targetSize;
            _progressTMP.text = $"{current} / {max}";

            _coFill = null;
        }
    }
}
