using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Lofelt.NiceVibrations.HapticPatterns;

namespace BabyNightmare.Util
{
    public static class HapticHelper
    {
        public static bool Enable { private get; set; } = false;

        public static void Init()
        {
            Enable = PlayerData.Instance.Haptic_Active;
        }

        public static void Haptic(PresetType type = PresetType.LightImpact)
        {
            if (false == Enable)
                return;

            PlayPreset(type);
        }
    }
}