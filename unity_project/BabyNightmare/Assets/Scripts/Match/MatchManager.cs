using System;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using BabyNightmare.HUD;
using Random = UnityEngine.Random;
using BabyNightmare.Talent;

namespace BabyNightmare.Match
{
    public class MatchManager : SingletonBase<MatchManager>
    {
        private const string PATH_MATCH_FIELD = "Match/MatchField";
        private const string PATH_MATCH_VIEW = "Match/UI/MatchView";
        private const string PATH_MATCH_FAIL_VIEW = "Match/UI/MatchFailView";
        private const string PATH_MATCH_COMPLETE_VIEW = "Match/UI/MatchCompleteView";
        private const string PATH_PROJECTILE_DATA = "StaticData/ProjectileData/ProjectileData_";

        private const int REROLL_EQUIPMENT_COUNT = 3;
        private const int REROLL_INITIAL_COST = 10;
        private const int INITIAL_GEM = 10;

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

            _matchField = ObjectUtil.LoadAndInstantiate<MatchField>(PATH_MATCH_FIELD, null);
            var matchFieldContext = new MatchFieldContext(
                                        chapterData,
                                        GetCoin,
                                        RefreshProgress,
                                        OnClearWave,
                                        OnFailMatch,
                                        GetProjectileData);
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
                                        GetUpgradeData,
                                        GetProjectileData
                                        );
            _matchView.Init(matchViewContext);
            _matchView.RefreshWave(_currentWave + 1, _maxWave);
            _matchView.RefreshProgress(0);

            PlayerData.Instance.OnChangedCoinEvent.AddListener(RefreshRerollCost);
            PlayerData.Instance.Coin = 0;
        }

        private void RefreshProgress(float factor)
        {
            _matchView?.RefreshProgress(factor);
        }

        private void OnFailMatch()
        {
            var gem = PlayerData.Instance.Chapter * INITIAL_GEM;
            var talentGem = TalentManager.Instance.GetValue(ETalentType.Gem_Earn_Percentage);
            gem += Mathf.CeilToInt(gem * talentGem);

            var failView = ObjectUtil.LoadAndInstantiate<MatchFailView>(PATH_MATCH_FAIL_VIEW, null);
            failView.Init(gem, CloseMatch);
        }

        private void OnCompleteMatch()
        {
            var gem = PlayerData.Instance.Chapter * INITIAL_GEM;
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
            _matchView.RefreshWave(_currentWave + 1, _maxWave);

            _matchField.OnClearWave(_matchView.WaveCircleTF);

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
            var probID = boxData.EquipmentProbDataID;
            var probData = StaticDataManager.Instance.GetEquipmentProbData(probID);
            var probDataList = probData.ProbDataList;

            var randomPicker = new WeightedRandomPicker<ProbData>();
            for (var i = 0; i < probDataList.Count; i++)
            {
                var data = probDataList[i];
                randomPicker.Add(data, data.Prob);
            }

            var dataList = new List<EquipmentData>();
            for (var i = 0; i < REROLL_EQUIPMENT_COUNT; i++)
            {
                var data = randomPicker.RandomPick();
                var equipment = StaticDataManager.Instance.GetEquipmentData(data.EquipmentID);
                dataList.Add(equipment);

                randomPicker.Remove(data);
            }

            _matchView.Reroll(dataList);
        }

        private void RefreshRerollCost(int coin)
        {
            _matchView?.RefreshRerollPrice(_rerollCost, coin);
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

        private ProjectileData GetProjectileData(int id)
        {
            var ptPath = $"{PATH_PROJECTILE_DATA}{id}";
            return Resources.Load<ProjectileData>(ptPath);
        }
    }
}
