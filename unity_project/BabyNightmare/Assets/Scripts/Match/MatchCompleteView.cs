using UnityEngine;
using System;
using System.Collections;
using Supercent.Util;
using BabyNightmare.Util;
using BabyNightmare.HUD;

namespace BabyNightmare.Match
{
    public class MatchCompleteView : MatchFailView_Base
    {
        [SerializeField] private AnimationCurve _sizeLerpCurve;
        [SerializeField] private AnimationCurve _bounceCurve;
        [SerializeField] private AnimationCurve _positionCurve;
        [SerializeField] private AnimationCurve _rouletteCurve;
        [SerializeField] private float[] _rouletteRotationArr;
        [SerializeField] private int[] _rouletteValueArr;
        [SerializeField] private float _rouletteSweepTime;
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private Animator _animator = null;

        private readonly static string ANI_SHOW = "Show";
        private Coroutine _coSequence = null;
        private Coroutine _coRoullete = null;
        private int _rvMultiplyValue;

        public void Init(int coin, Action doneCallback)
        {
            CLS_Reward.GO_Self.transform.localScale = new Vector3(0, 1, 1);
            CLS_Roulette.RTF_Self.localScale = Vector3.zero;
            BTN_RVReward.transform.localScale = Vector3.zero;

            BTN_RVReward.gameObject.SetActive(false);
            BTN_NoThanks.gameObject.SetActive(false);

            CLS_Reward.TMP_Coin.text = $"+{CurrencyUtil.GetUnit(coin)}";

            TMP_RVCoin.text = "";

            PTC_BG.Stop();

            BTN_RVReward.onClick.AddListener(OnRV);
            BTN_NoThanks.onClick.AddListener(OnNothanks);

            void OnRV()
            {
                if (null != _coSequence)
                    return;

                if (null != _coRoullete)
                    StopCoroutine(_coRoullete);

                CoinHUD.SetSpreadPoint(BTN_RVReward.transform.position);
                PlayerData.Instance.Coin += _rvMultiplyValue * coin;

                BTN_RVReward.enabled = false;
                BTN_NoThanks.enabled = false;
                doneCallback?.Invoke();

                Destroy(gameObject);
            }

            void OnNothanks()
            {
                if (null != _coSequence)
                    return;

                if (null != _coRoullete)
                    StopCoroutine(_coRoullete);

                CoinHUD.SetSpreadPoint(BTN_RVReward.transform.position);
                PlayerData.Instance.Coin += coin;

                BTN_RVReward.enabled = false;
                BTN_NoThanks.enabled = false;
                doneCallback?.Invoke();

                Destroy(gameObject);
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

                yield return CoroutineUtil.WaitForSeconds(0.2f);
                yield return SimpleLerp.Co_LerpScale(CLS_Roulette.GO_Self.transform, _sizeLerpCurve, 0.15f);

                yield return CoroutineUtil.WaitForSeconds(0.1f);
                _coRoullete = StartCoroutine(Co_Roulette(coin));
                BTN_RVReward.gameObject.SetActive(true);
                yield return SimpleLerp.Co_LerpScale(BTN_RVReward.transform, _sizeLerpCurve, 0.15f);

                _coSequence = null;

                yield return CoroutineUtil.WaitForSeconds(2f);

                BTN_NoThanks.gameObject.SetActive(true);

                var startPos = IMG_Icon.rectTransform.position;
                var targetPos = startPos + Vector3.up * 30f;
                StartCoroutine(SimpleLerp.Co_LerpPosition(IMG_Icon.rectTransform, startPos, targetPos, _positionCurve, 1.5f, true));
            }
        }

        private IEnumerator Co_Roulette(int coin)
        {
            var startRot = _rouletteRotationArr[0];
            var targetRot = _rouletteRotationArr[_rouletteRotationArr.Length - 1];
            var preIndex = -1;

            CLS_Roulette.TF_Arrow.rotation = Quaternion.Euler(0, 0, startRot);

            var elapsed = 0f;
            while (elapsed < _rouletteSweepTime)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var timeFactor = _rouletteCurve.Evaluate(elapsed / _rouletteSweepTime);
                var arrowRot = Mathf.Lerp(startRot, targetRot, timeFactor);
                CLS_Roulette.TF_Arrow.rotation = Quaternion.Euler(0, 0, arrowRot);

                for (int index = 0, length = _rouletteRotationArr.Length - 1; index < length; ++index)
                {
                    //각도 범위 계산
                    var maxRot = _rouletteRotationArr[index];
                    var minRot = _rouletteRotationArr[index + 1];

                    if (arrowRot < minRot)
                        continue;

                    if (arrowRot > maxRot)
                        continue;

                    //갱신 했던 인덱스인지 검사
                    if (index == preIndex)
                        break;

                    for (int i = 0, blockCount = CLS_Roulette.LIST_GO_Block.Count; i < blockCount; i++)
                    {
                        CLS_Roulette.LIST_GO_Block[i].SetActive(i == index);
                        CLS_Roulette.LIST_IMG_Value[i].color = i == index ? Vector4.one : new Vector4(1, 1, 1, 0.6f);
                    }

                    var value = CLS_Roulette.LIST_IMG_Value[index];
                    StartCoroutine(SimpleLerp.Co_BounceScale(value.transform, Vector3.one * 1.15f, _bounceCurve, 0.1f, false));

                    _rvMultiplyValue = _rouletteValueArr[index];
                    var rvCoin = CurrencyUtil.GetUnit(coin * _rvMultiplyValue);
                    TMP_RVCoin.text = $"+{rvCoin}";

                    //마지막 인덱스면 이전 인덱스 값 초기화
                    if (index == length)
                        preIndex = -1;
                    else
                        preIndex = index;
                }
            }

            yield return Co_Roulette(coin);
        }


#if UNITY_EDITOR
        protected override void _EDITOR_AssignObjectsForUser()
        {

        }
#endif
    }
}
