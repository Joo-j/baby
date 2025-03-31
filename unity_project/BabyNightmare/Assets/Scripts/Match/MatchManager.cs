using System;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using BabyNightmare.HUD;
using Random = UnityEngine.Random;
using BabyNightmare.Talent;
using System.Linq;
using Supercent.Core.Audio;

namespace BabyNightmare.Match
{
    public class MatchManager : SingletonBase<MatchManager>
    {
        private const string PATH_MATCH_FIELD = "Match/MatchField";
        private const string PATH_MATCH_VIEW = "Match/UI/MatchView";
        private const string PATH_MATCH_FAIL_VIEW = "Match/UI/MatchFailView";
        private const string PATH_MATCH_COMPLETE_VIEW = "Match/UI/MatchCompleteView";

        private const int INITIAL_EQUIPMENT_ID = 1001;
        private const int EQUIPMENT_MAX_LEVEL = 3;
        private const int REROLL_EQUIPMENT_COUNT = 3;
        private const int MATCH_START_COIN = 10;
        private const int BASE_REROLL_PRICE = 10;
        private const int BASE_BAG_SIZE_UP_PRICE = 150;
        private const int BASE_REWARD_GEM = 10;

        private MatchField _matchField = null;
        private MatchView _matchView = null;
        private Action _enterLobby = null;
        private List<WaveData> _waveDataList = null;
        private int _currentWave = 0;
        private int _maxWave = 0;
        private int _rerollCount = 0;
        private int _rerollPrice = 0;
        private int _bagSizeUpPrice = 0;
        private int _bagSizeUpCount = 0;

        public void Init(Action enterLobby)
        {
            _enterLobby = enterLobby;
        }

        public void StartMatch(int chapter)
        {
            var chapterData = StaticDataManager.Instance.GetChapterData(chapter);
            if (null == chapterData)
            {
                Debug.LogError($"{chapter} no chapter data");
                return;
            }

            _waveDataList = StaticDataManager.Instance.GetWaveDataList(chapterData.WaveDataGroup);
            if (null == _waveDataList)
            {
                Debug.LogError($"{chapterData.WaveDataGroup} no wave data");
                return;
            }

            _currentWave = 0;
            _maxWave = _waveDataList.Count;
            _rerollCount = 0;
            _rerollPrice = 0;
            _bagSizeUpCount = 0;
            _bagSizeUpPrice = BASE_BAG_SIZE_UP_PRICE;
            _matchField = ObjectUtil.LoadAndInstantiate<MatchField>(PATH_MATCH_FIELD, null);

            var matchFieldContext = new MatchFieldContext(
                                        chapterData,
                                        GetCoinInField,
                                        (factor, immediate) => _matchView?.RefreshProgress(factor, immediate),
                                        OnClearWave,
                                        OnFailMatch);
            _matchField.Init(matchFieldContext);

            _matchView = ObjectUtil.LoadAndInstantiate<MatchView>(PATH_MATCH_VIEW, null);

            var initEM = StaticDataManager.Instance.GetEquipmentData(INITIAL_EQUIPMENT_ID);
            var matchViewContext = new MatchViewContext(
                                        _matchField.RT,
                                        initEM,
                                        GetRerollData,
                                        OnClickReroll,
                                        OnClickBagSizeUp,
                                        OnStartWave,
                                        _matchField.UseEquipment,
                                        _matchField.MoveCamera,
                                        GetUpgradeData);
            _matchView.Init(matchViewContext);

            var waveData = _waveDataList[_currentWave];
            _matchView.RefreshWave(_currentWave + 1, _maxWave, waveData.BoxType);

            PlayerData.Instance.OnChangedCoinEvent.AddListener(OnChangeCoin);

            CoinHUD.UseFX(false);
            PlayerData.Instance.Coin = MATCH_START_COIN;
            CoinHUD.UseFX(true);

            AudioManager.PlaySFX("AudioClip/Start_Match");
        }

        private void CloseMatch()
        {
            PlayerData.Instance.OnChangedCoinEvent.RemoveListener(OnChangeCoin);

            ProjectilePool.Instance.ReturnAll();
            FXPool.Instance.ReturnAll();
            PopupTextPool.Instance.ReturnAll();

            GameObject.Destroy(_matchField.gameObject);
            _matchField = null;

            GameObject.Destroy(_matchView.gameObject);
            _matchView = null;

        }

        private void OnChangeCoin(int coin)
        {
            _matchView?.RefreshRerollPrice(_rerollPrice, coin);
            _matchView?.RefreshBagSizeUpPrice(_bagSizeUpPrice, coin);
        }

        private void OnFailMatch()
        {
            CloseMatch();

            var gem = PlayerData.Instance.Chapter * BASE_REWARD_GEM;
            var talentGem = TalentManager.Instance.GetValue(ETalentType.Gem_Earn_Percentage);
            gem += Mathf.CeilToInt(gem * talentGem);
            PlayerData.Instance.Save();

            var failView = ObjectUtil.LoadAndInstantiate<MatchFailView>(PATH_MATCH_FAIL_VIEW, null);
            failView.Init(gem, _enterLobby);
            AudioManager.PlaySFX("AudioClip/Match_Fail");
        }

        private void OnCompleteMatch()
        {
            CloseMatch();

            var gem = PlayerData.Instance.Chapter * BASE_REWARD_GEM;
            var talentGem = TalentManager.Instance.GetValue(ETalentType.Gem_Earn_Percentage);
            gem += Mathf.CeilToInt(gem * talentGem);

            var completeView = ObjectUtil.LoadAndInstantiate<MatchCompleteView>(PATH_MATCH_COMPLETE_VIEW, null);
            completeView.Init(gem, _enterLobby);

            ++PlayerData.Instance.Chapter;
            PlayerData.Instance.Save();
            AudioManager.PlaySFX("AudioClip/Match_Complete");
        }

        private void OnStartWave()
        {
            var waveData = _waveDataList[_currentWave];

            var id = waveData.EnemySpawnDataID;
            var enemySpanwData = StaticDataManager.Instance.GetEnemySpawnData(id);
            if (null == enemySpanwData)
            {
                Debug.LogError($"{id}Group enemy spawn data is empty ");
                return;
            }

            var enemyDataList = new List<EnemyData>();
            var typeList = enemySpanwData.EnemyTypeList;
            for (var i = 0; i < typeList.Count; i++)
            {
                var type = typeList[i];
                var enemyData = StaticDataManager.Instance.GetEnemyData(type);
                if (null == enemyData)
                {
                    Debug.LogError($"{type} enemy is null");
                    continue;
                }

                enemyDataList.Add(enemyData);
            }

            _matchField.StartWave(enemyDataList);
            _matchView.RefreshWave(_currentWave + 1, _maxWave, waveData.BoxType);
        }

        private void OnClearWave()
        {
            if (_currentWave + 1 >= _maxWave)
            {
                OnCompleteMatch();
                return;
            }

            var waveData = _waveDataList[_currentWave];

            _matchView.OnClearWave();

            var dataList = GetRerollData();
            _matchField.EncounterBox(waveData.BoxType, () => _matchView.ShowBox(waveData.BoxType, _matchField.GetWaveCoin(), dataList));
            AudioManager.PlaySFX("AudioClip/Clear_Wave");

            ++_currentWave;

        }

        private void OnClickReroll()
        {
            var prePrice = _rerollPrice;
            ++_rerollCount;
            _rerollPrice = BASE_REROLL_PRICE * (int)Mathf.Pow(_rerollCount, 2);

            PlayerData.Instance.Coin -= prePrice;
        }

        private void OnClickBagSizeUp()
        {
            var prePrice = _bagSizeUpPrice;
            ++_bagSizeUpCount;
            _bagSizeUpPrice = BASE_BAG_SIZE_UP_PRICE * (_bagSizeUpCount + 1);

            PlayerData.Instance.Coin -= prePrice;
        }

        private List<EquipmentData> GetRerollData()
        {
            var waveData = _waveDataList[_currentWave];
            var weightFactor = 3;
            switch (waveData.BoxType)
            {
                case EBoxType.Blue:
                    weightFactor = 8;
                    break;
                case EBoxType.Gold:
                    weightFactor = 3;
                    break;
            }

            var equipmentDataList = StaticDataManager.Instance.EquipmentDataList;
            var randomPicker = new WeightedRandomPicker<EquipmentData>();

            for (var i = 0; i < equipmentDataList.Count; i++)
            {
                var data = equipmentDataList[i];
                var prob = data.Prob * Mathf.RoundToInt(Mathf.Pow(EQUIPMENT_MAX_LEVEL - data.Level + 1, weightFactor));
                randomPicker.Add(data, prob);
            }

            var dataDict = new Dictionary<EEquipmentType, EquipmentData>();
            while (dataDict.Count < REROLL_EQUIPMENT_COUNT)
            {
                var data = randomPicker.RandomPick();
                randomPicker.Remove(data);
                if (true == dataDict.ContainsKey(data.Type))
                    continue;

                dataDict.Add(data.Type, data);
            }

            return dataDict.Values.ToList();
        }

        private void GetCoinInField(int coin, Vector3 worldPos)
        {
            CoinHUD.SetSpreadPoint(worldPos, _matchField.RenderCamera, _matchView.FieldImage);
            PlayerData.Instance.Coin += coin;
        }

        private EquipmentData GetUpgradeData(EquipmentData data1, EquipmentData data2)
        {
            if (data1.ID != data2.ID)
                return null;

            return StaticDataManager.Instance.GetEquipmentData(data1.ID + 100);
        }
    }
}
