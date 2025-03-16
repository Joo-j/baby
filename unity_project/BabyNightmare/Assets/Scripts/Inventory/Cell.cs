using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare.InventorySystem
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Image _image;

        public RectTransform RTF => _rtf;
        public Action OnClickAction { get; set; }

        public void RefreshColor(Color color)
        {
            _image.color = color;
        }   

        public void OnClickButton()
        {
            OnClickAction?.Invoke();
        }
    }
}