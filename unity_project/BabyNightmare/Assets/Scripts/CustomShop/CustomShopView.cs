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
        public Action<int> Select { get; }
        public Action<int> TryPurchase { get; }

        public CustomShopViewContext(
            Dictionary<int, CustomItemData> itemDataDict,
            Dictionary<int, CustomShopData> shopDataDict,
            Dictionary<int, int> rvDataDict,
            Action<int> select,
            Action<int> tryPurchase)
        {
            this.ItemDataDict = itemDataDict;
            this.ShopDataDict = shopDataDict;
            this.RvDataDict = rvDataDict;
            this.Select = select;
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
        private Dictionary<ECustomItemType, Dictionary<int, CustomItemView>> _itemViewDict = null;
        private CustomItemPreview _itemPreview = null;

        public void Init(CustomShopViewContext context)
        {
            _context = context;

            _itemViewDict = new Dictionary<ECustomItemType, Dictionary<int, CustomItemView>>();

            foreach (var pair in _context.ShopDataDict)
            {
                var shopData = pair.Value;
                var itemID = shopData.Item_ID;

                var itemData = _context.ItemDataDict[itemID];
                var type = itemData.Type;
                var content = _contentsArr[(int)type - 1];

                var itemView = ObjectUtil.LoadAndInstantiate<CustomItemView>(PATH_CUSTOM_ITEM_VIEW, content);

                itemView.Init(
                type,
                shopData,
                itemData.Thumbnail,
                _context.GetRVCount(shopData.ID),
                () =>
                {
                    _context.Select?.Invoke(itemID);
                    PTC_Select.Simulate(0f, true, true, false);
                    PTC_Select.Play();
                },
                () =>
                {
                    _context.TryPurchase?.Invoke(itemID);
                });

                if (false == _itemViewDict.ContainsKey(type))
                    _itemViewDict.Add(type, new Dictionary<int, CustomItemView>());

                var typeDict = _itemViewDict[type];
                typeDict.Add(itemID, itemView);
            }

            OnClickTab(0);
        }

        public void Release()
        {
            _context = null;
            _itemViewDict = null;
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

        public void RefreshPurchase(HashSet<int> purchasedIDSet)
        {
            foreach (var id in purchasedIDSet)
            {
                var item = _context.GetItemData(id);
                var type = item.Type;

                var typeDict = _itemViewDict[type];

                foreach (var pair in typeDict)
                {
                    pair.Value.RefreshPurchase(pair.Key == id, _context.GetRVCount(pair.Key));
                }
            }
        }

        public void RefreshEquip(HashSet<int> equipItemIDSet)
        {
            foreach (var id in equipItemIDSet)
            {
                var item = _context.GetItemData(id);
                var type = item.Type;

                var typeDict = _itemViewDict[type];

                foreach (var pair in typeDict)
                {
                    pair.Value.RefreshSelect(pair.Key == id);
                }
            }
        }

        public void RefreshSelect(int selectID)
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

            var itemData = _context.GetItemData(selectID);
            _itemPreview.RefreshCustomItem(itemData);

            foreach (var pair_1 in _itemViewDict)
            {
                var type = pair_1.Key;
                if (type != itemData.Type)
                    continue;
                    
                var typeDict = pair_1.Value;
                foreach (var pair_2 in typeDict)
                {
                    pair_2.Value.RefreshSelect(pair_2.Key == selectID);
                }
            }

            Scroll(selectID);
        }

        //select
        //equip -> select
        //purchase -> equip -> select

        public void OnEquip()
        {
            PTC_Equip.Simulate(0f, true, true, false);
            PTC_Equip.Play();
        }

        public void SetActiveRedDot(int itemID, bool active)
        {
            var itemView = GetItemView(itemID);
            itemView.SetActive_RedDot(active);
        }

        private CustomItemView GetItemView(int itemID)
        {
            foreach (var typeDict in _itemViewDict.Values)
            {
                if (true == typeDict.TryGetValue(itemID, out var itemView))
                    return itemView;
            }

            return null;
        }

        public void OnClickTab(int index)
        {
            for (var i = 0; i < _tabArr.Length; i++)
                _tabArr[i].sprite = index == i ? _focusTab : _noFocusTab;

            for (var i = 0; i < _contentsArr.Length; i++)
            {
                _contentsArr[i].gameObject.SetActive(index == i);

                if (index == i)
                {
                    SR_Content.content = _contentsArr[i];
                }
            }
        }

        private void Scroll(int itemID, Action doneCallback = null)
        {
            var itemView = GetItemView(itemID);

            StartCoroutine(Co_Scroll());

            IEnumerator Co_Scroll()
            {
                yield return CoroutineUtil.WaitForSeconds(0.2f);

                var viewRTF = itemView.RTF;
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

    }
}