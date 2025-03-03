using UnityEngine;
using UnityEngine.UI;
using BabyNightmare.StaticData;

namespace BabyNightmare.InventorySystem
{
    public class DraggedEquipment
    {
        public enum DropMode
        {
            Added,
            Returned,
            Dropped,
        }

        private readonly RectTransform _canvasRect;
        private readonly DynamicCell _image;
        private Vector2 _offset;

        public Vector2Int OriginPoint { get; private set; }
        public EquipmentData Data { get; private set; }
        public Inventory OriginalInventory { get; private set; }
        public Inventory CurrentInventory { get; set; }


        public DraggedEquipment(
            Canvas canvas,
            Inventory originalInventory,
            Vector2Int originPoint,
            EquipmentData data,
            Vector2 offset,
            DynamicCell image)
        {
            this.OriginalInventory = originalInventory;
            this.CurrentInventory = originalInventory;
            this.OriginPoint = originPoint;
            this.Data = data;

            _canvasRect = canvas.transform as RectTransform;

            _offset = offset;

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
                    Data.Position = CurrentInventory.GetCellPos(value + _offset + GetOffset(CurrentInventory, Data));
                    var canAdd = CurrentInventory.TryOverlap(Data, Data.Position);
                    CurrentInventory.SelectGrid(Data, !canAdd, Color.white);
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
                var grid = CurrentInventory.GetCellPos(pos + _offset + GetOffset(CurrentInventory, Data));

                // Try to add new equipment
                if (CurrentInventory.TryOverlap(Data, grid))
                {
                    CurrentInventory.TryAddCell(Data, grid); // Place the equipment in a new location
                    mode = DropMode.Added;
                }
                // Could not add or swap, return the equipment
                else
                {
                    OriginalInventory.TryAddCell(Data, OriginPoint); // Return the equipment to its previous location
                    mode = DropMode.Returned;

                }

                CurrentInventory.ClearGrid();
            }
            else
            {
                mode = DropMode.Dropped;
                if (false == OriginalInventory.TryRemoveCell(Data)) // Drop the equipment on the ground
                {
                    OriginalInventory.TryAddCell(Data, OriginPoint);
                }
            }

            // Destroy the image representing the equipment
            Object.Destroy(_image.gameObject);

            return mode;
        }

        /*
         * Returns the offset between dragged equipment and the grid 
         */
        private Vector2 GetOffset(Inventory manager, EquipmentData data)
        {
            var scale = new Vector2(
                Screen.width / _canvasRect.sizeDelta.x,
                Screen.height / _canvasRect.sizeDelta.y
            );
            var gx = -(data.Width * manager.CellSize.x / 2f) + (manager.CellSize.x / 2);
            var gy = -(data.Height * manager.CellSize.y / 2f) + (manager.CellSize.y / 2);
            return new Vector2(gx, gy) * scale;
        }
    }
}