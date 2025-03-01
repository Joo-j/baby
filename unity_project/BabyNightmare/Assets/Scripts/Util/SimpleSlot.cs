using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare.Util
{
    public class SimpleSlot : MonoBehaviour
    {
        [SerializeField] private Image _bg;

        public MonoBehaviour Owner
        {
            get;
            set;
        }

        public bool Enabled
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }
    }
}
