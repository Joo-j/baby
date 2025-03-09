using System;
using System.Collections;
using System.Collections.Generic;
using BabyNightmare.HUD;
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
        public Func<EquipmentData, EquipmentData, EquipmentData> GetUpgradeData { get; }
        public MatchViewContext(
        RenderTexture rt,
        EquipmentData initEquipment,
        Action onClickReroll,
        Action startWave,
        Action<EquipmentData> onCooldown,
        Func<EquipmentData, EquipmentData, EquipmentData> getUpgradeData)
        {
            this.RT = rt;
            this.InitEquipment = initEquipment;
            this.OnClickReroll = onClickReroll;
            this.StartWave = startWave;
            this.OnCoolDown = onCooldown;
            this.GetUpgradeData = getUpgradeData;
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
        [SerializeField] private Inventory _inventoryInside;
        [SerializeField] private Inventory _inventoryOutside;
        [SerializeField] private Vector2 _topYPosRange = new Vector2(285, 0);
        [SerializeField] private Vector2 _botYPosRange = new Vector2(495, 780);
        [SerializeField] private GameObject _startGO;
        [SerializeField] private GameObject _rerollGO;
        [SerializeField] private GameObject _rerollGO_Normal;
        [SerializeField] private GameObject _rerollGO_Free;
        [SerializeField] private GameObject _rerollGO_AD;
        [SerializeField] private TextMeshProUGUI _rerollCostTMP;
        [SerializeField] private GameObject _fightGO;
        [SerializeField] private GameObject _boxGO;
        [SerializeField] private Image _boxIMG;
        [SerializeField] private RectTransform _hpRTF;
        [SerializeField] private RectTransform _atkRTF;
        [SerializeField] private RectTransform _defRTF;
        [SerializeField] private TextMeshProUGUI _hpTMP;
        [SerializeField] private TextMeshProUGUI _atkTMP;
        [SerializeField] private TextMeshProUGUI _defTMP;
        [SerializeField] private AnimationCurve _bounceCurve;

        private const string PATH_EQUIPMENT_BOX_ICON = "Match/EquipmentBox/ICN_Box_";
        private MatchViewContext _context = null;
        private Dictionary<EStatType, float> _statDict = null;
        private EquipmentBoxData _boxData = null;

        public RectTransform FieldImage => _fieldIMG.rectTransform;

        public void Init(MatchViewContext context)
        {
            _context = context;
            _statDict = new Dictionary<EStatType, float>();

            foreach (EStatType type in Enum.GetValues(typeof(EStatType)))
                _statDict.Add(type, 0f);

            RefreshStat();

            _fieldIMG.texture = _context.RT;

            _inventoryInside.Init(_rtf, _inventoryOutside, OnEquip, OnUnequip, context.GetUpgradeData);
            _inventoryOutside.Init(_rtf, _inventoryOutside, null, null, context.GetUpgradeData);

            _inventoryInside.TryAdd(_context.InitEquipment);

            _rerollGO.SetActive(false);
            _fightGO.SetActive(false);

            _boxGO.SetActive(false);
            _canvasGroup.blocksRaycasts = false;
            _startGO.SetActive(true);

            ChangeRectPos(false, true);
        }

        public void RefreshProgress(int curWave, int maxWave, bool immediate)
        {
            _waveProgress.Refresh(curWave, maxWave, immediate);
        }

        public void Reroll(List<EquipmentData> dataList)
        {
            _inventoryOutside.RemoveAll();

            for (var i = 0; i < dataList.Count; i++)
            {
                _inventoryOutside.TryAdd(dataList[i]);
            }
        }

        public void RefreshRerollCost(int cost, int coin)
        {
            _rerollGO_Free.SetActive(false);
            _rerollGO_AD.SetActive(false);
            _rerollGO_Normal.SetActive(false);

            _rerollCostTMP.text = $"{cost}";

            if (cost <= 0)
            {
                _rerollGO_Free.SetActive(true);
                return;
            }

            if (cost > coin)
            {
                _rerollGO_AD.SetActive(true);
                return;
            }

            _rerollGO_Normal.SetActive(true);
        }

        public void OnClickStart()
        {
            _startGO.SetActive(false);
            _rerollGO.SetActive(true);
            _fightGO.SetActive(true);
            _canvasGroup.blocksRaycasts = true;
        }

        public void ShowBox(EquipmentBoxData boxData)
        {
            ChangeRectPos(false);

            _boxData = boxData;
            var iconPath = $"{PATH_EQUIPMENT_BOX_ICON}{boxData.Type}";
            _boxIMG.sprite = Resources.Load<Sprite>(iconPath);
            _boxGO.SetActive(true);
        }

        public void OnClearWave()
        {
            _inventoryInside.StopUseEquipment();
            _canvasGroup.blocksRaycasts = true;
        }

        public void OnClickBox()
        {
            if (null == _boxData)
            {
                Debug.LogError("no box data");
                return;
            }

            _boxGO.SetActive(false);

            _inventoryOutside.RemoveAll();

            var equipmentIDList = _boxData.EquipmentIDList;
            var dataList = new List<EquipmentData>();
            for (var i = 0; i < equipmentIDList.Count; i++)
            {
                var equipment = StaticDataManager.Instance.GetEquipmentData(equipmentIDList[i]);
                dataList.Add(equipment);
            }

            for (var i = 0; i < dataList.Count; i++)
            {
                _inventoryOutside.TryAdd(dataList[i]);
            }

            _boxData = null;

            _rerollGO.SetActive(true);
            _fightGO.SetActive(true);
        }

        public void OnClickReroll()
        {
            _context.OnClickReroll?.Invoke();
        }

        public void OnClickFight()
        {
            _rerollGO.SetActive(false);
            _fightGO.SetActive(false);
            _inventoryOutside.RemoveAll();
            _inventoryInside.StartUseEquipment(_context.OnCoolDown);
            _canvasGroup.blocksRaycasts = false;

            ChangeRectPos(true);

            _context.StartWave?.Invoke();
        }

        public void ChangeRectPos(bool top, bool immediate = false)
        {
            var topStartPos = _topRTF.anchoredPosition;
            var topTargetPos = top ? new Vector2(topStartPos.x, _topYPosRange.x) : new Vector2(topStartPos.x, _topYPosRange.y);
            if (_topRTF.anchoredPosition == topTargetPos)
                return;

            var botStartPos = _botRTF.anchoredPosition;
            var botTargetPos = top ? new Vector2(botStartPos.x, _botYPosRange.x) : new Vector2(botStartPos.x, _botYPosRange.y);
            if (_botRTF.anchoredPosition == botTargetPos)
                return;

            if (true == immediate)
            {
                _topRTF.anchoredPosition = topTargetPos;
                _botRTF.anchoredPosition = botTargetPos;
                return;
            }

            StartCoroutine(Co_ChangeRectPos());
            IEnumerator Co_ChangeRectPos()
            {
                var elapsed = 0f;
                var duration = 0.4f;
                while (elapsed < duration)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                    var factor = elapsed / duration;
                    _topRTF.anchoredPosition = Vector2.Lerp(topStartPos, topTargetPos, factor);
                    _botRTF.anchoredPosition = Vector2.Lerp(botStartPos, botTargetPos, factor);
                }

                _topRTF.anchoredPosition = topTargetPos;
                _botRTF.anchoredPosition = botTargetPos;
            }
        }

        private void OnEquip(EquipmentData data)
        {
            if (data.Heal > 0)
            {
                _statDict[EStatType.HP] += data.Heal;
                StartCoroutine(SimpleLerp.Co_BounceScale(_hpRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }

            if (data.Damage > 0)
            {
                _statDict[EStatType.ATK] += data.Damage;
                StartCoroutine(SimpleLerp.Co_BounceScale(_atkRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }

            if (data.Defence > 0)
            {
                _statDict[EStatType.DEF] += data.Defence;
                StartCoroutine(SimpleLerp.Co_BounceScale(_defRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }
        }

        private void OnUnequip(EquipmentData data)
        {

            if (data.Heal > 0)
            {
                _statDict[EStatType.HP] -= data.Heal;
                StartCoroutine(SimpleLerp.Co_BounceScale(_hpRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }

            if (data.Damage > 0)
            {
                _statDict[EStatType.ATK] -= data.Damage;
                StartCoroutine(SimpleLerp.Co_BounceScale(_atkRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }

            if (data.Defence > 0)
            {
                _statDict[EStatType.DEF] -= data.Defence;
                StartCoroutine(SimpleLerp.Co_BounceScale(_defRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }
        }

        private void RefreshStat()
        {
            _hpTMP.text = $"{_statDict[EStatType.HP]}/s";
            _atkTMP.text = $"{_statDict[EStatType.ATK]}/s";
            _defTMP.text = $"{_statDict[EStatType.DEF]}/s";
        }
    }
}