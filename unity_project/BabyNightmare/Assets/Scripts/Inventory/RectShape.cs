using System;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Util
{
    [Serializable]
    public class RectShape
    {
        [SerializeField] private int _column;
        [SerializeField] private int _row;
        [SerializeField] private bool[] _shape;

        public int Column => _column;
        public int Row => _row;

        public List<Vector2Int> ValidIndexList
        {
            get
            {
                var validIndexList = new List<Vector2Int>();
                for (var x = 0; x < _column; x++)
                {
                    for (var y = 0; y < _row; y++)
                    {
                        var index = new Vector2Int(x, y);

                        if (false == IsValid(index))
                            continue;

                        validIndexList.Add(index);
                    }
                }

                return validIndexList;
            }
        }

        public bool IsValid(Vector2Int index)
        {
            if (index.x < 0 || index.y < 0 || index.x >= _column || index.y >= _row)
                return false;

            var arrIndex = GetArrIndex(index.x, index.y);
            return _shape[arrIndex];
        }

        private int GetArrIndex(int x, int y)
        {
            y = (_row - 1) - y;
            return x + _column * y;
        }


    }
}