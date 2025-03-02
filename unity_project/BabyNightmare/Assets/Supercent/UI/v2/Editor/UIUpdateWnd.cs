using UnityEngine;
using UnityEditor;

namespace Supercent.UIv2.EDT
{
    public class UIUpdateWnd : EditorWindow
    {
        private enum EState
        {
            None,
            Idle,
            
            UpdateBegan,
            Updating,

            CompileBegan,
            CompileUpdate,

            AssignBegan,
            AssignUpdate,

            Finish,
        }

        //------------------------------------------------------------------------------
        // static functions
        //------------------------------------------------------------------------------
        public static void UpdateUI(UIBase ui)
        {
            var wnd = EditorWindow.GetWindow<UIUpdateWnd>(true, "UI Generator v2");

            // params
            wnd.minSize = new Vector2(500f, 250f);
            wnd.maxSize = new Vector2(500f, 250f);
            wnd._target = ui;

            var script = MonoScript.FromMonoBehaviour(ui);
            var uiType = ui.GetType();

            wnd._baseClassName = uiType.BaseType.Name;
            wnd._namespace     = FindNamespace(script);

            wnd._filePath = Application.dataPath.Replace("Assets", string.Empty) + AssetDatabase.GetAssetPath(script);
            wnd._filePath = wnd._filePath.Replace(uiType.Name + ".cs", string.Empty);

            // gui style
            wnd._scFileStyle = new GUIStyle(EditorStyles.boldLabel);
            wnd._scFileStyle.fontSize = 18;
            wnd._scFileStyle.normal.textColor = new Color(0.35f, 0.65f, 1f, 1f);

            wnd._scProgMsg = new GUIStyle(EditorStyles.label);
            wnd._scProgMsg.fontSize = 15;
            wnd._scProgMsg.normal.textColor = Color.green;

            wnd._updateCount = 0;
            wnd._state = EState.Idle;
        }

        private static string FindNamespace(MonoScript script)
        {
            if (null == script)
                return string.Empty;

            var codeLines = script.text.Split('\n');
            if (null == codeLines || 0 == codeLines.Length)
                return string.Empty;

            for (int i = 0, size = codeLines.Length; i < size; ++i)
            {
                if (codeLines[i].IndexOf("namespace ") == 0)
                    return codeLines[i].Substring(10, codeLines[i].Length - 10);
            }

            return string.Empty;
        }

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private EState _state = EState.None;

        private UIBase _target = null;
        private string _baseClassName = string.Empty;
        private string _namespace = string.Empty;
        private string _filePath  = string.Empty;

        private int    _updateCount  = 0;

        private GUIStyle   _scFileStyle  = null;
        private GUIStyle   _scProgMsg    = null;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void OnGUI()
        {
            EditorGUILayout.LabelField($" {_baseClassName}.cs", _scFileStyle);
            EditorGUILayout.Space();

            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.LabelField(_namespace + "." + _baseClassName);
                EditorGUILayout.LabelField(" ", GUILayout.Height(5f));
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                switch (_state)
                {
                case EState.None: break;
                case EState.Idle:           _OnGUI_Idle();          break;
                case EState.UpdateBegan:    _OnGUI_UpdateBegan();   break;
                case EState.Updating:       _OnGUI_Updating();      break;
                case EState.CompileBegan:   _OnGUI_CompileBegan();  break;
                case EState.CompileUpdate:  _OnGUI_CompileUpdate(); break;
                case EState.AssignBegan:    _OnGUI_AssignBegan();   break;
                case EState.AssignUpdate:   _OnGUI_AssignUpdate();  break;
                case EState.Finish:         _OnGUI_Finish();        break;
                }

            }
            --EditorGUI.indentLevel;

            Repaint();
        }

        private void _OnGUI_Idle()
        {
            ++_updateCount;
            if (3 < _updateCount)
            {
                _updateCount = 0;
                _state = EState.UpdateBegan;
            }
        }

        private void _OnGUI_UpdateBegan()
        {
            EditorGUILayout.LabelField("업데이트 중", _scProgMsg);

            ++_updateCount;
            if (2 < _updateCount)
                _state = EState.Updating;
        }

        private void _OnGUI_Updating()
        {
            EditorGUILayout.LabelField("업데이트 중", _scProgMsg);

            UIGenerator.Generate(_target.gameObject, _namespace, _filePath, true);

            _updateCount = 0;
            _state = EState.CompileBegan;
        }

        private void _OnGUI_CompileBegan()
        {
            EditorGUILayout.LabelField("컴파일 중", _scProgMsg);

            ++_updateCount;
            if (2 < _updateCount)
                _state = EState.CompileUpdate;
        }

        private void _OnGUI_CompileUpdate()
        {
            EditorGUILayout.LabelField("컴파일 중", _scProgMsg);

            AssetDatabase.Refresh();
            
            _updateCount = 0;
            _state = EState.Finish;
        }

        private void _OnGUI_AssignBegan()
        {
            EditorGUILayout.LabelField("오브젝트 연결 중", _scProgMsg);

            ++_updateCount;
            if (2 < _updateCount 
                && !EditorApplication.isCompiling
                && !EditorApplication.isUpdating)
                _state = EState.AssignUpdate;
        }

        private void _OnGUI_AssignUpdate()
        {
            EditorGUILayout.LabelField("오브젝트 연결 중", _scProgMsg);

            _target.SendMessage("EDITOR_AssignObjects");

            _updateCount = 0;
            _state = EState.Finish;
        }

        private void _OnGUI_Finish()
        {
            EditorGUILayout.LabelField("업데이트 완료", _scProgMsg);

            ++_updateCount;
            if (5 < _updateCount 
                && !EditorApplication.isCompiling 
                && !EditorApplication.isUpdating)
            {
                _state = EState.CompileUpdate;
                _target.SendMessage("EDITOR_AssignObjects");
                this.Close();
            }
        }
    }
}