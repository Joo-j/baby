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
        public EEquipmentGrade Grade;
        [SerializeField] private EEquipmentType _type = EEquipmentType.Weapons;
        [SerializeField] private EquipmentShape _shape = null;
        [SerializeField] private Sprite _sprite = null;

        public EEquipmentType Type => _type;
        public Sprite Image => _sprite;
        public int Width => _shape.Width;
        public int Height => _shape.Height;

        public bool IsPartOfShape(Vector2Int localPosition)
        {
            return _shape.IsPartOfShape(localPosition);
        }

        public Vector2Int Position;
    }
}