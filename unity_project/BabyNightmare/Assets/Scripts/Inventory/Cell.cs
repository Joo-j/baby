using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BabyNightmare.InventorySystem
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Image _image;

        public RectTransform RTF => _rtf;

        public void RefreshColor(Color color)
        {
            _image.color = color;
        }

        public void AddButton(UnityAction callback)
        {
            var button = gameObject.AddComponent<Button>();
            button.onClick.AddListener(callback);
        }
    }
}