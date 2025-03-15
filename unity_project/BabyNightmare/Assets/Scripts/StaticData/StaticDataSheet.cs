using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "StaticDataSheet", menuName = "BabyNightmare/StaticDataSheet")]
    public class StaticDataSheet : ScriptableObject
    {
        public List<LobbyButtonData> LobbyButtonDataList;
        public List<WaveData> WaveDataList;
        public List<EquipmentProbData> EquipmentProbDataList;
    }
}