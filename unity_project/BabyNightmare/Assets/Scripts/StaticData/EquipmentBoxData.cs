using System;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "EquipmentBoxData", menuName = "BabyNightmare/EquipmentBoxData")]
    public class EquipmentBoxData : ScriptableObject
    {
        public int ID;
        public EBoxType Type;
        public List<int> EquipmentIDList;
    }
}

