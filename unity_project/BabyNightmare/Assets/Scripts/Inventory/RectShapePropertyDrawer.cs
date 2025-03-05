using UnityEditor;
using UnityEngine;

namespace BabyNightmare.InventorySystem
{
    /// <summary>
    /// Custom Property Drawer for InventoryShape
    /// </summary>
    [CustomPropertyDrawer(typeof(RectShape))]
    public class EquipmentShapePropertyDrawer : PropertyDrawer
    {
        const int GridSize = 16; // The size between the boold-fields that make up the shape matrix

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Find properties
            var pRow = property.FindPropertyRelative("_row");
            var pColumn = property.FindPropertyRelative("_column");
            var pShape = property.FindPropertyRelative("_shape");

            // Clamp column & row
            if (pRow.intValue <= 0) { pRow.intValue = 1; }
            if (pColumn.intValue <= 0) { pColumn.intValue = 1; }

            // Begin property
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Fix intent
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var halfRow = position.width / 2;
            var rowRect = new Rect(position.x, position.y, halfRow, GridSize);
            var columnRect = new Rect(position.x + halfRow, position.y, halfRow, GridSize);

            // Row & Column
            EditorGUIUtility.labelWidth = 40;
            EditorGUI.PropertyField(rowRect, pRow, new GUIContent("row"));
            EditorGUI.PropertyField(columnRect, pColumn, new GUIContent("column"));

            // Draw grid
            var row = pRow.intValue;
            var column = pColumn.intValue;
            pShape.arraySize = row * column;
            for (var x = 0; x < column; x++)
            {
                for (var y = 0; y < row; y++)
                {
                    var index = x + column * y;
                    var rect = new Rect(position.x + (x * GridSize), position.y + GridSize + (y * GridSize), GridSize, GridSize);
                    EditorGUI.PropertyField(rect, pShape.GetArrayElementAtIndex(index), GUIContent.none);
                }
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            // End property
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float row = EditorGUI.GetPropertyHeight(property, label);
            row += property.FindPropertyRelative("_row").intValue * GridSize;
            return row;
        }
    }
}