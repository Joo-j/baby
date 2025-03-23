using System;
using System.Collections.Generic;
using Supercent.Util;
using UnityEngine;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using Newtonsoft.Json;
using System.Linq;
using BabyNightmare.HUD;

namespace BabyNightmare.Talent
{
    public class TalentManager : SingletonBase<TalentManager>
    {
        private const string FILE_TALENT_SAVE_DATA = "talent_data";
        private const string PATH_TALENT_DATA = "StaticData/TalentData";
        private const string PATH_TALENT_VIEW = "Talent/TalentView";
        private const string KEY_SHOW_COUNT = "Talent_ShowCount";
        private Dictionary<ETalentType, TalentData> _dataDict = null;
        private Dictionary<ETalentType, int> _levelDict = null;
        private TalentView _talentView = null;

        public int ShowCount
        {
            get => PlayerPrefs.GetInt(KEY_SHOW_COUNT);
            set => PlayerPrefs.SetInt(KEY_SHOW_COUNT, value);
        }

        public void Init()
        {
            Load();

            var talentDataArr = Resources.LoadAll<TalentData>(PATH_TALENT_DATA);
            if (null == talentDataArr || talentDataArr.Length == 0)
            {
                Debug.LogError($"{PATH_TALENT_DATA}에 데이터가 없습니다.");
                return;
            }

            _dataDict = new Dictionary<ETalentType, TalentData>();

            for (var i = 0; i < talentDataArr.Length; i++)
            {
                var data = talentDataArr[i];
                var type = data.TalentType;
                _dataDict.Add(type, data);
            }
        }

        public void Show(Transform parentTF)
        {
            if (null == _talentView)
            {
                _talentView = ObjectUtil.LoadAndInstantiate<TalentView>(PATH_TALENT_VIEW, parentTF);
                if (null == _talentView)
                {
                    Debug.Log($"{PATH_TALENT_VIEW} no prefab");
                    return;
                }

                (_talentView.transform as RectTransform).SetFullStretch();

                var dataList = _dataDict.Values.ToList();
                dataList.Sort();

                _talentView.Init(dataList, Upgrade);
                _talentView.RefreshLevel(_levelDict, false);
            }

            _talentView.gameObject.SetActive(true);
            _talentView.RefreshButton(PlayerData.Instance.Gem, GetPrice());

            ++ShowCount;
        }

        public void Hide()
        {
            _talentView.gameObject.SetActive(false);
        }

        private int GetPrice()
        {
            var totalLevel = 0;
            var levelList = _levelDict.Values.ToList();
            for (var i = 0; i < levelList.Count; i++)
            {
                totalLevel += levelList[i];
            }

            return (int)Mathf.Pow(totalLevel, 2) * 10;
        }

        private void Upgrade()
        {
            var randomPicker = new WeightedRandomPicker<TalentData>();

            foreach (var pair in _dataDict)
            {
                var data = pair.Value;
                randomPicker.Add(data, data.Prob);
            }

            var pickData = randomPicker.RandomPick();
            var upgradeType = pickData.TalentType;
            ++_levelDict[upgradeType];

            var price = GetPrice();
            PlayerData.Instance.Gem -= price;
            PlayerData.Instance.Save();
            _talentView.RefreshButton(PlayerData.Instance.Gem, price);

            _talentView.ShowGacha(() =>
            {
                _talentView.RefreshLevel(_levelDict, true);
                _talentView.RefreshButton(PlayerData.Instance.Gem, price);
            });

            Debug.Log($"{upgradeType}타입 업그레이드");

            Save();
        }

        public float GetValue(ETalentType type)
        {
            if (false == _dataDict.TryGetValue(type, out var data))
                return 0;

            if (false == _levelDict.TryGetValue(type, out var level))
                return 0;

            switch (data.ValueType)
            {
                case EValueType.Amount: return level * data.IncreaseValue;
                case EValueType.Percentage: return level * data.IncreaseValue * 0.01f;
                default: return level * data.IncreaseValue;
            }
        }

        public bool IsRedDot()
        {
            return PlayerData.Instance.Gem >= GetPrice();
        }

        private void Save()
        {
            var binaryData = JsonConvert.SerializeObject(_levelDict);
            FileSaveUtil.Save(FILE_TALENT_SAVE_DATA, binaryData, false, false);
        }

        private void Load()
        {
            _levelDict = new Dictionary<ETalentType, int>();

            var types = Enum.GetValues(typeof(ETalentType));

            var binaryData = FileSaveUtil.Load(FILE_TALENT_SAVE_DATA, false, false);
            if (true == binaryData.IsNullOrEmpty())
            {
                InitLevelDict(types); //저장 데이터가 없을 시 초기화 후 리턴
                return;
            }

            var tempDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(binaryData);
            foreach (var pair in tempDict)
            {
                var type_string = pair.Key;
                if (false == Enum.TryParse<ETalentType>(type_string, out var type))
                    continue;

                var level = pair.Value;

                _levelDict.Add(type, level);
            }

            InitLevelDict(types); //저장된 데이터를 파싱한 이후에 초기화 하여 추가된 타입 대응

            void InitLevelDict(Array types)
            {
                foreach (ETalentType type in types)
                {
                    if (type == ETalentType.Unknown)
                        continue;

                    if (true == _levelDict.ContainsKey(type))
                        continue;

                    _levelDict.Add(type, 0);
                }
            }
        }
    }
}