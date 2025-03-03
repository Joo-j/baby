using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare
{
    public class DynamicCell : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Image _image;
        [SerializeField] private Image _coolImage;

        public GameObject GO => gameObject;
        public Image Image => _image;
        public RectTransform RTF => _rtf;

        public void StartCoolDownLoop(float coolTime, Action onCoolDown)
        {
            StartCoroutine(Co_CoolDown(true, coolTime, onCoolDown));
        }

        private IEnumerator Co_CoolDown(bool isLoop, float coolTime, Action onCoolDown)
        {
            while (true)
            {
                var elapsed = 0f;
                while (elapsed < coolTime)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                    _coolImage.fillAmount = elapsed / coolTime;
                }

                onCoolDown?.Invoke();

                if (false == isLoop)
                    break;
            }
        }

        public void ResetCool()
        {
            StopAllCoroutines();
            _coolImage.fillAmount = 0;
        }
    }
}