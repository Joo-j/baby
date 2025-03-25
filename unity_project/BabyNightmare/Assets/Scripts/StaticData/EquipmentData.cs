using UnityEngine;
using BabyNightmare.Util;
using System.Collections.Generic;
using System;

namespace BabyNightmare.StaticData
{
    [Serializable]
    public struct StatData
    {
        public EStatType Type;
        public float Value;
    }

    [CreateAssetMenu(fileName = "EquipmentData", menuName = "BabyNightmare/EquipmentData")]
    public class EquipmentData : ScriptableObject
    {
        public int ID;
        public EEquipmentType Type;
        public int Level;
        public int Prob = 1;
        public List<StatData> StatDataList;
        public float CoolTime;
        public EDamageType DamageType = EDamageType.Direct;
        public RectShape Shape;
        public String Desc;

        public int GetStatValueByCool(float value)
        {
            return Mathf.CeilToInt(value / CoolTime);
        }
    }
}