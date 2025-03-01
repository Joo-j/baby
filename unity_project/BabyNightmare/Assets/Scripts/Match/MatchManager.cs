using System;
using System.Collections.Generic;
using BabyNightmare.Util;
using UnityEngine;
using BabyNightmare.StaticData;
using BabyNightmare.InventorySystem;

namespace BabyNightmare.Match
{
    public class MatchManager : MonoBehaviour
    {
        private static MatchManager _instance = null;
        public static MatchManager Instance
        {
            get
            {
                if (null == _instance)
                    _instance = new MatchManager();

                return _instance;
            }
        }

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
        private List<Equipment> _rerollItems = new List<Equipment>();

        public void Init(Action enterLobby)
        {
            Load();

            _enterLobby = enterLobby;
        }

        public void StartMatch()
        {
            _waveDataList = StaticDataManager.Instance.GetWaveDataList(PlayerData.Stage);
            if (null == _waveDataList)
            {
                Debug.LogError($"{PlayerData.Stage}stage no wave data");
                return;
            }
            _currentWave = 0;
            _maxWave = _waveDataList.Count;

            var fieldRes = Resources.Load<MatchField>(PATH_MATCH_FIELD);
            _matchField = GameObject.Instantiate(fieldRes);
            _matchField.Init(OnClearWave, OnFailMatch);

            var viewRes = Resources.Load<MatchView>(PATH_MATCH_VIEW);
            _matchView = GameObject.Instantiate(viewRes);

            var matchViewContext = new MatchViewContext(Reroll, OnStartWave);
            _matchView.Init(matchViewContext);

            Reroll();
        }


        private void Reroll()
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

            var equipmentDataList = new List<EquipmentData>();
            for (var i = 0; i < REROLL_EQUIPMENT_COUNT; i++)
            {
                var probData = randomPicker.RandomPick();
                randomPicker.Remove(probData);

                var equipmentData = StaticDataManager.Instance.GetEquipmentData(probData.EquipmentID);

                equipmentDataList.Add(equipmentData);
            }

            ClearOutside();
        }


        private void ClearOutside()
        {
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

            ++PlayerData.Stage;
            PlayerData.Save();
        }

        private void CloseMatch()
        {
            Destroy(_matchField.gameObject);
            _matchField = null;

            Destroy(_matchView.gameObject);
            _matchView = null;

            _enterLobby?.Invoke();
        }

        private void OnStartWave()
        {
            var waveData = _waveDataList[_currentWave];

            var group = waveData.EnemySpawnDataGroup;
            var enemySpanwDataList = StaticDataManager.Instance.GetEnemySpawnDataList(group);

            var enemyDataList = new List<EnemyData>();
            for (var i = 0; i < enemySpanwDataList.Count; i++)
            {
                var spawnData = enemySpanwDataList[i];
                var enemyData = StaticDataManager.Instance.GetEnemyData(spawnData.EnemyID);
                enemyDataList.Add(enemyData);
            }

            _matchField.StartWave(enemyDataList);
            _matchView.SetActiveButtons(false);

            ClearOutside();
        }

        private void OnClearWave()
        {
            ++_currentWave;

            if (_currentWave >= _maxWave)
            {
                OnCompleteMatch();
                return;
            }

            _matchView.RefreshWave(_currentWave, _maxWave);
            _matchView.SetActiveButtons(true);
        }


        private void ShowInfoPopup(EquipmentData equipmentData)
        {

        }

        private void Save()
        {

        }

        private void Load()
        {

        }
    }
}
