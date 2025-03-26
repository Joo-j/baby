using System;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class ComponentRemover : MonoBehaviour
{
    [Tooltip("제거할 컴포넌트 타입 (예: BoxCollider, MeshRenderer 등)")]
    public string componentTypeName;

#if UNITY_EDITOR
    [ContextMenu("Remove Specified Components")]
    public void RemoveSpecifiedComponents()
    {
        if (string.IsNullOrEmpty(componentTypeName))
        {
            Debug.LogError("componentTypeName이 비어 있습니다.");
            return;
        }

        Type targetType = GetTypeByName(componentTypeName);
        if (targetType == null || !typeof(Component).IsAssignableFrom(targetType))
        {
            Debug.LogError($"'{componentTypeName}' 은(는) 유효한 컴포넌트 타입이 아닙니다.");
            return;
        }

        Component[] targets = GetComponentsInChildren(targetType, includeInactive: true);
        int removedCount = 0;

        foreach (var comp in targets)
        {
            if (comp.gameObject == this.gameObject) continue;

            UnityEditor.Undo.DestroyObjectImmediate(comp);
            removedCount++;
        }

        Debug.Log($"{removedCount}개의 '{componentTypeName}' 컴포넌트가 제거되었습니다.");
    }

    private Type GetTypeByName(string name)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = asm.GetType(name);
            if (type != null)
                return type;
        }

        // 유니티 컴포넌트 짧은 이름 대응 (예: "BoxCollider" → "UnityEngine.BoxCollider")
        return Type.GetType("UnityEngine." + name + ", UnityEngine");
    }
#endif
}
