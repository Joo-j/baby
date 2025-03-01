using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare.InventorySystem
{
    /// <summary>
    /// Class for keeping track of dragged equipments
    /// </summary>
    public class DraggedEquipment
    {
        public enum DropMode
        {
            Added,
            Swapped,
            Returned,
            Dropped,
        }

        private readonly Canvas _canvas;
        private readonly RectTransform _canvasRect;
        private readonly Image _image;
        private Vector2 _offset;

        public Vector2Int OriginPoint { get; private set; }
        public Equipment Equipment { get; private set; }
        public Inventory CurrentInventory;
        public Inventory OriginalInventory { get; private set; }


        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        public DraggedEquipment(
            Canvas canvas,
            Inventory originalInventory,
            Vector2Int originPoint,
            Equipment equipment,
            Vector2 offset)
        {
            OriginalInventory = originalInventory;
            CurrentInventory = OriginalInventory;
            this.OriginPoint = originPoint;
            this.Equipment = equipment;

            _canvas = canvas;
            _canvasRect = canvas.transform as RectTransform;

            _offset = offset;

            // Create an image representing the dragged equipment
            _image = new GameObject("DraggedEquipment").AddComponent<Image>();
            _image.raycastTarget = false;
            _image.transform.SetParent(_canvas.transform);
            _image.transform.SetAsLastSibling();
            _image.transform.localScale = Vector3.one;
            _image.sprite = equipment.Sprite;
            _image.SetNativeSize();
        }

        public Vector2 Position
        {
            set
            {
                // Move the image
                var camera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, value + _offset, camera, out var newValue);
                _image.rectTransform.localPosition = newValue;


                // Make selections
                if (CurrentInventory != null)
                {
                    Equipment.Position = CurrentInventory.ScreenToGrid(value + _offset + GetDraggedEquipmentOffset(CurrentInventory, Equipment));
                    var canAdd = CurrentInventory.CanAddAt(Equipment, Equipment.Position) || CanSwap();
                    CurrentInventory.SelectEquipment(Equipment, !canAdd, Color.white);
                }

                // Slowly animate the equipment towards the center of the mouse pointer
                _offset = Vector2.Lerp(_offset, Vector2.zero, Time.deltaTime * 10f);
            }
        }

        public DropMode Drop(Vector2 pos)
        {
            DropMode mode;
            if (null != CurrentInventory)
            {
                var grid = CurrentInventory.ScreenToGrid(pos + _offset + GetDraggedEquipmentOffset(CurrentInventory, Equipment));

                // Try to add new equipment
                if (CurrentInventory.CanAddAt(Equipment, grid))
                {
                    CurrentInventory.TryAddEquipmentAtPoint(Equipment, grid); // Place the equipment in a new location
                    mode = DropMode.Added;
                }
                // Adding did not work, try to swap
                else if (true == CanSwap())
                {
                    var otherEquipment = CurrentInventory.FirstEquipment;
                    CurrentInventory.TryRemove(otherEquipment);
                    OriginalInventory.TryAdd(otherEquipment);
                    CurrentInventory.TryAdd(Equipment);
                    mode = DropMode.Swapped;
                }
                // Could not add or swap, return the equipment
                else
                {
                    OriginalInventory.TryAddEquipmentAtPoint(Equipment, OriginPoint); // Return the equipment to its previous location
                    mode = DropMode.Returned;

                }

                CurrentInventory.ClearSelection();
            }
            else
            {
                mode = DropMode.Dropped;
                if (false == OriginalInventory.TryForceDrop(Equipment)) // Drop the equipment on the ground
                {
                    OriginalInventory.TryAddEquipmentAtPoint(Equipment, OriginPoint);
                }
            }

            // Destroy the image representing the equipment
            Object.Destroy(_image.gameObject);

            return mode;
        }

        /*
         * Returns the offset between dragged equipment and the grid 
         */
        private Vector2 GetDraggedEquipmentOffset(Inventory manager, Equipment equipment)
        {
            var scale = new Vector2(
                Screen.width / _canvasRect.sizeDelta.x,
                Screen.height / _canvasRect.sizeDelta.y
            );
            var gx = -(equipment.Width * manager.CellSize.x / 2f) + (manager.CellSize.x / 2);
            var gy = -(equipment.Height * manager.CellSize.y / 2f) + (manager.CellSize.y / 2);
            return new Vector2(gx, gy) * scale;
        }

        /* 
         * Returns true if its possible to swap
         */
        private bool CanSwap()
        {
            if (false == CurrentInventory.CanSwap(Equipment))
                return false;
                
            var otherEquipment = CurrentInventory.FirstEquipment;
            return OriginalInventory.CanAdd(otherEquipment) && CurrentInventory.CanRemove(otherEquipment);
        }
    }
}