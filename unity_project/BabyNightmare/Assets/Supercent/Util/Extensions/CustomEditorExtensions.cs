using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif// UNITY_EDITOR

namespace Supercent.Util
{
    public static class CustomEditorExtension
    {
#if UNITY_EDITOR
        public static void BoolField(string label, ref bool self, ref bool isChanged)
        {
            var b = EditorGUILayout.Toggle(label, self);
            if (b != self)
                isChanged = true;
            self = b;
        }

        public static void EnumField<T>(string label, ref T self, ref bool isChanged) where T : System.Enum
        {
            T t = (T)EditorGUILayout.EnumPopup(label, self);
            if (!System.Enum.Equals(self, t))
                isChanged = true;
            self = t;
        }

        public static void TextField(string label, ref string self, ref bool isChanged)
        {
            var s = EditorGUILayout.TextField(label, self);
            if (s != self)
                isChanged = true;
            self = s;
        }

        public static void ObjectField<T>(string label, ref T self, ref bool isChanged) where T : Object
        {
            var c = (T)EditorGUILayout.ObjectField(label, self, typeof(T), true);
            if (c != self)
                isChanged = true;
            self = c;
        }

        public static void CurveField(string label, ref AnimationCurve self, ref bool isChanged)
        {
            var c = EditorGUILayout.CurveField(label, self);
            if (c != self)
                isChanged = true;
            self = c;   
        }
#endif// UNITY_EDITOR
    }
}