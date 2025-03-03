using System.Collections.Generic;
using System;
using System.Linq;

namespace BabyNightmare.Util
{
    /// <summary>
    /// A generic pool of objects that can be retrieved and recycled without invoking additional allocations.
    /// 
	/// Please note that care must be taken when pooling objects, since the object
	/// has to be manually reset after retrieval from the pool. Its constructor will
	/// not be run again after the first time!
    /// </summary>
    public sealed class Pool<T> where T : class
    {
        private List<T> _inactive = new List<T>();
        private List<T> _active = new List<T>();
        private Func<T> _creator;
        private bool _allowTakingWhenEmpty;

        public Pool(Func<T> creator, int initialCount = 0, bool allowTakingWhenEmpty = true)
        {
            if (creator == null) throw new ArgumentNullException("pCreator");
            if (initialCount < 0) throw new ArgumentOutOfRangeException("pInitialCount", "Initial count cannot be negative");

            _creator = creator;
            _inactive.Capacity = initialCount;
            _allowTakingWhenEmpty = allowTakingWhenEmpty;

            // Create initial items
            while (_inactive.Count < initialCount)
            {
                _inactive.Add(_creator());
            }
        }

        public int Count => _inactive.Count;
        public bool IsEmpty => Count == 0;

        public T Get()
        {
            if (IsEmpty)
            {
                if (_allowTakingWhenEmpty)
                {
                    var obj = _creator();
                    _inactive.Add(obj);
                    return GetInternal();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return GetInternal();
            }
        }

        private T GetInternal()
        {
            T obj = _inactive[_inactive.Count - 1];
            _inactive.RemoveAt(_inactive.Count - 1);
            _active.Add(obj);
            return obj;
        }

        public void Return(T item)
        {
            if (false == _active.Contains(item))
            {
                throw new InvalidOperationException("An item was recycled even though it was not part of the pool");
            }

            _inactive.Add(item);
            _active.Remove(item);
        }
        
        public List<T> GetInactive()
        {
            return _inactive.ToList();
        }

        public List<T> GetActive()
        {
            return _active.ToList();
        }
    }
}