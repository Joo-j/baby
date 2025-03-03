using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BabyNightmare.Util;
using BabyNightmare.StaticData;
using Supercent.Util;

namespace BabyNightmare.InventorySystem
{
    public class Inventory : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
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
        private Canvas _grandCanvas = null;
        private DynamicCell[] _grid = null;
        private Pool<DynamicCell> _cellPool = null;
        private Dictionary<DynamicCell, EquipmentData> _equipmentCellDict = null;
        private EquipmentData _draggedData = null;
        private EquipmentData _lastHoveredData = null;
        private static DraggedEquipment _draggedEquipment = null;
        private PointerEventData _currentEventData = null;

        public Vector2 CellSize => _cellSize;
        public int Width => _width;
        public int Height => _height;

        public void Init(Canvas grandCanvas)
        {
            _grandCanvas = grandCanvas;

            var poolTF = new GameObject("PoolTF").GetComponent<Transform>();
            poolTF.SetParent(transform);
            poolTF.localPosition = Vector3.zero;
            poolTF.localScale = Vector3.one;

            _equipmentCellDict = new Dictionary<DynamicCell, EquipmentData>();
            _cellPool = new Pool<DynamicCell>(() => ObjectUtil.LoadAndInstantiate<DynamicCell>(PATH_DYNAMIC_CELL, poolTF));
            _grid = new DynamicCell[Width * Height];

            var gridSize = new Vector2(CellSize.x * Width, CellSize.y * Height);
            _rtf.sizeDelta = gridSize;

            var topLeft = new Vector3(-gridSize.x / 2, -gridSize.y / 2, 0); // Calculate topleft corner
            var halfCellSize = new Vector3(CellSize.x / 2, CellSize.y / 2, 0); // Calulcate cells half-size

            var count = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var cell = GetCell(_cellSpriteEmpty, true);
                    cell.RTF.SetAsFirstSibling();
                    cell.Image.type = Image.Type.Sliced;
                    cell.RTF.localPosition = topLeft + new Vector3(CellSize.x * (Width - 1 - x), CellSize.y * y, 0) + halfCellSize;
                    cell.RTF.sizeDelta = CellSize;
                    cell.GO.name = $"grid {count}";
                    _grid[count] = cell;
                    count++;
                }
            }
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

        public void SelectGrid(EquipmentData data, bool blocked, Color color)
        {
            if (null == data)
                return;

            ClearGrid();

            for (var x = 0; x < data.Width; x++)
            {
                for (var y = 0; y < data.Height; y++)
                {
                    var shape = data.Shape;
                    if (false == shape.IsPartOfShape(new Vector2Int(x, y)))
                        continue;

                    var pos = data.Position + new Vector2Int(x, y);
                    if (pos.x < 0)
                        continue;
                    if (pos.x >= Width)
                        continue;
                    if (pos.y < 0)
                        continue;
                    if (pos.y >= Height)
                        continue;

                    var index = pos.y * Width + (Width - 1 - pos.x);
                    var cell = _grid[index];

                    cell.Image.sprite = blocked ? _cellSpriteBlocked : _cellSpriteSelected;
                    cell.Image.color = color;
                }
            }
        }

        public void ClearGrid()
        {
            for (var i = 0; i < _grid.Length; i++)
            {
                var cell = _grid[i];
                cell.Image.sprite = _cellSpriteEmpty;
                cell.Image.color = Color.white;
            }
        }

        public bool TryAddCell(EquipmentData data) //가능한 가장 가까운 자리
        {
            if (data.Width > Width)
                return false;

            if (data.Height > Height)
                return false;

            for (var x = 0; x < Width - (data.Width - 1); x++)
            {
                for (var y = 0; y < Height - (data.Height - 1); y++)
                {
                    if (false == TryAddCell(data, new Vector2Int(x, y)))
                        continue;

                    return true;
                }
            }

            return false;
        }

        public bool TryAddCell(EquipmentData data, Vector2Int pos)
        {
            if (false == TryOverlap(data, pos))
                return false;

            data.Position = pos;

            var cell = GetCell(data.Sprite, false);
            cell.RTF.localPosition = GetPivot(data);

            _equipmentCellDict.Add(cell, data);
            return true;
        }

        public bool TryRemoveCell(EquipmentData data)
        {
            foreach (var pair in _equipmentCellDict)
            {
                if (pair.Value == data)
                {
                    var cell = pair.Key;
                    ReturnCell(cell);
                    _equipmentCellDict.Remove(cell);
                    return true;
                }
            }

            return false;
        }

        public void RemoveAllCell()
        {
            foreach (var pair in _equipmentCellDict)
            {
                var cell = pair.Key;
                ReturnCell(cell);
            }

            _equipmentCellDict.Clear();
        }

        public bool TryOverlap(EquipmentData targetData, Vector2Int pos)
        {
            var previousPos = targetData.Position;
            targetData.Position = pos;

            foreach (var pair in _equipmentCellDict)
            {
                var data = pair.Value;
                for (var i = 0; i < data.Width; i++)
                {
                    for (var j = 0; j < data.Height; j++)
                    {
                        var shape = data.Shape;
                        if (false == shape.IsPartOfShape(new Vector2Int(i, j)))
                            continue;

                        var iPos = data.Position + new Vector2Int(i, j);
                        for (var x = 0; x < targetData.Width; x++)
                        {
                            for (var y = 0; y < targetData.Height; y++)
                            {
                                var targetShape = targetData.Shape;
                                if (false == targetShape.IsPartOfShape(new Vector2Int(x, y)))
                                    continue;

                                var oPos = targetData.Position + new Vector2Int(x, y);
                                if (oPos != iPos)
                                    continue;

                                targetData.Position = previousPos;
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
            {
                _draggedEquipment.CurrentInventory = this;
            }

            _currentEventData = eventData;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
            {
                _draggedEquipment.CurrentInventory = null;
                ClearGrid();
            }
            else
            {
                _lastHoveredData = null;
            }

            _currentEventData = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
                return;

            var pos = GetCellPos(eventData.position);
            _draggedData = GetDataAtPos(pos);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (null != _draggedData)
            {
                //InventoryUtil.ShowInfoPopup(_draggedData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ClearGrid();

            if (null == _draggedData || null != _draggedEquipment)
                return;

            var localPosition = GetLocalPos(eventData.position);
            var offset = GetPivot(_draggedData) - localPosition;

            _draggedEquipment = new DraggedEquipment(
                _grandCanvas,
                this,
                _draggedData.Position,
                _draggedData,
                offset,
                GetCell(_draggedData.Sprite, false)
            );

            TryRemoveCell(_draggedData);
        }


        public void OnDrag(PointerEventData eventData)
        {
            _currentEventData = eventData;
            if (_draggedEquipment != null)
            {
                // Update the equipments position
                //_draggedEquipment.Position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (null == _draggedEquipment)
                return;

            var mode = _draggedEquipment.Drop(eventData.position);

            switch (mode)
            {
                case DraggedEquipment.DropMode.Added:
                    break;
                case DraggedEquipment.DropMode.Returned:
                    break;
                case DraggedEquipment.DropMode.Dropped:
                    _lastHoveredData = null;
                    break;
            }

            _draggedEquipment = null;
            _currentEventData = null;
        }

        private void Update()
        {
            if (null == _currentEventData)
                return;

            if (null == _draggedEquipment)
            {
                // Detect hover
                var pos = GetCellPos(_currentEventData.position);
                var data = GetDataAtPos(pos);
                if (data == _lastHoveredData)
                    return;

                _lastHoveredData = data;
            }
            else
            {
                // Update position while dragging
                _draggedEquipment.Position = _currentEventData.position;
            }
        }

        private EquipmentData GetDataAtPos(Vector2Int pos)
        {
            foreach (var pair in _equipmentCellDict)
            {
                var data = pair.Value;

                for (var i = 0; i < data.Width; i++)
                {
                    for (var j = 0; j < data.Height; j++)
                    {
                        if (pos == data.Position + new Vector2Int(i, j))
                            return data;
                    }
                }
            }

            return null;
        }

        private Vector2 GetPivot(EquipmentData data)
        {
            var x = (-(Width * 0.5f) + data.Position.x + data.Width * 0.5f) * CellSize.x;
            var y = (-(Height * 0.5f) + data.Position.y + data.Height * 0.5f) * CellSize.y;
            return new Vector2(x, y);
        }

        public Vector2Int GetCellPos(Vector2 screenPos)
        {
            var pos = GetLocalPos(screenPos);
            pos.x += _rtf.sizeDelta.x / 2;
            pos.y += _rtf.sizeDelta.y / 2;
            return new Vector2Int(Mathf.FloorToInt(pos.x / CellSize.x), Mathf.FloorToInt(pos.y / CellSize.y));
        }

        private Vector2 GetLocalPos(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rtf, screenPos, null, out var localPos);
            return localPos;
        }

        public void StartCoolDown(Action<EquipmentData> onCoolDown)
        {
            foreach (var pair in _equipmentCellDict)
            {
                var cell = pair.Key;
                var data = pair.Value;
                cell.StartCoolDownLoop(data.CoolTime, () => onCoolDown?.Invoke(data));
            }

            _canvasGroup.interactable = false;
        }

        public void StopCoolDown()
        {
            foreach (var pair in _equipmentCellDict)
            {
                var cell = pair.Key;
                cell.ResetCool();
            }

            _canvasGroup.interactable = true;
        }
    }
}