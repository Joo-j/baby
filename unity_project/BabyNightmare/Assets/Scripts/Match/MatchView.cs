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

            var canvasRect = _canvas.transform as RectTransform;
            _inventory.Init(canvasRect);
            _outside.Init(canvasRect);

            _inventory.TryAdd(_context.InitEquipment);
        }

        public void RefreshWave(int curWave, int maxWave, bool immediate)
        {
            _waveProgress.Refresh(curWave, maxWave, immediate);
            _inventory.StopUseEquipment();

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
        public void ShowBox(EquipmentBoxData boxData)
        {
            AdaptTopSize(false, false);

            _boxData = boxData;
            var iconPath = $"{PATH_BOX_ICON}{boxData.Type}";
            _boxIMG.sprite = Resources.Load<Sprite>(iconPath);
            _boxButtonGO.SetActive(true);
        }

        public void OnClickBox()
        {
            if (null == _boxData)
            {
                Debug.LogError("no box data");
                return;
            }

            _boxButtonGO.SetActive(false);

            _outside.RemoveAll();

            var equipmentList = _context.GetRerollData?.Invoke();
            var equipmentIDList = _boxData.EquipmentIDList;
            var dataList = new List<EquipmentData>();
            for (var i = 0; i < equipmentIDList.Count; i++)
            {
                var equipment = StaticDataManager.Instance.GetEquipmentData(equipmentIDList[i]);
                dataList.Add(equipment);
            }

            for (var i = 0; i < dataList.Count; i++)
            {
                _outside.TryAdd(dataList[i]);
            }

            for (var i = 0; i < equipmentList.Count; i++)
                _outside.TryAdd(equipmentList[i]);
            _boxData = null;
            _bottomButtonsGO.SetActive(true);
        }
        }

        public void OnClickFight()
        {
            _outside.RemoveAll();
            _inventory.StartUseEquipment(_context.OnCoolDown);

            var startSize = new Vector2(_topRTF.sizeDelta.x, _noMatchTopHeight);
            var targetSize = new Vector2(_topRTF.sizeDelta.x, _matchTopHeight);
            StartCoroutine(Co_LerpSizeTop(startSize, targetSize, 0.4f));

            _context.StartWave?.Invoke();
        }

        public void ShowBox(EBoxType type)
        {
            var path = $"{PATH_BOX_ICON}{type}";
            _boxIMG.sprite = Resources.Load<Sprite>(path);
            Debug.Assert(null != _boxIMG.sprite, $"{path} no sprite");

            _boxButtonGO.SetActive(true);
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