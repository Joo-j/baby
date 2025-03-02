using System.Collections.Generic;
using UnityEngine;

namespace Supercent.Util
{
    public class ScreenLogPrinter
    {
        public interface IPrinterHandler
        {
            string Key { get; }
            string Log { get; set; }
        }

        private class PrinterHandler : IPrinterHandler
        {
            public string Key { get; private set; } = string.Empty;
            public string Log { get; set; } = string.Empty;

            public PrinterHandler(string key) => Key = key;
        }

        //------------------------------------------------------------------------------
        // singleton
        //------------------------------------------------------------------------------
        private ScreenLogPrinter() {}
        private static ScreenLogPrinter _inst = null;

        public static ScreenLogPrinter Instance
        {
            get 
            {
                if (null != _inst)
                    return _inst;

                _inst = new ScreenLogPrinter();
                _inst.Init();
                return _inst;
            }
        }

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private _Printer _printer = null;
        
        private bool _useDebugLogScreen = true;
        private int _fontSize = 25;
        private float _spacingNormal = 0.025f;
        private float _heightNormal = 0.02f;

        private Dictionary<int, PrinterHandler> _handlerSet = new Dictionary<int, PrinterHandler>();
        private List<PrinterHandler> _handlers = new List<PrinterHandler>();


        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public bool UseDebugLogScreen
        {
            get => _useDebugLogScreen;
            set
            {
                if (value != _useDebugLogScreen)
                {
                    if (true == value)
                        CreatePrinter();
                    else
                        DestroyPrinter();
                }

                _useDebugLogScreen = value;
            }
        }

        public int FontSize
        {
            get => _fontSize;
            set => _fontSize = value;
        }

        public float SpacingNormal
        {
            get => _spacingNormal;
            set => _spacingNormal = value;
        }

        public float HeightNormal
        {
            get => _heightNormal;
            set => _heightNormal = value;
        }

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public IPrinterHandler GetPrinterHandler(string key)
        {
            var hash = key.GetHashCode();
            if (false == _handlerSet.TryGetValue(hash, out var handler))
            {
                handler = new PrinterHandler(key);
                _handlerSet.Add(hash, handler);
                _handlers.Add(handler);
            }

            return handler;
        }

        private void Init()
        {
#if AD_MONETIZATION            
            if (true != Supercent.BuildTools.BuildSetting.IS_DEV)
                return;
#endif

            CreatePrinter();
        }

        private void CreatePrinter()
        {
            if (null != _printer || false == UseDebugLogScreen)
                return;

            var go = new GameObject("DISDebugScreenPrinter");
            if (null == go)
                return;

            _printer = go.AddComponent<_Printer>();
            if (null == _printer)
                return;

            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        private void DestroyPrinter()
        {
            if (null == _printer)
                return;

            UnityEngine.Object.Destroy(_printer.gameObject);
            _printer = null;
        }


        //------------------------------------------------------------------------------
        // _Printer
        //------------------------------------------------------------------------------
        private class _Printer : MonoBehaviour
        {
            private Vector2[] _shadowOffsets = new Vector2[]
        {
            new Vector2(-2f, -2f),
            new Vector2(-2f, +2f),
            new Vector2(+2f, -2f),
            new Vector2(+2f, +2f),
        };

            private Color[] _colors = new Color[]
            {
                Color.yellow, 
                Color.cyan, 
                Color.green,
                Color.magenta,
                Color.white,
                Color.blue,
            };

            private void OnGUI()
            {
                if (null == _inst
                    || false == _inst.UseDebugLogScreen
                    || null == _inst._handlers
                    || 0 == _inst._handlers.Count)
                    return;

                var screenWidth             = Screen.width;
                var screenHeight            = Screen.height;
                var safeArea                = Screen.safeArea;
                var additionalTopPadding    = 0.0f;

                if (!Mathf.Approximately(safeArea.height, screenHeight))
                    additionalTopPadding = screenHeight - safeArea.y - safeArea.height;

                var style = new GUIStyle()
                {
                    alignment = TextAnchor.UpperLeft,
                    fontSize  = _inst._fontSize,
                };

                var topPadding      = 15.0f + additionalTopPadding;
                var leftPadding     = 15.0f;
                var spacingY        = screenHeight * _inst._spacingNormal;
                var width           = screenWidth * 0.1f;
                var height          = screenHeight * _inst._heightNormal;

                for (int handlerIndex = 0, colorIndex = 0, handlerSize = _inst._handlers.Count, colorSize = _colors.Length; handlerIndex < handlerSize; ++handlerIndex)
                {
                    style.normal.textColor = Color.black;
                    style.fontStyle = FontStyle.Bold;

                    var handler = _inst._handlers[handlerIndex];
                    var log     = $"[{handler.Key}] {handler.Log}";
                    var y       = spacingY * handlerIndex + topPadding;

                    // shadow
                    for (int i = 0, size = _shadowOffsets.Length; i < size; ++i)
                        GUI.Label(new Rect(leftPadding + _shadowOffsets[i].x, y + _shadowOffsets[i].y, width, height), log, style);

                    // text
                    style.normal.textColor = _colors[colorIndex];
                    GUI.Label(new Rect(leftPadding, y, width, height), log, style);

                    // increase color index
                    ++colorIndex;
                    if (colorSize <= colorIndex)
                        colorIndex = 0;
                }
            }
        }
    }
}