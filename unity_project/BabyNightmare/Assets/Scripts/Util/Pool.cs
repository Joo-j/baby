using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace BabyNightmare.Util
{
    public sealed class Pool<T> where T : class
    {
        private List<T> _itemList = new List<T>();
        private Func<T> _creator = null;

        public Pool(Func<T> creator, int initialCount = 0)
        {
            if (null == creator)
                throw new ArgumentNullException("pCreator");
            if (initialCount < 0)
                throw new ArgumentOutOfRangeException("pInitialCount", "Initial count cannot be negative");

            _creator = creator;
            _itemList.Capacity = initialCount;

            while (_itemList.Count < initialCount)
            {
                _itemList.Add(_creator?.Invoke());
            }
        }

        public T Get()
        {
            if (_itemList.Count == 0)
                return _creator?.Invoke();

            var obj = _itemList[_itemList.Count - 1];
            _itemList.RemoveAt(_itemList.Count - 1);
            return obj;
        }

        public void Return(T item)
        {
            _itemList.Add(item);
        }
    }
}