using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Supercent.UI
{
    [CustomEditor(typeof(ImageTextSetting))]
    public class ImageTextSettingEditor : UnityEditor.Editor
    {
        SerializedProperty _imageTextThemeData = null;

        Dictionary<int, SerializedProperty> _imageMetaDataTable = null;


        void OnEnable()
        {
            _imageTextThemeData = serializedObject.FindProperty("_datas");

            _imageMetaDataTable = new Dictionary<int, SerializedProperty>();

            for (int i = 0; i < _imageTextThemeData.arraySize; i++)
            {
                var element = _imageTextThemeData.GetArrayElementAtIndex(i);

                var imageMetaDataArray = element.FindPropertyRelative("_characterImageMetaDatas");

                _imageMetaDataTable.Add(i, imageMetaDataArray);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            for (int i = 0; i < _imageMetaDataTable.Count; i++)
            {
                var kvp = _imageMetaDataTable.ElementAt(i);

                for (int j = 0; j < kvp.Value.arraySize; j++) // imageMetaDataArray
                {
                    var element = kvp.Value.GetArrayElementAtIndex(j); // ImageMetaData

                    var spriteProperty = element.FindPropertyRelative("_sprite");

                    var pivotProperty = element.FindPropertyRelative("_pivot");
                    
                    pivotProperty.vector2Value = new Vector2(0.5f, 0f);

                    if(null != spriteProperty.objectReferenceValue)
                    {
                        Sprite sprite = (Sprite)spriteProperty.objectReferenceValue;

                        var path = GetProperPath(AssetDatabase.GetAssetPath(sprite));
                        
                        var size = new Vector2(sprite.rect.width, sprite.rect.height);

                        var pathProperty = element.FindPropertyRelative("_characterImagePaths");
                        var sizeProperty = element.FindPropertyRelative("_size");

                        pathProperty.stringValue = path;
                        sizeProperty.vector2Value = size;
                    }
                    
                }

                serializedObject.ApplyModifiedProperties();

            }
        }

        private string GetProperPath(string original)
        {
            string[] keywords = new string[]{"Assets/Resources/", ".png", ".jpg"};

            var path = original;

            for (int i = 0; i < keywords.Length; i++)
            {
                path = path.Replace(keywords[i], "");
            }

            return path;
        }
    }
}


