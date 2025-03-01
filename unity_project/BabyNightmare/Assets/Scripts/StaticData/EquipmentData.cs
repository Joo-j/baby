using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BabyNightmare.Util;

namespace BabyNightmare.StaticData
{
    [Serializable]
    public class EquipmentData
    {
        public int ID;
        public EEquipmentGrade Grade;
        public string Name;
        public int Level;
        public float Damage;
        public float CoolTime;
    }
}
