using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Supercent.Util.AppleIconGenerator.EDT
{
    public class AppleIconControllerWnd : EditorWindow
    {
        //------------------------------------------------------------------------------
        // menu item
        //------------------------------------------------------------------------------
        // [MenuItem("Supercent/Util/애플 앱 아이콘/아이콘 생성기")]
        private static void MenuItem_ShowAppleIconGeneratorWnd()
        {
            var wnd = GetWindow<AppleIconControllerWnd>(true, "Apple Icon Controller");
            wnd.minSize = new Vector2(350f, 350f);
        }

        //------------------------------------------------------------------------------
        // 
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

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private List<Sprite> _sprites    = new List<Sprite>();
        private System.Type  _spriteType = typeof(Sprite);

        private List<Sprite> _itemsForRemoval = new List<Sprite>();

        private Vector2 _scrollPos = Vector2.zero;

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
            new HeaderInfo("Sprite", 200f),
            new HeaderInfo("delete", 100f),
        };

        private const int INDEX_SPRITE = 0;
        private const int INDEX_DELETE = 1;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        private string RootFolder => $"{Application.dataPath}/../../../AppleIcons";

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void OnGUI()
        {
            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.LabelField("Apple Icon Controller");
                EditorGUILayout.Space();

                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                {
                    // header
                    DrawHeader();

                    // sprites
                    for (int i = 0, size = _sprites.Count; i < size; ++i)
                        DrawSpriteItem(i);

                    // add button
                    EditorGUILayout.Space();
                    if (GUILayout.Button("스프라이트 추가"))
                        _sprites.Add(null);

                    // generate button
                    EditorGUILayout.Space();
                    if (GUILayout.Button("아이콘 생성하기", GUILayout.Height(25f)))
                        TryGenerateIcons();

                    // proc delete
                    if (0 < _itemsForRemoval.Count)
                    {
                        for (int i = 0, size = _itemsForRemoval.Count; i < size; ++i)
                            DeleteSprite(_itemsForRemoval[i]);

                        _itemsForRemoval.Clear();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            --EditorGUI.indentLevel;
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
                _sprites[index] = (Sprite)EditorGUILayout.ObjectField($"Style{index}", _sprites[index], _spriteType, true, GUILayout.Width(HEADER_INFOS[INDEX_SPRITE].Width));

                // delete
                if (GUILayout.Button("제거", GUILayout.Width(HEADER_INFOS[INDEX_DELETE].Width)))
                    _itemsForRemoval.Add(_sprites[index]);
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

        private void TryGenerateIcons()
        {
            if (0 == _sprites.Count)
            {
                Debug.LogError($"[AppleIconGenerator] 생성할 아이콘이 하나도 없습니다.");
                return;
            }

            // root folder
            var root = RootFolder;
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            // load contents template
            var contentsTemplate = Resources.Load<TextAsset>("AppleIconContentsTemplate").text;

            // generate icons
            for (int i = 0, size = _sprites.Count; i < size; ++i)
            {
                if (null == _sprites[i])
                {
                    Debug.Log($"[AppleIconGenerator] <color=#ff0000>failed generate.</color> asssets: Style{i}".CornflowerBlue());
                    continue;
                }

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

                Debug.Log($"[AppleIconGenerator] <color=#55ff88>Generated.</color>  asssets: Style{i}".CornflowerBlue());
            }
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