using System;
using System.Text;

namespace BabyNightmare.Util
{
    public static class ExtensionMethod
    {
        public static string EncodeToBase64(this string txt)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(txt));
        }

        public static string DecodeFromBase64(this string base64)
        {
            return Encoding.Unicode.GetString(Convert.FromBase64String(base64));
        }
    }
}