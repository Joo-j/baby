using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using UnityEngine;
using BabyNightmare.Util;

namespace BabyNightmare.Match
{
    public class PopupTextPool : SingletonBase<PopupTextPool>
    {
        private const string PATH_POPUP_TEXT = "Match/UI/PopupText";

        private Pool<PopupText> _pool = null;
        private Transform _poolTF = null;

        private PopupText Get()
        {
            if (null == _pool)
            {
                _poolTF = new GameObject("PopupTextPoolTF").transform;
                _pool = new Pool<PopupText>(() => ObjectUtil.LoadAndInstantiate<PopupText>(PATH_POPUP_TEXT, _poolTF));
            }

            return _pool.Get();
        }

        private void Return(PopupText pt)
        {
            _pool.Return(pt);
        }

        public void ShowTemporary(Vector3 pos, Quaternion rot, string text, Color color)
        {
            var pt = Get();
            pt.transform.position = pos + Vector3.forward * -2;
            pt.transform.rotation = rot;
            pt.Refresh(text, color, () => Return(pt));
        }
    }
}