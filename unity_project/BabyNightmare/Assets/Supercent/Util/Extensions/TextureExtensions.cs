using UnityEngine;
using UnityEngine.Rendering;

namespace Supercent.Util
{
    public static class TextureExtension
    {
        static readonly bool IsCoordLB = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore
                                      || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2
                                      || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3
                                      || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal;

        #region Capture
        public static void Capture(this RenderTexture texture, Camera camera)
        {
            if (texture == null) { Debug.LogAssertion($"{nameof(Capture)} : {nameof(texture)} is null"); return; }
            if (camera == null) { Debug.LogAssertion($"{nameof(Capture)} : {nameof(camera)} is null"); return; }

            CaptureJob(camera, texture);
        }
        public static void Capture(this RenderTexture texture, Camera camera, int cullingMask)
        {
            if (texture == null) { Debug.LogAssertion($"{nameof(Capture)} : {nameof(texture)} is null"); return; }
            if (camera == null) { Debug.LogAssertion($"{nameof(Capture)} : {nameof(camera)} is null"); return; }

            int oldCullingMask = camera.cullingMask;
            bool notEqauls = cullingMask == oldCullingMask;
            if (!notEqauls)
                camera.cullingMask = cullingMask;

            CaptureJob(camera, texture);

            if (!notEqauls)
                camera.cullingMask = oldCullingMask;
        }

        static void CaptureJob(Camera camera, RenderTexture texture)
        {
            var texOld = camera.targetTexture;
            bool notEquals = texOld != texture;
            if (notEquals)
                camera.targetTexture = texture;

            bool disabled = !camera.enabled;
            if (disabled) camera.enabled = true;
            camera.Render();
            if (disabled) camera.enabled = false;

            if (notEquals)
                camera.targetTexture = texOld;
        }
        #endregion// Capture


        #region Picture
        public static void Picture(this RenderTexture src, Texture2D dst) => src.Picture(dst, 0, 0, 0, 0);
        public static void Picture(this RenderTexture src, Texture2D dst, int srcX, int srcY) => src.Picture(dst, srcX, srcY, 0, 0);
        public static void Picture(this RenderTexture src, Texture2D dst, int srcX, int srcY, int dstX, int dstY)
        {
            if (src == null) { Debug.LogAssertion($"{nameof(Picture)} : {nameof(src)} is null"); return; }
            if (dst == null) { Debug.LogAssertion($"{nameof(Picture)} : {nameof(dst)} is null"); return; }
            if (!dst.isReadable) { Debug.LogAssertion($"{nameof(Picture)} : {dst.name} is not readable"); return; }

            PictureJob(src, dst, new Rect(srcX, srcY, src.width - srcX, src.height - srcY), dstX, dstY);
        }

        public static void Picture(this RenderTexture src, Texture2D dst, Rect srcRect) => src.Picture(dst, srcRect, 0, 0);
        public static void Picture(this RenderTexture src, Texture2D dst, Rect srcRect, int dstX, int dstY)
        {
            if (src == null) { Debug.LogAssertion($"{nameof(Picture)} : {nameof(src)} is null"); return; }
            if (dst == null) { Debug.LogAssertion($"{nameof(Picture)} : {nameof(dst)} is null"); return; }
            if (!dst.isReadable) { Debug.LogAssertion($"{nameof(Picture)} : {dst.name} is not readable"); return; }

            PictureJob(src, dst, srcRect, dstX, dstY);
        }

        static void PictureJob(RenderTexture src, Texture2D dst, Rect srcRect, int dstX, int dstY)
        {
            var texOld = RenderTexture.active;
            bool notEqauls = texOld != src;
            if (notEqauls)
                RenderTexture.active = src;

            if (IsCoordLB)
                srcRect.y = dst.height - (int)(srcRect.y + srcRect.height);

            dst.ReadPixels(srcRect, dstX, dstY, false);
            dst.Apply(false, false);

            if (notEqauls)
                RenderTexture.active = texOld;
        }
        #endregion// Picture
    }
}