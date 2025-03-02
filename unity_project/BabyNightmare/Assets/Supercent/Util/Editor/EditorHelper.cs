using System.Reflection;
using UnityEditor;

namespace Supercent.Util.Editor
{
    public static class EditorHelper
    {
        public static void ClearConsole()
        {
            var asm = Assembly.GetAssembly(typeof(SceneView));
            var type = asm.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method?.Invoke(new object(), null);
        }
    }
}
