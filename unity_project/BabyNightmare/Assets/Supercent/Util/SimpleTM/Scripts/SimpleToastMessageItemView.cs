using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.Util.STM
{
    public class SimpleToastMessageItemView : MonoBehaviour
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private Image           _imageBg;
        [SerializeField] private TextMeshProUGUI _tmpMessage;
        [SerializeField] private CanvasGroup     _canvasGroup;
        [SerializeField] private AnimationCurve  _curve;
        [SerializeField] private RectTransform   _rtfSelf;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private Coroutine _coShow = null;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public void Init(Sprite bgSprite, Color bgColor, TMP_FontAsset fontAsset, Material fontMaterial)
        {
            if (null != bgSprite)
                _imageBg.sprite = bgSprite;

            _imageBg.color = bgColor;

            _tmpMessage.font = fontAsset;
            _tmpMessage.fontSharedMaterial = fontMaterial;
        }

        public void Show(string message, Action doneCallback)
        {
            _tmpMessage.text = message;

            if (null != _coShow)
                StopCoroutine(_coShow);

            _coShow = StartCoroutine(Co_Show(doneCallback));
        }

        public void SetFont(TMP_FontAsset fontAsset, Material fontMaterial)
        {
            _tmpMessage.font = fontAsset;
            _tmpMessage.fontSharedMaterial = fontMaterial;
        }

        private IEnumerator Co_Show(Action doneCallback)
        {
            // show
            var timer = 0f;
            var limit = 0.25f;
            var begin = new Vector2(0f, 0f);
            var end   = new Vector2(0f, 50f);
            var temp  = 0f;

            while (timer < limit)
            {
                temp = timer / limit;

                _rtfSelf.anchoredPosition = Vector2.Lerp(begin, end, _curve.Evaluate(temp));
                _canvasGroup.alpha        = temp;

                yield return null;
                timer += Time.deltaTime;
            }

            _rtfSelf.anchoredPosition = end;
            _canvasGroup.alpha        = 1f;

            // idle
            yield return new WaitForSeconds(2f);

            // hide
            timer = 0f;
            limit = 0.25f;
            begin = end;
            end   = new Vector2(0f, 120f);

            while (timer < limit)
            {
                temp = timer / limit;

                _rtfSelf.anchoredPosition = Vector2.Lerp(begin, end, _curve.Evaluate(temp));
                _canvasGroup.alpha        = 1f - temp;

                yield return null;
                timer += Time.deltaTime;
            }

            _rtfSelf.anchoredPosition = end;
            _canvasGroup.alpha        = 0f;

            doneCallback?.Invoke();

            _coShow = null;
        }
    }
}
