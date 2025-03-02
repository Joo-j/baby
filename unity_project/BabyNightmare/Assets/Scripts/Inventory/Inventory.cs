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
        private Rect _fullRect;
        private DynamicCell[] _grid = null;
        private Dictionary<EquipmentData, DynamicCell> _cellDict = null;
        private Pool<DynamicCell> _cellPool = null;
        private PointerEventData _currentEventData = null;
        private List<EquipmentData> _dataList = null;
        private EquipmentData _draggedData = null;
        private EquipmentData _lastHoveredData = null;
        private static DraggedEquipment _draggedEquipment;

        public Vector2 CellSize => _cellSize;
        public int Width => _width;
        public int Height => _height;

        public void Init(Canvas grandCanvas)
        {
            _grandCanvas = grandCanvas;

            var poolTF = new GameObject("PoolTF").AddComponent<RectTransform>();
            poolTF.transform.SetParent(transform);
            poolTF.transform.localPosition = Vector3.zero;
            poolTF.transform.localScale = Vector3.one;

            _dataList = new List<EquipmentData>();
            _cellDict = new Dictionary<EquipmentData, DynamicCell>();
            _cellPool = new Pool<DynamicCell>(() => ObjectUtil.LoadAndInstantiate<DynamicCell>(PATH_DYNAMIC_CELL, poolTF));
            _fullRect = new Rect(0, 0, _width, _height);

            RerenderGrid();
            RerenderCells();
        }

        private DynamicCell GetCell(Sprite sprite, bool enable)
        {
            var cell = _cellPool.Get();
            cell.Show();
            cell.RefreshImage(sprite);
            cell.EnableRaycast(enable);
            cell.SetType(Image.Type.Simple);
            cell.RTF.SetAsLastSibling();

            return cell;
        }

        private void ReturnCell(DynamicCell cell)
        {
            cell.Hide();
            _cellPool.Return(cell);
        }

        private void RerenderGrid()
        {
            if (null != _grid)
            {
                for (var i = 0; i < _grid.Length; i++)
                {
                    ReturnCell(_grid[i]);
                    _grid[i].transform.SetSiblingIndex(i);
                }
            }
            _grid = null;

            // Render new grid
            var gridSize = new Vector2(CellSize.x * Width, CellSize.y * Height);

            var topLeft = new Vector3(-gridSize.x / 2, -gridSize.y / 2, 0); // Calculate topleft corner
            var halfCellSize = new Vector3(CellSize.x / 2, CellSize.y / 2, 0); // Calulcate cells half-size

            _grid = new DynamicCell[Width * Height];

            var count = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var cell = GetCell(_cellSpriteEmpty, true);
                    cell.RTF.SetAsFirstSibling();
                    cell.SetType(Image.Type.Sliced);
                    cell.RTF.localPosition = topLeft + new Vector3(CellSize.x * ((Width - 1) - x), CellSize.y * y, 0) + halfCellSize;
                    cell.RTF.sizeDelta = CellSize;
                    cell.gameObject.name = $"Grid {count}";
                    _grid[count] = cell;
                    count++;
                }
            }

            _rtf.sizeDelta = gridSize;
        }

        private void RerenderCells()
        {
            _cellDict ??= new Dictionary<EquipmentData, DynamicCell>();

            foreach (var cell in _cellDict.Values)
            {
                ReturnCell(cell);
            }

            _cellDict.Clear();

            foreach (var data in _dataList)
            {
                AddCell(data);
            }
        }

        private void AddCell(EquipmentData data)
        {
            var cell = GetCell(data.Image, false);
            cell.RTF.localPosition = GetPivot(data);

            _cellDict.Add(data, cell);
        }

        private void RemoveCell(EquipmentData data)
        {
            if (false == _cellDict.TryGetValue(data, out var cell))
                return;

            ReturnCell(cell);
            _cellDict.Remove(data);
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
                    if (false == data.IsPartOfShape(new Vector2Int(x, y)))
                        continue;

                    var p = data.Position + new Vector2Int(x, y);
                    if (p.x < 0)
                        continue;
                    if (p.x >= Width)
                        continue;
                    if (p.y < 0)
                        continue;
                    if (p.y >= Height)
                        continue;

                    var index = p.y * Width + (Width - 1 - p.x);
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

        public bool TryAdd(EquipmentData data)
        {
            if (false == TryGetFitPoint(data, out var point))
                return false;

            return TryAdd(data, point);
        }

        public bool TryAdd(EquipmentData data, Vector2Int point)
        {
            if (true == _dataList.Contains(data))
                return false;

            if (false == CanAddAtPoint(data, point))
                return false;

            _dataList.Add(data);

            data.Position = point;

            AddCell(data);
            return true;
        }


        public bool TryRemove(EquipmentData data)
        {
            if (false == _dataList.Contains(data))
                return false;

            if (false == _dataList.Remove(data))
                return false;

            RemoveCell(data);
            return true;
        }

        public void RemoveAll()
        {
            _dataList.Clear();

            foreach (var data in _dataList)
            {
                RemoveCell(data);
            }
        }

        public bool TryDrop(EquipmentData data)
        {
            if (false == _dataList.Contains(data))
            {
                Debug.Log($"You're not allowed to drop {data.Name} on the ground");
                return false;
            }

            if (false == _dataList.Remove(data))
            {
                Debug.Log($"You're not allowed to drop {data.Name} on the ground");
                return false;
            }

            RemoveCell(data);
            Debug.Log(data.Name + " was dropped on the ground");

            return true;
        }

        private bool TryGetFitPoint(EquipmentData data, out Vector2Int point)
        {
            if (data.Width <= Width && data.Height <= Height)
            {
                for (var x = 0; x < Width - (data.Width - 1); x++)
                {
                    for (var y = 0; y < Height - (data.Height - 1); y++)
                    {
                        point = new Vector2Int(x, y);
                        if (true == CanAddAtPoint(data, point))
                            return true;
                    }
                }
            }

            Debug.Log($"not fit  {data.Width} <= {Width} && {data.Height} <= {Height}");
            point = Vector2Int.zero;
            return false;
        }

        private EquipmentData GetDataAtPoint(Vector2Int point)
        {
            foreach (var data in _dataList)
            {
                if (true == data.Contains(point))
                {
                    return data;
                }
            }

            return null;
        }


        public bool CanAddAtPoint(EquipmentData data, Vector2Int point)
        {
            var previousPoint = data.Position;
            data.Position = point;
            var padding = Vector2.one * 0.00f;

            // Check if data is outside of inventory
            // if (false == _fullRect.Contains(data.GetMinPoint() + padding) || false == _fullRect.Contains(data.GetMaxPoint() - padding))
            // {
            //     data.Position = previousPoint;
            //     Debug.Log($"You can't put {data.Name} there!");
            //     return false;
            // }

            // Check if data overlaps another data already in the inventory
            if (false == _dataList.Any(otherEquipment => data.Overlaps(otherEquipment)))
                return true; // Equipment can be added

            data.Position = previousPoint;
            return false;

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

            var grid = ScreenToGrid(eventData.position);
            _draggedData = GetDataAtPoint(grid);
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

            var localPosition = ScreenToLocalPositionInRenderer(eventData.position);
            var equipmentOffest = GetPivot(_draggedData);
            var offset = equipmentOffest - localPosition;

            _draggedEquipment = new DraggedEquipment(
                _grandCanvas,
                this,
                _draggedData.Position,
                _draggedData,
                offset,
                GetCell(_draggedData.Image, false)
            );

            TryRemove(_draggedData);
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

        void Update()
        {
            if (null == _currentEventData)
                return;

            if (null == _draggedEquipment)
            {
                // Detect hover
                var grid = ScreenToGrid(_currentEventData.position);
                var data = GetDataAtPoint(grid);
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

        internal Vector2 GetPivot(EquipmentData data)
        {
            var x = (-(Width * 0.5f) + data.Position.x + data.Width * 0.5f) * CellSize.x;
            var y = (-(Height * 0.5f) + data.Position.y + data.Height * 0.5f) * CellSize.y;
            return new Vector2(x, y);
        }

        internal Vector2Int ScreenToGrid(Vector2 screenPoint)
        {
            var pos = ScreenToLocalPositionInRenderer(screenPoint);
            var sizeDelta = _rtf.sizeDelta;
            pos.x += sizeDelta.x / 2;
            pos.y += sizeDelta.y / 2;
            return new Vector2Int(Mathf.FloorToInt(pos.x / CellSize.x), Mathf.FloorToInt(pos.y / CellSize.y));
        }

        private Vector2 ScreenToLocalPositionInRenderer(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rtf,
                screenPosition,
                null,
                out var localPosition
            );
            return localPosition;
        }

        public void StartCoolDown(Action<EquipmentData> onCoolDown)
        {
            foreach (var pair in _cellDict)
            {
                var data = pair.Key;
                var image = pair.Value;
                image.StartCoolDownLoop(data.CoolTime, () => onCoolDown?.Invoke(data));
            }

            _canvasGroup.interactable = false;
        }

        public void StopCoolDown()
        {
            foreach (var pair in _cellDict)
            {
                var image = pair.Value;
                image.ResetCool();
            }

            _canvasGroup.interactable = true;
        }
    }
}