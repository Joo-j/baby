using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Application;

namespace Supercent.Util.Editor
{
    public class Menu : ScriptableObject
    {
        static readonly string CAPTURE_DIR = $"{Application.dataPath}/../../../capture/";



        [MenuItem("Supercent/Util/Clear PlayerPrefab")]
        static void Clear_PlayerPrefab()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Supercent/Util/Delete persistentDataPath")]
        static void Delete_PersistentDataPath()
        {
            Directory.Delete(Application.persistentDataPath, true);
        }

        [MenuItem("Supercent/Util/Screen Capture")]
        static void CurrentScreenCapture01() => ScreenCaptureJob(1);
        [MenuItem("Supercent/Util/Screen Capture x4 &C")]
        static void CurrentScreenCapture04() => ScreenCaptureJob(4);

        static void ScreenCaptureJob(int superSize)
        {
            if (!Directory.Exists(CAPTURE_DIR))
                Directory.CreateDirectory(CAPTURE_DIR);

            var filename = $"{CAPTURE_DIR}ScreenCapture_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            ScreenCapture.CaptureScreenshot(filename, superSize);
            Debug.Log($"Screen Capture : {filename}".Green());
        }

        [MenuItem("Supercent/Util/Camera Capture")]
        static void CurrentCameraCapture01() => CameraCaptureJob(1f);
        [MenuItem("Supercent/Util/Camera Capture x4 #&C")]
        static void CurrentCameraCapture04() => CameraCaptureJob(4f);

        static void CameraCaptureJob(float ratio)
        {
            if (ratio <= 0)
            {
                Debug.LogAssertion($"{nameof(CameraCaptureJob)} : {nameof(ratio)}({ratio}) <= 0");
                return;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                Debug.Log("$Camera Capture : Not found main camera");
                return;
            }
            if (!cam.enabled)
            {
                Debug.Log("$Camera Capture : Disabled main camera");
                return;
            }

            if (!Directory.Exists(CAPTURE_DIR))
                Directory.CreateDirectory(CAPTURE_DIR);

            var filename = $"{CAPTURE_DIR}CameraCapture_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            var binPng  = default(byte[]);
            var rtex    = RenderTexture.GetTemporary((int)(Screen.width * ratio),
                                                     (int)(Screen.height * ratio),
                                                     32,
                                                     RenderTextureFormat.ARGB32);
            {
                var rtexOld = cam.targetTexture;
                cam.targetTexture = rtex;
                cam.Render();
                cam.targetTexture = rtexOld;

                rtexOld = RenderTexture.active;
                RenderTexture.active = rtex;
                var tex = new Texture2D(rtex.width, rtex.height, TextureFormat.RGBA32, false, true);
                tex.ReadPixels(new Rect(0f, 0f, rtex.width, rtex.height), 0, 0, false);
                tex.Apply(false, false);
                RenderTexture.active = rtexOld;

                binPng = tex.EncodeToPNG();
            }
            RenderTexture.ReleaseTemporary(rtex);

            File.WriteAllBytes(filename, binPng);
            Debug.Log($"Camera Capture : {filename}".Green());
        }
    }
}