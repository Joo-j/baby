using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    public static class PlayerData
    {
        public static int Stage = 1;
        public static float Health = 100;
        public static bool[,] EnableSlotArr = new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } };

        public static void Init()
        {
            Load();
        }

        private static void Load()
        {

        }

        public static void Save()
        {

        }
    }
}