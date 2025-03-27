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
        public float CoolTime;
        public List<StatData> StatDataList;
        public String Desc;
        public int Prob = 1;
        public EDamageType DamageType = EDamageType.Direct;
        public float Radius = 1;
        public RectShape Shape;
        public Mesh Mesh;

        public int GetStatValueByCool(float value)
        {
            return Mathf.CeilToInt(value / CoolTime);
        }
    }
}