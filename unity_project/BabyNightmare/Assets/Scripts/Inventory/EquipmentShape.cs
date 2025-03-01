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

        public EquipmentShape(int width, int height)
        {
            _width = width;
            _height = height;
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

        public int Width => _width;
        public int Height => _height;

        public bool IsPartOfShape(Vector2Int localPoint)
        {
            if (localPoint.x < 0 || localPoint.x >= _width || localPoint.y < 0 || localPoint.y >= _height)
                return false;

            var index = GetIndex(localPoint.x, localPoint.y);

            if (index < 0 || index >= _shape.Length)
            {
                //Debug.Log($"0 < {index} < {_shape.Length}");
                return false;
            }

            return _shape[index];
        }

        private int GetIndex(int x, int y)
        {
            y = (_height - 1) - y;
            return x + _width * y;
        }
    }
}