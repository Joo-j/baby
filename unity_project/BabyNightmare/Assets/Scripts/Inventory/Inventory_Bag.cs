using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Supercent.Core.Audio;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;

namespace BabyNightmare.InventorySystem
{
    public class Inventory_Bag : Inventory
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Vector2Int _defaultGridSize = new Vector2Int(3, 3);
        [SerializeField] private Color _cellClearColor;
        [SerializeField] private Color _cellEnableColor;
        [SerializeField] private Color _cellOverlappedColor;
        [SerializeField] private Color _cellUpgradableColor;

        private const string PATH_CELL = "Inventory/Cell";
        private const string PATH_CELL_ADDABLE = "Inventory/Cell_Addable";
        private const float CELL_LENGTH = 100;
        private Vector2Int _gridSize = default;
        private Vector2Int _gridOffset = default;
        private Dictionary<Vector2Int, Cell> _cellDict = null;
        private HashSet<Equipment> _equipmentSet = null;
        private Inventory _outsideInventory = null;
        private Action<EquipmentData, bool> _onEquip = null;

        public void Init
        (
            Inventory outsideInventory,
            Action<EquipmentData, bool> onEquip
        )
        {
            _outsideInventory = outsideInventory;
            _onEquip = onEquip;

            _equipmentSet = new HashSet<Equipment>();
            _cellDict = new Dictionary<Vector2Int, Cell>();

            for (int y = 0; y < _defaultGridSize.y; y++)
                for (int x = 0; x < _defaultGridSize.x; x++)
                    AddCell(new Vector2Int(x, y));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(Co_RefreshCell());
        }

        private bool IsValid(Vector2Int index)
        {
            if (index.x < 0 || index.y < 0 || index.x >= _gridSize.x || index.y >= _gridSize.y)
                return false;

            return _cellDict.ContainsKey(index);
        }

        private Cell CreateCell(Vector2Int index)
        {
            var cell = ObjectUtil.LoadAndInstantiate<Cell>(PATH_CELL, transform);
            cell.RTF.sizeDelta = new Vector2(CELL_LENGTH, CELL_LENGTH);
            cell.RTF.anchoredPosition = GetLocalPos(index);
            cell.RefreshColor(_cellClearColor);
            return cell;
        }

        private Cell CreateCell_Addable(Vector2Int index)
        {
            var cell = ObjectUtil.LoadAndInstantiate<Cell>(PATH_CELL_ADDABLE, transform);
            cell.RTF.sizeDelta = new Vector2(CELL_LENGTH, CELL_LENGTH);
            cell.RTF.anchoredPosition = GetLocalPos(index);
            cell.RefreshColor(_cellClearColor);
            return cell;
        }

        public void AddCell(Vector2Int index)
        {
            if (true == _cellDict.ContainsKey(index))
            {
                Debug.Log($"{index}에 이미 cell이 들어있습니다.");
                return;
            }

            if (index.x < 0 || index.y < 0)
            {
                var offset = new Vector2Int(index.x < 0 ? -index.x : 0, index.y < 0 ? -index.y : 0);
                var newDict = new Dictionary<Vector2Int, Cell>();

                foreach (var pair in _cellDict)
                {
                    var newIndex = pair.Key + offset;
                    newDict.Add(newIndex, pair.Value);
                    pair.Value.RTF.anchoredPosition = GetLocalPos(newIndex);
                }

                foreach (var equipment in _equipmentSet)
                {
                    var newIndex = equipment.Index + offset;
                    equipment.Index = newIndex;
                }

                _cellDict = newDict;
                _gridOffset += offset;
                _gridSize += offset;
                index += offset;

                var width = _gridSize.x * CELL_LENGTH;
                var height = _gridSize.y * CELL_LENGTH;
                _rtf.sizeDelta = new Vector2(width, height);

                foreach (var pair in _cellDict)
                {
                    pair.Value.RTF.anchoredPosition = GetLocalPos(pair.Key);
                }

                foreach (var equipment in _equipmentSet)
                {
                    equipment.RTF.anchoredPosition = GetLocalPos(equipment.Index, equipment.Data);
                }
            }
            else if (index.x >= _gridSize.x || index.y >= _gridSize.y)
            {
                _gridSize = new Vector2Int(Mathf.Max(_gridSize.x, index.x + 1), Mathf.Max(_gridSize.y, index.y + 1));

                var width = _gridSize.x * CELL_LENGTH;
                var height = _gridSize.y * CELL_LENGTH;
                _rtf.sizeDelta = new Vector2(width, height);

                foreach (var pair in _cellDict)
                {
                    pair.Value.RTF.anchoredPosition = GetLocalPos(pair.Key);
                }

                foreach (var equipment in _equipmentSet)
                {
                    equipment.RTF.anchoredPosition = GetLocalPos(equipment.Index, equipment.Data);
                }
            }

            // 새 셀 추가
            var newCell = CreateCell(index);
            _cellDict.Add(index, newCell);
        }

        public bool TryGetAddableIndexs(out List<Vector2Int> addableIndexs)
        {
            addableIndexs = new List<Vector2Int>();

            var minLength = Mathf.Min(_gridSize.x - _gridOffset.x, _gridSize.y - _gridOffset.y);
            for (var x = _gridOffset.x; x < _gridOffset.x + minLength; x++)
            {
                var index = new Vector2Int(x, _gridOffset.y - 1);
                if (false == _cellDict.ContainsKey(index))
                    addableIndexs.Add(index); // 아래
            }

            for (var y = _gridOffset.y; y < _gridOffset.y + minLength; y++)
            {
                var leftIndex = new Vector2Int(_gridOffset.x - 1, y);
                if (false == _cellDict.ContainsKey(leftIndex))
                    addableIndexs.Add(leftIndex); // 왼쪽

                var rightIndex = new Vector2Int(_gridOffset.x + minLength, y);
                if (false == _cellDict.ContainsKey(rightIndex))
                    addableIndexs.Add(rightIndex); // 오른쪽
            }

            return addableIndexs.Count > 0;
        }

        public void ShowAddableCell(Action doneCallback)
        {
            if (false == TryGetAddableIndexs(out var addableIndexs))
                return;

            var addableCells = new List<Cell>();

            foreach (var index in addableIndexs)
            {
                var cell = CreateCell_Addable(index);
                cell.AddButton(() => OnClickButton(index));
                addableCells.Add(cell);
            }

            void OnClickButton(Vector2Int index)
            {
                AddCell(index);
                foreach (var cell in addableCells)
                {
                    Destroy(cell.gameObject);
                }

                doneCallback?.Invoke();
                AudioManager.PlaySFX("AudioClip/Inventory_AddCell");
            }
        }

        private IEnumerator Co_RefreshCell()
        {
            while (true)
            {
                yield return null;

                foreach (var pair in _cellDict)
                    pair.Value.RefreshColor(_cellClearColor);

                if (null == _draggedEquipment)
                    continue;

                if (null == _dragEventData)
                    continue;

                var data = _draggedEquipment.Data;
                var validList = data.Shape.ValidIndexList;

                var halfLength = CELL_LENGTH * 0.5f;
                var offset = new Vector2((1 - data.Shape.Column) * halfLength, (1 - data.Shape.Row) * halfLength);
                var targetIndex = GetIndex(_dragEventData.position + offset);

                for (var i = 0; i < validList.Count; i++)
                {
                    var newIndex = targetIndex + validList[i];

                    if (false == IsValid(newIndex))
                        continue;

                    if (false == _cellDict.TryGetValue(newIndex, out var cell))
                    {
                        Debug.Log($"{newIndex}에 cell이 없습니다.");
                        continue;
                    }

                    var isOverlapped = TryGetOverlap(newIndex, out var overlappedEquipment);
                    if (true == isOverlapped)
                    {
                        var upgradeData = _getUpgradeData.Invoke(_draggedEquipment.Data, overlappedEquipment.Data);
                        if (null != upgradeData)
                        {
                            cell.RefreshColor(_cellUpgradableColor);
                        }
                        else
                        {
                            cell.RefreshColor(_cellOverlappedColor);
                        }
                    }
                    else
                    {
                        cell.RefreshColor(_cellEnableColor);
                    }
                }
            }
        }

        public override bool TryEquip(Equipment equipment, Vector2 screenPos)
        {
            var data = equipment.Data;
            var halfLength = CELL_LENGTH * 0.5f;
            var offset = new Vector2((1 - data.Shape.Column) * halfLength, (1 - data.Shape.Row) * halfLength);
            var targetIndex = GetIndex(screenPos + offset);

            var validList = data.Shape.ValidIndexList;
            var oeSet = new HashSet<Equipment>();
            for (var i = 0; i < validList.Count; i++)
            {
                var newIndex = targetIndex + validList[i];
                if (false == IsValid(newIndex))
                {
                    //Debug.Log($"{newIndex} 그리드 밖이라 실패");
                    return false;
                }

                if (true == TryGetOverlap(newIndex, out var overlapEquipment))
                    oeSet.Add(overlapEquipment);
            }

            var overlapCount = oeSet.Count;
            if (overlapCount <= 0)
            {
                //Debug.Log($"겹친 장비가 없다면 바로 배치");
                Equip(equipment, targetIndex);
                AudioManager.PlaySFX("AudioClip/Inventory_Equip");
                return true;
            }

            if (overlapCount == 1)
            {
                //Debug.Log($"겹친 장비가 하나일 때");
                var oe = oeSet.ToList()[0];
                var upgradeData = _getUpgradeData(oe.Data, data);
                if (null != upgradeData)
                {
                    //Debug.Log($"업그레이드 데이터가 존재하면, 기존 장비 삭제 후 배치, 업그레이드");
                    Remove(oe);
                    equipment.Refresh(upgradeData, true);
                    Equip(equipment, targetIndex);
                    AudioManager.PlaySFX("AudioClip/Inventory_Merge");
                    HapticHelper.Haptic(Lofelt.NiceVibrations.HapticPatterns.PresetType.MediumImpact);
                    _showMergeMessage?.Invoke(equipment.transform, $"LV {upgradeData.Level}!");
                    return true;
                }
                else
                {
                    //Debug.Log($"같은 장비가 아니면 기존 장비 밖으로 내보내고 배치");
                    Eject(oe);
                    Equip(equipment, targetIndex);
                    AudioManager.PlaySFX("AudioClip/Inventory_Equip");
                    return true;
                }
            }

            //Debug.Log("겹친 장비가 2개 이상이면 기존 장비 전부 밖으로 내보내고 배치");
            foreach (var oe in oeSet)
                Eject(oe);

            Equip(equipment, targetIndex);
            AudioManager.PlaySFX("AudioClip/Inventory_Equip");
            return true;
        }

        public override void Equip(Equipment equipment, bool immediate)
        {
            var data = equipment.Data;
            if (false == TryGetRandomIndex(data, out var randomIndex))
            {
                Debug.Log($"빈자리가 없습니다.");
                return;
            }

            if (true == immediate)
            {
                Equip(equipment, randomIndex);
                return;
            }

            equipment.transform.SetParent(transform);
            _equipmentSet.Add(equipment);
            equipment.Index = randomIndex;

            var targetPos = GetLocalPos(randomIndex, equipment.Data);
            equipment.Move(targetPos, () => _onEquip?.Invoke(equipment.Data, true));
        }

        public override void Equip(Equipment equipment, Vector2Int index)
        {
            equipment.transform.SetParent(transform);
            equipment.Index = index;
            equipment.RTF.anchoredPosition = GetLocalPos(index, equipment.Data);
            _equipmentSet.Add(equipment);
            _onEquip?.Invoke(equipment.Data, true);
        }

        private void Remove(Equipment equipment)
        {
            if (false == _equipmentSet.Contains(equipment))
                return;

            DestroyImmediate(equipment.gameObject);
            _equipmentSet.Remove(equipment);
            _onEquip?.Invoke(equipment.Data, false);
        }

        private Equipment Unequip(Vector2Int targetIndex)
        {
            var equipment = Get(targetIndex);
            if (null == equipment)
                return null;

            if (false == _equipmentSet.Contains(equipment))
                return null;

            _equipmentSet.Remove(equipment);
            _onEquip?.Invoke(equipment.Data, false);
            return equipment;
        }

        public override Equipment Unequip(Vector2 screenPos)
        {
            var index = GetIndex(screenPos);
            var equipment = Unequip(index);
            if (null == equipment)
                return null;

            return equipment;
        }

        private void Eject(Equipment equipment)
        {
            if (false == _equipmentSet.Contains(equipment))
                return;

            _equipmentSet.Remove(equipment);
            _onEquip?.Invoke(equipment.Data, false);

            _outsideInventory.Equip(equipment, false);
        }

        public override Equipment Get(Vector2 screenPos)
        {
            var targetIndex = GetIndex(screenPos);
            var equipment = Get(targetIndex);
            if (null == equipment)
                return null;

            return equipment;
        }

        private Equipment Get(Vector2Int targetIndex)
        {
            foreach (var equipment in _equipmentSet)
            {
                var validList = equipment.Data.Shape.ValidIndexList;
                for (var i = 0; i < validList.Count; i++)
                {
                    var newIndex = equipment.Index + validList[i];
                    if (targetIndex == newIndex)
                        return equipment;
                }
            }

            return null;
        }

        private bool IsOverlap(EquipmentData data, Vector2Int targetIndex)
        {
            var validList = data.Shape.ValidIndexList;
            for (var i = 0; i < validList.Count; i++)
            {
                var newIndex = targetIndex + validList[i];
                if (false == IsValid(newIndex))
                    continue;

                if (true == TryGetOverlap(newIndex, out var overlappedEquipment))
                    return true;
            }

            return false;
        }

        private bool TryGetOverlap(Vector2 targetIndex, out Equipment overlapped)
        {
            overlapped = null;

            foreach (var equipment in _equipmentSet)
            {
                var data = equipment.Data;
                var validList = data.Shape.ValidIndexList;
                for (var i = 0; i < validList.Count; i++)
                {
                    var index = validList[i];
                    var newIndex = equipment.Index + index;
                    if (false == IsValid(newIndex))
                        continue;

                    if (targetIndex == newIndex)
                    {
                        overlapped = equipment;
                        return true;
                    }
                }
            }

            return false;
        }

        public override HashSet<Equipment> TryGetOverlap(Equipment equipment, Vector2 screenPos)
        {
            HashSet<Equipment> ovelapSet = null;
            var currentIndex = GetIndex(screenPos);

            var data = equipment.Data;
            var validList = data.Shape.ValidIndexList;
            for (var i = 0; i < validList.Count; i++)
            {
                var index = validList[i];
                var newIndex = currentIndex + index;
                if (false == IsValid(newIndex))
                    continue;

                if (true == TryGetOverlap(newIndex, out var overlapped))
                {
                    if (equipment != overlapped)
                    {
                        ovelapSet ??= new HashSet<Equipment>();
                        ovelapSet.Add(overlapped);
                    }
                }
            }

            return ovelapSet;
        }

        private bool TryGetRandomIndex(EquipmentData data, out Vector2Int index)
        {
            index = Vector2Int.zero;

            List<Vector2Int> enableIndexList = new List<Vector2Int>();

            for (int x = 0; x <= _gridSize.x - data.Shape.Column; x++)
            {
                for (int y = 0; y <= _gridSize.y - data.Shape.Row; y++)
                {
                    var targetIndex = new Vector2Int(x, y);
                    if (false == IsValid(targetIndex))
                        continue;

                    if (true == IsOverlap(data, targetIndex))
                        continue;

                    enableIndexList.Add(targetIndex);
                }
            }

            if (enableIndexList.Count <= 0)
                return false;

            index = enableIndexList[UnityEngine.Random.Range(0, enableIndexList.Count)];
            return true;
        }

        private Vector2Int GetIndex(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rtf, screenPos, null, out var anchoredPos);
            var pos = anchoredPos + _rtf.sizeDelta * 0.5f; //가운데를 기준으로 하기 때문에 왼쪽 아래로 기준을 맞추기 위해 더해주는 작업
            var x = Mathf.FloorToInt(pos.x / CELL_LENGTH);
            var y = Mathf.FloorToInt(pos.y / CELL_LENGTH);
            var index = new Vector2Int(x, y);

            return index;
        }


        private Vector2 GetLocalPos(Vector2 index)
        {
            var x = (index.x * CELL_LENGTH) + (-_rtf.sizeDelta.x * 0.5f) + (CELL_LENGTH * 0.5f);
            var y = (index.y * CELL_LENGTH) + (-_rtf.sizeDelta.y * 0.5f) + (CELL_LENGTH * 0.5f);
            return new Vector2(x, y);
        }

        private Vector2 GetLocalPos(Vector2Int index, EquipmentData data)
        {
            var offset = (new Vector2(data.Shape.Column, data.Shape.Row) - _gridSize) * 0.5f;
            return (index + offset) * new Vector2(CELL_LENGTH, CELL_LENGTH);
        }

        public void StartUseEquipment(Action<EquipmentData> onCoolDown, float speed)
        {
            _canvasGroup.interactable = false;

            StartCoroutine(Co_StartUseEquipment());

            IEnumerator Co_StartUseEquipment()
            {
                foreach (var equipment in _equipmentSet)
                {
                    var data = equipment.Data;
                    var coolTime = data.CoolTime;
                    coolTime -= coolTime * speed;
                    yield return CoroutineUtil.WaitForSeconds(0.1f);
                    equipment.StartCoolDown(coolTime, () => onCoolDown?.Invoke(data));
                }
            }
        }

        public void StopUseEquipment()
        {
            foreach (var equipment in _equipmentSet)
            {
                equipment.StopCoolDown();
            }

            _canvasGroup.interactable = true;
        }
    }
}