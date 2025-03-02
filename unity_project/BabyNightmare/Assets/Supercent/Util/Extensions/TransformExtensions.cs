using System;
using UnityEngine;

namespace Supercent.Util
{
    public static class TransformExtensions
    {
        static readonly Vector2 ZeroHalf = new Vector2(0f, 0.5f);
        static readonly Vector2 HalfOne = new Vector2(0.5f, 1f);
        static readonly Vector2 Half = new Vector2(0.5f, 0.5f);
        static readonly Vector2 HalfZero = new Vector2(0.5f, 0f);
        static readonly Vector2 OneHalf = new Vector2(1f, 0.5f);


        public static void LookAt_OnlyYAxis(this Transform tf, Transform lookTarget) => tf.LookAt_OnlyYAxis(lookTarget.position);
        public static void LookAt_OnlyYAxis(this Transform tf, Vector3 lookTarget)
        {
            var degree = LookAngle_OnlyYAxis(tf, lookTarget);
            tf.rotation = Quaternion.Euler(0.0f, degree, 0.0f);
        }

        public static float LookAngle_OnlyYAxis(this Transform tf, Transform lookTarget) => tf.LookAngle_OnlyYAxis(lookTarget.position);
        public static float LookAngle_OnlyYAxis(this Transform tf, Vector3 lookTarget)
        {
            var dir = lookTarget - tf.position;
            var degree = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            return degree;
        }


        #region Anchor, Pivot
        public static void SetFullStretch(this RectTransform src)
        {
            src.anchorMin = Vector2.zero;
            src.anchorMax = Vector2.one;
            src.sizeDelta = Vector2.zero;
            src.anchoredPosition = Vector2.zero;
        }


        static readonly Action<RectTransform>[] AnchorJob = new Action<RectTransform>[]
        {
            src => { src.anchorMin = Vector2.up; src.anchorMax = Vector2.up; },
            src => { src.anchorMin = ZeroHalf; src.anchorMax = ZeroHalf; },
            src => { src.anchorMin = Vector2.zero; src.anchorMax = Vector2.zero; },

            src => { src.anchorMin = HalfOne; src.anchorMax = HalfOne; },
            src => { src.anchorMin = Half; src.anchorMax = Half; },
            src => { src.anchorMin = HalfZero; src.anchorMax = HalfZero; },

            src => { src.anchorMin = Vector2.one; src.anchorMax = Vector2.one; },
            src => { src.anchorMin = OneHalf; src.anchorMax = OneHalf; },
            src => { src.anchorMin = Vector2.right; src.anchorMax = Vector2.right; },

            src => { src.anchorMin = Vector2.zero; src.anchorMax = Vector2.up; },
            src => { src.anchorMin = HalfZero; src.anchorMax = HalfOne; },
            src => { src.anchorMin = Vector2.right; src.anchorMax = Vector2.one; },

            src => { src.anchorMin = Vector2.up; src.anchorMax = Vector2.one; },
            src => { src.anchorMin = ZeroHalf; src.anchorMax = OneHalf; },
            src => { src.anchorMin = Vector2.zero; src.anchorMax = Vector2.right; },

            src => { src.anchorMin = Vector2.zero; src.anchorMax = Vector2.one; },
        };

        public static void SetAnchor(this RectTransform src, AnchorType type)   => AnchorJob[(int)type](src);
        public static void SetAnchor(this RectTransform src, Rect rect)         { src.anchorMin = rect.min; src.anchorMax = rect.max; }


        static readonly Action<RectTransform>[] PivotJob = new Action<RectTransform>[]
        {
            src => src.pivot = Vector2.up,
            src => src.pivot = ZeroHalf,
            src => src.pivot = Vector2.zero,

            src => src.pivot = HalfOne,
            src => src.pivot = Half,
            src => src.pivot = HalfZero,

            src => src.pivot = Vector2.one,
            src => src.pivot = OneHalf,
            src => src.pivot = Vector2.right,
        };

        public static void SetPivot(this RectTransform src, PivotType type) => PivotJob[(int)type](src);
        #endregion// Anchor, Pivot


        public enum AnchorType
        {
            LeftTop = 0,
            LeftMiddle,
            LeftBottom,

            CenterTop,
            CenterMiddle,
            CenterBottom,

            RightTop,
            RightMiddle,
            RightBottom,

            LeftStretch,
            CenterStretch,
            RightStretch,

            TopStretch,
            MiddleStretch,
            BottomStretch,

            FullStretch,
        }

        public enum PivotType
        {
            LeftTop = 0,
            LeftMiddle,
            LeftBottom,

            CenterTop,
            CenterMiddle,
            CenterBottom,

            RightTop,
            RightMiddle,
            RightBottom,
        }
    }
}