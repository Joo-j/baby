using System;
using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace BabyNightmare.Util
{
    public class LoadingView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _bgIMG;
        [SerializeField] private Sprite[] _bgResArr;
        [SerializeField] private float _duration = 2f;

        public void Init(Action doneCallback)
        {
            _canvasGroup.alpha = 0f;
            _bgIMG.sprite = _bgResArr[Random.Range(0, _bgResArr.Length)];
            StartCoroutine(Co_Loading(doneCallback));
        }

        IEnumerator Co_Loading(Action doneCallback)
        {
            yield return SimpleLerp.Co_LerpAlpha(_canvasGroup, 0, 1f, 0.2f, CurveHelper.Preset.Linear);

            yield return CoroutineUtil.WaitForSeconds(_duration);
            
            yield return SimpleLerp.Co_LerpAlpha(_canvasGroup, 1f, 0f, 0.2f, CurveHelper.Preset.Linear);

            doneCallback?.Invoke();
            Destroy(gameObject);
        }
    }
}
