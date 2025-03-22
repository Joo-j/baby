using UnityEngine;
using UnityEngine.UI;
using System;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using TMPro;
using Supercent.Util.STM;

namespace BabyNightmare.CustomShop
{
    public class CustomItemView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Image IMG_Icon;
        [SerializeField] private GameObject GO_Select;
        [SerializeField] private GameObject GO_Purchased;
        [SerializeField] private GameObject GO_RedDot;
        [SerializeField] private GameObject GO_PurchaseButton;
        [SerializeField] private Image IMG_Price_Purchase;
        [SerializeField] private TextMeshProUGUI TMP_Price_Purchase;
        [SerializeField] private GameObject GO_NoPurchaseButton;
        [SerializeField] private Image IMG_Price_NoPurchase;
        [SerializeField] private TextMeshProUGUI TMP_Price_No_Purchase;
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

            IMG_Icon.sprite = thumbnail;

            if (null == IMG_Icon.sprite)
                IMG_Icon.color = Color.clear;

            var currencyIcon = Resources.Load<Sprite>($"Icon/ICN_{_shopData.CurrencyType}");
            if (null == currencyIcon)
                IMG_Price_Purchase.gameObject.SetActive(false);
            else
                IMG_Price_Purchase.sprite = currencyIcon;

            if (null == currencyIcon)
                IMG_Price_NoPurchase.gameObject.SetActive(false);
            else
                IMG_Price_NoPurchase.sprite = currencyIcon;

            RefreshPurchase(false);
        }

        public void RefreshSelect(bool isSelect)
        {
            GO_Select.SetActive(isSelect);
        }

        public void RefreshEquip(bool isEquip)
        {
            GO_Purchased.SetActive(isEquip);
        }

        public void RefreshPurchase(bool purchased)
        {
            if (purchased == true)
            {
                GO_Purchased.SetActive(true);
                GO_PurchaseButton.gameObject.SetActive(false);
                GO_NoPurchaseButton.gameObject.SetActive(false);
                return;
            }

            GO_PurchaseButton.gameObject.SetActive(true);
            var price = _shopData.Price_Value;

            var isPurchasable = PlayerData.Instance.Gem >= price;
            GO_PurchaseButton.SetActive(isPurchasable);
            GO_NoPurchaseButton.SetActive(!isPurchasable);

            switch (_shopData.CurrencyType)
            {
                case ECurrencyType.Gem:
                    TMP_Price_Purchase.text = $"{price}";
                    TMP_Price_No_Purchase.text = $"{price}";
                    break;

                default:
                    TMP_Price_Purchase.gameObject.SetActive(false);
                    TMP_Price_No_Purchase.gameObject.SetActive(false);
                    break;
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

        public void OnClickNoPurchase()
        {
            SimpleToastMessage.Show("Need More Gem!", null);
        }

        public void OnClickPurchase()
        {
            _onClickPurchase?.Invoke();

            _rtf.localScale = Vector3.one;
            StartCoroutine(SimpleLerp.Co_BounceScale(_rtf, Vector3.one * 1.1f, _bounceCurve, 0.05f));
        }
    }
}