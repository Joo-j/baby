using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    public class StaticDataManager : SingletonBase<StaticDataManager>
    {
        private const string PATH_STATIC_DATA_SHEET = "StaticData/StaticDataSheet";
        private StaticDataSheet _sheet = null;

        private Dictionary<int, List<WaveData>> _waveDataDict = null;
        private Dictionary<int, EquipmentData> _equipmentDataDict = null;
        private Dictionary<int, List<EquipmentProbData>> _equipmentProbDataDict = null;
        private Dictionary<int, EnemyData> _enemyDataDict = null;
        private Dictionary<int, List<EnemySpawnData>> _enemySpawnDataDict = null;

        public void Init()
        {
            _sheet = Resources.Load<StaticDataSheet>(PATH_STATIC_DATA_SHEET);
            if (null == _sheet)
            {
                Debug.LogError($"{PATH_STATIC_DATA_SHEET} no prefab");
                return;
            }

            InitWaveData();
            InitEquipmentData();
            InitEquipmentProbData();
            InitEnemyData();
            InitEnemySpawnData();
        }

        private void InitWaveData()
        {
            _waveDataDict = new Dictionary<int, List<WaveData>>();

            var waveDataList = _sheet.WaveDataList;

            for (var i = 0; i < waveDataList.Count; i++)
            {
                var waveData = waveDataList[i];

                var stage = waveData.Stage;
                if (false == _waveDataDict.ContainsKey(stage))
                    _waveDataDict.Add(stage, new List<WaveData>());

                _waveDataDict[stage].Add(waveData);
            }
        }

        private void InitEquipmentData()
        {
            _equipmentDataDict = new Dictionary<int, EquipmentData>();

            var equipmentDataList = _sheet.EquipmentDataList;

            for (var i = 0; i < equipmentDataList.Count; i++)
            {
                var data = equipmentDataList[i];
                _equipmentDataDict.Add(data.ID, data);
            }
        }

        private void InitEquipmentProbData()
        {
            _equipmentProbDataDict = new Dictionary<int, List<EquipmentProbData>>();

            var equipmentProbDataList = _sheet.EquipmentProbDataList;

            for (var i = 0; i < equipmentProbDataList.Count; i++)
            {
                var data = equipmentProbDataList[i];
                var group = data.Group;

                if (false == _equipmentProbDataDict.ContainsKey(group))
                    _equipmentProbDataDict.Add(group, new List<EquipmentProbData>());

                _equipmentProbDataDict[group].Add(data);
            }
        }

        private void InitEnemyData()
        {
            _enemyDataDict = new Dictionary<int, EnemyData>();

            var enemyDataList = _sheet.EnemyDataList;

            for (var i = 0; i < enemyDataList.Count; i++)
            {
                var data = enemyDataList[i];
                _enemyDataDict.Add(data.ID, data);
            }
        }

        private void InitEnemySpawnData()
        {
            _enemySpawnDataDict = new Dictionary<int, List<EnemySpawnData>>();

            var enemySpanwDataList = _sheet.EnemySpawnDataList;

            for (var i = 0; i < enemySpanwDataList.Count; i++)
            {
                var data = enemySpanwDataList[i];
                var group = data.Group;

                if (false == _enemySpawnDataDict.ContainsKey(group))
                    _enemySpawnDataDict.Add(group, new List<EnemySpawnData>());

                _enemySpawnDataDict[group].Add(data);
            }
        }

        public List<WaveData> GetWaveDataList(int stage)
        {
            if (false == _waveDataDict.TryGetValue(stage, out var waveDataList))
            {
                Debug.LogError($"{stage} wave is not exist");
                return null;
            }

            return waveDataList;
        }


        public EquipmentData GetEquipmentData(int id)
        {
            if (false == _equipmentDataDict.TryGetValue(id, out var data))
            {
                Debug.LogError($"{id} equipment data is null");
                return null;
            }

            return data;
        }

        public List<EquipmentProbData> GetEquipmentProbDataList(int group)
        {
            if (false == _equipmentProbDataDict.TryGetValue(group, out var dataList))
            {
                Debug.LogError($"{group} equipment prob data is null");
                return null;
            }

            return dataList;
        }

        public EnemyData GetEnemyData(int id)
        {
            if (false == _enemyDataDict.TryGetValue(id, out var data))
            {
                Debug.LogError($"{id} enemy data is null");
                return null;
            }

            return data;
        }

        public List<EnemySpawnData> GetEnemySpawnDataList(int group)
        {
            if (false == _enemySpawnDataDict.TryGetValue(group, out var dataList))
            {
                Debug.LogError($"{group} enemy spawn prob data is null");
                return null;
            }

            return dataList;
        }
    }
}