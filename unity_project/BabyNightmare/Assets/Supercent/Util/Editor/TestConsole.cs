using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Supercent.Util.Editor
{
    public sealed class TestConsole : EditorWindowBase
    {
        int idMenu = 0;
        int idPrefs = 0;
        string keyPrefs = string.Empty;
        int iPrefs = 0;
        float fPrefs = 0f;
        string sPrefs = string.Empty;
        Vector2 posScroll = Vector2.zero;

        GUILayoutOption optMenuWidth = GUILayout.Width(50);
        GUILayoutOption optLabelWidth_S = GUILayout.Width(40);
        GUILayoutOption optLabelWidth_M = GUILayout.Width(70);
        GUILayoutOption optDimensionWidth = GUILayout.Width(12);



        [MenuItem("Supercent/Util/Test Console &R")]
        public static void OpenWindow()
        {
            var window = GetWindow<TestConsole>(false, "Test Console");
            if (window != null) window.Show();
        }

        void OnEnable()
        {
            guiDraw = GUIDraw;
        }

        void GUIDraw()
        {
            var isPlaying = EditorApplication.isPlaying;

            var curPos = EditorGUILayout.BeginScrollView(posScroll, GUIStyle.none);
            {
                var curEvent = Event.current.type;
                if (curEvent != EventType.Repaint)
                    posScroll = curPos;

                EditorGUILayout.BeginHorizontal();
                {
                    if (idMenu < 0) idMenu = 0;
                    if (DrawButton("Basic", idMenu < 1, optMenuWidth)) idMenu = 0;
                    if (DrawButton("Prefs", idMenu == 1, optMenuWidth)) idMenu = 1;
                }
                EditorGUILayout.EndHorizontal();

                switch (idMenu)
                {
                case 0:
                    EditorGUI.BeginDisabledGroup(!isPlaying);
                    DrawBasic(isPlaying);
                    EditorGUI.EndDisabledGroup();
                    break;
                case 1:
                    DrawPrefs(isPlaying);
                    break;
                }

                if (curEvent == EventType.Repaint)
                    posScroll = curPos;
            }
            EditorGUILayout.EndScrollView();
        }

        bool DrawButton(string text, bool isSelected, params GUILayoutOption[] options)
        {
            if (isSelected)
            {
                using (new BackgroundColorScope(Color.cyan))
                    return Button(text, options);
            }

            return Button(text, options);
        }

        void DrawBasic(bool isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Time", optLabelWidth_S);
                var curValue = Time.timeScale;
                var newValue = EditorGUILayout.Slider(curValue, 0, 100f);
                if (isPlaying && curValue != newValue)
                    Time.timeScale = newValue;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("FPS", optLabelWidth_S);
                //var hz = (int)Math.Round(Screen.currentResolution.refreshRateRatio.value);
                var hz = Screen.currentResolution.refreshRate;
                if (hz < 120)
                    hz = 120;
                var curValue = Application.targetFrameRate;
                var newValue = EditorGUILayout.IntSlider(curValue, -1, hz);
                if (isPlaying && curValue != newValue)
                    Application.targetFrameRate = newValue;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Gravity 3D", optLabelWidth_M);
                var curValue = Physics.gravity;
                var newValue = curValue;
                GUILayout.Label("X", optDimensionWidth);
                newValue.x = EditorGUILayout.FloatField(curValue.x);
                GUILayout.Label("Y", optDimensionWidth);
                newValue.y = EditorGUILayout.FloatField(curValue.y);
                GUILayout.Label("Z", optDimensionWidth);
                newValue.z = EditorGUILayout.FloatField(curValue.z);
                if (isPlaying && curValue != newValue)
                    Physics.gravity = newValue;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Gravity 2D", optLabelWidth_M);
                var curValue = Physics2D.gravity;
                var newValue = curValue;
                GUILayout.Label("X", optDimensionWidth);
                newValue.x = EditorGUILayout.FloatField(curValue.x);
                GUILayout.Label("Y", optDimensionWidth);
                newValue.y = EditorGUILayout.FloatField(curValue.y);
                if (isPlaying && curValue != newValue)
                    Physics2D.gravity = newValue;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                if (Button("Unload & GC"))
                {
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Delete", optLabelWidth_S);
                if (Button("PlayerPrefs"))
                    PlayerPrefs.DeleteAll();
                if (Button("Persistent"))
                    Directory.Delete(Application.persistentDataPath, true);
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawPrefs(bool isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Type", optLabelWidth_S);

                if (idPrefs < 0) idPrefs = 0;
                if (DrawButton("Int", idPrefs < 1, optMenuWidth))
                {
                    idPrefs = 0;
                    _ClearValue();
                }
                if (DrawButton("Float", idPrefs == 1, optMenuWidth))
                {
                    idPrefs = 1;
                    _ClearValue();
                }
                if (DrawButton("String", idPrefs == 2, optMenuWidth))
                {
                    idPrefs = 2;
                    _ClearValue();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                if (string.IsNullOrEmpty(keyPrefs))
                    keyPrefs = string.Empty;

                GUILayout.Label("Key", optLabelWidth_S);
                keyPrefs = GUILayout.TextField(keyPrefs);

                if (Button("Get", optLabelWidth_S))
                {
                    switch (idPrefs)
                    {
                    case 0: iPrefs = PlayerPrefs.GetInt(keyPrefs, 0); break;
                    case 1: fPrefs = PlayerPrefs.GetFloat(keyPrefs, 0f); break;
                    case 2: sPrefs = PlayerPrefs.GetString(keyPrefs, string.Empty); break;
                    }
                }

                if (Button("Delete", optLabelWidth_M))
                {
                    PlayerPrefs.DeleteKey(keyPrefs);
                    PlayerPrefs.Save();

                    keyPrefs = string.Empty;
                    _ClearValue();
                }
            }
            EditorGUILayout.EndHorizontal();

            switch (idPrefs)
            {
            case 0: iPrefs = EditorGUILayout.IntField(iPrefs); break;
            case 1: fPrefs = EditorGUILayout.FloatField(fPrefs); break;
            case 2: sPrefs = EditorGUILayout.TextArea(sPrefs); break;
            }

            if (Button("Set"))
            {
                switch (idPrefs)
                {
                case 0: PlayerPrefs.SetInt(keyPrefs, iPrefs); break;
                case 1: PlayerPrefs.SetFloat(keyPrefs, fPrefs); break;
                case 2: PlayerPrefs.SetString(keyPrefs, sPrefs); break;
                }
                PlayerPrefs.Save();
            }

            if (Button("DeleteAll"))
            {
                PlayerPrefs.DeleteAll();
                keyPrefs = string.Empty;
                _ClearValue();
            }


            void _ClearValue()
            {
                iPrefs = 0;
                fPrefs = 0;
                sPrefs = string.Empty;
            }
        }
    }
}
