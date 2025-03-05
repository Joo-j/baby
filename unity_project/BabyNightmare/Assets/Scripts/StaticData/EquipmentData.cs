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
        public float CoolTime;
        public float ThrowDuration;
        [SerializeField] private EquipmentShape _shape = null;
        [SerializeField] private Sprite _sprite = null;

        public Sprite Sprite => _sprite;
        public int Row => _shape.Row;
        public int Column => _shape.Column;
        public bool IsInside(Vector2Int point) => _shape.IsInside(point);
    }
}