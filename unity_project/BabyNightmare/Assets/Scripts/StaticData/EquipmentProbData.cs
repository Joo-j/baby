using System;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [Serializable]
    public class ProbData
    {
        public int EquipmentID;
        public int Prob;
    }

    [CreateAssetMenu(fileName = "EquipmentProbData", menuName = "BabyNightmare/EquipmentProbData")]
    public class EquipmentProbData : ScriptableObject
    {
        public int ID;
        public List<ProbData> ProbDataList;
    }
}