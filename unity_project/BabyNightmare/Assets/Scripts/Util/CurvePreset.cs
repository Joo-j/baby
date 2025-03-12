using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Util
{
    [CreateAssetMenu(fileName = "CurvePreset", menuName = "BabyNightmare/CurvePreset")]
    public class CurvePreset : ScriptableObject
    {
        public AnimationCurve Linear;
        public AnimationCurve Linear_Reverse;
        public AnimationCurve EaseIn;
        public AnimationCurve EaseOut;
        public AnimationCurve EaseInOut;
        public AnimationCurve EaseOutIn;
        public AnimationCurve EaseInBack;
        public AnimationCurve EaseOutBack;
        public AnimationCurve EaseInOutBack;
        public AnimationCurve EaseOutInBack;
        public AnimationCurve EaseCycle;
    }
}