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
        [SerializeField] private Inventory_Bag _bag;
        [SerializeField] private Inventory_Loot _loot;
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
        private Dictionary<EStatType, int> _statDict = null;
        private Action _onGetBox = null;

        public RectTransform FieldImage => _fieldIMG.rectTransform;

        public void Init(MatchViewContext context)
        {
            _context = context;
            _statDict = new Dictionary<EStatType, int>();

            foreach (EStatType type in Enum.GetValues(typeof(EStatType)))
                _statDict.Add(type, 0);

            RefreshStat();

            _fieldIMG.texture = _context.RT;

            _loot.Init(context.GetUpgradeData);
            _bag.Init(_loot, OnEquip, OnUnequip, context.GetUpgradeData);

            var addedIndexList = PlayerData.Instance.AddedIndexList;
            for (var i = 0; i < addedIndexList.Count; i++)
            {
                var index = addedIndexList[i];
                _bag.AddCell(index);
            }

            _bag.TryAdd(_context.InitEquipment);

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
            _loot.RemoveAll();

            for (var i = 0; i < dataList.Count; i++)
            {
                _loot.TryAdd(dataList[i]);
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

        public void OnClearWave()
        {
            _bag.StopUseEquipment();
            _canvasGroup.blocksRaycasts = true;
        }

        public void ShowBox(EBoxType type, Action onGetBox)
        {
            ChangeRectPos(false);

            _onGetBox = onGetBox;
            var iconPath = $"{PATH_EQUIPMENT_BOX_ICON}{type}";
            _boxIMG.sprite = Resources.Load<Sprite>(iconPath);
            _boxGO.SetActive(true);
        }

        public void OnClickBox()
        {
            _boxGO.SetActive(false);
            _rerollGO.SetActive(true);
            _fightGO.SetActive(true);

            _onGetBox?.Invoke();
        }

        public void OnClickReroll()
        {
            _context.OnClickReroll?.Invoke();
        }

        public void OnClickFight()
        {
            _rerollGO.SetActive(false);
            _fightGO.SetActive(false);
            _loot.RemoveAll();
            _bag.StartUseEquipment(_context.OnCoolDown);
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
                _statDict[EStatType.HP] += Mathf.CeilToInt(data.Heal / data.CoolTime);
                StartCoroutine(SimpleLerp.Co_BounceScale(_hpRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }

            if (data.Damage > 0)
            {
                _statDict[EStatType.ATK] += Mathf.CeilToInt(data.Damage / data.CoolTime);
                StartCoroutine(SimpleLerp.Co_BounceScale(_atkRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }

            if (data.Defence > 0)
            {
                _statDict[EStatType.DEF] += Mathf.CeilToInt(data.Defence / data.CoolTime);
                StartCoroutine(SimpleLerp.Co_BounceScale(_defRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }
        }

        private void OnUnequip(EquipmentData data)
        {

            if (data.Heal > 0)
            {
                _statDict[EStatType.HP] -= Mathf.CeilToInt(data.Heal / data.CoolTime);
                StartCoroutine(SimpleLerp.Co_BounceScale(_hpRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }

            if (data.Damage > 0)
            {
                _statDict[EStatType.ATK] -= Mathf.CeilToInt(data.Damage / data.CoolTime);
                StartCoroutine(SimpleLerp.Co_BounceScale(_atkRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }

            if (data.Defence > 0)
            {
                _statDict[EStatType.DEF] -= Mathf.CeilToInt(data.Defence / data.CoolTime);
                StartCoroutine(SimpleLerp.Co_BounceScale(_defRTF, Vector3.one * 1.2f, _bounceCurve, 0.1f, RefreshStat));
            }
        }

        private void RefreshStat()
        {
            _hpTMP.text = $"{_statDict[EStatType.HP]}/s";
            _atkTMP.text = $"{_statDict[EStatType.ATK]}/s";
            _defTMP.text = $"{_statDict[EStatType.DEF]}/s";
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                _bag.ShowAddableCell();
            }
        }

#endif
    }
}