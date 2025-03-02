using System;
using System.Collections.Generic;
using BabyNightmare.Util;
using UnityEngine;
using BabyNightmare.StaticData;
using Supercent.Util;
using Unity.VisualScripting;

namespace BabyNightmare.Match
{
    public class MatchManager : SingletoneBase<MatchManager>
    {
        private const string PATH_MATCH_FIELD = "Match/MatchField";
        private const string PATH_MATCH_VIEW = "Match/MatchView";
        private const string PATH_MATCH_FAIL_VIEW = "Match/MatchFailView";
        private const string PATH_MATCH_COMPLETE_VIEW = "Match/MatchCompleteView";

        private const int REROLL_EQUIPMENT_COUNT = 3;

        private MatchField _matchField = null;
        private MatchView _matchView = null;
        private Action _enterLobby = null;
        private List<WaveData> _waveDataList = null;
        private int _currentWave = 0;
        private int _maxWave = 0;

        public void Init(Action enterLobby)
        {
            Load();

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
            _matchField.Init(OnClearWave, OnFailMatch);

            _matchView = ObjectUtil.LoadAndInstantiate<MatchView>(PATH_MATCH_VIEW, null);

            var initEM = StaticDataManager.Instance.GetEquipmentData(1001);
            var matchViewContext = new MatchViewContext(_matchField.RT, initEM, Reroll, OnStartWave, _matchField.AttackEnemy);
            _matchView.Init(matchViewContext);
            _matchView.RefreshWave(_currentWave + 1, _maxWave);

            Reroll();
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

            _matchView.RefreshWave(_currentWave + 1, _maxWave);
        }

        private List<EquipmentData> Reroll()
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

            var datalist = new List<EquipmentData>();
            for (var i = 0; i < REROLL_EQUIPMENT_COUNT; i++)
            {
                var probData = randomPicker.RandomPick();
                randomPicker.Remove(probData);

                var equipment = StaticDataManager.Instance.GetEquipmentData(probData.EquipmentID);
                datalist.Add(equipment);
            }

            return datalist;
        }


        private void Save()
        {

        }

        private void Load()
        {

        }
    }
}
