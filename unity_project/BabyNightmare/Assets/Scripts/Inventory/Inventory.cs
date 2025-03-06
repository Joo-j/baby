using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Supercent.Util;
using BabyNightmare.Util;
using BabyNightmare.StaticData;

namespace BabyNightmare.InventorySystem
{
    public class Inventory : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Vector2Int _cellSize = new Vector2Int(32, 32);
        [SerializeField] private RectShape _shape;
        [SerializeField] private Sprite _cellSpriteEmpty = null;
        [SerializeField] private Sprite _cellSpriteSelected = null;
        [SerializeField] private Sprite _cellSpriteBlocked = null;

        private const string PATH_DYNAMIC_CELL = "Inventory/DynamicCell";
        private RectTransform _grandCanvasRect = null;
        private DynamicCell[] _bgCellArr = null;
        private Transform _poolTF = null;
        private Pool<DynamicCell> _cellPool = null;
        private HashSet<Equipment> _equipmentSet = null;
        private Equipment _clickedEquipment = null;
        private static DraggedItem _draggedEquipment = null;
        private Action<EquipmentData> _onEquip = null;
        private Action<EquipmentData> _onUnequip = null;

        public Vector2 CellSize => _cellSize;

        public void Init(RectTransform grandCanvasRect, Action<EquipmentData> onEquip, Action<EquipmentData> onUnequip)
        {
            _grandCanvasRect = grandCanvasRect;
            _onEquip = onEquip;
            _onUnequip = onUnequip;

            _poolTF = new GameObject("PoolTF").GetComponent<Transform>();
            _poolTF.SetParent(transform);
            _poolTF.localPosition = Vector3.zero;
            _poolTF.localScale = Vector3.one;

            _cellPool = new Pool<DynamicCell>(() => ObjectUtil.LoadAndInstantiate<DynamicCell>(PATH_DYNAMIC_CELL, _poolTF));

            _bgCellArr = new DynamicCell[_shape.Row * _shape.Column];

            var gridSize = new Vector2(_cellSize.x * _shape.Column, _cellSize.y * _shape.Row);
            _rtf.sizeDelta = gridSize;

            var topLeft = new Vector3(-gridSize.x / 2, -gridSize.y / 2, 0); // Calculate topleft corner
            var half_cellSize = new Vector3(_cellSize.x / 2, _cellSize.y / 2, 0); // Calulcate cells half-size

            var count = 0;
            for (int y = 0; y < _shape.Row; y++)
            {
                for (int x = 0; x < _shape.Column; x++)
                {
                    var cell = GetCell(_cellSpriteEmpty, true);
                    cell.RTF.SetAsFirstSibling();
                    cell.Image.type = Image.Type.Sliced;
                    cell.RTF.localPosition = topLeft + new Vector3(_cellSize.x * (_shape.Column - 1 - x), _cellSize.y * y, 0) + half_cellSize;
                    cell.RTF.sizeDelta = _cellSize;
                    cell.GO.name = $"grid {count}";
                    _bgCellArr[count] = cell;
                    count++;
                }
            }

            _equipmentSet = new HashSet<Equipment>();
            _draggedEquipment ??= new DraggedItem(_grandCanvasRect);
        }

        private DynamicCell GetCell(Sprite sprite, bool enable)
        {
            var cell = _cellPool.Get();
            cell.GO.SetActive(true);
            cell.Image.sprite = sprite;
            cell.RTF.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
            cell.Image.raycastTarget = enable;
            cell.Image.type = Image.Type.Simple;
            cell.RTF.SetAsLastSibling();

            return cell;
        }

        private void ReturnCell(DynamicCell cell)
        {
            cell.RTF.SetParent(_poolTF);
            cell.GO.SetActive(false);
            _cellPool.Return(cell);
        }

        public void PaintBG(Equipment equipment, Color color)
        {
            ClearBG();

            bool isAddable = IsAddable(equipment);
            var data = equipment.Data;
            var point = equipment.Point;
            for (var x = 0; x < data.Column; x++)
            {
                for (var y = 0; y < data.Row; y++)
                {
                    var offset = new Vector2Int(x, y);
                    if (false == data.IsValid(offset))
                        continue;

                    var newPoint = point + offset;
                    if (newPoint.x < 0)
                        continue;
                    if (newPoint.x >= _shape.Column)
                        continue;
                    if (newPoint.y < 0)
                        continue;
                    if (newPoint.y >= _shape.Row)
                        continue;

                    var index = newPoint.y * _shape.Column + (_shape.Column - 1 - newPoint.x);
                    var cell = _bgCellArr[index];

                    cell.Image.sprite = isAddable ? _cellSpriteSelected : _cellSpriteBlocked;
                    cell.Image.color = color;
                }
            }
        }

        public void ClearBG()
        {
            for (var i = 0; i < _bgCellArr.Length; i++)
            {
                var cell = _bgCellArr[i];
                cell.Image.sprite = _cellSpriteEmpty;
                cell.Image.color = Color.white;
            }
        }

        public bool TryAdd(EquipmentData data) //랜덤 배치
        {
            if (data.Row > _shape.Row)
                return false;

            if (data.Column > _shape.Column)
                return false;

            List<Vector2Int> enablePoints = new List<Vector2Int>();

            for (var x = 0; x < _shape.Column - (data.Column - 1); x++)
            {
                for (var y = 0; y < _shape.Row - (data.Row - 1); y++)
                {
                    var newPoint = new Vector2Int(x, y);
                    if (true == IsAddable(data, newPoint))
                        enablePoints.Add(newPoint);
                }
            }

            if (enablePoints.Count == 0)
                return false;

            var finalPoint = enablePoints[UnityEngine.Random.Range(0, enablePoints.Count)];
            return TryAdd(data, finalPoint);
        }

        public bool TryAdd(EquipmentData data, Vector2Int point)
        {
            if (false == IsAddable(data, point))
                return false;

            var cell = GetCell(data.Sprite, false);

            var equipment = new Equipment(cell, data);
            equipment.Point = point;

            cell.RTF.localPosition = GetPivot(equipment);
            cell.GO.name = $"{data.Name}";

            _equipmentSet.Add(equipment);
            ClearBG();
            _onEquip?.Invoke(equipment.Data);
            return true;
        }

        public bool TryRemove(Equipment equipment)
        {
            if (false == _equipmentSet.Contains(equipment))
                return false;

            ReturnCell(equipment.Cell);
            _equipmentSet.Remove(equipment);
            _onUnequip?.Invoke(equipment.Data);
            ClearBG();

            return true;
        }

        public void RemoveAll()
        {
            foreach (var equipment in _equipmentSet)
            {
                var cell = equipment.Cell;
                _onUnequip?.Invoke(equipment.Data);
                ReturnCell(cell);
            }

            _equipmentSet.Clear();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (true == _draggedEquipment.IsDragging)
            {
                _draggedEquipment.CurrentOwner = this;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (true == _draggedEquipment.IsDragging)
            {
                _draggedEquipment.CurrentOwner = null;
                ClearBG();
            }
            else
            {
                _clickedEquipment = null;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (true == _draggedEquipment.IsDragging)
                return;

            var pos = GetPoint(eventData.position);
            _clickedEquipment = TryGetEquipmentAtPos(pos);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var pos = GetPoint(eventData.position);
            var equipment = TryGetEquipmentAtPos(pos);
            if (null == equipment)
                return;

            if (true == _draggedEquipment.IsDragging)
                return;

            InventoryUtil.ShowInfoPopup(equipment.Data);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ClearBG();

            if (null == _clickedEquipment)
                return;

            if (true == _draggedEquipment.IsDragging)
                return;

            var localPos = GetLocalPos(eventData.position);
            var offset = GetPivot(_clickedEquipment) - localPos;

            _draggedEquipment.Init(_clickedEquipment.Cell, this, _clickedEquipment, offset);

            TryRemove(_clickedEquipment);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (true == _draggedEquipment.IsDragging)
            {
                _draggedEquipment.SetPosition(eventData.position);
            }
            else
            {
                var pos = GetPoint(eventData.position);
                var equipment = TryGetEquipmentAtPos(pos);
                if (equipment == _clickedEquipment)
                    return;

                _clickedEquipment = equipment;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (true == _draggedEquipment.IsDragging)
            {
                _draggedEquipment.Release(eventData.position);
                _clickedEquipment = null;
            }

            ClearBG();
        }

        public bool IsAddable(Equipment equipment) => IsAddable(equipment.Data, equipment.Point);
        public bool IsAddable(EquipmentData data, Vector2Int point)
        {
            if (false == IsValid(data, point))
                return false;

            if (GetOverlapEquipments(data, point).Count > 0)
                return false;

            return true;
        }

        public bool IsValid(EquipmentData data, Vector2Int point)
        {
            for (var x = 0; x < data.Column; x++)
            {
                for (var y = 0; y < data.Row; y++)
                {
                    var offset = new Vector2Int(x, y);
                    if (false == data.IsValid(offset))
                        continue;

                    var newPoint = point + offset;
                    if (false == _shape.IsValid(newPoint))
                        return false;

                    if (newPoint.x < 0)
                        return false;
                    if (newPoint.x >= _shape.Column)
                        return false;
                    if (newPoint.y < 0)
                        return false;
                    if (newPoint.y >= _shape.Row)
                        return false;
                }
            }

            return true;
        }

        public HashSet<Equipment> GetOverlapEquipments(EquipmentData targetData, Vector2Int point)
        {
            HashSet<Equipment> overlapEquipments = new HashSet<Equipment>();

            foreach (var equipment in _equipmentSet)
            {
                var data = equipment.Data;
                for (var i = 0; i < data.Column; i++)
                {
                    for (var j = 0; j < data.Row; j++)
                    {
                        var offset = new Vector2Int(i, j);
                        if (false == data.IsValid(offset))
                            continue;

                        var ePoint = equipment.Point + offset;
                        for (var x = 0; x < targetData.Column; x++)
                        {
                            for (var y = 0; y < targetData.Row; y++)
                            {
                                var targetOffset = new Vector2Int(x, y);
                                if (false == targetData.IsValid(targetOffset))
                                    continue;

                                var tPoint = point + targetOffset;
                                if (ePoint == tPoint)
                                {
                                    overlapEquipments.Add(equipment);
                                }
                            }
                        }
                    }
                }
            }

            return overlapEquipments;
        }

        private Equipment TryGetEquipmentAtPos(Vector2Int point)
        {
            foreach (var equipment in _equipmentSet)
            {
                var data = equipment.Data;

                for (var i = 0; i < data.Column; i++)
                {
                    for (var j = 0; j < data.Row; j++)
                    {
                        if (point == equipment.Point + new Vector2Int(i, j))
                            return equipment;
                    }
                }
            }

            return null;
        }

        private Vector2 GetPivot(Equipment equipment)
        {
            var data = equipment.Data;

            var x = (-(_shape.Column * 0.5f) + equipment.Point.x + data.Column * 0.5f) * _cellSize.x;
            var y = (-(_shape.Row * 0.5f) + equipment.Point.y + data.Row * 0.5f) * _cellSize.y;
            return new Vector2(x, y);
        }

        private Vector2 GetLocalPos(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rtf, screenPos, null, out var localPos);
            return localPos;
        }

        public Vector2Int GetPoint(Vector2 screenPos)
        {
            var point = GetLocalPos(screenPos);
            point.x += _rtf.sizeDelta.x / 2;
            point.y += _rtf.sizeDelta.y / 2;
            return new Vector2Int(Mathf.FloorToInt(point.x / _cellSize.x), Mathf.FloorToInt(point.y / _cellSize.y));
        }

        public void StartUseEquipment(Action<EquipmentData> onCoolDown)
        {
            foreach (var equipment in _equipmentSet)
            {
                var cell = equipment.Cell;
                var data = equipment.Data;
                this.Invoke(CoroutineUtil.WaitForSeconds(0.1f), () => cell.StartCoolDownLoop(data.CoolTime, () => onCoolDown?.Invoke(data)));
            }

            _canvasGroup.interactable = false;
        }

        public void StopUseEquipment()
        {
            foreach (var equipment in _equipmentSet)
            {
                var cell = equipment.Cell;
                cell.ResetCool();
            }

            _canvasGroup.interactable = true;
        }
    }
}