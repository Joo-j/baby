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
        public int Width => _shape.Width;
        public int Height => _shape.Height;
        public EquipmentShape Shape => _shape;
    }
}