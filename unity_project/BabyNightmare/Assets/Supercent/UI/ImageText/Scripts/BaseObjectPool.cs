using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Supercent.UI
{
    public abstract class BaseObjectPool<T, TEnum> : MonoBehaviour where T : Component, Supercent.Util.IPoolObject where TEnum : Enum
    {
        [SerializeField]
        protected T _prefabTarget  = default(T);

        [SerializeField]
        protected Transform _parent = null;

        [SerializeField]
        protected int _createCount = 0;

        [SerializeField]
        protected bool _isInit = false;

        [SerializeField]
        protected TEnum _enumType;
        public TEnum EumType => _enumType;


        protected Supercent.Util.ObjectPool<T> _pool = null;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        public virtual void Init()
        {
            if(true == _isInit)
            {
                return;
            }

            if(null == _prefabTarget)
            {
                Debug.LogError($"BaseObjectPool Init Error, _prefabTarget is null");

                return;
            }

            if(null == _parent)
            {
                _parent = this.transform;
            }

            _pool = new Supercent.Util.ObjectPool<T>(_prefabTarget, _parent, _createCount);

            OnInit();
            
            _prefabTarget = null;

            _isInit = true;
        }

        public abstract void OnInit();
        
        public void Release()
        {
            _parent = null;

            Clear();
        }

        public virtual void SetPrefabTarget()
        {
            _prefabTarget = Resources.Load<T>(ImageTextSetting.BaseImageTextPrefabPath);
        }

        public void SetParent(Transform parent)
        {
            _parent = parent;
        }

        public void SetCapacity(int capacity)
        {
            _createCount = capacity;
        }

        public void SetEnumType(TEnum enumType)
        {
            _enumType = enumType;
        }

        public T Get()
        {
            if(null == _pool)
            {
                Debug.LogError($"_pool is null");

            }

            return _pool.Get();
        }

        public void Return(T module)
        {
            _pool.Return(module);
        }

        public void ReturnAll()
        {
            _pool.ReturnAll();
        }

        private void Clear()
        {
            _pool?.Clear();

        }
    }
}
