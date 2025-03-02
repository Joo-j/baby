using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Supercent.Util.SimpleCC
{
    public class CameraInfo : MonoBehaviour
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private float _fieldOfView = 60f;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public float FieldOfView => _fieldOfView;

        //------------------------------------------------------------------------------
        // custom editor
        //------------------------------------------------------------------------------
#if UNITY_EDITOR
        [CustomEditor(typeof(CameraInfo), true)]
        private class EDITOR_CameraInfo : Editor
        {
            public override void OnInspectorGUI()
            {
                var camTf  = Camera.main.transform;
                var self   = (CameraInfo)target;
                var selfTf = self.transform;

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Field Of View", GUILayout.Width(100f));
                    EditorGUILayout.LabelField(self._fieldOfView.ToString());
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                if (GUILayout.Button("Camera >> This", GUILayout.Height(25f)))
                {
                    UnityEditor.Undo.RecordObject(selfTf, "cic_collect_camera_info");

                    selfTf.position   = camTf.position;
                    selfTf.rotation   = camTf.rotation;
                    self._fieldOfView = Camera.main.fieldOfView;

                    EditorUtility.SetDirty(target);
                }

                if (GUILayout.Button("This >> Camera", GUILayout.Height(25f)))
                {
                    UnityEditor.Undo.RecordObject(selfTf, "cic_camera_to_this");

                    camTf.position = selfTf.position;
                    camTf.rotation = selfTf.rotation;
                    Camera.main.fieldOfView = self._fieldOfView;

                    EditorUtility.SetDirty(target);
                }
            }
        }
#endif
    }
}