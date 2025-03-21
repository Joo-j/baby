using UnityEngine;
using UnityEngine.UI;
using System;
using BabyNightmare.StaticData;
using BabyNightmare.Util;

namespace BabyNightmare.CustomShop
{
    public class CustomItemView : CustomItemView_Base
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Sprite _bg;
        [SerializeField] private Sprite _checkBoxBG;
        [SerializeField] private AnimationCurve _bounceCurve;

        private Action _onClickSelect = null;
        private CustomShopData _shopData = null;
        private Action _guideCallback = null;

        public RectTransform RTF => _rtf;

        public void Init(CustomShopData shopData, Sprite thumbnail, int rvCount, Action onClickSelect)
        {
            _shopData = shopData;
            _onClickSelect = onClickSelect;

            BTN_Select.image.sprite = _bg;
            IMG_EquipCheckBox.sprite = _checkBoxBG;
            IMG_Icon.sprite = thumbnail;

            TMP_FeedID.text = $"{_shopData.Item_ID}";

            if (null == IMG_Icon.sprite)
                IMG_Icon.color = Color.clear;

            var currencyIcon = Resources.Load<Sprite>("$Icon/{_shopData.CurrencyType}");
            if (null == currencyIcon)
                IMG_Cost.gameObject.SetActive(false);
            else
                IMG_Cost.sprite = currencyIcon;

            var cost = _shopData.Price_Value;
            switch (_shopData.CurrencyType)
            {
                case ECurrencyType.Coin:
                case ECurrencyType.Gem:
                    TMP_Cost.text = $"{cost}";
                    break;

                case ECurrencyType.RV:
                    TMP_Cost.text = $"{rvCount}/{cost}";
                    break;

                default:
                    TMP_Cost.gameObject.SetActive(false);
                    break;
            }
        }

        public void RefreshSelect(bool isSelect)
        {
            GO_Select.SetActive(isSelect);
        }

        public void RefreshEquip(bool isEquip)
        {
            GO_Equip.SetActive(isEquip);
        }

        public void RefreshCost(bool hasSkin, int rvCount)
        {
            if (hasSkin == true)
            {
                IMG_EquipCheckBox.gameObject.SetActive(true);
                GO_Cost.SetActive(false);
            }
            else if (hasSkin == false)
            {
                IMG_EquipCheckBox.gameObject.SetActive(false);

                var cost = _shopData.Price_Value;
                switch (_shopData.CurrencyType)
                {
                    case ECurrencyType.Coin:
                    case ECurrencyType.Gem:
                        TMP_Cost.text = $"{cost}";
                        break;

                    case ECurrencyType.RV:
                        TMP_Cost.text = $"{rvCount}/{cost}";
                        break;

                    default:
                        TMP_Cost.gameObject.SetActive(false);
                        break;
                }
            }
        }

        public void SetActive_RedDot(bool active)
        {
            GO_RedDot.SetActive(active);
        }

        public void ShowGuide(Action guideCallback)
        {
            SetActive_RedDot(true);
            FocusOverlayHelper.Apply(gameObject, 0.75f);
            _guideCallback = guideCallback;
        }

        public override void OnButtonEvent(Button button)
        {
            if (button == BTN_Select)
            {
                _onClickSelect?.Invoke();

                _rtf.localScale = Vector3.one;
                StartCoroutine(SimpleLerp.Co_BounceScale(_rtf, Vector3.one * 1.1f, _bounceCurve, 0.05f));

                if (null != _guideCallback)
                {
                    FocusOverlayHelper.Clear();
                    gameObject.SetActive(false);
                    gameObject.SetActive(true);
                    _guideCallback?.Invoke();
                    _guideCallback = null;
                }
            }
        }
    }
}