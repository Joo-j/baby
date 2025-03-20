using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.Util;

namespace BabyNightmare.Match
{
    public class ProjectilePool : SingletonBase<ProjectilePool>
    {
        private const string PATH_PROJECTILE = "Match/Projectile";

        private Pool<Projectile> _pool = null;
        private Transform _poolTF = null;

        public Projectile Get()
        {
            if (null == _pool)
            {
                _poolTF = new GameObject("ProjectilePoolTF").transform;
                _pool = new Pool<Projectile>(() => ObjectUtil.LoadAndInstantiate<Projectile>(PATH_PROJECTILE, _poolTF));
            }

            return _pool.Get();
        }

        public void Return(Projectile pj)
        {
            _pool.Return(pj);
        }

        public void ReturnAll()
        {
            _pool.ReturnAll();
        }
    }
}