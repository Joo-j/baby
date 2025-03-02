using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Supercent.Util.SimpleTF
{
    public class SimpleTransformer : MonoBehaviour
    {
        [System.Serializable] public class CurveInfo
        {
            public string Key;
            public AnimationCurve Curve;
        }

        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private Transform _target;
        [SerializeField] private List<SimpleTransformInfo> _infos;
        [SerializeField] private List<CurveInfo> _curves;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private Coroutine _coChangePosition  = null;
        private Coroutine _coChangeRotataion = null;
        private Coroutine _coChangeScale     = null;

        //------------------------------------------------------------------------------
        // Change Position
        //------------------------------------------------------------------------------
        public void ChangePositionImmediate(string key)
        {
            if (null == _target)
            {
                Debug.LogError("[SimpleTF.SimpleTransformer.ChangePositionImmediate] 타겟이 지정되지 않았습니다.");
                return;
            }

            var info = GetInfo(key);
            if (null == info)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangePositionImmediate] 해당 키의 정보를 찾을 수 없습니다. key: {key}");
                return;
            }

            if (null != _coChangePosition)
            {
                StopCoroutine(_coChangePosition);
                _coChangePosition = null;
            }

            _target.position = info.transform.position;
        }

        public void ChangePosition(string infoKey, string curveKey, float delay, float duration)
        {
            if (null == _target)
            {
                Debug.LogError("[SimpleTF.SimpleTransformer.ChangePosition] 타겟이 지정되지 않았습니다.");
                return;
            }

            var info = GetInfo(infoKey);
            if (null == info)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangePosition] 해당 키의 정보를 찾을 수 없습니다. key: {infoKey}");
                return;
            }

            var curve = GetCurve(curveKey);
            if (null == curve)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangePosition] 해당 키의 커브를 찾을 수 없습니다. key: {curveKey}");
                return;
            }

            if (null != _coChangePosition)
                StopCoroutine(_coChangePosition);
            
            _coChangePosition = StartCoroutine(Co_Change(_target.position,
                                                         info.transform.position,
                                                         curve.Curve,
                                                         delay,
                                                         duration,
                                                         (b, e, t) => Vector3.Lerp(b, e, t),
                                                         (v) => _target.transform.position = v,
                                                         ( ) => _coChangePosition = null));
        }

        //------------------------------------------------------------------------------
        // Change Local Position
        //------------------------------------------------------------------------------
        public void ChangeLocalPositionImmediate(string key)
        {
            if (null == _target)
            {
                Debug.LogError("[SimpleTF.SimpleTransformer.ChangeLocalPositionImmediate] 타겟이 지정되지 않았습니다.");
                return;
            }

            var info = GetInfo(key);
            if (null == info)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeLocalPositionImmediate] 해당 키의 정보를 찾을 수 없습니다. key: {key}");
                return;
            }

            if (null != _coChangePosition)
            {
                StopCoroutine(_coChangePosition);
                _coChangePosition = null;
            }

            _target.localPosition = info.transform.localPosition;
        }

        public void ChangeLocalPosition(string infoKey, string curveKey, float delay, float duration)
        {
            if (null == _target)
            {
                Debug.LogError("[SimpleTF.SimpleTransformer.ChangeLocalPosition] 타겟이 지정되지 않았습니다.");
                return;
            }

            var info = GetInfo(infoKey);
            if (null == info)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeLocalPosition] 해당 키의 정보를 찾을 수 없습니다. key: {infoKey}");
                return;
            }

            var curve = GetCurve(curveKey);
            if (null == curve)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeLocalPosition] 해당 키의 커브를 찾을 수 없습니다. key: {curveKey}");
                return;
            }

            if (null != _coChangePosition)
                StopCoroutine(_coChangePosition);
            
            _coChangePosition = StartCoroutine(Co_Change(_target.localPosition,
                                                         info.transform.localPosition,
                                                         curve.Curve,
                                                         delay,
                                                         duration,
                                                         (b, e, t) => Vector3.Lerp(b, e, t),
                                                         (v) => _target.transform.localPosition = v,
                                                         ( ) => _coChangePosition = null));
        }

        //------------------------------------------------------------------------------
        // Change rotation
        //------------------------------------------------------------------------------
        public void ChangeRotationImmediate(string key)
        {
            if (null == _target)
            {
                Debug.LogError("[SimpleTF.SimpleTransformer.ChangeRotationImmediate] 타겟이 지정되지 않았습니다.");
                return;
            }

            var info = GetInfo(key);
            if (null == info)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeRotationImmediate] 해당 키의 정보를 찾을 수 없습니다. key: {key}");
                return;
            }

            if (null != _coChangeRotataion)
            {
                StopCoroutine(_coChangeRotataion);
                _coChangeRotataion = null;
            }

            _target.rotation = info.transform.rotation;
        }

        public void ChangeRotation(string infoKey, string curveKey, float delay, float duration)
        {
            if (null == _target)
            {
                Debug.LogError("[SimpleTF.SimpleTransformer.ChangeRotation] 타겟이 지정되지 않았습니다.");
                return;
            }

            var info = GetInfo(infoKey);
            if (null == infoKey)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeRotation] 해당 키의 정보를 찾을 수 없습니다. key: {infoKey}");
                return;
            }

            var curve = GetCurve(curveKey);
            if (null == curve)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeRotation] 해당 키의 커브를 찾을 수 없습니다. key: {curveKey}");
                return;
            }

            if (null != _coChangeRotataion)
                StopCoroutine(_coChangeRotataion);

            _coChangeRotataion = StartCoroutine(Co_Change(_target.rotation,
                                                          info.transform.rotation,
                                                          curve.Curve,
                                                          delay,
                                                          duration,
                                                          (b, e, t) => Quaternion.Lerp(b, e, t),
                                                          (q) => _target.rotation = q,
                                                          ( ) => _coChangeRotataion = null));
        }

        //------------------------------------------------------------------------------
        // Change Local Rotation 
        //------------------------------------------------------------------------------
        public void ChangeLocalRotationImmediate(string key)
        {
            if (null == _target)
            {
                Debug.LogError("[SimpleTF.SimpleTransformer.ChangeLocalRotationImmediate] 타겟이 지정되지 않았습니다.");
                return;
            }

            var info = GetInfo(key);
            if (null == info)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeLocalRotationImmediate] 해당 키의 정보를 찾을 수 없습니다. key: {key}");
                return;
            }

            if (null != _coChangeRotataion)
            {
                StopCoroutine(_coChangeRotataion);
                _coChangeRotataion = null;
            }

            _target.localRotation = info.transform.localRotation;
        }

        public void ChangeLocalRotation(string infoKey, string curveKey, float delay, float duration)
        {
            if (null == _target)
            {
                Debug.LogError("[SimpleTF.SimpleTransformer.ChangeLocalRotation] 타겟이 지정되지 않았습니다.");
                return;
            }

            var info = GetInfo(infoKey);

            if (null == infoKey)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeLocalRotation] 해당 키의 정보를 찾을 수 없습니다. key: {infoKey}");
                return;
            }

            var curve = GetCurve(curveKey);
            if (null == curve)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeLocalRotation] 해당 키의 커브를 찾을 수 없습니다. key: {curveKey}");
                return;
            }

            if (null != _coChangeRotataion)
                StopCoroutine(_coChangeRotataion);

            _coChangeRotataion = StartCoroutine(Co_Change(_target.localRotation,
                                                          info.transform.localRotation,
                                                          curve.Curve,
                                                          delay,
                                                          duration,
                                                          (b, e, t) => Quaternion.Lerp(b, e, t),
                                                          (q) => _target.localRotation = q,
                                                          ( ) => _coChangeRotataion = null));
        }

        //------------------------------------------------------------------------------
        // Change Local Scale
        //------------------------------------------------------------------------------
        public void ChangeLocalScaleImmediate(string key)
        {
            if (null == _target)
            {
                Debug.LogError("[SimpleTF.SimpleTransformer.ChangeLocalScaleImmediate] 타겟이 지정되지 않았습니다.");
                return;
            }

            var info = GetInfo(key);
            if (null == info)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeLocalScaleImmediate] 해당 키의 정보를 찾을 수 없습니다. key: {key}");
                return;
            }

            if (null != _coChangeScale)
            {
                StopCoroutine(_coChangeScale);
                _coChangeScale = null;
            }

            _target.localScale = info.transform.localScale;
        }

        public void ChangeLocalScale(string infoKey, string curveKey, float delay, float duration)
        {
            if (null == _target)
            {
                Debug.LogError("[SimpleTF.SimpleTransformer.ChangeLocalScale] 타겟이 지정되지 않았습니다.");
                return;
            }

            var info = GetInfo(infoKey);
            if (null == info)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeLocalScale] 해당 키의 정보를 찾을 수 없습니다. key: {infoKey}");
                return;
            }

            var curve = GetCurve(curveKey);
            if (null == curve)
            {
                Debug.LogError($"[SimpleTF.SimpleTransformer.ChangeLocalScale] 해당 키의 커브를 찾을 수 없습니다. key: {curveKey}");
                return;
            }

            if (null != _coChangeScale)
                StopCoroutine(_coChangeScale);
            
            _coChangeScale = StartCoroutine(Co_Change(_target.localScale,
                                                      info.transform.localScale,
                                                      curve.Curve,
                                                      delay,
                                                      duration,
                                                      (b, e, t) => Vector3.Lerp(b, e, t),
                                                      (v) => _target.transform.localScale = v,
                                                      ( ) => _coChangePosition = null));
        }

        //------------------------------------------------------------------------------
        // inner functions
        //------------------------------------------------------------------------------
        private SimpleTransformInfo GetInfo(string key)
        {
            if (null == _infos)
                return null;

            for (int i = 0, size = _infos.Count; i < size; ++i)
            {
                if (key == _infos[i].Key)
                    return _infos[i];
            }

            return null;
        }

        private CurveInfo GetCurve(string key)
        {
            if (null == _curves)
                return null;

            for (int i = 0, size = _curves.Count; i < size; ++i)
            {
                if (key == _curves[i].Key)
                    return _curves[i];
            }

            return null;
        }

        private IEnumerator Co_Change<T>(T begin, T end, AnimationCurve curve, float delay, float duration, Func<T, T, float, T> onLerp, Action<T> onChanged, Action onFinish)
        {
            if (null == onLerp || null == onChanged)
                yield break;

            if (0f < delay)
            {
                var secDone = Time.timeAsDouble + delay;
                while (Time.timeAsDouble < secDone)
                    yield return null;
            }

            var timer = 0f;
            while (timer < duration)
            {
                onChanged(onLerp(begin, end, curve.Evaluate(timer / duration)));
                yield return null;

                timer += Time.deltaTime;
            }

            onChanged(end);
            onFinish?.Invoke();
        }

#if UNITY_EDITOR
        //------------------------------------------------------------------------------
        // custom editor
        //------------------------------------------------------------------------------
        [CustomEditor(typeof(SimpleTransformer), true)]
        private class EDITOR_SimpleTransformer : Editor
        {
            private Texture2D _EDITOR_BG_TEX2D = null;
            private GUIStyle  _EDITOR_BG_STYLE = null;

            public override void OnInspectorGUI()
            {
                CreateBgStyle();

                var tt = (SimpleTransformer)target;
                var bb = false;

                // target
                CustomEditorExtension.ObjectField<Transform>("Target", ref tt._target, ref bb);

                // curve
                DrawCurveList(tt, ref bb);

                // info
                DrawInfoList(tt, ref bb);

                // save
                if (bb)
                    EditorUtility.SetDirty(tt);
            }

            private void CreateBgStyle()
            {
                if (null != _EDITOR_BG_TEX2D)
                    return;

                var colors = new Color[64];
                var color  = new Color(0.19f, 0.19f, 0.19f, 1f);
                for (int i = 0, size = colors.Length; i < size; ++i)
                    colors[i] = color;

                _EDITOR_BG_TEX2D = new Texture2D(8, 8);
                _EDITOR_BG_TEX2D.SetPixels(colors);
                _EDITOR_BG_TEX2D.Apply();

                _EDITOR_BG_STYLE = GUI.skin.box;
                _EDITOR_BG_STYLE.normal.background = _EDITOR_BG_TEX2D;
            }

            private void DrawCurveList(SimpleTransformer tt, ref bool bb)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("CURVE LIST", EditorStyles.boldLabel);
                
                ++EditorGUI.indentLevel;

                EditorGUILayout.BeginVertical(_EDITOR_BG_STYLE);

                // list
                if (null == tt._curves)
                    tt._curves = new List<CurveInfo>();

                for (int i = 0, size = tt._curves.Count; i < size; ++i)
                {
                    var curveInfo = tt._curves[i];

                    EditorGUILayout.LabelField(curveInfo.Key);

                    ++EditorGUI.indentLevel;

                    CustomEditorExtension.TextField("Key", ref curveInfo.Key, ref bb);
                    CustomEditorExtension.CurveField("Curve", ref curveInfo.Curve, ref bb);
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(" ");
                        if (GUILayout.Button("커브 제거"))
                        {
                            tt._curves.RemoveAt(i);
                            return;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField(" ", GUILayout.Height(5f));

                    --EditorGUI.indentLevel;
                }

                EditorGUILayout.EndVertical();

                // add button
                EditorGUILayout.LabelField(" ", GUILayout.Height(5f));
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(" ", GUILayout.Width(100f));
                    if (GUILayout.Button("커브 추가"))
                    {
                        tt._curves.Add(new CurveInfo());
                        bb = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                --EditorGUI.indentLevel;
            }

            private void DrawInfoList(SimpleTransformer tt, ref bool bb)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("INFO LIST", EditorStyles.boldLabel);

                ++EditorGUI.indentLevel;

                EditorGUILayout.BeginVertical(_EDITOR_BG_STYLE);
                {
                    if (null == tt._infos)
                        tt._infos = new List<SimpleTransformInfo>();

                    for (int i = 0, size = tt._infos.Count; i < size; ++i)
                    {
                        var info = tt._infos[i];

                        EditorGUILayout.LabelField(info.Key);

                        ++EditorGUI.indentLevel;
                        {
                            EditorGUILayout.Vector3Field("Position", info.transform.position);
                            EditorGUILayout.Vector3Field("Rotation", info.transform.eulerAngles);
                            EditorGUILayout.Vector3Field("Scale",  info.transform.localScale);

                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(" ", GUILayout.Width(50f));

                                if (GUILayout.Button("타겟 >> 정보", EditorStyles.miniButtonLeft))
                                {
                                    var targetTf = tt._target.transform;
                                    var infoTf   = info.transform;

                                    infoTf.position   = targetTf.position;
                                    infoTf.rotation   = targetTf.rotation;
                                    infoTf.localScale = targetTf.localScale;            

                                    bb = true;        
                                }

                                if (GUILayout.Button("정보 >> 타겟", EditorStyles.miniButtonRight))
                                {
                                    var targetTf = tt._target.transform;
                                    var infoTf   = info.transform;
                                    
                                    targetTf.position   = infoTf.position;
                                    targetTf.rotation   = infoTf.rotation;
                                    targetTf.localScale = infoTf.localScale;

                                    bb = true;
                                }

                                if (GUILayout.Button("Key 변경", EditorStyles.miniButton))
                                    EDITOR_EditInfoKeyWnd.ShowWnd(tt, info);

                                if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(30f)))
                                {
                                    UnityEngine.Object.DestroyImmediate(info.gameObject);
                                    info = null;

                                    tt._infos.RemoveAt(i);
                                    bb = true;
                                    return;
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        --EditorGUI.indentLevel;

                        EditorGUILayout.LabelField(" ", GUILayout.Height(5f));
                    }
                }
                EditorGUILayout.EndVertical();

                // add button
                EditorGUILayout.LabelField(" ", GUILayout.Height(5f));
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(" ", GUILayout.Width(100f));
                    if (GUILayout.Button("정보 추가"))
                        EDITOR_AddInfoWnd.ShowWnd(tt);
                }
                EditorGUILayout.EndHorizontal();

                --EditorGUI.indentLevel;
            }
        }

        //------------------------------------------------------------------------------
        // add info window
        //------------------------------------------------------------------------------
        private class EDITOR_AddInfoWnd : EditorWindow
        {
            private SimpleTransformer _transformer = null;

            private string _key   = string.Empty;
            private string _error = string.Empty;

            public static void ShowWnd(SimpleTransformer transformer)
            {
                var wnd = EditorWindow.GetWindow<EDITOR_AddInfoWnd>(true, "Add info window");
                if (null == wnd)
                    return;

                wnd.minSize = new Vector2(300f, 300f);
                wnd.maxSize = new Vector2(400f, 400f);

                wnd._transformer = transformer;
                wnd._error       = string.Empty;
            }

            private void OnGUI() 
            {
                if (null == _transformer)
                {
                    _key = string.Empty;
                    this.Close();
                    return;
                }

                _key = EditorGUILayout.TextField("Key", _key);
                EditorGUILayout.Space();

                if (GUILayout.Button("정보 추가"))
                {
                    var hasSameName = false;
                    
                    for (int i = 0, size = _transformer._infos.Count; i < size; ++i)
                    {
                        if (_transformer._infos[i].name == _key)
                        {
                            hasSameName = true;
                            _error = "동일한 키가 존재합니다. 다른 키를 입력하세요.";
                            break;
                        }
                    }

                    if (!hasSameName)
                    {
                        var go = new GameObject(_key);
                        go.transform.SetParent(_transformer.transform);

                        var comp = go.AddComponent<SimpleTransformInfo>();
                        comp.Key = _key;

                        _transformer._infos.Add(comp);

                        EditorUtility.SetDirty(_transformer);

                        _key = string.Empty;
                        this.Close();
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(_error);
            }
        }

        //------------------------------------------------------------------------------
        // edit info key window
        //------------------------------------------------------------------------------
        private class EDITOR_EditInfoKeyWnd : EditorWindow
        {
            private SimpleTransformer _transformer = null;
            private SimpleTransformInfo _info = null;
        
            private string _key   = string.Empty;
            private string _error = string.Empty;

            public static void ShowWnd(SimpleTransformer transformer, SimpleTransformInfo info)
            {
                var wnd = EditorWindow.GetWindow<EDITOR_EditInfoKeyWnd>(true, "Add info window");
                if (null == wnd)
                    return;

                wnd.minSize = new Vector2(300f, 300f);
                wnd.maxSize = new Vector2(400f, 400f);

                wnd._transformer = transformer;
                wnd._info        = info;
                wnd._key         = info.Key;
                wnd._error       = string.Empty;
            }

            private void OnGUI()
            {
                if (null == _transformer)
                {
                    HideWnd();
                    return;
                }

                _key = EditorGUILayout.TextField("Key", _key);
                EditorGUILayout.Space();

                if (GUILayout.Button("키 변경"))
                {
                    if (_key == _info.Key)
                    {
                        HideWnd();
                        return;
                    }

                    var hasSameName = false;
                    
                    for (int i = 0, size = _transformer._infos.Count; i < size; ++i)
                    {
                        if (_transformer._infos[i].Key == _key)
                        {
                            hasSameName = true;
                            _error = "동일한 키가 존재합니다. 다른 키를 입력하세요.";
                            break;
                        }
                    }

                    if (!hasSameName)
                    {
                        _info.Key  = _key;
                        _info.name = _key;

                        EditorUtility.SetDirty(_transformer);

                        HideWnd();
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(_error);
            }

            private void HideWnd()
            {
                _key = string.Empty;
                _error = string.Empty;
                _info = null;
                _transformer = null;
                this.Close();
            }
        }
#endif        
    }
}