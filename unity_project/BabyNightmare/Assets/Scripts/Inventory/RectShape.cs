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

        private List<Vector2Int> _indexList = null;
        public int Column => _column;
        public int Row => _row;

        public List<Vector2Int> IndexList
        {
            get
            {
                if (null == _indexList)
                {
                    _indexList = new List<Vector2Int>();
                    for (var x = 0; x < _column; x++)
                    {
                        for (var y = 0; y < _row; y++)
                        {
                            var index = new Vector2Int(x, y);

                            Debug.Log(index);

                            if (false == IsValid(index))
                                continue;

                            _indexList.Add(index);
                        }
                    }
                }
                
                Debug.Assert(_indexList.Count != 0, "index count is 0");

                return _indexList;
            }
        }

        public bool IsValid(Vector2Int index)
        {
            if (index.x < 0 || index.x >= _column || index.y < 0 || index.y >= _row)
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