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
            var chapter = PlayerData.Instance.Chapter;
            _waveDataList = StaticDataManager.Instance.GetWaveDataList(chapter);
            if (null == _waveDataList)
            {
                Debug.LogError($"{chapter}chapter no wave data");
                return;
            }
            _currentWave = 0;
            _maxWave = _waveDataList.Count;

            _matchField = ObjectUtil.LoadAndInstantiate<MatchField>(PATH_MATCH_FIELD, null);
            _matchField.Init(chapter, GetCoin, OnClearWave, OnFailMatch);

            _matchView = ObjectUtil.LoadAndInstantiate<MatchView>(PATH_MATCH_VIEW, null);

            var initEM = StaticDataManager.Instance.GetEquipmentData(1001);
            var matchViewContext = new MatchViewContext(_matchField.RT, initEM, OnClickReroll, OnStartWave, _matchField.AttackEnemy, GetUpgradeData);
            _matchView.Init(matchViewContext);
            _matchView.RefreshProgress(_currentWave + 1, _maxWave, true);

            HUDManager.Instance.ActiveHUD(EHUDType.Coin, true);
            PlayerData.Instance.OnChangedCoinEvent.AddListener(RefreshRerollCost);
            PlayerData.Instance.Coin = 0;
        }

        private void OnFailMatch()
        {
            HUDManager.Instance.ActiveHUD(EHUDType.Coin, false);
            HUDManager.Instance.ActiveHUD(EHUDType.Gem, true);

            var failView = ObjectUtil.LoadAndInstantiate<MatchFailView>(PATH_MATCH_FAIL_VIEW, null);
            failView.Init(100, CloseMatch);
        }

        private void OnCompleteMatch()
        {
            HUDManager.Instance.ActiveHUD(EHUDType.Coin, false);
            HUDManager.Instance.ActiveHUD(EHUDType.Gem, true);

            var completeView = ObjectUtil.LoadAndInstantiate<MatchCompleteView>(PATH_MATCH_COMPLETE_VIEW, null);
            completeView.Init(100, CloseMatch);

            ++PlayerData.Instance.Chapter;
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

            _matchView.OnClearWave();
            _matchView.RefreshProgress(_currentWave, _maxWave, false);

            var boxData = GetBoxData();
            _matchField.EncounterBox(boxData, () => _matchView.ShowBox(boxData.Type, () => OnGetBox(boxData)));
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

        private void OnGetBox(EquipmentBoxData boxData)
        {
            var equipmentIDList = boxData.EquipmentIDList;
            var dataList = new List<EquipmentData>();
            for (var i = 0; i < equipmentIDList.Count; i++)
            {
                var equipment = StaticDataManager.Instance.GetEquipmentData(equipmentIDList[i]);
                dataList.Add(equipment);
            }

            _matchView.Reroll(dataList);
        }

        private void RefreshRerollCost(int coin)
        {
            _matchView?.RefreshRerollCost(_rerollCost, coin);
        }

        private List<EquipmentData> GetRerollData()
        {
            var waveData = _waveDataList[_currentWave];

            var id = waveData.EquipmentProbDataID;
            var equipmentProbData = StaticDataManager.Instance.GetEquipmentProbData(id);

            var randomPicker = new WeightedRandomPicker<ProbData>();

            var probDataList = equipmentProbData.ProbDataList;
            for (var i = 0; i < probDataList.Count; i++)
            {
                var data = probDataList[i];
                randomPicker.Add(data, data.Prob);
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

            return StaticDataManager.Instance.GetEquipmentData(data1.ID + 100);
        }
    }
}
