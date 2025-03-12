using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare.Util
{
    public class DimmedView : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Image _dimd;

        public RectTransform RTF => _rtf;

        public void SetAlpha(float alpha)
        {
            _dimd.color = new Color(0, 0, 0, alpha);
        }

        public void SetSortingOrder(int order)
        {
            _canvas.sortingOrder =order;
        }
    }
}
