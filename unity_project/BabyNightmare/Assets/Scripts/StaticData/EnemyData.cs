using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [Serializable]
    public class EnemyData
    {
        public int ID;
        public string Name;
        public float Damage;
        public int Health;
        public float Attack_Interval;
        public float Move_Step_Duration;
        public float Move_Step_Speed;
        public float Stop_Step_Duration;
    }
}