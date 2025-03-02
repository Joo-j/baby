using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Supercent.Util
{
    public static class SimpleLoading
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private static SimpleLoadingSettings _settings = null;
        private static SimpleLoadingView     _view     = null;

        private const string FOLDER_SETTINGS = "Supercent";
        private const string FILE_SETTINGS   = "SimpleLoadingSettings";

        private static string SettingsPath => $"{FOLDER_SETTINGS}/{FILE_SETTINGS}";

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public static bool IsVisibleFullOfScreen  => _view?.IsVisibleFullOfScreen ?? false;
        public static bool IsVisiblePartsOfScreen => _view?.IsVisiblePartsOfScreen ?? false;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        /// <summary>
        /// 화면 전체를 가리는 로딩 보이기
        /// </summary>
        public static void ShowFullOfScreen(Action doneCallback)
        {
            Init();

            if (null == _view)
            {
                doneCallback?.Invoke();
                return;
            }

            _view.ShowFullOfScreen(doneCallback);
        }

        /// <summary>
        /// 화면 전체를 가리는 로딩 가리기
        /// </summary>
        public static void HideFullOfScreen(Action doneCallback)
        {
            Init();

            if (null == _view)
            {
                doneCallback?.Invoke();
                return;
            }

            _view.HideFullOfScreen(doneCallback);
        }

        /// <summary>
        /// 화면 중앙에 표시되는 이미지 로딩 보이기
        /// </summary>
        public static void ShowPartsOfScreen()
        {
            Init();

            if (null == _view)
                return;

            _view?.ShowPartsOfScreen();
        }

        /// <summary>
        /// 화면 중앙에 표시되는 이미지 로딩 가리기
        /// </summary>
        public static void HidePartsOfScreen()
        {
            Init();

            _view?.HidePartsOfScreen();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        private static void Init()
        {
            InitSettings();
            InitView();
        }

        private static void InitSettings()
        {
            if (null != _settings)
                return;

            _settings = Resources.Load<SimpleLoadingSettings>(SettingsPath);

            if (null == _settings)
            {
#if UNITY_EDITOR
                // 에디터일 경우 파일 생성
                EDITOR_CreateSettingsFile();
#else
                // 빌드일 경우 더미 추가
                _settings = new SimpleLoadingSettings();
#endif
            }

            if (null == _settings)
            {
                Debug.LogError("[SimpleLoading.InitSettings] 셋팅 파일을 로드하지 못했습니다.");
                return;
            }
        }

        private static void InitView()
        {
            if (null != _view)
                return;

            var prefab = Resources.Load<SimpleLoadingView>("SimpleLoadingView");
            if (null == prefab)
            {
                Debug.LogError("[SimpleLoading.InitView] 뷰 프리팹을 로드하지 못했습니다.");
                return;                
            }

            _view = ObjectUtil.Instantiate<SimpleLoadingView>(prefab, null);
            if (null == _view)
            {
                Debug.LogError("[SimpleLoading.InitView] 뷰 생성에 실패했습니다.");
                return;
            }

            UnityEngine.Object.DontDestroyOnLoad(_view);

            _view.Init(_settings);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Supercent/Util/심플로딩 설정파일 생성하기")]
        private static void EDITOR_CreateSettingsFile()
        {
            // 폴더 확인
            var folder = $"Assets/Resources/{FOLDER_SETTINGS}";
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            // 파일 확인 및 생성
            var resourcePath = $"Resources/{SettingsPath}.asset";
            var assetPath    = $"Assets/{resourcePath}";

            if (!System.IO.File.Exists($"{Application.dataPath}/{resourcePath}"))
            {
                _settings = ScriptableObject.CreateInstance<SimpleLoadingSettings>();

                UnityEditor.AssetDatabase.CreateAsset(_settings, assetPath);
                UnityEditor.AssetDatabase.SaveAssets();
            }
            else
                _settings = UnityEditor.AssetDatabase.LoadAssetAtPath<SimpleLoadingSettings>(assetPath);

            UnityEditor.EditorUtility.FocusProjectWindow();
            UnityEditor.Selection.activeObject = _settings;
        }
#endif
    }
}