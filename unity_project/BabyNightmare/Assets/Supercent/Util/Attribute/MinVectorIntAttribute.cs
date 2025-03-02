using UnityEngine;

namespace Supercent.Util
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(MinVectorIntAttribute))]
    public class MinVectorIntDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var minAttribute = (MinVectorIntAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);
            {
                if (property.propertyType == SerializedPropertyType.Vector2Int)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Vector2Int vector = property.vector2IntValue;
                    vector.x = Mathf.Max(vector.x, minAttribute.minX);
                    vector.y = Mathf.Max(vector.y, minAttribute.minY);
                    property.vector2IntValue = vector;
                }
                else if (property.propertyType == SerializedPropertyType.Vector3Int)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    Vector3Int vector = property.vector3IntValue;
                    vector.x = Mathf.Max(vector.x, minAttribute.minX);
                    vector.y = Mathf.Max(vector.y, minAttribute.minY);
                    vector.z = Mathf.Max(vector.z, minAttribute.minZ);
                    property.vector3IntValue = vector;
                }
                else
                {
                    EditorGUI.LabelField(position, label.text, "Use MinVectorInt with Vector2Int or Vector3Int.");
                }
            }
            EditorGUI.EndProperty();
        }
    }
#endif// UNITY_EDITOR


    public class MinVectorIntAttribute : PropertyAttribute
    {
        public int minX;
        public int minY;
        public int minZ;
        public int minW;

        public MinVectorIntAttribute(int minX, int minY = 0, int minZ = 0, int minW = 0)
        {
            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;
            this.minW = minW;
        }
    }
}
