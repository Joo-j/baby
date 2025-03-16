using System.Collections;
using System.Collections.Generic;
using BabyNightmare.Talent;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare
{
    public class TalentItemView : MonoBehaviour
    {
        [SerializeField] private GameObject _lockGO;
        [SerializeField] private GameObject _focusGO;
        [SerializeField] private TextMeshProUGUI _titleTMP;
        [SerializeField] private TextMeshProUGUI _levelTMP;
        [SerializeField] private Image _icon;

        private const string PATH_ICON = "Talent/Icon/ICN_";
        private TalentData _data = null;

        public void Init(TalentData data)
        {
            _data = data;
            _icon.sprite = Resources.Load<Sprite>($"{PATH_ICON}{data.TalentType}");

            RefreshLevel(0);
            Focus(false);
        }

        public void RefreshLevel(int level)
        {
            if (level == 0)
            {
                _lockGO.SetActive(true);
                _titleTMP.text = "Lock";
                return;
            }

            _lockGO.SetActive(false);
            _titleTMP.text = $"{_data.TalentType}";
            _levelTMP.text = $"{level}";
        }

        public void Focus(bool on)
        {
            _focusGO.SetActive(on);
        }
    }
}