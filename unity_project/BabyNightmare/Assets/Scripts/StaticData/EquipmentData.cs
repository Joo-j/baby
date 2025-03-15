using UnityEngine;
using BabyNightmare.Util;
using System.Collections.Generic;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "BabyNightmare/EquipmentData")]
    public class EquipmentData : ScriptableObject
    {
        public int ID;
        public string Name;
        public string Desc;
        public int Level;
        public float Damage;
        public float Heal;
        public float Defence;
        public int Coin;
        public float CoolTime;
        public Sprite Sprite;
        public RectShape Shape;
    }
}