using UnityEngine;
using UnityEditor;

namespace Supercent.UIv2.EDT
{
    [CustomEditor(typeof(UIBase), true)]
    public class EDITOR_UIBase : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Supercent.UI v2");
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("UI 갱신", EditorStyles.miniButtonLeft))
                {
                    UIUpdateWnd.UpdateUI((UIBase)target);
                    return;
                }

                if (GUILayout.Button("오브젝트 연결", EditorStyles.miniButtonRight))
                {
                    var cc = (UIBase)target;
                    if (null != cc)
                    {
                        if (cc.gameObject.activeInHierarchy)
                            cc.SendMessage("EDITOR_AssignObjects");
                        else
                        {
                            var type = cc.GetType();
                            type.GetMethod
                            (
                                "EDITOR_AssignObjects",
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.InvokeMethod
                            )?.Invoke(cc, null);
                        }
                    }

                    EditorUtility.SetDirty(target);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }
    }
}