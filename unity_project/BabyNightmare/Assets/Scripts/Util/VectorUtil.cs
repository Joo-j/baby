using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Util
{
    public static class VectorUtil
    {
        public static Vector2 CalcBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            var u = 1f - t;
            return (p0 * u * u)
                 + (p1 * u * t * 2f)
                 + (p2 * t * t);
        }
    }
}
