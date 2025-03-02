using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Supercent.Util.SimpleLoadingSettings;

namespace Supercent.Util
{
    public class SimpleLoadingView : MonoBehaviour
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [System.Serializable]
        public class ScreenInfo
        {
            public GameObject    Self;
            public CanvasGroup   CanvasGroup;
            public Image         Dimd;
            public Image         Thumbnail;
            public RectTransform ThumbnailRtf;
        }

        [SerializeField] private ScreenInfo _fullOfScreen;
        [SerializeField] private ScreenInfo _partsOfScreen;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private SimpleLoadingSettings _settings = null;

        private System.Action _doneCallback_fullOfScreen = null;

        private Coroutine _coFullOfScreen  = null;
        private Coroutine _coPartsOfScreen = null;

        private float _rotZ_partsOfScreenIcon = 0f;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public bool IsVisibleFullOfScreen  { get; private set; } = false;
        public bool IsVisiblePartsOfScreen { get; private set; } = false;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public void Init(SimpleLoadingSettings settings)
        {
            _settings = settings;

            // full of scree
            _fullOfScreen.CanvasGroup.alpha = 0;
            _fullOfScreen.Self.SetActive(true);

            var fullOfScreenIcon = settings?.FullOfScreenInfo?.Thumbnail;
            if (null == fullOfScreenIcon)
                _fullOfScreen.Thumbnail.gameObject.SetActive(false);
            else
            {
                _fullOfScreen.Thumbnail.gameObject.SetActive(true);
                _fullOfScreen.Thumbnail.sprite = fullOfScreenIcon;
            }

            _fullOfScreen.Self.SetActive(false);
            
            if (null != settings?.FullOfScreenInfo)
                _fullOfScreen.Dimd.color = settings.FullOfScreenInfo.DimdColor;
            
            // parts of screen
            _partsOfScreen.Self.SetActive(true);

            if (null != settings?.PartsOfScreenInfo)
                _partsOfScreen.Dimd.color = settings.PartsOfScreenInfo.DimdColor;

            var partsOfScreenIcon = settings?.PartsOfScreenInfo?.LoadingIcon;
            if (null != partsOfScreenIcon)
                _partsOfScreen.Thumbnail.sprite = partsOfScreenIcon;

            _partsOfScreen.Self.SetActive(false);
        }

        public void ShowFullOfScreen(System.Action doneCallback)
        {
            if (IsVisiblePartsOfScreen)
                HidePartsOfScreen();

            if (IsVisibleFullOfScreen)
            {
                doneCallback?.Invoke();
                return;
            }

            IsVisibleFullOfScreen = true;

            if (null != _coFullOfScreen)
            {
                StopCoroutine(_coFullOfScreen);
                _coFullOfScreen = null;

                _doneCallback_fullOfScreen?.Invoke();
                _doneCallback_fullOfScreen = null;
            }

            _fullOfScreen.Self.SetActive(true);

            _doneCallback_fullOfScreen = doneCallback;
            _coFullOfScreen = StartCoroutine(Co_ChangeFullScreenAlpha(true, _settings?.FullOfScreenInfo?.Duration_show ?? 0.25f));
        }

        public void HideFullOfScreen(System.Action doneCallback)
        {
            if (!IsVisibleFullOfScreen)
            {
                doneCallback?.Invoke();
                return;
            }

            IsVisibleFullOfScreen = false;

            if (null != _coFullOfScreen)
            {
                StopCoroutine(_coFullOfScreen);
                _coFullOfScreen = null;

                _doneCallback_fullOfScreen?.Invoke();
                _doneCallback_fullOfScreen = null;
            }

            _doneCallback_fullOfScreen = doneCallback;
            _coFullOfScreen = StartCoroutine(Co_ChangeFullScreenAlpha(false, _settings?.FullOfScreenInfo.Duration_hide ?? 0.25f));
        }

        public void ShowPartsOfScreen()
        {
            if (IsVisibleFullOfScreen || IsVisiblePartsOfScreen)
                return;

            IsVisiblePartsOfScreen = true;

            _partsOfScreen.Self.SetActive(true);
            
            _rotZ_partsOfScreenIcon = 0f;
            _partsOfScreen.ThumbnailRtf.localScale    = Vector3.one;
            _partsOfScreen.ThumbnailRtf.localRotation = Quaternion.identity;

            if (null != _settings?.PartsOfScreenInfo)
            {
                switch (_settings?.PartsOfScreenInfo?.DirectingStyle)
                {
                case EPartsOfScreenIconDirectingStyle.Rotation:
                    _coPartsOfScreen = StartCoroutine(Co_PartsOfScreenIconDirecting_Rotation());
                    break;

                case EPartsOfScreenIconDirectingStyle.HeartBeat:
                    _coPartsOfScreen = StartCoroutine(Co_PartsOfScreenIconDirecting_HeartBeat());
                    break;
                }
            }
        }

        public void HidePartsOfScreen()
        {
            if (IsVisibleFullOfScreen || !IsVisiblePartsOfScreen)
                return;

            IsVisiblePartsOfScreen = false;

            if (null != _coPartsOfScreen)
            {
                StopCoroutine(_coPartsOfScreen);
                _coPartsOfScreen = null;
            }

            _partsOfScreen.Self.SetActive(false);
        }

        private IEnumerator Co_ChangeFullScreenAlpha(bool isShow, float duration)
        {
            if (isShow)
                _fullOfScreen.Self.SetActive(true);

            var timer  = 0f;
            var target = _fullOfScreen.CanvasGroup;
            var begin  = target.alpha;
            var end    = isShow ? 1f : 0f;
            var dist   = end - begin;

            while (timer < duration)
            {
                target.alpha = timer / duration * dist + begin;
                yield return null;

                timer += Time.deltaTime;
            }

            target.alpha = end;
            _coFullOfScreen = null;

            if (!isShow)
                _fullOfScreen.Self.SetActive(false);

            _doneCallback_fullOfScreen?.Invoke();
            _doneCallback_fullOfScreen = null;
        }

        private IEnumerator Co_PartsOfScreenIconDirecting_Rotation()
        {
            var speed = _settings?.PartsOfScreenInfo?.DirectingSpeed ?? 1f;
            var rtf   = _partsOfScreen?.ThumbnailRtf;
            
            if (null == rtf)
                yield break;

            while (true)
            {
                _rotZ_partsOfScreenIcon -= Time.deltaTime * speed * 360f;
                
                if (360f < _rotZ_partsOfScreenIcon)
                    _rotZ_partsOfScreenIcon -= 360f;

                rtf.localRotation = Quaternion.Euler(0f, 0f, _rotZ_partsOfScreenIcon);
                yield return null;
            }
        }

        private IEnumerator Co_PartsOfScreenIconDirecting_HeartBeat()
        {
            yield return null;
        }
    }
}