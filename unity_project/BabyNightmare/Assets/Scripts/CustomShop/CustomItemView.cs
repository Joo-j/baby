using UnityEngine;
using UnityEngine.UI;
using System;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using TMPro;

namespace BabyNightmare.CustomShop
{
    public class CustomItemView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] protected Button BTN_Select;
        [SerializeField] protected Button BTN_Purchase;

        [SerializeField] protected GameObject GO_Equip;
        [SerializeField] protected GameObject GO_Select;
        [SerializeField] protected GameObject GO_RedDot;

        [SerializeField] protected Image IMG_Icon;
        [SerializeField] protected Image IMG_EquipCheckBox;
        [SerializeField] protected Image IMG_Price;

        [SerializeField] protected TextMeshProUGUI TMP_Price;
        [SerializeField] protected TextMeshProUGUI TMP_FeedID;

        [SerializeField] private Sprite _bg;
        [SerializeField] private Sprite _checkBoxBG;
        [SerializeField] private AnimationCurve _bounceCurve;

        private Action _onClickSelect = null;
        private Action _onClickPurchase = null;
        private CustomShopData _shopData = null;

        public RectTransform RTF => _rtf;
        public ECustomItemType Type { get; private set; }

        public void Init
        (
                    ECustomItemType type,
                    CustomShopData shopData,
                    Sprite thumbnail,
                    int rvCount,
                    Action onClickSelect,
                    Action onClickPurchase)
        {
            Type = type;
            _shopData = shopData;
            _onClickSelect = onClickSelect;
            _onClickPurchase = onClickPurchase;

            BTN_Select.image.sprite = _bg;
            IMG_EquipCheckBox.sprite = _checkBoxBG;
            IMG_Icon.sprite = thumbnail;

            TMP_FeedID.text = $"{_shopData.Item_ID}";

            if (null == IMG_Icon.sprite)
                IMG_Icon.color = Color.clear;

            var currencyIcon = Resources.Load<Sprite>($"Icon/ICN_{_shopData.CurrencyType}");
            if (null == currencyIcon)
                IMG_Price.gameObject.SetActive(false);
            else
                IMG_Price.sprite = currencyIcon;

            var cost = _shopData.Price_Value;
            switch (_shopData.CurrencyType)
            {
                case ECurrencyType.Coin:
                case ECurrencyType.Gem:
                    TMP_Price.text = $"{cost}";
                    break;

                case ECurrencyType.RV:
                    TMP_Price.text = $"{rvCount}/{cost}";
                    break;

                default:
                    TMP_Price.gameObject.SetActive(false);
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
                BTN_Purchase.gameObject.SetActive(false);
            }
            else if (hasSkin == false)
            {
                IMG_EquipCheckBox.gameObject.SetActive(false);

                var cost = _shopData.Price_Value;
                switch (_shopData.CurrencyType)
                {
                    case ECurrencyType.Coin:
                    case ECurrencyType.Gem:
                        TMP_Price.text = $"{cost}";
                        break;

                    case ECurrencyType.RV:
                        TMP_Price.text = $"{rvCount}/{cost}";
                        break;

                    default:
                        TMP_Price.gameObject.SetActive(false);
                        break;
                }
            }
        }

        public void SetActive_RedDot(bool active)
        {
            GO_RedDot.SetActive(active);
        }

        public void OnClickSelect()
        {
            _onClickSelect?.Invoke();

            _rtf.localScale = Vector3.one;
            StartCoroutine(SimpleLerp.Co_BounceScale(_rtf, Vector3.one * 1.1f, _bounceCurve, 0.05f));
        }

        public void OnClickPurchase()
        {
            Debug.Log("Purchase");
            _onClickPurchase?.Invoke();

            _rtf.localScale = Vector3.one;
            StartCoroutine(SimpleLerp.Co_BounceScale(_rtf, Vector3.one * 1.1f, _bounceCurve, 0.05f));
        }
    }
}