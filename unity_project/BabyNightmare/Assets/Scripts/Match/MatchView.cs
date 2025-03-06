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
        public Action OnClickReroll { get; }
        public Action StartWave { get; }
        public Action<EquipmentData> OnCoolDown { get; }

        public MatchViewContext(
        RenderTexture rt,
        EquipmentData initEquipment,
        Action onClickReroll,
        Action startWave,
        Action<EquipmentData> onCooldown)
        {
            this.RT = rt;
            this.InitEquipment = initEquipment;
            this.OnClickReroll = onClickReroll;
            this.StartWave = startWave;
            this.OnCoolDown = onCooldown;
        }
    }

    public class MatchView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private RectTransform _topRTF;
        [SerializeField] private RectTransform _botRTF;
        [SerializeField] private RawImage _fieldIMG;
        [SerializeField] private SimpleProgress _waveProgress;
        [SerializeField] private Inventory _inventory;
        [SerializeField] private Inventory _outside;
        [SerializeField] private float _noMatchTopHeight = 640;
        [SerializeField] private float _matchTopHeight = 820;
        [SerializeField] private GameObject _startButtonGO;
        [SerializeField] private GameObject _bottomButtonsGO;
        [SerializeField] private Image _rerollBG;
        [SerializeField] private TextMeshProUGUI _rerollCostTMP;
        [SerializeField] private GameObject _boxButtonGO;
        [SerializeField] private Image _boxIMG;
        [SerializeField] private TextMeshProUGUI _hpTMP;
        [SerializeField] private TextMeshProUGUI _attackTMP;
        [SerializeField] private TextMeshProUGUI _defTMP;

        private const string PATH_BOX_ICON = "Match/UI/ICN_Box_";
        private MatchViewContext _context = null;
        private Dictionary<EStatType, float> _statDict = null;
        private EquipmentBoxData _boxData = null;

        public void Init(MatchViewContext context)
        {
            _context = context;
            _statDict = new Dictionary<EStatType, float>();

            foreach (EStatType type in Enum.GetValues(typeof(EStatType)))
                _statDict.Add(type, 0f);

            RefreshStat();

            _fieldIMG.texture = _context.RT;

            _inventory.Init(_rtf, OnEquip, OnUnequip);
            _outside.Init(_rtf, null, null);

            _inventory.TryAdd(_context.InitEquipment);

            _bottomButtonsGO.SetActive(false);
            _boxButtonGO.SetActive(false);
            _canvasGroup.blocksRaycasts = false;

            _startButtonGO.SetActive(true);
        }

        public void RefreshProgress(int curWave, int maxWave, bool immediate)
        {
            _waveProgress.Refresh(curWave, maxWave, immediate);
        }

        public void Reroll(List<EquipmentData> dataList)
        {
            _outside.RemoveAll();

            for (var i = 0; i < dataList.Count; i++)
            {
                _outside.TryAdd(dataList[i]);
            }
        }

        public void RefreshRerollCost(int cost, bool purchasable)
        {
            if (cost <= 0)
                _rerollCostTMP.text = $"FREE";
            else
                _rerollCostTMP.text = $"{cost}";

            _rerollBG.color = purchasable ? Color.white : Color.red;
        }

        public void OnClickStart()
        {
            _startButtonGO.SetActive(false);
            _bottomButtonsGO.SetActive(true);
            _canvasGroup.blocksRaycasts = true;
        }

        public void ShowBox(EquipmentBoxData boxData)
        {
            AdaptTopSize(false, false);

            _boxData = boxData;
            var iconPath = $"{PATH_BOX_ICON}{boxData.Type}";
            _boxIMG.sprite = Resources.Load<Sprite>(iconPath);
            _boxButtonGO.SetActive(true);
        }

        public void OnClearWave()
        {
            _inventory.StopUseEquipment();
            _canvasGroup.blocksRaycasts = true;
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

            _boxData = null;
            _bottomButtonsGO.SetActive(true);
        }

        public void OnClickReroll()
        {
            _context.OnClickReroll?.Invoke();
        }

        public void OnClickFight()
        {
            _bottomButtonsGO.SetActive(false);
            _outside.RemoveAll();
            _inventory.StartUseEquipment(_context.OnCoolDown);
            _canvasGroup.blocksRaycasts = false;

            AdaptTopSize(true, false);

            _context.StartWave?.Invoke();
        }

        public void AdaptTopSize(bool onMatch, bool immediate)
        {
            var startSize = onMatch ? new Vector2(_topRTF.sizeDelta.x, _matchTopHeight) : new Vector2(_topRTF.sizeDelta.x, _noMatchTopHeight);
            var targetSize = onMatch ? new Vector2(_topRTF.sizeDelta.x, _matchTopHeight) : new Vector2(_topRTF.sizeDelta.x, _noMatchTopHeight);

            if (_topRTF.sizeDelta == targetSize)
                return;

            if (true == immediate)
            {
                _topRTF.sizeDelta = targetSize;
                return;
            }

            StartCoroutine(Co_LerpSizeTop(startSize, targetSize, 0.4f));
        }


        private void OnEquip(EquipmentData data)
        {
            _statDict[EStatType.HP] += data.Heal;
            _statDict[EStatType.ATK] += data.Damage;
            _statDict[EStatType.DEF] += data.Defence;

            RefreshStat();
        }

        private void OnUnequip(EquipmentData data)
        {
            _statDict[EStatType.HP] -= data.Heal;
            _statDict[EStatType.ATK] -= data.Damage;
            _statDict[EStatType.DEF] -= data.Defence;

            RefreshStat();
        }

        private void RefreshStat()
        {
            _hpTMP.text = $"{_statDict[EStatType.HP]}/s";
            _attackTMP.text = $"{_statDict[EStatType.ATK]}/s";
            _defTMP.text = $"{_statDict[EStatType.DEF]}/s";
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