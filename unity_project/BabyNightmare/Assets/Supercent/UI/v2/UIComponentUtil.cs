using UnityEngine;

namespace Supercent.UIv2
{
    public static class UIComponentUtil
    {
        public static Transform FindChild(Transform self, string path)
        {
            if (null == self)
            {
                Debug.LogError("[UIv2.UIComponentUtil.FindChild] self 파라메터가 null 입니다.");
                return null;
            }

            return self.Find(path);
        }

        public static T FindComponent<T>(Component self, string path) where T : Component
        {
            if (null == self)
            {
                Debug.LogError($"[UIv2.UIComponentUtil.FindComponent<{typeof(T).Name}>] 오브젝트를 찾지 못했습니다. path: {path}");
                return null;
            }

            return FindComponent<T>(self.transform, path);
        }

        public static T FindComponent<T>(GameObject self, string path) where T : Component
        {
            if (null == self)
            {
                Debug.LogError($"[UIv2.UIComponentUtil.FindComponent<{typeof(T).Name}>] 오브젝트를 찾지 못했습니다. path: {path}");
                return null;
            }

            return FindComponent<T>(self.transform, path);
        }

        public static T FindComponent<T>(Transform self, string path) where T : Component
        {
            var child = FindChild(self, path);
            if (null == child)
            {
                Debug.LogError($"[UIv2.UIComponentUtil.FindComponent<{typeof(T).Name}>] 오브젝트를 찾지 못했습니다. path: {path}");
                return null;
            }

            return child.GetComponent<T>();
        }
    }
}