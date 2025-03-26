using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    public class StaticDataManager : SingletonBase<StaticDataManager>
    {
        private const string PATH_LOBBY_BUTTON_DATA = "StaticData/LobbyButtonData";
        private const string PATH_CHAPTER_DATA = "StaticData/ChapterData";
        private const string PATH_WAVE_DATA = "StaticData/WaveData";
        private const string PATH_EQUIPMENT_DATA = "StaticData/EquipmentData";
        private const string PATH_ENEMY_DATA = "StaticData/EnemyData";
        private const string PATH_ENEMY_SPAWN_DATA = "StaticData/EnemySpawnData";

        private Dictionary<ELobbyButtonType, LobbyButtonData> _lobbyButtonDict = null;
        private Dictionary<int, ChapterData> _chapterDataDict = null;
        private Dictionary<int, List<WaveData>> _waveDataDict = null;
        private Dictionary<int, EquipmentData> _equipmentDataDict = null;
        private Dictionary<EEnemyType, EnemyData> _enemyDataDict = null;
        private Dictionary<int, EnemySpawnData> _enemySpawnDataDict = null;

        public int LastChapter { get; private set; }

        public List<EquipmentData> EquipmentDataList => _equipmentDataDict.Values.ToList();

        public void Init()
        {
            InitLobbyButtonData();
            InitChapterData();
            InitWaveData();
            InitEquipmentData();
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

        private void InitChapterData()
        {
            var chapterDataArr = Resources.LoadAll<ChapterData>(PATH_CHAPTER_DATA);
            if (null == chapterDataArr || chapterDataArr.Length == 0)
            {
                Debug.LogError($"{PATH_CHAPTER_DATA}에 데이터가 없습니다.");
                return;
            }

            _chapterDataDict = new Dictionary<int, ChapterData>();
            for (var i = 0; i < chapterDataArr.Length; i++)
            {
                var chapterData = chapterDataArr[i];

                var chapter = chapterData.Chapter;
                if (false == _chapterDataDict.ContainsKey(chapter))
                    _chapterDataDict.Add(chapter, chapterData);

                LastChapter = Mathf.Max(LastChapter, chapter);
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

                var group = waveData.Group;
                if (false == _waveDataDict.ContainsKey(group))
                    _waveDataDict.Add(group, new List<WaveData>());

                _waveDataDict[group].Add(waveData);

                LastChapter = Mathf.Max(LastChapter, group);
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

        private void InitEnemyData()
        {
            var enemyDataArr = Resources.LoadAll<EnemyData>(PATH_ENEMY_DATA);
            if (null == enemyDataArr || enemyDataArr.Length == 0)
            {
                Debug.LogError($"{PATH_ENEMY_DATA}에 데이터가 없습니다.");
                return;
            }

            _enemyDataDict = new Dictionary<EEnemyType, EnemyData>();

            for (var i = 0; i < enemyDataArr.Length; i++)
            {
                var data = enemyDataArr[i];
                _enemyDataDict.Add(data.Type, data);
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

        public ChapterData GetChapterData(int chapter)
        {
            if (false == _chapterDataDict.TryGetValue(chapter, out var chapterData))
            {
                return _chapterDataDict[LastChapter];
            }

            return chapterData;
        }

        public List<WaveData> GetWaveDataList(int group)
        {
            if (false == _waveDataDict.TryGetValue(group, out var waveDataList))
                return null;

            return waveDataList;
        }

        public EquipmentData GetEquipmentData(int id)
        {
            if (false == _equipmentDataDict.TryGetValue(id, out var data))
                return null;

            return data;
        }

        public EnemyData GetEnemyData(EEnemyType type)
        {
            if (false == _enemyDataDict.TryGetValue(type, out var data))
            {
                Debug.LogError($"{type} enemy data is null");
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