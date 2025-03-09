using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;

namespace BabyNightmare.InventorySystem
{
    public class Inventory : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectShape _shape;
        [SerializeField] private Sprite _cellClear;
        [SerializeField] private Sprite _cellEnable;
        [SerializeField] private Sprite _cellOverlapped;
        [SerializeField] private Sprite _cellUpgradable;
        [SerializeField] private AnimationCurve _equipmentMoveCurve;

        private const string PATH_EQUIPMENT = "Inventory/Equipment";

        private RectTransform _parentRTF = null;
        private Image[] _cellArr = null;
        private Vector2 _cellSize = default;
        private HashSet<Equipment> _equipmentSet = null;
        private Inventory _outsideInventory = null;
        private Action<EquipmentData> _onEquip = null;
        private Action<EquipmentData> _onUnequip = null;
        private Func<EquipmentData, EquipmentData, EquipmentData> _getUpgradeData;
        public static Inventory _currentInventory = null;
        private static Equipment _draggedEquipment = null;

        public void Init(
        RectTransform parentRTF,
        Inventory outsideInventory,
        Action<EquipmentData> onEquip,
        Action<EquipmentData> onUnequip,
        Func<EquipmentData, EquipmentData, EquipmentData> getUpgradeData)
        {
            _parentRTF = parentRTF;
            _outsideInventory = outsideInventory ?? this;
            _onEquip = onEquip;
            _onUnequip = onUnequip;
            _getUpgradeData = getUpgradeData;

            _cellArr = new Image[_shape.Row * _shape.Column];

            var gridSize = _rtf.sizeDelta;

            var topLeft = new Vector2(-gridSize.x / 2, -gridSize.y / 2);
            var width = gridSize.x / _shape.Column;
            var height = gridSize.y / _shape.Row;
            _cellSize = new Vector2(width, height);

            var half_cellSize = new Vector2(_cellSize.x / 2, _cellSize.y / 2);

            var count = 0;
            for (int y = 0; y < _shape.Row; y++)
            {
                for (int x = 0; x < _shape.Column; x++)
                {
                    var cell = new GameObject($"cell {y} {x}").AddComponent<Image>();
                    cell.transform.SetParent(transform);
                    cell.sprite = _cellClear;
                    cell.type = Image.Type.Sliced;
                    cell.rectTransform.anchoredPosition = topLeft + new Vector2(_cellSize.x * (_shape.Column - 1 - x), _cellSize.y * y) + half_cellSize;
                    cell.rectTransform.sizeDelta = _cellSize;
                    _cellArr[count] = cell;
                    ++count;
                }
            }

            _equipmentSet = new HashSet<Equipment>();
        }

        public bool TryAdd(EquipmentData data)
        {
            if (data.Row > _shape.Row)
                return false;

            if (data.Column > _shape.Column)
                return false;

            if (false == TryGetRandomPoint(data, out var index))
                return false;

            var equipment = ObjectUtil.LoadAndInstantiate<Equipment>(PATH_EQUIPMENT, transform);
            equipment.Refresh(data, false);
            Equip(equipment, index);

            return true;
        }

        private bool TryEquip(Equipment equipment, Vector2Int targetIndex)
        {
            var data = equipment.Data;
            var indexList = data.IndexList;
            for (var i = 0; i < indexList.Count; i++)
            {
                var index = indexList[i];
                var newIndex = targetIndex + index;
                if (false == _shape.IsValid(newIndex))
                    return false;
            }

            HashSet<Equipment> oes = new HashSet<Equipment>();
            for (var x = 0; x < data.Column; x++)
            {
                for (var y = 0; y < data.Row; y++)
                {
                    var index = new Vector2Int(x, y);
                    if (false == data.IsValid(index))
                        continue;

                    var newIndex = targetIndex + index;
                    if (true == IsOverlap(newIndex, out var overlapEquipment))
                        oes.Add(overlapEquipment);
                }
            }

            var overlapCount = oes.Count;
            if (overlapCount <= 0)
            {
                Debug.Log($"겹친 장비가 없다면 바로 배치");
                Equip(equipment, targetIndex);
                return true;
            }

            if (overlapCount == 1)
            {
                Debug.Log($"겹친 장비가 하나일 때");

                foreach (var oe in oes)
                {
                    var upgradeData = _getUpgradeData(oe.Data, data);
                    if (null != upgradeData)
                    {
                        Debug.Log($"업그레이드 데이터가 존재하면, 기존 장비 삭제 후 배치, 업그레이드");
                        Remove(oe);
                        Equip(equipment, targetIndex);
                        equipment.Refresh(upgradeData, true);
                        return true;
                    }
                    else
                    {
                        Debug.Log($"같은 장비가 아니면 기존 장비 밖으로 내보내고 배치");
                        KickOut(oe);
                        Equip(equipment, targetIndex);
                        return true;
                    }
                }
            }

            Debug.Log("겹친 장비가 2개 이상이면 기존 장비 전부 밖으로 내보내고 배치");
            foreach (var oe in oes)
            {
                KickOut(oe);
            }

            Equip(equipment, targetIndex);
            return true;
        }

        private void Equip(Equipment equipment, Vector2Int index)
        {
            equipment.transform.SetParent(transform);
            equipment.Index = index;
            equipment.AnchoredPos = GetAnchoredPos(index, equipment.Data);
            _equipmentSet.Add(equipment);
            _onEquip?.Invoke(equipment.Data);
            ClearCell();
        }

        private void Equip_Lerp(Equipment equipment)
        {
            var data = equipment.Data;
            if (false == TryGetRandomPoint(data, out var index))
            {
                Debug.Log("랜덤 포인트를 찾지 못했습니다.");
                Destroy(equipment.gameObject);
                return;
            }

            _equipmentSet.Add(equipment);
            equipment.Index = index;

            var startPos = equipment.AnchoredPos;
            var targetPos = GetAnchoredPos(index, equipment.Data);
            StartCoroutine(SimpleLerp.Co_LerpAnchoredPosition(equipment.RTF, startPos, targetPos, _equipmentMoveCurve, 0.1f,
            () =>
            {
                equipment.transform.SetParent(transform);
                _onEquip?.Invoke(equipment.Data);
                ClearCell();
            }));
        }

        private void Remove(Equipment equipment)
        {
            if (false == _equipmentSet.Contains(equipment))
                return;

            Destroy(equipment.gameObject);
            _equipmentSet.Remove(equipment);
            _onUnequip?.Invoke(equipment.Data);
            ClearCell();
        }

        public void RemoveAll()
        {
            foreach (var equipment in _equipmentSet)
            {
                Destroy(equipment.gameObject);
                _onUnequip?.Invoke(equipment.Data);
            }

            _equipmentSet.Clear();
            ClearCell();
        }

        private Equipment Unequip(Vector2Int targetIndex)
        {
            var equipment = Get(targetIndex);
            if (null == equipment)
            {
                Debug.Log($"{targetIndex}에서 장비를 찾지 못함");
                return null;
            }

            if (false == _equipmentSet.Contains(equipment))
                return null;

            _equipmentSet.Remove(equipment);
            _onUnequip?.Invoke(equipment.Data);
            equipment.transform.SetParent(_parentRTF);
            return equipment;
        }

        private void KickOut(Equipment equipment)
        {
            if (false == _equipmentSet.Contains(equipment))
                return;

            _equipmentSet.Remove(equipment);
            _onUnequip?.Invoke(equipment.Data);

            _outsideInventory.Equip_Lerp(equipment);

            ClearCell();
        }

        private Equipment Get(Vector2Int targetIndex)
        {
            foreach (var equipment in _equipmentSet)
            {
                var data = equipment.Data;
                for (var i = 0; i < data.Column; i++)
                {
                    for (var j = 0; j < data.Row; j++)
                    {
                        var newIndex = equipment.Index + new Vector2Int(i, j);

                        if (targetIndex == newIndex)
                            return equipment;
                    }
                }
            }

            return null;
        }

        private bool IsOverlap(Vector2 targetIndex, out Equipment overlappedEquipment)
        {
            overlappedEquipment = null;

            foreach (var equipment in _equipmentSet)
            {
                var data = equipment.Data;
                var indexList = data.IndexList;
                for (var i = 0; i < indexList.Count; i++)
                {
                    var index = indexList[i];
                    var newIndex = equipment.Index + index;
                    if (targetIndex == newIndex)
                    {
                        overlappedEquipment = equipment;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryGetRandomPoint(EquipmentData data, out Vector2Int targetIndex)
        {
            targetIndex = default;
            List<Vector2Int> enableIndexList = new List<Vector2Int>();

            var indexList = data.IndexList;

            var maxX = _shape.Column - (data.Column - 1);
            var maxY = _shape.Row - (data.Row - 1);

            for (var x = 0; x < maxX; x++)
            {
                for (var y = 0; y < maxY; y++)
                {
                    for (var i = 0; i < indexList.Count; i++)
                    {
                        var index = indexList[i];
                        var newIndex = targetIndex + index;
                        if (false == _shape.IsValid(newIndex))
                            continue;

                        if (true == IsOverlap(newIndex, out var overlappedEquipment))
                            continue;

                        enableIndexList.Add(newIndex);
                    }
                }
            }

            if (enableIndexList.Count == 0)
            {
                Debug.Log("드랍 가능한 포인트가 없습니다.");
                return false;
            }

            targetIndex = enableIndexList[UnityEngine.Random.Range(0, enableIndexList.Count)];
            return true;
        }

        private Vector2Int GetIndex(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rtf, screenPos, null, out var anchoredPos);
            var pos = anchoredPos + _rtf.sizeDelta * 0.5f; //가운데를 기준으로 하기 때문에 왼쪽 아래로 기준을 맞추기 위해 더해주는 작업
            var x = Mathf.FloorToInt(pos.x / _cellSize.x);
            var y = Mathf.FloorToInt(pos.y / _cellSize.y);
            var index = new Vector2Int(x, y);

            return index;
        }

        private Vector2 GetAnchoredPos(Vector2Int index, EquipmentData data)
        {
            var offsetX = (data.Column - _shape.Column) * 0.5f;
            var offsetY = (data.Row - _shape.Row) * 0.5f;

            return new Vector2((index.x + offsetX) * _cellSize.x, (index.y + offsetY) * _cellSize.y);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _currentInventory = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ClearCell();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
                return;

            var index = _currentInventory.GetIndex(eventData.position);
            var equipment = Get(index);
            if (null == equipment)
            {
                Debug.Log("{index}에서 장비를 찾지 못함");
                return;
            }

            InventoryUtil.ShowInfoPopup(equipment.Data);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
                return;

            var index = _currentInventory.GetIndex(eventData.position);
            var equipment = Unequip(index);
            if (null == equipment)
                return;

            _draggedEquipment = equipment;
            _draggedEquipment.Reset = () =>
            {
                Equip(equipment, equipment.Index);
                _draggedEquipment = null;
            };
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (null == _draggedEquipment || null == _currentInventory)
                return;

            _currentInventory.InvokeBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (null == _draggedEquipment || null == _currentInventory)
                return;

            _currentInventory.InvokeDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (null == _draggedEquipment || null == _currentInventory)
                return;

            _currentInventory.InvokeEndDrag(eventData);
        }

        private void InvokeBeginDrag(PointerEventData eventData)
        {
        }

        private void InvokeDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRTF, eventData.position, null, out var anchoredPos);

            _draggedEquipment.AnchoredPos = anchoredPos;
            _currentInventory.RefreshCell(_draggedEquipment, eventData.position);
        }

        private void InvokeEndDrag(PointerEventData eventData)
        {
            if (null == _draggedEquipment || null == _currentInventory)
                return;

            var screenPos = eventData.position;
            var index = GetIndex(screenPos);

            if (true == _currentInventory.TryEquip(_draggedEquipment, index))
            {
                _draggedEquipment = null;
            }
            else
            {
                _draggedEquipment.Reset?.Invoke();
            }

            _currentInventory.ClearCell();
        }

        private void RefreshCell(Equipment equipment, Vector2 screenPos)
        {
            ClearCell();

            var data = equipment.Data;
            var indexList = data.IndexList;
            var targetIndex = GetIndex(screenPos);

            for (var i = 0; i < indexList.Count; i++)
            {
                var index = indexList[i];
                var newIndex = targetIndex + index;

                if (false == _shape.IsValid(newIndex))
                    continue;

                var cellIndex = newIndex.y * _shape.Column + (_shape.Column - 1 - newIndex.x);
                var cell = _cellArr[cellIndex];

                var isOverlapped = IsOverlap(newIndex, out var overlappedEquipment);
                if (true == isOverlapped)
                {
                    var upgradeData = _getUpgradeData.Invoke(equipment.Data, overlappedEquipment.Data);
                    if (null != upgradeData)
                    {
                        cell.sprite = _cellUpgradable;
                    }
                    else
                    {
                        cell.sprite = _cellOverlapped;
                    }
                }
                else
                {
                    cell.sprite = _cellEnable;
                }
            }
        }

        private void ClearCell()
        {
            for (var i = 0; i < _cellArr.Length; i++)
            {
                var cell = _cellArr[i];
                cell.sprite = _cellClear;
                cell.color = Color.white;
            }
        }

        public void StartUseEquipment(Action<EquipmentData> onCoolDown)
        {
            foreach (var equipment in _equipmentSet)
            {
                var data = equipment.Data;
                this.Invoke(CoroutineUtil.WaitForSeconds(0.1f), () => equipment.StartCoolDownLoop(data.CoolTime, () => onCoolDown?.Invoke(data)));
            }

            _canvasGroup.interactable = false;
        }

        public void StopUseEquipment()
        {
            foreach (var equipment in _equipmentSet)
            {
                equipment.ResetCool();
            }

            _canvasGroup.interactable = true;
        }
    }
}