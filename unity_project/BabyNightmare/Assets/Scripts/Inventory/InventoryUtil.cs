using UnityEngine;

namespace BabyNightmare.InventorySystem
{
    internal static class InventoryUtil
    {
        internal static Vector2Int GetMinPoint(this Equipment equipment)
        {
            return equipment.Position;
        }
        internal static Vector2Int GetMaxPoint(this Equipment equipment)
        {
            return equipment.Position + new Vector2Int(equipment.Width, equipment.Height);
        }
        internal static bool Contains(this Equipment equipment, Vector2Int inventoryPoint)
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
        internal static bool Overlaps(this Equipment equipment, Equipment otherEquipment)
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
    }
}