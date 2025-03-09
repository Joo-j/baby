using UnityEngine;
using BabyNightmare.InventorySystem;
using System.Collections.Generic;

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

        private List<Vector2Int> _indexList = null;

        public int Row => Shape.Row;
        public int Column => Shape.Column;
        public List<Vector2Int> IndexList
        {
            get
            {
                if (null == _indexList)
                {
                    _indexList = new List<Vector2Int>();
                    for (var x = 0; x < Column; x++)
                    {
                        for (var y = 0; y < Row; y++)
                        {
                            var index = new Vector2Int(x, y);
                            if (false == IsValid(index))
                                continue;

                            _indexList.Add(index);
                        }
                    }
                }

                return _indexList;
            }
        }

        public bool IsValid(Vector2Int index) => Shape.IsValid(index);
    }
}