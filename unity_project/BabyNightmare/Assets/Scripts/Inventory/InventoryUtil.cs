using UnityEngine;
using BabyNightmare.StaticData;
using Supercent.Util;

namespace BabyNightmare.InventorySystem
{
    internal static class InventoryUtil
    {
        private const string PATH_EQUIPMENT_INFO_POPUP = "Inventory/EquipmentInfoPopup";
        private static EquipmentInfoPopup _infoPopup = null;

        internal static Vector2Int GetMinPoint(this EquipmentData equipment)
        {
            return equipment.Position;
        }
        internal static Vector2Int GetMaxPoint(this EquipmentData equipment)
        {
            return equipment.Position + new Vector2Int(equipment.Width, equipment.Height);
        }
        internal static bool Contains(this EquipmentData equipment, Vector2Int inventoryPoint)
        {
            for (var i = 0; i < equipment.Width; i++)
            {
                for (var j = 0; j < equipment.Height; j++)
                {
                    var iPoint = equipment.Position + new Vector2Int(i, j);
                    if (iPoint == inventoryPoint)
                        return true;
                }
            }

            return false;
        }
        internal static bool Overlaps(this EquipmentData equipment, EquipmentData otherEquipment)
        {
            for (var i = 0; i < equipment.Width; i++)
            {
                for (var j = 0; j < equipment.Height; j++)
                {
                    if (equipment.IsPartOfShape(new Vector2Int(i, j)))
                    {
                        var iPoint = equipment.Position + new Vector2Int(i, j);
                        for (var oX = 0; oX < otherEquipment.Width; oX++)
                        {
                            for (var oY = 0; oY < otherEquipment.Height; oY++)
                            {
                                if (otherEquipment.IsPartOfShape(new Vector2Int(oX, oY)))
                                {
                                    var oPoint = otherEquipment.Position + new Vector2Int(oX, oY);
                                    if (oPoint == iPoint) { return true; } // Hit! Equipments overlap
                                }
                            }
                        }
                    }
                }
            }
            return false; // Equipments does not overlap
        }

        internal static void ShowInfoPopup(EquipmentData data)
        {
            if (null == _infoPopup)
            {
                _infoPopup = ObjectUtil.LoadAndInstantiate<EquipmentInfoPopup>(PATH_EQUIPMENT_INFO_POPUP, null);                
            }

            _infoPopup.Show(data);
        }
    }
}