using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Supercent.Util;

namespace BabyNightmare.Match
{
    public class SkinRewardView : SkinRewardView_Base
    {
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private Animator _animator = null;

        private readonly static int ANI_SHOW = Animator.StringToHash("Show");
        private Action _onTapToClaim = null;
        private Action _onEquip = null;

        public void Init(bool isShowEquip, Action onTapToClaim, Action onEquip)
        {

            _onTapToClaim = onTapToClaim;
            _onEquip = onEquip;

            if (true == isShowEquip)
            {
                BTN_Equip.gameObject.SetActive(true);
                TMP_EquipInfo.gameObject.SetActive(true);
                BTN_TapToClaim.gameObject.SetActive(false);
                TMP_TaptoclaimInfo.gameObject.SetActive(false);
            }
            else
            {
                BTN_Equip.gameObject.SetActive(false);
                TMP_EquipInfo.gameObject.SetActive(false);
                BTN_TapToClaim.gameObject.SetActive(true);
                TMP_TaptoclaimInfo.gameObject.SetActive(true);
            }

            IMG_Halo.transform.localScale = Vector3.zero;
            GO_Title.transform.localScale = new Vector3(0, 1, 1);
            RIMG_Preview.transform.localScale = Vector3.zero;

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0;

            StartCoroutine(Co_Show());
            IEnumerator Co_Show()
            {
                _animator.Play(ANI_SHOW);
                yield return null;

                yield return CoroutineUtil.WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);

                // IMG_Halo.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
                // GO_Title.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                // RIMG_Preview.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack);

                yield return CoroutineUtil.WaitForSeconds(0.5f);

                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.alpha = 1;
            }
        }

        public override void OnButtonEvent(Button button)
        {
            Destroy(gameObject);

            if (button == BTN_TapToClaim)
            {
                _onTapToClaim?.Invoke();
            }
            else if (button == BTN_Equip)
            {
                _onEquip?.Invoke();
            }
        }
    }
}