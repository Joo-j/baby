using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using BabyNightmare.Util;
using BabyNightmare.StaticData;
using TMPro;
using Supercent.UIv2;

namespace BabyNightmare.CustomShop
{
    public class CustomShopViewContext
    {
        public Dictionary<int, CustomItemData> ItemDataDict { get; }
        public Dictionary<int, CustomShopData> ShopDataDict { get; }
        public Dictionary<int, int> RvDataDict { get; }
        public int EquipItemID { get; }
        public Action<int> OnClickSelect { get; }
        public Action OnClickEquip { get; }
        public Action OnClickPurchase { get; }

        public CustomShopViewContext(
            Dictionary<int, CustomItemData> itemDataDict,
            Dictionary<int, CustomShopData> shopDataDict,
            Dictionary<int, int> rvDataDict,
            int equipitemID,
            Action<int> onClickSelect,
            Action onClickEquip,
            Action onClickPurchase)
        {
            this.ItemDataDict = itemDataDict;
            this.ShopDataDict = shopDataDict;
            this.RvDataDict = rvDataDict;
            this.EquipItemID = equipitemID;
            this.OnClickSelect = onClickSelect;
            this.OnClickEquip = onClickEquip;
            this.OnClickPurchase = onClickPurchase;
        }

        public int GetRVCount(int shopDataID)
        {
            if (false == RvDataDict.TryGetValue(shopDataID, out var rvCount))
                return 0;

            return rvCount;
        }
    }

    public class CustomShopView : UIBase
    {
        [SerializeField] protected Image IMG_CurrenyTypeIcon;
        [SerializeField] protected ParticleSystem PTC_Select;
        [SerializeField] protected ParticleSystem PTC_Equip;
        [SerializeField] protected TextMeshProUGUI TMP_CurrenyAmount;
        [SerializeField] protected Image IMG_AlreadyEquip;
        [SerializeField] protected RawImage RIMG_Preview;
        [SerializeField] protected Button BTN_Equip;
        [SerializeField] protected Button BTN_Purchase;
        [SerializeField] protected ScrollRect SR_Content;
        [SerializeField] protected RectTransform RTF_Viewport;
        [SerializeField] protected RectTransform RTF_Button;
        [SerializeField] protected RectTransform RTF_Guide;
        [SerializeField] protected TextMeshProUGUI TMP_ItemName;
        [SerializeField] private RectTransform _purchaseBtnRTF;
        [SerializeField] private RectTransform _equipBtnRTF;
        [SerializeField] private AnimationCurve _scrollCurve;
        [SerializeField] private Sprite _purchaseGemButtonSprite;
        [SerializeField] private Sprite _purchaseGemIconSprite;

        private const string PATH_CUSTOM_ITEM_VIEW = "CustomShop/CustomItemView";
        private const string PATH_CUSTOM_ITEM_PREVIEW = "CustomShop/CustomItemPreview";
        private CustomShopViewContext _context = null;
        private Dictionary<int, CustomItemView> _itemViewDict = null;
        private CustomItemPreview _itemPreview = null;
        private bool _isGuide = false;

        public void Init(CustomShopViewContext context)
        {
            _context = context;

            _itemViewDict = new Dictionary<int, CustomItemView>();

            foreach (var pair in _context.ShopDataDict)
            {
                var shopData = pair.Value;
                var itemID = shopData.Item_ID;

                var customData = _context.ItemDataDict[itemID];
                if (null == customData)
                {
                    Debug.LogError($"{itemID}에 대한 CustomItem이 없습니다.");
                    return;
                }

                var itemView = ObjectUtil.LoadAndInstantiate<CustomItemView>(PATH_CUSTOM_ITEM_VIEW, SR_Content.content);
                if (null == itemView)
                {
                    Debug.LogError($"{PATH_CUSTOM_ITEM_VIEW}에 프리팹이 업습니다.");
                    return;
                }

                itemView.Init(
                shopData,
                customData.Thumbnail,
                _context.GetRVCount(shopData.ID),
                () =>
                {
                    _context.OnClickSelect?.Invoke(itemID);
                    OnSelect();
                });

                _itemViewDict.Add(itemID, itemView);
            }
        }

        public void Release()
        {
            _context = null;
            _itemViewDict = null;
            _itemPreview?.Release();
            _itemPreview = null;
        }

        public void Show(object identifier)
        {
            (transform as RectTransform).SetFullStretch();
            gameObject.SetActive(true);

            OnSelect();
            PTC_Equip.Stop();

            if (false == identifier is int equipID)
                return;

            Scroll(equipID);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SelectItem(HashSet<int> purchasedIDSet, int selectID, int equipeditemID)
        {
            var shopData = _context.ShopDataDict[selectID];
            if (null == shopData)
            {
                Debug.LogError($"{selectID}에 대한 FeedShopData가 없습니다.");
                return;
            }

            RefreshPreview(shopData.Item_ID);
            RefreshItemView(shopData, purchasedIDSet, equipeditemID);
            RefreshEquipPurchaseButton(shopData, purchasedIDSet.Contains(selectID), selectID == equipeditemID);
            RefreshFeedInfo(shopData.Item_ID);
        }

        private void RefreshPreview(int itemID)
        {
            if (null == _itemPreview)
            {
                _itemPreview = ObjectUtil.LoadAndInstantiate<CustomItemPreview>(PATH_CUSTOM_ITEM_PREVIEW, transform);
                if (null == _itemPreview)
                {
                    Debug.LogError($"{PATH_CUSTOM_ITEM_PREVIEW}에 프리팹이 없습니다.");
                    return;
                }

                RIMG_Preview.texture = _itemPreview.RT;
            }

            var customData = _context.ItemDataDict[itemID];
            if (null == customData)
            {
                Debug.Log($"{itemID} 먹이 아이템이 null 입니다.");
            }

            _itemPreview.RefreshCustomItem(customData);
        }

        private void RefreshItemView(CustomShopData shopData, HashSet<int> purchasedIDSet, int equipitemID)
        {
            foreach (var pair in _itemViewDict)
            {
                var itemID = pair.Key;
                var itemView = pair.Value;

                itemView.RefreshSelect(itemID == shopData.Item_ID);
                itemView.RefreshEquip(itemID == equipitemID);
                itemView.RefreshCost(purchasedIDSet.Contains(itemID), _context.GetRVCount(itemID));
            }
        }

        // 구매, 장착 버튼 활성화 여부
        private void RefreshEquipPurchaseButton(CustomShopData shopData, bool isHasFeed, bool isEquipFeed)
        {
            if (true == isHasFeed)
            {
                BTN_Purchase.gameObject.SetActive(false);
                BTN_Equip.gameObject.SetActive(!isEquipFeed);
                IMG_AlreadyEquip.gameObject.SetActive(isEquipFeed);

                if (true == isEquipFeed)
                {
                    IMG_AlreadyEquip.rectTransform.localScale = Vector3.one;
                    StartCoroutine(SimpleLerp.Co_BounceScale(IMG_AlreadyEquip.rectTransform, Vector3.one * 1.1f, _scrollCurve, 0.05f));
                }
                else
                {
                    _equipBtnRTF.localScale = Vector3.one;
                    StartCoroutine(SimpleLerp.Co_BounceScale(_equipBtnRTF, Vector3.one * 1.1f, _scrollCurve, 0.05f));
                }

                return;
            }

            BTN_Purchase.gameObject.SetActive(true);
            BTN_Equip.gameObject.SetActive(false);
            IMG_AlreadyEquip.gameObject.SetActive(false);

            _purchaseBtnRTF.localScale = Vector3.one;
            StartCoroutine(SimpleLerp.Co_BounceScale(_purchaseBtnRTF, Vector3.one * 1.1f, _scrollCurve, 0.05f));

            var currencyType = shopData.CurrencyType;
            var cost = shopData.Price_Value;
            var itemID = shopData.Item_ID;

            switch (currencyType)
            {
                case ECurrencyType.Gem:
                    BTN_Purchase.image.sprite = _purchaseGemButtonSprite;
                    IMG_CurrenyTypeIcon.sprite = _purchaseGemIconSprite;
                    TMP_CurrenyAmount.text = $"{cost}";
                    TMP_CurrenyAmount.color = cost > PlayerData.Instance.Gem ? Color.red : Color.white;
                    break;
            }
        }

        // 먹이 아이템 정보 뷰 갱신
        private void RefreshFeedInfo(int itemID)
        {
            var shopData = _context.ShopDataDict[itemID];

            TMP_ItemName.text = $"{shopData.name}";
        }

        // 콘텐츠 목록들 중 itemID 에 해당하는 항목으로 스크롤 갱신
        private void Scroll(int itemID, Action doneCallback = null)
        {
            if (false == _itemViewDict.TryGetValue(itemID, out var customItemView))
                return;

            StartCoroutine(Co_Scroll());

            IEnumerator Co_Scroll()
            {
                yield return CoroutineUtil.WaitForSeconds(0.2f);

                var viewRTF = customItemView.RTF;
                var contentHeight = SR_Content.content.rect.height;
                var scrollPos = contentHeight + viewRTF.anchoredPosition.y;
                var viewHeight = RTF_Viewport.rect.height;
                var viewHalfHeight = viewHeight * 0.5f;
                var end = Mathf.Clamp((scrollPos - viewHalfHeight) / (contentHeight - viewHeight), 0f, 1f); //뷰 렉트가 어디로 갈지 비율 맞추기

                var timer = 0f;
                var limit = 0.35f;
                var begin = SR_Content.verticalNormalizedPosition;
                var dist = end - begin;

                while (timer < limit)
                {
                    SR_Content.verticalNormalizedPosition = _scrollCurve.Evaluate(timer / limit) * dist + begin;
                    yield return null;

                    timer += Time.deltaTime;
                }

                SR_Content.verticalNormalizedPosition = end;

                doneCallback?.Invoke();
            }
        }

        private void OnSelect()
        {
            PTC_Select.Simulate(0f, true, true, false);
            PTC_Select.Play();
        }

        public void OnEquip()
        {
            PTC_Equip.Simulate(0f, true, true, false);
            PTC_Equip.Play();

            if (true == _isGuide)
            {
                FocusOverlayHelper.Clear();
                RTF_Guide.gameObject.SetActive(false);
                _isGuide = false;
            }
        }

        public void GuideItemView(int itemID)
        {
            if (false == _itemViewDict.TryGetValue(itemID, out var itemView))
            {
                Debug.LogError($"ID: {itemID} 먹이 아이템 뷰가 없습니다.");
                return;
            }

            Scroll(itemID, () =>
            {
                RTF_Guide.gameObject.SetActive(true);
                RTF_Guide.SetParent(itemView.transform);
                RTF_Guide.anchoredPosition = Vector2.zero;

                itemView.ShowGuide(
                () =>
                {
                    FocusOverlayHelper.Apply(BTN_Equip.gameObject, 0.75f);
                    RTF_Guide.gameObject.SetActive(true);
                    RTF_Guide.SetParent(BTN_Equip.transform);
                    RTF_Guide.anchoredPosition = Vector2.zero;

                    _isGuide = true;
                });
            });
        }

        public void SetActiveRedDot(int itemID, bool active)
        {
            if (false == _itemViewDict.TryGetValue(itemID, out var itemView))
                return;

            itemView.SetActive_RedDot(active);
        }

        public override void OnButtonEvent(Button button)
        {
            if (button == BTN_Equip)
            {
                _context.OnClickEquip?.Invoke();
            }
            else if (button == BTN_Purchase)
            {
                _context.OnClickPurchase?.Invoke();
            }
        }
    }
}