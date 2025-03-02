using System.Numerics;
using System.Text;
using UnityEngine;

namespace Supercent.Util
{
    public static class BigIntegerUtil
    {
        const char  ALPHABET_START_CHAR = 'A';
        const int   ALPHABET_COUNT      = 26; // A (65) ~ Z (90), Count 26



        // NOTE: 변환된 값 보다 작은 자릿수의 값들은 버림 처리 됩니다.
        // ex) 1111     => ToUnitString => 1.11A   => ParseFromUnitString => 1110
        // ex) 543231   => ToUnitString => 543A    => ParseFromUnitString => 543000
        public static string ToUnitString(this BigInteger self)
        {
            var str = self.ToString();
            var len = str.Length;
            if (len < 4)
                return str;

            var unitFactor  = (len - 1) / 3;
            var numberCount = len - (unitFactor * 3);
            var finalUnit   = Calc_Unit(unitFactor);
            switch (numberCount)
            {
                case 1: return $"{str.Substring(0, 1)}.{str.Substring(1, 2)}{finalUnit}";
                case 2: return $"{str.Substring(0, 2)}.{str.Substring(2, 1)}{finalUnit}";
                case 3: return $"{str.Substring(0, 3)}{finalUnit}";
            }

            Debug.LogWarning($"[ToDigitString] Invalid Value\n\nLength: {len}\nDigit Count: {numberCount}\nValue: {str}\n");
            return string.Empty;



            // 1A   => 1        => 1
            // 1Z   => 1 0      => 26
            // 1AA  => 1 1      => 27
            // 1AZ  => 2 0      => 52
            // 1ZZ  => 1 1 0    => 702
            // 1AAA => 1 1 1    => 703
            string Calc_Unit(int unitFactor)
            {
                var unitStr = string.Empty;
                while (unitFactor > 0) 
                {
                    --unitFactor;

                    var alphabetIndex   = unitFactor % ALPHABET_COUNT;
                    unitStr             = (char)(ALPHABET_START_CHAR + alphabetIndex) + unitStr;

                    unitFactor /= ALPHABET_COUNT;
                }

                return unitStr;
            }
        }



        public static BigInteger ParseFromUnitString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return BigInteger.Zero;

            var sb              = new StringBuilder();
            var unitIndex       = -1;
            var decimalCount    = 0;
            var checkDecimal    = false;
            for (int n = 0, cnt = str.Length; n < cnt; ++n)
            {
                var pickChar = str[n];
                if ('A' <= pickChar && pickChar <= 'Z')
                {
                    if (n == 0)
                    {
                        Debug.LogWarning($"[ParseFromDigitString] Is Not Unit String!\n\nValue: {str}");
                        return BigInteger.Zero;
                    }

                    unitIndex = n;
                    break;
                }

                if ('.' == pickChar)
                {
                    checkDecimal = true;
                    continue;
                }

                if (pickChar < '0' || '9' < pickChar)
                {
                    Debug.LogWarning($"[ParseFromDigitString] Is Not Unit String!\n\nValue: {str}");
                    return BigInteger.Zero;
                }

                if (checkDecimal)
                    ++decimalCount;

                sb.Append(pickChar);
                if (sb.Length < 4)
                    continue;

                Debug.LogWarning($"[ParseFromDigitString] Is Not Unit String!\n\nValue: {str}");
                return BigInteger.Zero;
            }

            var finalValue = BigInteger.Zero;
            if (unitIndex < 0)
            {
                if (!BigInteger.TryParse(sb.ToString(), out finalValue))
                    Debug.LogWarning($"[ParseFromDigitString] Failed to Parse!\n\nValue: {str}");

                return finalValue;
            }

            var unitFactor  = 0;
            var pow         = 0;
            var unit        = str.Substring(unitIndex, str.Length - unitIndex);
            for (int n = unit.Length - 1; 0 <= n; --n)
            {
                var alphabetIndex   = unit[n] - ALPHABET_START_CHAR;
                unitFactor         += (alphabetIndex + 1) * MathUtil.PowInt(ALPHABET_COUNT, pow);

                ++pow;
            }

            if (0 < unitFactor)
            {
                switch (decimalCount)
                {
                    case 0: sb.Append("000");   break;
                    case 1: sb.Append("00");    break;
                    case 2: sb.Append("0");     break;
                }

                for (int n = 1, cnt = unitFactor; n < cnt; ++n)
                    sb.Append("000");
            }

            if (!BigInteger.TryParse(sb.ToString(), out finalValue))
                Debug.LogWarning($"[ParseFromDigitString] Failed to Parse!\n\nValue: {str}");

            return finalValue;
        }
    }
}
