using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Supercent.Util
{
    public static class TextureFormatUtil
    {
        const int TEXTURE_MAX_SIZE = 2048;

        const TextureImporterFormat AOS_DefaultFormat = TextureImporterFormat.ASTC_6x6;
        const TextureImporterFormat IOS_DefaultFormat = TextureImporterFormat.ASTC_6x6;
        const TextureImporterFormat AOS_SpriteFormat = TextureImporterFormat.ASTC_4x4;
        const TextureImporterFormat IOS_SpriteFormat = TextureImporterFormat.ASTC_4x4;

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

        [MenuItem("Assets/Supercent/Quick Texture Batch Settings", true)]
        static bool ValidateTextureBatchSettings() => null != GetSelectedFolderPathes();

        [MenuItem("Assets/Supercent/Quick Texture Batch Settings", false, 1)]
        static void TextureBatchSettings()
        {
            var pathes = GetSelectedFolderPathes();
            if (null == pathes)
                return;

            if (pathes.Length <= 0)
                return;

            var fileGUIDs = AssetDatabase.FindAssets("t:Texture", pathes);
            var importers = new List<TextureImporter>(fileGUIDs.Length);

            var cntDefault = 0;
            var cntSprite = 0;
            for (var index = 0; index < fileGUIDs.Length; ++index)
            {
                var pathTex = AssetDatabase.GUIDToAssetPath(fileGUIDs[index]);
                if (TextureImporter.GetAtPath(pathTex) is TextureImporter importer)
                {
                    switch (importer.textureType)
                    {
                    case TextureImporterType.Default:
                        ++cntDefault;
                        importers.Add(importer);
                        break;
                    case TextureImporterType.Sprite:
                        ++cntSprite;
                        importers.Add(importer);
                        break;
                    }
                }
            }

            if (importers.Count < 1)
            {
                EditorUtility.DisplayDialog
                (
                    "Info",
                    $"Not found textures{NL}{NL}",
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
                $"Are you sure you want to the texture settings?{NL}{NL}" +
                $"Texture : {cntDefault}{NL}" +
                $"Sprite : {cntSprite}{NL}" +
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
                TextureImporter _importer = null;
                try
                {
                    for (; _index < importers.Count; ++_index)
                    {
                        _importer = importers[_index];
                        SetSupercentTexture(_importer);
                        _importer.SaveAndReimport();
                    }
                    return;
                }
                catch (Exception error)
                {
                    var _path = _importer == null ? string.Empty : _importer.assetPath;
                    Debug.LogError($"{nameof(TextureBatchSettings)} : Setting error ({_path}){NL}{error}");
                }
                _importer = null;

                _IterateJob(++_index);
            }
        }

        static void SetSupercentTexture(TextureImporter importer)
        {
            if (importer == null)
                return;

            var isSprite = importer.textureType == TextureImporterType.Sprite;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            {
                if (isSprite)
                {
                    settings.mipmapEnabled = false;
                    settings.streamingMipmaps = false;
                }
                settings.spriteGenerateFallbackPhysicsShape = false;
            }
            importer.SetTextureSettings(settings);

            var isSizeOver = false;
            _SetPlatformConfig("DefaultTexturePlatform", TextureImporterFormat.RGBA32);
            _SetPlatformConfig(BuildTargetGroup.Android.ToString(), isSprite ? AOS_SpriteFormat : AOS_DefaultFormat);
            _SetPlatformConfig(BuildTargetGroup.iOS.ToString(), isSprite ? IOS_SpriteFormat : IOS_DefaultFormat);

            if (isSizeOver)
                Debug.LogError($"Exceeds {TEXTURE_MAX_SIZE:0} Size : {importer.assetPath}");


            void _SetPlatformConfig(string platformName, TextureImporterFormat format)
            {
                if (string.IsNullOrEmpty(platformName))
                    return;

                var _platform = importer.GetPlatformTextureSettings(platformName);
                if (_platform != null)
                {
                    _platform.overridden = true;
                    _platform.allowsAlphaSplitting = true;
                    _platform.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality32BitDownscaled;
                    _platform.format = format;

                    if (TEXTURE_MAX_SIZE < _platform.maxTextureSize)
                    {
                        _platform.maxTextureSize = TEXTURE_MAX_SIZE;
                        isSizeOver = true;
                    }

                    importer.SetPlatformTextureSettings(_platform);
                }
            }
        }
    }
}
