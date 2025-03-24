using System;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using BabyNightmare.HUD;
using Random = UnityEngine.Random;
using BabyNightmare.Talent;
using UnityEngine.AI;
using System.Linq;

namespace BabyNightmare.Match
{
    public class MatchManager : SingletonBase<MatchManager>
    {
        private const string PATH_MATCH_FIELD = "Match/MatchField";
        private const string PATH_MATCH_VIEW = "Match/UI/MatchView";
        private const string PATH_MATCH_FAIL_VIEW = "Match/UI/MatchFailView";
        private const string PATH_MATCH_COMPLETE_VIEW = "Match/UI/MatchCompleteView";

        private const int EQUIPMENT_MAX_LEVEL = 3;
        private const int REROLL_EQUIPMENT_COUNT = 3;
        private const int INITIAL_COIN = 10;
        private const int REROLL_BASE_PRICE = 10;
        private const int BASE_REWARD_GEM = 10;
        private const int EQUIPMENT_WEIGHT_FACTOR_MAX = 5;
        private const int EQUIPMENT_WEIGHT_FACTOR_MIN = 2;

        private MatchField _matchField = null;
        private MatchView _matchView = null;
        private Action _enterLobby = null;
        private List<WaveData> _waveDataList = null;
        private int _currentWave = 0;
        private int _maxWave = 0;
        private int _rerollCount = 0;
        private int _rerollPrice = 0;

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

            _matchField = ObjectUtil.LoadAndInstantiate<MatchField>(PATH_MATCH_FIELD, null);
            var matchFieldContext = new MatchFieldContext(
                                        chapterData,
                                        GetCoin,
                                        (factor, immediate) => _matchView?.RefreshProgress(factor, immediate),
                                        OnClearWave,
                                        OnFailMatch);
            _matchField.Init(matchFieldContext);

            _matchView = ObjectUtil.LoadAndInstantiate<MatchView>(PATH_MATCH_VIEW, null);

            var initEM = StaticDataManager.Instance.GetEquipmentData(1001);
            var matchViewContext = new MatchViewContext(
                                        _matchField.RT,
                                        initEM,
                                        OnClickReroll,
                                        OnStartWave,
                                        _matchField.AttackEnemy,
                                        _matchField.MoveCamera,
                                        GetUpgradeData
                                        );
            _matchView.Init(matchViewContext);

            var waveData = _waveDataList[_currentWave];
            _matchView.RefreshWave(_currentWave + 1, _maxWave, waveData.BoxType);

            CoinHUD.UseFX(false);
            PlayerData.Instance.Coin = INITIAL_COIN;
            CoinHUD.UseFX(true);

            _matchView?.RefreshRerollPrice(_rerollPrice, PlayerData.Instance.Coin);
        }

        private void OnFailMatch()
        {
            var gem = PlayerData.Instance.Chapter * BASE_REWARD_GEM;
            var talentGem = TalentManager.Instance.GetValue(ETalentType.Gem_Earn_Percentage);
            gem += Mathf.CeilToInt(gem * talentGem);

            var failView = ObjectUtil.LoadAndInstantiate<MatchFailView>(PATH_MATCH_FAIL_VIEW, null);
            failView.Init(gem, CloseMatch);
        }

        private void OnCompleteMatch()
        {
            var gem = PlayerData.Instance.Chapter * BASE_REWARD_GEM;
            var talentGem = TalentManager.Instance.GetValue(ETalentType.Gem_Earn_Percentage);
            gem += Mathf.CeilToInt(gem * talentGem);

            var completeView = ObjectUtil.LoadAndInstantiate<MatchCompleteView>(PATH_MATCH_COMPLETE_VIEW, null);
            completeView.Init(gem, CloseMatch);

            ++PlayerData.Instance.Chapter;
            PlayerData.Instance.Save();
        }

        private void CloseMatch()
        {
            ProjectilePool.Instance.ReturnAll();
            FXPool.Instance.ReturnAll();
            PopupTextPool.Instance.ReturnAll();

            GameObject.Destroy(_matchField.gameObject);
            _matchField = null;

            GameObject.Destroy(_matchView.gameObject);
            _matchView = null;

            _enterLobby?.Invoke();

            PlayerData.Instance.Save();
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

            var idList = enemySpanwData.EnemyIDList;
            for (var i = 0; i < idList.Count; i++)
            {
                var enemyData = StaticDataManager.Instance.GetEnemyData(idList[i]);
                enemyDataList.Add(enemyData);
            }

            _matchField.StartWave(enemyDataList);
        }

        private void OnClearWave()
        {
            ++_currentWave;

            if (_currentWave >= _maxWave)
            {
                OnCompleteMatch();
                return;
            }

            var waveData = _waveDataList[_currentWave];

            _matchView.OnClearWave();
            _matchView.RefreshWave(_currentWave + 1, _maxWave, waveData.BoxType);

            _matchField.EncounterBox(waveData.BoxType,
            () => _matchView.ShowBox(waveData.BoxType,
            (boxPos) =>
            {
                var dataList = GetRerollData();
                _matchView.Reroll(dataList);
                _matchField.GetWaveCoin(boxPos);
            }));
        }

        private void OnClickReroll()
        {
            var dataList = GetRerollData();
            _matchView.Reroll(dataList);

            var preCost = _rerollPrice;
            ++_rerollCount;
            _rerollPrice = REROLL_BASE_PRICE * (int)Mathf.Pow(_rerollCount, 2);

            PlayerData.Instance.Coin -= preCost;
            _matchView?.RefreshRerollPrice(_rerollPrice, PlayerData.Instance.Coin);
        }

        private List<EquipmentData> GetRerollData()
        {
            var waveData = _waveDataList[_currentWave];
            var weightFactor = 3;
            switch (waveData.BoxType)
            {
                case EBoxType.Blue:
                    weightFactor = 5;
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

        private void GetCoin(int coin, Vector3 worldPos)
        {
            CoinHUD.SetSpreadPoint(worldPos, _matchField.RenderCamera, _matchView.FieldImage);
            PlayerData.Instance.Coin += coin;
            _matchView?.RefreshRerollPrice(_rerollPrice, PlayerData.Instance.Coin);
        }

        private EquipmentData GetUpgradeData(EquipmentData data1, EquipmentData data2)
        {
            if (data1.ID != data2.ID)
                return null;

            return StaticDataManager.Instance.GetEquipmentData(data1.ID + 100);
        }
    }
}
