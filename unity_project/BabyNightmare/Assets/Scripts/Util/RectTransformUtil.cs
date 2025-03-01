using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace BabyNightmare.Util
{
    public static class RectTransformUtil
    {
        public static void SetFullStretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;  // 좌측 하단 (0,0)
            rectTransform.anchorMax = Vector2.one;   // 우측 상단 (1,1)
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // 중심점 (기본값)
            rectTransform.offsetMin = Vector2.zero;  // 좌측 하단 오프셋
            rectTransform.offsetMax = Vector2.zero;  // 우측 상단 오프셋
        }
    }
}