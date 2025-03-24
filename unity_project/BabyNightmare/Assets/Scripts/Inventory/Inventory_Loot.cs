using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BabyNightmare.Util;
using BabyNightmare.StaticData;

namespace BabyNightmare.InventorySystem
{
    public class Inventory_Loot : Inventory
    {
        [SerializeField] private float _spacing;
        [SerializeField] private AnimationCurve _moveCurve;

        private List<Equipment> _equipmentList = new List<Equipment>();

        public List<Equipment> EquipmentList => _equipmentList;

        public override bool TryEquip(Equipment equipment, Vector2 screenPos)
        {
            var getEquipment = Get(screenPos);
            if (null != getEquipment)
            {
                var upgradeData = _getUpgradeData?.Invoke(getEquipment.Data, equipment.Data);
                if (null != upgradeData)
                {
                    Remove(getEquipment);
                    equipment.Refresh(upgradeData, true);
                    Equip(equipment);
                    _showMergeMessage?.Invoke(equipment.transform, $"LV {upgradeData.Level}!");
                }
                else
                {
                    Equip(equipment);
                }

                return true;
            }

            Equip(equipment);
            return true;
        }

        public override void Equip(Equipment equipment, Vector2Int targetIndex)
        {
            Equip(equipment);
        }

        public override void Equip(Equipment equipment)
        {
            equipment.RTF.SetParent(transform);
            _equipmentList.Add(equipment);

            _rtf.sizeDelta += new Vector2(equipment.RTF.sizeDelta.x + _spacing, 0);
            Refresh();
        }

        public override Equipment Unequip(Vector2 screenPos)
        {
            var equipment = Get(screenPos);
            if (null == equipment)
                return null;

            _equipmentList.Remove(equipment);

            _rtf.sizeDelta -= new Vector2(equipment.RTF.sizeDelta.x + _spacing, 0);
            Refresh();
            return equipment;
        }

        private void Remove(Equipment equipment)
        {
            if (null == equipment)
                return;

            if (false == _equipmentList.Contains(equipment))
                return;

            Destroy(equipment.gameObject);
            _equipmentList.Remove(equipment);

            _rtf.sizeDelta -= new Vector2(equipment.RTF.sizeDelta.x + _spacing, 0);
            Refresh();
        }

        public void RemoveAll()
        {
            for (var i = 0; i < _equipmentList.Count; i++)
            {
                var equipment = _equipmentList[i];
                Destroy(equipment.gameObject);
                _rtf.sizeDelta -= new Vector2(equipment.RTF.sizeDelta.x + _spacing, 0);
            }

            _equipmentList.Clear();
            Refresh();
        }

        public override Equipment Get(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rtf, screenPos, null, out var localPos);
            Debug.Log($"{screenPos} {localPos}");

            for (var i = 0; i < _equipmentList.Count; i++)
            {
                var equipment = _equipmentList[i];
                var equipmentPos = equipment.RTF.anchoredPosition;
                var clickSize = equipment.RTF.sizeDelta * 0.5f;
                var dist = Vector2.Distance(equipmentPos, localPos);

                if (dist < clickSize.x || dist < clickSize.y)
                    return equipment;
            }

            return null;
        }


        public override HashSet<Equipment> TryGetOverlap(Equipment data, Vector2 screenPos)
        {
            return null;
        }


        private void Refresh()
        {
            if (_equipmentList.Count == 0)
                return;

            StopAllCoroutines();

            // 전체 너비 계산 (각 장비의 너비 + 간격 포함)
            float totalWidth = _spacing * (_equipmentList.Count - 1);
            foreach (var equipment in _equipmentList)
            {
                totalWidth += equipment.RTF.sizeDelta.x;
            }

            // 시작 x 위치를 중앙 정렬 기준으로 설정
            float x = -totalWidth * 0.5f;

            for (var i = 0; i < _equipmentList.Count; i++)
            {
                var equipment = _equipmentList[i];
                var rtf = equipment.RTF;
                x += rtf.sizeDelta.x * 0.5f;
                var startPos = rtf.anchoredPosition;
                var targetPos = new Vector2(x, 0);
                StartCoroutine(SimpleLerp.Co_LerpAnchoredPosition(rtf, startPos, targetPos, _moveCurve, 0.1f));
                x += rtf.sizeDelta.x * 0.5f;

                if (i < _equipmentList.Count - 1)
                    x += _spacing;
            }
        }
    }
}