using System;
using UnityEngine;

namespace BabyNightmare.InventorySystem
{
    [Serializable]
    public class EquipmentShape
    {
        [SerializeField] private int _row;
        [SerializeField] private int _column;
        [SerializeField] private bool[] _shape;

        public int Row => _row;
        public int Column => _column;

        public EquipmentShape(int row, int column)
        {
            _row = row;
            _column = column;
            _shape = new bool[_row * _column];
        }

        public EquipmentShape(bool[,] shape)
        {
            _row = shape.GetLength(0);
            _column = shape.GetLength(1);
            _shape = new bool[_row * _column];
            for (int x = 0; x < _row; x++)
            {
                for (int y = 0; y < _column; y++)
                {
                    _shape[GetIndex(x, y)] = shape[x, y];
                }
            }
        }

        public bool IsInside(Vector2Int point)
        {
            if (point.x < 0 || point.x >= _row || point.y < 0 || point.y >= _column)
                return false;

            var index = GetIndex(point.x, point.y);
            if (index < 0 || index >= _shape.Length)
                return false;

            return _shape[index];
        }

        private int GetIndex(int x, int y)
        {
            y = (_column - 1) - y;
            return x + _row * y;
        }
    }
}