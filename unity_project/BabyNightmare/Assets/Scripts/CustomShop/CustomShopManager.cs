using System;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using Supercent.Util;
using BabyNightmare.Util;
using BabyNightmare.StaticData;
using BabyNightmare.GlobalEvent;

namespace BabyNightmare.CustomShop
{
    public class CustomShopManager : SingletonBase<CustomShopManager>
    {
        private const string PATH_SAVE_FILE = "custom_shop_save";
        private const string KEY_EQUIPED_ITEM_ID = "fe_ke_eq_fe_id";
        private const string KEY_PURCHASED_ITEM_ID = "fe_ke_pu_fe_id";
        private const string KEY_NEW_ITEM_ID = "fe_ke_new_fe_id";
        private const string KEY_RV_DATA = "fe_ke_rv_da";
        private const string KEY_SHOW_COUNT = "fe_ke_show_count";
        private const string PATH_CUSTOM_ITEM_DATA = "StaticData/CustomItemData/";
        private const string PATH_CUSTOM_SHOP_DATA = "StaticData/CustomShopData/";
        private const string PATH_CUSTOM_SHOP_VIEW = "CustomShop/CustomShopView";
        private const string PATH_CUSTOM_ITEM_REWARD_VIEW = "CustomShop/CustomItemRewardView";
        private readonly List<int> BASIC_ITEM_ID_LIST = new List<int>() { 101, 104, 107 };

        private static readonly LogClassPrinter _printer = new LogClassPrinter("CustomShopManager", "#984313");

        private Dictionary<int, CustomItemData> _itemDataDict = null;
        private Dictionary<int, CustomShopData> _shopDataDict = null;
        private Dictionary<int, int> _rvDataDict = null;
        private CustomShopView _customShopView = null;
        private HashSet<int> _hasItems = null;
        private HashSet<int> _newItems = null;
        private List<int> _equipItemIDs = null;
        private bool _initiated = false;

        public int ShowCount { get; private set; }

        private bool USE_ENCODE
        {
            get
            {
#if UNITY_EDITOR
                return false;
#else
            return true;
#endif
            }
        }

        public void Init()
        {
            if (true == _initiated)
                return;

            var itemArr = Resources.LoadAll<CustomItemData>(PATH_CUSTOM_ITEM_DATA);
            _itemDataDict = new Dictionary<int, CustomItemData>();
            for (var i = 0; i < itemArr.Length; i++)
            {
                var itemData = itemArr[i];
                _itemDataDict.Add(itemData.ID, itemData);
            }

            var shopArr = Resources.LoadAll<CustomShopData>(PATH_CUSTOM_SHOP_DATA);
            _shopDataDict = new Dictionary<int, CustomShopData>();
            for (var i = 0; i < shopArr.Length; i++)
            {
                var shopData = shopArr[i];
                _shopDataDict.Add(shopData.Item_ID, shopData);
            }

            Load();

            _initiated = true;
        }

        public void Release()
        {
            _customShopView?.Release();
            _customShopView = null;
            _hasItems = null;
            _rvDataDict = null;
        }

        public void Show(RectTransform viewParent)
        {
            if (null == _customShopView)
            {
                _customShopView = ObjectUtil.LoadAndInstantiate<CustomShopView>(PATH_CUSTOM_SHOP_VIEW, viewParent);
                if (null == _customShopView)
                {
                    _printer.Error("Show", $"{PATH_CUSTOM_SHOP_VIEW}에 프리팹이 없습니다.");
                    return;
                }

                var context = new CustomShopViewContext(
                                _itemDataDict,
                                _shopDataDict,
                                _rvDataDict,
                                TryPurchase);


                _customShopView.Init(context);
            }

            _customShopView.Show();
            _customShopView.Refresh(_hasItems, _equipItemIDs);

            ShowRedDot();

            ++ShowCount;
        }

        public void Hide()
        {
            if (null == _customShopView)
                return;

            _customShopView.Hide();

            Save();
        }

        private void TryPurchase(int itemID)
        {
            if (true == _hasItems.Contains(itemID))
            {
                _printer.Error("TryPurchase", $"이미 가지고 있는 아이템 입니다. {itemID}.");
                return;
            }

            if (false == _shopDataDict.TryGetValue(itemID, out var shopData))
            {
                _printer.Error("TryPurchase", $"{itemID}에 해당하는 ItemData가 없습니다.");
                return;
            }

            var currencyType = shopData.CurrencyType;
            var price = shopData.Price_Value;

            switch (currencyType)
            {
                case ECurrencyType.Gem:

                    PlayerData.Instance.Gem -= price;
                    ShowCustomItemRewardView(itemID, true, () => EquipItem(itemID), "CustomShop", currencyType.ToString(), price.ToString());
                    Save();
                    break;
            }
        }

        private void Purchase(int itemID, bool equip)
        {
            if (true == _hasItems.Contains(itemID))
            {
                _printer.Log("Purchase", $"itemID:{itemID} 이미 존재하는 먹이입니다.");
                return;
            }

            _hasItems.Add(itemID);
            AddNewItem(itemID);

            if (true == equip)
                EquipItem(itemID);

            Save();
            GlobalEventManager.AddValue(EGlobalEventType.Purchase_CustomItem, 1);

            _printer.Log("Purchase", $"Item ID {itemID} 구매 완료");
        }

        public bool IsPurchased(int itemID) => _hasItems.Contains(itemID);

        private void EquipItem(int itemID)
        {
            var itemData = GetItemData(itemID);

            for (var i = 0; i < _equipItemIDs.Count; i++)
            {
                var id = _equipItemIDs[i];
                var equipItemData = GetItemData(id);
                if (itemData.Type == equipItemData.Type)
                {
                    _equipItemIDs.Remove(id);
                    break;
                }
            }

            _equipItemIDs.Add(itemData.ID);

            _customShopView?.Refresh(_hasItems, _equipItemIDs);
            _customShopView?.OnEquip();
            RemoveNewItem(itemID);
        }

        public CustomItemData GetItemData(int itemID)
        {
            if (false == _itemDataDict.TryGetValue(itemID, out var itemData))
                return null;

            return itemData;
        }

        public List<CustomItemData> GetEquipItemListData()
        {
            var list = new List<CustomItemData>();

            for (var i = 0; i < _equipItemIDs.Count; i++)
            {
                var id = _equipItemIDs[i];
                var itemData = GetItemData(id);
                if (null == itemData)
                {
                    _printer.Error("GetEquipItemListData", $"{id}에 해당하는 ItemData이 없습니다.");
                    return null;
                }

                list.Add(itemData);
            }

            return list;
        }

        private void AddNewItem(int itemID)
        {
            if (null != _customShopView && true == _customShopView.gameObject.activeSelf)
                return;

            if (true == BASIC_ITEM_ID_LIST.Contains(itemID))
                return;

            _newItems.Add(itemID);
            ShowRedDot();
        }

        private void RemoveNewItem(int itemID)
        {
            if (null != _customShopView && false == _customShopView.gameObject.activeSelf)
                return;

            if (true == BASIC_ITEM_ID_LIST.Contains(itemID))
                return;

            _newItems.Remove(itemID);
            _customShopView?.SetActiveRedDot(itemID, false);
        }

        private void ShowRedDot()
        {
            if (null == _customShopView)
                return;

            foreach (var itemID in _newItems)
            {
                _customShopView.SetActiveRedDot(itemID, true);
            }
        }

        /// <summary>
        /// 획득한 먹이 보상 뷰를 열어주며, 스킨을 즉시 지급한다.
        /// </summary>
        public void ShowCustomItemRewardView(int itemID, bool equip, Action doneCallback, string location, string currencyType, string currencyAmount)
        {
            var itemData = GetItemData(itemID);

            var itemRewardView = ObjectUtil.LoadAndInstantiate<CustomItemRewardView>(PATH_CUSTOM_ITEM_REWARD_VIEW, null);
            itemRewardView.Init(
            equip,
            itemData,
            doneCallback,
            () =>
            {
                EquipItem(itemID);
                doneCallback?.Invoke();
            });

            Purchase(itemID, equip);
        }

        private void Load()
        {
            _hasItems = new HashSet<int>();
            _newItems = new HashSet<int>();
            _rvDataDict = new Dictionary<int, int>();
            _equipItemIDs = new List<int>();

            var rvIDList = new List<int>();

            foreach (var pair in _itemDataDict)
            {
                var data = pair.Value;
                var id = data.ID;

                if (false == _shopDataDict.TryGetValue(id, out var shopData))
                {
                    _printer.Error("OnClickPurchase", $"{id}에 해당하는 ItemData가 없습니다.");
                    return;
                }

                if (shopData.CurrencyType != ECurrencyType.RV)
                    continue;

                rvIDList.Add(id);
                _rvDataDict.Add(id, 0);
            }

            var binaryData = FileSaveUtil.Load(PATH_SAVE_FILE, USE_ENCODE, USE_ENCODE);
            if (true == binaryData.IsNullOrEmpty())
            {
                for (var i = 0; i < BASIC_ITEM_ID_LIST.Count; i++)
                {
                    Purchase(BASIC_ITEM_ID_LIST[i], true);
                }
                return;
            }

            var jsonClass = JSONClass.Parse(binaryData);
            if (null == jsonClass)
                return;

            var purchasedIDArr = jsonClass[KEY_PURCHASED_ITEM_ID] as JSONArray;
            if (null != purchasedIDArr)
            {
                foreach (JSONNode node in purchasedIDArr)
                {
                    if (false == int.TryParse(node, out var id))
                        continue;

                    _hasItems.Add(id);
                }
            }

            var newitemIDArr = jsonClass[KEY_NEW_ITEM_ID] as JSONArray;
            if (null != newitemIDArr)
            {
                foreach (JSONNode node in newitemIDArr)
                {
                    if (false == int.TryParse(node, out var id))
                        continue;

                    _newItems.Add(id);
                }
            }

            var equipItemIDArr = jsonClass[KEY_NEW_ITEM_ID] as JSONArray;
            if (null != equipItemIDArr)
            {
                foreach (JSONNode node in equipItemIDArr)
                {
                    if (false == int.TryParse(node, out var id))
                        continue;

                    _equipItemIDs.Add(id);
                }
            }

            for (var i = 0; i < rvIDList.Count; i++)
            {
                var id = rvIDList[i];
                var key = $"{KEY_RV_DATA}_{id}";
                var value = jsonClass[key]?.AsInt ?? 0;

                _rvDataDict[id] = value;
            }

            ShowCount = jsonClass[KEY_SHOW_COUNT]?.AsInt ?? 0;
            _printer.Log("Load", "불러오기에 성공");
        }

        private void Save()
        {
            var jsonClass = new JSONClass();
            jsonClass.Add(KEY_SHOW_COUNT, ShowCount.ToString());

            var jsonArray = new JSONArray();

            foreach (var id in _equipItemIDs)
            {
                jsonArray.Add(new JSONData(id));
            }

            jsonClass.Add(KEY_EQUIPED_ITEM_ID, jsonArray);

            jsonArray = new JSONArray();
            foreach (var id in _hasItems)
            {
                jsonArray.Add(new JSONData(id));
            }

            jsonClass.Add(KEY_PURCHASED_ITEM_ID, jsonArray);

            jsonArray = new JSONArray();
            foreach (var id in _newItems)
            {
                jsonArray.Add(new JSONData(id));
            }

            jsonClass.Add(KEY_NEW_ITEM_ID, jsonArray);

            foreach (var pair in _rvDataDict)
            {
                var key = $"{KEY_RV_DATA}_{pair.Key}";
                jsonClass.Add(key, pair.Value.ToString());
            }

            var binaryData = jsonClass.ToString();
            FileSaveUtil.Save(PATH_SAVE_FILE, binaryData, USE_ENCODE, USE_ENCODE);
            _printer.Log("Save", "저장에 성공");
        }
    }
}