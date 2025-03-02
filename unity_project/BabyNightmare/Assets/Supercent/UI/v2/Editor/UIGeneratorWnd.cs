using UnityEngine;
using UnityEditor;
using System.IO;

namespace Supercent.UIv2.EDT
{
    public class UIGeneratorWnd : EditorWindow
    {
        [MenuItem("Supercent/UI_v2/해당 오브젝트에 ui 스크립트 생성")]
        private static void CreateScriptAtObject_FromMenu()
        {
            CreateScriptAtObject();
        }

        [MenuItem("GameObject/Supercent/UI_v2/해당 오브젝트에 ui 스크립트 생성")]
        private static void CreateScriptAtObject_FromHierarchy()
        {
            CreateScriptAtObject();
        }

        private static void CreateScriptAtObject()
        {
            var wnd = GetWindow<UIGeneratorWnd>(true, "UI Generator v2");
            wnd.minSize = new Vector2(500f, 250f);
            wnd.maxSize = new Vector2(500f, 250f);

            wnd._scFileStyle = new GUIStyle(EditorStyles.boldLabel);
            wnd._scFileStyle.fontSize = 18;
            wnd._scFileStyle.normal.textColor = new Color(0.35f, 0.65f, 1f, 1f);

            wnd._scErrorStyle = new GUIStyle(EditorStyles.label);
            wnd._scErrorStyle.fontSize = 15;
            wnd._scErrorStyle.normal.textColor = Color.red;

            wnd._scProgMsg = new GUIStyle(EditorStyles.label);
            wnd._scProgMsg.fontSize = 15;
            wnd._scProgMsg.normal.textColor = Color.green;

            wnd._target       = null;
            wnd._namespace    = PlayerPrefs.GetString("UIv2_GW_NAMESPACE", string.Empty);
            wnd._outputFolder = GetValidatePath(PlayerPrefs.GetString("UIv2_GW_OUTPUTFOLDER"));
            wnd._useStop      = PlayerPrefs.GetInt("UIv2_GW_USESTOP", 1) == 1;
            wnd._state        = EState.Idle;
        }

        private static bool IsInvalidPath(string path)
        {
            return string.IsNullOrEmpty(path)
                || path.IndexOf(Application.dataPath) != 0;
        }

        private static string GetValidatePath(string path)
        {
            var dataPath = Application.dataPath;
            if (string.IsNullOrEmpty(path)
             || path.IndexOf(dataPath) != 0)
                path = dataPath;
            return path;
        }

        //------------------------------------------------------------------------------
        // state
        //------------------------------------------------------------------------------
        private enum EState
        {
            None,

            Idle,

            GenerateBegan,            
            GenerateUpdate,

            CompileBegan,
            CompileUpdate,

            AddComponentBegan,
            AddComponentUpdate,

            AssignBegan,
            AssignUpdate,

            Finish,
        }

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private EState     _state        = EState.None;
        private string     _namespace    = string.Empty;
        private string     _outputFolder = string.Empty;
        private bool       _useStop      = true;
        private int        _updateCount  = 0;
        private GameObject _target       = null;

        private GUIStyle   _scFileStyle  = null;
        private GUIStyle   _scErrorStyle = null;
        private GUIStyle   _scProgMsg    = null;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void OnGUI()
        {
            switch (_state)
            {
            case EState.None: return;
            case EState.Idle:               _OnGUI_Idle();               break;
            case EState.GenerateBegan:      _OnGUI_GenerateBegan();      break;
            case EState.GenerateUpdate:     _OnGUI_GenerateUpdate();     break;
            case EState.CompileBegan:       _OnGUI_CompileBegan();       break;
            case EState.CompileUpdate:      _OnGUI_CompileUpdate();      break;
            case EState.AddComponentBegan:  _OnGUI_AddComponentBegan();  break;
            case EState.AddComponentUpdate: _OnGUI_AddComponentUpdate(); break;
            case EState.AssignBegan:        _OnGUI_AssignBegan();        break;
            case EState.AssignUpdate:       _OnGUI_AssignUpdate();       break;
            case EState.Finish:             _OnGUI_Finish();             break;
            }

            Repaint();
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_Idle()
        {
            var transforms = Selection.GetTransforms(SelectionMode.Unfiltered);
            if (null == transforms || 0 == transforms.Length)
            {
                _OnGUI_Idle_Error(null, "오브젝트를 선택하세요.");
                return;
            }

            if (1 < transforms.Length)
            {
                _OnGUI_Idle_Error(null, "하나의 오브젝트만 선택하세요.");
                return;
            }

            var target = transforms[0];
            if (null != target.GetComponent<UIBase>())
            {
                _OnGUI_Idle_Error(target, "이미 UI 컴포넌트가 존재합니다.");
                return;
            }

            EditorGUILayout.LabelField($" {target.name}.cs", _scFileStyle);
            EditorGUILayout.Space();

            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.LabelField(GetHierarchyPath(target));
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                _namespace = EditorGUILayout.TextField("Namespace", _namespace);
                EditorGUILayout.LabelField(" ", GUILayout.Height(3f));

                EditorGUILayout.BeginHorizontal();
                {
                    _outputFolder = EditorGUILayout.TextField("스크립트 생성 폴더", _outputFolder);

                    if (GUILayout.Button("찾기", GUILayout.Width(60f)))
                    {
                        _outputFolder = EditorUtility.OpenFolderPanel("UI Generator v2", _outputFolder, string.Empty);
                        _outputFolder = GetValidatePath(_outputFolder);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(" ", GUILayout.Height(3f));
                _useStop = EditorGUILayout.Toggle("하위 UIBase 건너뛰기", _useStop);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(" ", GUILayout.Width(10f));
                    if (GUILayout.Button("스크립트 생성", GUILayout.Height(35f)))
                    {
                        // TryCreateScript(target.gameObject);
                        _updateCount = 0;
                        _state       = EState.GenerateBegan;
                        _target      = target.gameObject;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_Idle_Error(Transform targetTf, string error)
        {
            if (null != targetTf)
                EditorGUILayout.LabelField(" " + GetHierarchyPath(targetTf));   

            if (!string.IsNullOrEmpty(error))
                EditorGUILayout.LabelField(" " + error, _scErrorStyle);
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_GenerateBegan()
        {
            _OnGUI_GenerateMessage();

            ++_updateCount;
            if (2 < _updateCount)
                _state = EState.GenerateUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_GenerateMessage()
        {
            EditorGUILayout.LabelField($" {_target.name}.cs", _scFileStyle);
            EditorGUILayout.Space();

            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.LabelField(GetHierarchyPath(_target.transform));
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("코드 생성 중", _scProgMsg);
            }
            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_GenerateUpdate()
        {
            _OnGUI_GenerateMessage();

            TryCreateScript(_target);
            _state = EState.CompileBegan;
            _updateCount = 0;
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_CompileMessage()
        {
            EditorGUILayout.LabelField($" {_target.name}.cs", _scFileStyle);
            EditorGUILayout.Space();

            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.LabelField(GetHierarchyPath(_target.transform));
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("컴파일 중", _scProgMsg);
            }
            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_CompileBegan()
        {
            _OnGUI_CompileMessage();

            ++_updateCount;
            if (2 < _updateCount)
                _state = EState.CompileUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_CompileUpdate()
        {
            _OnGUI_CompileMessage();
            AssetDatabase.Refresh();
            
            _updateCount = 0;
            _state = EState.AddComponentBegan;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_AddComponentMessage()
        {
            EditorGUILayout.LabelField($" {_target.name}.cs", _scFileStyle);
            EditorGUILayout.Space();

            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.LabelField(GetHierarchyPath(_target.transform));
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("컴포넌트 추가 중", _scProgMsg);
            }
            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_AddComponentBegan()
        {
            _OnGUI_AddComponentMessage();

            ++_updateCount;
            if (2 < _updateCount && !EditorApplication.isCompiling)
                _state = EState.AddComponentUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_AddComponentUpdate()
        {
            _OnGUI_AddComponentMessage();

            UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(_target, "", _namespace + "." + _target.name);

            _updateCount = 0;
            _state = EState.AssignBegan;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_AssignMessage()
        {
            EditorGUILayout.LabelField($" {_target.name}.cs", _scFileStyle);
            EditorGUILayout.Space();

            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.LabelField(GetHierarchyPath(_target.transform));
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("오브젝트 연결 중", _scProgMsg);
            }
            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_AssignBegan()
        {
            _OnGUI_AssignMessage();

            ++_updateCount;
            if (2 < _updateCount && !EditorApplication.isCompiling && !EditorApplication.isUpdating)
                _state = EState.AssignUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_AssignUpdate()
        {
            _OnGUI_AssignMessage();
            
            _target.SendMessage("EDITOR_AssignObjects");

            _updateCount = 0;
            _state = EState.Finish;
        }

        /// <summary>
        /// 
        /// </summary>
        private void _OnGUI_Finish()
        {
            EditorGUILayout.LabelField($" {_target.name}.cs", _scFileStyle);
            EditorGUILayout.Space();

            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.LabelField(GetHierarchyPath(_target.transform));
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("스크립트 생성 완료.", _scProgMsg);
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(" ", GUILayout.Width(10f));
                    if (GUILayout.Button("확인", GUILayout.Height(35f)))
                    {
                        this.Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        private void TryCreateScript(GameObject target)
        {
            if (string.IsNullOrEmpty(_namespace))
            {
                EditorUtility.DisplayDialog("알림", "Namespace를 입력하세요.", "확인");
                return;
            }

            if (IsInvalidPath(_outputFolder))
            {
                EditorUtility.DisplayDialog("알림", "유효한 생성 폴더를 입력하세요.", "확인");
                return;
            }

            if (!Directory.Exists(_outputFolder))
                Directory.CreateDirectory(_outputFolder);

            if (_outputFolder[_outputFolder.Length - 1] != '/')
                _outputFolder += "/";

            if (File.Exists(_outputFolder + target.name + ".cs"))
            {
                EditorUtility.DisplayDialog("알림", "스크립트 파일이 이미 존재합니다.", "확인");
                return;
            }

            PlayerPrefs.SetString("UIv2_GW_NAMESPACE", _namespace);
            PlayerPrefs.SetString("UIv2_GW_OUTPUTFOLDER", _outputFolder);
            PlayerPrefs.SetInt("UIv2_GW_USESTOP", _useStop ? 1 : 0);

            UIGenerator.Generate(target, _namespace, _outputFolder, _useStop);
        }

        private string GetHierarchyPath(Transform self)
        {
            var path   = self.name;
            var parent = self.parent;

            while (null != parent)
            {
                path   = parent.name + " / " + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}