using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supercent.UI
{
    public class ImageTextManagerOnEditor
    {
        private static ImageTextManagerOnEditor _instance = null;
        public static ImageTextManagerOnEditor Instance
        {
            get
            {
                if(null == _instance)
                {
                    _instance = new ImageTextManagerOnEditor();
                }

                return _instance;
            }
        }

        private ImageTextManagerOnEditor()
        {
            var rootGo = new GameObject("ImageTextManagerOnEditor");

            _rootTF = rootGo.transform;

            rootGo.hideFlags = HideFlags.HideAndDontSave;
        }

        ~ImageTextManagerOnEditor()
        {
            Debug.Log("소멸자 발동");

            _instance = null;
        }

        private Transform _rootTF = null;

        private Dictionary<ImageTextTheme, ImageTextPool[]> _themePoolTable = null;
        private Dictionary<ImageTextTheme, Sprite[]> _themeSpriteTable = null;

        public void Init()
        {
            _themePoolTable = new Dictionary<ImageTextTheme, ImageTextPool[]>();
            _themeSpriteTable = new Dictionary<ImageTextTheme, Sprite[]>();

            for (int i = 0, cnt = ImageTextSetting.ImageTextThemeDatas.Length; i < cnt; i++)
            {
                var data = ImageTextSetting.ImageTextThemeDatas[i];

                var imageTextPools = new ImageTextPool[data.GetImagePathCounts()];

                if(true == _themePoolTable.ContainsKey(data.Theme))
                {
                    Debug.LogError($"aleady in _themePoolTable : {data.Theme}");

                    return;
                }

                _themePoolTable.Add(data.Theme, imageTextPools);


                var digitImageSprites = new Sprite[data.GetImagePathCounts()];

                var themeParent = new GameObject(data.Theme.ToString()).transform;
                themeParent.SetParent(_rootTF);

                for (int j = 0, cntJ = digitImageSprites.Length; j < cntJ; j++)
                {
                    var sprite = GetProperSpriteFromResources(data.GetProperPath(j));

                    digitImageSprites[j] = sprite;


                    var o = new GameObject(j.ToString());

                    o.transform.SetParent(themeParent);

                    var pool = o.AddComponent<ImageTextPool>();

                    pool.SetEnumType((CharacterType)j);

                    pool.SetPrefabTarget();
                    pool.SetCapacity(5);

                    pool.SetSprite(sprite);
                    pool.SetPivot(data.GetProperPivot(j));

                    pool.Init();

                    imageTextPools[j] = pool;
                }

                _themeSpriteTable.Add(data.Theme, digitImageSprites);
            }
            
        }

    #region Get Image Component - GameObject in Object Pool
        public ImageTextModuleOnCanvas GetImageTextFromObjectPool(ImageTextTheme type, int index, Transform parent)
        {
            if(((CharacterType)index).Equals(CharacterType.None))
            {
                return null;
            }

            if(true == _themePoolTable.TryGetValue(type, out var pools))
            {
                if(index >= pools.Length)
                {
                    Debug.LogError($"GetImageTextFromObjectPool error, out of length");

                    return null;
                }

                var tmp = pools[index].Get();

                tmp.SetParent(parent);

                return tmp;
            }
            else
            {
                Debug.LogError($"there is no type : {type}");
            }

            return  null;
        }

    #endregion

        public Sprite GetProperSpriteFromResources(string path)
        {
            var origin = Resources.Load<Sprite>(path);

            if(null == origin)
            {
                Debug.LogError($"There is no proper sprite in path : {path}");

                return null;
            }

            return origin;
        }
    }
}

