using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

namespace Supercent.Util.Editor
{
    public class EditorWindowBase : EditorWindow
    {
        protected const int ArrayLimit = int.MaxValue - 56;

        public const string DIF_EDITOR = "UNITY_EDITOR";
        public const string PATH_SETTING = @"ProjectSettings";
        protected static readonly string NL = Environment.NewLine;

        static string pathProjectSetting = string.Empty;
        protected string PathProjectSetting
        {
            get => string.IsNullOrEmpty(pathProjectSetting)
                 ? pathProjectSetting = Path.Combine(Path.GetDirectoryName(Application.dataPath), PATH_SETTING)
                 : pathProjectSetting;
        }

        readonly List<int> routineKeys = new List<int>();
        readonly Dictionary<int, IEnumerator> routines = new Dictionary<int, IEnumerator>();
        readonly Dictionary<KeyCode, Action> keyDownJobs = new Dictionary<KeyCode, Action>();
        readonly Dictionary<KeyCode, Action> keyUpJobs = new Dictionary<KeyCode, Action>();

        SerializedObject objThis = null;
        protected SerializedObject THIS => objThis != null ? objThis : objThis = new SerializedObject(this);
        protected Action guiDraw = null;



        protected void OnGUI()
        {
            var evt = Event.current;
            switch (evt.type)
            {
            case EventType.KeyDown:
                {
                    if (keyDownJobs.TryGetValue(evt.keyCode, out var job)) job?.Invoke();
                    if (evt.keyCode == KeyCode.Escape)
                    {
                        if (GUIUtility.keyboardControl != 0)
                            GUIUtility.keyboardControl = 0;
                        else
                            Close();
                        return;
                    }
                }
                break;

            case EventType.KeyUp:
                {
                    if (keyUpJobs.TryGetValue(evt.keyCode, out var job)) job?.Invoke();
                }
                break;
            }

            guiDraw?.Invoke();

            // Coroutine jobs
            routineKeys.Clear();
            {
                routineKeys.AddRange(routines.Keys);
                for (int index = 0, cnt = routineKeys.Count; index < cnt; ++index)
                {
                    var key = routineKeys[index];
                    if (routines.TryGetValue(key, out var routine)
                     && !routine.MoveNext())
                        routines.Remove(key);
                }
            }
            routineKeys.Clear();
        }



        protected static bool IsNullOrEmpty<T>(ICollection<T> collection) => null == collection || collection.Count <= 0;

        protected static bool TryGetValue<T>(IList<T> list, int index, out T value)
        {
            value = default;
            {
                if (null == list) return false;
                if (index < 0) return false;
                if (list.Count <= index) return false;

                value = list[index];
            }
            return true;
        }

        protected static int GetOptimalCapacity(int count)
        {
            if (count < 4) return 4;
            if (ArrayLimit <= count) return ArrayLimit;

            int round = 0;
            uint number = (uint)count;
            while (1 < number)
            {
                ++round;
                number = number >> 1;
            }

            number = number << round;
            if (number < count)
                number = number << 1;
            return number < ArrayLimit ? (int)number : ArrayLimit;
        }
        protected static void SetOptimalCapacity<T>(List<T> list, int value, bool isIncreaseOnly)
        {
            var capacity = GetOptimalCapacity(value);
            if (!isIncreaseOnly)
                list.Capacity = capacity;
            else if (list.Capacity < capacity)
                list.Capacity = capacity;
        }

        protected static bool IsAsciiAlphabet(char value)
        {
            return 'Z' < value ? ('a' <= value && value <= 'z')
                 : 'A' <= value;
        }
        protected static bool IsCodableChar(char value, bool isFirst)
        {
            return value == '_' ? true
                 : value < 'A' ? (!isFirst && '0' <= value && value <= '9')
                 : 'Z' < value ? ('a' <= value && value <= 'z')
                 : true;
        }
        protected static bool IsCodableName(string value)
        {
            if (string.IsNullOrEmpty(value)
             || !IsCodableChar(value[0], true))
                return false;

            for (int index = 1; index < value.Length; ++index)
            {
                if (!IsCodableChar(value[index], false))
                    return false;
            }

            return true;
        }

        protected static bool IsPrefix(string source, string prefix, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) => IsPrefix(source, 0, prefix, comparisonType);
        protected static bool IsPrefix(string source, int index, string prefix, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return string.Compare(source, index, prefix, 0, prefix.Length, comparisonType) == 0;
        }
        protected static bool IsSuffix(string source, string suffix, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) => IsSuffix(source, source.Length, suffix, comparisonType);
        protected static bool IsSuffix(string source, int index, string suffix, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return string.Compare(source, Math.Max(index - suffix.Length, 0), suffix, 0, suffix.Length, comparisonType) == 0;
        }
        protected static bool Contains(string source, string word, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) => Contains(source, 0, source.Length, word, comparisonType);
        protected static bool Contains(string source, int index, int length, string word, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) => -1 < source.IndexOf(word, index, length, comparisonType);


        protected static bool IsLoadedScene(GameObject obj)
        {
            return obj == null ? false
                 : obj.scene == null ? false
                 : !obj.scene.isLoaded ? false
                 : string.IsNullOrEmpty(obj.scene.name) ? false
                 : true;
        }

        protected void StartCoroutine(IEnumerator routine)
        {
            if (routine?.MoveNext() ?? false)
                routines[routine.GetHashCode()] = routine;
        }
        protected void StopCoroutine(IEnumerator routine)
        {
            if (routine != null)
                routines.Remove(routine.GetHashCode());
        }


        #region Path
        protected static bool GetPath(Transform target, Transform root, out string path)
        {
            if (target == null) { path = string.Empty; return false; }

            var sb = new StringBuilder(target.name.Length).Insert(0, target.name);
            var parent = target.parent;

            if (root == null)
            {
                while (parent != null)
                {
                    sb.Insert(0, $"{parent.name}/");
                    parent = parent.parent;
                }
                path = sb.ToString();
                return true;
            }
            else
            {
                bool findSameRoot = false;
                while (parent != null)
                {
                    if (parent == root) { findSameRoot = true; break; }

                    sb.Insert(0, $"{parent.name}/");
                    parent = parent.parent;
                }

                path = findSameRoot ? sb.ToString() : string.Empty;
                return findSameRoot;
            }
        }

        protected static string ConvertAssetsPath(string path)
        {
            var pathProj = Application.dataPath;
            return string.CompareOrdinal(path, 0, pathProj, 0, pathProj.Length) != 0 ? string.Empty
                                       : pathProj.Length == path.Length ? string.Empty
                                       : path.Remove(0, pathProj.Length + 1);
        }
        #endregion// Path


        #region Input
        protected void KeyDownSubscribe(KeyCode code, Action callback)
        {
            if (keyDownJobs.TryGetValue(code, out var job))
            {
                keyDownJobs[code] -= callback;
                keyDownJobs[code] += callback;
            }
            else
                keyDownJobs[code] = callback;
        }

        protected void KeyDownUnsubscribe(KeyCode code, Action callback)
        {
            if (keyDownJobs.TryGetValue(code, out var job))
                keyDownJobs[code] -= callback;
        }

        protected void KeyDownUnsubscribeAll() => keyDownJobs.Clear();

        protected void KeyUpSubscribe(KeyCode code, Action callback)
        {
            if (keyUpJobs.TryGetValue(code, out var job))
            {
                keyUpJobs[code] -= callback;
                keyUpJobs[code] += callback;
            }
            else
                keyUpJobs[code] = callback;
        }

        protected void KeyUpUnsubscribe(KeyCode code, Action callback)
        {
            if (keyUpJobs.TryGetValue(code, out var job))
                keyUpJobs[code] -= callback;
        }

        protected void KeyUpUnsubscribe() => keyUpJobs.Clear();
        #endregion// Input


        #region GUI
        protected static float GetContentWidth(GUIStyle src) => src == null ? 0 : src.fixedWidth + src.padding.left + src.padding.right;
        protected static float GetContentHeight(GUIStyle src) => src == null ? 0 : src.fixedHeight + src.padding.top + src.padding.bottom;


        protected static bool Button(string text, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(text, options))
            {
                GUIUtility.keyboardControl = 0;
                return true;
            }
            return false;
        }
        protected static bool Button(GUIContent content, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(content, options))
            {
                GUIUtility.keyboardControl = 0;
                return true;
            }
            return false;
        }
        protected static bool Button(string text, GUIStyle style, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(text, style, options))
            {
                GUIUtility.keyboardControl = 0;
                return true;
            }
            return false;
        }
        protected static bool Button(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(content, style, options))
            {
                GUIUtility.keyboardControl = 0;
                return true;
            }
            return false;
        }

        protected T EnumPopup<T>(T selected, params GUILayoutOption[] options) where T : Enum                       => (T)EditorGUILayout.EnumPopup(selected, options);
        protected T EnumPopup<T>(string label, T selected, params GUILayoutOption[] options) where T : Enum         => (T)EditorGUILayout.EnumPopup(label, selected, options);
        protected T EnumFlagsField<T>(T selected, params GUILayoutOption[] options) where T : Enum                  => (T)EditorGUILayout.EnumFlagsField(selected, options);
        protected T EnumFlagsField<T>(string label, T selected, params GUILayoutOption[] options) where T : Enum    => (T)EditorGUILayout.EnumFlagsField(label, selected, options);

        protected void ToggleEnumPopup<T>(ref bool enabled, ref T selected, string label, float widthUnchecked) where T : Enum
        {
            GUILayout.BeginHorizontal();
            if (!(enabled = SwapToggleJob(enabled, label, widthUnchecked)))
                selected = (T)EditorGUILayout.EnumPopup(selected);
            GUILayout.EndHorizontal();
        }
        protected void ToggleEnumPopupWithNone<T>(ref bool enabled, ref T selected, string label, float widthUnchecked, string noneName, T noneValue) where T : Enum
        {
            GUILayout.BeginHorizontal();
            if (!(enabled = SwapToggleJob(enabled, label, widthUnchecked)))
                selected = EnumPopupWithNone(selected, noneName, noneValue);
            GUILayout.EndHorizontal();
        }
        protected void ToggleIntField(ref bool enabled, ref int value, string label, float widthUnchecked, int min, int max)
        {
            GUILayout.BeginHorizontal();
            if (!(enabled = SwapToggleJob(enabled, label, widthUnchecked)))
                value = Mathf.Clamp(EditorGUILayout.IntField(value), min, max);
            GUILayout.EndHorizontal();
        }
        protected void ToggleFloatField(ref bool enabled, ref float value, string label, float widthUnchecked, float min, float max)
        {
            GUILayout.BeginHorizontal();
            if (!(enabled = SwapToggleJob(enabled, label, widthUnchecked)))
                value = Mathf.Clamp(EditorGUILayout.FloatField(value), min, max);
            GUILayout.EndHorizontal();
        }
        protected void ToggleVector2Field(ref bool enabled, ref Vector2 value, string label, float widthUnchecked)
        {
            GUILayout.BeginHorizontal();
            if (!(enabled = SwapToggleJob(enabled, label, widthUnchecked)))
            {
                value.x = EditorGUILayout.FloatField(value.x);
                value.y = EditorGUILayout.FloatField(value.y);
            }
            GUILayout.EndHorizontal();
        }
        bool SwapToggleJob(bool value, string label, float widthUnchecked)
        {
            return value
                 ? EditorGUILayout.ToggleLeft(label, value)
                 : EditorGUILayout.ToggleLeft(label, value, GUILayout.MaxWidth(widthUnchecked));
        }
        T EnumPopupWithNone<T>(T selected, string noneName, T noneValue) where T : Enum
        {
            var names = Enum.GetNames(typeof(T));
            var popNames = new string[1 + names.Length];
            popNames[0] = noneName;
            for (int index = 0; index < names.Length; ++index)
                popNames[index + 1] = names[index];

            var values = Enum.GetValues(typeof(T));
            var popValues = new T[1 + values.Length];
            popValues[0] = noneValue;
            for (int index = 0; index < values.Length; ++index)
                popValues[index + 1] = (T)values.GetValue(index);

            int curIndex = Array.IndexOf(popValues, selected);
            curIndex = EditorGUILayout.Popup(curIndex, popNames);
            return popValues[curIndex];
        }

        protected void DivisionLine(Color color, float thickness) => DivisionLine(color, 6, thickness);
        protected void DivisionLine(Color color, float height, float thickness)
        {
            GUILayout.Space(height);
            var y = GUILayoutUtility.GetLastRect().yMax;
            EditorGUI.DrawRect(new Rect(0f, y, Screen.width, thickness), color);
            GUILayout.Space(height);
        }

        protected void GetSplitWdith(out GUILayoutOption minWidth, out GUILayoutOption maxWidth, int count, float minOffset = 5f)
        {
            if (count < 1) count = 1;
            var width = EditorGUIUtility.currentViewWidth / count;
            minWidth = GUILayout.MinWidth(width - minOffset);
            maxWidth = GUILayout.MaxWidth(width);
        }
        #endregion// GUI


        #region Dialog
        protected static bool OpenAssetsFolderPanel(string title, string pathFolder, string defaultName, out string pathAssets)
        {
            var pathProj = Application.dataPath;

            if (string.IsNullOrEmpty(pathFolder))
                pathFolder = pathProj;
            else if (0 != string.CompareOrdinal(pathFolder, 0, pathProj, 0, pathProj.Length))
            {
                var path = Path.Combine(pathProj, pathFolder);
                pathFolder = Directory.Exists(path) ? path : pathProj;
            }

            var pathSelect = EditorUtility.OpenFolderPanel(title, pathFolder, defaultName);
            if (string.IsNullOrEmpty(pathSelect))
            {
                pathAssets = string.Empty;
                return false;
            }

            pathAssets = ConvertAssetsPath(pathSelect);
            return true;
        }
        #endregion// Dialog


        #region Format
        protected static bool SaveJson(string path, object obj)
        {
            try
            {
                var json = JsonUtility.ToJson(obj);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                UnityDebug.LogException(e);
                return false;
            }

            return true;
        }
        protected static bool LoadJson<T>(string path, out T result)
        {
            var json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
            {
                UnityDebug.LogAssertion($"{nameof(LoadJson)} : Empty path");
                result = default;
                return false;
            }

            try     { result = JsonUtility.FromJson<T>(json); }
            catch   (Exception e)
            {
                UnityDebug.LogException(e);
                result = default;
                return false;
            }

            return true;
        }
        #endregion// Format
    }

    public class BackgroundColorScope : GUI.Scope
    {
        readonly Color color;



        public BackgroundColorScope(Color color)
        {
            this.color = GUI.backgroundColor;
            GUI.backgroundColor = color;
        }

        protected override void CloseScope() => GUI.backgroundColor = color;
    }
}
