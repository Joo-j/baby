using System.Collections.Generic;
using UnityEngine;

namespace Supercent.UIv2.EDT
{
    public static partial class UIGenerator
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private const string TOKEN_LIST_CLS = "LIST_CLS_";
        private const string TOKEN_CLS      = "CLS_";
        private const string TOKEN_LIST     = "LIST_";

        private const string USING_LIST = "System.Collections.Generic";

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private static void AnalyzeHierarchy(ClassTokenInfo tokenInfo, Transform parent, string parentHierarchyPath)
        {
            for (int childIndex = 0, childSize = parent.childCount; childIndex < childSize; ++childIndex)
            {
                var child = parent.GetChild(childIndex);
                if (null != child.GetComponent<Supercent.UIv2.UIBase>())
                    continue;

                var name = child.name;

                if (0 == name.IndexOf(TOKEN_LIST_CLS))
                    TryAppendToListClass(tokenInfo, child, parentHierarchyPath);
                else if (0 == name.IndexOf(TOKEN_CLS))
                    TryAppendToClass(tokenInfo, child, parentHierarchyPath);
                else if (0 == name.IndexOf(TOKEN_LIST))
                    TryAppendToList(tokenInfo, child, parentHierarchyPath);
                else
                    TryAppendToBasic(tokenInfo, child, parentHierarchyPath);
            }
        }

        /// <summary>
        /// 클래스 리스트
        /// </summary>
        private static void TryAppendToListClass(ClassTokenInfo parent, Transform child, string parentHierarchyPath)
        {
            var listName  = NameToListIdName(child.name);
            var className = listName.Substring(TOKEN_LIST_CLS.Length, listName.Length - TOKEN_LIST_CLS.Length);
            var fullPath  = MakeHierarchyPath(parentHierarchyPath, child.name);

            if (parent.ClassInfoSet.ContainsKey(className))
            {
                PrintError_AlreadyExistsObjectName(fullPath);
                return;
            }

            if (!parent.ListClassInfoSet.TryGetValue(className, out var listClass))
            {
                listClass = new List<ClassTokenInfo>();
                parent.ListClassInfoSet.Add(className, listClass);
            }

            var info = new ClassTokenInfo()
            {
                Self          = child,
                TokenData     = null,
                HierarchyPath = fullPath,
                VariablePath = MakeVariablePath(parent, listName, listClass.Count),
                ClassName     = className,
                ClassFullName = MakeClassFullName(parent, className),
            };

            listClass.Add(info);
            AddUsing(USING_LIST);

            AnalyzeHierarchy(info, child, fullPath);
        }

        /// <summary>
        /// 클래스
        /// </summary>
        private static void TryAppendToClass(ClassTokenInfo parent, Transform child, string parentHierarchyPath)
        {
            var name      = child.name;
            var className = name.Substring(TOKEN_CLS.Length, name.Length - TOKEN_CLS.Length);
            var fullPath  = MakeHierarchyPath(parentHierarchyPath, name);

            if (parent.ClassInfoSet.ContainsKey(className)
                || parent.ListClassInfoSet.ContainsKey(className))
            {
                PrintError_AlreadyExistsObjectName(fullPath);
                return;
            }

            var info = new ClassTokenInfo()
            {
                Self          = child,
                TokenData     = null,
                HierarchyPath = fullPath,
                VariablePath  = MakeVariablePath(parent, name, -1),
                ClassName     = className,
                ClassFullName = MakeClassFullName(parent, className),
            };

            parent.ClassInfoSet.Add(name, info);

            AnalyzeHierarchy(info, child, info.HierarchyPath);
        }

        /// <summary>
        /// list
        /// </summary>
        private static void TryAppendToList(ClassTokenInfo parentInfo, Transform child, string parentHierarchyPath)
        {
            string    listName  = NameToListIdName(child.name);
            string    tokenName = listName.Substring(TOKEN_LIST.Length, listName.Length - TOKEN_LIST.Length);
            TokenData tokenData = null;

            for (int i = 0, size = _tokenDataList.Count; i < size; ++i)
            {
                if (0 != tokenName.IndexOf(_tokenDataList[i].Key))
                    continue;

                if (CheckIgnoreCase(tokenName))
                    continue;

                tokenData = _tokenDataList[i];
                break;                
            }

            if (null == tokenData)
                return;

            if (!parentInfo.ListInfoSet.TryGetValue(listName, out var list))
            {
                list = new List<ListTokenInfo>();
                parentInfo.ListInfoSet.Add(listName, list);
            }

            var info = new ListTokenInfo()
            {
                Self          = child,
                TokenData     = tokenData,
                HierarchyPath = MakeHierarchyPath(parentHierarchyPath, child.name),
                VariablePath  = MakeVariablePath(parentInfo, listName, list.Count),
                ListName      = listName,
            };

            list.Add(info);

            AddUsing(tokenData);
            AddUsing(USING_LIST);

            AnalyzeHierarchy(parentInfo, child, info.HierarchyPath);
        }

        /// <summary>
        /// 일반
        /// </summary>
        private static void TryAppendToBasic(ClassTokenInfo parentInfo, Transform child, string parentHierarchyPath)
        {
            var name = child.name;

            for (int i = 0, size = _tokenDataList.Count; i < size; ++i)
            {
                var tokenIndex = name.IndexOf(_tokenDataList[i].Key);

                if (0 != tokenIndex)
                    continue;

                if (CheckIgnoreCase(name))
                    continue;

                var hierarchyPath = MakeHierarchyPath(parentHierarchyPath, name);

                if (parentInfo.BasicInfoSet.ContainsKey(name))
                {
                    PrintError_AlreadyExistsObjectName(hierarchyPath);
                    return;
                }

                parentInfo.BasicInfoSet.Add(name, new TokenInfo()
                {
                    Self          = child,
                    TokenData     = _tokenDataList[i],
                    HierarchyPath = hierarchyPath,
                    VariablePath  = MakeVariablePath(parentInfo, name, -1),
                });

                AddUsing(_tokenDataList[i]);
                break;
            }

            AnalyzeHierarchy(parentInfo, child, MakeHierarchyPath(parentHierarchyPath, name));
        }

        /// <summary>
        /// Ignore 케이스 검사
        /// </summary>
        private static bool CheckIgnoreCase(string tokenName)
        {
            for (int i = 0, size = _ignoreTokenList.Count; i < size; ++i)
            {
                if (-1 != tokenName.IndexOf(_ignoreTokenList[i]))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// 하이러키 패스 가져오기
        /// </summary>
        private static string MakeHierarchyPath(string parentHierarchyPath, string childName)
        {
            return string.IsNullOrEmpty(parentHierarchyPath)
                 ? childName
                 : parentHierarchyPath + "/" + childName;
        }

        /// <summary>
        /// 변수 경로 가져오기
        /// </summary>
        private static string MakeVariablePath(ClassTokenInfo parent, string childName, int listIndex)
        {
            var name = string.IsNullOrEmpty(parent.VariablePath)
                     ? childName
                     : parent.VariablePath + "." + childName;

            if (-1 < listIndex)
                name += $"[{listIndex}]";

            return name;
        }

        private static string MakeClassFullName(ClassTokenInfo parent, string childName)
        {
            return string.IsNullOrEmpty(parent.ClassFullName)
                 ? childName
                 : parent.ClassFullName + "." + childName;
        }

        /// <summary>
        /// 에러 로그
        /// </summary>
        private static void PrintError_AlreadyExistsObjectName(string hierarchyFullName)
        {
            Debug.LogWarning($"<color=#ffaa55>[UIv2.UIGenerator]</color> 동일한 이름의 오브젝트가 이미 존재합니다. 이 오브젝트는 무시됩니다.\n  - {hierarchyFullName}");
        }

        /// <summary>
        /// using 에 추가
        /// </summary>
        private static void AddUsing(TokenData tokenData)
        {
            if (_usingSet.Contains(tokenData.Namespace))
                return;

            _usingSet.Add(tokenData.Namespace);
        }

        private static void AddUsing(string s)
        {
            if (_usingSet.Contains(s))
                return;

            _usingSet.Add(s);
        }
        
        /// <summary>
        /// 리스트용 이름 가져오기 (영어, 숫자, 특수문자 중 _ 만 사용가능)
        /// </summary>
        private static string NameToListIdName(string name)
        {
            if (0 != name.IndexOf("LIST_"))
            {
                Debug.LogError($"<color=#ffaa55>[UIv2.UIGenerator]</color> 해당 오브젝트는 list 용 이 아닙니다. name: {name}");
                return string.Empty;
            }

            if (name.Length <= 5)
            {
                Debug.LogError($"<color=#ffaa55>[UIv2.UIGenerator]</color> 리스트로 만들기에 이름이 부족합니다. name: {name}");
                return string.Empty;
            }

            int i = 5;
            for (int size = name.Length; i < size; ++i)
            {
                char c = name[i];
                if (('a' <= c && c <= 'z')
                    || ('A' <= c && c <= 'Z')
                    || ('0' <= c && c <= '9')
                    || c == '_')
                    continue;

                break;
            }

            if (i <= 5)
            {
                Debug.LogError($"<color=#ffaa55>[UIv2.UIGenerator]</color> 리스트로 만들기에 이름이 부족합니다. name: {name}");
                return string.Empty;
            }

            return name.Substring(0, i);
        }
    }
}