#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

namespace Supercent.UI.Editor
{
    public class UIMonoEditorWindow : UnityEditor.EditorWindow
    {
        public string FileName = null;
        public string FilePath = null;
        void Update()
        {
            if (FileName == null || FileName.Length == 0
            || FilePath == null || FilePath.Length == 0)
            {
                this.Close();
                return;
            }
            if (EditorApplication.isCompiling) return;
            if (FileName.Length < 2) return;
            if (Selection.activeTransform == null) return;
            try
            {
                UIMono c = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(Selection.activeTransform.gameObject, FilePath, FileName) as UIMono;
                if (c == null) return;
                c.GetType().GetMethod("AssignObj")?.Invoke(c, null);
                c.AssignButtonEvent();
                c.AssignInputFieldEvent();
            }
            catch
            {

            }
            FileName = null;
            this.Close();
        }
    }
}
#endif