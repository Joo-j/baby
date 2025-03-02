using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Supercent.Util;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Supercent.UI
{
    public class PrintImageText : BehaviourBase
    {
        [SerializeField]
        private ImageTextTheme _theme = ImageTextTheme.None;

        [SerializeField]
        private bool _needComma = false;

        [SerializeField]
        private bool _needSign = false;

        [Space]
        [SerializeField]
        private bool _usingImageTextSettingFontSize = false;

        [SerializeField]
        private Vector2 _fontSize = Vector2.zero; 

        [SerializeField]
        private float _spacing = 0f;

        [Space]
        [Space]
        [SerializeField]
        private RectOffset _paddingOffset;

        [SerializeField]
        private TextAnchor _align = TextAnchor.MiddleCenter;

        [Space]
        [SerializeField]
        [HideInInspector]
        private Transform _parentOfPools = null;

        private bool _isInit = false;

        private Transform _parent = null;

        private ImageTextThemeData _currentThemeData;

        private HorizontalLayoutGroup _horizontalLayoutGroup = null;

        private CanvasGroup _canvasGroup = null;

        private Stack<int> _indexStack = new Stack<int>();

        private List<ImageTextModuleOnCanvas> _list = new List<ImageTextModuleOnCanvas>();

        private ImageTextTheme _currentTheme = ImageTextTheme.None;

        private Dictionary<ImageTextTheme, ImageTextPool[]> _themePoolTable = null;

        private Dictionary<ImageTextTheme, Sprite[]> _themeSpriteTable = null;

        private Action<float> _printTextCallback = null;

        // Start is called before the first frame update
        void Start()
        {
            Init();

        }

        private void Init()
        {
            if(true == _isInit)
            {
                return;
            }

            var parentCanvas = this.GetComponentInParent<Canvas>();

            if(null == parentCanvas)
            {
                Debug.LogError("There is No Parent Canvas, please do canvas setting.");

                return;
            }

            var rt = new GameObject("Parent").AddComponent<RectTransform>();

            rt.SetParent(this.transform);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(1000f, 200f);

            rt.localScale = Vector3.one;

            _parent = rt;

            _horizontalLayoutGroup = rt.gameObject.AddComponent<HorizontalLayoutGroup>();

            _horizontalLayoutGroup.padding = _paddingOffset;
            _horizontalLayoutGroup.spacing = _spacing;

            _horizontalLayoutGroup.childAlignment = _align;

            _horizontalLayoutGroup.childControlWidth = false;
            _horizontalLayoutGroup.childControlHeight = false;
            _horizontalLayoutGroup.childForceExpandHeight = false;
            _horizontalLayoutGroup.childForceExpandWidth = false;

            var contentSizeFitter = rt.gameObject.AddComponent<ContentSizeFitter>();

            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            _canvasGroup = rt.gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if(true == ImageTextSetting.SetInnerObjectPool)
            {
                InitTables();

                _printTextCallback = _PrintTextWithInnerObjectPool;
            }
            else
            {
                _printTextCallback = _PrintText;
            }

            _currentTheme = _theme;
            _currentThemeData = ImageTextSetting.ImageTextThemeDatas[((int)_theme) -1];
        
            _isInit = true;
        }

        private void InitTables()
        {
            _themePoolTable = new Dictionary<ImageTextTheme, ImageTextPool[]>();
            _themeSpriteTable = new Dictionary<ImageTextTheme, Sprite[]>();

            if(_parentOfPools == null)
            {
                // 미리 binding 를 안했다는 것
                CreateInnerObjectPool();
            }
        
            for (int i = 0, cnt = _parentOfPools.childCount; i < cnt ; i++)
            {
                var tmp = _parentOfPools.GetChild(i).GetComponent<ThemeModule>();

                var pools = tmp.GetComponentsInChildren<ImageTextPool>();

                var digitImageSprites = new Sprite[pools.Length];

                for (int j = 0, cntJ = pools.Length; j < cntJ; j++)
                {
                    pools[j].Init();

                    digitImageSprites[j] = pools[j].GetSprite();
                }

                _themePoolTable.Add(tmp.Theme, pools);
                _themeSpriteTable.Add(tmp.Theme, digitImageSprites);
            }

            if(false == _themePoolTable.ContainsKey(_theme))
            {
                var data = ImageTextSetting.ImageTextThemeDatas[((int)_theme)-1];

                var imageTextPools = new ImageTextPool[data.GetImagePathCounts()];

                _themePoolTable.Add(data.Theme, imageTextPools);


                var digitImageSprites = new Sprite[data.GetImagePathCounts()];

                var themeParent = new GameObject(data.Theme.ToString()).transform;
                themeParent.SetParent(_parentOfPools);

                for (int j = 0, cntJ = digitImageSprites.Length; j < cntJ; j++)
                {
                    var sprite = ImageTextManager.Instance.GetProperSpriteFromResources(data.GetProperPath(j));

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

    #if UNITY_EDITOR
        private void InitOnEditor()
        {
            var parentCanvas = this.GetComponentInParent<Canvas>();

            if(null == parentCanvas)
            {
                Debug.LogError("There is No Parent Canvas, please do canvas setting.");

                return;
            }

            if(null != _parent)
            {
                DestroyImmediate(_parent.gameObject);

                _parent = null;
            }

            var rt = new GameObject("Parent").AddComponent<RectTransform>();

            rt.SetParent(this.transform);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(1000f, 200f);

            rt.localScale = Vector3.one;        

            rt.gameObject.hideFlags = HideFlags.HideAndDontSave;

            _parent = rt;

            _horizontalLayoutGroup = rt.gameObject.AddComponent<HorizontalLayoutGroup>();

            _horizontalLayoutGroup.padding = _paddingOffset;
            _horizontalLayoutGroup.spacing = _spacing;

            _horizontalLayoutGroup.childAlignment = _align;

            _horizontalLayoutGroup.childControlWidth = false;
            _horizontalLayoutGroup.childControlHeight = false;
            _horizontalLayoutGroup.childForceExpandHeight = false;
            _horizontalLayoutGroup.childForceExpandWidth = false;

            var contentSizeFitter = rt.gameObject.AddComponent<ContentSizeFitter>();

            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            _canvasGroup = rt.gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            _currentTheme = _theme;
            _currentThemeData = ImageTextSetting.ImageTextThemeDatas[((int)_theme) -1];

        }
    #endif

        private void OnDestroy() {
            
            Release();
        }

        public void Release()
        {
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

            if(null != _list)
            {
                _list.Clear();

                _list = null;
            }

            if(null != _indexStack)
            {
                _indexStack.Clear();
                
                _indexStack = null;
            }

            _horizontalLayoutGroup = null;

            _parent = null;

            _printTextCallback = null;

            _canvasGroup = null;
        }

        public void PrintText(int damage)
        {
            if(false == _isInit)
            {
                Debug.LogError("no init complete");

                return;
            }

            if(true == _theme.Equals(ImageTextTheme.None))
            {
                Debug.LogError("Please, Set 'Image Text Theme'");

                return;
            }

            ClearPoolObjs();

            InitIndexStack(damage, _needComma);

            CheckThemeChanged();

            if(null != _horizontalLayoutGroup)
            {
                _horizontalLayoutGroup.childAlignment = _align;

                _horizontalLayoutGroup.spacing = _spacing;

            }

            _printTextCallback?.Invoke(damage);
        }

        public void PrintText(float damage) 
        {
            if(false == _isInit)
            {
                Debug.LogError("no init complete");
                
                return;
            }

            if(true == _theme.Equals(ImageTextTheme.None))
            {
                Debug.LogError("Please, Set 'Image Text Theme'");

                return;
            }
            
            ClearPoolObjs();

            InitIndexStack(damage, _needComma);

            CheckThemeChanged();

            if(null != _horizontalLayoutGroup)
            {
                _horizontalLayoutGroup.childAlignment = _align;

                _horizontalLayoutGroup.spacing = _spacing;

            }

            _printTextCallback?.Invoke(damage);
        }

        private void _PrintText(float damage)
        {
            var prefix = ImageTextManager.Instance.GetImageTextFromObjectPool(_theme,
                _needSign ? (int)GetProperPrefixType(damage) : (int)CharacterType.None, 
                _parent);

            if(null != prefix)
            {
                prefix.SetImageSize(GetFontSize((int)GetProperPrefixType(damage)));
                
                _list.Add(prefix);
            }

            var stackCount = _indexStack.Count;

            if(stackCount > 0)
            {
                for (int i = 0; i < stackCount; i++)
                {
                    var index = _indexStack.Pop();

                    var poolObj = ImageTextManager.Instance.GetImageTextFromObjectPool(_theme, index, _parent);

                    poolObj.SetImageSize(GetFontSize(index));

                    if(null == poolObj)
                    {
                        continue;
                    }
                    else
                    {
                        _list.Add(poolObj);
                    }
                }
            }
        }

        private void _PrintTextWithInnerObjectPool(float damage)
        {
            var prefix = GetImageTextFromObjectPool(_theme,
                _needSign ? (int)GetProperPrefixType(damage) : (int)CharacterType.None, 
                _parent);

            if(null != prefix)
            {
                prefix.SetImageSize(GetFontSize((int)GetProperPrefixType(damage)));
                
                _list.Add(prefix);
            }

            var stackCount = _indexStack.Count;

            if(stackCount > 0)
            {
                for (int i = 0; i < stackCount; i++)
                {
                    var index = _indexStack.Pop();

                    var poolObj = GetImageTextFromObjectPool(_theme, index, _parent);

                    if(null == poolObj)
                    {
                        continue;
                    }
                    else
                    {
                        poolObj.SetImageSize(GetFontSize(index));

                        _list.Add(poolObj);
                    }
                }
            }
        }

        private void _PrintTextOnEditorForTest(float damage)
        {
            var prefix = ImageTextManagerOnEditor.Instance.GetImageTextFromObjectPool(_theme,
                _needSign ? (int)GetProperPrefixType(damage) : (int)CharacterType.None, 
                _parent);

            if(null != prefix)
            {
                prefix.SetImageSize(GetFontSize((int)GetProperPrefixType(damage)));
                
                _list.Add(prefix);
            }

            var stackCount = _indexStack.Count;

            if(stackCount > 0)
            {
                for (int i = 0; i < stackCount; i++)
                {
                    var index = _indexStack.Pop();

                    var poolObj = ImageTextManagerOnEditor.Instance.GetImageTextFromObjectPool(_theme, index, _parent);

                    poolObj.SetImageSize(GetFontSize(index));

                    if(null == poolObj)
                    {                     
                        continue;
                    }
                    else
                    {
                        _list.Add(poolObj);
                    }
                }
            }
        }

        public void HideText()
        {
            ClearPoolObjs();
        }

        private void ClearPoolObjs()
        {
            for (int i = 0, cnt = _list.Count; i < cnt; i++)
            {
                _list[i].ReturnToPool();

            }

            _list.Clear();

            _indexStack.Clear();
        }

        private void InitIndexStack(int damage, bool needComma)
        {
            if(damage < 0)
            {
                damage *= -1;
            }

            var length = (int)(Mathf.Log10(damage) +1);

            var tmp = damage;

            for(var i = 0; i < length; i++)
            {
                var share = tmp / 10;
                var rest = tmp % 10;

                if(true == needComma && i != 0 && i % 3 == 0)
                {
                    _indexStack.Push((int)CharacterType.Comma);
                }

                _indexStack.Push(rest);

                tmp = share;
            }
        }

        private void InitIndexStack(float damage, bool needComma)
        {
            if(damage < 0)
            {
                damage *= -1f;
            }

            var share = (int)damage;
            
            var rest0 = damage % 1;
            var r2 = Mathf.RoundToInt(rest0 * 100); // 소수점 2 째 자리까지만 표기

            InitIndexStack(r2, false);

            _indexStack.Push((int)CharacterType.Period);

            InitIndexStack(share, needComma);

        }

        private CharacterType GetProperPrefixType(float damage)
        {
            var tmpPrefixType = CharacterType.None;

            if(true == _needSign)
            {
                if(0 <= damage)
                {
                    tmpPrefixType = CharacterType.Plus;
                }
                else
                {
                    tmpPrefixType = CharacterType.Minus;
                }
            }

            return tmpPrefixType;
        }

        public Vector2 GetFontSize(int index)
        {
            if(false == _usingImageTextSettingFontSize)
            {
                return _fontSize;
            }
            else
            {
                return _currentThemeData.GetProperSize(index);
            }
        }

        private void CheckThemeChanged()
        {
            if(_theme.Equals(ImageTextTheme.None))
            {
                Debug.LogError("Current Theme is None");

                return;
            }

            if(false == _currentTheme.Equals(_theme))
            {
                _currentTheme = _theme;

                _currentThemeData = ImageTextSetting.ImageTextThemeDatas[((int)_theme)-1];
            }
        }

        
        public void ChangeAlpha(float alpha)
        {
            _canvasGroup.alpha = alpha;
        }

        /// <summary>
        /// Changing color of image texts, it needs high costs
        /// </summary>
        /// <param name="color"></param>
        public void ChangeColor(Color color)
        {
            if(null == _list || 0 == _list.Count)
            {
                return;
            }

            for (int i = 0, cnt = _list.Count; i < cnt; i++)
            {
                _list[i].SetColor(color);
            }
        }

        #region Get Image Component - GameObject in Object Pool
        private ImageTextModuleOnCanvas GetImageTextFromObjectPool(ImageTextTheme type, int index, Transform parent)
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

        private void CreateInnerObjectPool()
        {
            var index = (int)_theme;

            if(index == 0)
            {
                Debug.LogError("Theme is None");

                return;
            }

            _parentOfPools = this.transform.Find("ParentOfPools");

            if(null == _parentOfPools)
            {
                _parentOfPools = new GameObject("ParentOfPools").transform;

                _parentOfPools.SetParent(this.transform);
            }
            else
            {
                for (int i = 0; i < _parentOfPools.childCount; i++)
                {
                    var tmp = _parentOfPools.GetChild(i).GetComponent<ThemeModule>();

                    if(true == tmp.Theme.Equals((ImageTextTheme)index))
                    {
                        Debug.LogError($"Aleay created theme pool : {((ImageTextTheme)index)}");

                        return;
                    }
                }
            }

            var data = ImageTextSetting.ImageTextThemeDatas[index-1];

            var digitImageSprites = new Sprite[data.GetImagePathCounts()];

            var themeParent = new GameObject(data.Theme.ToString()).transform;
            themeParent.SetParent(_parentOfPools);

            themeParent.gameObject.AddComponent<ThemeModule>().SetTheme((ImageTextTheme)index);

            for (int j = 0, cntJ = digitImageSprites.Length; j < cntJ; j++)
            {
                var sprite = ImageTextManager.Instance.GetProperSpriteFromResources(data.GetProperPath(j));

                var o = new GameObject(j.ToString());

                o.transform.SetParent(themeParent);

                var pool = o.AddComponent<ImageTextPool>();

                pool.SetEnumType((CharacterType)j);

                pool.SetPrefabTarget();
                pool.SetCapacity(5);

                pool.SetSprite(sprite);
                pool.SetPivot(data.GetProperPivot(j));
            }
        }

    #endregion

    #if UNITY_EDITOR

        private void Update() {
            
            // if(Input.GetKeyDown(KeyCode.Q))
            // {
            //     var rndValue = Random.Range(-9999f, 9999f);
            
            //     Debug.Log($" Print number : {rndValue}");

            //     PrintText(rndValue);
            // }

            // if(Input.GetKeyDown(KeyCode.W))
            // {
            //     var rndValue = Random.Range(-9999, 9999);

            //     Debug.Log($" Print number : {rndValue}");

            //     PrintText(rndValue);
            // }

            // if(Input.GetKeyDown(KeyCode.A))
            // {
            //     var rndValue = Random.Range(0f, 1f);

            //     ChangeAlpha(rndValue);
            // }

            // if(Input.GetKeyDown(KeyCode.S))
            // {
            //     float[] rgb = new float[3];

            //     for (int i = 0; i < rgb.Length; i++)
            //     {
            //         rgb[i] = Random.Range(0f, 1f);
            //     }

            //     Color color = new Color (rgb[0], rgb[1], rgb[2], 1);

            //     ChangeColor(color);
            // }
        }

        protected override void OnBindSerializedField()
        {
            if(true == ImageTextSetting.SetInnerObjectPool)
            {
                CreateInnerObjectPool();

            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Show in Editor"))
            {
                ImageTextManagerOnEditor.Instance.Init();
                            
                InitOnEditor();

                PrintImageTextForTest();
            }
        }

        private void PrintImageTextForTest()
        {
            var rndValue = Random.Range(-9999f, 9999f);
            
            Debug.Log($" Print number : {rndValue}");

            if(true == _theme.Equals(ImageTextTheme.None))
            {
                Debug.LogError("Please, Set 'Image Text Theme'");

                return;
            }
            
            _list.Clear();

            _indexStack.Clear();

            InitIndexStack(rndValue, _needComma);

            CheckThemeChanged();

            if(null != _horizontalLayoutGroup)
            {
                _horizontalLayoutGroup.childAlignment = _align;

                _horizontalLayoutGroup.spacing = _spacing;

            }

            _PrintTextOnEditorForTest(rndValue);

            if(null != _parent)
            {
                _parent.gameObject.SetActive(true);

            }
        }

    #endif
    }
}

