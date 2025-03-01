using System;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [Serializable]
    public class WaveData
    {
        public int ID;
        public int Stage;
        public int Wave;
        public int EquipmentProbDataGroup;
        public int EnemySpawnDataGroup;
    }
}