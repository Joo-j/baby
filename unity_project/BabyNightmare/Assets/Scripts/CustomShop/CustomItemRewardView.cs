using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Supercent.Util;
using BabyNightmare.Util;
using BabyNightmare.StaticData;

namespace BabyNightmare.CustomShop
{
    public class CustomItemRewardView : CustomItemRewardView_Base
    {
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private Animator _animator = null;
        [SerializeField] private AnimationCurve _scaleCurve;

        private readonly static int ANI_SHOW = Animator.StringToHash("Show");
        private const string PATH_CUSTOM_ITEM_PRIVIEW_REWARD = "CustomShop/CustomItemPreview_Reward";
        private CustomItemPreview _preview;
        private Action _onTapToClaim = null;
        private Action _onEquip = null;

        public void Init(bool isShowEquip, CustomItemData itemData, Action onTapToClaim, Action onEquip)
        {
            _preview = ObjectUtil.LoadAndInstantiate<CustomItemPreview>(PATH_CUSTOM_ITEM_PRIVIEW_REWARD, transform);
            if (null == _preview)
            {
                Debug.LogError("먹이 프리뷰가 없습니다.");
                return;
            }

            RIMG_Preview.texture = _preview.RT;
            _preview.RefreshCustomItem(itemData);

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

                StartCoroutine(SimpleLerp.Co_LerpScale(IMG_Halo.transform, _scaleCurve, 0.4f));
                StartCoroutine(SimpleLerp.Co_LerpScale(GO_Title.transform, _scaleCurve, 0.5f));
                StartCoroutine(SimpleLerp.Co_LerpScale(RIMG_Preview.transform, _scaleCurve, 0.6f));

                yield return CoroutineUtil.WaitForSeconds(0.5f);

                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.alpha = 1;
            }
        }

        public override void OnButtonEvent(Button button)
        {
            _preview.Release();
            _preview = null;
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
