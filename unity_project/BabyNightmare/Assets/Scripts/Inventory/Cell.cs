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
        public Image Image => _image;
        public Action OnClickAction { get; set; }

        public void OnClickButton()
        {
            OnClickAction?.Invoke();
        }
    }
}