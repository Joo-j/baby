using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

namespace Supercent.UIv2
{
    public class SimpleOpenURLBtn : MonoBehaviour
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private string _url;
        [SerializeField] private Button _button;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void OnClickButton_OpenUrlEvent(Button button)
        {
            if (_button != button || string.IsNullOrEmpty(_url))
                return;

            Application.OpenURL(_url);
        }   

#if UNITY_EDITOR
        public void EDT_ONLY_TryAssignButton()
        {
            var button = GetComponent<Button>();
            if (null == button)
            {
                Debug.LogError("[UIv2.SimpleClickAnimation] 버튼을 찾을 수 없습니다.");
                return;
            }

            var eventName  = nameof(OnClickButton_OpenUrlEvent);
            var addedEvent = false;

            for (int i = 0, size = button.onClick.GetPersistentEventCount(); i < size; ++i)
            {
                if (button.onClick.GetPersistentMethodName(i) == eventName)
                {
                    addedEvent = true;
                    break;
                }
            }

            if (false == addedEvent)
                UnityEventTools.AddObjectPersistentListener(button.onClick, OnClickButton_OpenUrlEvent, button);

            _button = button;

            EditorUtility.SetDirty(gameObject);
        }
#endif        

        //------------------------------------------------------------------------------
        // editor
        //------------------------------------------------------------------------------
#if UNITY_EDITOR
        [CustomEditor(typeof(SimpleOpenURLBtn), true)]
        private class _EDITOR_SimpleOpenURLBtn : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                if (true == GUILayout.Button("버튼 연결하기", GUILayout.Height(25f)))
                {
                    var temp = target as SimpleOpenURLBtn;
                    if (null != temp)
                        temp.EDT_ONLY_TryAssignButton();
                }
            }
        }
#endif
    }
}