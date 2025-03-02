using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace Supercent.Util
{
    public static class ModelFormatUtil
    {
        const ModelImporterMeshCompression DEFAULT_MeshCompression = ModelImporterMeshCompression.Medium;

        static readonly string NL = Environment.NewLine;



        static string[] GetSelectedFolderPathes()
        {
            var guids = Selection.assetGUIDs;
            if (null == guids)
                return null;

            var targetPathes = new List<string>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!Directory.Exists(path))
                    continue;

                targetPathes.Add(path);
            }

            return targetPathes.ToArray();
        }

        [MenuItem("Assets/Supercent/Quick Model Batch Settings", true)]
        static bool ValidateModelBatchSettings() => null != GetSelectedFolderPathes();

        [MenuItem("Assets/Supercent/Quick Model Batch Settings", false, 1)]
        static void ModelBatchSettings()
        {
            var pathes = GetSelectedFolderPathes();
            if (null == pathes)
                return;

            if (pathes.Length <= 0)
                return;

            var fileGUIDs = AssetDatabase.FindAssets("t:Model", pathes);
            var importers = new List<ModelImporter>(fileGUIDs.Length);

            for (var index = 0; index < fileGUIDs.Length; ++index)
            {
                var pathTex = AssetDatabase.GUIDToAssetPath(fileGUIDs[index]);
                if (ModelImporter.GetAtPath(pathTex) is ModelImporter importer)
                {
                    importers.Add(importer);
                }
            }

            if (importers.Count < 1)
            {
                EditorUtility.DisplayDialog
                (
                    "Info",
                    $"Not found models{NL}{NL}",
                    "Ok"
                );
                return;
            }

            var pathStr = string.Empty;
            if (1 < pathes.Length)
                pathStr = $"{pathes[0]} and {pathes.Length - 1} others";
            else
                pathStr = $"{pathes[0]}";

            var result = EditorUtility.DisplayDialog
            (
                $"Info",
                $"Are you sure you want to the model settings?{NL}{NL}" +
                $"Model : {importers.Count}{NL}" +
                $"Path : {pathStr}{NL}",
                "Yes",
                "No"
            );
            if (result)
            {
                AssetDatabase.StartAssetEditing();
                _IterateJob(0);
                AssetDatabase.StopAssetEditing();
                EditorUtility.DisplayDialog("Info", "Done", "Ok");
            }


            void _IterateJob(int _index)
            {
                ModelImporter _importer = null;
                try
                {
                    for (; _index < importers.Count; ++_index)
                    {
                        _importer = importers[_index];
                        SetSupercentModel(_importer);
                        _importer.SaveAndReimport();
                    }
                    return;
                }
                catch (Exception error)
                {
                    var _path = _importer == null ? string.Empty : _importer.assetPath;
                    Debug.LogError($"{nameof(ModelBatchSettings)} : Setting error ({_path}){NL}{error}");
                }
                _importer = null;

                _IterateJob(++_index);
            }
        }

        static void SetSupercentModel(ModelImporter importer)
        {
            if (importer == null)
                return;

            importer.meshCompression = DEFAULT_MeshCompression;
        }

    }
}
