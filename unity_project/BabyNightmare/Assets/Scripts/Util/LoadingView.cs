using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace BabyNightmare.Util
{
    public class LoadingView : MonoBehaviour
    {
        [SerializeField] private Image _bgIMG;
        [SerializeField] private Image _progressIMG;
        [SerializeField] private TextMeshProUGUI _progresTMP;
        [SerializeField] private float _duration = 10f;
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private Sprite[] _bgResArr;

        public void Init(Action doneCallback)
        {
            _bgIMG.sprite = _bgResArr[Random.Range(0, _bgResArr.Length)];
            _progressIMG.fillAmount = 0f;
            _progresTMP.text = "0/100";

            StartCoroutine(Co_Loading(doneCallback));
        }

        IEnumerator Co_Loading(Action doneCallback)
        {
            var elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                yield return null;

                var factor = _curve.Evaluate(elapsed / _duration);
                _progressIMG.fillAmount = factor;
                var progress = factor * 100f;
                _progresTMP.text = $"{progress:F1}/100";
            }

            _progressIMG.fillAmount = 1f;
            _progresTMP.text = "100/100";

            doneCallback?.Invoke();
            Destroy(gameObject);
        }
    }
}
