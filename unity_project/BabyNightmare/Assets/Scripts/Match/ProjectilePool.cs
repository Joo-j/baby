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

        public Projectile Get()
        {
            _pool ??= new Pool<Projectile>(() => ObjectUtil.LoadAndInstantiate<Projectile>(PATH_PROJECTILE, null));

            var pj = _pool.Get();
            pj.gameObject.SetActive(true);

            return pj;
        }

        public void Return(Projectile pj)
        {
            pj.gameObject.SetActive(false);
            _pool.Return(pj);
        }
    }
}