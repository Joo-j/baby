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

        private PopupText Get()
        {
            _pool ??= new Pool<PopupText>(() => ObjectUtil.LoadAndInstantiate<PopupText>(PATH_POPUP_TEXT, null));

            var pt = _pool.Get();
            pt.gameObject.SetActive(true);

            return pt;
        }

        private void Return(PopupText pt)
        {
            pt.gameObject.SetActive(false);
            _pool.Return(pt);
        }

        public void ShowTemporary(Vector3 pos, Quaternion rot, string text, Color color)
        {
            var pt = Get();
            pt.transform.position = pos;
            pt.transform.rotation = rot;
            pt.Refresh(text, color, () => Return(pt));
        }
    }
}