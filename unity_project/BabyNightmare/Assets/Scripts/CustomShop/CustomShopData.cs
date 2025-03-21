using System;
using System.Collections.Generic;
using BabyNightmare.StaticData;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "CustomShopData", menuName = "BabyNightmare/CustomShopData")]

    public class CustomShopData : ScriptableObject, IComparable<CustomShopData>
    {
        public int ID;
        public int Item_ID;
        public int Order;
        public ECurrencyType CurrencyType;
        public int Price_Value;

        public int CompareTo(CustomShopData data)
        {
            return Order.CompareTo(data.Order);
        }
    }
}