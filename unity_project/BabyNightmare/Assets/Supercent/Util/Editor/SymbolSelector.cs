using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Supercent.Util.Editor
{
    public sealed class SymbolSelector : EditorWindowBase
    {
        const string PathRoot = "Assets";
        const string PathBase = "Supercent";

        #region Symbol form
        [Serializable]
        class SymbolList
        {
            [SerializeField] public List<string> symbols = new List<string>();
        }

        public sealed class SymbolInfo
        {
            public bool isNew = false;
            public bool isApplied = false;
            public string name = string.Empty;
            public string caption = string.Empty;

            public override int GetHashCode()       => base.GetHashCode();
            public override bool Equals(object obj) => obj is SymbolInfo && name.Equals((obj as SymbolInfo).name, StringComparison.CurrentCultureIgnoreCase);

            public static bool TryGetWords(string text, out string symbol, out string caption)
            {
                if (string.IsNullOrEmpty(text)) { symbol = string.Empty; caption = string.Empty; return false; }

                var words = text.Split(':');
                symbol  = words != null && 0 < words.Length ? words[0].Replace(" ", "").Replace(";", "") : string.Empty;
                caption = words != null && 1 < words.Length ? words[1] : string.Empty;
                return true;
            }
        }
        #endregion Symbol form



        static readonly string PATH_INFO = @$"{PATH_SETTING}/SymbolInfo.json";
        static readonly string[] separator = new string[] { "&&", "||" };
        BuildTargetGroup targetBuild = BuildTargetGroup.Standalone;
        readonly SortedDictionary<string, SymbolInfo> symbolInfoMap = new SortedDictionary<string, SymbolInfo>();
        readonly string[] paths = { string.IsNullOrEmpty(PathBase) ? PathRoot : Path.Combine(PathRoot, PathBase) };
        readonly List<string> tempPath = new List<string>();

        bool includeUnity = false;
        bool includeEditor = false;
        bool isFoldFindSymbol = false;
        bool isFoldAddSymbol = false;
        bool isFoldUnappliedList = true;
        bool isFoldAppliedList = true;
        string lastPath = PathBase;
        string newSymbol = string.Empty;
        string newCaption = string.Empty;
        Vector2 posScroll = Vector2.zero;

        GUIContent conAllApplied = new GUIContent("A↓", "All Applied");
        GUIContent conAllUnapplied = new GUIContent("A↑", "All Unapplied");
        GUIContent conOpen = new GUIContent("O", "Open");
        GUIContent conDefault = new GUIContent("D", "Default");
        GUIContent conClear = new GUIContent("C", "Clear");
        GUIContent conApplied = new GUIContent("↓", "Applied");
        GUIContent conUnapplied = new GUIContent("↑", "Unapplied");
        GUIContent conAdd = new GUIContent("+", "Add");
        GUIContent conDelete = new GUIContent("x", "Delete");

        GUIStyle stySymbolBtn = null;
        GUIStyle stySymbolLableBtn = null;
        GUIStyle stySymbolCap = null;
        GUIStyle styPathText = null;
        GUIStyle styMenuBtn = null;
        GUIStyle styAddText = null;
        GUILayoutOption optLabelWidth = GUILayout.Width(50);
        GUILayoutOption optSubBtnWdith = GUILayout.Width(30);
        GUILayoutOption optFindBtnWdith = GUILayout.Width(50);
        GUILayoutOption optTglWidth = GUILayout.Width(15);
        GUILayoutOption optTglLabelWidth = GUILayout.Width(35);
        GUILayoutOption optSymbolMinWidth = GUILayout.MinWidth(100);



        [MenuItem("Supercent/Util/Symbol Selector &D")]
        public static void OpenWindow()
        {
            var window = GetWindow<SymbolSelector>(false, "Symbol Selector");
            if (window != null) window.Show();
        }

        void OnEnable()
        {
            stySymbolBtn = null;
            stySymbolLableBtn = null;
            stySymbolCap = null;
            styPathText = null;
            styMenuBtn = null;
            styAddText = null;

            KeyDownUnsubscribeAll();
            KeyDownSubscribe(KeyCode.F5, () => LoadSymbolInfo());

            targetBuild = EditorUserBuildSettings.selectedBuildTargetGroup;
            LoadSymbolInfo();
            SetPath(lastPath);

            guiDraw = GUIDraw;
        }

        void OnDestroy()
        {
            tempPath.Clear();
        }



        #region Draw menu
        void GUIDraw()
        {
            SetStyle();

            ViewTopMenu();
            EditorGUILayout.Space();
            var curPos = EditorGUILayout.BeginScrollView(posScroll, GUIStyle.none);
            {
                var curEvent = Event.current.type;
                if (curEvent != EventType.Repaint)
                    posScroll = curPos;

                ViewUnappliedList();
                DivisionLine(Color.gray, 5, 1f);
                ViewAppliedList();

                if (curEvent == EventType.Repaint)
                    posScroll = curPos;
            }
            EditorGUILayout.EndScrollView();
        }

        void SetStyle()
        {
            if (stySymbolBtn == null)
            {
                stySymbolBtn = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset { bottom = 1, },
                    fontSize = 13,
                    fixedWidth = 18,
                    fixedHeight = 18,
                };
            }
            if (stySymbolLableBtn == null)
            {
                stySymbolLableBtn = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset { top = 3, },
                    stretchWidth = true,
                };
            }
            if (stySymbolCap == null)
            {
                stySymbolCap = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset { top = 3, left = 20, },
                    stretchWidth = true,
                };
            }

            if (styPathText == null)
            {
                styPathText = new GUIStyle(GUI.skin.textField)
                {
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset { top = 3, right = 3, left = 3, },
                    fixedHeight = 18,
                };
            }
            if (styMenuBtn == null)
            {
                styMenuBtn = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset { left = 1, bottom = 1 },
                    fontSize = 12,
                    fixedWidth = 18,
                    fixedHeight = 18,
                };
                var margin = styMenuBtn.margin;
                margin.top += 1;
                styMenuBtn.margin = margin;
            }

            if (styAddText == null)
            {
                styAddText = new GUIStyle(GUI.skin.textField)
                {
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset { top = 3, right = 3, left = 3, },
                    fixedHeight = 18,
                };
            }
        }

        void ViewTopMenu()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Platform", optLabelWidth);
                var target = EnumPopup(targetBuild);
                if (target == BuildTargetGroup.Unknown)
                    target = EditorUserBuildSettings.selectedBuildTargetGroup;
                if (target != targetBuild)
                {
                    targetBuild = target;
                    LoadSymbolInfo();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                if (!EditorApplication.isPlaying
                 && Button("Apply"))
                    SaveSymbolInfo();

                if (Button("Reload"))
                    LoadSymbolInfo();

                if (Button(conAllApplied, optSubBtnWdith))
                {
                    foreach (var item in symbolInfoMap.Values)
                        item.isApplied = true;
                }

                if (Button(conAllUnapplied, optSubBtnWdith))
                {
                    foreach (var item in symbolInfoMap.Values)
                        item.isApplied = false;
                }
            }
            EditorGUILayout.EndHorizontal();


            if (isFoldFindSymbol = EditorGUILayout.Foldout(isFoldFindSymbol, "Find symbols"))
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var editPath = EditorGUILayout.TextField(lastPath, styPathText);
                    if (string.CompareOrdinal(editPath, lastPath) != 0)
                        SetPath(editPath);

                    if (Button(conOpen, styMenuBtn))
                    {
                        if (OpenAssetsFolderPanel("Select a folder", lastPath, string.Empty, out var assetsPath))
                            SetPath(assetsPath);
                    }

                    if (Button(conDefault, styMenuBtn))
                        SetPath(PathBase);

                    if (Button(conClear, styMenuBtn))
                        SetPath(string.Empty);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Button("Find", optFindBtnWdith))
                        FindAll();

                    includeUnity = EditorGUILayout.Toggle(includeUnity, optTglWidth);
                    EditorGUILayout.LabelField("Unity", optTglLabelWidth);
                    includeEditor = EditorGUILayout.Toggle(includeEditor, optTglWidth);
                    EditorGUILayout.LabelField("Editor", optTglLabelWidth);
                }
                EditorGUILayout.EndHorizontal();
            }


            if (isFoldAddSymbol = EditorGUILayout.Foldout(isFoldAddSymbol, "Add symbol"))
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Name", optLabelWidth);
                    newSymbol = EditorGUILayout.TextField(newSymbol, styAddText);

                    if (!string.IsNullOrEmpty(newSymbol)
                     && Button(conAdd, styMenuBtn))
                    {
                        if (!symbolInfoMap.TryGetValue(newSymbol, out var info))
                            symbolInfoMap.Add(newSymbol, new SymbolInfo { isNew = false, isApplied = false, name = newSymbol, caption = newCaption, });
                        else
                        {
                            if (info == null)
                            {
                                info = new SymbolInfo { isApplied = false, name = newSymbol, };
                                symbolInfoMap[newSymbol] = info;
                            }
                            info.caption = newCaption;
                            info.isNew = false;
                        }

                        newSymbol = string.Empty;
                        SaveSymbolInfo();
                    }

                    if (Button(conClear, styMenuBtn))
                        newSymbol = string.Empty;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Caption", optLabelWidth);
                    newCaption = EditorGUILayout.TextField(newCaption, styAddText);

                    if (Button(conClear, styMenuBtn))
                        newCaption = string.Empty;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        void ViewUnappliedList()
        {
            if (isFoldUnappliedList = EditorGUILayout.Foldout(isFoldUnappliedList, "Unapplied"))
            {
                var labelContent = new GUIContent();
                foreach (var info in symbolInfoMap.Values)
                {
                    if (info.isApplied) continue;

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (Button(conApplied, stySymbolBtn))
                            info.isApplied = true;

                        ViewSymbolLabel(labelContent, info);

                        if (info.isNew)
                        {
                            if (Button(conAdd, styMenuBtn))
                            {
                                info.isNew = false;
                                SaveSymbolInfo();
                            }
                        }
                        else
                        {
                            if (Button(conDelete, styMenuBtn))
                            {
                                symbolInfoMap.Remove(info.name);
                                SaveSymbolInfo();
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    ViewCaptionLable(labelContent, info);
                }
            }
        }

        void ViewAppliedList()
        {
            if (isFoldAppliedList = EditorGUILayout.Foldout(isFoldAppliedList, "Applied"))
            {
                var labelContent = new GUIContent();
                foreach (var info in symbolInfoMap.Values)
                {
                    if (!info.isApplied) continue;

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (Button(conUnapplied, stySymbolBtn))
                            info.isApplied = false;
                        ViewSymbolLabel(labelContent, info);

                        if (info.isNew)
                        {
                            if (Button(conAdd, styMenuBtn))
                            {
                                info.isNew = false;
                                SaveSymbolInfo();
                            }
                        }
                        else
                        {
                            if (Button(conDelete, styMenuBtn))
                            {
                                info.isNew = true;
                                SaveSymbolInfo();
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    ViewCaptionLable(labelContent, info);
                }
            }
        }

        void ViewSymbolLabel(GUIContent labelContent, SymbolInfo info)
        {
            if (info == null) return;

            labelContent.text = info.name;
            if (Button(labelContent, stySymbolLableBtn, optSymbolMinWidth))
            {
                isFoldAddSymbol = true;
                newSymbol = info.name;
                newCaption = info.caption;
            }
        }

        void ViewCaptionLable(GUIContent labelContent, SymbolInfo info)
        {
            if (info == null) return;
            if (string.IsNullOrEmpty(info.caption)) return;

            labelContent.text = info.caption;
            if (Button(labelContent, stySymbolCap, optSymbolMinWidth))
            {
                isFoldAddSymbol = true;
                newSymbol = info.name;
                newCaption = info.caption;
            }
        }
        #endregion// Draw menu


        #region Find
        void SetPath(string path)
        {
            lastPath = path;
            paths[0] = string.IsNullOrEmpty(lastPath)
                     ? PathRoot
                     : Path.Combine(PathRoot, lastPath);
        }

        void FindAll()
        {
            tempPath.Clear();
            {
                var fileGUIDs = AssetDatabase.FindAssets("t:Script", paths);
                var pathProj = Application.dataPath;
                var offset = pathProj.LastIndexOf('/');
                if (-1 < offset)
                    pathProj = pathProj.Remove(offset);

                if (includeEditor)
                {
                    for (int index = 0; index < fileGUIDs.Length; ++index)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(fileGUIDs[index]);
                        if (string.IsNullOrEmpty(path)) continue;

                        tempPath.Add(Path.Combine(pathProj, path));
                    }
                }
                else
                {
                    for (int index = 0; index < fileGUIDs.Length; ++index)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(fileGUIDs[index]);
                        if (string.IsNullOrEmpty(path)) continue;
                        if (-1 < path.IndexOf("editor", StringComparison.OrdinalIgnoreCase)) continue;

                        tempPath.Add(Path.Combine(pathProj, path));
                    }
                }

                Parallel.ForEach(tempPath, ScriptCheck);
            }
            tempPath.Clear();
        }

        void ScriptCheck(string path)
        {
            if (!File.Exists(path))
                return;

            var script = File.ReadAllText(path);
            if (string.IsNullOrEmpty(script))
                return;

            var sb = new StringBuilder(script);
            script = null;
            for (int index = sb.Length - 1; -1 < index; --index)
            {
                var c = sb[index];
                if (c == '#' || c == '|' || c == '&'
                 || c == '_' || c == '\n'
                 || char.IsNumber(c)
                 || char.IsUpper(c) || char.IsLower(c))
                    continue;

                sb.Remove(index, 1);
            }

            for (int isd = sb.Length - 1; -1 < isd; --isd)
            {
                if (sb[isd] != '\n') continue;

                for (int isub = isd - 1; -1 < isub; --isub)
                {
                    if (sb[isub] == '#')
                    {
                        isd = isub;
                        break;
                    }
                    if (sb[isub] == '\n')
                    {
                        var i = isub + 1;
                        sb.Remove(i, isd - isub);
                        isd = i;
                        break;
                    }
                }
            }

            if (sb.Length < 1)
                return;

            var lines = sb.ToString().Split('\n');
            if (lines == null || lines.Length < 1)
                return;
            sb = null;

            for (var il = 0; il < lines.Length; ++il)
            {
                var line = lines[il];
                if (string.IsNullOrEmpty(line))
                    continue;

                if (0 == string.CompareOrdinal(line, 0, "#if", 0, 3))
                {
                    ParseAndSet(line.Remove(0, 3));
                    continue;
                }
                if (0 == string.CompareOrdinal(line, 0, "#elif", 0, 5))
                {
                    ParseAndSet(line.Remove(0, 5));
                    continue;
                }
                if (0 == string.CompareOrdinal(line, 0, "#define", 0, 7))
                {
                    ParseAndSet(line.Remove(0, 7));
                    continue;
                }
            }
        }

        void ParseAndSet(string line)
        {
            var symbols = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (symbols == null || symbols.Length < 1)
                return;

            if (includeUnity)
            {
                for (int index = 0; index < symbols.Length; ++index)
                {
                    var symbol = symbols[index];
                    if (0 == string.Compare(symbol, "TRUE", true)) continue;
                    if (0 == string.Compare(symbol, "FALSE", true)) continue;
                    if (symbolInfoMap.ContainsKey(symbol)) continue;

                    lock ((symbolInfoMap as ICollection).SyncRoot)
                        symbolInfoMap[symbol] = new SymbolInfo() { isNew = true, isApplied = false, name = symbol };
                }
            }
            else
            {
                for (int index = 0; index < symbols.Length; ++index)
                {
                    var symbol = symbols[index];
                    if (0 == string.Compare(symbol, "TRUE", true)) continue;
                    if (0 == string.Compare(symbol, "FALSE", true)) continue;
                    if (0 == string.CompareOrdinal(symbol, 0, "UNITY_", 0, 6)) continue;
                    if (symbolInfoMap.ContainsKey(symbol)) continue;

                    lock ((symbolInfoMap as ICollection).SyncRoot)
                        symbolInfoMap[symbol] = new SymbolInfo() { isNew = true, isApplied = false, name = symbol };
                }
            }
        }
        #endregion// Find


        #region Info IO
        void SaveSymbolInfo()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogAssertion($"{nameof(SaveSymbolInfo)} : It is not supported when playing");
                return;
            }

            var infos = new SymbolList();
            var symbolBuilder = new StringBuilder();
            foreach (var symbol in symbolInfoMap.Values)
            {
                if (symbol == null) continue;
                if (string.IsNullOrEmpty(symbol.name)) continue;
                if (symbol.isApplied) symbolBuilder.Append(symbol.name).Append(";");
                if (symbol.isNew) continue;

                infos.symbols.Add($"{symbol.name}:{symbol.caption}");
            }

            SaveJson(PATH_INFO, infos);

            // Set symbol
            if (targetBuild != BuildTargetGroup.Unknown)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetBuild, symbolBuilder.ToString());
        }

        void LoadSymbolInfo()
        {
            symbolInfoMap.Clear();

            if (File.Exists(PATH_INFO))
                ParseSymbol(File.ReadAllText(PATH_INFO), symbolInfoMap);
            GetSymbol(targetBuild, symbolInfoMap);
        }

        public static void ParseSymbol(string json, SortedDictionary<string, SymbolInfo> map)
        {
            if (map == null) return;
            if (string.IsNullOrEmpty(json))return;

            try
            {
                var infos = JsonUtility.FromJson<SymbolList>(json);
                foreach (var name in infos.symbols)
                {
                    if (SymbolInfo.TryGetWords(name, out var symbolWord, out var commentWord))
                        map.Add(symbolWord, new SymbolInfo { isNew = false, isApplied = false, name = symbolWord, caption = commentWord });
                }
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        public static void GetSymbol(BuildTargetGroup target, SortedDictionary<string, SymbolInfo> map)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target)?.Split(';');
            if (symbols == null || symbols.Length < 1) return;

            foreach (var symbol in symbols)
            {
                if (string.IsNullOrEmpty(symbol) || symbol[0] == ' ' || symbol[0] == ';') continue;

                if (!map.TryGetValue(symbol, out var info))
                    map.Add(symbol, new SymbolInfo { isNew = true, isApplied = true, name = symbol });
                else
                {
                    if (info == null)
                    {
                        info = new SymbolInfo { name = symbol };
                        map[symbol] = info;
                    }
                    info.isNew = false;
                    info.isApplied = true;
                }
            }
        }
        #endregion// Info IO
    }
}