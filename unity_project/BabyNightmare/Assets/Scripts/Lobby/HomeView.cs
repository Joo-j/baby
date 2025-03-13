using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using TMPro;

namespace BabyNightmare.Lobby
{
    public class HomeView : MonoBehaviour
    {
        [SerializeField] private RectTransform _center;
        [SerializeField] private TextMeshProUGUI _chapterTMP;
        [SerializeField] private Image _chapterICN;
        [SerializeField] private TextMeshProUGUI _fieldTMP;

        private Action _startGame = null;
        private bool _isStarted = false;

        public void Init(Action startGame)
        {
            _startGame = startGame;
        }

        public void Refresh()
        {
            _isStarted = false;
            _chapterTMP.text = $"Chapter {PlayerData.Instance.Chapter}";
            _fieldTMP.text = "Night of Dessert";
        }

        private GridLayoutGroup GetLayoutGroup(ELobbyButtonType type)
        {
            switch (type)
            {

                default: return null;
            }
        }

        private IOpenButton GetButton(ELobbyButtonType type, Func<bool> isEnterDone)
        {
            switch (type)
            {

                default: return null;
            }
        }

        public void OpenButton(ELobbyButtonType type, bool immediate, Func<bool> isEnterDone, Action doneCallback)
        {
            var button = GetButton(type, isEnterDone);
            if (null == button)
            {
                Debug.Log($"HomeView에 {type} 버튼이 없습니다.");
                doneCallback?.Invoke();
                return;
            }

            if (true == immediate)
            {
                button.GO.SetActive(true);
                button.Open(immediate, doneCallback);
                return;
            }

            var layoutGroup = GetLayoutGroup(type);
            if (null == layoutGroup)
            {
                Debug.LogError($"{type}타입 버튼에 붙은 레이아웃이 없습니다.");
                return;
            }

            StartCoroutine(Co_Open(button, layoutGroup, doneCallback));
        }

        private IEnumerator Co_Open(IOpenButton button, GridLayoutGroup layoutGroup, Action doneCallback)
        {
            var rtf = button.RTF;
            var gradation = button.Gradation;
            var title = button.Title;
            var mask = button.Mask;
            var canvasGroup = button.CanvasGroup;

            button.GO.SetActive(true);
            canvasGroup.alpha = 0f;
            yield return null;

            layoutGroup.enabled = false;
            yield return null;

            button.BeforeOpen();
            FocusOverlayHelper.Apply(button.GO);
            FocusOverlayHelper.SetDimdViewAlpha(0.8f);

            rtf.SetPivot(TransformExtensions.PivotType.CenterMiddle);

            var originPos = rtf.position;
            var targetPos = _center.position;

            rtf.position = targetPos;

            var targetScale = Vector3.one * 2.5f;
            StartCoroutine(SimpleLerp.Co_LerpColor(mask, Vector4.one, new Vector4(1, 1, 1, 0f), 1f, CurveHelper.Preset.EaseOut));
            StartCoroutine(SimpleLerp.Co_LerpScale(rtf, Vector3.zero, targetScale, CurveHelper.Preset.EaseIn, 0.45f, () => StartCoroutine(SimpleLerp.Co_Bounce_Horizontal(button.Icon, 0.15f, CurveHelper.Preset.EaseInOut))));

            var midPos = targetPos + new Vector3(0, 400, 0);
            yield return SimpleLerp.Co_LerpPoision_Bezier(rtf, targetPos, midPos, targetPos, 0.5f, CurveHelper.Preset.EaseOut);

            yield return CoroutineUtil.WaitForSeconds(0.5f);

            StartCoroutine(SimpleLerp.Co_LerpScale(gradation, CurveHelper.Preset.EaseIn, 0.25f));
            yield return SimpleLerp.Co_LerpScale(title, CurveHelper.Preset.EaseIn, 0.3f);

            HapticHelper.Haptic(Lofelt.NiceVibrations.HapticPatterns.PresetType.Success);

            var waiter = new CoroutineWaiter();
            button.Open(false, waiter.Signal);
            yield return waiter.Wait();

            FocusOverlayHelper.SetDimdViewAlpha(0.4f);

            midPos = Vector3.Lerp(originPos, targetPos, 0.5f);
            midPos.y *= 2f;
            StartCoroutine(SimpleLerp.Co_LerpScale(rtf, targetScale, Vector3.one, CurveHelper.Preset.EaseOut, 0.25f));
            StartCoroutine(SimpleLerp.Co_Invoke(0.45f, () => StartCoroutine(SimpleLerp.Co_Bounce_Horizontal(button.Icon, 0.15f, CurveHelper.Preset.EaseOut))));

            yield return SimpleLerp.Co_LerpPoision_Bezier(rtf, targetPos, midPos, originPos, 0.5f, CurveHelper.Preset.EaseOut);

            layoutGroup.enabled = true;

            FocusOverlayHelper.Clear();
            button.AfterOpen();

            doneCallback?.Invoke();
        }

        public void ShowGuide(ELobbyButtonType type, bool force, Func<bool> isEnterDone)
        {
            var button = GetButton(type, isEnterDone);
            if (null == button)
            {
                Debug.LogError($"{type} 버튼이 없습니다.");
                return;
            }

            button.ShowGuide(force);
        }

        public void OnClickPlay()
        {
            if (true == _isStarted)
            {
                Debug.Log("HomeView Already Started!!");
                return;
            }

            _startGame?.Invoke();
        }
    }
}