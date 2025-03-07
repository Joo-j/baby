using BabyNightmare.StaticData;
using UnityEngine;

namespace BabyNightmare.InventorySystem
{
    public class DraggedItem
    {
        private RectTransform _canvasRect = null;

        public Inventory OriginOwner { get; private set; }
        public Inventory CurrentOwner { get; set; }
        public Equipment Equipment { get; private set; }
        public Vector2Int OriginPoint { get; private set; }
        public Vector2 Offset { get; private set; }
        public DynamicCell Cell { get; private set; }
        public bool IsDragging { get; private set; }

        public DraggedItem(RectTransform canvasRect)
        {
            _canvasRect = canvasRect;
        }

        public void Init(DynamicCell cell, Inventory owner, Equipment equipment, Vector2 offset)
        {
            OriginOwner = owner;
            CurrentOwner = owner;
            Equipment = equipment;
            OriginPoint = equipment.Point;
            Offset = offset;

            Cell = GameObject.Instantiate(cell);
            Cell.RTF.SetParent(_canvasRect);
            Cell.RTF.SetAsLastSibling();
            Cell.RTF.localScale = Vector3.one;
            Cell.Image.SetNativeSize();

            IsDragging = true;
        }

        public void Release(Vector2 point)
        {
            if (false == IsDragging)
                return;

            GameObject.Destroy(Cell.GO);
            Cell = null;

            IsDragging = false;

            if (null == CurrentOwner)
                return;

            var newPoint = CurrentOwner.GetPoint(point + Offset + GetOffset());
            var data = Equipment.Data;

            if (false == CurrentOwner.IsValid(data, newPoint))
            {
                OriginOwner.TryAdd(data, OriginPoint);
                return;
            }

            var overlappedEquipments = CurrentOwner.GetOverlapEquipments(data, newPoint);
            var overlapCount = overlappedEquipments.Count;

            if (overlapCount == 0)
            {
                CurrentOwner.TryAdd(data, newPoint);
                return;
            }

            if (overlapCount > 1)
            {
                foreach (var equipment in overlappedEquipments)
                {
                    CurrentOwner.TryRemove(equipment);
                    OriginOwner.TryAdd(equipment.Data);
                }

                CurrentOwner.TryAdd(data, newPoint);
                return;
            }


            foreach (var equipment in overlappedEquipments)
            {
                var upgradeData = StaticDataManager.Instance.GetEquipmentData(data.UpgradeDataID);
                if (equipment.Data.ID == data.ID && null != upgradeData)
                {
                    CurrentOwner.TryRemove(equipment);
                    CurrentOwner.TryAdd(upgradeData, newPoint);
                    Debug.Log("Upgrade");
                    return;
                }

                CurrentOwner.TryRemove(equipment);
                OriginOwner.TryAdd(equipment.Data);
                CurrentOwner.TryAdd(data, newPoint);
            }
        }

        public void SetPosition(Vector2 pos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, pos + Offset, null, out var localPos);
            Cell.RTF.localPosition = localPos;

            if (null != CurrentOwner)
            {
                Equipment.Point = CurrentOwner.GetPoint(pos + Offset + GetOffset());
                CurrentOwner.PaintBG(Equipment, Color.white);
            }

            Offset = Vector2.Lerp(Offset, Vector2.zero, Time.deltaTime * 10f);
        }

        private Vector2 GetOffset()
        {
            if (null == CurrentOwner)
                return default;

            var data = Equipment.Data;
            var cellSize = CurrentOwner.CellSize;

            var scale = new Vector2(Screen.width / _canvasRect.sizeDelta.x, Screen.height / _canvasRect.sizeDelta.y);
            var gx = -(data.Column * cellSize.x / 2f) + (cellSize.x / 2);
            var gy = -(data.Row * cellSize.y / 2f) + (cellSize.y / 2);
            return new Vector2(gx, gy) * scale;
        }
    }
}