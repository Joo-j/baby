using UnityEngine;

namespace Supercent.Util
{
    public static class EnumUtil
    {
        /// <summary>
        /// 
        /// </summary>
        public static T GetRandomValue<T>() where T : System.Enum
        {
            var array = System.Enum.GetValues(typeof(T));
            if (null == array || 0 == array.Length)
            {
                Debug.LogError($"[EnumUtil.GetRandomValue] 해당 enum 은 값이 하나도 없습니다. enum: {typeof(T).Name}");
                return default(T);
            }

            return (T)array.GetValue(UnityEngine.Random.Range(0, array.Length));
        }

        /// <summary>
        /// 
        /// </summary>
        public static T GetRandomValueWithIgnoreValue<T>(T ignoreValue) where T : System.Enum
        {
            for (int i = 0; i < 20; ++i)
            {
                var t = GetRandomValue<T>();
                if (!System.Enum.Equals(t, ignoreValue))
                    return t;
            }

            Debug.LogError($"[EnumUtil.GetRandomValue] 해당 enum 은 값이 하나도 없습니다. enum: {typeof(T).Name}");
            return default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        public static T GetRandomValueWithIgnoreValues<T>(params T[] ignoreValues) where T : System.Enum
        {
            for (int i = 0; i < 20; ++i)
            {
                var t = GetRandomValue<T>();
                var b = true;

                for (int ignoreIndex = 0, ignoreSize = ignoreValues.Length; ignoreIndex < ignoreSize; ++ignoreIndex)
                {
                    if (System.Enum.Equals(ignoreValues[ignoreIndex], t))
                    {
                        b = false;
                        break;
                    }
                }

                if (b)
                    return t;
            }

            Debug.LogError($"[EnumUtil.GetRandomValue] 해당 enum 은 값이 하나도 없습니다. enum: {typeof(T).Name}");
            return default(T);
        }
    }
}