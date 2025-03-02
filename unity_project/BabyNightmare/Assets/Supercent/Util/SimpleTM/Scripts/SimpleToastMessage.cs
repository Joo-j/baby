using System;
using TMPro;
using UnityEngine;

namespace Supercent.Util.STM
{
    public static class SimpleToastMessage
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private static SimpleToastMessageSettings _settings = null;
        private static SimpleToastMessageView     _view = null;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public static void SetFont(TMP_FontAsset fontAsset, Material fontMaterial)
        {
            Init();

            _view.SetFont(fontAsset, fontMaterial);
        }

        public static void Show(string message, Action doneCallback)
        {
            if (string.IsNullOrEmpty(message))
                return;

            Init();

            _view?.Show(message, doneCallback);
        }

        private static void Init()
        {
            if (null != _settings)
                return;

            _settings = Resources.Load<SimpleToastMessageSettings>("Supercent/SimpleToastMessageSettings");
            if (null == _settings)
            {
                Debug.LogError("[SimpleToastMessage.Init] 셋팅 파일을 로드하지 못했습니다.");
                return;
            }
            
            _view = Supercent.Util.ObjectUtil.LoadAndInstantiate<SimpleToastMessageView>("SimpleToastMessageView", null);
            if (null == _view)
            {
                Debug.LogError("[SimpleToastMessage.Init] 토스트 팝업 프리팹을 로드하지 못했습니다.");
                return;
            }

            UnityEngine.Object.DontDestroyOnLoad(_view.gameObject);
            _view.Init(_settings.PositionOffset, _settings.BgSprite, _settings.BgColor, _settings.FontAsset, _settings.FontMaterial);
            _view.SetSortingOrder(_settings.SortingOrder);
        }
    }
}