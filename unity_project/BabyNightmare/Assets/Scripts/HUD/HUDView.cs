using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace BabyNightmare.HUD
{
    public class HUDView : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup _layout;

        private Dictionary<EHUDType, IHUD> _hudDict = null;

        public void Init()
        {
            var hudArr = _layout.GetComponentsInChildren<IHUD>();
            if (null == hudArr || hudArr.Length == 0)
            {
                Debug.LogError($"계층 구조에 HUD가 없습니다.");
                return;
            }

            _hudDict = new Dictionary<EHUDType, IHUD>();

            for (var i = 0; i < hudArr.Length; i++)
            {
                var hud = hudArr[i];
                _hudDict.Add(hud.Type, hud);
            }
        }

        public void Show(bool enableShortcut)
        {
            gameObject.SetActive(true);

            foreach (var pair in _hudDict)
            {
                var hud = pair.Value;
                hud.EnableShortcut(enableShortcut);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ActiveHUD(EHUDType type, bool active)
        {
            if (false == _hudDict.TryGetValue(type, out var hud))
                return;

            hud.GO.SetActive(active);
        }
    }
}