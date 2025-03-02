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
        [SerializeField] private int _width = 4;
        [SerializeField] private int _height = 4;
        [SerializeField] private Vector2Int _cellSize = new Vector2Int(32, 32);
        [SerializeField] private Sprite _cellSpriteEmpty = null;
        [SerializeField] private Sprite _cellSpriteSelected = null;
        [SerializeField] private Sprite _cellSpriteBlocked = null;
        [SerializeField] private int _maximumAllowedCount = -1;

        private const string PATH_EQUIPMENT_IMAGE = "Inventory/EquipmentImage";
        private Canvas _grandCanvas = null;
        private Rect _fullRect;
        private EquipmentImage[] _grids;
        private Vector2Int _size = Vector2Int.one;
        private Dictionary<EquipmentData, EquipmentImage> _imageDict = new Dictionary<EquipmentData, EquipmentImage>();
        private Pool<EquipmentImage> _imagePool;
        private PointerEventData _currentEventData;
        private List<EquipmentData> _dataList = new List<EquipmentData>();
        private EquipmentData _draggedData;
        private EquipmentData _lastHoveredData;
        private static DraggedEquipment _draggedEquipment;

        public Vector2 CellSize => _cellSize;
        public int Width => _size.x;
        public int Height => _size.y;
        public EquipmentData FirstEquipmentData => _dataList[0];
        public bool IsInventoryFull
        {
            get
            {
                if (_maximumAllowedCount < 0)
                    return false;

                return _dataList.Count >= _maximumAllowedCount;
            }
        }

        public Action<EquipmentData> OnHovered { get; set; }

        public void Init(Canvas grandCanvas)
        {
            _grandCanvas = grandCanvas;

            var poolTF = new GameObject("PoolTF").AddComponent<RectTransform>();
            poolTF.transform.SetParent(transform);
            poolTF.transform.localPosition = Vector3.zero;
            poolTF.transform.localScale = Vector3.one;

            _imagePool = new Pool<EquipmentImage>(() => ObjectUtil.LoadAndInstantiate<EquipmentImage>(PATH_EQUIPMENT_IMAGE, poolTF));

            ReRenderAllEquipments();

            _size.x = _width;
            _size.y = _height;

            _fullRect = new Rect(0, 0, _size.x, _size.y);

            for (int i = 0; i < _dataList.Count;)
            {
                var data = _dataList[i];
                var shouldBeDropped = false;
                var padding = Vector2.one * 0.01f;

                if (!_fullRect.Contains(data.GetMinPoint() + padding) || !_fullRect.Contains(data.GetMaxPoint() - padding))
                {
                    shouldBeDropped = true;
                }

                if (shouldBeDropped)
                {
                    TryDrop(data);
                }
                else
                {
                    i++;
                }
            }

            ReRenderGrid();
            ReRenderAllEquipments();
        }

        private EquipmentImage GetEuipmentImage(Sprite sprite, bool raycastTarget)
        {
            var img = _imagePool.Get();
            img.gameObject.SetActive(true);
            img.RTF.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
            img.Image.sprite = sprite;
            img.Image.type = Image.Type.Simple;
            img.Image.raycastTarget = raycastTarget;
            img.RTF.SetAsLastSibling();

            return img;
        }

        private void ReturnEuipmentImage(EquipmentImage image)
        {
            image.gameObject.SetActive(false);
            _imagePool.Return(image);
        }

        private void ReRenderGrid()
        {
            if (null != _grids)
            {
                for (var i = 0; i < _grids.Length; i++)
                {
                    ReturnEuipmentImage(_grids[i]);
                    _grids[i].transform.SetSiblingIndex(i);
                }
            }
            _grids = null;

            // Render new grid
            var containerSize = new Vector2(CellSize.x * Width, CellSize.y * Height);
            EquipmentImage grid;
            var topLeft = new Vector3(-containerSize.x / 2, -containerSize.y / 2, 0); // Calculate topleft corner
            var halfCellSize = new Vector3(CellSize.x / 2, CellSize.y / 2, 0); // Calulcate cells half-size
            _grids = new EquipmentImage[Width * Height];
            var c = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    grid = GetEuipmentImage(_cellSpriteEmpty, true);
                    grid.gameObject.name = "Grid " + c;
                    grid.RTF.SetAsFirstSibling();
                    grid.Image.type = Image.Type.Sliced;
                    grid.RTF.localPosition = topLeft + new Vector3(CellSize.x * ((Width - 1) - x), CellSize.y * y, 0) + halfCellSize;
                    grid.RTF.sizeDelta = CellSize;
                    _grids[c] = grid;
                    c++;
                }
            }

            // Set the size of the main RectTransform
            // This is useful as it allowes custom graphical elements
            // suchs as a border to mimic the size of the inventory.
            _rtf.sizeDelta = containerSize;
        }

        /*
        Clears and renders all equipments
        */
        private void ReRenderAllEquipments()
        {
            _imageDict ??= new Dictionary<EquipmentData, EquipmentImage>();

            // Clear all equipments
            foreach (var image in _imageDict.Values)
            {
                ReturnEuipmentImage(image);
            }

            _imageDict.Clear();

            // Add all equipments
            foreach (var data in _dataList)
            {
                AddEquipmentImage(data);
            }
        }

        private void AddEquipmentImage(EquipmentData data)
        {
            var img = GetEuipmentImage(data.Image, false);
            img.RTF.localPosition = GetEquipmentOffset(data);

            _imageDict.Add(data, img);
        }

        /*
        Handler for when inventory.OnEquipmentRemoved is invoked
        */
        private void RemoveEquipmentImage(EquipmentData data)
        {
            if (false == _imageDict.TryGetValue(data, out var image))
                return;

            ReturnEuipmentImage(image);
            _imageDict.Remove(data);
        }

        public void SelectEquipment(EquipmentData data, bool blocked, Color color)
        {
            if (null == data)
                return;

            ClearSelection();

            for (var x = 0; x < data.Width; x++)
            {
                for (var y = 0; y < data.Height; y++)
                {
                    if (true == data.IsPartOfShape(new Vector2Int(x, y)))
                    {
                        var p = data.Position + new Vector2Int(x, y);
                        if (p.x >= 0 && p.x < Width && p.y >= 0 && p.y < Height)
                        {
                            var index = p.y * Width + ((Width - 1) - p.x);
                            _grids[index].Image.sprite = blocked ? _cellSpriteBlocked : _cellSpriteSelected;
                            _grids[index].Image.color = color;
                        }
                    }
                }
            }
        }

        public void ClearSelection()
        {
            for (var i = 0; i < _grids.Length; i++)
            {
                _grids[i].Image.sprite = _cellSpriteEmpty;
                _grids[i].Image.color = Color.white;
            }
        }

        public bool Contains(EquipmentData data) => _dataList.Contains(data);

        public bool TryAdd(EquipmentData data)
        {
            if (false == CanAdd(data))
                return false;

            if (false == GetFirstFitPoint(data, out var point))
                return false;

            return TryAddEquipmentAtPoint(data, point);
        }

        public bool TryAddEquipmentAtPoint(EquipmentData data, Vector2Int point)
        {
            if (false == CanAddAtPoint(data, point))
                return false;

            if (true == _dataList.Contains(data))
            {
                Debug.Log("Already Contains");
                return false;
            }

            _dataList.Add(data);

            data.Position = point;

            AddEquipmentImage(data);
            return true;
        }

        public bool CanAdd(EquipmentData data)
        {
            if (false == _dataList.Contains(data) && true == GetFirstFitPoint(data, out var point))
            {
                return CanAddAtPoint(data, point);
            }

            return false;
        }


        public bool TryRemove(EquipmentData data)
        {
            if (false == _dataList.Contains(data))
                return false;

            if (false == _dataList.Remove(data))
                return false;

            RemoveEquipmentImage(data);
            return true;
        }

        public void RemoveAll()
        {
            _dataList.Clear();

            foreach (var data in _dataList)
            {
                RemoveEquipmentImage(data);
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

            RemoveEquipmentImage(data);
            Debug.Log(data.Name + " was dropped on the ground");

            return true;
        }

        public void DropAll()
        {
            foreach (var data in _dataList)
            {
                TryDrop(data);
            }
        }

        private bool GetFirstFitPoint(EquipmentData data, out Vector2Int point)
        {
            if (true == IsEquipmentFit(data))
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

        private bool IsEquipmentFit(EquipmentData data) => data.Width <= Width && data.Height <= Height;

        private Vector2Int GetCenterPosition(EquipmentData data)
        {
            return new Vector2Int(
                (_size.x - data.Width) / 2,
                (_size.y - data.Height) / 2
            );
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
            if (true == IsInventoryFull)
            {
                Debug.Log("Inventory is Full");
                return false;
            }

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
                ClearSelection();
            }
            else
            {
                ClearHoveredEquipment();
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
            ClearSelection();

            if (null == _draggedData || null != _draggedEquipment)
                return;

            var localPosition = ScreenToLocalPositionInRenderer(eventData.position);
            var equipmentOffest = GetEquipmentOffset(_draggedData);
            var offset = equipmentOffest - localPosition;

            _draggedEquipment = new DraggedEquipment(
                _grandCanvas,
                this,
                _draggedData.Position,
                _draggedData,
                offset,
                GetEuipmentImage(_draggedData.Image, false)
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
                    ClearHoveredEquipment();
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

                OnHovered?.Invoke(data);
                _lastHoveredData = data;
            }
            else
            {
                // Update position while dragging
                _draggedEquipment.Position = _currentEventData.position;
            }
        }

        private void ClearHoveredEquipment()
        {
            if (null != _lastHoveredData)
            {
                OnHovered?.Invoke(null);
            }

            _lastHoveredData = null;
        }


        internal Vector2 GetEquipmentOffset(EquipmentData data)
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
            foreach (var pair in _imageDict)
            {
                var data = pair.Key;
                var image = pair.Value;
                image.StartCoolDownLoop(data.CoolTime, () => onCoolDown?.Invoke(data));
            }
        }

        public void StopCoolDown()
        {
            foreach (var pair in _imageDict)
            {
                var image = pair.Value;
                image.ResetCool();
            }
        }
    }
}