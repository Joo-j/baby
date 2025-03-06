using System;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [Serializable]
    public class EquipmentBoxData
    {
        public int ID;
        public EBoxType Type;
        public List<int> EquipmentIDList;
    }
}

