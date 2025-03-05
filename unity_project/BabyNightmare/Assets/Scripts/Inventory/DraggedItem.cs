using UnityEngine;
using UnityEngine.UI;
using BabyNightmare.StaticData;

namespace BabyNightmare.InventorySystem
{
    public class DraggedItem
    {
        public enum DropMode
        {
            Added,
            Returned,
            Dropped,
        }

        private RectTransform _canvasRect;
        private Equipment _equipment;
        private Vector2 _offset;
        private Vector2Int _originPos;
        private DynamicCell _cell = null;

        private Inventory _originOwner = null;
        private Inventory _currentOwner = null;
        private EquipmentData _data { get; }

        public DraggedItem
        (
            RectTransform canvasRect,
            Inventory originalOwner,
            Equipment equipment,
            Vector2 offset)
        {
            this._canvasRect = canvasRect;
            this._originOwner = originalOwner;
            this._currentOwner = originalOwner;
            this._equipment = equipment;
            this._originPos = equipment.Point;
            this._offset = offset;

            _data = equipment.Data;

            _cell = GameObject.Instantiate(_equipment.Cell);
            _cell.RTF.SetParent(_canvasRect);
            _cell.RTF.SetAsLastSibling();
            _cell.RTF.localScale = Vector3.one;
            _cell.Image.SetNativeSize();
        }

        public void SetPosition(Vector2 value)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, value + _offset, null, out var localPos);
            _cell.RTF.localPosition = localPos;

            if (null != _currentOwner)
            {
                _equipment.Point = _currentOwner.GetCellPos(value + _offset + GetOffset(_data));
                var isAddable = _currentOwner.IsAddable(_data, _equipment.Point);
                _currentOwner.PaintBG(_equipment, false == isAddable, Color.white);
            }

            // Slowly animate the equipment towards the center of the mouse pointer
            _offset = Vector2.Lerp(_offset, Vector2.zero, Time.deltaTime * 10f);
        }

        public DropMode Drop(Vector2 pos)
        {
            DropMode mode;
            if (null != _currentOwner)
            {
                var cellPos = _currentOwner.GetCellPos(pos + _offset + GetOffset(_data));

                if (true == _currentOwner.IsAddable(_data, cellPos))
                {
                    _currentOwner.TryAdd(_data, cellPos);
                    mode = DropMode.Added;
                }
                else
                {
                    _originOwner.TryAdd(_data, _originPos);
                    mode = DropMode.Returned;
                }

                _currentOwner.ClearBG();
            }
            else
            {
                mode = DropMode.Dropped;
                if (false == _originOwner.TryRemove(_equipment))
                {
                    _originOwner.TryAdd(_data, _originPos);
                }
            }

            Object.Destroy(_cell.GO);
            _cell = null;

            return mode;
        }

        private Vector2 GetOffset(EquipmentData data)
        {
            if (null == _currentOwner)
                return default;

            var cellSize = _currentOwner.CellSize;

            var scale = new Vector2(Screen.width / _canvasRect.sizeDelta.x, Screen.height / _canvasRect.sizeDelta.y);
            var gx = -(data.Row * cellSize.x / 2f) + (cellSize.x / 2);
            var gy = -(data.Column * cellSize.y / 2f) + (cellSize.y / 2);
            return new Vector2(gx, gy) * scale;
        }

        public void SetOwner(Inventory owner)
        {
            _currentOwner = owner;
        }
    }
}