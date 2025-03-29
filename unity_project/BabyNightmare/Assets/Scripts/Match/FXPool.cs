using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.Util;

namespace BabyNightmare.Match
{
    public enum EFXType
    {
        Unknown = -1,
        Receive_Damage = 0,
        Equipment_Merge = 1,
        Equipment_Level_2,
        Equipment_Level_3,
        Heal,
        Pop,

        Projectile_Level_2,
        Projectile_Level_3,

        Projectile_Bomb,
        Projectile_Missle,
        Projectile_WaterGun,

        Defense,
        Receive_Damage_2,
        Die,
    }

    public class FXPool : SingletonBase<FXPool>
    {
        private const string PATH_FX = "FX";

        private Dictionary<EFXType, Pool<FX>> _poolDict = null;
        private Transform _poolTF = null;

        public FX Get(EFXType type)
        {
            if (null == _poolDict)
            {
                _poolDict = new Dictionary<EFXType, Pool<FX>>();

                _poolTF = new GameObject("FXPoolTF").transform;

                var resArr = Resources.LoadAll<FX>(PATH_FX);

                for (var i = 0; i < resArr.Length; i++)
                {
                    var res = resArr[i];
                    _poolDict.Add(res.Type, new Pool<FX>(() => ObjectUtil.Instantiate<FX>(res, _poolTF)));
                }
            }

            if (false == _poolDict.TryGetValue(type, out var pool))
            {
                Debug.LogError($"{type}에 관련된 풀이 없습니다.");
                return null;
            }

            return pool.Get();
        }

        public void Return(FX fx)
        {
            if (false == _poolDict.TryGetValue(fx.Type, out var pool))
                return;

            fx.transform.SetParent(_poolTF);

            pool.Return(fx);
        }

        public void ShowTemporary(EFXType type, Vector3 pos)
        {
            var fx = Get(type);
            fx.transform.position = pos;
            GameLifeCycle.Start_Coroutine(SimpleLerp.Co_Invoke(fx.AppearDuration, () => Return(fx)));
        }


        public void ShowTemporary(EFXType type, Vector3 pos, Vector3 scale)
        {
            var fx = Get(type);
            fx.transform.position = pos;
            fx.transform.localScale = scale;
            GameLifeCycle.Start_Coroutine(SimpleLerp.Co_Invoke(fx.AppearDuration, () => Return(fx)));
        }

        public void ShowTemporary(EFXType type, Vector3 pos, Color color)
        {
            var fx = Get(type);
            fx.ChangeStartColor(color);
            fx.transform.position = pos;
            GameLifeCycle.Start_Coroutine(SimpleLerp.Co_Invoke(fx.AppearDuration, () => Return(fx)));
        }

        public void ReturnAll()
        {
            foreach (var pair in _poolDict)
            {
                pair.Value.ReturnAll();
            }
        }
    }
}