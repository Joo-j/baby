using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare.Util
{
    public class SimpleProgress : MonoBehaviour
    {
        [SerializeField] private Image _progressIMG;
        [SerializeField] private TextMeshProUGUI _progressTMP;
        [SerializeField] private float _fillDuration = 1f;

        private Coroutine _coFill = null;

        public void Refresh(float current, float max, bool immediate)
        {
            if (true == immediate)
            {
                _progressTMP.text = $"{current}/ {max}";
                _progressIMG.fillAmount = current / max;
                return;
            }

            if (null != _coFill)
                StopCoroutine(_coFill);

            _coFill = StartCoroutine(Co_Fill(current, max));
        }

        private IEnumerator Co_Fill(float current, float max)
        {
            var elapsed = 0f;

            var startAmount = _progressIMG.fillAmount;
            var targetAmount = current / max;
            while (elapsed < _fillDuration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = elapsed / _fillDuration;
                _progressIMG.fillAmount = Mathf.Lerp(startAmount, targetAmount, factor);
            }

            _progressIMG.fillAmount = targetAmount;
            _progressTMP.text = $"{current} / {max}";

            _coFill = null;
        }
    }
}
