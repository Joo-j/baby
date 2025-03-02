using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare
{
    public class PlayerData : SingletoneBase<PlayerData>
    {
        public int Stage = 1;
        public float Health = 100;
        public bool[,] EnableSlotArr = new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } };

        public void Init()
        {
            Load();
        }

        private void Load()
        {

        }

        public void Save()
        {

        }
    }
}
