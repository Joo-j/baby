using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using Supercent.Util;
using BabyNightmare.StaticData;

namespace BabyNightmare.InventorySystem
{
    public abstract class Inventory : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const string PATH_EQUIPMENT = "Inventory/Equipment";

        protected RectTransform _rtf;
        protected RectTransform _canvasRTF;
        protected Func<EquipmentData, EquipmentData, EquipmentData> _getUpgradeData;
        private static Inventory _dragStartInventory = null;
        private static Inventory _currentInventory = null;
        protected static Equipment _draggedEquipment = null;
        protected static PointerEventData _dragEventData = null;

        public abstract bool TryEquip(Equipment equipment, Vector2 screenPos);
        public abstract void Equip(Equipment equipment, Vector2Int targetIndex);
        public abstract void Equip(Equipment equipment);
        public abstract Equipment Unequip(Vector2 screenPos);
        public abstract Equipment Get(Vector2 screenPos);

        private void Awake()
        {
            _rtf = GetComponent<RectTransform>();
            _canvasRTF = GetComponentInParent<Canvas>().transform as RectTransform;
        }

        protected virtual void OnEnable()
        {
            StartCoroutine(Co_DetectEquipment());
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

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
                return;

            var equipment = _currentInventory.Unequip(eventData.position);
            if (null == equipment)
                return;

            equipment.transform.SetParent(_canvasRTF);
            _draggedEquipment = equipment;

            _dragStartInventory = this;
        }

        private IEnumerator Co_DetectEquipment()
        {
            while (true)
            {
                yield return null;

                if (null == _dragEventData)
                    continue;

                if (null == _draggedEquipment)
                    continue;

                var equipment = Get(_dragEventData.position);
                if (null == equipment)
                    continue;

                var upgradeData = _getUpgradeData?.Invoke(_draggedEquipment.Data, equipment.Data);
                if (null != upgradeData)
                {
                    equipment.Swing();
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (null != _draggedEquipment || null == _currentInventory)
                return;

            var equipment = _currentInventory.Get(eventData.position);
            if (null == equipment)
                return;

            InventoryUtil.ShowInfoPopup(equipment.Data);
            _draggedEquipment = null;
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

            if (false == _currentInventory.TryEquip(_draggedEquipment, screenPos))
            {
                _dragStartInventory.Equip(_draggedEquipment, _draggedEquipment.Index);
            }

            _draggedEquipment = null;
            _dragStartInventory = null;
        }
    }
}