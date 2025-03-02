using UnityEngine;

namespace Supercent.Util
{
    public static class VectorExtensions
    {
        public static Vector3 Multiply(this Vector3 a, Vector3 b)           => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector4 Multiply(this Vector4 a, Vector4 b)           => new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);

        public static Vector3 Divide(this Vector3 a, Vector3 b)             => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        public static Vector4 Divide(this Vector4 a, Vector4 b)             => new Vector4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
        public static Vector2Int Divide(this Vector2Int a, Vector2Int b)    => new Vector2Int(a.x / b.x, a.y / b.y);
        public static Vector3Int Divide(this Vector3Int a, Vector3Int b)    => new Vector3Int(a.x / b.x, a.y / b.y, a.z / b.z);

        public static Vector2 Remainder(this Vector2 a, Vector2 b)          => new Vector2(a.x % b.x, a.y % b.y);
        public static Vector3 Remainder(this Vector3 a, Vector3 b)          => new Vector3(a.x % b.x, a.y % b.y, a.z % b.z);
        public static Vector4 Remainder(this Vector4 a, Vector4 b)          => new Vector4(a.x % b.x, a.y % b.y, a.z % b.z, a.w % b.w);
        public static Vector2Int Remainder(this Vector2Int a, Vector2Int b) => new Vector2Int(a.x % b.x, a.y % b.y);
        public static Vector3Int Remainder(this Vector3Int a, Vector3Int b) => new Vector3Int(a.x % b.x, a.y % b.y, a.z % b.z);

        public static Vector2Int DivRem(this Vector2Int a, Vector2Int b, out Vector2Int result)
        {
            var div = a.Divide(b);
            result = a - (div * b);
            return div;
        }
        public static Vector3Int DivRem(this Vector3Int a, Vector3Int b, out Vector3Int result)
        {
            var div = a.Divide(b);
            result = a - (div * b);
            return div;
        }


        public static float SqrDistance(this Vector2 a, Vector2 b)          => Vector2.SqrMagnitude(b - a);
        public static float SqrDistance(this Vector3 a, Vector3 b)          => Vector3.SqrMagnitude(b - a);
        public static float SqrDistance(this Vector4 a, Vector4 b)          => Vector4.SqrMagnitude(b - a);


        public static Vector2 CalcBezier(this Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            var u = 1f - t;
            return (p0 * u * u)
                 + (p1 * u * t * 2f)
                 + (p2 * t * t);
        }
        public static Vector2 CalcBezier(this Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            var u = 1f - t;
            var u2 = u * u;
            var t2 = t * t;
            return (p0 * u * u2)
                 + (p1 * u2 * t * 3f)
                 + (p2 * u * t2 * 3f)
                 + (p3 * t * t2);
        }

        public static Vector3 CalcBezier(this Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            var u = 1f - t;
            return (p0 * u * u)
                 + (p1 * u * t * 2f)
                 + (p2 * t * t);
        }
        public static Vector3 CalcBezier(this Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            var u = 1f - t;
            var u2 = u * u;
            var t2 = t * t;
            return (p0 * u * u2)
                 + (p1 * u2 * t * 3f)
                 + (p2 * u * t2 * 3f)
                 + (p3 * t * t2);
        }



        public static Vector2 Clamp(this in Vector2 value, in Vector2 min, in Vector2 max)
        {
            return new Vector2(value.x < min.x ? min.x : value.x > max.x ? max.x : value.x,
                               value.y < min.y ? min.y : value.y > max.y ? max.y : value.y);
        }
        public static Vector3 Clamp(this in Vector3 value, in Vector3 min, in Vector3 max)
        {
            return new Vector3(value.x < min.x ? min.x : value.x > max.x ? max.x : value.x,
                               value.y < min.y ? min.y : value.y > max.y ? max.y : value.y,
                               value.z < min.z ? min.z : value.z > max.z ? max.z : value.z);
        }
        public static Vector4 Clamp(this in Vector4 value, in Vector4 min, in Vector4 max)
        {
            return new Vector4(value.x < min.x ? min.x : value.x > max.x ? max.x : value.x,
                               value.y < min.y ? min.y : value.y > max.y ? max.y : value.y,
                               value.z < min.z ? min.z : value.z > max.z ? max.z : value.z,
                               value.w < min.w ? min.w : value.w > max.w ? max.w : value.w);
        }

        public static Vector2 Clamp01(this in Vector2 value)
        {
            return new Vector2(value.x < 0 ? 0 : value.x > 1 ? 1 : value.x,
                               value.y < 0 ? 0 : value.y > 1 ? 1 : value.y);
        }
        public static Vector3 Clamp01(this in Vector3 value)
        {
            return new Vector3(value.x < 0 ? 0 : value.x > 1 ? 1 : value.x,
                               value.y < 0 ? 0 : value.y > 1 ? 1 : value.y,
                               value.z < 0 ? 0 : value.z > 1 ? 1 : value.z);
        }
        public static Vector4 Clamp01(this in Vector4 value)
        {
            return new Vector4(value.x < 0 ? 0 : value.x > 1 ? 1 : value.x,
                               value.y < 0 ? 0 : value.y > 1 ? 1 : value.y,
                               value.z < 0 ? 0 : value.z > 1 ? 1 : value.z,
                               value.w < 0 ? 0 : value.w > 1 ? 1 : value.w);
        }


        public static Vector2 Lerp(this in Vector2 a, in Vector2 b, float t) => a + ((b - a) * Mathf.Clamp01(t));
        public static Vector3 Lerp(this in Vector3 a, in Vector3 b, float t) => a + ((b - a) * Mathf.Clamp01(t));
        public static Vector4 Lerp(this in Vector4 a, in Vector4 b, float t) => a + ((b - a) * Mathf.Clamp01(t));
    }
}