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
        public string Name;
        public int Level;
        public List<StatData> StatDataList;
        public float CoolTime;
        public RectShape Shape;

        public int GetStatValueByCool(float value)
        {
            return Mathf.CeilToInt(value / CoolTime);
        }
    }
}