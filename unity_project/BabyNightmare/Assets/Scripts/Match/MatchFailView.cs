using UnityEngine;
using System;
using System.Collections;
using Supercent.Util;
using Supercent.Core.Audio;
using BabyNightmare.Util;
using BabyNightmare.HUD;

namespace BabyNightmare.Match
{
    public class MatchFailView : MatchFailView_Base
    {
        [SerializeField] private AnimationCurve _sizeLerpCurve;
        [SerializeField] private AnimationCurve _positionCurve;
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private Animator _animator = null;

        private readonly static string ANI_SHOW = "Show";
        private Coroutine _coSequence = null;
        private Coroutine _coRoullete = null;

        public void Init(int gem, Action doneCallback)
        {
            HUDManager.Instance.ActiveHUD(EHUDType.Gem, true);

            CLS_Reward.GO_Self.transform.localScale = new Vector3(0, 1, 1);
            BTN_NoThanks.gameObject.SetActive(false);

            CLS_Reward.TMP_Coin.text = $"+{CurrencyUtil.GetUnit(gem)}";

            PTC_BG.Stop();

            BTN_NoThanks.onClick.AddListener(OnNothanks);

            void OnNothanks()
            {
                if (null != _coSequence)
                    return;

                if (null != _coRoullete)
                    StopCoroutine(_coRoullete);

                GemHUD.SetSpreadPoint(BTN_NoThanks.transform.position);
                PlayerData.Instance.Gem += gem;
                AudioManager.PlaySFX("AudioClip/Earn_Gem");

                BTN_NoThanks.enabled = false;
                
                StartCoroutine(SimpleLerp.Co_Invoke(2f, () =>
                {
                    doneCallback?.Invoke();
                    Destroy(gameObject);
                }));
            }

            _coSequence = StartCoroutine(Co_ShowRewardInfo());

            IEnumerator Co_ShowRewardInfo()
            {
                _animator.Play(ANI_SHOW);
                yield return null;

                yield return CoroutineUtil.WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);

                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;

                PTC_BG.Simulate(0f, true, true, false);
                PTC_BG.Stop();
                PTC_BG.Play();

                yield return CoroutineUtil.WaitForSeconds(0.3f);
                yield return SimpleLerp.Co_LerpScale(CLS_Reward.GO_Self.transform, _sizeLerpCurve, 0.15f);

                _coSequence = null;

                BTN_NoThanks.gameObject.SetActive(true);

                var startPos = IMG_Icon.rectTransform.position;
                var targetPos = startPos + Vector3.up * 30f;
                StartCoroutine(SimpleLerp.Co_LerpPosition(IMG_Icon.rectTransform, startPos, targetPos, _positionCurve, 1.5f, true));
            }
        }
    }
}
