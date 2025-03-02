using System;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.Util
{
    public static class UIBehaviourExtensions
    {
        static readonly Rect UVNormal = new Rect(0f, 0f, 1f, 1f);
        static readonly Rect UVFlipX = new Rect(1f, 0f, -1f, 1f);
        static readonly Rect UVFlipY = new Rect(0f, 1f, 1f, -1f);
        static readonly Rect UVFlipXY = new Rect(1f, 1f, -1f, -1f);



        #region Clear
        public static void ClearAsset(this Text src, bool fontOnly = false)
        {
            if (src == null) return;
            src.font = null;

            if (!fontOnly)
                src.material = null;
        }
        public static void ClearAsset(this Image src, bool spriteOnly = false)
        {
            if (src == null) return;
            src.sprite = null;
            src.overrideSprite = null;

            if (!spriteOnly)
                src.material = null;
        }
        public static void ClearAsset(this RawImage src, bool textureOnly = false)
        {
            if (src == null) return;
            src.texture = null;

            if (!textureOnly)
                src.material = null;
        }
        #endregion// Clear


        #region Size
        public static void SetNativeOrMinSize(this Image src, Vector2 size)
        {
            if (GetNativeSize(src, out var result))
            {
                CalcMinSize(size, ref result);
                SetSize(src, result, true);
            }
        }
        public static void SetNativeOrMaxSize(this Image src, Vector2 size)
        {
            if (GetNativeSize(src, out var result))
            {
                CalcMaxSize(size, ref result);
                SetSize(src, result, true);
            }
        }
        static bool GetNativeSize(Image src, out Vector2 result)
        {
            var sprite = src.overrideSprite;
            if (sprite == null) sprite = src.sprite;
            if (sprite == null) { result = default; return false; }

            result.x = sprite.rect.width / src.pixelsPerUnit;
            result.y = sprite.rect.height / src.pixelsPerUnit;
            return true;
        }

        public static void SetNativeOrMinSize(this RawImage src, Vector2 size)
        {
            if (GetNativeSize(src, out var result))
            {
                CalcMinSize(size, ref result);
                SetSize(src, result, false);
            }
        }
        public static void SetNativeOrMaxSize(this RawImage src, Vector2 size)
        {
            if (GetNativeSize(src, out var result))
            {
                CalcMaxSize(size, ref result);
                SetSize(src, result, false);
            }
        }
        static bool GetNativeSize(RawImage src, out Vector2 result)
        {
            var tex = src.mainTexture;
            if (tex == null) { result = default; return false; }

            result.x = Mathf.RoundToInt(tex.width * src.uvRect.width);
            result.y = Mathf.RoundToInt(tex.height * src.uvRect.height);
            return true;
        }

        static void CalcMinSize(Vector2 min, ref Vector2 value)
        {
            if (min.x < 0f) min.x = 0f;
            if (min.y < 0f) min.y = 0f;

            if (value.x <= 0f || value.y <= 0f)
                value = min;
            else if (min.x <= 0f)
            {
                if (0f < min.y && value.y < min.y)
                    value *= min.y / value.y;
            }
            else if (min.y <= 0f)
            {
                if (0f < min.x && value.x < min.x)
                    value *= min.x / value.x;
            }
            else if (value.x < min.x || value.y < min.y)
            {
                var minRatio = min.x / min.y;
                var valueRatio = value.x / value.y;
                value = valueRatio == minRatio ? min
                      : valueRatio < minRatio ? value * (min.x / value.x)
                      : value * (min.y / value.y);
            }
        }
        static void CalcMaxSize(Vector2 max, ref Vector2 value)
        {
            if (max.x <= 0f || max.y <= 0f
             || value.x <= 0f || value.y <= 0f)
                value = Vector2.zero;
            else if (max.x < value.x || max.y < value.y)
            {
                var maxRatio = max.x / max.y;
                var valueRatio = value.x / value.y;
                value = valueRatio == maxRatio ? max
                      : valueRatio < maxRatio ? value * (max.y / value.y)
                      : value * (max.x / value.x);
            }
        }
        static void SetSize(Graphic src, Vector2 size, bool sDirty)
        {
            var rectTransform = src.rectTransform;
            rectTransform.anchorMax = rectTransform.anchorMin;
            rectTransform.sizeDelta = size;
            if (sDirty) src.SetAllDirty();
        }


        public static void SetMinSize<T>(this T src, bool horizontal, bool vertical) where T : Graphic, ILayoutElement
        {
            var size = src.rectTransform.sizeDelta;
            if (horizontal) size.x = src.minWidth;
            if (vertical)   size.y = src.minHeight;
            src.rectTransform.sizeDelta = size;
        }
        public static void SetPreferredSize<T>(this T src, bool horizontal, bool vertical) where T : Graphic, ILayoutElement
        {
            var size = src.rectTransform.sizeDelta;
            if (horizontal) size.x = src.preferredWidth;
            if (vertical)   size.y = src.preferredHeight;
            src.rectTransform.sizeDelta = size;
        }
        public static void SetFlexibleSize<T>(this T src, bool horizontal, bool vertical) where T : Graphic, ILayoutElement
        {
            var size = src.rectTransform.sizeDelta;
            if (horizontal) size.x = src.flexibleWidth;
            if (vertical)   size.y = src.flexibleHeight;
            src.rectTransform.sizeDelta = size;
        }
        #endregion// Size


        static readonly Action<RawImage>[] UVJob = new Action<RawImage>[]
        {
            src => src.uvRect = UVNormal,
            src => src.uvRect = UVFlipX,
            src => src.uvRect = UVFlipY,
            src => src.uvRect = UVFlipXY,
        };
        public static void SetUV(this RawImage src, UVType type) => UVJob[(int)type](src);


        static readonly Func<RectMask2D, float, Vector4>[] Mask2DJob = new Func<RectMask2D, float, Vector4>[]
        {
            (src, ratio) => src.padding = new Vector4(0f, 0f, src.rectTransform.rect.width * (1f - Mathf.Clamp01(ratio)), 0f),
            (src, ratio) => src.padding = new Vector4(0f, src.rectTransform.rect.height * (1f - Mathf.Clamp01(ratio)), 0f, 0f),
            (src, ratio) => src.padding = new Vector4(src.rectTransform.rect.width * (1f - Mathf.Clamp01(ratio)), 0f, 0f, 0f),
            (src, ratio) => src.padding = new Vector4(0f, 0f, 0f, src.rectTransform.rect.height * (1f - Mathf.Clamp01(ratio))),

            (src, ratio) =>
            {
                var half = src.rectTransform.rect.width * (1f - Mathf.Clamp01(ratio)) * 0.5f;
                return new Vector4(half, 0f, half, 0f);
            },
            (src, ratio) =>
            {
                var half = src.rectTransform.rect.height * (1f - Mathf.Clamp01(ratio)) * 0.5f;
                return new Vector4(0f, half, 0f, half);
            },
            (src, ratio) =>
            {
                var half = (1f - Mathf.Clamp01(ratio)) * 0.5f;
                var horizontal = src.rectTransform.rect.width * half;
                var vertical = src.rectTransform.rect.height * half;
                return new Vector4(horizontal, vertical, horizontal, vertical);
            },


            // Left
            (src, ratio) =>
            {
                var value = 1f - Mathf.Clamp01(ratio);
                return new Vector4(0f, src.rectTransform.rect.height * value, src.rectTransform.rect.width * value, 0f);
            },
            (src, ratio) =>
            {
                var value = 1f - Mathf.Clamp01(ratio);
                var half = src.rectTransform.rect.height * value * 0.5f;
                return new Vector4(0f, half, src.rectTransform.rect.width * value, half);
            },
            (src, ratio) =>
            {
                var value = 1f - Mathf.Clamp01(ratio);
                return new Vector4(0f, 0f, src.rectTransform.rect.width * value, src.rectTransform.rect.height * value);
            },

            // Center
            (src, ratio) =>
            {
                var value = 1f - Mathf.Clamp01(ratio);
                var half = src.rectTransform.rect.width * value * 0.5f;
                return new Vector4(half, src.rectTransform.rect.height * value, half, 0f);
            },
            (src, ratio) =>
            {
                var value = 1f - Mathf.Clamp01(ratio);
                var half = src.rectTransform.rect.width * value * 0.5f;
                return new Vector4(half, 0f, half, src.rectTransform.rect.height * value);
            },

            // Right
            (src, ratio) =>
            {
                var value = 1f - Mathf.Clamp01(ratio);
                return new Vector4(src.rectTransform.rect.width * value, src.rectTransform.rect.height * value, 0f);
            },
            (src, ratio) =>
            {
                var value = 1f - Mathf.Clamp01(ratio);
                var half = src.rectTransform.rect.height * value * 0.5f;
                return new Vector4(src.rectTransform.rect.width * value, half, 0f, half);
            },
            (src, ratio) =>
            {
                var value = 1f - Mathf.Clamp01(ratio);
                return new Vector4(src.rectTransform.rect.width * value, 0f, 0f, src.rectTransform.rect.height * value);
            },
        };
        public static void SetFill(this RectMask2D src, FillOrigin type, float ratio) => src.padding = Mask2DJob[(int)type](src, ratio);
        public static void SetFill(this RectMask2D src, FillOrigin type, float ratio, Vector2 scale)
        {
            var padding = Mask2DJob[(int)type](src, ratio);
            padding.x *= scale.x;
            padding.y *= scale.y;
            padding.z *= scale.y;
            padding.w *= scale.x;
            src.padding = padding;
        }

        public static void SetPadding(this RectMask2D src, Vector2 pivot, Vector2 ratio) => src.padding = GetPadding(src, pivot, ratio);
        public static void SetPadding(this RectMask2D src, Vector2 pivot, Vector2 ratio, Vector2 scale)
        {
            var padding = GetPadding(src, pivot, ratio);
            padding.x *= scale.x;
            padding.y *= scale.y;
            padding.z *= scale.y;
            padding.w *= scale.x;
            src.padding = padding;
        }
        static Vector4 GetPadding(this RectMask2D src, Vector2 pivot, Vector2 ratio)
        {
            var pivotX = Mathf.Clamp01(pivot.x);
            var pivotY = Mathf.Clamp01(pivot.y);
            var width = src.rectTransform.rect.width * (1f - Mathf.Clamp01(ratio.x));
            var height = src.rectTransform.rect.height * (1f - Mathf.Clamp01(ratio.y));

            return new Vector4(width * pivotX,
                               height * pivotY,
                               width * (1f - pivotX),
                               height * (1f - pivotY));
        }


        public enum FillOrigin
        {
            Left = 0,
            Top,
            Right,
            Bottom,
            Horizontal,
            Vertical,
            Center,

            LeftTop,
            LeftCenter,
            LeftBottom,
            CenterTop,
            CenterBottom,
            RightTop,
            RightCenter,
            RightBottom,
        }

        public enum UVType
        {
            Normal = 0,
            FlipX,
            FlipY,
            FlipXY,
        }
    }
}
