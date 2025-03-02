using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Supercent.UI
{
    public class ImageTextManager 
    {
        private static ImageTextManager _instance = null;
        public static ImageTextManager Instance
        {
            get
            {
                if(null == _instance)
                {
                    _instance = new ImageTextManager();
                }

                return _instance;
            }
        }

        private ImageTextManager()
        {
        if(true == ImageTextSetting.SetInnerObjectPool)
            {
                return;
            }

            var rootGo = new GameObject("ImageTextPools");

            _rootTF = rootGo.transform;

            GameObject.DontDestroyOnLoad(rootGo);

            Init();

            
        }

        private Transform _rootTF = null;

        private bool _isInit = false;

        private ImageTextTheme _currentTheme = ImageTextTheme.None; // 테마 비교 후 다르면 이전 테마 회수 후 지금 테마 출력하기

        private Dictionary<ImageTextTheme, ImageTextPool[]> _themePoolTable = null;
        private Dictionary<ImageTextTheme, Sprite[]> _themeSpriteTable = null;

        private void Init()
        {
            if(true == _isInit)
            {
                return;
            }

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

            _isInit = true;
        }

        public void Release()
        {
            if(null != _rootTF)
            {
                GameObject.Destroy(_rootTF.gameObject);

                _rootTF = null;
            }

            if(null != _themePoolTable)
            {
                for (int i = 0; i < _themePoolTable.Count; i++)
                {
                    var pool = _themePoolTable.ElementAt(i).Value;

                    for (int j = 0; j < pool.Length; j++)
                    {
                        pool[j].Release();
                    }

                    pool = null;
                }

                _themePoolTable.Clear();
                _themePoolTable = null;
            }
            
            if(null != _themeSpriteTable)
            {
                for (int i = 0; i < _themeSpriteTable.Count; i++)
                {
                    var sprites = _themeSpriteTable.ElementAt(i).Value;

                    for (int j = 0; j < sprites.Length; j++)
                    {
                        sprites[j] = null;
                    }

                    sprites = null;
                }

                _themeSpriteTable.Clear();
                _themeSpriteTable = null;
            }

            _instance = null;
        }

    #region Get Sprite

        public Sprite GetTargetSprite(ImageTextTheme type, int index)
        {
            if(_themeSpriteTable == null)
            {
                return null;
            }
            
            if(true == _themeSpriteTable.TryGetValue(type, out var sprites))
            {
                if(null == sprites)
                {
                    return null;
                }

                if(index >= sprites.Length)
                {
                    return null;
                }
                else
                {
                    return sprites[index];
                }
            }
            else
            {
                return null;
            }
        }

    #endregion

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

