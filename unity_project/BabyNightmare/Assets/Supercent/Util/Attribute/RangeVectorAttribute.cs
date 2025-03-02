using UnityEngine;

namespace Supercent.Util
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(RangeVectorAttribute))]
    public class RangeVectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rangeAttribute = (RangeVectorAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);
            {
                if (property.propertyType == SerializedPropertyType.Vector2)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Vector2 vector = property.vector2Value;
                    vector.x = Mathf.Clamp(vector.x, rangeAttribute.minX, rangeAttribute.maxX);
                    vector.y = Mathf.Clamp(vector.y, rangeAttribute.minY, rangeAttribute.maxY);
                    property.vector2Value = vector;
                }
                else if (property.propertyType == SerializedPropertyType.Vector3)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Vector3 vector = property.vector3Value;
                    vector.x = Mathf.Clamp(vector.x, rangeAttribute.minX, rangeAttribute.maxX);
                    vector.y = Mathf.Clamp(vector.y, rangeAttribute.minY, rangeAttribute.maxY);
                    vector.z = Mathf.Clamp(vector.z, rangeAttribute.minZ, rangeAttribute.maxZ);
                    property.vector3Value = vector;
                }
                else if (property.propertyType == SerializedPropertyType.Vector4)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Vector4 vector = property.vector4Value;
                    vector.x = Mathf.Clamp(vector.x, rangeAttribute.minX, rangeAttribute.maxX);
                    vector.y = Mathf.Clamp(vector.y, rangeAttribute.minY, rangeAttribute.maxY);
                    vector.z = Mathf.Clamp(vector.z, rangeAttribute.minZ, rangeAttribute.maxZ);
                    vector.w = Mathf.Clamp(vector.w, rangeAttribute.minW, rangeAttribute.maxW);
                    property.vector4Value = vector;
                }
                else
                {
                    EditorGUI.LabelField(position, label.text, "Use RangeVector with Vector2, Vector3, Vector4.");
                }
            }
            EditorGUI.EndProperty();
        }
    }
#endif// UNITY_EDITOR


    public class RangeVectorAttribute : PropertyAttribute
    {
        public float minX, maxX;
        public float minY, maxY;
        public float minZ, maxZ;
        public float minW, maxW;

        public RangeVectorAttribute(float minX, float maxX,
                                    float minY = 0, float maxY = 0,
                                    float minZ = 0, float maxZ = 0,
                                    float minW = 0, float maxW = 0)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            this.minZ = minZ;
            this.maxZ = maxZ;
            this.minW = minW;
            this.maxW = maxW;
        }
    }
}
