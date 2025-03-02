using System;
using System.Collections;
using System.Collections.Generic;
using BabyNightmare.InventorySystem;
using BabyNightmare.StaticData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare.Match
{
    public class MatchViewContext
    {
        public RenderTexture RT { get; }
        public EquipmentData InitEquipment { get; }
        public Func<List<EquipmentData>> GetRerollData { get; }
        public Action StartWave { get; }
        public Action<EquipmentData> OnCoolDown { get; }

        public MatchViewContext(
        RenderTexture rt,
        EquipmentData initEquipment,
        Func<List<EquipmentData>> getRerollData,
        Action startWave,
        Action<EquipmentData> onCooldown)
        {
            this.RT = rt;
            this.InitEquipment = initEquipment;
            this.GetRerollData = getRerollData;
            this.StartWave = startWave;
            this.OnCoolDown = onCooldown;
        }
    }

    public class MatchView : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RawImage _fieldIMG;
        [SerializeField] private Image _progressIMG;
        [SerializeField] private TextMeshProUGUI _waveTMP;
        [SerializeField] private RectTransform _topRTF;
        [SerializeField] private RectTransform _bottomRTF;
        [SerializeField] private RectTransform _bagViewRTF;
        [SerializeField] private RectTransform _rerollRTF;
        [SerializeField] private Button _rerollBTN;
        [SerializeField] private Button _fightBTN;
        [SerializeField] private Inventory _inventory;
        [SerializeField] private Inventory _outside;

        private MatchViewContext _context = null;

        public void Init(MatchViewContext context)
        {
            _context = context;

            _fieldIMG.texture = _context.RT;

            _inventory.Init(_canvas);
            _outside.Init(_canvas);

            _inventory.TryAdd(_context.InitEquipment);
        }

        public void RefreshWave(int curWave, int maxWave)
        {
            _waveTMP.text = $"{curWave}/{maxWave}";
            _progressIMG.fillAmount = curWave / maxWave;
            _rerollBTN.gameObject.SetActive(true);
            _fightBTN.gameObject.SetActive(true);
            _inventory.StopCoolDown();
        }

        public void OnClickReroll()
        {
            _outside.RemoveAll();

            var equipmentList = _context.GetRerollData?.Invoke();

            for (var i = 0; i < equipmentList.Count; i++)
                _outside.TryAdd(equipmentList[i]);
        }

        public void OnClickFight()
        {
            _rerollBTN.gameObject.SetActive(false);
            _fightBTN.gameObject.SetActive(false);

            _outside.RemoveAll();
            _inventory.StartCoolDownLoop(_context.OnCoolDown);

            _context.StartWave?.Invoke();
        }
    }
}