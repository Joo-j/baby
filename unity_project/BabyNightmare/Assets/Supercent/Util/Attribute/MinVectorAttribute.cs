using UnityEngine;

namespace Supercent.Util
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(MinVectorAttribute))]
    public class MinVectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var minAttribute = (MinVectorAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);
            {
                if (property.propertyType == SerializedPropertyType.Vector2)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Vector2 vector = property.vector2Value;
                    vector.x = Mathf.Max(vector.x, minAttribute.minX);
                    vector.y = Mathf.Max(vector.y, minAttribute.minY);
                    property.vector2Value = vector;
                }
                else if (property.propertyType == SerializedPropertyType.Vector3)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Vector3 vector = property.vector3Value;
                    vector.x = Mathf.Max(vector.x, minAttribute.minX);
                    vector.y = Mathf.Max(vector.y, minAttribute.minY);
                    vector.z = Mathf.Max(vector.z, minAttribute.minZ);
                    property.vector3Value = vector;
                }
                else if (property.propertyType == SerializedPropertyType.Vector4)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Vector4 vector = property.vector4Value;
                    vector.x = Mathf.Max(vector.x, minAttribute.minX);
                    vector.y = Mathf.Max(vector.y, minAttribute.minY);
                    vector.z = Mathf.Max(vector.z, minAttribute.minZ);
                    vector.w = Mathf.Max(vector.w, minAttribute.minW);
                    property.vector4Value = vector;
                }
                else
                {
                    EditorGUI.LabelField(position, label.text, "Use MinVector with Vector2, Vector3, Vector4.");
                }
            }
            EditorGUI.EndProperty();
        }
    }
#endif// UNITY_EDITOR


    public class MinVectorAttribute : PropertyAttribute
    {
        public float minX;
        public float minY;
        public float minZ;
        public float minW;

        public MinVectorAttribute(float minX, float minY = 0f, float minZ = 0f, float minW = 0f)
        {
            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;
            this.minW = minW;
        }
    }
}
