using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Supercent.Util.Editor
{
    [CustomEditor(typeof(SpriteAnimator))]
    public class SpriteAnimatorEditor : SpriteImportEditor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}