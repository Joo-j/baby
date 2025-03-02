using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Supercent.Util.Editor
{
    public class SceneSelector : EditorWindowBase
    {
        const string PathRoot = "Assets";
        const string PathBase = "Supercent";

        static readonly string KEY_PATH = $"{nameof(SceneSelector)}.{nameof(lastPath)}";

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

            public string GetAssetPath() => System.IO.Path.Combine("Assets", Folder, $"{Name}.unity");
        }

        readonly List<PathData> assets = new List<PathData>();
        readonly string[] paths = { string.Empty, };
        bool includeInBuild = false;
        bool isNameSort = false;
        bool isDescending = false;
        string lastPath = PathBase;
        string lastFind = string.Empty;
        SearchType typeSearch = SearchType.Free;
        Vector2 posScroll = Vector2.zero;

        GUIContent conOpen = new GUIContent("O", "Open");
        GUIContent conDefault = new GUIContent("D", "Default");
        GUIContent conClear = new GUIContent("C", "Clear");
        GUIContent conName = new GUIContent("N", "Name");
        GUIContent conPath = new GUIContent("P", "Path");
        GUIContent conAsc = new GUIContent("↓", "Ascending");
        GUIContent conDesc = new GUIContent("↑", "Descending");
        GUIContent conAdd = new GUIContent("A", "Add");
        GUIContent conSelect = new GUIContent("S", "Select");

        GUIStyle styPathText = null;
        GUIStyle styMenuBtn = null;
        GUIStyle stySubGrp = null;
        GUIStyle stySubBtn = null;
        GUIStyle styAssetBtn = null;
        GUILayoutOption optLabelWidth = GUILayout.Width(55);
        GUILayoutOption optDropWidth = GUILayout.Width(55);
        GUILayoutOption optSymbolMinWidth = GUILayout.MinWidth(0);



        [MenuItem("Supercent/Util/Scene Selector &S")]
        public static void OpenWindow()
        {
            GetWindow<SceneSelector>("Scene Selector");
        }

        void OnEnable()
        {
            styPathText = null;
            styMenuBtn = null;
            stySubGrp = null;
            stySubBtn = null;
            styAssetBtn = null;
            SetPathAndUpdate(EditorPrefs.GetString(KEY_PATH, lastPath));

            guiDraw = GUIDraw;
        }
        void OnFocus()
        {
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

                    ViewSceneList();

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
                var newValue = includeInBuild
                             ? EditorGUILayout.ToggleLeft("Include In Build", includeInBuild)
                             : EditorGUILayout.ToggleLeft("Path", includeInBuild, optLabelWidth);
                if (includeInBuild != newValue)
                {
                    includeInBuild = newValue;
                    LoadAssetList();
                }

                if (!includeInBuild)
                {
                    var editPath = EditorGUILayout.TextField(lastPath, styPathText);
                    if (string.CompareOrdinal(editPath, lastPath) != 0)
                        SetPathAndUpdate(editPath);

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
        }

        void ViewSceneList()
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
                    var sceneCur = EditorSceneManager.GetActiveScene();

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
                            EditorGUILayout.BeginVertical(stySubGrp);
                            {
                                if (Button(conAdd, stySubBtn))
                                    AddScene(asset);
                                if (Button(conSelect, stySubBtn))
                                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(asset.GetAssetPath());
                            }
                            EditorGUILayout.EndVertical();

                            if (string.CompareOrdinal(sceneCur.path, asset.Path) == 0)
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
        #endregion// Draw menu


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
            if (string.IsNullOrEmpty(lastFind))
            {
                Parallel.For(0, assets.Count, index =>
                {
                    var asset = assets[index];
                    asset.invisible = false;
                    assets[index] = asset;
                });
            }
            else if (typeSearch == SearchType.Prefix)
            {
                Parallel.For(0, assets.Count, index =>
                {
                    var asset = assets[index];
                    asset.invisible = !IsPrefix(asset.Name, lastFind);
                    assets[index] = asset;
                });
            }
            else if (typeSearch == SearchType.Suffix)
            {
                Parallel.For(0, assets.Count, index =>
                {
                    var asset = assets[index];
                    asset.invisible = !IsSuffix(asset.Name, asset.Name.Length, lastFind);
                    assets[index] = asset;
                });
            }
            else
            {
                Parallel.For(0, assets.Count, index =>
                {
                    var asset = assets[index];
                    asset.invisible = !Contains(asset.Name, 0, asset.Name.Length, lastFind);
                    assets[index] = asset;
                });
            }
        }

        void LoadAssetList()
        {
            assets.Clear();
            if (includeInBuild)
            {
                var scenes = EditorBuildSettings.scenes;
                for (int index = 0; index < scenes.Length; ++index)
                {
                    var scene = scenes[index];
                    if (scene == null
                     || string.IsNullOrEmpty(scene.path))
                        continue;

                    assets.Add(new PathData() { Path = scene.path, });
                }
            }
            else
            {
                var fileGUIDs = AssetDatabase.FindAssets("t:Scene", paths);
                SetOptimalCapacity(assets, fileGUIDs.Length, true);

                for (int index = 0; index < fileGUIDs.Length; ++index)
                {
                    var path = AssetDatabase.GUIDToAssetPath(fileGUIDs[index]);
                    if (!string.IsNullOrEmpty(path))
                        assets.Add(new PathData() { Path = path, });
                }
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

        void OpenAsset(string path)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogAssertion($"{nameof(OpenAsset)} : It is not supported when playing");
                return;
            }

            bool sceneSaveCheckBool = true;
            if (EditorSceneManager.GetActiveScene().isDirty)
                sceneSaveCheckBool = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            if (sceneSaveCheckBool)
                EditorSceneManager.OpenScene(path);
        }

        void AddScene(PathData asset)
        {
            var scenes = EditorBuildSettings.scenes;
            var path = asset.GetAssetPath();
            if (IsNullOrEmpty(scenes))
                scenes = new EditorBuildSettingsScene[] { new EditorBuildSettingsScene(path, true) };
            else
            {
                var fullPathAdd = Path.GetFullPath(path);
                for (int no = 0; no < scenes.Length; ++no)
                {
                    var scene = scenes[no];
                    if (scene == null)
                        continue;

                    var fullPath = Path.GetFullPath(scene.path);
                    if (0 == string.CompareOrdinal(fullPath, fullPathAdd))
                        return;
                }

                Array.Resize(ref scenes, scenes.Length + 1);
                scenes[scenes.Length - 1] = new EditorBuildSettingsScene(path, true);
            }

            EditorBuildSettings.scenes = scenes;
            AssetDatabase.SaveAssets();
        }
    }
}
