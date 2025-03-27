using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using UnityEngine;
using BabyNightmare.Util;

namespace BabyNightmare.Match
{
    public enum EPopupTextType
    {
        Damage,
        Heal,
    }

    public class PopupTextPool : SingletonBase<PopupTextPool>
    {
        private const string PATH_POPUP_TEXT = "Match/UI";

        private Dictionary<EPopupTextType, Pool<PopupText>> _poolDict = null;
        private Transform _poolTF = null;

        private PopupText Get(EPopupTextType type)
        {
            if (null == _poolDict)
            {
                _poolTF = new GameObject("PopupTextPoolTF").transform;

                _poolDict = new Dictionary<EPopupTextType, Pool<PopupText>>();

                var resArr = Resources.LoadAll<PopupText>(PATH_POPUP_TEXT);
                for (var i = 0; i < resArr.Length; i++)
                {
                    var res = resArr[i];
                    _poolDict.Add(res.Type, new Pool<PopupText>(() => ObjectUtil.Instantiate<PopupText>(res, _poolTF)));
                }
            }

            if (false == _poolDict.TryGetValue(type, out var pool))
                return null;

            return pool.Get();
        }

        private void Return(PopupText pt)
        {
            if (false == _poolDict.TryGetValue(pt.Type, out var pool))
                return;

            pool.Return(pt);
        }

        public void ShowTemporary(EPopupTextType type, Vector3 pos, Quaternion rot, Vector3 scale, string text)
        {
            var pt = Get(type);
            pt.transform.position = pos + Vector3.forward * -2;
            pt.transform.rotation = rot;
            pt.transform.localScale = scale;
            pt.Refresh(text, () => Return(pt));
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