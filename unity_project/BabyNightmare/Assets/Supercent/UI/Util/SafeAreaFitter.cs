using System;
using UnityEngine;

namespace Supercent.UI.Util
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        [Flags]
        public enum AreaType
        {
            None        = 0,
            Vertical    = Top | Bottom,
            Horizontal  = Left | Right,

            Top         = 0x01,
            Bottom      = 0x02,
            Left        = 0x04,
            Right       = 0x08,
        }

        [SerializeField] bool isOuter = false;
        [SerializeField] AreaType type = AreaType.Vertical;

        RectTransform rectTransform = null;

        static double stampUpdate = -1d;
        static double stampCalc = -1d;
        double stampSet = -1d;

        static Rect safeArea = default;
        static float width = default;
        static float height = default;
        static Vector2 ratioMin = default;
        static Vector2 ratioMax = default;



        void Awake()
        {
            rectTransform = transform as RectTransform;
        }
        void LateUpdate()
        {
            var stampCur = Time.unscaledTimeAsDouble;
            if (stampUpdate < stampCur)
            {
                stampUpdate = stampCur;
                CalcRatio(stampCur);
            }
            if (stampSet < stampCalc)
                SetRatio(stampCur);
        }

        void CalcRatio(double stamp)
        {
            var safeAreaCur = Screen.safeArea;
            var widthCur = Screen.width;
            var heightCur = Screen.height;
            if (safeAreaCur.Equals(safeArea)
             && widthCur == width
             && heightCur == height)
                return;

            stampCalc = stamp;
            safeArea = safeAreaCur;
            width = widthCur;
            height = heightCur;

            ratioMin.x = safeArea.xMin / width;
            ratioMin.y = safeArea.yMin / height;

            ratioMax.x = safeArea.xMax / width;
            ratioMax.y = safeArea.yMax / height;
        }
        void SetRatio(double stamp)
        {
            stampSet = stamp;
            if (rectTransform == null)
                return;

            // Outer
            if (isOuter)
            {
                var min = new Vector2();
                var max = new Vector2();

                if (type.HasFlag(AreaType.Horizontal))
                {
                    min.x = ratioMin.x;
                    max.x = ratioMax.x;
                }
                else
                {
                    if (type.HasFlag(AreaType.Left))
                    {
                        min.x = 0f;
                        max.x = ratioMin.x;
                    }
                    else if (type.HasFlag(AreaType.Right))
                    {
                        min.x = ratioMax.x;
                        max.x = 1f;
                    }
                    else
                    {
                        min.x = 0f;
                        max.x = 1f;
                    }
                }

                if (type.HasFlag(AreaType.Vertical))
                {
                    min.y = ratioMin.y;
                    max.y = ratioMax.y;
                }
                else
                {
                    if (type.HasFlag(AreaType.Bottom))
                    {
                        min.y = 0f;
                        max.y = ratioMin.y;
                    }
                    else if (type.HasFlag(AreaType.Top))
                    {
                        min.y = ratioMax.y;
                        max.y = 1f;
                    }
                    else
                    {
                        min.y = 0f;
                        max.y = 1f;
                    }
                }

                rectTransform.anchorMin = min;
                rectTransform.anchorMax = max;
            }
            // Inner
            else
            {
                rectTransform.anchorMin = new Vector2
                (
                    type.HasFlag(AreaType.Left) ? ratioMin.x : 0f,
                    type.HasFlag(AreaType.Bottom) ? ratioMin.y : 0f
                );
                rectTransform.anchorMax = new Vector2
                (
                    type.HasFlag(AreaType.Right) ? ratioMax.x : 1f,
                    type.HasFlag(AreaType.Top) ? ratioMax.y : 1f
                );
            }
        }



#if UNITY_EDITOR
        [Header("#Edit")]
        [SerializeField] bool edit_visibleGizmos;
        [SerializeField] bool edit_logScreenInfo;

        void OnValidate()
        {
            if (edit_logScreenInfo)
            {
                edit_logScreenInfo = false;
                Debug.Log($"{nameof(Screen)} : {Screen.width}x{Screen.height}, SafeArea : {Screen.safeArea}, Orientation : {Screen.orientation}");
            }
        }

        void OnDrawGizmos()
        {
            if (!edit_visibleGizmos) return;
            if (rectTransform == null) return;

            var old = Gizmos.color;
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                
                Gizmos.DrawCube
                (
                    rectTransform.position + (Vector3)(rectTransform.rect.center * rectTransform.lossyScale),
                    rectTransform.rect.size * rectTransform.lossyScale
                );
            }
            Gizmos.color = old;
        }
#endif// UNITY_EDITOR
    }
}