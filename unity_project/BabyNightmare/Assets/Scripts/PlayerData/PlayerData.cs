using System;
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
        private const string KEY_VERSION = "player_data_Version";
        private const string KEY_TOTAL_PLAY_TIME = "player_data_PlayTime";
        private const string KEY_DAY_PLAY_TIME = "player_data_DayTime";
        private const string KEY_SESSION_PLAY_TIME = "player_data_SessionTime";
        private const string KEY_LAST_PLAY_DATE = "player_data_LastPlayDate";
        private const string KEY_HP = "player_data_hp";
        private const string KEY_COIN = "player_data_coin";
        private const string KEY_GEM = "player_data_gem";
        private const string KEY_CHAPTER = "player_data_chapter";
        private const string KEY_TOTAL_ATTEMPT_COUNT = "player_data_total_attempt_count";
        private const string KEY_CHAPTER_ATTEMPT_COUNT = "player_data_chapter_attempt_count";

        public static int Version = 1;
        public static float TotalPlayTime = 0f;
        public static float DayPlayTime = 0f;
        public static float SessionPlayTime = 0f;
        public static DateTime LastPlayDate = DateTime.MinValue;
        private int _coin = 0;
        private int _gem = 0;
        public float HP = 0;
        public int Chapter = 1;
        public int TotalAttemptCount;
        public int ChapterAttemptCount;
        public bool Haptic_Active;


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

            if (DateTime.Now >= LastPlayDate.Date.AddDays(1))
            {
                Debug.Log($"최근 접속 날짜({LastPlayDate.Date})로 부터 하루 이상 지나 DayPlayTime을 초기화합니다.");
                DayPlayTime = 0f;
            }

            Debug.Log($"최근 접속 날짜를 오늘({DateTime.Now.Date})로 갱신합니다.");
            LastPlayDate = DateTime.Now.Date;
            Save();

            GameLifeCycle.Start_Coroutine(Co_CountTime());
        }

        private IEnumerator Co_CountTime()
        {
            while (true)
            {
                yield return null;

                TotalPlayTime += Time.deltaTime;
                DayPlayTime += Time.deltaTime;
                SessionPlayTime += Time.deltaTime;
            }
        }



        private void Load()
        {
            var binaryData = FileSaveUtil.Load(FILE_PLAYER_DATA, false, false);
            if (true == binaryData.IsNullOrEmpty())
                return;

            var jsonClass = JSONClass.Parse(binaryData);
            if (null == jsonClass)
                return;

            TotalPlayTime = jsonClass[KEY_TOTAL_PLAY_TIME]?.AsFloat ?? 0f;
            DayPlayTime = jsonClass[KEY_DAY_PLAY_TIME]?.AsFloat ?? 0f;
            SessionPlayTime = 0f;

            if (string.IsNullOrEmpty(jsonClass[KEY_LAST_PLAY_DATE]))
            {
                LastPlayDate = DateTime.MinValue;
            }
            else
            {
                if (DateTime.TryParse(jsonClass[KEY_LAST_PLAY_DATE], out var date))
                    LastPlayDate = date;
            }

            _coin = jsonClass[KEY_COIN]?.AsInt ?? 0;
            _gem = jsonClass[KEY_GEM]?.AsInt ?? 0;
            HP = jsonClass[KEY_HP]?.AsInt ?? 0;
            Chapter = jsonClass[KEY_CHAPTER]?.AsInt ?? 0;
            TotalAttemptCount = jsonClass[KEY_TOTAL_ATTEMPT_COUNT]?.AsInt ?? 0;
            ChapterAttemptCount = jsonClass[KEY_CHAPTER_ATTEMPT_COUNT]?.AsInt ?? 0;

            Debug.Log("PlayerData Load Success");
        }

        public void Save()
        {
            var jsonClass = new JSONClass();

            jsonClass[KEY_TOTAL_PLAY_TIME] = new JSONData(TotalPlayTime);
            jsonClass[KEY_DAY_PLAY_TIME] = new JSONData(DayPlayTime);
            jsonClass[KEY_SESSION_PLAY_TIME] = new JSONData(SessionPlayTime);
            jsonClass[KEY_LAST_PLAY_DATE] = new JSONData(LastPlayDate.ToString());
            jsonClass[KEY_COIN] = new JSONData(_coin);
            jsonClass[KEY_GEM] = new JSONData(_gem);
            jsonClass[KEY_HP] = new JSONData(HP);
            jsonClass[KEY_CHAPTER] = new JSONData(Chapter);
            jsonClass[KEY_TOTAL_ATTEMPT_COUNT] = new JSONData(TotalAttemptCount);
            jsonClass[KEY_CHAPTER_ATTEMPT_COUNT] = new JSONData(ChapterAttemptCount);

            var binaryData = jsonClass.ToString();
            FileSaveUtil.Save(FILE_PLAYER_DATA, binaryData, false, false);
            Debug.Log("PlayerData Save Success");
        }
    }
}
