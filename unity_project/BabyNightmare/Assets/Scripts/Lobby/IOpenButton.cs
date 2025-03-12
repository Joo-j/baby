using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare.Lobby
{
    public interface IOpenButton
    {
        public GameObject GO { get; }
        public RectTransform RTF { get; }
        public Transform Icon { get; }
        public Image Mask { get; }
        public Transform Gradation { get; }
        public Transform Title { get; }
        public CanvasGroup CanvasGroup { get; }

        public void BeforeOpen();
        public void AfterOpen();
        public void Open(bool immediate, Action doneCallback);
        public void ShowGuide(bool force, Action guideCallback = null);
    }
}
