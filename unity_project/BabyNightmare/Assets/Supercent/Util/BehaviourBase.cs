using System.Reflection;
using UnityEngine;

namespace Supercent.Util
{
    public class BehaviourBase : MonoBehaviour
    {
#if UNITY_EDITOR
        [ContextMenu("Bind Serialized Field")]
        protected void BindSerializedField()
        {
            UnityEditor.Undo.RecordObject(this, "Bind Serialized Field");
            OnBindSerializedField();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        protected virtual void OnBindSerializedField() { }
        public virtual void OnInspectorGUI() { }
        public virtual void OnSceneGUI() { }
#endif // UNITY_EDITOR
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(BehaviourBase), true)]
    [UnityEditor.CanEditMultipleObjects]
    [ExecuteInEditMode]
    public class BehaviourBaseEditor : UnityEditor.Editor
    {
        public static readonly MethodInfo BIND_SERIALIZED_FIELD = typeof(BehaviourBase).GetMethod("BindSerializedField", BindingFlags.NonPublic | BindingFlags.Instance);



        public override void OnInspectorGUI()
        {
            var targetList = serializedObject.targetObjects;
            if (null == targetList)
                return;

            if (GUILayout.Button("Bind Serialized Field"))
            {
                for (int n = 0, cnt = targetList.Length; n < cnt; ++n)
                {
                    var target = targetList[n] as BehaviourBase;
                    if (null == target)
                        continue;

                    BIND_SERIALIZED_FIELD.Invoke(target, null);
                }
            }

            CustomInspectorGUI();

            for (int n = 0, cnt = targetList.Length; n < cnt; ++n)
            {
                var target = targetList[n] as BehaviourBase;
                if (null == target)
                    continue;

                target.OnInspectorGUI();
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 유니티에서 기본적으로 제공되는 Inspector GUI부분을 커스텀하는 메서드
        /// </summary>
        protected virtual void CustomInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        void OnSceneGUI() 
        {
            if (null == target)
                return;

            var bb = target as BehaviourBase;
            bb.OnSceneGUI();
        }
    }
#endif // UNITY_EDITOR
}