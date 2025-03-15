using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    public enum EBoxType
    {
        Bronze,
        Silver,
        Gold
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

    public enum EConditionType
    {
        Unknown = 0,
        TotalAttemptCount = 1,
        Chapter = 2,
        ChapterAttemptCount,
    }

    public enum EComparisonType
    {
        Greater = 1,
        Less = 2,
        Greater_Equal,
        Less_Eqaul,
        Equal
    }
}
