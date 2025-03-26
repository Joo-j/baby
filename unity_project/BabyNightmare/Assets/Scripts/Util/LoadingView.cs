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
        [SerializeField] private Image _bgIMG;
        [SerializeField] private Sprite[] _bgResArr;
        [SerializeField] private float _duration = 2f;

        public void Init(Action doneCallback)
        {
            _bgIMG.sprite = _bgResArr[Random.Range(0, _bgResArr.Length)];
            StartCoroutine(Co_Loading(doneCallback));
        }

        IEnumerator Co_Loading(Action doneCallback)
        {
            yield return CoroutineUtil.WaitForSeconds(_duration);
            doneCallback?.Invoke();
            Destroy(gameObject);
        }
    }
}
