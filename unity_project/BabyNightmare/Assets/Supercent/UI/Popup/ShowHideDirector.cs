using System;
using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.UI.Popup
{
    public class ShowHideDirector : MonoBehaviour
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private List<CanvasGroup>  _canvasGroupList;
        [SerializeField] private List<RawImage>     _rawImageList;
        [SerializeField] private List<Image>        _imageList;
        [SerializeField] private List<Transform>    _transforms;

        [Space]
        [SerializeField] private float _duration;
        [SerializeField] private AnimationCurve _curveForShow;
        [SerializeField] private AnimationCurve _curveForHide;


        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private Coroutine _coDirecting = null;
        private Action _doneCallback = null;
        private bool   _isSetToIsShown = false;


        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public bool IsShown { get; private set; } = false;


        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public void SetDuration(float duration) => _duration = duration;

        public void Show(Action doneCallback, bool isImmediately = false)
        {
            if (false != _isSetToIsShown && true == IsShown)
                return;

            _isSetToIsShown = true;
            IsShown         = true;

            if (null != _coDirecting)
            {
                StopCoroutine(_coDirecting);

                _doneCallback?.Invoke();
                _doneCallback = null;
            }

            _coDirecting = StartCoroutine(Co_Directing(doneCallback, isImmediately, 0f, 1f, Vector3.zero, Vector3.one, _curveForShow));
        }

        public void Hide(Action doneCallback, bool isImmediately = false)
        {
            if (false != _isSetToIsShown && false == IsShown)
                return;

            _isSetToIsShown = true;
            IsShown         = false;

            if (null != _coDirecting)
            {
                StopCoroutine(_coDirecting);

                _doneCallback?.Invoke();
                _doneCallback = null;
            }
            
            _coDirecting = StartCoroutine(Co_Directing(doneCallback, isImmediately, 1f, 0f, Vector3.one, Vector3.zero, _curveForHide));
        }

        private IEnumerator Co_Directing(Action doneCallback, bool isImmediately, 
                                         float beginAlpha, float endAlpha, 
                                         Vector3 beginScale, Vector3 endScale,
                                         AnimationCurve curve)
        {
            if (false == isImmediately)
            {
                var timer     = 0f;
                var normal    = 0f;
                var alpha     = 0f;
                var distAlpha = endAlpha - beginAlpha;
                var distScale = endScale - beginScale;

                while (timer < _duration)
                {
                    normal = timer / _duration;
                    alpha  = distAlpha * normal + beginAlpha;

                    for (int i = 0, size = _canvasGroupList?.Count ?? 0; i < size; ++i)
                        _canvasGroupList[i].alpha = alpha;

                    for (int i = 0, size = _rawImageList?.Count ?? 0; i < size; ++i)
                        _rawImageList[i].SetAlpha(alpha);

                    for (int i = 0, size = _imageList?.Count ?? 0; i < size; ++i)
                        _imageList[i].SetAlpha(alpha);

                    var scale = Vector3.Slerp(beginScale, endScale, curve.Evaluate(normal));
                    for (int i = 0, size = _transforms?.Count ?? 0; i < size; ++i)
                        _transforms[i].localScale = scale;

                    yield return null;
                    timer += Time.deltaTime;
                }
            }

            for (int i = 0, size = _canvasGroupList?.Count ?? 0; i < size; ++i)
                _canvasGroupList[i].alpha = endAlpha;

            for (int i = 0, size = _rawImageList?.Count ?? 0; i < size; ++i)
                _rawImageList[i].SetAlpha(endAlpha);

            for (int i = 0, size = _imageList?.Count ?? 0; i < size; ++i)
                _imageList[i].SetAlpha(endAlpha);

            for (int i = 0, size = _transforms?.Count ?? 0; i < size; ++i)
                _transforms[i].localScale = endScale;

            doneCallback?.Invoke();
        }
    }
}