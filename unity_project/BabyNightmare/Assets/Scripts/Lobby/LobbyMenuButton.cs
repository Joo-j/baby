using UnityEngine;
using UnityEngine.UI;
using System;
using Supercent.Util.STM;
using BabyNightmare.Util;
using BabyNightmare.StaticData;

namespace BabyNightmare.Lobby
{
    public class LobbyMenuButton : LobbyMenuButton_Base, IOpenButton, IMenuButton
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Vector3 _onIconScale = Vector3.one * 1.45f;
        [SerializeField] private Vector2 _onIconPos = new Vector2(0, 40f);
        [SerializeField] private Vector2 _offIconPos = Vector2.zero;
        [SerializeField] private LobbyIcon _lobbyIcon;

        private ELobbyButtonType _buttonType;
        private Action _onClick = null;
        private Action _guideCallback = null;
        private bool _unlocked = false;

        public int Index { get; set; }
        public ELobbyButtonType Type => _buttonType;
        public RectTransform RTF => _rtf;
        public GameObject GO => gameObject;
        public Transform Icon => _lobbyIcon.transform;
        public Image Mask => null;
        public Transform Gradation => null;
        public Transform Title => TMP_Desc.transform;
        public CanvasGroup CanvasGroup => _canvasGroup;

        public void Init(ELobbyButtonType buttonType, Action onClick)
        {
            _buttonType = buttonType;
            _onClick = onClick;

            _lobbyIcon.Init(buttonType);

            RefreshDesc();

            SetActive_RedDot(false);
            GO_Guide.SetActive(false);
            GO_Lock.SetActive(false);
        }

        private void RefreshDesc()
        {
            TMP_Desc.text = LobbyUtil.GetDesc(_buttonType);
            _lobbyIcon.RefreshDesc(_buttonType);
        }

        public void Focus(bool on)
        {
            GO_ChoiceBG.SetActive(on);
            TMP_Desc.transform.localScale = on ? Vector3.one : Vector3.zero;

            _lobbyIcon.RTF.localScale = on ? _onIconScale : Vector3.one;
            _lobbyIcon.RTF.anchoredPosition = on ? _onIconPos : _offIconPos;

            if (true == on)
                _lobbyIcon.Bounce();
        }

        public void ShowGuide(bool force, Action guideCallback = null)
        {
            _guideCallback = guideCallback;
            GO_Guide.SetActive(true);

            if (force)
            {
                FocusOverlayHelper.Apply(gameObject);
                GO_Line.SetActive(false);
            }
        }

        public void HideGuide()
        {
            GO_Guide.SetActive(false);
            GO_Line.SetActive(true);
        }

        public void BeforeOpen()
        {
            BTN_Click.enabled = false;
            IMG_RedDot.color = Color.clear;
            GO_ChoiceBG.SetActive(false);
            GO_Lock.SetActive(true);
            GO_Line.SetActive(false);
        }

        public void AfterOpen()
        {
            BTN_Click.enabled = true;
            IMG_RedDot.color = Color.white;
            GO_Lock.SetActive(false);
            GO_Line.SetActive(true);
            TMP_Desc.transform.localScale = Vector3.zero;
            StartCoroutine(SimpleLerp.Co_Bounce_Horizontal(Icon, 0.15f, CurveHelper.Preset.EaseOut));
            _lobbyIcon.Bounce();
        }

        public void Open(bool immediate, Action doneCallback = null)
        {
            _unlocked = true;

            if (true == immediate)
            {
                _lobbyIcon.Unlock(true);
                GO_Lock.SetActive(false);
                doneCallback?.Invoke();
                return;
            }

            _lobbyIcon.Unlock(false, doneCallback);
        }

        public void SetActive_RedDot(bool active) => IMG_RedDot.gameObject.SetActive(active);

        public override void OnButtonEvent(Button button)
        {
            if (button == BTN_Click)
            {
                if (false == _unlocked)
                {
                    SimpleToastMessage.Show("Play More", null);
                    return;
                }
                
                FocusOverlayHelper.Clear();

                _onClick?.Invoke();
                HideGuide();

                _guideCallback?.Invoke();
                _guideCallback = null;
            }
        }
    }
}
