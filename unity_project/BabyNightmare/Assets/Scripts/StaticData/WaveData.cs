using System;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "BabyNightmare/WaveData")]
    public class WaveData : ScriptableObject
    {
        public int ID;
        public int Group;
        public int EnemySpawnDataID;
        public EBoxType BoxType = EBoxType.Blue;
    }
}