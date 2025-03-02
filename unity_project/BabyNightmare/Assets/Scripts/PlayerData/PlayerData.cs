using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare
{
    public class PlayerData : SingletoneBase<PlayerData>
    {
        public int Stage = 1;
        public float Health = 100;
        private int _coin = 0;
        private int _gem = 0;


        public class ChangedCoinEvent : UnityEngine.Events.UnityEvent<int> { }
        public ChangedCoinEvent OnChangedCoinEvent { get; private set; } = new ChangedCoinEvent();

        public int Coin
        {
            get => _coin;
            set
            {
                _coin = value;
                if (_coin < 0)
                    _coin = 0;

                OnChangedGemEvent?.Invoke(_coin);
            }
        }



        public class ChangedGemEvent : UnityEngine.Events.UnityEvent<int> { }
        public ChangedGemEvent OnChangedGemEvent { get; private set; } = new ChangedGemEvent();

        public int Gem
        {
            get => _gem;
            set
            {
                _gem = value;
                if (_gem < 0)
                    _gem = 0;

                OnChangedGemEvent?.Invoke(_gem);
            }
        }



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
