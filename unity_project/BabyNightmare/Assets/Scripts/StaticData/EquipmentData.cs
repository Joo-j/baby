using UnityEngine;
using BabyNightmare.Util;
using System.Collections.Generic;
using UnityEngine.UIElements;
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
        public string Desc;
        public int Level;
        public List<StatData> StatDataList;
        public float CoolTime;
        public Sprite Sprite;
        public RectShape Shape;


        public int GetStatValueByCool(float value)
        {
            return Mathf.CeilToInt(value / CoolTime);
        }
    }
}