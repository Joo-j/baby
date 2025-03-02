using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Supercent.Util
{
    public class SpriteImport : MonoBehaviour
    {

        public Sprite[] m_Sprites;
        public string[] m_Names;

        public SpriteInfo[] m_SpriteInfoList;
        [System.Serializable]
        public class SpriteInfo
        {
            public string name;
            public float scale_x;
            public float scale_y;
        }

        int _index;
        public Sprite GetSprite(string name)
        {
            _index = System.Array.IndexOf(m_Names, name);

            if (_index == -1
               || _index >= m_Sprites.Length)
            {
                return null;
            }

            return m_Sprites[_index];
        }

        bool _ascending = true;
        object _ascendingSymbol = null;
        public void Sort()
        {
            List<Sprite> spriteList = new List<Sprite>();
            List<string> nameList = new List<string>(m_Names);

            nameList.Sort((string str1, string str2) =>
            {
                return str1.CompareTo(str2) * (_ascending ? 1 : -1);
            });

            _ascendingSymbol = _ascending;
            _ascending = !_ascending;

            foreach (string name in nameList)
            {
                spriteList.Add(GetSprite(name));
            }

            m_Sprites = spriteList.ToArray();
            m_Names = nameList.ToArray();
        }

        public string GetSortSymbol()
        {
            return _ascendingSymbol == null ? string.Empty : (bool)_ascendingSymbol ? " ▲" : " ▼";
        }
    }
}