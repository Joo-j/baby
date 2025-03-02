using System;
using TMPro;
using UnityEngine;

namespace Supercent.Util.STM
{
    public class SimpleToastMessageView : MonoBehaviour
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private RectTransform _rtfAlign;
        [SerializeField] private SimpleToastMessageItemView _itemView;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private Vector2? _orgAlignPos = null;

        private Canvas _canvas = null;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public void Init(Vector2 posOffset, Sprite bgSprite, Color bgColor, TMP_FontAsset fontAsset, Material fontMaterial)
        {
            if (null == _orgAlignPos)
                _orgAlignPos = _rtfAlign.anchoredPosition;

            _rtfAlign.anchoredPosition = _orgAlignPos.Value + posOffset;

            _itemView.Init(bgSprite, bgColor, fontAsset, fontMaterial);
        }

        public void SetFont(TMP_FontAsset fontAsset, Material fontMaterial)
        {
            _itemView.SetFont(fontAsset, fontMaterial);
        }

        public void SetSortingOrder(int order)
        {
            if (null == _canvas)
                _canvas = GetComponent<Canvas>();

            if (null != _canvas)
                _canvas.sortingOrder = order;
        }

        public void Show(string message, Action doneCallback)
        {
            _itemView.Show(message, doneCallback);
        }
    }
}