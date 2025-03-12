using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Util
{
    public static class CurveHelper
    {
        private const string PATH_PRESET = "Util/CurvePreset";
        public static CurvePreset Preset { get; private set; }

        static CurveHelper()
        {
            Preset = Resources.Load<CurvePreset>(PATH_PRESET);
        }
    }
}