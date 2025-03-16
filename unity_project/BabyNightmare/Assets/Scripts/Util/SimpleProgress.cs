using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Supercent.Util;

namespace BabyNightmare.Util
{
    public class SimpleProgress : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Image _progressIMG;
        [SerializeField] private TextMeshProUGUI _progressTMP;
        [SerializeField] private float _fillDuration = 1f;

        private Coroutine _coFill = null;
        private Vector2 _progressSize;

        private void Awake()
        {
            _progressSize = _progressIMG.rectTransform.rect.size;
        }

        public void Refresh(float current, float max, bool immediate)
        {
            if (true == immediate)
            {
                _progressTMP.text = $"{current}";
                _progressIMG.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(0, _progressSize.x, current / max), _progressSize.y);
                return;
            }

            if (null != _coFill)
                StopCoroutine(_coFill);

            _coFill = StartCoroutine(Co_Fill(current, max));
        }

        private IEnumerator Co_Fill(float current, float max)
        {
            var elapsed = 0f;
            var startSize = _progressIMG.rectTransform.sizeDelta;
            var targetSize = new Vector2(Mathf.Lerp(0, _progressSize.x, current / max), _progressSize.y);
            while (elapsed < _fillDuration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = elapsed / _fillDuration;
                _progressIMG.rectTransform.sizeDelta = Vector2.Lerp(startSize, targetSize, factor);
            }

            _progressIMG.rectTransform.sizeDelta = targetSize;
            _progressTMP.text = $"{current}";

            _coFill = null;
        }
    }
}
