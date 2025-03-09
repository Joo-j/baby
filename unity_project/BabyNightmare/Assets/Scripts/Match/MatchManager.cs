using System;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using BabyNightmare.HUD;

namespace BabyNightmare.Match
{
    public class MatchManager : SingletonBase<MatchManager>
    {
        private const string PATH_MATCH_FIELD = "Match/MatchField";
        private const string PATH_MATCH_VIEW = "Match/MatchView";
        private const string PATH_MATCH_FAIL_VIEW = "Match/MatchFailView";
        private const string PATH_MATCH_COMPLETE_VIEW = "Match/MatchCompleteView";

        private const int REROLL_EQUIPMENT_COUNT = 3;
        private const int REROLL_INITIAL_COST = 10;

        private MatchField _matchField = null;
        private MatchView _matchView = null;
        private Action _enterLobby = null;
        private List<WaveData> _waveDataList = null;
        private int _currentWave = 0;
        private int _maxWave = 0;
        private int _rerollCount = 0;
        private int _rerollCost = 0;

        public void Init(Action enterLobby)
        {
            _enterLobby = enterLobby;
        }

        public void StartMatch()
        {
            var stage = PlayerData.Instance.Stage;
            _waveDataList = StaticDataManager.Instance.GetWaveDataList(stage);
            if (null == _waveDataList)
            {
                Debug.LogError($"{stage}stage no wave data");
                return;
            }
            _currentWave = 0;
            _maxWave = _waveDataList.Count;

            _matchField = ObjectUtil.LoadAndInstantiate<MatchField>(PATH_MATCH_FIELD, null);
            _matchField.Init(GetCoin, OnClearWave, OnFailMatch);

            _matchView = ObjectUtil.LoadAndInstantiate<MatchView>(PATH_MATCH_VIEW, null);

            var initEM = StaticDataManager.Instance.GetEquipmentData(1001);
            var matchViewContext = new MatchViewContext(_matchField.RT, initEM, OnClickReroll, OnStartWave, _matchField.AttackEnemy, GetUpgradeData);
            _matchView.Init(matchViewContext);
            _matchView.RefreshProgress(_currentWave + 1, _maxWave, true);

            PlayerData.Instance.OnChangedCoinEvent.AddListener(RefreshRerollCost);
            PlayerData.Instance.Coin += 0;
        }

        private void OnFailMatch()
        {
            var res = Resources.Load<MatchFailView>(PATH_MATCH_FAIL_VIEW);
            var failView = GameObject.Instantiate(res);
            failView.Init(CloseMatch);
        }

        private void OnCompleteMatch()
        {
            var res = Resources.Load<MatchFailView>(PATH_MATCH_FAIL_VIEW);
            var completeView = GameObject.Instantiate(res);
            completeView.Init(CloseMatch);

            ++PlayerData.Instance.Stage;
            PlayerData.Instance.Save();
        }

        private void CloseMatch()
        {
            PlayerData.Instance.OnChangedCoinEvent.RemoveListener(RefreshRerollCost);

            GameObject.Destroy(_matchField.gameObject);
            _matchField = null;

            GameObject.Destroy(_matchView.gameObject);
            _matchView = null;

            _enterLobby?.Invoke();
        }

        private void OnStartWave()
        {
            var waveData = _waveDataList[_currentWave];

            var group = waveData.EnemySpawnDataGroup;
            var enemySpanwDataList = StaticDataManager.Instance.GetEnemySpawnDataList(group);
            if (null == enemySpanwDataList)
            {
                Debug.LogError($"{group}Group enemy spawn data is empty ");
                return;
            }

            var enemyDataList = new List<EnemyData>();
            for (var i = 0; i < enemySpanwDataList.Count; i++)
            {
                var spawnData = enemySpanwDataList[i];
                var enemyData = StaticDataManager.Instance.GetEnemyData(spawnData.EnemyID);
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

            _matchView.OnClearWave();
            _matchView.RefreshProgress(_currentWave, _maxWave, false);

            var boxData = GetBoxData();
            _matchField.EncounterBox(boxData, () => _matchView.ShowBox(boxData));
        }

        private EquipmentBoxData GetBoxData()
        {
            var waveData = _waveDataList[_currentWave];

            var id = waveData.EquipmentBoxDataID;
            var boxData = StaticDataManager.Instance.GetEquipmentBoxDataList(id);

            return boxData;
        }

        private void OnClickReroll()
        {
            var dataList = GetRerollData();
            _matchView.Reroll(dataList);

            var preCost = _rerollCost;
            ++_rerollCount;
            _rerollCost = REROLL_INITIAL_COST * (int)Mathf.Pow(_rerollCount, 2);
            PlayerData.Instance.Coin -= preCost;
        }

        private void RefreshRerollCost(int coin)
        {
            _matchView?.RefreshRerollCost(_rerollCost, coin);
        }

        private List<EquipmentData> GetRerollData()
        {
            var waveData = _waveDataList[_currentWave];

            var group = waveData.EquipmentProbDataGroup;
            var equipmentProbDataList = StaticDataManager.Instance.GetEquipmentProbDataList(group);

            var randomPicker = new WeightedRandomPicker<EquipmentProbData>();
            for (var i = 0; i < equipmentProbDataList.Count; i++)
            {
                var probDataGroup = equipmentProbDataList[i];
                randomPicker.Add(probDataGroup, probDataGroup.Prob);
            }

            var dataList = new List<EquipmentData>();
            for (var i = 0; i < REROLL_EQUIPMENT_COUNT; i++)
            {
                var probData = randomPicker.RandomPick();
                randomPicker.Remove(probData);

                var equipment = StaticDataManager.Instance.GetEquipmentData(probData.EquipmentID);
                dataList.Add(equipment);
            }

            return dataList;
        }

        private void GetCoin(int coin, Vector3 worldPos)
        {
            CoinHUD.SetSpreadPoint(worldPos, _matchField.RenderCamera, _matchView.FieldImage);
            PlayerData.Instance.Coin += coin;
        }

        private EquipmentData GetUpgradeData(EquipmentData data1, EquipmentData data2)
        {
            if (data1.ID != data2.ID)
                return null;

            if (data1.UpgradeDataID == -1)
                return null;

            return StaticDataManager.Instance.GetEquipmentData(data1.UpgradeDataID);
        }
    }
}
