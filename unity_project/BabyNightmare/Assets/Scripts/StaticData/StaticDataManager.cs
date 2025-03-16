using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    public class StaticDataManager : SingletonBase<StaticDataManager>
    {
        private const string PATH_LOBBY_BUTTON_DATA = "StaticData/LobbyButtonData";
        private const string PATH_WAVE_DATA = "StaticData/WaveData";
        private const string PATH_EQUIPMENT_DATA = "StaticData/EquipmentData";
        private const string PATH_EQUIPMENT_PROB_DATA = "StaticData/EquipmentProbData";
        private const string PATH_EQUIPMENT_BOX_DATA = "StaticData/EquipmentBoxData";
        private const string PATH_ENEMY_DATA = "StaticData/EnemyData";
        private const string PATH_ENEMY_SPAWN_DATA = "StaticData/EnemySpawnData";

        private Dictionary<ELobbyButtonType, LobbyButtonData> _lobbyButtonDict = null;
        private Dictionary<int, List<WaveData>> _waveDataDict = null;
        private Dictionary<int, EquipmentData> _equipmentDataDict = null;
        private Dictionary<int, EquipmentProbData> _equipmentProbDataDict = null;
        private Dictionary<int, EquipmentBoxData> _equipmentBoxDataDict = null;
        private Dictionary<int, EnemyData> _enemyDataDict = null;
        private Dictionary<int, EnemySpawnData> _enemySpawnDataDict = null;

        private int _lastChapter = 0;

        public void Init()
        {
            InitLobbyButtonData();
            InitWaveData();
            InitEquipmentData();
            InitEquipmentProbData();
            InitEquipmentBoxData();
            InitEnemyData();
            InitEnemySpawnData();
        }

        private void InitLobbyButtonData()
        {
            var lobbyButtonDataArr = Resources.LoadAll<LobbyButtonData>(PATH_LOBBY_BUTTON_DATA);
            if (null == lobbyButtonDataArr || lobbyButtonDataArr.Length == 0)
            {
                Debug.LogError($"{PATH_LOBBY_BUTTON_DATA}에 데이터가 없습니다.");
                return;
            }

            _lobbyButtonDict = new Dictionary<ELobbyButtonType, LobbyButtonData>();

            for (var i = 0; i < lobbyButtonDataArr.Length; i++)
            {
                var buttonData = lobbyButtonDataArr[i];

                if (false == _lobbyButtonDict.ContainsKey(buttonData.ButtonType))
                    _lobbyButtonDict.Add(buttonData.ButtonType, buttonData);
            }
        }

        private void InitWaveData()
        {
            var waveDataArr = Resources.LoadAll<WaveData>(PATH_WAVE_DATA);
            if (null == waveDataArr || waveDataArr.Length == 0)
            {
                Debug.LogError($"{PATH_WAVE_DATA}에 데이터가 없습니다.");
                return;
            }

            _waveDataDict = new Dictionary<int, List<WaveData>>();

            for (var i = 0; i < waveDataArr.Length; i++)
            {
                var waveData = waveDataArr[i];

                var chapter = waveData.Chapter;
                if (false == _waveDataDict.ContainsKey(chapter))
                    _waveDataDict.Add(chapter, new List<WaveData>());

                _waveDataDict[chapter].Add(waveData);

                _lastChapter = Mathf.Max(_lastChapter, chapter);
            }
        }

        private void InitEquipmentData()
        {
            var equipmentDataArr = Resources.LoadAll<EquipmentData>(PATH_EQUIPMENT_DATA);
            if (null == equipmentDataArr || equipmentDataArr.Length == 0)
            {
                Debug.LogError($"{PATH_EQUIPMENT_DATA}에 데이터가 없습니다.");
                return;
            }

            _equipmentDataDict = new Dictionary<int, EquipmentData>();

            for (var i = 0; i < equipmentDataArr.Length; i++)
            {
                var data = equipmentDataArr[i];
                _equipmentDataDict.Add(data.ID, data);
            }
        }

        private void InitEquipmentProbData()
        {
            var equipmentProbDataArr = Resources.LoadAll<EquipmentProbData>(PATH_EQUIPMENT_PROB_DATA);
            if (null == equipmentProbDataArr || equipmentProbDataArr.Length == 0)
            {
                Debug.LogError($"{PATH_EQUIPMENT_PROB_DATA}에 데이터가 없습니다.");
                return;
            }

            _equipmentProbDataDict = new Dictionary<int, EquipmentProbData>();

            for (var i = 0; i < equipmentProbDataArr.Length; i++)
            {
                var data = equipmentProbDataArr[i];
                var id = data.ID;

                if (false == _equipmentProbDataDict.ContainsKey(id))
                    _equipmentProbDataDict.Add(id, data);
            }
        }

        private void InitEquipmentBoxData()
        {
            var equipmentBoxDataArr = Resources.LoadAll<EquipmentBoxData>(PATH_EQUIPMENT_BOX_DATA);
            if (null == equipmentBoxDataArr || equipmentBoxDataArr.Length == 0)
            {
                Debug.LogError($"{PATH_EQUIPMENT_BOX_DATA}에 데이터가 없습니다.");
                return;
            }

            _equipmentBoxDataDict = new Dictionary<int, EquipmentBoxData>();

            for (var i = 0; i < equipmentBoxDataArr.Length; i++)
            {
                var data = equipmentBoxDataArr[i];
                _equipmentBoxDataDict.Add(data.ID, data);
            }
        }

        private void InitEnemyData()
        {
            var enemyDataArr = Resources.LoadAll<EnemyData>(PATH_ENEMY_DATA);
            if (null == enemyDataArr || enemyDataArr.Length == 0)
            {
                Debug.LogError($"{PATH_ENEMY_DATA}에 데이터가 없습니다.");
                return;
            }

            _enemyDataDict = new Dictionary<int, EnemyData>();

            for (var i = 0; i < enemyDataArr.Length; i++)
            {
                var data = enemyDataArr[i];
                _enemyDataDict.Add(data.ID, data);
            }
        }

        private void InitEnemySpawnData()
        {
            var enemySpawnDataArr = Resources.LoadAll<EnemySpawnData>(PATH_ENEMY_SPAWN_DATA);
            if (null == enemySpawnDataArr || enemySpawnDataArr.Length == 0)
            {
                Debug.LogError($"{PATH_ENEMY_SPAWN_DATA}에 데이터가 없습니다.");
                return;
            }

            _enemySpawnDataDict = new Dictionary<int, EnemySpawnData>();

            for (var i = 0; i < enemySpawnDataArr.Length; i++)
            {
                var data = enemySpawnDataArr[i];
                var id = data.ID;

                if (false == _enemySpawnDataDict.ContainsKey(id))
                    _enemySpawnDataDict.Add(id, data);
            }
        }

        public LobbyButtonData GetLobbyButtonData(ELobbyButtonType type)
        {
            if (false == _lobbyButtonDict.TryGetValue(type, out var data))
                return null;

            return data;
        }

        public List<WaveData> GetWaveDataList(int chapter)
        {
            if (false == _waveDataDict.TryGetValue(chapter, out var waveDataList))
            {
                Debug.Log($"{chapter} Chapter가 없어 마지막 챕터 {_lastChapter} 데이터로 대체");
                return _waveDataDict[_lastChapter];
            }

            return waveDataList;
        }


        public EquipmentData GetEquipmentData(int id)
        {
            if (false == _equipmentDataDict.TryGetValue(id, out var data))
                return null;

            return data;
        }

        public EquipmentProbData GetEquipmentProbData(int id)
        {
            if (false == _equipmentProbDataDict.TryGetValue(id, out var data))
            {
                Debug.LogError($"{id} equipment prob data is null");
                return null;
            }

            return data;
        }


        public EquipmentBoxData GetEquipmentBoxDataList(int id)
        {
            if (false == _equipmentBoxDataDict.TryGetValue(id, out var boxData))
            {
                Debug.LogError($"{id} box data is not exist");
                return null;
            }

            return boxData;
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

        public EnemySpawnData GetEnemySpawnData(int id)
        {
            if (false == _enemySpawnDataDict.TryGetValue(id, out var data))
            {
                Debug.LogError($"{id} enemy spawn prob data is null");
                return null;
            }

            return data;
        }

    }
}