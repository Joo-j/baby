using System.Collections;
using System.Collections.Generic;
using BabyNightmare.Util;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "StaticDataSheet", menuName = "BabyNightmare/StaticDataSheet")]
    public class StaticDataSheet : ScriptableObject
    {
        public List<WaveData> WaveDataList;
        public List<EquipmentData> EquipmentDataList;
        public List<EquipmentProbData> EquipmentProbDataList;
        public List<EnemyData> EnemyDataList;
        public List<EnemySpawnData> EnemySpawnDataList;
        public List<BoolMatrixData> SlotDataList;
    }
}