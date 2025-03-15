using System;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "LobbyButtonData", menuName = "BabyNightmare/LobbyButtonData")]
    public class LobbyButtonData : ScriptableObject, IComparable<LobbyButtonData>
    {
        //[Auto generate code begin]

        public ELobbyButtonType ButtonType;
        public EComparisonType ComparisonType;
        public EConditionType ConditionType;
        public string ConditionValue;
        public int Order;
        public bool ShowOpenAni;

        public int CompareTo(LobbyButtonData other)
        {
            return Order.CompareTo(other.Order);
        }
    }
}