using System;
using UnityEngine;

namespace BabyNightmare.InventorySystem
{
    [Serializable]
    public class RectShape
    {
        [SerializeField] private int _row;
        [SerializeField] private int _column;
        [SerializeField] private bool[] _shape;

        public int Row => _row;
        public int Column => _column;

        public RectShape(int row, int column)
        {
            _row = row;
            _column = column;
            _shape = new bool[_row * _column];
        }

        public RectShape(bool[,] shape)
        {
            _column = shape.GetLength(0);
            _row = shape.GetLength(1);
            _shape = new bool[_row * _column];
            for (int x = 0; x < _column; x++)
            {
                for (int y = 0; y < _row; y++)
                {
                    _shape[GetIndex(x, y)] = shape[x, y];
                }
            }
        }

        public bool IsValid(Vector2Int point)
        {
            if (point.x < 0 || point.x >= _column || point.y < 0 || point.y >= _row)
                return false;

            var index = GetIndex(point.x, point.y);
            if (index < 0 || index >= _shape.Length)
                return false;

            return _shape[index];
        }

        private int GetIndex(int x, int y)
        {
            y = (_row - 1) - y;
            return x + _column * y;
        }
    }
}