using System.Collections.Generic;
using BabyNightmare.StaticData;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "CustomItemData", menuName = "BabyNightmare/CustomItemData")]

    public class CustomItemData : ScriptableObject
    {
        public int ID;
        public ECustomType Type;
        public Mesh Mesh;
        public Material Material;
        public string Name;
        public Sprite Thumbnail;
    }
}