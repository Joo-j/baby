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
        private const string KEY_CHAPTER = "player_data_chapter";
        private const string KEY_TOTAL_ATTEMPT_COUNT = "player_data_total_attempt_count";
        private const string KEY_CHAPTER_ATTEMPT_COUNT = "player_data_chapter_attempt_count";

        private const string KEY_HP = "player_data_hp";
        private const string KEY_COIN = "player_data_coin";
        private const string KEY_GEM = "player_data_gem";

        public float HP = 0;
        private int _coin = 0;
        private int _gem = 0;
        public bool Haptic_Active;
        public int Chapter = 1;
        public int TotalAttemptCount;
        public int ChapterAttemptCount;
        public List<Vector2Int> _bagShape;


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

            Chapter = jsonClass[KEY_CHAPTER]?.AsInt ?? 0;
            TotalAttemptCount = jsonClass[KEY_TOTAL_ATTEMPT_COUNT]?.AsInt ?? 0;
            ChapterAttemptCount = jsonClass[KEY_CHAPTER_ATTEMPT_COUNT]?.AsInt ?? 0;

            HP = jsonClass[KEY_HP]?.AsInt ?? 0;
            _coin = jsonClass[KEY_COIN]?.AsInt ?? 0;
            _gem = jsonClass[KEY_GEM]?.AsInt ?? 0;

            Debug.Log("PlayerData Load Success");
        }

        public void Save()
        {
            var jsonClass = new JSONClass();

            jsonClass[KEY_CHAPTER] = new JSONData(Chapter);
            jsonClass[KEY_TOTAL_ATTEMPT_COUNT] = new JSONData(TotalAttemptCount);
            jsonClass[KEY_CHAPTER_ATTEMPT_COUNT] = new JSONData(ChapterAttemptCount);
            jsonClass[KEY_HP] = new JSONData(HP);
            jsonClass[KEY_COIN] = new JSONData(_coin);
            jsonClass[KEY_GEM] = new JSONData(_gem);

            var binaryData = jsonClass.ToString();
            FileSaveUtil.Save(FILE_PLAYER_DATA, binaryData, false, false);
            Debug.Log("PlayerData Save Success");
        }
    }
}
