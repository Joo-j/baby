using System;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using Unity.VisualScripting;

namespace BabyNightmare.Util
{
    public static class TempViewHelper
    {
        public static Canvas GetTouchBlocker()
        {
            var viewPath = "Util/TouchBlocker";
            var touchBlocker = ObjectUtil.LoadAndInstantiate<Canvas>(viewPath, null);
            if (null == touchBlocker)
            {
                Debug.LogError($"{viewPath} 경로에서 프리팹을 찾을 수 없습니다.");
                return null;
            }

            return touchBlocker;
        }

        public static void ShowLoadingView(Action doneCallback)
        {
            var viewPath = "Util/LoadingView";
            var loadingView = ObjectUtil.LoadAndInstantiate<LoadingView>(viewPath, null);
            if (null == loadingView)
            {
                Debug.LogError($"{viewPath} 경로에서 프리팹을 찾을 수 없습니다.");
                return;
            }

            loadingView.Init(doneCallback);
        }
    }
}

