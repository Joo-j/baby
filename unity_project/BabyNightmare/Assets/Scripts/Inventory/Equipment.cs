using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BabyNightmare.StaticData;

namespace BabyNightmare.InventorySystem
{
    public class Equipment
    {
        public DynamicCell Cell;
        public EquipmentData Data;

        public Vector2Int Position { get; set; }

        public Equipment(DynamicCell cell, EquipmentData data)
        {
            this.Cell = cell;
            this.Data = data;
        }
    }
}