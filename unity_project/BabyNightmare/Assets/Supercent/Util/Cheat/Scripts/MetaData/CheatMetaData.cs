using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Supercent.Util.Cheat.MetaData
{
    [Serializable]
    public class CheatMetaData
    {
        private static readonly string FILE_PATH = Path.Combine(Application.persistentDataPath, "cheat_metafile.json");
        private static LogClassPrinter _logPrinter = new LogClassPrinter(nameof(CheatMetaData), "2266bb");
        
        [SerializeField] private int _lastSelectGroupIndex;
        [SerializeField] private List<CheatStatusData> _cheatStatusDatas = new List<CheatStatusData>();
        
        public int LastSelectGroupIndex
        {
            get => _lastSelectGroupIndex;
            set
            {
                _lastSelectGroupIndex = value;
                SaveMetaData();
            }
        }
        
        public static CheatMetaData CreateMetaData()
        {
            if (!File.Exists(FILE_PATH))
                return new CheatMetaData();

            try
            {
                string jsonData = File.ReadAllText(FILE_PATH);
                return JsonUtility.FromJson<CheatMetaData>(jsonData);
            }
            catch (Exception e)
            {
                _logPrinter.Error(nameof(CreateMetaData), $"Failed to load CheatMetaData: {e.Message}");
            }
                
            return new CheatMetaData();
        }

        public string GetStatus(string groupName, string cheatName)
        {
            var cheatData = GetorCreateCheatItemData(groupName, cheatName);

            return cheatData.Status;
        }

        public void SetStatus(string groupName, string cheatName, string status)
        {
            var cheatData = GetorCreateCheatItemData(groupName, cheatName);

            cheatData.Status = status;
            SaveMetaData();
        }
        
        private CheatStatusData GetorCreateCheatItemData(string groupName, string cheatName)
        {
            var groupCheatName = $"{groupName}_{cheatName}";
            int groupCheatHashCode = groupCheatName.GetHashCode();
            for (int i = 0; i < _cheatStatusDatas.Count; i++)
            {
                var cheatStatusData = _cheatStatusDatas[i];
                if (cheatStatusData.GroupCheatHash == groupCheatHashCode)
                    return cheatStatusData;
            }
            CheatStatusData result = new CheatStatusData() { GroupCheatHash = groupCheatHashCode, Status = string.Empty };
            _cheatStatusDatas.Add(result);
            return result;
        }

        private void SaveMetaData()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(this, true);
                File.WriteAllText(FILE_PATH, jsonData);
            }
            catch (Exception e)
            {
                _logPrinter.Error(nameof(SaveMetaData), $"Failed to save CheatMetaData: {e.Message}");
            }
        }
    }
}