using System;
using System.Collections;
using System.Collections.Generic;
using BabyNightmare.InventorySystem;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
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
        [SerializeField] private RectTransform _topRTF;
        [SerializeField] private RectTransform _botRTF;
        [SerializeField] private RawImage _fieldIMG;
        [SerializeField] private SimpleProgress _waveProgress;
        [SerializeField] private Inventory _inventory;
        [SerializeField] private Inventory _outside;
        [SerializeField] private float _noMatchTopHeight = 640;
        [SerializeField] private float _matchTopHeight = 820;

        private MatchViewContext _context = null;

        public void Init(MatchViewContext context)
        {
            _context = context;

            _fieldIMG.texture = _context.RT;

            _inventory.Init(_canvas);
            _outside.Init(_canvas);

            _inventory.TryAddCell(_context.InitEquipment);
        }

        public void RefreshWave(int curWave, int maxWave, bool immediate)
        {
            _waveProgress.Refresh(curWave, maxWave, immediate);
            _inventory.StopCoolDown();

            var startSize = new Vector2(_topRTF.sizeDelta.x, _matchTopHeight);
            var targetSize = new Vector2(_topRTF.sizeDelta.x, _noMatchTopHeight);

            if (true == immediate)
            {
                _topRTF.sizeDelta = targetSize;
                return;
            }

            StartCoroutine(Co_LerpSizeTop(startSize, targetSize, 1f));
        }

        public void OnClickReroll()
        {
            _outside.RemoveAll();

            var equipmentList = _context.GetRerollData?.Invoke();

            for (var i = 0; i < equipmentList.Count; i++)
                _outside.TryAddCell(equipmentList[i]);
        }

        public void OnClickFight()
        {
            _outside.RemoveAll();
            _inventory.StartCoolDown(_context.OnCoolDown);

            var startSize = new Vector2(_topRTF.sizeDelta.x, _noMatchTopHeight);
            var targetSize = new Vector2(_topRTF.sizeDelta.x, _matchTopHeight);
            StartCoroutine(Co_LerpSizeTop(startSize, targetSize, 0.4f));

            _context.StartWave?.Invoke();
        }

        private IEnumerator Co_LerpSizeTop(Vector2 startSize, Vector2 targetSize, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                _topRTF.sizeDelta = Vector2.Lerp(startSize, targetSize, elapsed / duration);
            }

            _topRTF.sizeDelta = targetSize;
        }

        private void LateUpdate()
        {
            _botRTF.anchoredPosition = new Vector2(0, -_topRTF.sizeDelta.y);
        }
    }
}