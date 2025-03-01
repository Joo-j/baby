using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BabyNightmare.Util;
using UnityEngine.EventSystems;

namespace BabyNightmare.InventorySystem
{
    public enum InventoryRenderMode
    {
        Grid,
        Single,
    }

    public class Inventory : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerExitHandler, IPointerEnterHandler
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private int _width = 4;
        [SerializeField] private int _height = 4;
        [SerializeField] private Vector2Int _cellSize = new Vector2Int(32, 32);
        [SerializeField] private Sprite _cellSpriteEmpty = null;
        [SerializeField] private Sprite _cellSpriteSelected = null;
        [SerializeField] private Sprite _cellSpriteBlocked = null;
        [SerializeField] private int _maximumAllowedCount = -1;
        [SerializeField] private InventoryRenderMode _renderMode = InventoryRenderMode.Grid;
        [SerializeField] private Equipment[] _initEquipments;

        private Canvas _canvas;
        private Image[] _grids;
        private Vector2Int _size = Vector2Int.one;
        private Rect _fullRect;
        private Pool<Image> _imagePool;
        private Dictionary<Equipment, Image> _equipmentImageDict = new Dictionary<Equipment, Image>();

        private List<Equipment> _equipments = new List<Equipment>();
        private Equipment _equipmentToDrag;
        private Equipment _lastHoveredEquipment;
        private PointerEventData _currentEventData;
        private static DraggedEquipment _draggedEquipment;

        public Vector2 CellSize => _cellSize;
        public int Width => _size.x;
        public int Height => _size.y;
        public Equipment FirstEquipment => _equipments[0];
        public bool IsInventoryFull
        {
            get
            {
                if (_maximumAllowedCount < 0)
                    return false;

                return _equipments.Count >= _maximumAllowedCount;
            }
        }

        public Action<Equipment> OnHovered { get; set; }

        void Awake()
        {
            var imageContainer = new GameObject("Image Pool").AddComponent<RectTransform>();
            imageContainer.transform.SetParent(transform);
            imageContainer.transform.localPosition = Vector3.zero;
            imageContainer.transform.localScale = Vector3.one;

            _imagePool = new Pool<Image>(
                delegate
                {
                    var image = new GameObject("Image").AddComponent<Image>();
                    image.transform.SetParent(imageContainer);
                    image.transform.localScale = Vector3.one;
                    return image;
                });


            // Find the canvas
            var canvases = GetComponentsInParent<Canvas>();
            if (canvases.Length == 0)
            {
                throw new NullReferenceException("Could not find a canvas.");
            }

            _canvas = canvases[canvases.Length - 1];
        }


        private void Start()
        {
            Rebuild(false);

            _size.x = _width;
            _size.y = _height;

            _fullRect = new Rect(0, 0, _size.x, _size.y);

            for (int i = 0; i < _equipments.Count;)
            {
                var equipment = _equipments[i];
                var shouldBeDropped = false;
                var padding = Vector2.one * 0.01f;

                if (!_fullRect.Contains(equipment.GetMinPoint() + padding) || !_fullRect.Contains(equipment.GetMaxPoint() - padding))
                {
                    shouldBeDropped = true;
                }

                if (shouldBeDropped)
                {
                    TryDrop(equipment);
                }
                else
                {
                    i++;
                }
            }

            ReRenderGrid();
            ReRenderAllEquipments();

            for (var i = 0; i < _initEquipments.Length; i++)
            {
                var equipment = _initEquipments[i];
                var inst = equipment.CreateInstance();
                if (null == inst)
                    break;

                TryAdd(inst);
            }

            ReRenderGrid();
            ReRenderAllEquipments();
        }


        private void Rebuild(bool silent)
        {
            if (false == silent)
                ReRenderAllEquipments();
        }


        private void ReRenderGrid()
        {
            // Clear the grid
            if (null != _grids)
            {
                for (var i = 0; i < _grids.Length; i++)
                {
                    _grids[i].gameObject.SetActive(false);
                    ReturnImage(_grids[i]);
                    _grids[i].transform.SetSiblingIndex(i);
                }
            }
            _grids = null;

            // Render new grid
            var containerSize = new Vector2(CellSize.x * Width, CellSize.y * Height);
            Image grid;
            switch (_renderMode)
            {
                case InventoryRenderMode.Single:
                    grid = GetImage(_cellSpriteEmpty, true);
                    grid.rectTransform.SetAsFirstSibling();
                    grid.type = Image.Type.Sliced;
                    grid.rectTransform.localPosition = Vector3.zero;
                    grid.rectTransform.sizeDelta = containerSize;
                    _grids = new[] { grid };
                    break;
                default:
                    // Spawn grid images
                    var topLeft = new Vector3(-containerSize.x / 2, -containerSize.y / 2, 0); // Calculate topleft corner
                    var halfCellSize = new Vector3(CellSize.x / 2, CellSize.y / 2, 0); // Calulcate cells half-size
                    _grids = new Image[Width * Height];
                    var c = 0;
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            grid = GetImage(_cellSpriteEmpty, true);
                            grid.gameObject.name = "Grid " + c;
                            grid.rectTransform.SetAsFirstSibling();
                            grid.type = Image.Type.Sliced;
                            grid.rectTransform.localPosition = topLeft + new Vector3(CellSize.x * ((Width - 1) - x), CellSize.y * y, 0) + halfCellSize;
                            grid.rectTransform.sizeDelta = CellSize;
                            _grids[c] = grid;
                            c++;
                        }
                    }
                    break;
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
            _equipmentImageDict ??= new Dictionary<Equipment, Image>();

            // Clear all equipments
            foreach (var image in _equipmentImageDict.Values)
            {
                image.gameObject.SetActive(false);
                ReturnImage(image);
            }

            _equipmentImageDict.Clear();

            // Add all equipments
            foreach (var equipment in _equipments)
            {
                HandleAdded(equipment);
            }
        }

        /*
        Handler for when inventory.OnEquipmentAdded is invoked
        */
        private void HandleAdded(Equipment equipment)
        {
            var img = GetImage(equipment.Sprite, false);

            if (_renderMode == InventoryRenderMode.Single)
            {
                img.rectTransform.localPosition = _rtf.rect.center;
            }
            else
            {
                img.rectTransform.localPosition = GetEquipmentOffset(equipment);
            }

            _equipmentImageDict.Add(equipment, img);
        }

        /*
        Handler for when inventory.OnEquipmentRemoved is invoked
        */
        private void HandleRemoved(Equipment equipment)
        {
            if (false == _equipmentImageDict.TryGetValue(equipment, out var image))
                return;

            image.gameObject.SetActive(false);
            ReturnImage(image);
            _equipmentImageDict.Remove(equipment);
        }


        public void SelectEquipment(Equipment equipment, bool blocked, Color color)
        {
            if (null == equipment)
                return;

            ClearSelection();

            switch (_renderMode)
            {
                case InventoryRenderMode.Single:
                    _grids[0].sprite = blocked ? _cellSpriteBlocked : _cellSpriteSelected;
                    _grids[0].color = color;
                    break;
                default:
                    for (var x = 0; x < equipment.Width; x++)
                    {
                        for (var y = 0; y < equipment.Height; y++)
                        {
                            if (true == equipment.IsPartOfShape(new Vector2Int(x, y)))
                            {
                                var p = equipment.Position + new Vector2Int(x, y);
                                if (p.x >= 0 && p.x < Width && p.y >= 0 && p.y < Height)
                                {
                                    var index = p.y * Width + ((Width - 1) - p.x);
                                    _grids[index].sprite = blocked ? _cellSpriteBlocked : _cellSpriteSelected;
                                    _grids[index].color = color;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Clears all selections made in this inventory
        /// </summary>
        public void ClearSelection()
        {
            for (var i = 0; i < _grids.Length; i++)
            {
                _grids[i].sprite = _cellSpriteEmpty;
                _grids[i].color = Color.white;
            }
        }

        /*
        Returns the appropriate offset of an equipment to make it fit nicely in the grid
        */
        internal Vector2 GetEquipmentOffset(Equipment equipment)
        {
            var x = (-(Width * 0.5f) + equipment.Position.x + equipment.Width * 0.5f) * CellSize.x;
            var y = (-(Height * 0.5f) + equipment.Position.y + equipment.Height * 0.5f) * CellSize.y;
            return new Vector2(x, y);
        }

        public bool TryRemove(Equipment equipment)
        {
            if (false == CanRemove(equipment))
                return false;

            if (false == _equipments.Remove(equipment))
                return false;

            Rebuild(true);
            HandleRemoved(equipment);
            return true;
        }


        public bool TryDrop(Equipment equipment)
        {
            if (false == CanDrop(equipment))
            {
                Debug.Log($"You're not allowed to drop {equipment.Name} on the ground");
                return false;
            }

            if (false == _equipments.Remove(equipment))
            {
                Debug.Log($"You're not allowed to drop {equipment.Name} on the ground");
                return false;
            }

            Rebuild(true);
            HandleRemoved(equipment);
            Debug.Log(equipment.Name + " was dropped on the ground");

            return true;
        }

        internal bool TryForceDrop(Equipment equipment)
        {
            if (false == equipment.CanDrop)
            {
                Debug.Log($"You're not allowed to drop {equipment.Name} on the ground");
                return false;
            }

            HandleRemoved(equipment);
            Debug.Log((equipment as Equipment).Name + " was dropped on the ground");
            return true;
        }


        public bool CanAddAt(Equipment equipment, Vector2Int point)
        {
            if (true == IsInventoryFull)
            {
                return false;
            }

            if (_renderMode == InventoryRenderMode.Single)
            {
                return true;
            }

            var previousPoint = equipment.Position;
            equipment.Position = point;
            var padding = Vector2.one * 0.01f;

            // Check if equipment is outside of inventory
            if (!_fullRect.Contains(equipment.GetMinPoint() + padding) || !_fullRect.Contains(equipment.GetMaxPoint() - padding))
            {
                equipment.Position = previousPoint;
                return false;
            }

            // Check if equipment overlaps another equipment already in the inventory
            if (false == _equipments.Any(otherEquipment => equipment.Overlaps(otherEquipment)))
                return true; // Equipment can be added
            equipment.Position = previousPoint;
            return false;

        }


        public bool TryAddEquipmentAtPoint(Equipment equipment, Vector2Int point)
        {
            if (false == CanAddAt(equipment, point))
            {
                Debug.Log($"You can't put {equipment.Name} there!");
                return false;
            }

            if (true == _equipments.Contains(equipment))
            {
                Debug.Log($"You can't put {equipment.Name} there!");
                return false;
            }

            _equipments.Add(equipment);

            switch (_renderMode)
            {
                case InventoryRenderMode.Single:
                    equipment.Position = GetCenterPosition(equipment);
                    break;
                case InventoryRenderMode.Grid:
                    equipment.Position = point;
                    break;
                default:
                    throw new NotImplementedException($"InventoryRenderMode.{_renderMode} have not yet been implemented");
            }
            Rebuild(true);
            HandleAdded(equipment);
            return true;
        }


        public bool CanAdd(Equipment equipment)
        {
            Vector2Int point;
            if (false == Contains(equipment) && GetFirstPointThatFitsEquipment(equipment, out point))
            {
                return CanAddAt(equipment, point);
            }
            return false;
        }

        public bool TryAdd(Equipment equipment)
        {
            if (false == CanAdd(equipment))
                return false;

            if (false == GetFirstPointThatFitsEquipment(equipment, out var point))
                return false;

            return TryAddEquipmentAtPoint(equipment, point);
        }

        public bool CanSwap(Equipment equipment)
        {
            return _renderMode == InventoryRenderMode.Single && IsEquipmentFit(equipment);
        }

        public void DropAll()
        {
            foreach (var equipment in _equipments)
            {
                TryDrop(equipment);
            }
        }

        public void Clear()
        {
            foreach (var equipment in _equipments)
            {
                TryRemove(equipment);
            }
        }

        public bool Contains(Equipment equipment) => _equipments.Contains(equipment);
        public bool CanRemove(Equipment equipment) => Contains(equipment);
        public bool CanDrop(Equipment equipment) => Contains(equipment) && equipment.CanDrop;

        /*
         * Get first free point that will fit the given equipment
         */
        private bool GetFirstPointThatFitsEquipment(Equipment equipment, out Vector2Int point)
        {
            if (true == IsEquipmentFit(equipment))
            {
                for (var x = 0; x < Width - (equipment.Width - 1); x++)
                {
                    for (var y = 0; y < Height - (equipment.Height - 1); y++)
                    {
                        point = new Vector2Int(x, y);
                        if (CanAddAt(equipment, point)) return true;
                    }
                }
            }
            point = Vector2Int.zero;
            return false;
        }

        private bool IsEquipmentFit(Equipment equipment) => equipment.Width <= Width && equipment.Height <= Height;

        private Vector2Int GetCenterPosition(Equipment equipment)
        {
            return new Vector2Int(
                (_size.x - equipment.Width) / 2,
                (_size.y - equipment.Height) / 2
            );
        }

        private Equipment GetEquipmentAtPoint(Vector2Int point)
        {
            // Single equipment override
            if (_renderMode == InventoryRenderMode.Single && IsInventoryFull && _equipments.Count > 0)
            {
                return _equipments[0];
            }

            foreach (var equipment in _equipments)
            {
                if (true == equipment.Contains(point))
                {
                    return equipment;
                }
            }

            return null;
        }

        /*
         * Grid was clicked (IPointerDownHandler)
         */
        public void OnPointerDown(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
                return;

            // Get which equipment to drag (equipment will be null of none were found)
            var grid = ScreenToGrid(eventData.position);
            _equipmentToDrag = GetEquipmentAtPoint(grid);
        }

        /*
         * Dragging started (IBeginDragHandler)
         */
        public void OnBeginDrag(PointerEventData eventData)
        {
            ClearSelection();

            if (null == _equipmentToDrag || null != _draggedEquipment)
                return;

            var localPosition = ScreenToLocalPositionInRenderer(eventData.position);
            var equipmentOffest = GetEquipmentOffset(_equipmentToDrag);
            var offset = equipmentOffest - localPosition;

            // Create a dragged equipment 
            _draggedEquipment = new DraggedEquipment(
                _canvas,
                this,
                _equipmentToDrag.Position,
                _equipmentToDrag,
                offset
            );

            // Remove the equipment from inventory
            TryRemove(_equipmentToDrag);
        }

        /*
         * Dragging is continuing (IDragHandler)
         */
        public void OnDrag(PointerEventData eventData)
        {
            _currentEventData = eventData;
            if (_draggedEquipment != null)
            {
                // Update the equipments position
                //_draggedEquipment.Position = eventData.position;
            }
        }

        /*
         * Dragging stopped (IEndDragHandler)
         */
        public void OnEndDrag(PointerEventData eventData)
        {
            if (null == _draggedEquipment)
                return;

            var mode = _draggedEquipment.Drop(eventData.position);

            switch (mode)
            {
                case DraggedEquipment.DropMode.Added:
                    break;
                case DraggedEquipment.DropMode.Swapped:
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

        /*
         * Pointer left the inventory (IPointerExitHandler)
         */
        public void OnPointerExit(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
            {
                // Clear the equipment as it leaves its current controller
                _draggedEquipment.CurrentInventory = null;
                ClearSelection();
            }
            else
            {
                ClearHoveredEquipment();
            }

            _currentEventData = null;
        }

        /*
         * Pointer entered the inventory (IPointerEnterHandler)
         */
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
            {
                _draggedEquipment.CurrentInventory = this;
            }

            _currentEventData = eventData;
        }

        /*
         * Update loop
         */
        void Update()
        {
            if (null == _currentEventData)
                return;

            if (null == _draggedEquipment)
            {
                // Detect hover
                var grid = ScreenToGrid(_currentEventData.position);
                var equipment = GetEquipmentAtPoint(grid);
                if (equipment == _lastHoveredEquipment)
                    return;

                OnHovered?.Invoke(equipment);
                _lastHoveredEquipment = equipment;
            }
            else
            {
                // Update position while dragging
                _draggedEquipment.Position = _currentEventData.position;
            }
        }

        private void ClearHoveredEquipment()
        {
            if (_lastHoveredEquipment != null)
            {
                OnHovered?.Invoke(null);
            }
            _lastHoveredEquipment = null;
        }

        /*
         * Get a point on the grid from a given screen point
         */
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
                _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
                out var localPosition
            );
            return localPosition;
        }

        private Image GetImage(Sprite sprite, bool raycastTarget)
        {
            var img = _imagePool.Take();
            img.gameObject.SetActive(true);
            img.sprite = sprite;
            img.rectTransform.sizeDelta = new Vector2(img.sprite.rect.width, img.sprite.rect.height);
            img.transform.SetAsLastSibling();
            img.type = Image.Type.Simple;
            img.raycastTarget = raycastTarget;
            return img;
        }

        private void ReturnImage(Image image)
        {
            image.gameObject.name = "Image";
            image.gameObject.SetActive(false);
            _imagePool.Recycle(image);
        }
    }
}