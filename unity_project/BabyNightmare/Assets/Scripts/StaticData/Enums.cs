using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    public enum EBoxType
    {
        Blue,
        Gold,
    }

    public enum EStatType
    {
        HP = 0,
        ATK = 1,
        DEF,
        Coin,
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

    public enum ETalentType
    {
        Unknown = 0,
        Max_HP_Amount = 1,
        Damage_Percentage,
        Defense_Percentage,
        Attack_Speed_Percentage,
        Critical_Prob_Percentage,
        Critical_Damage_Percentage,
        Coin_Earn_Percentage,
        Gem_Earn_Percentage,
        Bag_Size_Amount,
    }

    public enum EValueType
    {
        Unknown,
        Amount,
        Percentage,
    }

    public enum ECurrencyType
    {
        Coin,
        Gem,
        RV,
    }

    public enum ECustomItemType
    {
        Unknown = 0,
        Bag = 1,
        Clothes = 2,
        Shoes,
    }

    public enum EGlobalEventType
    {
        Unknown = 0,
        Game_Start = 1,
        Kill_Enemy,
        Die,
        Purchase_CustomItem,
    }

    public enum EEquipmentType
    {
        BaseBall,
        Boomerang,
        Horse,
        Knife,
        Lego,
        Rabbit,
        RingToy,
        Robot,
        WaterGun,
        Pouch,
        Glove,
        Shield,
        Milk,
        MilkBottle,
        Bomb,
        Missile,
    }

    public enum EDamageType
    {
        Direct,
        Area,
    }

    public enum EEnemyType
    {
        Slime_Blue,
        Slime_Green,
        Slime_Orange,
        Slime_Purple,
        Slime_Yellow,
        Mike,
        Celia,
        Sullivan,

        Slime_Blue_LV2,
        Slime_Green_LV2,
        Slime_Orange_LV2,
        Slime_Purple_LV2,
        Slime_Yellow_LV2,
        Mike_LV2,
        Celia_LV2,
        Sullivan_LV2,

        Mike_AD,
        Zombie_AD,

        Zombie,

        Sullivan_AD,
    }

    public enum ESizeType
    {
        Big,
        Mid,
        Small,
    }

    public enum ETargetType
    {
        Random,
        Nearest,
        Farthest,
        LowestHP,
        HightstHP,
    }
}
