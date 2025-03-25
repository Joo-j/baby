using System;
using System.Collections;
using System.Collections.Generic;
using BabyNightmare.StaticData;
using UnityEngine;

namespace BabyNightmare.Talent
{
    [CreateAssetMenu(fileName = "TalentData", menuName = "BabyNightmare/TalentData")]
    public class TalentData : ScriptableObject, IComparable<TalentData>
    {
        public ETalentType TalentType;
        public float IncreaseValue;
        public EValueType ValueType;
        public int MaxLevel = 100;
        public int Prob;
        public int Order;

        public int CompareTo(TalentData data)
        {
            return Order.CompareTo(data.Order);
        }
    }
}