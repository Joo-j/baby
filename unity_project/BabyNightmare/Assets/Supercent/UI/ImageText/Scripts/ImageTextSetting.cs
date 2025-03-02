using System;
using UnityEngine;

namespace Supercent.UI
{
    public class ImageTextSetting : ScriptableObject
    {
        private static ImageTextSetting _instance = null;

        private static ImageTextSetting Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Resources.Load<ImageTextSetting>("Supercent/ImageText/ImageTextSetting");

                }

    #if UNITY_EDITOR
                if(_instance == null)
                {
                    CreateMyAsset();

                    Debug.Log("Create ImageTextSetting, Please type paths");
                }

        #endif
                return _instance;
            }
        }

    #region Create Setting
    #if UNITY_EDITOR
        [UnityEditor.MenuItem("Supercent/UI/Create ImageTextSetting", false)]
        private static void  CreateMyAsset()
        {
            var path = "Assets/Resources/Supercent";

            if(false == System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            var filePath = "Resources/Supercent/ImageText/ImageTextSetting.asset";

            ImageTextSetting asset = null;

            if(false == System.IO.Directory.Exists($"{Application.dataPath}/Resources/Supercent/ImageText"))
            {
                System.IO.Directory.CreateDirectory($"{Application.dataPath}/Resources/Supercent/ImageText");
            }

            if(false == System.IO.File.Exists($"{Application.dataPath}/{filePath}"))
            {
                asset = ScriptableObject.CreateInstance<ImageTextSetting>();

                UnityEditor.AssetDatabase.CreateAsset(asset, $"Assets/{filePath}");
                UnityEditor.AssetDatabase.SaveAssets();
            }
            else
            {
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath($"Assets/{filePath}", typeof(ImageTextSetting)) as ImageTextSetting;
            }

            UnityEditor.EditorUtility.FocusProjectWindow();
            UnityEditor.Selection.activeObject = asset;

            _instance = asset;
        }

        #endif
    #endregion  

        public static ImageTextThemeData[] ImageTextThemeDatas => Instance._datas;

        public static bool SetInnerObjectPool => Instance._setInnerObjectPool;

        public static string BaseImageTextPrefabPath => Instance._baseImageTextPrefabPath;
        
        [SerializeField]
        private bool _setInnerObjectPool = false;
    
        [Space]
        [SerializeField]
        private string _baseImageTextPrefabPath = "";

        [Space]
        [SerializeField]
        private ImageTextThemeData[] _datas = null;


    
    }

    [Serializable]
    public class ImageTextThemeData
    {
        [SerializeField]
        private ImageTextTheme _theme;
        public ImageTextTheme Theme => _theme;

        [Space]
        [Header("Image Paths (Please type a path in 'Resources' folder)")]
        [SerializeField]
        private ImageMetaData[] _characterImageMetaDatas;
    
        public int GetImagePathCounts()
        {
            if(null == _characterImageMetaDatas)
            {
                return 0;
            }
            else
            {
                return _characterImageMetaDatas.Length;
            }
        }

        public Sprite GetProperSprite(int index)
        {
            if(index >= _characterImageMetaDatas.Length)
            {
                return null;
            }
            else
            {
                return _characterImageMetaDatas[index].Sprite;
            }
        }

        public string GetProperPath(int index)
        {
            if(index >= _characterImageMetaDatas.Length)
            {
                return null;
            }
            else
            {
                return _characterImageMetaDatas[index].CharacterImagePath;
            }
        }

        public Vector2 GetProperPivot(int index)
        {
            if(index >= _characterImageMetaDatas.Length)
            {
                return new Vector2(0.5f, 0f);
            }
            else
            {
                return _characterImageMetaDatas[index].Pivot;
            }
        }

        public Vector2 GetProperSize(int index)
        {
            if(index >= _characterImageMetaDatas.Length)
            {
                return new Vector2(100f, 100f);
            }
            else
            {
                return _characterImageMetaDatas[index].Size;
            }
        }
    }

    [Serializable]
    public class ImageMetaData
    {
        [HideInInspector]
        [SerializeField]
        private string _characterImagePaths = "";
        public string CharacterImagePath => _characterImagePaths;

        [Space]
        [SerializeField]
        private Sprite _sprite = null;
        public Sprite Sprite => _sprite;

        [Space]
        [SerializeField]
        private Vector2 _pivot = new Vector2(0.5f, 0f);
        public Vector2 Pivot => _pivot;

        [Space]
        [SerializeField]
        private Vector2 _size = new Vector2(100f, 100f);
        public Vector2 Size => _size;
    }
}


