using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    public enum EBoxType
    {
        Normal,
        Rare,
        Epic
    }

    public enum EStatType
    {
        HP,
        ATK,
        DEF,
    }

    public enum ELobbyButtonType
    {
        Unknown,
        Home,
        Shop,
        Talent,        
        Mission,
        CustomShop,
    }

     public enum ESpawnOrder
    {
        Near = 1,
        Middle = 2,
        Far = 3,
        Random = 4,
    }
}
