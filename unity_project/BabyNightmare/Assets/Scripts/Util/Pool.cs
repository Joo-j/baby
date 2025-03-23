using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace BabyNightmare.Util
{
    public sealed class Pool<T> where T : Component
    {
        private List<T> _itemList = new List<T>();
        private List<T> _activeList = new List<T>();
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
                var item = _creator?.Invoke();
                item.gameObject.SetActive(false);
                _itemList.Add(item);
            }
        }

        public T Get()
        {
            if (_itemList.Count == 0)
                return _creator?.Invoke();

            var item = _itemList[_itemList.Count - 1];
            _itemList.RemoveAt(_itemList.Count - 1);

            if (null == item.gameObject)
                return Get();

            _activeList.Add(item);
            item.gameObject.SetActive(true);

            return item;
        }

        public void Return(T item)
        {
            item.gameObject.SetActive(false);

            _itemList.Add(item);
            _activeList.Remove(item);
        }

        public void ReturnAll()
        {
            for (var i = 0; i < _activeList.Count; i++)
            {
                var item = _activeList[i];
                item.gameObject.SetActive(false);
                _itemList.Add(item);
            }

            _activeList.Clear();
        }
    }
}