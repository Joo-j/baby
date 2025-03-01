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
        public float Move_Speed;
        public float Attack_Interval;        
    }
}