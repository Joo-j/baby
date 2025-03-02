using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Supercent.Util.Editor
{
    [CustomEditor(typeof(SpriteImport))]
    public class SpriteImportEditor : UnityEditor.Editor
    {

        Texture m_Texture;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SpriteImport spriteImport = (SpriteImport)target;
            m_Texture = EditorGUILayout.ObjectField(m_Texture, typeof(Texture), false) as Texture;

            if (spriteImport.m_Names != null && spriteImport.m_Names.Length > 0)
            {
                if (GUILayout.Button("Sort" + spriteImport.GetSortSymbol()))
                {
                    spriteImport.Sort();
                }
            }

            if (m_Texture == null)
                return;

            if (GUILayout.Button("Assign"))
            {
                Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(m_Texture));
                ArrayList sprites = new ArrayList();
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i] is Sprite)
                    {
                        sprites.Add(objs[i] as Sprite);
                    }
                }
                spriteImport.m_Sprites = sprites.ToArray(typeof(Sprite)) as Sprite[];
                spriteImport.m_Names = new string[spriteImport.m_Sprites.Length];
                for (int i = 0; i < spriteImport.m_Names.Length; i++)
                {
                    spriteImport.m_Names[i] = spriteImport.m_Sprites[i].name;
                }
            }

        }
    }
}