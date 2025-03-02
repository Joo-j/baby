using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Supercent.Util
{
    public static class FrameHelper
    {
        private class OnChangeFrameAction : UnityEvent<float> { }

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private static _Comp _comp     = null;
        private static bool  _useAvg   = false;
        private static int   _avgCount = 0;

        private static OnChangeFrameAction _onChangeFrameAction = new OnChangeFrameAction();

        private static ScreenLogPrinter.IPrinterHandler _printer = null;
        

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public static bool  Visible { get; set; }           = true; 
        public static float FPS     { get; private set; }   = 0f;
        public static float MinFPS  { get; private set; }   = float.MaxValue;
        public static float MaxFPS  { get; private set; }   = 0f;

        public static float OldAvgFPS   { get; private set; } = 0f; 
        public static float AvgFPS      { get; private set; } = 0f;
        public static bool  ValidAvgFPS { get; private set; } = false;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public static void Init(bool useAvg = false, int avgCount = 10)
        {
            _useAvg   = useAvg;
            _avgCount = avgCount;

            if (null != _comp)
                return;
                
            _printer = ScreenLogPrinter.Instance.GetPrinterHandler("FPS");

            var go = new GameObject("[FrameHelper]");
            if (null == go)
            {
                Debug.LogError("[FrameHelper.Init] 프레임 헬퍼 오브젝트를 생성하지 못했습니다.");
                return;
            }

            UnityEngine.Object.DontDestroyOnLoad(go);

            _comp = go.AddComponent<_Comp>();
            if (null == _comp)
            {
                Debug.LogError("[FrameHelper.Init] 프레임 헬퍼 컴포넌트를 생성하지 못했습니다.");
                return;
            }
        }

        public static void Clear()
        {
            FPS    = 0f;
            MaxFPS = 0f;
            MinFPS = 300.0f;
        }

        public static void AddOnChangeFrameListener(UnityAction<float> listener)
        {
            _onChangeFrameAction.AddListener(listener);
        }

        public static void RemoveOnChangeFrameListener(UnityAction<float> listener)
        {
            _onChangeFrameAction.RemoveListener(listener);
        }

        public static void RemoveallOnChangeFrameListeners()
        {
            _onChangeFrameAction.RemoveAllListeners();
        }

        //------------------------------------------------------------------------------
        // _Comp
        //------------------------------------------------------------------------------
        private class _Comp : MonoBehaviour
        {
            private float _temp = 0f;
            private float _prevRealtimeSinceStartup = 0f;

            private bool _useAvg = false;
            private List<float> _avgFpsList = null;
            private int _avgIndex = 0;
            private float _sumFps = 0f;

            private void Start() 
            {
                _useAvg = FrameHelper._useAvg && 0 < FrameHelper._avgCount;

                if (_useAvg)
                {
                    _avgFpsList?.Clear();
                    _avgFpsList = new List<float>();

                    for (int i = 0, size = FrameHelper._avgCount; i < size; ++i)
                        _avgFpsList.Add(0f);

                    _avgIndex = 0;
                    _sumFps = 0f;
                }
            }

            private void Update() 
            {
                // float deltaTime = Time.unscaledDeltaTime;
                float currRealtimeStartup = Time.realtimeSinceStartup;
                float deltaTime = currRealtimeStartup - _prevRealtimeSinceStartup;
                _prevRealtimeSinceStartup = currRealtimeStartup;

                _temp += (deltaTime - _temp);

                var fps = 1f / _temp;;
                
                FrameHelper.FPS = fps;

                if (FrameHelper.MaxFPS < fps)
                    FrameHelper.MaxFPS = fps;

                if (fps < FrameHelper.MinFPS)
                    FrameHelper.MinFPS = fps;

                if (FrameHelper._useAvg && 0 < FrameHelper._avgCount)
                {
                    var count = _avgFpsList.Count;

                    _sumFps -= _avgFpsList[_avgIndex];
                    _avgFpsList[_avgIndex] = fps;
                    _sumFps += _avgFpsList[_avgIndex];

                    FrameHelper.AvgFPS = _sumFps / (float)count;

                    ++_avgIndex;
                    if (count <= _avgIndex)
                    {
                        _avgIndex = 0;

                        if (!FrameHelper.ValidAvgFPS)
                        {
                            FrameHelper.Clear();
                            FrameHelper.ValidAvgFPS = true;
                            FrameHelper.OldAvgFPS = FrameHelper.AvgFPS;
                        }
                    }

                    if (ValidAvgFPS)
                    {
                        var dist = FrameHelper.OldAvgFPS - FrameHelper.AvgFPS;
                        if (dist <= -1f || 1f <= dist)
                        {
                            FrameHelper.OldAvgFPS = FrameHelper.AvgFPS;
                            FrameHelper._onChangeFrameAction?.Invoke(FrameHelper.AvgFPS);
                        }
                    }
                }

                if (!FrameHelper.Visible)
                {
                    _printer.Log = string.Empty;
                    return;
                }

                if (true == _useAvg)
                    _printer.Log = $"MIN: {Math.Round(MinFPS, 1)},  MAX: {Math.Round(MaxFPS, 1)},  AVG: {Math.Round(AvgFPS, 1)}";
                else
                    _printer.Log = $"MIN: {Math.Round(MinFPS, 1)},  MAX: {Math.Round(MaxFPS, 1)},  FPS: {Math.Round(AvgFPS, 1)}";
            }
        }
    }
}
