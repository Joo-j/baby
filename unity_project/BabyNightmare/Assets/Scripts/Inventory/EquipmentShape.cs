using System;
using UnityEngine;

namespace BabyNightmare.InventorySystem
{
    [Serializable]
    public class EquipmentShape
    {
        [SerializeField] int _width;
        [SerializeField] int _height;
        [SerializeField] bool[] _shape;

        public int Row => _width;
        public int Column => _height;

        public EquipmentShape(int row, int column)
        {
            _width = row;
            _height = column;
            _shape = new bool[_width * _height];
        }

        public EquipmentShape(bool[,] shape)
        {
            _width = shape.GetLength(0);
            _height = shape.GetLength(1);
            _shape = new bool[_width * _height];
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _shape[GetIndex(x, y)] = shape[x, y];
                }
            }
        }

        public bool IsInside(Vector2Int point)
        {
            if (point.x < 0 || point.x >= _width || point.y < 0 || point.y >= _height)
                return false;

            var index = GetIndex(point.x, point.y);
            if (index < 0 || index >= _shape.Length)
                return false;

            return _shape[index];
        }

        private int GetIndex(int x, int y)
        {
            y = (_height - 1) - y;
            return x + _width * y;
        }
    }
}