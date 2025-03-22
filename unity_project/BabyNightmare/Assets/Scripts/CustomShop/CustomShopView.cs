using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using BabyNightmare.StaticData;

namespace BabyNightmare.CustomShop
{
    public class CustomShopViewContext
    {
        public Dictionary<int, CustomItemData> ItemDataDict { get; }
        public Dictionary<int, CustomShopData> ShopDataDict { get; }
        public Dictionary<int, int> RvDataDict { get; }
        public Action<int> TryPurchase { get; }

        public CustomShopViewContext(
            Dictionary<int, CustomItemData> itemDataDict,
            Dictionary<int, CustomShopData> shopDataDict,
            Dictionary<int, int> rvDataDict,
            Action<int> tryPurchase)
        {
            this.ItemDataDict = itemDataDict;
            this.ShopDataDict = shopDataDict;
            this.RvDataDict = rvDataDict;
            this.TryPurchase = tryPurchase;
        }

        public CustomItemData GetItemData(int itemID)
        {
            if (false == ItemDataDict.TryGetValue(itemID, out var itemData))
            {
                Debug.LogError($"{itemID} itemData null");
                return null;
            }

            return itemData;
        }

        public CustomShopData GetShopData(int itemID)
        {
            if (false == ShopDataDict.TryGetValue(itemID, out var shopData))
            {
                Debug.LogError($"{itemID} shop data null");
                return null;
            }

            return shopData;
        }

        public int GetRVCount(int shopDataID)
        {
            if (false == RvDataDict.TryGetValue(shopDataID, out var rvCount))
                return 0;

            return rvCount;
        }
    }

    public class CustomShopView : MonoBehaviour
    {
        [SerializeField] private RawImage RIMG_Preview;
        [SerializeField] private ScrollRect SR_Content;
        [SerializeField] private Image[] _tabArr;
        [SerializeField] private RectTransform[] _contentsArr;
        [SerializeField] private ParticleSystem PTC_Select;
        [SerializeField] private ParticleSystem PTC_Equip;
        [SerializeField] private AnimationCurve _scrollCurve;
        [SerializeField] private Sprite _focusTab;
        [SerializeField] private Sprite _noFocusTab;

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

                var itemData = _context.ItemDataDict[itemID];
                if (null == itemData)
                {
                    Debug.LogError($"{itemID}에 대한 CustomItem이 없습니다.");
                    return;
                }

                var type = itemData.Type;
                var content = _contentsArr[(int)type - 1];

                var itemView = ObjectUtil.LoadAndInstantiate<CustomItemView>(PATH_CUSTOM_ITEM_VIEW, content);
                if (null == itemView)
                {
                    Debug.LogError($"{PATH_CUSTOM_ITEM_VIEW}에 프리팹이 업습니다.");
                    return;
                }

                itemView.Init(
                type,
                shopData,
                itemData.Thumbnail,
                _context.GetRVCount(shopData.ID),
                () =>
                {
                    RefreshPreview(shopData.Item_ID);
                    PTC_Select.Simulate(0f, true, true, false);
                    PTC_Select.Play();
                },
                () =>
                {
                    _context.TryPurchase?.Invoke(itemID);
                });

                _itemViewDict.Add(itemID, itemView);
            }

            OnClickTab(0);
        }

        public void Release()
        {
            _context = null;
            _itemViewDict = null;
            _itemPreview?.Release();
            _itemPreview = null;
        }

        public void Show()
        {
            (transform as RectTransform).SetFullStretch();
            gameObject.SetActive(true);

            PTC_Equip.Stop();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Refresh(HashSet<int> purchasedIDSet, List<int> equipItemIDs)
        {
            for (var i = 0; i < equipItemIDs.Count; i++)
            {
                var id = equipItemIDs[i];
                var shopData = _context.GetShopData(id);
                RefreshPreview(id);
                RefreshItemView(shopData, purchasedIDSet, equipItemIDs);

                Debug.Log($"refresh {id}");
            }
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

            var itemData = _context.GetItemData(itemID);
            _itemPreview.RefreshCustomItem(itemData);
        }

        private void RefreshItemView(CustomShopData shopData, HashSet<int> purchasedIDSet, List<int> equipitemIDList)
        {
            var itemData = _context.GetItemData(shopData.Item_ID);
            var type = itemData.Type;

            foreach (var pair in _itemViewDict)
            {
                var itemView = pair.Value;
                if (type != itemView.Type)
                    continue;

                var itemID = pair.Key;
                itemView.RefreshSelect(itemID == shopData.Item_ID);
                itemView.RefreshEquip(equipitemIDList.Contains(itemID));
                itemView.RefreshCost(purchasedIDSet.Contains(itemID), _context.GetRVCount(itemID));
            }

            for (var i = 0; i < equipitemIDList.Count; i++)
            {
                Scroll(equipitemIDList[i]);
            }
        }

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
                var viewHeight = SR_Content.viewport.rect.height;
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

        public void OnClickTab(int index)
        {
            for (var i = 0; i < _tabArr.Length; i++)
                _tabArr[i].sprite = index == i ? _focusTab : _noFocusTab;

            for (var i = 0; i < _contentsArr.Length; i++)
            {
                _contentsArr[i].gameObject.SetActive(index == i);

                if (index == i)
                    SR_Content.content = _contentsArr[i];
            }
        }
    }
}