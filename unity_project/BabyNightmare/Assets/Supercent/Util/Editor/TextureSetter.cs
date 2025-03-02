using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Supercent.Util.Editor
{
    public sealed class TextureSetter : EditorWindowBase
    {
        const string PathRoot = "Assets";
        const string PathBase = "Supercent";

        static readonly string KEY_PATH = $"{nameof(TextureSetter)}.{nameof(lastPath)}";

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

        public enum MaxSizeType
        {
            Default = 0,
            _32 = 5,
            _64 = 6,
            _128 = 7,
            _256 = 8,
            _512 = 9,
            _1024 = 10,
            _2048 = 11,
            _4096 = 12,
            _8192 = 13,
            _16384 = 14,
        }

        [Serializable]
        public class Common
        {
            public bool keepSpritePixelsPerUnit = true;
            public float spritePixelsPerUnit = 100f;

            public bool keepSpriteMeshType = true;
            public SpriteMeshType spriteMeshType = SpriteMeshType.FullRect;

            public bool keepSpriteExtrude = true;
            public uint spriteExtrude = 1;

            public bool keepSpritePivot = true;
            public Vector2 spritePivot = new Vector2(0.5f, 0.5f);

            public BoolType spriteGenerateFallbackPhysicsShape = BoolType.Keep;

            public bool keepAlphaSource = true;
            public TextureImporterAlphaSource alphaSource = TextureImporterAlphaSource.FromInput;
            public BoolType alphaIsTransparency = BoolType.Keep;

            public BoolType readable = BoolType.Keep;
            public BoolType ignorePngGamma = BoolType.Keep;
            public BoolType streamingMipmaps = BoolType.Keep;
            public BoolType mipmapEnabled = BoolType.Keep;

            public bool keepWrapMode = true;
            public TextureWrapMode wrapMode = TextureWrapMode.Repeat;

            public bool keepFilterMode = true;
            public FilterMode filterMode = FilterMode.Bilinear;


            public static Common MakeDefault() => new Common()
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
            };
        }

        [Serializable]
        public class Platform
        {
            public PlatformType type = default;
            [NonSerialized] public string name = string.Empty;

            public bool keepPlatform = true;
            public BoolType overridden = BoolType.Keep;

            public bool keepMaxTextureSize = true;
            public MaxSizeType maxTextureSize = MaxSizeType.Default;

            public bool keepResizeAlgorithm = true;
            public TextureResizeAlgorithm resizeAlgorithm = (TextureResizeAlgorithm)(-1);

            public bool keepFormat = true;
            public TextureImporterFormat format = default;

            public BoolType allowsAlphaSplitting = BoolType.Keep;
            public bool keepAndroidETC2FallbackOverride = true;
            public AndroidETC2FallbackOverride androidETC2FallbackOverride = default;


            public static Platform MakeDefault(PlatformType type)
            {
                var platform = new Platform()
                {
                    type = type,
                    name = GetPlatformName(type),
                };

                switch (type)
                {
                case PlatformType.Standalone:
                    platform.format = TextureImporterFormat.DXT5;
                    break;

                case PlatformType.iOS:
                    platform.format = TextureImporterFormat.ASTC_5x5;
                    break;

                case PlatformType.Android:
                    platform.format = TextureImporterFormat.ETC2_RGBA8;
                    platform.keepAndroidETC2FallbackOverride = true;
                    platform.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
                    break;

                default:
                    platform.format = TextureImporterFormat.RGBA32;
                    break;
                }

                return platform;
            }

            public static string GetPlatformName(PlatformType type)
            {
                return type == PlatformType.Standalone ? BuildTargetGroup.Standalone.ToString()
                     : type == PlatformType.iOS ? BuildTargetGroup.iOS.ToString()
                     : type == PlatformType.Android ? BuildTargetGroup.Android.ToString()
                     : "DefaultTexturePlatform";
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
        bool isTypeAll = true;
        TextureImporterType textureType = TextureImporterType.Default;
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



        [MenuItem("Supercent/Util/Texture Setter &T")]
        public static void OpenWindow()
        {
            var window = GetWindow<TextureSetter>(false, "Texture Setter");
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
            ToggleEnumPopup(ref isTypeAll, ref textureType, isTypeAll ? "All types" : "Type", 50f);


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
                    var pathSave = EditorUtility.SaveFilePanel("Save preset", PathProjectSetting, "TextureSetting", "preset");
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
                ToggleFloatField(ref common.keepSpritePixelsPerUnit,
                                 ref common.spritePixelsPerUnit,
                                 common.keepSpritePixelsPerUnit ? "Keep Pixels Per Unit" : "Pixels Per Unit",
                                 MenuLabelWidth,
                                 0.001f,
                                 float.MaxValue);

                ToggleEnumPopup(ref common.keepSpriteMeshType,
                                ref common.spriteMeshType,
                                common.keepSpriteMeshType ? "Keep Mesh Type" : "Mesh Type",
                                MenuLabelWidth);

                int spriteExtrude = (int)common.spriteExtrude;
                ToggleIntField(ref common.keepSpriteExtrude,
                               ref spriteExtrude,
                               common.keepSpriteExtrude ? "Keep Extrude Edges" : "Extrude Edges",
                               MenuLabelWidth,
                               0,
                               32);
                common.spriteExtrude = (uint)spriteExtrude;

                ToggleVector2Field(ref common.keepSpritePivot,
                                   ref common.spritePivot,
                                   common.keepSpritePivot ? "Keep Pivot" : "Pivot (x,y)",
                                   MenuLabelWidth);

                common.spriteGenerateFallbackPhysicsShape = EnumPopup("Generate Physics Shape", common.spriteGenerateFallbackPhysicsShape);

                EditorGUILayout.Space();
                ToggleEnumPopup(ref common.keepAlphaSource,
                                ref common.alphaSource,
                                common.keepAlphaSource ? "Keep Alpha Source" : "Alpha Source",
                                MenuLabelWidth);
                common.alphaIsTransparency = EnumPopup("Alpha Is Transparency", common.alphaIsTransparency);

                common.ignorePngGamma = EnumPopup("Ignore Png file Gamma", common.ignorePngGamma);
                common.readable = EnumPopup("Read/Write", common.readable);
                common.streamingMipmaps = EnumPopup("Streaming Mipmaps", common.streamingMipmaps);
                common.mipmapEnabled = EnumPopup("Generate Mip Maps", common.mipmapEnabled);

                ToggleEnumPopup(ref common.keepWrapMode,
                                ref common.wrapMode,
                                common.keepWrapMode ? "Keep Wrap Mode" : "Wrap Mode",
                                MenuLabelWidth);

                ToggleEnumPopup(ref common.keepFilterMode,
                                ref common.filterMode,
                                common.keepFilterMode ? "Keep Filter Mode" : "Filter Mode",
                                MenuLabelWidth);
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

                PopupMenu(ref _platform.keepMaxTextureSize,
                          ref _platform.maxTextureSize,
                          _platform.keepMaxTextureSize ? "Keep Max Texture Size" : "Max Texture Size",
                          _type == PlatformType.Default,
                          MaxSizeType.Default,
                          MaxSizeType._2048);

                PopupMenuAddDefault(ref _platform.keepResizeAlgorithm,
                                    ref _platform.resizeAlgorithm,
                                    _platform.keepResizeAlgorithm ? "Keep Resize Algorithm" : "Resize Algorithm",
                                    _type == PlatformType.Default,
                                    (TextureResizeAlgorithm)(-1),
                                    default);

                ToggleEnumPopup(ref _platform.keepFormat,
                                ref _platform.format,
                                _platform.keepFormat ? "Keep Format" : "Format",
                                MenuLabelWidth);

                if (_type == PlatformType.Android)
                {
                    _platform.allowsAlphaSplitting = EnumPopup("Split Alpha Channel", _platform.allowsAlphaSplitting);
                    ToggleEnumPopup(ref _platform.keepAndroidETC2FallbackOverride,
                                    ref _platform.androidETC2FallbackOverride,
                                    _platform.keepAndroidETC2FallbackOverride ? "Keep Override ETC2 fallback" : "Override ETC2 fallback",
                                    MenuLabelWidth);
                }
                else
                {
                    _platform.allowsAlphaSplitting = BoolType.Keep;
                    _platform.keepAndroidETC2FallbackOverride = true;
                }
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
            var fileGUIDs = AssetDatabase.FindAssets("t:Texture", paths);
            var importers = new List<TextureImporter>(fileGUIDs.Length);
            var pathEnd = paths[0].Length + 1;

            if (isTypeAll)
            {
                for (int index = 0; index < fileGUIDs.Length; ++index)
                {
                    var path = AssetDatabase.GUIDToAssetPath(fileGUIDs[index]);
                    if (_InvalidPath(path)) continue;
                    if (TextureImporter.GetAtPath(path) is TextureImporter importer)
                        importers.Add(importer);
                }
            }
            else
            {
                for (int index = 0; index < fileGUIDs.Length; ++index)
                {
                    var path = AssetDatabase.GUIDToAssetPath(fileGUIDs[index]);
                    if (_InvalidPath(path)) continue;
                    if (TextureImporter.GetAtPath(path) is TextureImporter importer
                     && importer.textureType == textureType)
                        importers.Add(importer);
                }
            }

            var keyward = string.IsNullOrEmpty(lastFind)
                        ? string.Empty
                        : $"{typeSearch} : {lastFind}{NL}";
            if (importers.Count < 1)
            {
                EditorUtility.DisplayDialog
                (
                    "Info",
                    $"Not found textures{NL}{NL}" +
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
                $"Are you sure you want to the texture settings?{NL}{NL}" +
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
                TextureImporter _importer = null;
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

        void SettingJob(TextureImporter importer)
        {
            if (importer == null) return;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            {
                bool bValue = false;
                if (!common.keepSpritePixelsPerUnit)
                    settings.spritePixelsPerUnit = common.spritePixelsPerUnit;

                if (!common.keepSpriteMeshType)
                    settings.spriteMeshType = common.spriteMeshType;

                if (!common.keepSpriteExtrude)
                    settings.spriteExtrude = common.spriteExtrude;

                if (!common.keepSpritePivot)
                    settings.spritePivot = common.spritePivot;

                if (GetBoolValue(common.spriteGenerateFallbackPhysicsShape, out bValue))
                    settings.spriteGenerateFallbackPhysicsShape = bValue;


                if (!common.keepAlphaSource)
                    settings.alphaSource = common.alphaSource;

                if (GetBoolValue(common.alphaIsTransparency, out bValue))
                    settings.alphaIsTransparency = bValue;

                if (GetBoolValue(common.ignorePngGamma, out bValue))
                    settings.ignorePngGamma = bValue;

                if (GetBoolValue(common.readable, out bValue))
                    settings.readable = bValue;

                if (GetBoolValue(common.streamingMipmaps, out bValue))
                    settings.streamingMipmaps = bValue;

                if (GetBoolValue(common.mipmapEnabled, out bValue))
                    settings.mipmapEnabled = bValue;

                if (!common.keepWrapMode)
                    settings.wrapMode = common.wrapMode;

                if (!common.keepFilterMode)
                    settings.filterMode = common.filterMode;
            }
            importer.SetTextureSettings(settings);

            foreach (var platform in platforms.Values)
                SetPlatformConfig(importer, platform);
        }

        void SetPlatformConfig(TextureImporter importer, Platform platform)
        {
            if (importer == null) return;
            if (platform == null) return;
            if (platform.keepPlatform) return;

            var isDefault = platform.type == PlatformType.Default;
            var settings_default = importer.GetDefaultPlatformTextureSettings();
            var settings = isDefault
                         ? settings_default
                         : importer.GetPlatformTextureSettings(platform.name);
            {
                bool result = false;
                if (GetBoolValue(platform.overridden, out result))
                    settings.overridden = result;

                if (!platform.keepMaxTextureSize)
                {
                    settings.maxTextureSize = platform.maxTextureSize == MaxSizeType.Default
                                            ? settings_default.maxTextureSize
                                            : 1 << (int)platform.maxTextureSize;
                }

                if (!platform.keepResizeAlgorithm)
                {
                    settings.resizeAlgorithm = platform.resizeAlgorithm == (TextureResizeAlgorithm)(-1)
                                             ? settings_default.resizeAlgorithm
                                             : platform.resizeAlgorithm;
                }

                if (!platform.keepFormat)
                    settings.format = platform.format;

                if (GetBoolValue(platform.allowsAlphaSplitting, out result))
                    settings.allowsAlphaSplitting = result;

                if (!platform.keepAndroidETC2FallbackOverride)
                    settings.androidETC2FallbackOverride = platform.androidETC2FallbackOverride;
            }
            importer.SetPlatformTextureSettings(settings);
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
