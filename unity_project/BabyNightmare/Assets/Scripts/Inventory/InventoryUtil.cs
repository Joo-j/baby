using UnityEngine;
using BabyNightmare.StaticData;
using Supercent.Util;

namespace BabyNightmare.InventorySystem
{
    internal static class InventoryUtil
    {
        private const string PATH_EQUIPMENT_INFO_POPUP = "Inventory/EquipmentInfoPopup";
        private static EquipmentInfoPopup _infoPopup = null;

        public static void ShowInfoPopup(EquipmentData data)
        {
            if (null == _infoPopup)
            {
                _infoPopup = ObjectUtil.LoadAndInstantiate<EquipmentInfoPopup>(PATH_EQUIPMENT_INFO_POPUP, null);
                Debug.Assert(null != _infoPopup, $"{PATH_EQUIPMENT_INFO_POPUP} is not prefab");
            }

            _infoPopup.Show(data);
        }
    }
}