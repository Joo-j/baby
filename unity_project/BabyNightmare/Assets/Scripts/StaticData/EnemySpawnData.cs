using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "EnemySpawnData", menuName = "BabyNightmare/EnemySpawnData")]
    public class EnemySpawnData : ScriptableObject
    {
        public int ID;
        public List<int> EnemyIDList;
        public List<EEnemyType> EnemyTypeList;
    }
}