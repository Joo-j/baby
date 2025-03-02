using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using SimpleJSON;
using System.Text;

namespace Supercent.Util.AppleIconGeneratorV2.EDT
{
    public class AppleIconGeneratorV2Wnd : EditorWindow
    {
        //------------------------------------------------------------------------------
        // MenuItem
        //------------------------------------------------------------------------------
        [MenuItem("Supercent/Util/애플 앱 아이콘/아이콘 설정 창")]
        private static void MenuItem_ShowAppliIconGeneratorV2Wnd()
        {
            var wnd = GetWindow<AppleIconGeneratorV2Wnd>(true, "아이콘 설정 창");
            wnd.CreateGUIStyle();
            wnd.minSize = new Vector2(350f, 350f);
        }
        
        //------------------------------------------------------------------------------
        // data structures
        //------------------------------------------------------------------------------
        private struct HeaderInfo
        {
            public string Name  { get; private set; }
            public float  Width { get; private set; }

            public HeaderInfo(string name, float width)
            {
                Name  = name;
                Width = width;
            }
        }

        private struct IconInfo
        {
            public string Name { get; private set; }
            public int Size { get; private set; }

            public IconInfo(string name, int size)
            {
                Name = name;
                Size = size;
            }
        }

        private struct SpriteInfo
        {
            public int    Index { get; private set; }
            public string Path  { get; private set; }

            public SpriteInfo(int index, string path)
            {
                Index = index;
                Path  = path;
            }
        }

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private bool _loadedSettingFile = false;
        private bool _loadedConfigFile  = false;
        private bool _useMultipleIcons  = false;

        private List<Sprite> _sprites           = new List<Sprite>();
        private List<Sprite> _spritesForRemoval = new List<Sprite>();
        private System.Type  _spriteType        = typeof(Sprite);

        private Vector2 _scrollPos = Vector2.zero;
        private bool _isChangedSetting = false;

        private GUIStyle _styleUseMultipleIcons = null;
        private GUIStyle _styleNotuseMultipleIcons = null;

        private readonly IconInfo[] _iconInfos = new IconInfo[]
        {
            new IconInfo("Icon-iPhone-120",             120),
            new IconInfo("Icon-iPhone-180",             180),
            new IconInfo("Icon-iPhone-Settings-29",     29),
            new IconInfo("Icon-iPhone-Settings-58",     58),
            new IconInfo("Icon-iPhone-Settings-87",     87),
            new IconInfo("Icon-iPhone-Spotlight-80",    80),
            new IconInfo("Icon-iPhone-Spotlight-120",   120),
            new IconInfo("Icon-iPhone-Notification-40", 40),
            new IconInfo("Icon-iPhone-Notification-60", 60),

            new IconInfo("Icon-iPad-Settings-29",       29),
            new IconInfo("Icon-iPad-Settings-58",       58),
            new IconInfo("Icon-iPad-Spotlight-40",      40),
            new IconInfo("Icon-iPad-Spotlight-80",      80),

            new IconInfo("Icon-iPad-76",                76),
            new IconInfo("Icon-iPad-152",               152),
            new IconInfo("Icon-iPad-167",               167),
            new IconInfo("Icon-iPad-Notification-20",   20),
            new IconInfo("Icon-iPad-Notification-40",   40),
            new IconInfo("Icon-Store-1024",             1024),
        };

        private readonly HeaderInfo[] HEADER_INFOS = new HeaderInfo[]
        {
            new HeaderInfo("SPRITE", 200f),
            new HeaderInfo("DELETE", 100f),
        };

        private const int HEADER_INFO_INDEX_SPRITE = 0;
        private const int HEADER_INFO_INDEX_DELETE = 1;

        private const string JNN_USE_MULTIPLE_ICONS = "u";
        private const string JNN_SPRITE_INFOS       = "s";
        private const string JNN_INDEX              = "i";
        private const string JNN_SPRITE_PATH        = "p";

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        private string RootFolder       => AppleMultipleIconsHelper.RootFolder;
        private string SettingsFilePath => AppleMultipleIconsHelper.SettingsFilePath;
        private string ConfigFilePath   => AppleMultipleIconsHelper.ConfigFilePath;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void OnGUI()
        {
            if (!_loadedSettingFile)
                LoadSettings();

            if (!_loadedConfigFile)
            {
                _loadedConfigFile = true;
                _useMultipleIcons = Directory.Exists(RootFolder) && File.Exists(ConfigFilePath);
            }

            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.Space();

                if (_useMultipleIcons)
                    EditorGUILayout.LabelField("멀티플 아이콘을 사용 중 입니다.", _styleUseMultipleIcons);
                else
                    EditorGUILayout.LabelField("멀티플 아이콘을 사용하지 않고 있습니다.", _styleNotuseMultipleIcons);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                _isChangedSetting = false;

                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                {
                    // header
                    EditorGUILayout.LabelField("아이콘 목록");

                    // sprites
                    for (int i = 0, size = _sprites.Count; i < size; ++i)
                        DrawSpriteItem(i);

                    // add button
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("", GUILayout.Width(75f));

                        if (GUILayout.Button("스프라이트 추가", GUILayout.Height(25f)))
                            _sprites.Add(null);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("아이콘 사용하기 (생성)", GUILayout.Height(30f)))
                        {
                            if (TryGenerateIcons(out var folders))
                            {
                                SaveConfig(folders);
                            }
                        }

                        if (GUILayout.Button("아이콘 사용 해제하기", GUILayout.Height(30f)))
                            RemoveConfig();
                    }
                    EditorGUILayout.EndHorizontal();

                    // proc delete
                    if (0 < _spritesForRemoval.Count)
                    {
                        for (int i = 0, size = _spritesForRemoval.Count; i < size; ++i)
                            DeleteSprite(_spritesForRemoval[i]);

                        _spritesForRemoval.Clear();
                        _isChangedSetting = true;
                    }
                }
                EditorGUILayout.EndScrollView();

                if (_isChangedSetting)
                    SaveSettings();
            }
            --EditorGUI.indentLevel;
        }

        private void LoadSettings()
        {
            _loadedSettingFile = true;

            if (!Directory.Exists(RootFolder) || !File.Exists(SettingsFilePath))
                return;

            // 파일 로드
            var token = string.Empty;
            
            try
            {
                token = File.ReadAllText(SettingsFilePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AppleIconGeneratorV2Wnd.LoadConfig] 예외 발생. {ex.ToString()}");
                return;
            }

#if NO_JSON
            Debug.LogAssertion($"{nameof(LoadSettings)} : Please remove the NO_JSON preprocessor directive.");
#else
            // json 파싱 시도
            var json = JSON.Parse(token);
            if (null == json || !(json is JSONClass jc))
                return;

            var array = jc[JNN_SPRITE_INFOS];
            if (null == array || 0 == array.Count)
                return;

            var infos = new List<SpriteInfo>();

            foreach (JSONClass child in array.Childs)
            {
                if (null == child)
                    continue;

                var index = child[JNN_INDEX].AsInt;
                var path  = child[JNN_SPRITE_PATH].Value;

                if (index < 0 
                    || string.IsNullOrEmpty(path)
                    || !File.Exists(path))
                    continue;

                infos.Add(new SpriteInfo(index, path));
            }

            infos.Sort((l, r) => l.Index.CompareTo(r.Index));

            // sprite 로드 시도
            _sprites.Clear();

            for (int i = 0, size = infos.Count; i < size; ++i)
                _sprites.Add(AssetDatabase.LoadAssetAtPath<Sprite>(infos[i].Path));
#endif// NO_JSON
        }

        private void SaveSettings()
        {
            _isChangedSetting = false;
#if NO_JSON
            Debug.LogAssertion($"{nameof(SaveSettings)} : Please remove the NO_JSON preprocessor directive.");
#else
            var array = new JSONArray();

            for (int i = 0, size = _sprites.Count; i < size; ++i)
            {
                var child = new JSONClass();
                child[JNN_INDEX] = i.ToString();
                child[JNN_SPRITE_PATH] = AssetDatabase.GetAssetPath(_sprites[i]);
                array.Add(child);
            }

            var json = new JSONClass();
            json[JNN_SPRITE_INFOS] = array;

            try
            {
                File.WriteAllText(SettingsFilePath, json.ToString());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AppleIconGeneratorV2Wnd.SaveSettings] 예외 발생. {ex.ToString()}");
            }
#endif// NO_JSON
        }

        private void SaveConfig(StringBuilder folders)
        {
            File.WriteAllText(ConfigFilePath, folders.ToString());
            _useMultipleIcons = true;
        }

        private void RemoveConfig()
        {
            if (!Directory.Exists(RootFolder) || !File.Exists(ConfigFilePath))
                return;

            File.Delete(ConfigFilePath);
            _useMultipleIcons = false;
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            {
                for (int i = 0, size = HEADER_INFOS.Length; i < size; ++i)
                    EditorGUILayout.LabelField(HEADER_INFOS[i].Name, GUILayout.Width(HEADER_INFOS[i].Width));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSpriteItem(int index)
        {
            EditorGUILayout.BeginHorizontal();
            {
                // sprite
                var newSprite = (Sprite)EditorGUILayout.ObjectField($"Style{index}", _sprites[index], _spriteType, true, GUILayout.Width(HEADER_INFOS[HEADER_INFO_INDEX_SPRITE].Width));
                if (newSprite != _sprites[index])
                {
                    _sprites[index] = newSprite;
                    _isChangedSetting = true;
                }

                // delete
                if (GUILayout.Button("제거", GUILayout.Width(HEADER_INFOS[HEADER_INFO_INDEX_DELETE].Width)))
                    _spritesForRemoval.Add(_sprites[index]);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DeleteSprite(Sprite sprite)
        {
            for (int i = 0, size = _sprites.Count; i < size; ++i)
            {
                if (sprite == _sprites[i])
                {
                    _sprites.RemoveAt(i);
                    return;
                }
            }
        }

        private void CreateGUIStyle()
        {
            _styleUseMultipleIcons = new GUIStyle();
            _styleUseMultipleIcons.normal.textColor = new Color(92f / 255f, 150f / 255f, 214f / 255f, 1f);
            _styleUseMultipleIcons.fontSize = 18;

            _styleNotuseMultipleIcons = new GUIStyle();
            _styleNotuseMultipleIcons.normal.textColor = Color.yellow;
        }

        private bool TryGenerateIcons(out StringBuilder generatedAssetFolders)
        {
            generatedAssetFolders = new StringBuilder();

            if (0 == _sprites.Count)
            {
                Debug.LogError($"[AppleIconGenerator] 생성할 아이콘이 하나도 없습니다.");
                return false;
            }

            // root folder
            var root = RootFolder;
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            // load contents template
            var contentsTemplate = Resources.Load<TextAsset>("AppleIconContentsTemplate").text;
            var generatedCount   = 0;

            // generate icons
            for (int i = 0, size = _sprites.Count; i < size; ++i)
            {
                if (null == _sprites[i])
                    continue;

                var styleName  = $"Style{i}";
                var assetsName = $"{styleName}.appiconset";
                var folderPath = $"{root}/{assetsName}";

                // folder
                if (Directory.Exists(folderPath))
                    Directory.Delete(folderPath, true);
                    
                Directory.CreateDirectory(folderPath);

                // icons
                for (int infoIndex = 0, infoSize = _iconInfos.Length; infoIndex < infoSize; ++infoIndex)
                    CreatePNG(_sprites[i], ref _iconInfos[infoIndex], ref folderPath);

                // contents
                File.WriteAllText($"{folderPath}/Contents.json", contentsTemplate);

                ++generatedCount;
                generatedAssetFolders.AppendLine(assetsName);

                Debug.Log($"[AppleIconGenerator] <color=#55ff88>Generated.</color>  asssets: Style{i}".CornflowerBlue());
            }

            return 0 < generatedCount;
        }

        private void CreatePNG(Sprite sprite, ref IconInfo info, ref string folder)
        {
            RenderTexture.active = new RenderTexture(info.Size, info.Size, 16, RenderTextureFormat.ARGB32);

            // draw
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, info.Size, info.Size, 0);

            var infoRect = new Rect(0, 0, info.Size, info.Size);
            Graphics.DrawTexture(infoRect, sprite.texture);

            GL.PopMatrix();

            // to texture
            var texture = new Texture2D(info.Size, info.Size, TextureFormat.ARGB32, 0, false);
            texture.ReadPixels(infoRect, 0, 0, false);
            texture.Apply();

            // save
            File.WriteAllBytes($"{folder}/{info.Name}.png", texture.EncodeToPNG());
        }
    }
}
