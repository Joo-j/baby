using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using BabyNightmare.StaticData;

namespace BabyNightmare.InventorySystem
{
    public abstract class Inventory : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const string PATH_EQUIPMENT = "Inventory/Equipment";

        protected RectTransform _rtf = null;
        protected RectTransform _canvasRTF = null;
        protected Func<EquipmentData, EquipmentData, EquipmentData> _getUpgradeData = null;
        protected Action<Transform, String> _showMergeMessage = null;
        protected Action<Equipment, HashSet<Equipment>> _refreshChangeStat = null;

        private static Inventory _dragStartInventory = null;
        private static Inventory _currentInventory = null;
        protected static Equipment _draggedEquipment = null;
        protected static PointerEventData _dragEventData = null;

        public abstract void Equip(Equipment equipment, bool immediate);
        public abstract bool TryEquip(Equipment equipment, Vector2 screenPos);
        public abstract void Equip(Equipment equipment, Vector2Int targetIndex);
        public abstract Equipment Unequip(Vector2 screenPos);
        public abstract Equipment Get(Vector2 screenPos);
        public abstract HashSet<Equipment> TryGetOverlap(Equipment equipment, Vector2 screenPos);

        private void Awake()
        {
            _rtf = GetComponent<RectTransform>();
            _canvasRTF = GetComponentInParent<Canvas>().transform as RectTransform;
        }

        protected virtual void OnEnable()
        {
            StartCoroutine(Co_Refresh());
        }

        public void InitBase
        (
            Func<EquipmentData, EquipmentData, EquipmentData> getUpgradeData,
            Action<Transform, String> showMergeMessage,
            Action<Equipment, HashSet<Equipment>> refreshChangeStat
        )
        {
            _getUpgradeData = getUpgradeData;
            _showMergeMessage = showMergeMessage;
            _refreshChangeStat = refreshChangeStat;
        }

        public void TryAdd(EquipmentData data)
        {
            var equipment = ObjectUtil.LoadAndInstantiate<Equipment>(PATH_EQUIPMENT, transform);
            equipment.Refresh(data, false);
            Equip(equipment, true);
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

        private IEnumerator Co_Refresh()
        {
            while (true)
            {
                yield return null;
                RefreshStatChange();
                RefreshUpgradable();
            }
        }

        private void RefreshUpgradable()
        {
            if (null == _dragEventData)
                return;

            if (null == _draggedEquipment)
                return;

            if (null == _currentInventory)
                return;

            var equipment = _currentInventory.Get(_dragEventData.position);
            if (null == equipment)
                return;

            var upgradeData = _getUpgradeData?.Invoke(_draggedEquipment.Data, equipment.Data);
            if (null != upgradeData)
            {
                equipment.Swing();
            }
        }

        private void RefreshStatChange()
        {
            if (null == _dragEventData || null == _currentInventory || null == _draggedEquipment)
            {
                _refreshChangeStat?.Invoke(null, null);
                return;
            }

            var overlapSet = _currentInventory.TryGetOverlap(_draggedEquipment, _dragEventData.position);
            _refreshChangeStat?.Invoke(_draggedEquipment, overlapSet);

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (null != _draggedEquipment)
                return;

            if (null == _currentInventory)
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