using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BabyNightmare.HUD;
using BabyNightmare.InventorySystem;
using BabyNightmare.StaticData;
using BabyNightmare.Talent;
using BabyNightmare.Util;
using Supercent.Util;
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
        public Func<int, ProjectileData> GetProjectileData { get; }

        public MatchViewContext(
        RenderTexture rt,
        EquipmentData initEquipment,
        Action onClickReroll,
        Action startWave,
        Action<EquipmentData> onCooldown,
        Func<EquipmentData, EquipmentData, EquipmentData> getUpgradeData,
        Func<int, ProjectileData> getProjectileData)
        {
            this.RT = rt;
            this.InitEquipment = initEquipment;
            this.OnClickReroll = onClickReroll;
            this.StartWave = startWave;
            this.OnCoolDown = onCooldown;
            this.GetUpgradeData = getUpgradeData;
            this.GetProjectileData = getProjectileData;
        }
    }

    public class MatchView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private RectTransform _topRTF;
        [SerializeField] private RectTransform _botRTF;
        [SerializeField] private RawImage _fieldIMG;
        [SerializeField] private TextMeshProUGUI _waveTMP;
        [SerializeField] private Image _waveProgressIMG;
        [SerializeField] private Transform _waveCircleTF;
        [SerializeField] private Inventory_Bag _bag;
        [SerializeField] private Inventory_Loot _loot;
        [SerializeField] private Vector2 _topYPosRange = new Vector2(285, 0);
        [SerializeField] private Vector2 _botYPosRange = new Vector2(495, 780);
        [SerializeField] private float _rectChangeDruation = 0.4f;
        [SerializeField] private GameObject _startGO;
        [SerializeField] private GameObject _rerollGO;
        [SerializeField] private GameObject _rerollGO_Normal;
        [SerializeField] private GameObject _rerollGO_Free;
        [SerializeField] private GameObject _rerollGO_AD;
        [SerializeField] private TextMeshProUGUI _rerollCostTMP;
        [SerializeField] private GameObject _fightGO;
        [SerializeField] private GameObject _boxGO;
        [SerializeField] private Image _boxIMG;
        [SerializeField] private Transform _statItemViewGridTF;

        private const string PATH_STAT_ITEM_VIEW = "Match/Stat/StatItemView";
        private const string PATH_EQUIPMENT_BOX_ICON = "Match/EquipmentBox/ICN_Box_";
        private const int EQUIPMENT_PRICE = 10;
        private MatchViewContext _context = null;
        private Action _onGetBox = null;
        private Coroutine _coChangeRect = null;
        private Coroutine _coRefreshProgress = null;
        private Dictionary<EStatType, StatItemView> _statItemViewDict = null;
        private Dictionary<EStatType, int> _statDict = null;
        private Dictionary<EStatType, int> _statChangeDict = null;
        private Vector2 _progressSize;

        public RectTransform FieldImage => _fieldIMG.rectTransform;
        public Transform WaveCircleTF => _waveCircleTF;

        public void Init(MatchViewContext context)
        {
            _context = context;
            _statItemViewDict = new Dictionary<EStatType, StatItemView>();
            _statDict = new Dictionary<EStatType, int>();
            _statChangeDict = new Dictionary<EStatType, int>();

            foreach (EStatType type in Enum.GetValues(typeof(EStatType)))
            {
                var itemView = ObjectUtil.LoadAndInstantiate<StatItemView>(PATH_STAT_ITEM_VIEW, _statItemViewGridTF);
                itemView.Init(type);
                _statItemViewDict.Add(type, itemView);
                _statChangeDict.Add(type, 0);
                _statDict.Add(type, 0);
            }

            _fieldIMG.texture = _context.RT;

            _loot.InitBase(context.GetUpgradeData, RefreshStatChange, context.GetProjectileData);
            _bag.InitBase(context.GetUpgradeData, RefreshStatChange, context.GetProjectileData);

            _bag.Init(_loot, AddStat);

            var addedIndexList = PlayerData.Instance.AddedIndexList;
            if (null != addedIndexList)
            {
                for (var i = 0; i < addedIndexList.Count; i++)
                {
                    var index = addedIndexList[i];
                    _bag.AddCell(index);
                }
            }

            _bag.TryAdd(_context.InitEquipment);

            _rerollGO.SetActive(false);
            _fightGO.SetActive(false);

            _boxGO.SetActive(false);
            _canvasGroup.blocksRaycasts = false;
            _startGO.SetActive(true);

            ChangeRectPos(false, true);

            _progressSize = _waveProgressIMG.rectTransform.rect.size;
            _waveProgressIMG.rectTransform.sizeDelta = new Vector2(0, _progressSize.y);
        }

        public void RefreshWave(int curWave, int maxWave)
        {
            _waveTMP.text = $"Wave {curWave}/{maxWave}";
        }

        public void RefreshProgress(float factor)
        {
            var startSize = _waveProgressIMG.rectTransform.sizeDelta;
            var targetSize = Vector2.Lerp(startSize, _progressSize, factor);

            if (null != _coRefreshProgress)
                StopCoroutine(_coRefreshProgress);

            _coRefreshProgress = StartCoroutine(Co_RefreshProgress());

            IEnumerator Co_RefreshProgress()
            {
                var elapsed = 0f;
                var duration = 0.4f;
                while (elapsed < duration)
                {
                    yield return null;
                    elapsed += Time.deltaTime;

                    _waveProgressIMG.rectTransform.sizeDelta = Vector2.Lerp(startSize, targetSize, elapsed / duration);
                }

                _waveProgressIMG.rectTransform.sizeDelta = targetSize;
            }
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

            StartCoroutine(Co_ReadyFight());

            IEnumerator Co_ReadyFight()
            {
                var lootEquipmentList = _loot.EquipmentList;
                for (var i = 0; i < lootEquipmentList.Count; i++)
                {
                    var equipment = lootEquipmentList[i];
                    CoinHUD.SetSpreadPoint(equipment.transform.position);
                    PlayerData.Instance.Coin += EQUIPMENT_PRICE;
                    yield return null;
                }

                yield return CoroutineUtil.WaitForSeconds(0.1f);

                _loot.RemoveAll();

                var speed = TalentManager.Instance.GetValue(ETalentType.Attack_Speed_Percentage);
                _bag.StartUseEquipment(_context.OnCoolDown, speed);
                _canvasGroup.blocksRaycasts = false;

                ChangeRectPos(true);

                _context.StartWave?.Invoke();
            }
        }

        public void ChangeRectPos(bool top, bool immediate = false)
        {
            var topStartPos = _topRTF.anchoredPosition;
            var topTargetPos = top ? new Vector2(topStartPos.x, _topYPosRange.x) : new Vector2(topStartPos.x, _topYPosRange.y);
            if (topStartPos == topTargetPos)
                return;

            var botStartPos = _botRTF.anchoredPosition;
            var botTargetPos = top ? new Vector2(botStartPos.x, _botYPosRange.x) : new Vector2(botStartPos.x, _botYPosRange.y);
            if (botStartPos == botTargetPos)
                return;

            if (true == immediate)
            {
                _topRTF.anchoredPosition = topTargetPos;
                _botRTF.anchoredPosition = botTargetPos;
                return;
            }

            if (null != _coChangeRect)
                StopCoroutine(_coChangeRect);

            _coChangeRect = StartCoroutine(Co_ChangeRectPos(topTargetPos, botTargetPos));
        }

        private IEnumerator Co_ChangeRectPos(Vector2 topTargetPos, Vector2 botTargetPos)
        {
            var elapsed = 0f;

            var topStartPos = _topRTF.anchoredPosition;
            var botStartPos = _botRTF.anchoredPosition;
            while (elapsed < _rectChangeDruation)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = elapsed / _rectChangeDruation;
                _topRTF.anchoredPosition = Vector2.Lerp(topStartPos, topTargetPos, factor);
                _botRTF.anchoredPosition = Vector2.Lerp(botStartPos, botTargetPos, factor);
            }

            _topRTF.anchoredPosition = topTargetPos;
            _botRTF.anchoredPosition = botTargetPos;

            _coChangeRect = null;
        }

        private void AddStat(EquipmentData data, bool isEquip)
        {
            var statDataList = data.StatDataList;
            for (var i = 0; i < statDataList.Count; i++)
            {
                var statData = statDataList[i];
                var value = data.GetStatValueByCool(statData.Value);
                _statDict[statData.Type] += isEquip ? value : -value;
                //Debug.Log($"{data.Name} {data.Level} {statData.Type} {statData.Value}  {data.CoolTime}");
            }

            foreach (var pair in _statItemViewDict)
            {
                pair.Value.RefreshValue(_statDict[pair.Key]);
            }
        }

        private void RefreshStatChange(Equipment dragEquipment, List<Equipment> overlapList)
        {
            foreach (var key in _statChangeDict.Keys.ToList())
            {
                _statChangeDict[key] = 0;
            }

            var isUpgradable = false;
            if (null != overlapList && overlapList.Count > 0) // 겹치는 장비가 있을 때 
            {
                var upgradeData = _context.GetUpgradeData(dragEquipment.Data, overlapList[0].Data);
                if (overlapList.Count == 1 && null != upgradeData) //업그레이드 상황일 때 업그레이드 데이터 스탯 계산
                {
                    var statDataList = upgradeData.StatDataList;
                    for (var i = 0; i < statDataList.Count; i++)
                    {
                        var statData = statDataList[i];
                        var value = upgradeData.GetStatValueByCool(statData.Value);
                        _statChangeDict[statData.Type] += value;
                    }

                    isUpgradable = true;
                }
                else //업그레이드 상황이 아닐 때 겹치는 장비 스탯 계산
                {
                    for (var i = 0; i < overlapList.Count; i++)
                    {
                        var data = overlapList[i].Data;
                        var overlapStatDataList = data.StatDataList;
                        for (var j = 0; j < overlapStatDataList.Count; j++)
                        {
                            var statData = overlapStatDataList[j];
                            _statChangeDict[statData.Type] -= data.GetStatValueByCool(statData.Value);
                        }
                    }
                }
            }

            if (null != dragEquipment && false == isUpgradable) // 드래그 중인 장비 스탯 계산
            {
                var data = dragEquipment.Data;
                var statDataList = data.StatDataList;

                for (var i = 0; i < statDataList.Count; i++)
                {
                    var statData = statDataList[i];
                    _statChangeDict[statData.Type] += data.GetStatValueByCool(statData.Value);
                }
            }

            foreach (var pair in _statItemViewDict)
            {
                pair.Value.RefreshChangeValue(_statChangeDict[pair.Key]);
            }
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