using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Supercent.Util
{
    public static class StringExtensions
    {
        public static string Md5Sum(this string strToEncrypt)
        {
            var ue = new UTF8Encoding();
            var bytes = ue.GetBytes(strToEncrypt);

            // encrypt bytes
            var md5 = new MD5CryptoServiceProvider();
            var hashBytes = md5.ComputeHash(bytes);

            // Convert the encrypted bytes back to a string (base 16)
            var hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
                hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');

            return hashString.PadLeft(32, '0');
        }

        public static string ToSha256(this string strOrg)
        {
            using (var sha = new SHA256Managed())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(strOrg));
                return Convert.ToBase64String(bytes);
                // return BitConverter.ToString(bytes).Replace("-", "");
            }
        }


#if !NO_JSON
        public static bool DecodeTo<T>(this SimpleJSON.JSONClass jsonNode, ref T o)
        {
            if (jsonNode == null)
            {
                Debug.Log("Not JsonClass!!!");
                return false;
            }


            foreach (FieldInfo prop in o.GetType().GetFields())
            {
                if(prop.IsStatic)
                    continue;
                switch (Type.GetTypeCode(prop.FieldType))
                {
                case TypeCode.String:
                    prop.SetValue(o, jsonNode[prop.Name].Value);
                    break;
                case TypeCode.Single:
                    prop.SetValue(o, jsonNode[prop.Name].AsFloat);
                    break;
                case TypeCode.Int32:
                    if (prop.FieldType.IsEnum && !string.IsNullOrEmpty(jsonNode[prop.Name].Value))
                    {
                        try
                        {
                            prop.SetValue(o, Enum.Parse(prop.FieldType, jsonNode[prop.Name].Value));
                        }
                        catch
                        {
                            prop.SetValue(o, jsonNode[prop.Name].AsInt);
                        }
                    }
                    else
                    {
                        prop.SetValue(o, jsonNode[prop.Name].AsInt);
                    }
                    break;
                case TypeCode.Int64:
                    prop.SetValue(o, jsonNode[prop.Name].AsLong);
                    break;
                case TypeCode.Boolean:
                    prop.SetValue(o, jsonNode[prop.Name].AsBool);
                    break;
                    /*
                                    case System.TypeCode.Object:
                                        string className = ConvertClassString(prop.Name);
                                        if(className == "UserData"&&jsonNode[prop.Name].Count>0){
                                            ServerData.UserData uData = new ServerData.UserData();
                                            (JSONClass.Parse(jsonNode[prop.Name].ToString()) as JSONClass).DecodeTo<ServerData.UserData>(ref uData);
                                            prop.SetValue(o,uData);
                                        }
                                        break;
                    */
                }
            }
            return true;
        }

        public static SimpleJSON.JSONClass EncodeToJsonClass(this object o)
        {
            var jsonClass = new SimpleJSON.JSONClass();
            string s;
            foreach (FieldInfo prop in o.GetType().GetFields())
            {
                switch (Type.GetTypeCode(prop.FieldType))
                {
                case TypeCode.String:
                    s = prop.GetValue(o) as string;
                    if (!s.IsNullOrEmpty())
                        jsonClass[prop.Name] = s;
                    break;
                case TypeCode.Double:
                    jsonClass[prop.Name].AsDouble = (double)prop.GetValue(o);
                    break;
                case TypeCode.Single:
                    jsonClass[prop.Name].AsFloat = (float)prop.GetValue(o);
                    break;
                case TypeCode.Int32:
                    jsonClass[prop.Name].AsInt = (int)prop.GetValue(o);
                    break;
                case TypeCode.Boolean:
                    jsonClass[prop.Name].AsBool = (bool)prop.GetValue(o);
                    break;
                    /*
                                    case System.TypeCode.Object:
                                        string className = ConvertClassString(prop.Name);
                                        if(className == "UserData"){
                                            if(ConvertClassString(o.GetType().Name) == "Friend" && (o as ServerData.Friend).user_data != null)
                                            {
                                                SimpleJSON.JSONClass child = (o as ServerData.Friend).user_data.EncodeToJsonClass();
                                                jsonClass[prop.Name] = child;
                                            }
                                        }
                                        break;
                    */
                }
            }
            return jsonClass;
        }
#endif// NO_JSON

        public static string ConvertClassString(string inValue)
        {
            var result = string.Empty;
            for (int i = 0; i < inValue.Length; i++)
            {
                var value = inValue[i];
                if (i == 0)
                {
                    result += value.ToString().ToUpper();
                    continue;
                }
                if (value == '_')
                {
                    continue;
                }
                else if (inValue[i - 1] == '_')
                {
                    result += value.ToString().ToUpper();
                }
                else
                {
                    result += value;
                }
            }
            return result;
        }

        public static string Short(this string o, int length = 7)
        {
            return o != null && o.Length > length
                 ? o.Substring(0, length) + "..."
                 : o;
        }

        public static string Obj2String(this object o)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}\n", o.ToString());
            foreach (FieldInfo prop in o.GetType().GetFields())
            {
                sb.AppendFormat("{0}:{1}\n", prop.Name, prop.GetValue(o));
            }

            return sb.ToString();
        }


        public static string EncodeToBase64(this string txt)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(txt));
        }
        public static string DecodeFromBase64(this string base64)
        {
            return Encoding.Unicode.GetString(Convert.FromBase64String(base64));
        }

        public static string Unicode(this string o)
        {
            var defaultBytes = Encoding.Default.GetBytes(o);
            var unicodeBytes = Encoding.Convert(Encoding.Default, Encoding.Unicode, defaultBytes);

            return Encoding.Unicode.GetString(unicodeBytes);
        }

        public static string UTF8(this string str)
        {
            var sb = new StringBuilder();

            var charArray = str.ToCharArray();
            foreach (var c in charArray)
            {
                int codepoint = System.Convert.ToInt32(c);
                if ((codepoint >= 32) && (codepoint <= 126))
                    sb.Append(c);
                else
                {
                    sb.Append("\\u");
                    sb.Append(codepoint.ToString("x4"));
                }
            }
            return sb.ToString();
        }

        public static string Join(this List<string> list, string delemiter = ",")
        {
            var sb = new StringBuilder();
            sb.Append(list[0].ToString());
            for (int i = 1; i < list.Count; i++)
            {
                sb.Append(delemiter);
                sb.Append(list[i].ToString());
            }
            return sb.ToString();
        }

        public static string Join(this string[] list, string delemiter = ",")
        {
            return list == null
                 ? string.Empty
                 : string.Join(delemiter, list);
        }

        public static string ReplaceChar(this string value, Func<char, char?> callback)
        {
            if (callback == null) return value;
            if (string.IsNullOrEmpty(value)) return value;

            var sb = new StringBuilder(value);
            for (int index = sb.Length - 1; -1 < index; --index)
            {
                var origin = sb[index];
                var replace = callback(origin);
                if (!replace.HasValue)
                    sb.Remove(index, 1);
                else if (origin != replace.Value)
                    sb[index] = replace.Value;
            }
            return sb.ToString();
        }

        public const string DefaultFormatForDateTime = "yyyy/MM/dd HH:mm:ss";

        public static DateTime ToDateTime(this string s, 
                                          string format = DefaultFormatForDateTime, 
                                          IFormatProvider provider = null, 
                                          System.Globalization.DateTimeStyles style = System.Globalization.DateTimeStyles.None)
        {
            if (null == provider)
                provider = new System.Globalization.CultureInfo("en-US");

            if (true == DateTime.TryParseExact(s, format, provider, style, out var date))
                return date;

            return DateTime.MinValue;
        }

        public static string DateTimeToString(this DateTime t)
        {
            return t.ToString(DefaultFormatForDateTime, new System.Globalization.CultureInfo("en-US"));
        }
    }
}