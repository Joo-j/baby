using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics.CodeAnalysis;
using BabyNightmare.StaticData;

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
            Returned,
            Dropped,
        }

        private readonly RectTransform _canvasRect;
        private readonly EquipmentImage _image;
        private Vector2 _offset;

        public Vector2Int OriginPoint { get; private set; }
        public EquipmentData Equipment { get; private set; }
        public Inventory OriginalInventory { get; private set; }
        public Inventory CurrentInventory { get; set; }


        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        public DraggedEquipment(
            Canvas canvas,
            Inventory originalInventory,
            Vector2Int originPoint,
            EquipmentData equipment,
            Vector2 offset,
            EquipmentImage image)
        {
            this.OriginalInventory = originalInventory;
            this.CurrentInventory = originalInventory;
            this.OriginPoint = originPoint;
            this.Equipment = equipment;

            _canvasRect = canvas.transform as RectTransform;

            _offset = offset;

            // Create an image representing the dragged equipment
            _image = image;
            _image.transform.SetParent(_canvasRect);
            _image.transform.SetAsLastSibling();
            _image.transform.localScale = Vector3.one;
            _image.Image.SetNativeSize();
        }

        public Vector2 Position
        {
            set
            {
                // Move the image                
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, value + _offset, null, out var newValue);
                _image.RTF.localPosition = newValue;


                // Make selections
                if (CurrentInventory != null)
                {
                    Equipment.Position = CurrentInventory.ScreenToGrid(value + _offset + GetDraggedEquipmentOffset(CurrentInventory, Equipment));
                    var canAdd = CurrentInventory.CanAddAtPoint(Equipment, Equipment.Position);
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
                if (CurrentInventory.CanAddAtPoint(Equipment, grid))
                {
                    CurrentInventory.TryAddEquipmentAtPoint(Equipment, grid); // Place the equipment in a new location
                    mode = DropMode.Added;
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
                if (false == OriginalInventory.TryDrop(Equipment)) // Drop the equipment on the ground
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
        private Vector2 GetDraggedEquipmentOffset(Inventory manager, EquipmentData equipment)
        {
            var scale = new Vector2(
                Screen.width / _canvasRect.sizeDelta.x,
                Screen.height / _canvasRect.sizeDelta.y
            );
            var gx = -(equipment.Width * manager.CellSize.x / 2f) + (manager.CellSize.x / 2);
            var gy = -(equipment.Height * manager.CellSize.y / 2f) + (manager.CellSize.y / 2);
            return new Vector2(gx, gy) * scale;
        }
    }
}