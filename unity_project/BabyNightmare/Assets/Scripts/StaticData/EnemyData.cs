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
        public float Attack_Radius;
        public int Coin_Min = 1;
        public int Coin_Max = 10;
        public ESpawnOrder SpawnOrder;
        public float Spawn_Delay_Min = 1f;
        public float Spawn_Delay_Max = 2f;
    }
}