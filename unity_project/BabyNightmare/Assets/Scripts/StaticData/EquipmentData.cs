using UnityEngine;
using BabyNightmare.InventorySystem;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "Inventory/EquipmentData")]
    public class EquipmentData : ScriptableObject
    {
        public int ID;
        public string Name;
        public int Level;
        public float Damage;
        public float Heal;
        public float Defence;
        public float CoolTime;
        public float ThrowDuration;
        public Sprite Sprite;
        public EquipmentShape Shape;

        public int Row => Shape.Row;
        public int Column => Shape.Column;
        public bool IsInside(Vector2Int point) => Shape.IsInside(point);
    }
}