using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Supercent.Util.Editor
{
    public class PrefabSelector : EditorWindowBase
    {
        static readonly Type TypeComp = typeof(Component);

        const string PathRoot = "Assets";
        const string PathPackages = "Packages";
        const string PathBase = "Supercent";

        static readonly string KEY_PATH = $"{nameof(PrefabSelector)}.{nameof(lastPath)}";

        [Flags]
        enum AssetType
        {
            Component = 0,
            Engine = 1,
            Assets = 1 << 1,
            Packages = 1 << 2,
            Base = 1 << 3,
        }

        enum SearchType
        {
            Free = 0,
            Prefix,
            Suffix
        }

        struct PathData
        {
            public bool invisible;
            public string Name;
            public string Folder;
            public string Path;

            public string GetAssetPath() => System.IO.Path.Combine("Assets", Folder, $"{Name}.prefab");
        }

        readonly List<PathData> assets = new List<PathData>();
        readonly List<Type> typeComponents = new List<Type>();
        readonly string[] paths = { string.Empty, };
        readonly string[] temps = { string.Empty, };
        bool isNameSort = false;
        bool isDescending = false;
        string lastPath = PathBase;
        string lastFind = string.Empty;
        SearchType typeSearch = SearchType.Free;

        string[] typeNames = null;
        int lastTypeIndex = 0;
        AssetType flagAsset = AssetType.Base;
        bool isRootOnly = true;
        Vector2 posScroll = Vector2.zero;

        GUIContent conOpen = new GUIContent("O", "Open");
        GUIContent conDefault = new GUIContent("D", "Default");
        GUIContent conClear = new GUIContent("C", "Clear");
        GUIContent conName = new GUIContent("N", "Name");
        GUIContent conPath = new GUIContent("P", "Path");
        GUIContent conAsc = new GUIContent("↓", "Ascending");
        GUIContent conDesc = new GUIContent("↑", "Descending");
        GUIContent conSelect = new GUIContent("S", "Select");

        GUIStyle styPathText = null;
        GUIStyle styMenuBtn = null;
        GUIStyle styRootBtn = null;
        GUIStyle stySubGrp = null;
        GUIStyle stySubBtn = null;
        GUIStyle styAssetBtn = null;
        GUILayoutOption optLabelWidth = GUILayout.Width(28);
        GUILayoutOption optDropWidth = GUILayout.Width(55);
        GUILayoutOption optSelectWidth = GUILayout.Width(24);
        GUILayoutOption optSymbolMinWidth = GUILayout.MinWidth(0);



        [MenuItem("Supercent/Util/Prefab Selector &B")]
        public static void OpenWindow()
        {
            GetWindow<PrefabSelector>("Prefab Selector");
        }

        void OnEnable()
        {
            styPathText = null;
            styMenuBtn = null;
            styRootBtn = null;
            stySubGrp = null;
            stySubBtn = null;
            styAssetBtn = null;
            SetPathAndUpdate(EditorPrefs.GetString(KEY_PATH, lastPath));

            guiDraw = GUIDraw;
        }
        void OnFocus()
        {
            LoadComponents(flagAsset);
            SetPathAndUpdate(lastPath);
        }



        #region Draw menu
        void GUIDraw()
        {
            SetStyle();

            EditorGUILayout.BeginVertical();
            {
                ViewTopMenu();
                GUILayout.Space(3);
                var curPos = EditorGUILayout.BeginScrollView(posScroll, GUIStyle.none, GUI.skin.verticalScrollbar);
                {
                    var curEvent = Event.current.type;
                    if (curEvent != EventType.Repaint)
                        posScroll = curPos;

                    ViewPrefabList();

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
            if (styMenuBtn == null)
            {
                styMenuBtn = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset { left = 2, bottom = 1 },
                    fontSize = 12,
                    fixedWidth = 18,
                    fixedHeight = 18,
                };
                var margin = styMenuBtn.margin;
                margin.top += 1;
                styMenuBtn.margin = margin;
            }
            if (styRootBtn == null)
            {
                styRootBtn = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset { left = 2, bottom = 1 },
                    fontSize = 12,
                    fixedWidth = 39,
                    fixedHeight = 18,
                };
                var margin = styRootBtn.margin;
                margin.top += 1;
                styRootBtn.margin = margin;
            }
            if (stySubGrp == null)
            {
                stySubGrp = new GUIStyle()
                {
                    fixedWidth = 24,
                    fixedHeight = 52,
                };
            }
            if (stySubBtn == null)
            {
                stySubBtn = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    richText = false,
                    fixedWidth = 24,
                    fixedHeight = 24,
                };
                stySubBtn.margin.left -= 1;
            }
            if (styAssetBtn == null)
            {
                styAssetBtn = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    clipping = TextClipping.Clip,
                    padding = new RectOffset { left = 2, bottom = 1 },
                    richText = true,
                    fixedHeight = 50,
                    stretchWidth = true,
                };
            }
        }

        void ViewTopMenu()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Path", optLabelWidth);
                {
                    var editPath = EditorGUILayout.TextField(lastPath, styPathText);
                    if (string.CompareOrdinal(editPath, lastPath) != 0)
                        SetPathAndUpdate(editPath);
                }

                if (Button(conOpen, styMenuBtn))
                {
                    if (OpenAssetsFolderPanel("Select a folder", lastPath, string.Empty, out var assetsPath))
                        SetPathAndUpdate(assetsPath);
                }

                if (Button(conDefault, styMenuBtn))
                    SetPathAndUpdate(PathBase);

                if (Button(conClear, styMenuBtn))
                    SetPathAndUpdate(string.Empty);
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            {
                var editType = EnumPopup(typeSearch, optDropWidth);
                if (typeSearch != editType)
                {
                    typeSearch = editType;
                    SetVisible();
                }
                
                var editFind = EditorGUILayout.TextField(lastFind, styPathText);
                if (string.CompareOrdinal(editFind, lastFind) != 0)
                    SetFindAndUpdate(editFind);
                
                if (Button(isNameSort ? conName : conPath, styMenuBtn))
                {
                    isNameSort = !isNameSort;
                    SortJob();
                }
                
                if (Button(isDescending ? conDesc : conAsc, styMenuBtn))
                {
                    isDescending = !isDescending;
                    SortJob();
                }
                
                if (Button(conClear, styMenuBtn))
                    SetFindAndUpdate(string.Empty);
            }
            EditorGUILayout.EndHorizontal();


            if (typeNames != null)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var flag = EnumFlagsField(flagAsset, optDropWidth);
                    if (flag != flagAsset)
                        LoadComponents(flag);

                    var index = EditorGUILayout.Popup(lastTypeIndex, typeNames);
                    if (index != lastTypeIndex)
                        SelectType(index);

                    if (Button(conDefault, styMenuBtn))
                        LoadComponents(AssetType.Base);

                    if (Button(isRootOnly ? "Root" : "Child", styRootBtn))
                    {
                        isRootOnly = !isRootOnly;
                        SetVisible();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        void ViewPrefabList()
        {
            if (0 < assets.Count)
            {
                var sizeBtn = styAssetBtn.fixedHeight + styAssetBtn.margin.vertical - 2f;

                int index = 0;
                int cntAll = assets.Count;

                // Head
                {
                    var cntHead = Math.Max((int)(posScroll.y / sizeBtn) - 1, 0);
                    int noHead = 0;

                    for (; noHead < cntHead && index < cntAll; ++index)
                    {
                        if (!assets[index].invisible)
                            ++noHead;
                    }
                    if (0 < noHead)
                        GUILayout.Space(noHead * sizeBtn);
                }

                // Body
                {
                    var sizeBase = styAssetBtn.fontSize;
                    var sizeName = sizeBase + 4;
                    var pathCur = GetPrefabStagePath();

                    var cntBody = 2 + (int)Math.Ceiling(position.height / sizeBtn);
                    int noBody = 0;

                    for (; noBody < cntBody && index < cntAll; ++index)
                    {
                        var asset = assets[index];
                        if (asset.invisible)
                            continue;

                        ++noBody;
                        var strName = string.IsNullOrEmpty(asset.Folder)
                                    ? $"<size={sizeName}><color=#FF9999><b>{asset.Name}</b></Color></size>"
                                    : $"<size={sizeName}><color=#FF9999><b>{asset.Name}</b></Color></size>{NL}({asset.Folder})";

                        EditorGUILayout.BeginHorizontal();
                        {
                            if (Button(conSelect, styAssetBtn, optSelectWidth))
                                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(asset.GetAssetPath());

                            if (string.CompareOrdinal(pathCur, asset.Path) == 0)
                            {
                                using (new BackgroundColorScope(Color.cyan))
                                {
                                    if (Button(strName, styAssetBtn, optSymbolMinWidth))
                                        OpenAsset(asset.Path);
                                }
                            }
                            else
                            {
                                if (Button(strName, styAssetBtn, optSymbolMinWidth))
                                    OpenAsset(asset.Path);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                // Tail
                {
                    int noTail = 0;

                    for (; index < cntAll; ++index)
                    {
                        if (!assets[index].invisible)
                            ++noTail;
                    }
                    if (0 < noTail)
                        GUILayout.Space(noTail * sizeBtn);
                }
            }
        }
        #endregion Draw menu


        void SetPathAndUpdate(string path)
        {
            EditorPrefs.SetString(KEY_PATH, path);
            lastPath = path;
            paths[0] = string.IsNullOrEmpty(lastPath)
                     ? PathRoot
                     : Path.Combine(PathRoot, lastPath);

            LoadAssetList();
        }
        void SetFindAndUpdate(string find)
        {
            lastFind = find;
            SetVisible();
        }

        void SortJob()
        {
            if (isNameSort)
            {
                if (isDescending)
                    assets.Sort((a, b) => -string.CompareOrdinal(a.Name, b.Name));
                else
                    assets.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            }
            else
            {
                if (isDescending)
                    assets.Sort((a, b) => -string.CompareOrdinal(a.Path, b.Path));
                else
                    assets.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));
            }
        }

        void SetVisible()
        {
            var isFree = string.IsNullOrEmpty(lastFind);
            if (isFree)
            {
                Parallel.For(0, assets.Count, index =>
                {
                    var asset = assets[index];
                    asset.invisible = false;
                    assets[index] = asset;
                });
            }
            else
            {
                Parallel.For(0, assets.Count, index =>
                {
                    var asset = assets[index];
                    switch (typeSearch)
                    {
                    case SearchType.Prefix: asset.invisible = !IsPrefix(asset.Name, lastFind); break;
                    case SearchType.Suffix: asset.invisible = !IsSuffix(asset.Name, asset.Name.Length, lastFind); break;
                    default: asset.invisible = !Contains(asset.Name, 0, asset.Name.Length, lastFind); break;
                    }

                    assets[index] = asset;
                });
            }


            if (flagAsset != AssetType.Component)
            {
                if (lastTypeIndex < 0 || typeComponents.Count <= lastTypeIndex)
                {
                    if (!isFree)
                    {
                        Parallel.For(0, assets.Count, index =>
                        {
                            var asset = assets[index];
                            asset.invisible = true;
                            assets[index] = asset;
                        });
                    }
                }
                else if (0 < lastTypeIndex)
                {
                    var type = typeComponents[lastTypeIndex];
                    for (int index = 0; index < assets.Count; ++index)
                    {
                        var asset = assets[index];
                        if (asset.invisible)
                            continue;

                        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(asset.Path);
                        if (obj == null) continue;
                        if (isRootOnly
                          ? obj.TryGetComponent(type, out _)
                          : obj.GetComponentInChildren(type) != null)
                            continue;

                        asset.invisible = true;
                        assets[index] = asset;
                    }
                }
            }
        }

        void LoadAssetList()
        {
            assets.Clear();
            var fileGUIDs = AssetDatabase.FindAssets("t:Prefab", paths);
            SetOptimalCapacity(assets, fileGUIDs.Length, true);

            for (int index = 0; index < fileGUIDs.Length; ++index)
            {
                var path = AssetDatabase.GUIDToAssetPath(fileGUIDs[index]);
                if (!string.IsNullOrEmpty(path))
                    assets.Add(new PathData() { Path = path, });
            }

            Parallel.For(0, assets.Count, index =>
            {
                var asset = assets[index];
                var offset = asset.Path.LastIndexOf('/');
                asset.Name = Path.GetFileNameWithoutExtension(asset.Path);

                var folder = Path.GetDirectoryName(asset.Path);
                if (6 < folder.Length)      folder = folder.Remove(0, 7);
                else if (5 < folder.Length) folder = folder.Remove(0, 6);
                asset.Folder = folder;

                assets[index] = asset;
            });


            SortJob();
            SetVisible();
        }

        void LoadComponents(AssetType flag)
        {
            flagAsset = flag;
            typeComponents.Clear();
            typeComponents.Add(TypeComp);

            var cntTotal = 0;
            if (flag != AssetType.Component)
            {
                if (flag.HasFlag(AssetType.Engine))
                {
                    var types = Assembly.GetAssembly(TypeComp).GetTypes();
                    SetOptimalCapacity(typeComponents, cntTotal += types.Length, true);

                    Parallel.For(0, types.Length, index =>
                    {
                        var type = types[index];
                        if (type.IsSubclassOf(TypeComp))
                            typeComponents.Add(type);
                    });
                }

                if (flag.HasFlag(AssetType.Assets))
                    _FindTypeComponent(PathRoot);
                else if (flag.HasFlag(AssetType.Base))
                    _FindTypeComponent(Path.Combine(PathRoot, PathBase));

                if (flag.HasFlag(AssetType.Packages))
                    _FindTypeComponent(PathPackages);
            }

            if (typeNames == null || typeNames.Length != typeComponents.Count)
                typeNames = new string[typeComponents.Count];
            Parallel.For(0, typeComponents.Count, index => typeNames[index] = typeComponents[index].Name);

            SelectType(0);


            void _FindTypeComponent(string _folder)
            {
                if (string.IsNullOrEmpty(_folder))
                    return;

                temps[0] = _folder;
                var _fileGUIDs = AssetDatabase.FindAssets("t:Script", temps);
                SetOptimalCapacity(typeComponents, cntTotal += _fileGUIDs.Length, true);

                for (int index = 0; index < _fileGUIDs.Length; ++index)
                {
                    var _path = AssetDatabase.GUIDToAssetPath(_fileGUIDs[index]);
                    if (string.IsNullOrEmpty(_path))
                        continue;

                    var _mono = AssetDatabase.LoadAssetAtPath<MonoScript>(_path);
                    if (_mono == null)
                        continue;

                    var _type = _mono.GetClass();
                    if (_type == null)
                        continue;

                    if (_type.IsSubclassOf(TypeComp))
                        typeComponents.Add(_type);
                };
            }
        }

        void SelectType(int index)
        {
            lastTypeIndex = index;
            SetVisible();
        }

        void OpenAsset(string path)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogAssertion($"{nameof(OpenAsset)} : It is not supported when playing");
                return;
            }

            var typeUtil = Type.GetType("UnityEditor.SceneManagement.PrefabStageUtility, UnityEditor.CoreModule");
            if (typeUtil == null)
                typeUtil = Type.GetType("UnityEditor.Experimental.SceneManagement.PrefabStageUtility, UnityEditor.CoreModule");
            if (typeUtil == null)
                return;

            var methods = typeUtil.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (methods != null)
            {
                for (int index = 0; index < methods.Length; ++index)
                {
                    var method = methods[index];
                    if (string.CompareOrdinal(method.Name, "OpenPrefab") != 0) continue;

                    var args = method.GetParameters();
                    if (args.Length != 1 || args[0].ParameterType != typeof(string)) continue;

                    method?.Invoke(null, new object[] { path, });
                    break;
                }
            }
        }

        string GetPrefabStagePath()
        {
            var typeUtil = Type.GetType("UnityEditor.SceneManagement.PrefabStageUtility, UnityEditor.CoreModule");
            if (typeUtil == null)
                typeUtil = Type.GetType("UnityEditor.Experimental.SceneManagement.PrefabStageUtility, UnityEditor.CoreModule");
            if (typeUtil == null)
                return string.Empty;

            var methods = typeUtil.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (methods == null)
                return string.Empty;


            for (int mindex = 0; mindex < methods.Length; ++mindex)
            {
                var method = methods[mindex];
                if (string.CompareOrdinal(method.Name, "GetCurrentPrefabStage") != 0) continue;

                var args = method.GetParameters();
                if (args.Length != 0) continue;

                var stage = method?.Invoke(null, null);
                if (stage == null) continue;

                var typeStage = stage.GetType();
                if (typeStage == null) continue;

                var properties = typeStage.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (properties == null) continue;

                for (int pindex = 0; pindex < properties.Length; ++pindex)
                {
                    var property = properties[pindex];

                    if (string.CompareOrdinal(property.Name, "assetPath") == 0
                     && property.GetValue(stage) is string path)
                        return path;
                }
            }

            return string.Empty;
        }
    }
}
