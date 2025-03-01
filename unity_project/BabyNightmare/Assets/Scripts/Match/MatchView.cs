using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BabyNightmare.StaticData;
using BabyNightmare.InventorySystem;

namespace BabyNightmare.Match
{
    public class MatchViewContext
    {
        public Action OnClickReroll { get; }
        public Action OnClickFight { get; }

        public MatchViewContext(
        Action _onClickReroll,
        Action _onClickFight)
        {
            this.OnClickReroll = _onClickReroll;
            this.OnClickFight = _onClickFight;
        }
    }

    public class MatchView : MonoBehaviour
    {
        [SerializeField] private Image _progressIMG;
        [SerializeField] private TextMeshProUGUI _waveTMP;
        [SerializeField] private RectTransform _topRTF;
        [SerializeField] private RectTransform _bottomRTF;
        [SerializeField] private RectTransform _bagViewRTF;
        [SerializeField] private RectTransform _rerollRTF;
        [SerializeField] private Button _rerollBTN;
        [SerializeField] private Button _fightBTN;

        private MatchViewContext _context = null;

        public RectTransform BagViewRTF => _bagViewRTF;
        public RectTransform RerollRTF => _rerollRTF;

        public void Init(MatchViewContext context)
        {
            _context = context;
        }

        public void SetActiveButtons(bool active)
        {
            _rerollBTN.gameObject.SetActive(active);
            _fightBTN.gameObject.SetActive(active);
        }

        public void RefreshWave(int curWave, int maxWave)
        {
            _waveTMP.text = $"{curWave}/{maxWave}";
            _progressIMG.fillAmount = curWave / maxWave;
        }

        public void OnClickReroll()
        {
            Debug.Assert(null != _context, "context is null");

            _context.OnClickReroll?.Invoke();
        }

        public void OnClickFight()
        {
            Debug.Assert(null != _context, "context is null");

            _context.OnClickFight?.Invoke();
        }

    }
}