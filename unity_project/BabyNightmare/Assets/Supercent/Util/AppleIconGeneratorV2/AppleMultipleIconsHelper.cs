using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Supercent.Util.AppleIconGeneratorV2
{
    public static class AppleMultipleIconsHelper
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private static List<string> _assetFolderPathes = null;
        private static List<string> _assetFolderNames  = null;
        private static string       _allAssetNames     = string.Empty;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
#if IS_PARTNER_PROJECT
        public static string RootFolder       => $"{Application.dataPath}/../AppleIcons";
#else        
        public static string RootFolder       => $"{Application.dataPath}/../../../AppleIcons";
#endif
        public static string SettingsFilePath => $"{RootFolder}/IconSettings.json";
        public static string ConfigFilePath   => $"{RootFolder}/IconConfig.txt";

        public static bool UseMultipleIcons
        {
            get
            {
                LoadAssetNames();

                return null != _assetFolderPathes && 0 < _assetFolderPathes.Count;
            }
        }

        public static string AssetNames
        {
            get
            {
                LoadAssetNames();
                
                return _allAssetNames;
            }
        }

        private static void LoadAssetNames()
        {
            if (null != _assetFolderPathes)
                return;

            if (!Directory.Exists(RootFolder) || !File.Exists(ConfigFilePath))
                return;

            var text = string.Empty;

            try
            {
                text = File.ReadAllText(ConfigFilePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AppleMultipleIconsHelper.LoadAssetNames] 예외 발생. {ex.ToString()}");
                return;
            }

            var lines = text.Split('\n');
            if (null == lines || 0 == lines.Length)
                return;

            _assetFolderPathes = new List<string>();
            _assetFolderNames  = new List<string>();

            var assetNamesSB = new StringBuilder();

            for (int i = 0, size = lines.Length; i < size; ++i)
            {
                if (string.IsNullOrEmpty(lines[i]))
                    continue;

                if (0 < assetNamesSB.Length)
                    assetNamesSB.Append(" ");
                assetNamesSB.Append(lines[i].Replace(".appiconset", ""));

                var folderName = $"{RootFolder}/{lines[i]}";
                if (folderName.IsNullOrEmpty() || !Directory.Exists(folderName))
                    continue;

                _assetFolderPathes.Add(folderName);
                _assetFolderNames.Add(lines[i]);
            }

            if (0 < _assetFolderPathes.Count)
                assetNamesSB.Append(" AppIcon");

            _allAssetNames = assetNamesSB.ToString();
        }

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public static void CopyAssetsToXCode(string xCodeAssetFolderPath)
        {
            LoadAssetNames();

            if (null == _assetFolderPathes || 0 == _assetFolderPathes.Count)
                return;

            for (int i = 0, size = _assetFolderPathes.Count; i < size; ++i)
                _CopyAllFiles(_assetFolderPathes[i], Path.Combine(xCodeAssetFolderPath, _assetFolderNames[i]));
        }

        private static void _CopyAllFiles(string sourceFolder, string targetFolder)
        {
            if (!Directory.Exists(sourceFolder))
                return;

            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            foreach (string file in Directory.GetFiles(sourceFolder))
            {
                var fileName = Path.GetFileName(file);
                var destName = Path.Combine(targetFolder, fileName);
                File.Copy(file, destName, true);
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Supercent/Util/애플 앱 아이콘/아이콘 에셋 확인")]
        private static void CheckAssetFolderNames()
        {
            _assetFolderPathes = null;

            LoadAssetNames();

            if (null == _assetFolderPathes || 0 == _assetFolderPathes.Count)
            {
                Debug.Log($"[AppleMultipleIconHelper] 사용중인 아이콘 정보가 없습니다.".Orange());
                return;
            }

            for (int i = 0, size = _assetFolderPathes?.Count ?? 0; i < size; ++i)
                Debug.Log($"[AppleMultipleIconHelper] name: {_assetFolderNames[i]},  path: {_assetFolderPathes[i]}".Orange());

            Debug.Log($"[AppleMultipleIconHelper] All asset names: {_allAssetNames}".Orange());
        }
#endif        
    }
}