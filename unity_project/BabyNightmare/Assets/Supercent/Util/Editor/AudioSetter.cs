using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Supercent.Util.Editor
{
    public sealed class AudioSetter : EditorWindowBase
    {
        const string PathRoot = "Assets";
        const string PathBase = "Supercent";

        static readonly string KEY_PATH = $"{nameof(AudioSetter)}.{nameof(lastPath)}";

        enum SearchType
        {
            Free = 0,
            Prefix,
            Suffix
        }

        public enum PlatformType
        {
            Default = 0,
            Standalone,
            iOS,
            Android,
        }

        public enum BoolType
        {
            Keep = 0,
            Enable,
            Disable,
        }
        public enum DefaultBoolType
        {
            Default = 0,
            Enable,
            Disable,
        }

        public enum SampleRateType : uint
        {
            Default = 0,
            _8000 = 8000,
            _11025 = 11025,
            _22050 = 22050,
            _44100 = 44100,
            _48000 = 48000,
            _96000 = 96000,
            _192000 = 192000,
        }

        [Serializable]
        public class Common
        {
            public BoolType forceToMono = BoolType.Keep;
            public BoolType normalize = BoolType.Keep;
            public BoolType loadInBackground = BoolType.Keep;
#if !UNITY_2022_1_OR_NEWER
            public BoolType preloadAudioData = BoolType.Keep;
#endif// !UNITY_2022_1_OR_NEWER
            public BoolType ambisonic = BoolType.Keep;


            public static Common MakeDefault() => new Common();
        }

        [Serializable]
        public class Platform
        {
            public PlatformType type = default;
            [NonSerialized] public string name = string.Empty;

            public bool keepPlatform = true;
            public BoolType overridden = BoolType.Keep;

            public bool keepLoadType = true;
            public AudioClipLoadType loadType = (AudioClipLoadType)(-1);

#if UNITY_2022_1_OR_NEWER
            public bool keepPreloadAudioData = true;
            public DefaultBoolType preloadAudioData = DefaultBoolType.Default;
#endif// UNITY_2022_1_OR_NEWER

            public bool keepCompressionFormat = true;
            public AudioCompressionFormat compressionFormat = AudioCompressionFormat.ADPCM;

            public bool keepQuality = true;
            public float quality = 100f;

            public bool keepSampleRateSetting = true;
            public AudioSampleRateSetting sampleRateSetting = (AudioSampleRateSetting)(-1);

            public bool keepSampleRateOverride = true;
            public SampleRateType sampleRateOverride = SampleRateType.Default;


            public static Platform MakeDefault(PlatformType type)
            {
                return new Platform()
                {
                    type = type,
                    name = GetPlatformName(type),
                };
            }

            public static string GetPlatformName(PlatformType type)
            {
                return type == PlatformType.Standalone ? BuildTargetGroup.Standalone.ToString()
                     : type == PlatformType.iOS ? BuildTargetGroup.iOS.ToString()
                     : type == PlatformType.Android ? BuildTargetGroup.Android.ToString()
                     : "Default";
            }
        }
        
        [Serializable]
        public class Preset
        {
            public Common common;
            public Platform[] platforms;
        }

        const float MenuLabelWidth = 149f;
        readonly string[] paths = { string.Empty, };
        readonly SortedDictionary<PlatformType, Platform> platforms = new SortedDictionary<PlatformType, Platform>();

        bool includeSubfolders = true;
        Common common = null;


        string lastPath = PathBase;
        string lastFind = string.Empty;
        SearchType typeSearch = SearchType.Free;
        bool foldCommon = true;
        bool foldFormat = true;
        Vector2 posScroll = Vector2.zero;

        GUIContent conOpen = new GUIContent("O", "Open");
        GUIContent conDefault = new GUIContent("D", "Default");
        GUIContent conClear = new GUIContent("C", "Clear");

        GUIStyle styPathText = null;
        GUIStyle styPathBtn = null;
        GUILayoutOption optLabelWidth = GUILayout.Width(28);
        GUILayoutOption optDropWidth = GUILayout.Width(55);



        [MenuItem("Supercent/Util/Audio Setter &A")]
        public static void OpenWindow()
        {
            var window = GetWindow<AudioSetter>(false, "Audio Setter");
            if (window != null) window.Show();
        }

        void OnEnable()
        {
            styPathText = null;
            styPathBtn = null;
            ResetPreset();
            SetPath(EditorPrefs.GetString(KEY_PATH, lastPath));

            guiDraw = GUIDraw;
        }



        #region Draw menu
        void GUIDraw()
        {
            SetStyle();

            EditorGUILayout.BeginVertical();
            {
                ViewTopMene();
                var curPos = EditorGUILayout.BeginScrollView(posScroll, GUIStyle.none, GUI.skin.verticalScrollbar);
                {
                    var curEvent = Event.current.type;
                    if (curEvent != EventType.Repaint)
                        posScroll = curPos;

                    ViewSettingMenu();

                    if (curEvent == EventType.Repaint)
                        posScroll = curPos;
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        void SetStyle()
        {
            if (styPathText == null)
            {
                styPathText = new GUIStyle(GUI.skin.textField)
                {
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset { top = 3, right = 3, left = 3, },
                    fixedHeight = 18,
                };
            }
            if (styPathBtn == null)
            {
                styPathBtn = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset { left = 2, bottom = 1 },
                    fontSize = 12,
                    fixedWidth = 18,
                    fixedHeight = 18,
                };
                var margin = styPathBtn.margin;
                margin.top += 1;
                styPathBtn.margin = margin;
            }
        }

        void ViewTopMene()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Path", optLabelWidth);
                {
                    var editPath = EditorGUILayout.TextField(lastPath, styPathText);
                    if (string.CompareOrdinal(editPath, lastPath) != 0)
                        SetPath(editPath);
                }

                if (Button(conOpen, styPathBtn))
                {
                    if (OpenAssetsFolderPanel("Select a folder", lastPath, string.Empty, out var assetsPath))
                        SetPath(assetsPath);
                }

                if (Button(conDefault, styPathBtn))
                    SetPath(PathBase);

                if (Button(conClear, styPathBtn))
                    SetPath(string.Empty);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                typeSearch = EnumPopup(typeSearch, optDropWidth);
                var editFind = EditorGUILayout.TextField(lastFind, styPathText);
                if (string.CompareOrdinal(editFind, lastFind) != 0)
                    SetFind(editFind);

                if (Button(conClear, styPathBtn))
                    SetFind(string.Empty);
            }
            EditorGUILayout.EndHorizontal();

            includeSubfolders = EditorGUILayout.ToggleLeft("Include subfolders", includeSubfolders);


            GetSplitWdith(out var minWidth, out var maxWidth, 2);
            EditorGUILayout.BeginHorizontal();
            {
                if (Button("Apply", minWidth, maxWidth))
                    SetConfig();

                if (Button("Reset", minWidth, maxWidth))
                    ResetPreset();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                if (Button("Save", minWidth, maxWidth))
                {
                    var pathSave = EditorUtility.SaveFilePanel("Save preset", PathProjectSetting, "AudioSetting", "preset");
                    if (!string.IsNullOrEmpty(pathSave))
                        SavePreset(pathSave);
                }

                if (Button("Load", minWidth, maxWidth))
                {
                    var pathLoad = EditorUtility.OpenFilePanel("Load preset", PathProjectSetting, "preset");
                    if (!string.IsNullOrEmpty(pathLoad))
                        LoadPreset(pathLoad);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void ViewSettingMenu()
        {
            if (foldCommon = EditorGUILayout.Foldout(foldCommon, "Common"))
            {
                common.forceToMono = EnumPopup("Force To Mono", common.forceToMono);
                common.normalize = EnumPopup("Normalize", common.normalize);
                common.loadInBackground = EnumPopup("Load In Background", common.loadInBackground);
#if !UNITY_2022_1_OR_NEWER
                common.preloadAudioData = EnumPopup("Preload Audio Data", common.preloadAudioData);
#endif // !UNITY_2022_1_OR_NEWER
                common.ambisonic = EnumPopup("Ambisonic", common.ambisonic);
            }


            EditorGUILayout.Space();
            if (foldFormat = EditorGUILayout.Foldout(foldFormat, "Format"))
            {
                _DrawFormat(PlatformType.Default);
                DivisionLine(Color.gray, 5, 1f);

                _DrawFormat(PlatformType.Standalone);
                DivisionLine(Color.gray, 5, 1f);

                _DrawFormat(PlatformType.Android);
                DivisionLine(Color.gray, 5, 1f);

                _DrawFormat(PlatformType.iOS);
            }


            void _DrawFormat(PlatformType _type)
            {
                if (!platforms.TryGetValue(_type, out var _platform))
                    return;

                _platform.keepPlatform = _platform.keepPlatform
                                       ? EditorGUILayout.ToggleLeft($"Keep {_type}", _platform.keepPlatform)
                                       : EditorGUILayout.ToggleLeft($"Set {_type}", _platform.keepPlatform);
                if (_platform.keepPlatform)
                    return;


                _platform.overridden = _type == PlatformType.Default
                                     ? BoolType.Keep
                                     : EnumPopup("Override", _platform.overridden);

                PopupMenuAddDefault(ref _platform.keepLoadType,
                                    ref _platform.loadType,
                                    _platform.keepLoadType ? "Keep Load Type" : "Load Type",
                                    _type == PlatformType.Default,
                                    (AudioClipLoadType)(-1),
                                    default);

#if UNITY_2022_1_OR_NEWER
                PopupMenu(ref _platform.keepPreloadAudioData,
                          ref _platform.preloadAudioData,
                          _platform.keepPreloadAudioData ? "Keep Preload Audio Data" : "Preload Audio Data",
                          _type == PlatformType.Default,
                          DefaultBoolType.Default,
                          DefaultBoolType.Disable);
#endif// UNITY_2022_1_OR_NEWER

                ToggleEnumPopup(ref _platform.keepCompressionFormat,
                                ref _platform.compressionFormat,
                                _platform.keepCompressionFormat ? "Keep Compression Format" : "Compression Format",
                                MenuLabelWidth);

                ToggleFloatField(ref _platform.keepQuality,
                                 ref _platform.quality,
                                 _platform.keepQuality ? "Keep Quality": "Quality",
                                 MenuLabelWidth,
                                 1f, 100f);

                PopupMenuAddDefault(ref _platform.keepSampleRateSetting,
                                    ref _platform.sampleRateSetting,
                                    _platform.keepSampleRateSetting ? "Keep Sample Rate Setting" : "Sample Rate Setting",
                                    _type == PlatformType.Default,
                                    (AudioSampleRateSetting)(-1),
                                    default);

                PopupMenu(ref _platform.keepSampleRateOverride,
                          ref _platform.sampleRateOverride,
                          _platform.keepSampleRateOverride ? "Keep Sample Rate" : "Sample Rate",
                          _type == PlatformType.Default,
                          SampleRateType.Default,
                          SampleRateType._44100);
            }
        }

        void PopupMenu<T>(ref bool enabled, ref T selected, string label, bool useDefault, T none, T fallback) where T : Enum
        {
            ToggleEnumPopup(ref enabled, ref selected, label, MenuLabelWidth);
            if (useDefault && EqualityComparer<T>.Default.Equals(selected, none))
            {
                enabled = true;
                selected = fallback;
            }
        }
        void PopupMenuAddDefault<T>(ref bool enabled, ref T selected, string label, bool useDefault, T none, T fallback) where T : Enum
        {
            ToggleEnumPopupWithNone(ref enabled, ref selected, label, MenuLabelWidth, "Default", none);
            if (useDefault && EqualityComparer<T>.Default.Equals(selected, none))
            {
                enabled = true;
                selected = fallback;
            }
        }
        #endregion// Draw menu


        void SetPath(string path)
        {
            EditorPrefs.SetString(KEY_PATH, path);
            lastPath = path;
            paths[0] = string.IsNullOrEmpty(lastPath)
                        ? PathRoot
                        : Path.Combine(PathRoot, lastPath);
        }
        void SetFind(string find)
        {
            lastFind = find;
        }

        void ResetPreset()
        {
            common = Common.MakeDefault();
            foreach (PlatformType type in Enum.GetValues(typeof(PlatformType)))
                platforms[type] = Platform.MakeDefault(type);
        }

        void SetConfig()
        {
            var fileGUIDs = AssetDatabase.FindAssets("t:AudioClip", paths);
            var importers = new List<AudioImporter>(fileGUIDs.Length);
            var pathEnd = paths[0].Length + 1;

            for (int index = 0; index < fileGUIDs.Length; ++index)
            {
                var path = AssetDatabase.GUIDToAssetPath(fileGUIDs[index]);
                if (_InvalidPath(path)) continue;
                if (AudioImporter.GetAtPath(path) is AudioImporter importer)
                    importers.Add(importer);
            }

            var keyward = string.IsNullOrEmpty(lastFind)
                        ? string.Empty
                        : $"{typeSearch} : {lastFind}{NL}";
            if (importers.Count < 1)
            {
                EditorUtility.DisplayDialog
                (
                    "Info",
                    $"Not found audio clips{NL}{NL}" +
                    $"Count : 0{NL}" +
                    $"Path : {lastPath}{NL}" +
                    keyward +
                    $"Include subfolders : {includeSubfolders}",
                    "Ok"
                );
                return;
            }

            var result = EditorUtility.DisplayDialog
            (
                $"Info",
                $"Are you sure you want to the audio clip settings?{NL}{NL}" +
                $"Count : {importers.Count}{NL}" +
                $"Path : {lastPath}{NL}" +
                keyward +
                $"Include subfolders : {includeSubfolders}",
                "Yes",
                "No"
            );
            if (result)
            {
                AssetDatabase.StartAssetEditing();
                _IterateJob(0);
                AssetDatabase.StopAssetEditing();
                EditorUtility.DisplayDialog("Info", "Done", "Ok");
            }


            bool _InvalidPath(string _path)
            {
                if (string.IsNullOrEmpty(_path)) return true;
                if (!includeSubfolders)
                {
                    if (-1 < _path.IndexOf('/', pathEnd)
                     || -1 < _path.IndexOf('\\', pathEnd))
                        return true;
                }

                if (string.IsNullOrEmpty(lastFind))
                    return false;
                else
                {
                    var name = Path.GetFileNameWithoutExtension(_path);
                    if (string.IsNullOrWhiteSpace(name))
                        return true;

                    switch (typeSearch)
                    {
                    case SearchType.Prefix: return !IsPrefix(name, lastFind);
                    case SearchType.Suffix: return !IsSuffix(name, lastFind);
                    default: return !Contains(name, lastFind);
                    }
                }
            }

            void _IterateJob(int _index)
            {
                AudioImporter _importer = null;
                try
                {
                    for (; _index < importers.Count; ++_index)
                    {
                        _importer = importers[_index];
                        SettingJob(_importer);
                        _importer.SaveAndReimport();
                    }
                    return;
                }
                catch (Exception error)
                {
                    var _path = _importer == null ? string.Empty : _importer.assetPath;
                    Debug.LogError($"{nameof(SetConfig)} : Setting error ({_path}){NL}{error}");
                }
                _importer = null;

                _IterateJob(++_index);
            }
        }

        bool GetBoolValue(BoolType value, out bool result)
        {
            return value == BoolType.Enable ? result = true
                 : value == BoolType.Disable ? !(result = false)
                 : result = false;
        }

        void SettingJob(AudioImporter importer)
        {
            if (importer == null) return;

            bool bValue = false;
            if (GetBoolValue(common.forceToMono, out bValue))
                importer.forceToMono = bValue;

            if (GetBoolValue(common.normalize, out bValue))
            {
                var serial = new SerializedObject(importer);
                serial.FindProperty("m_Normalize").boolValue = bValue;
                serial.ApplyModifiedProperties();
            }

            if (GetBoolValue(common.loadInBackground, out bValue))
                importer.loadInBackground = bValue;

#if !UNITY_2022_1_OR_NEWER
            if (GetBoolValue(common.preloadAudioData, out bValue))
                importer.preloadAudioData = bValue;
#endif// !UNITY_2022_1_OR_NEWER

            if (GetBoolValue(common.ambisonic, out bValue))
                importer.ambisonic = bValue;

            foreach (var platform in platforms.Values)
                SetPlatformConfig(importer, platform);
        }

        void SetPlatformConfig(AudioImporter importer, Platform platform)
        {
            if (importer == null) return;
            if (platform == null) return;
            if (platform.keepPlatform) return;

            var isDefault = platform.type == PlatformType.Default;
            var settings_default = importer.defaultSampleSettings;
            var settings = isDefault
                         ? settings_default
                         : importer.GetOverrideSampleSettings(platform.name);
            {
                if (!platform.keepLoadType)
                {
                    settings.loadType = platform.loadType == (AudioClipLoadType)(-1)
                                      ? settings_default.loadType
                                      : platform.loadType;
                }

#if UNITY_2022_1_OR_NEWER
                if (!platform.keepPreloadAudioData)
                {
                    settings.preloadAudioData = platform.preloadAudioData == DefaultBoolType.Default ? settings_default.preloadAudioData
                                              : platform.preloadAudioData == DefaultBoolType.Enable ? true
                                              : false;
                }
#endif// UNITY_2022_1_OR_NEWER

                if (!platform.keepCompressionFormat)
                    settings.compressionFormat = platform.compressionFormat;

                if (!platform.keepQuality)
                    settings.quality = platform.quality;

                if (!platform.keepSampleRateSetting)
                {
                    settings.sampleRateSetting = platform.sampleRateSetting == (AudioSampleRateSetting)(-1)
                                               ? settings_default.sampleRateSetting
                                               : platform.sampleRateSetting;
                }

                if (!platform.keepSampleRateOverride)
                {
                    settings.sampleRateOverride = platform.sampleRateOverride == SampleRateType.Default
                                                ? settings_default.sampleRateOverride
                                                : (uint)platform.sampleRateOverride;
                }
            }
            if (isDefault)  importer.defaultSampleSettings = settings;
            else            importer.SetOverrideSampleSettings(platform.name, settings);
        }


        #region Info IO
        void SavePreset(string path)
        {
            var values = Enum.GetValues(typeof(PlatformType));
            var preset = new Preset()
            {
                common = common,
                platforms = new Platform[values.Length],
            };

            for (int index = 0; index < values.Length; ++index)
            {
                var type = (PlatformType)values.GetValue(index);
                platforms.TryGetValue(type, out var platform);
                preset.platforms[index] = platform;
            }

            SaveJson(path, preset);
        }

        void LoadPreset(string path)
        {
            if (!LoadJson(path, out Preset preset) || preset == null)
            {
                EditorUtility.DisplayDialog("Error", "Invalid preset", "Ok");
                return;
            }

            common = preset.common ?? Common.MakeDefault();

            platforms.Clear();
            foreach (PlatformType type in Enum.GetValues(typeof(PlatformType)))
                platforms[type] = Platform.MakeDefault(type);

            if (preset.platforms != null)
            {
                foreach (var platform in preset.platforms)
                {
                    platform.name = Platform.GetPlatformName(platform.type);
                    platforms[platform.type] = platform;
                }
            }
        }
        #endregion// Info IO
    }
}
