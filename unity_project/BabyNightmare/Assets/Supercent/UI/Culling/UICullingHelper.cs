using System;
using UnityEngine;

namespace Supercent.UI.Culling
{
    public static class UICullingHelper
    {
        private static readonly Vector3[] s_CornerCache = new Vector3[4];


        public static void CheckVisibility(RectTransform rectTransform, RectTransform viewport, Action<bool> OnChangeVisible)
        {
            var visible = CalculateVisibility(rectTransform, viewport);

            // if (visible)
            // {
            //     Debug.LogWarning($"UICullingHelper.CheckVisibility() == {visible}");
            // }

            OnChangeVisible(visible);
        }

        /// <summary>
        /// Calculates whether the rect is inside or overlaps the viewport.
        /// </summary>
        private static bool CalculateVisibility(RectTransform rectTransform, RectTransform viewPort)
        {
            if (rectTransform == null || viewPort == null)
                return false;

            var rect = GetWorldRect(rectTransform);
            if (rect.width <= 0 || rect.height <= 0)
                return false;

            var viewport = GetWorldRect(viewPort);
            if (viewport.width <= 0 || viewport.height <= 0)
                return false;

            var visible = rect.Overlaps(viewport);
            return visible;
        }

        /// <summary>
        /// Gets the rectangle of the specified <paramref name="rectTransform"/> in world-space.
        /// </summary>
        private static Rect GetWorldRect(RectTransform rectTransform)
        {
            // RectTransform.GetWorldCorners isn't particulary fast, see Unity thread:
            // https://forum.unity.com/threads/case-1420752-recttransform-rect-very-expensive.1268918/
            rectTransform.GetWorldCorners(s_CornerCache);

            // s_CornerCache[0] = bottom left
            // s_CornerCache[1] = top left
            // s_CornerCache[2] = top right
            // s_CornerCache[3] = bottom right
            return new Rect(
                s_CornerCache[0].x,
                s_CornerCache[0].y,
                s_CornerCache[2].x - s_CornerCache[0].x,
                s_CornerCache[2].y - s_CornerCache[0].y);
        }

    }
}