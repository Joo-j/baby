using UnityEngine;
using BabyNightmare.InventorySystem;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "BabyNightmare/EquipmentData")]
    public class EquipmentData : ScriptableObject
    {
        public int ID;
        public string Name;
        public int Level;
        public float Damage;
        public float Heal;
        public float Defence;
        public float CoolTime;
        public int UpgradeDataID = -1;
        public Sprite Sprite;
        public RectShape Shape;

        public int Row => Shape.Row;
        public int Column => Shape.Column;
        public bool IsValid(Vector2Int point) => Shape.IsValid(point);
    }
}