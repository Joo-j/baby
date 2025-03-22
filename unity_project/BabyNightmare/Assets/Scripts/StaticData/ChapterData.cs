using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "ChapterData", menuName = "BabyNightmare/ChapterData")]
    public class ChapterData : ScriptableObject
    {
        public int Chapter;
        public string Title;
        public Sprite Icon;
        public int WaveDataGroup;
    }
}