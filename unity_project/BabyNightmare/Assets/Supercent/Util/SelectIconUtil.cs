using System;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Supercent.Util
{
    public static class SelectIconUtil
    {
        //------------------------------------------------------------------------------
        // enums
        //------------------------------------------------------------------------------
        public enum LabelIcon
        {
            Gray = 0,
            Blue,
            Teal,
            Green,
            Yellow,
            Orange,
            Red,
            Purple
        }

        public enum Icon
        {
            CircleGray = 0,
            CircleBlue,
            CircleTeal,
            CircleGreen,
            CircleYellow,
            CircleOrange,
            CircleRed,
            CirclePurple,
            DiamondGray,
            DiamondBlue,
            DiamondTeal,
            DiamondGreen,
            DiamondYellow,
            DiamondOrange,
            DiamondRed,
            DiamondPurple
        }

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private static MethodInfo setIconForObjectMethodInfo;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public static void SetIcon(GameObject gameObject, LabelIcon labelIcon)
        {
            SetIcon(gameObject, $"sv_label_{(int)labelIcon}");
        }

        public static void SetIcon(GameObject gameObject, Icon shapeIcon)
        {
            SetIcon(gameObject, $"sv_icon_dot{(int)shapeIcon}_pix16_gizmo");
        }

        public static void RemoveIcon(GameObject gameObject)
        {
            SetIconForObject(gameObject, null);
        }

        public static void SetIconForObject(GameObject obj, Texture2D icon)
        {
#if UNITY_EDITOR
            if (setIconForObjectMethodInfo == null)
            {
                Type type = typeof(EditorGUIUtility);
                setIconForObjectMethodInfo = type.GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
            }

            setIconForObjectMethodInfo.Invoke(null, new object[] { obj, icon });
#endif            
        }

        private static void SetIcon(GameObject gameObject, string contentName)
        {
#if UNITY_EDITOR            
            GUIContent iconContent = EditorGUIUtility.IconContent(contentName);
            SetIconForObject(gameObject, (Texture2D)iconContent.image);
#endif            
        }
    }
}