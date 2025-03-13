using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Supercent.Util;
using BabyNightmare.StaticData;

namespace BabyNightmare.InventorySystem
{
    public abstract class Inventory : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private const string PATH_EQUIPMENT = "Inventory/Equipment";

        protected RectTransform _rtf;
        protected RectTransform _canvasRTF;
        protected Func<EquipmentData, EquipmentData, EquipmentData> _getUpgradeData;
        private static Inventory _currentInventory = null;
        protected static Equipment _draggedEquipment = null;
        protected static PointerEventData _dragEventData = null;

        public abstract bool TryEquip(Equipment equipment, Vector2 screenPos);
        public abstract void Equip(Equipment equipment);
        public abstract Equipment Unequip(Vector2 screenPos);
        public abstract Equipment Get(Vector2 screenPos);

        private void Awake()
        {
            _rtf = GetComponent<RectTransform>();
            _canvasRTF = GetComponentInParent<Canvas>().transform as RectTransform;
        }

        public void TryAdd(EquipmentData data)
        {
            var equipment = ObjectUtil.LoadAndInstantiate<Equipment>(PATH_EQUIPMENT, transform);
            equipment.Refresh(data, false);
            Equip(equipment);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _currentInventory = this;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
                return;

            var equipment = _currentInventory.Unequip(eventData.position);
            if (null == equipment)
                return;

            equipment.transform.SetParent(_canvasRTF);
            _draggedEquipment = equipment;
            _draggedEquipment.Reset = () =>
            {
                TryEquip(equipment, equipment.Index);
                _draggedEquipment = null;
            };
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (null == _draggedEquipment || null == _currentInventory)
                return;

            _dragEventData = eventData;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRTF, eventData.position, null, out var anchoredPos);

            _draggedEquipment.RTF.anchoredPosition = anchoredPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (null == _draggedEquipment || null == _currentInventory)
                return;

            var screenPos = eventData.position;

            if (true == _currentInventory.TryEquip(_draggedEquipment, screenPos))
            {
                _draggedEquipment = null;
            }
            else
            {
                _draggedEquipment.Reset?.Invoke();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
                return;

            if (null != _draggedEquipment)
                return;

            var equipment = _currentInventory.Get(eventData.position);
            if (null == equipment)
                return;

            InventoryUtil.ShowInfoPopup(equipment.Data);
        }
    }
}