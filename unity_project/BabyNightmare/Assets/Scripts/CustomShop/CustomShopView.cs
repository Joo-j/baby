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
        public Action<int> TryPurchase { get; }

        public CustomShopViewContext(
            Dictionary<int, CustomItemData> itemDataDict,
            Dictionary<int, CustomShopData> shopDataDict,
            Dictionary<int, int> rvDataDict,
            int equipitemID,
            Action<int> onClickSelect,
            Action<int> tryPurchase)
        {
            this.ItemDataDict = itemDataDict;
            this.ShopDataDict = shopDataDict;
            this.RvDataDict = rvDataDict;
            this.EquipItemID = equipitemID;
            this.OnClickSelect = onClickSelect;
            this.TryPurchase = tryPurchase;
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
        [SerializeField] protected ParticleSystem PTC_Select;
        [SerializeField] protected ParticleSystem PTC_Equip;
        [SerializeField] protected RawImage RIMG_Preview;
        [SerializeField] protected ScrollRect SR_Content;
        [SerializeField] protected RectTransform RTF_Viewport;
        [SerializeField] protected RectTransform RTF_Guide;
        [SerializeField] private AnimationCurve _scrollCurve;
        [SerializeField] private Sprite _purchaseGemButtonSprite;
        [SerializeField] private Sprite _purchaseGemIconSprite;

        private const string PATH_CUSTOM_ITEM_VIEW = "CustomShop/CustomItemView";
        private const string PATH_CUSTOM_ITEM_PREVIEW = "CustomShop/CustomItemPreview";
        private CustomShopViewContext _context = null;
        private Dictionary<int, CustomItemView> _itemViewDict = null;
        private CustomItemPreview _itemPreview = null;

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
                },
                () =>
                {
                    _context.TryPurchase?.Invoke(itemID);
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
        }

        public void SetActiveRedDot(int itemID, bool active)
        {
            if (false == _itemViewDict.TryGetValue(itemID, out var itemView))
                return;

            itemView.SetActive_RedDot(active);
        }
    }
}