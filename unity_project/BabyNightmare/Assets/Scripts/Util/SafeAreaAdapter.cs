using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Util
{
    public class SafeAreaAdapter : MonoBehaviour
    {
        private void Awake()
        {
            RectTransform rtf = GetComponent<RectTransform>();
            if (null == rtf)
                return;

            Rect safeArea = Screen.safeArea;
            Vector2 minAnchor = safeArea.position;
            Vector2 maxAnchor = minAnchor + safeArea.size;

            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            rtf.anchorMin = minAnchor;
            rtf.anchorMax = maxAnchor;
        }
    }
}