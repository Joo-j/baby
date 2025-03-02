using System;
using UnityEngine;

namespace Supercent.Util
{
    public static class MathUtil
    {
        public const double PI_HALF = Math.PI / 2;
        public const double TAU = Math.PI * 2;
        public static readonly double PHI = (Math.Sqrt(5) + 1) / 2;
        public static readonly double SQRT2 = Math.Sqrt(2);


        public static bool IsOdd(int num)   => 1 == (num & 1);
        public static bool IsOdd(uint num)  => 1 == (num & 1);
        public static bool IsOdd(long num)  => 1 == (num & 1);
        public static bool IsOdd(ulong num) => 1 == (num & 1);


        public static int Clamp(int value, int min, int max)                => value < min ? min : value > max ? max : value;
        public static long Clamp(long value, long min, long max)            => value < min ? min : value > max ? max : value;
        public static double Clamp(double value, double min, double max)    => value < min ? min : value > max ? max : value;
        public static double Clamp01(double value)                          => value < 0 ? 0 : value > 1 ? 1 : value;


        public static int Lerp(int a, int b, float t)           => (int)(a + ((b - a) * Mathf.Clamp01(t)));
        public static int Lerp(int a, int b, double t)          => (int)(a + ((b - a) * Clamp01(t)));
        public static long Lerp(long a, long b, float t)        => (long)(a + ((b - a) * Mathf.Clamp01(t)));
        public static long Lerp(long a, long b, double t)       => (long)(a + ((b - a) * Clamp01(t)));
        public static double Lerp(double a, double b, double t) => a + ((b - a) * Clamp01(t));


        public static int Repeat(int point, int length, bool isMirror)      => (int)(isMirror ? PingPong(point, length) : Repeat(point, length));
        public static long Repeat(long point, long length, bool isMirror)   => (long)(isMirror ? PingPong(point, length) : Repeat(point, length));
        public static double Repeat(double t, double length)                => Clamp(t - Math.Floor(t / length) * length, 0f, length);
        public static double PingPong(double t, double length)              => length - Math.Abs(Repeat(t, length * 2f) - length);


        #region DivRem
        public static int DivRem(int a, int b, out int result)
        {
            var div = a / b;
            result = a - (div * b);
            return div;
        }
        public static uint DivRem(uint a, uint b, out uint result)
        {
            var div = a / b;
            result = a - (div * b);
            return div;
        }

        public static long DivRem(long a, long b, out long result)
        {
            var div = a / b;
            result = a - (div * b);
            return div;
        }
        public static ulong DivRem(ulong a, ulong b, out ulong result)
        {
            var div = a / b;
            result = a - (div * b);
            return div;
        }

        public static float DivRem(float a, float b, out float result)
        {
            var div = (float)Math.Floor(a / b);
            result = a - (div * b);
            return div;
        }
        public static double DivRem(double a, double b, out double result)
        {
            var div = Math.Floor(a / b);
            result = a - (div * b);
            return div;
        }
        #endregion// DivRem


        [Obsolete("VectorExtensions.SqrDistance�� �̿��ϼ���")]
        public static float SqrDistance(Vector3 a, Vector3 b) => Vector3.SqrMagnitude(b - a);

        [Obsolete("VectorExtensions.CalcBezier �̿��ϼ���")]
        public static Vector3 Calc_Bezier(Vector3 start, Vector3 middle, Vector3 end, float factor)
        {
            return ((1f - factor) * (1f - factor) * start) +
                   (2f * (1 - factor) * factor * middle) +
                   (factor * factor * end);
        }


        public static float Calc_Percent_Between_A_and_B(float a, float b, float factor)
        {
            if (Mathf.Approximately(a, b))
            {
                if (factor < a) return 0.0f;
                else            return 1.0f;
            }

            var deltaBA         = b - a;
            var deltaFactorA    = factor - a;

            return Mathf.Clamp01(deltaFactorA / deltaBA);
        }

        public static float Lerp_Percent_Between_A_and_B(float a, float b, float factor, float aValue, float bValue)
        {
            var t = Calc_Percent_Between_A_and_B(a, b, factor);
            return Mathf.Lerp(aValue, bValue, t);
        }



        public static int PowInt(int value, int pow)
        {
            var finalValue = 1;
            for (int n = 0; n < pow; ++n)
                finalValue *= value;

            return finalValue;
        }
    }
}