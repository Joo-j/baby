using System;
using System.Collections;
using System.Collections.Generic;
using BabyNightmare.StaticData;
using UnityEngine;

namespace BabyNightmare.Lobby
{
    [Serializable]
    public class MenuButtonData : IComparable<MenuButtonData>
    {
        public ELobbyButtonType Type;
        public int Index;
        public bool ShowGuide;
        public bool Guide_Force;

        public int CompareTo(MenuButtonData obj)
        {
            return Index.CompareTo(obj.Index);
        }
    }

    [CreateAssetMenu(fileName = "MenuButtonConfig", menuName = "SnakeClash/MenuButtonConfig")]
    public class MenuButtonConfig : ScriptableObject
    {
        [SerializeField] private List<MenuButtonData> _menuButtonDataList;

        public List<MenuButtonData> MenuButtonDataList => _menuButtonDataList;
    }
}