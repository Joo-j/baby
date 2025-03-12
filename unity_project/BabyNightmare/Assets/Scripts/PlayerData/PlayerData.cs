using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using Supercent.Util;
using BabyNightmare.Util;

namespace BabyNightmare
{
    public class PlayerData : SingletonBase<PlayerData>
    {
        private const string FILE_PLAYER_DATA = "player_data";
        private const string KEY_STAGE = "player_data_stage";
        private const string KEY_HP = "player_data_hp";
        private const string KEY_COIN = "player_data_coin";
        private const string KEY_GEM = "player_data_gem";

        public bool Haptic_Active;
        public int Stage = 1;
        public float HP = 50;
        private int _coin = 10;
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

                OnChangedCoinEvent?.Invoke(_coin);
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
            var binaryData = FileSaveUtil.Load(FILE_PLAYER_DATA, false, false);
            if (true == binaryData.IsNullOrEmpty())
                return;

            var jsonClass = JSONClass.Parse(binaryData);
            if (null == jsonClass)
                return;

            Stage = jsonClass[KEY_STAGE]?.AsInt ?? 0;
            HP = jsonClass[KEY_HP]?.AsInt ?? 0;
            _coin = jsonClass[KEY_COIN]?.AsInt ?? 0;
            _gem = jsonClass[KEY_GEM]?.AsInt ?? 0;
        }

        public void Save()
        {
            var jsonClass = new JSONClass();

            jsonClass[KEY_STAGE] = new JSONData(Stage);
            jsonClass[KEY_HP] = new JSONData(HP);
            jsonClass[KEY_COIN] = new JSONData(_coin);
            jsonClass[KEY_GEM] = new JSONData(_gem);

            var binaryData = jsonClass.ToString();
            FileSaveUtil.Save(FILE_PLAYER_DATA, binaryData, true, true);
        }
    }
}
