using System.Threading;
using UnityEngine;

namespace Supercent.Util
{
    public static class TextureScaleUtil
    {
        //------------------------------------------------------------------------------
        // threadData
        //------------------------------------------------------------------------------
        public class ThreadData
        {
            public int Start;
            public int End;
            
            public ThreadData(int start, int end)
            {
                Start = start;
                End   = end;
            }
        }

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private static Color[] _textureColors = null;
        private static Color[] _newColors = null;

        private static Vector2 _ratio = Vector2.zero;

        private static int _orgWidth = 0;
        private static int _targetWidth = 0;

        private static int _finishCount = 0;
        private static Mutex _mutex = null;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public static void ChangeScale(Texture2D texture, int width, int height, bool useBilinear = true)
        {
            // 파라메터 확인
            if (null == texture)
            {
                Debug.LogError("[TextureScaleUtil.ChangeScale] 텍스쳐가 정상적이지 않습니다.");
                return;
            }

            if (0 == width || 0 == height)
            {
                Debug.LogError($"[TextureScaleUtil.ChangeScale] {(0 == width ? "width" : "height")} 가 정상적이지 않습니다.");
                return;
            }

            // 정보 셋팅
            _textureColors = texture.GetPixels();
            _newColors     = new Color[width * height];

            if (useBilinear)
            {
                _ratio.x = 1f / ((float)width / (float)(texture.width - 1));
                _ratio.y = 1f / ((float)height / (float)(texture.height - 1));
            }
            else
            {
                _ratio.x = (float)texture.width / (float)width;
                _ratio.y = (float)texture.height / (float)height;
            }

            _orgWidth    = texture.width;
            _targetWidth = width;

            var coresCount = Mathf.Min(SystemInfo.processorCount, height);
            var sliceCount = height / coresCount;

            _finishCount = 0;

            if (null == _mutex)
                _mutex = new Mutex(false);

            if (1 < coresCount)
            {
                // 멀티 쓰레드 사용 가능한 경우
                int i = 0;

                _finishCount = coresCount;
                
                for (int size = coresCount - 1; i < size; ++i)
                {
                    var threadData = new ThreadData(sliceCount * i, sliceCount * (i + 1));
                    var threadStart = useBilinear
                                    ? new ParameterizedThreadStart(BilinearScale)
                                    : new ParameterizedThreadStart(PointScale);
                    
                    var thread = new Thread(threadStart);
                    thread.Start(threadData);
                }

                var lastThreadData = new ThreadData(sliceCount * i, height);
                
                if (useBilinear)
                    BilinearScale(lastThreadData);
                else
                    PointScale(lastThreadData);

                Debug.Log("Scaleing Process Begin.");

                while (0 < _finishCount)
                    Thread.Sleep(1);

                Debug.Log("Scaleing Process End.");
            }
            else
            {
                // 싱글 쓰레드만 사용 가능한 경우
                var threadData = new ThreadData(0, height);
                if (useBilinear)
                    BilinearScale(threadData);
                else
                    PointScale(threadData);
            }

#if UNITY_2021_2 || UNITY_2021_3 || UNITY_2022
            texture.Reinitialize(width, height);
#else // UNITY_2021_2 || UNITY_2021_3 || UNITY_2022
            texture.Resize(width, height);
#endif // UNITY_2021_2 || UNITY_2021_3 || UNITY_2022
            texture.SetPixels(_newColors);
            texture.Apply();

            _textureColors = null;
            _newColors     = null;
        }

        private static void BilinearScale(System.Object obj)
        {
            if (null == obj)
            {
                _mutex.WaitOne();
                _finishCount -= 1;
                _mutex.ReleaseMutex();

                Debug.LogError("obj is null");
                return;
            }

            if (!(obj is ThreadData td))
            {
                _mutex.WaitOne();
                _finishCount -= 1;
                _mutex.ReleaseMutex();

                Debug.LogError("변환 실패!");
                return;
            }

            var textureColorSize = _textureColors.Length;

            for (int y = td.Start, size = td.End; y < size; ++y)
            {
                var floorY = Mathf.FloorToInt(y * _ratio.y);
                var y1     = floorY * _orgWidth;
                var y2     = (floorY + 1) * _orgWidth;
                var yw     = y * _targetWidth;

                for (int x = 0; x < _targetWidth; ++x)
                {
                    var floorX = Mathf.FloorToInt(x * _ratio.x);
                    var lerpX  = x * _ratio.x - floorX;

                    if (textureColorSize <= y1 + floorX)
                        Debug.LogError(textureColorSize + ", " + y1 + ", " + floorX);

                    if (textureColorSize <= y1 + floorX + 1)
                        Debug.LogError(textureColorSize + ", " + y1 + ", " + (floorX + 1));

                    if (textureColorSize <= y2 + floorX)
                        Debug.LogError(textureColorSize + ", " + y2 + ", " + floorX);

                    if (textureColorSize<= y2 + floorX + 1)
                        Debug.LogError(textureColorSize + ", " + y2 + ", " + (floorX + 1));
                    
                    var color1 = ColorLerpUnclamped(_textureColors[y1 + floorX],
                                                    _textureColors[y1 + floorX + 1],
                                                    lerpX);
                    var color2 = ColorLerpUnclamped(_textureColors[y2 + floorX],
                                                    _textureColors[y2 + floorX + 1],
                                                    lerpX);

                    _newColors[yw + x] = ColorLerpUnclamped(color1, color2, y * _ratio.y - floorY);
                }

                _mutex.WaitOne();
                _finishCount -= 1;
                _mutex.ReleaseMutex();
            }
        }

        private static void PointScale(System.Object obj)
        {
            if (!(obj is ThreadData td))
            {
                _mutex.WaitOne();
                _finishCount -= 1;
                _mutex.ReleaseMutex();
                return;
            }

            for (int y = td.Start, size = td.End; y < size; ++y)
            {
                var thisY = (int)(_ratio.y * y) * _orgWidth;
                var yw    = y * _targetWidth;

                for (int x = 0; x < _targetWidth; ++x)
                    _newColors[yw + x] = _textureColors[(int)(thisY + _ratio.x * x)];
            }

            _mutex.WaitOne();
            _finishCount -= 1;
            _mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped (Color c1, Color c2, float value)
        {
            return new Color (c1.r + (c2.r - c1.r) * value, 
                            c1.g + (c2.g - c1.g) * value, 
                            c1.b + (c2.b - c1.b) * value, 
                            c1.a + (c2.a - c1.a) * value);
        }
    }
}