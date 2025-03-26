using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ComponentRemover))]
public class ComponentRemoverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ComponentRemover remover = (ComponentRemover)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Remove Specified Components"))
        {
            remover.RemoveSpecifiedComponents();
        }
    }
}
