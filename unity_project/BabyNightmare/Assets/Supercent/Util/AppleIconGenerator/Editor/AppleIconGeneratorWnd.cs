using UnityEngine;
using UnityEditor;
using System.IO;

namespace Supercent.Util.AppleIconGenerator.EDT
{
    public class AppleIconGeneratorWnd : EditorWindow
    {
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
        // menu item
        //------------------------------------------------------------------------------
        // [MenuItem("Supercent/Util/애플 앱 아이콘/아이콘 생성기")]
        private static void MenuItem_ShowAppleIconGeneratorWnd()
        {
            var wnd = GetWindow<AppleIconGeneratorWnd>(true, "Apple Icon Generator");
            wnd.minSize = new Vector2(500f, 250f);
        }

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private Sprite _icon = null;
        private string _folderName = string.Empty;

        private System.Type _iconType = typeof(Sprite);

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

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        private string IconFolder => $"{Application.dataPath}/../../../AppleIcons/{_folderName}.appiconset";

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void OnGUI()
        {
            ++EditorGUI.indentLevel;
            {

                _icon = (Sprite)EditorGUILayout.ObjectField("아이콘", _icon, _iconType, true);

                EditorGUILayout.Space();
                _folderName = EditorGUILayout.TextField("폴더명", _folderName); 

                EditorGUILayout.Space();
                if (GUILayout.Button("아이콘 생성", GUILayout.Height(25f)))
                    TryGenerateIcons();
            }
            --EditorGUI.indentLevel;
        }

        private bool TryGenerateIcons()
        {
            if (null == _icon)
            {
                Debug.LogError("[AppleIconGenerator] 아이콘을 지정하세요.");
                return false;
            }

            if (string.IsNullOrEmpty(_folderName))
            {
                Debug.LogError("[AppleIconGenerator] 폴더명을 입력하세요.");
                return false;
            }

            CreateFolder();
            CreateIcons();
            CreateContentsJson();

            return true;
        }

        private void CreateFolder()
        {
            var path = Application.dataPath;

            // root 폴더 확인
            var appleIconFolder = $"{path}/../../../AppleIcons/";
            if (!Directory.Exists(appleIconFolder))
                Directory.CreateDirectory(appleIconFolder);

            // icon 폴더 확인
            if (!Directory.Exists(IconFolder))
                Directory.CreateDirectory(IconFolder);
        }

        private void CreateIcons()
        {
            for (int i = 0, size = _iconInfos.Length; i < size; ++i)
                CreateIconSingle(_iconInfos[i]);

            Debug.Log("<color=#55ff88>[AppleIconGenerator] 아이콘 생성이 완료되었습니다.</color>");
        }

        private void CreateIconSingle(IconInfo info)
        {
            var rt = new RenderTexture(info.Size, info.Size, 16, RenderTextureFormat.ARGB32);
            RenderTexture.active = rt;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, info.Size, info.Size, 0);
            Graphics.DrawTexture(new Rect(0, 0, info.Size, info.Size), _icon.texture);
            GL.PopMatrix();
            
            var tex2D = new Texture2D(info.Size, info.Size, TextureFormat.ARGB32, 0, false);
            tex2D.ReadPixels(new Rect(0, 0, info.Size, info.Size), 0, 0, false);
            tex2D.Apply();
            
            File.WriteAllBytes($"{IconFolder}/{info.Name}.png", tex2D.EncodeToPNG());
        }

        private void CreateContentsJson()
        {
            var template = Resources.Load<TextAsset>("AppleIconContentsTemplate").text;
            var fileName = $"{IconFolder}/Contents.json";
            
            File.WriteAllText(fileName, template);
        }
    }
}