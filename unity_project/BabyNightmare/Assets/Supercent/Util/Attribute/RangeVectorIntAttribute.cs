using UnityEngine;

namespace Supercent.Util
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(RangeVectorIntAttribute))]
    public class RangeVectorIntDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rangeAttribute = (RangeVectorIntAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);
            {
                if (property.propertyType == SerializedPropertyType.Vector2Int)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Vector2Int vector = property.vector2IntValue;
                    vector.x = Mathf.Clamp(vector.x, rangeAttribute.minX, rangeAttribute.maxX);
                    vector.y = Mathf.Clamp(vector.y, rangeAttribute.minY, rangeAttribute.maxY);
                    property.vector2IntValue = vector;
                }
                else if (property.propertyType == SerializedPropertyType.Vector3Int)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Vector3Int vector = property.vector3IntValue;
                    vector.x = Mathf.Clamp(vector.x, rangeAttribute.minX, rangeAttribute.maxX);
                    vector.y = Mathf.Clamp(vector.y, rangeAttribute.minY, rangeAttribute.maxY);
                    vector.z = Mathf.Clamp(vector.z, rangeAttribute.minZ, rangeAttribute.maxZ);
                    property.vector3IntValue = vector;
                }
                else
                {
                    EditorGUI.LabelField(position, label.text, "Use RangeVectorInt with Vector2Int, Vector3Int.");
                }
            }
            EditorGUI.EndProperty();
        }
    }
#endif// UNITY_EDITOR


    public class RangeVectorIntAttribute : PropertyAttribute
    {
        public int minX, maxX;
        public int minY, maxY;
        public int minZ, maxZ;

        public RangeVectorIntAttribute(int minX, int maxX,
                                       int minY = 0, int maxY = 0,
                                       int minZ = 0, int maxZ = 0)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            this.minZ = minZ;
            this.maxZ = maxZ;
        }
    }
}
