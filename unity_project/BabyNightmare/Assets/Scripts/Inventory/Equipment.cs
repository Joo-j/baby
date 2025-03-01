using UnityEngine;
using BabyNightmare.StaticData;

namespace BabyNightmare.InventorySystem
{
    [CreateAssetMenu(fileName = "Equipment", menuName = "Inventory/Equipment")]
    public class Equipment : ScriptableObject
    {
        [SerializeField] private Sprite _sprite = null;
        [SerializeField] private EquipmentShape _shape = null;
        [SerializeField] private EEquipmentType _type = EEquipmentType.Utility;
        [SerializeField] private bool _canDrop = true;
        [SerializeField, HideInInspector] private Vector2Int _position = Vector2Int.zero;

        public string Name => this.name;
        public EEquipmentType Type => _type;
        public Sprite Sprite => _sprite;
        public int Width => _shape.Width;
        public int Height => _shape.Height;

        public Vector2Int Position
        {
            get => _position;
            set => _position = value;
        }

        public bool IsPartOfShape(Vector2Int localPosition)
        {
            return _shape.IsPartOfShape(localPosition);
        }

        public bool CanDrop => _canDrop;

        public Equipment CreateInstance()
        {
            var clone = ScriptableObject.Instantiate(this);
            clone.name = clone.name.Substring(0, clone.name.Length - 7);
            return clone;
        }
    }
}