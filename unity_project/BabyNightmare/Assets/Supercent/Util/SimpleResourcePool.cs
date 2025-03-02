using System;
using System.Collections.Generic;
using UnityEngine;

namespace Supercent.Util
{
    public class SimpleResourcePool<T> where T : UnityEngine.Object
    {
        //------------------------------------------------------------------------------
        // static 
        //------------------------------------------------------------------------------
        private static Dictionary<Type, SimpleResourcePool<T>> _instanceSet = new Dictionary<Type, SimpleResourcePool<T>>();

        public static T GetResource(string resourcePath)
        {
            if (false == _instanceSet.TryGetValue(typeof(T), out var instance))
            {
                instance = new SimpleResourcePool<T>();
                _instanceSet.Add(typeof(T), instance);
            }

            if (true == instance._loadedResourceSet.TryGetValue(resourcePath, out var resource))
                return resource;

            resource = Resources.Load<T>(resourcePath);
            if (null == resource)
            {
                Debug.LogError($"[SimpleResourcePool<{typeof(T).Name}>.GetResource] 해당 경로의 리소스를 로드하지 못했습니다. path: {resourcePath}");
                return null;
            }

            instance._loadedResourceSet.Add(resourcePath, resource);
            return resource;
        }

        //------------------------------------------------------------------------------
        // instance
        //------------------------------------------------------------------------------
        private SimpleResourcePool() {}

        private Dictionary<string, T> _loadedResourceSet = new Dictionary<string, T>();
    }
}