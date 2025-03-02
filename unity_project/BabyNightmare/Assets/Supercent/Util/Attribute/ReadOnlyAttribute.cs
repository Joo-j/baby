using System;
using UnityEngine;

namespace Supercent.Util
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute), true)]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label, true);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = !Application.isPlaying && (attribute as ReadOnlyAttribute).RuntimeOnly;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif// UNITY_EDITOR


    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public readonly bool RuntimeOnly = false;

        public ReadOnlyAttribute(bool runtimeOnly = false) => RuntimeOnly = runtimeOnly;
    }
}