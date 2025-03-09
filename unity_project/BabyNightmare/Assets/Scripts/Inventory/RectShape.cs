using System;
using UnityEngine;

namespace BabyNightmare.InventorySystem
{
    [Serializable]
    public class RectShape
    {
        [SerializeField] private int _column;
        [SerializeField] private int _row;
        [SerializeField] private bool[] _shape;

        public int Column => _column;
        public int Row => _row;

        public bool IsValid(Vector2Int index)
        {
            if (index.x < 0 || index.x >= _column || index.y < 0 || index.y >= _row)
                return false;

            var arrIndex = GetArrIndex(index.x, index.y);
            if (arrIndex < 0 || arrIndex >= _shape.Length)
                return false;

            return _shape[arrIndex];
        }

        private int GetArrIndex(int x, int y)
        {
            y = (_row - 1) - y;
            return x + _column * y;
        }
    }
}