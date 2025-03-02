using UnityEngine;

namespace Supercent.Util
{
    public static class ScreenShotUtil
    {
        //------------------------------------------------------------------------------
        // enums
        //------------------------------------------------------------------------------
        private enum EOutputPostProcessing
        {
            None,
            ChangeScale,
        }

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        /// <summary>
        /// 현재 씬 (카메라 배경을 제외한) png 파일로 저장합니다.
        /// </summary>
        public static void SaveScene(string folder, string fileName, Camera camera, int outputSize)
        {
            // 파라메터 확인
            if (string.IsNullOrEmpty(folder))
            {
                Debug.LogError("[ScreenShotUtil.SaveScene] 폴더 경로가 정상적이지 않습니다.");
                return;
            }

            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            if (folder[folder.Length - 1] == '/')
                folder = folder.Substring(0, folder.Length - 1);

            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("[ScreenShotUtil.SaveScene] 파일 이름이 지정되지 않았습니다.");
                return;
            }

            var temp = fileName.LastIndexOf(".png");
            if (-1 == temp)
                fileName += ".png";

            if (null == camera)
            {
                Debug.LogError("[ScreenShotUtil.SaveScene] 카메라가 지정되지 않았습니다.");
                return;
            }

            // 스크린 샷
            var renderTextureSize = 1024;
            var postProcessing    = EOutputPostProcessing.None;

            if (renderTextureSize < outputSize)
            {
                renderTextureSize = outputSize;
            }
            else if (outputSize < renderTextureSize)
            {
                renderTextureSize = outputSize;
                postProcessing    = EOutputPostProcessing.ChangeScale;
            }

            // 카메라 렌더
            RenderTexture renderTexture = null;
            if (null != camera.targetTexture)
            {
                renderTexture = camera.targetTexture;
            }
            else
            {
                renderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 32, RenderTextureFormat.ARGB32);
                renderTexture.wrapMode   = TextureWrapMode.Clamp;
                renderTexture.filterMode = FilterMode.Bilinear;

                camera.targetTexture = renderTexture;
                camera.Render();
            }

            RenderTexture.active = renderTexture;

            // 렌더한 화면 텍스쳐에 옮기기
            var texture = new Texture2D(renderTextureSize, renderTextureSize, TextureFormat.ARGB32, false);
            texture.wrapMode   = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.ReadPixels(new Rect(0, 0, renderTextureSize, renderTextureSize), 0, 0);
            texture.Apply();

            // 텍스쳐 스케일 변경
            if (EOutputPostProcessing.ChangeScale == postProcessing)
                TextureScaleUtil.ChangeScale(texture, outputSize, outputSize);

            // 파일로 저장
            var bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(folder + "/" + fileName, bytes);

            // 렌더 텍스쳐 반환
            RenderTexture.active = null;
            camera.targetTexture = null;

            Debug.Log($"<color=#00ff55>화면 저장을 완료했습니다.</color>\n{folder}/{fileName}");
        }
    }
}