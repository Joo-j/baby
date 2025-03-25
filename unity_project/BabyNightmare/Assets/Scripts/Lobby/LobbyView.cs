using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using Supercent.Core.Audio;
using BabyNightmare.StaticData;
using BabyNightmare.Util;

namespace BabyNightmare.Lobby
{
    public class LobbyView : LobbyView_Base
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Dock _dock;
        [SerializeField] private Vector2 _onLayoutPos;
        [SerializeField] private Vector2 _offLayoutPos;
        [SerializeField] private Vector2 _onBannerSize;
        [SerializeField] private Vector2 _offBannerSize;
        [SerializeField] private Vector2 _onScreenOffset;
        [SerializeField] private Vector2 _offScreenOffset;
        [SerializeField] private RectTransform _center;

        private const string PATH_LOBBY_MENU_BUTTON = "Lobby/LobbyMenuButton";
        private List<LobbyMenuButton> _buttonList = null;
        private Func<ELobbyButtonType, bool> _checkRedDot = null;

        public RectTransform ScreenRTF => RTF_Screen;

        public void Init(List<ELobbyButtonType> buttonTypeList, Action<ELobbyButtonType> onClick, Func<ELobbyButtonType, bool> checkRedDot)
        {
            _buttonList = new List<LobbyMenuButton>();
            var layoutButtonList = new List<IMenuButton>();
            for (var i = 0; i < buttonTypeList.Count; i++)
            {
                var type = buttonTypeList[i];

                var button = ObjectUtil.LoadAndInstantiate<LobbyMenuButton>(PATH_LOBBY_MENU_BUTTON, transform);
                if (null == button)
                {
                    Debug.LogError($"{PATH_LOBBY_MENU_BUTTON}에 프리팹이 없습니다.");
                    return;
                }

                button.Init(type, () => onClick(type));

                _buttonList.Add(button);
                layoutButtonList.Add(button);
            }

            _checkRedDot = checkRedDot;

            _dock.Init(layoutButtonList);
        }

        private void OnEnable()
        {
            StartCoroutine(Co_CheckRedDot());
        }

        private IEnumerator Co_CheckRedDot()
        {
            while (true)
            {
                if (null == _checkRedDot)
                    yield return null;

                for (var i = 0; i < _buttonList.Count; i++)
                {
                    var button = _buttonList[i];
                    button.SetActive_RedDot(_checkRedDot.Invoke(button.Type));
                }

                yield return CoroutineUtil.WaitForSeconds(1f);
            }
        }


        private void Update()
        {
            AdaptBanner(false);
        }

        private void AdaptBanner(bool on)
        {
            if (true == _dock.gameObject.activeSelf)
            {
                RTF_Screen.offsetMin = on ? _onScreenOffset : _offScreenOffset;
                _dock.RTF.anchoredPosition = on ? _onLayoutPos : _offLayoutPos;
                RTF_Banner.sizeDelta = on ? _onBannerSize : _offBannerSize;
                GO_LoadingIcon.SetActive(on);
            }
            else
            {
                RTF_Screen.offsetMin = on ? _onBannerSize : Vector2.zero;
                RTF_Banner.sizeDelta = _offBannerSize;
                GO_LoadingIcon.SetActive(false);
            }
        }

        public void FocusButton(ELobbyButtonType type)
        {
            var button = GetButton(type);
            if (null == button)
            {
                Debug.LogError($"{type} 버튼을 찾을 수 없습니다.");
                return;
            }

            _dock.FocusButton(button.Index);
        }

        public LobbyMenuButton GetButton(ELobbyButtonType type)
        {
            for (var i = 0; i < _buttonList.Count; i++)
            {
                var button = _buttonList[i];
                if (type == button.Type)
                    return button;
            }

            return null;
        }

        public void OpenButton(ELobbyButtonType type, bool immediate, Action doneCallback = null)
        {
            var button = GetButton(type);
            if (null == button)
            {
                doneCallback?.Invoke();
                return;
            }

            if (true == immediate)
            {
                button.Open(true, doneCallback);
                return;
            }

            StartCoroutine(Co_Open(button, doneCallback));
        }

        private IEnumerator Co_Open(IOpenButton button, Action doneCallback)
        {
            var icon = button.Icon;

            button.BeforeOpen();
            FocusOverlayHelper.Apply(button.GO);
            FocusOverlayHelper.SetDimdViewAlpha(0.8f);

            var originPos = icon.position;
            var targetPos = _center.position;
            var midPos = Vector3.Lerp(originPos, targetPos, 0.5f);
            midPos.y *= 2f;
            midPos.x *= 1.4f;
            icon.position = targetPos;

            var targetScale = Vector3.one * 2f;
            icon.localScale = Vector3.zero;
            yield return SimpleLerp.Co_LerpScale(icon, Vector3.zero, targetScale, CurveHelper.Preset.EaseIn, 0.45f, () => StartCoroutine(SimpleLerp.Co_Bounce_Horizontal(button.Icon, 0.15f, CurveHelper.Preset.EaseInOut)));

            var waiter = new CoroutineWaiter();
            button.Open(false, waiter.Signal);
            yield return waiter.Wait();

            FocusOverlayHelper.SetDimdViewAlpha(0.4f);

            StartCoroutine(SimpleLerp.Co_LerpScale(icon, targetScale, Vector3.one, CurveHelper.Preset.EaseOut, 0.3f));
            AudioManager.PlaySFX("AudioClip/Lobby_Menu_Move");

            yield return SimpleLerp.Co_LerpPoision_Bezier(icon, targetPos, midPos, originPos, 0.35f, CurveHelper.Preset.EaseIn, () => StartCoroutine(SimpleLerp.Co_Bounce_Horizontal(button.Icon, 0.15f, CurveHelper.Preset.EaseOut)));
            AudioManager.PlaySFX("AudioClip/Skin_Slot_Card");

            FocusOverlayHelper.Clear();
            button.AfterOpen();

            doneCallback?.Invoke();
        }

        public void ShowGuide(ELobbyButtonType type, bool force)
        {
            var button = GetButton(type);
            if (null == button)
            {
                Debug.LogError($"{type} 버튼이 없습니다.");
                return;
            }

            button.ShowGuide(force);
        }

        public void ClearGuide()
        {
            for (var i = 0; i < _buttonList.Count; i++)
            {
                _buttonList[i].HideGuide();
            }
        }

        public void ActiveInteract(bool on) => _canvasGroup.interactable = on;
    }
}