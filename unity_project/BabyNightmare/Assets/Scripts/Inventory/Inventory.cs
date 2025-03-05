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
        [SerializeField] private int _width = 4;
        [SerializeField] private int _height = 4;
        [SerializeField] private Vector2Int _cellSize = new Vector2Int(32, 32);
        [SerializeField] private Sprite _cellSpriteEmpty = null;
        [SerializeField] private Sprite _cellSpriteSelected = null;
        [SerializeField] private Sprite _cellSpriteBlocked = null;

        private const string PATH_DYNAMIC_CELL = "Inventory/DynamicCell";
        private RectTransform _grandCanvasRect = null;
        private DynamicCell[] _bgCellArr = null;
        private Pool<DynamicCell> _cellPool = null;
        private HashSet<Equipment> _equipmentSet = null;
        private Equipment _clickedEquipment = null;
        private static DraggedItem _draggedEquipment = null;

        public int Row => _width;
        public int Column => _height;
        public Vector2 CellSize => _cellSize;

        public void Init(RectTransform grandCanvasRect)
        {
            _grandCanvasRect = grandCanvasRect;

            var poolTF = new GameObject("PoolTF").GetComponent<Transform>();
            poolTF.SetParent(transform);
            poolTF.localPosition = Vector3.zero;
            poolTF.localScale = Vector3.one;

            _cellPool = new Pool<DynamicCell>(() => ObjectUtil.LoadAndInstantiate<DynamicCell>(PATH_DYNAMIC_CELL, poolTF));

            _bgCellArr = new DynamicCell[Row * Column];

            var gridSize = new Vector2(_cellSize.x * Row, _cellSize.y * Column);
            _rtf.sizeDelta = gridSize;

            var topLeft = new Vector3(-gridSize.x / 2, -gridSize.y / 2, 0); // Calculate topleft corner
            var half_cellSize = new Vector3(_cellSize.x / 2, _cellSize.y / 2, 0); // Calulcate cells half-size

            var count = 0;
            for (int y = 0; y < Column; y++)
            {
                for (int x = 0; x < Row; x++)
                {
                    var cell = GetCell(_cellSpriteEmpty, true);
                    cell.RTF.SetAsFirstSibling();
                    cell.Image.type = Image.Type.Sliced;
                    cell.RTF.localPosition = topLeft + new Vector3(_cellSize.x * (Row - 1 - x), _cellSize.y * y, 0) + half_cellSize;
                    cell.RTF.sizeDelta = _cellSize;
                    cell.GO.name = $"grid {count}";
                    _bgCellArr[count] = cell;
                    count++;
                }
            }

            _equipmentSet = new HashSet<Equipment>();
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
            cell.GO.SetActive(false);
            _cellPool.Return(cell);
        }

        public void PaintBG(Equipment equipment, bool blocked, Color color)
        {
            ClearBG();

            var data = equipment.Data;
            var point = equipment.Point;
            for (var x = 0; x < data.Row; x++)
            {
                for (var y = 0; y < data.Column; y++)
                {
                    var offset = new Vector2Int(x, y);
                    if (false == data.IsInside(offset))
                        continue;

                    var newPoint = point + offset;
                    if (newPoint.x < 0)
                        continue;
                    if (newPoint.x >= Row)
                        continue;
                    if (newPoint.y < 0)
                        continue;
                    if (newPoint.y >= Column)
                        continue;

                    var index = newPoint.y * Row + (Row - 1 - newPoint.x);
                    var cell = _bgCellArr[index];

                    cell.Image.sprite = blocked ? _cellSpriteBlocked : _cellSpriteSelected;
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

        public bool TryAdd(EquipmentData data) //가능한 가장 가까운 자리
        {
            if (data.Row > Row)
                return false;

            if (data.Column > Column)
                return false;

            for (var x = 0; x < Row - (data.Row - 1); x++)
            {
                for (var y = 0; y < Column - (data.Column - 1); y++)
                {
                    if (false == TryAdd(data, new Vector2Int(x, y)))
                        continue;

                    return true;
                }
            }

            return false;
        }

        public bool TryAdd(EquipmentData data, Vector2Int point)
        {
            if (false == IsAddable(data, point))
                return false;

            var cell = GetCell(data.Sprite, false);

            var equipment = new Equipment(cell, data);
            equipment.Point = point;

            cell.RTF.localPosition = GetPivot(equipment);

            _equipmentSet.Add(equipment);
            return true;
        }

        public bool TryRemove(Equipment equipment)
        {
            if (false == _equipmentSet.Contains(equipment))
                return false;

            ReturnCell(equipment.Cell);
            _equipmentSet.Remove(equipment);

            return true;
        }

        public void RemoveAll()
        {
            foreach (var equipment in _equipmentSet)
            {
                var cell = equipment.Cell;
                ReturnCell(cell);
            }

            _equipmentSet.Clear();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
            {
                _draggedEquipment.SetOwner(this);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
            {
                _draggedEquipment.SetOwner(null);
                ClearBG();
            }
            else
            {
                _clickedEquipment = null;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
                return;

            var pos = GetCellPos(eventData.position);
            _clickedEquipment = TryGetEquipmentAtPos(pos);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var pos = GetCellPos(eventData.position);
            var equipment = TryGetEquipmentAtPos(pos);
            if (null == equipment)
                return;

            if (null != _draggedEquipment)
                return;

            InventoryUtil.ShowInfoPopup(equipment.Data);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ClearBG();

            if (null == _clickedEquipment)
                return;

            if (null != _draggedEquipment)
                return;

            var localPos = GetLocalPos(eventData.position);
            var offset = GetPivot(_clickedEquipment) - localPos;

            _draggedEquipment = new DraggedItem(
                _grandCanvasRect.transform as RectTransform,
                this,
                _clickedEquipment,
                offset
            );

            TryRemove(_clickedEquipment);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (null == _draggedEquipment)
            {
                var pos = GetCellPos(eventData.position);
                var equipment = TryGetEquipmentAtPos(pos);
                if (equipment == _clickedEquipment)
                    return;

                _clickedEquipment = equipment;
            }
            else
            {
                _draggedEquipment.SetPosition(eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (null == _draggedEquipment)
                return;

            var mode = _draggedEquipment.Drop(eventData.position);

            switch (mode)
            {
                case DraggedItem.DropMode.Added:
                    break;
                case DraggedItem.DropMode.Returned:
                    break;
                case DraggedItem.DropMode.Dropped:
                    _clickedEquipment = null;
                    break;
            }

            _draggedEquipment = null;
        }

        public bool IsAddable(EquipmentData data, Vector2Int point)
        {
            if (false == IsInside(data, point))
                return false;

            if (true == IsOverlap(data, point))
                return false;

            return true;
        }

        private bool IsInside(EquipmentData data, Vector2Int point)
        {
            for (var x = 0; x < data.Row; x++)
            {
                for (var y = 0; y < data.Column; y++)
                {
                    var offset = new Vector2Int(x, y);
                    if (false == data.IsInside(offset))
                        continue;

                    var newPoint = point + offset;
                    if (newPoint.x < 0)
                        return false;
                    if (newPoint.x >= Row)
                        return false;
                    if (newPoint.y < 0)
                        return false;
                    if (newPoint.y >= Column)
                        return false;
                }
            }

            return true;
        }

        private bool IsOverlap(EquipmentData targetData, Vector2Int point)
        {
            foreach (var equipment in _equipmentSet)
            {
                var data = equipment.Data;
                for (var i = 0; i < data.Row; i++)
                {
                    for (var j = 0; j < data.Column; j++)
                    {
                        var offset = new Vector2Int(i, j);
                        if (false == data.IsInside(offset))
                            continue;

                        var ePoint = equipment.Point + offset;
                        for (var x = 0; x < targetData.Row; x++)
                        {
                            for (var y = 0; y < targetData.Column; y++)
                            {
                                var targetOffset = new Vector2Int(x, y);
                                if (false == targetData.IsInside(targetOffset))
                                    continue;

                                var tPoint = point + targetOffset;
                                if (ePoint == tPoint)
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private Equipment TryGetEquipmentAtPos(Vector2Int point)
        {
            foreach (var equipment in _equipmentSet)
            {
                var data = equipment.Data;

                for (var i = 0; i < data.Row; i++)
                {
                    for (var j = 0; j < data.Column; j++)
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

            var x = (-(Row * 0.5f) + equipment.Point.x + data.Row * 0.5f) * _cellSize.x;
            var y = (-(Column * 0.5f) + equipment.Point.y + data.Column * 0.5f) * _cellSize.y;
            return new Vector2(x, y);
        }

        private Vector2 GetLocalPos(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rtf, screenPos, null, out var localPos);
            return localPos;
        }

        public Vector2Int GetCellPos(Vector2 screenPos)
        {
            var pos = GetLocalPos(screenPos);
            pos.x += _rtf.sizeDelta.x / 2;
            pos.y += _rtf.sizeDelta.y / 2;
            return new Vector2Int(Mathf.FloorToInt(pos.x / _cellSize.x), Mathf.FloorToInt(pos.y / _cellSize.y));
        }

        public void StartUseEquipment(Action<EquipmentData> onCoolDown)
        {
            foreach (var equipment in _equipmentSet)
            {
                var cell = equipment.Cell;
                var data = equipment.Data;
                cell.StartCoolDownLoop(data.CoolTime, () => onCoolDown?.Invoke(data));
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