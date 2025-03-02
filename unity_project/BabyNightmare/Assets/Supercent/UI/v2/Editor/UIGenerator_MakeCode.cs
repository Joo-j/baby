using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Supercent.UIv2.EDT
{
    public static partial class UIGenerator
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private const string  TOKEN_USING          = "$USING$";
        private const string  TOKEN_NAMESPACE      = "$NAMESPACE$";
        private const string  TOKEN_BASECLASSNAME  = "$BASECLASSNAME$";
        private const string  TOKEN_USERCLASSNAME  = "$USERCLASSNAME$";
        private const string  TOKEN_COMPONENTS     = "$COMPONENTS$";
        private const string  TOKEN_ASSIGNOBJECTS  = "$ASSIGNOBJECTS$";

        private const string CLASSNAME_GAMEOBJECT = "GameObject";
        private const string CLASSNAME_BUTTON     = "Button";
        private const string CLASSNAME_TOGGLE     = "Toggle";

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private static void MakeCode()
        {
            _baseClassCodes = string.Empty;
            _userClassCodes = string.Empty;

            MakeBaseClassCode();
            MakeUserClassCode();
        }

        private static void MakeBaseClassCode()
        {
            var templateLines = Resources.Load<TextAsset>("UIv2/UIBaseClassTemplate").text.Split('\n');
            var writeCode     = new StringBuilder();

            for (int i = 0, size = templateLines.Length; i < size; ++i)
            {
                var lineStr = templateLines[i] + '\n';

                if (-1 == lineStr.IndexOf("$"))
                {
                    writeCode.Append(lineStr);
                    continue;
                }
                
                lineStr = TryReplaceUsing(lineStr);
                lineStr = TryReplaceNamespace(lineStr);
                lineStr = TryReplaceBaseClassName(lineStr);
                lineStr = TryReplaceComponents(lineStr);
                lineStr = TryReplaceAssignObjects(lineStr);

                writeCode.Append(lineStr);
            }

            _baseClassCodes = writeCode.ToString();
        }

        private static void MakeUserClassCode()
        {
            var templateLines = Resources.Load<TextAsset>("UIv2/UIUserClassTemplate").text.Split('\n');
            var writeCode     = new StringBuilder();

            for (int i = 0, size = templateLines.Length; i < size; ++i)
            {
                var lineStr = templateLines[i] + '\n';

                if (-1 == lineStr.IndexOf("$"))
                {
                    writeCode.Append(lineStr);
                    continue;
                }

                lineStr = TryReplaceNamespace(lineStr);
                lineStr = TryReplaceBaseClassName(lineStr);
                lineStr = TryReplaceUserClassName(lineStr);

                writeCode.Append(lineStr);
            }

            _userClassCodes = writeCode.ToString();
        }

        private static string TryReplaceUsing(string str)
        {
            var index = str.IndexOf(TOKEN_USING);
            if (-1 == index)
                return str;

            AddUsing("UnityEngine");

            var sb = new StringBuilder();
            foreach (var usingStr in _usingSet)
                sb.AppendLine("using " + usingStr + ";");

            return str.Replace(TOKEN_USING, sb.ToString());
        }

        /// <summary>
        /// 네임스페이스 변경 시도
        /// </summary>
        private static string TryReplaceNamespace(string str)
        {
            var index = str.IndexOf(TOKEN_NAMESPACE);
            if (-1 == index)
                return str;

            return str.Replace(TOKEN_NAMESPACE, _codeNamespace);
        }

        /// <summary>
        /// Base 클래스 이름 변경 시도
        /// </summary>
        private static string TryReplaceBaseClassName(string str)
        {
            var index = str.IndexOf(TOKEN_BASECLASSNAME);
            if (-1 == index)
                return str;

            return str.Replace(TOKEN_BASECLASSNAME, _selfInfo.ClassName + "_Base");
        }

        /// <summary>
        /// 유저 클래스 이름 변경 시도
        /// </summary>
        private static string TryReplaceUserClassName(string str)
        {
            var index = str.IndexOf(TOKEN_USERCLASSNAME);
            if (-1 == index)
                return str;

            return str.Replace(TOKEN_USERCLASSNAME, _selfInfo.ClassName);
        }

        /// <summary>
        /// 컴포넌트 코드 변경 시도
        /// </summary>
        private static string TryReplaceComponents(string str)
        {
            var index = str.IndexOf(TOKEN_COMPONENTS);
            if (-1 == index)
                return str;

            var tab = string.Empty;
            for (int i = 0; i < index; ++i)
                tab += " ";

            var sb = new StringBuilder();

            // list class
            MakeComponentsCode_ListClass(tab, false, _selfInfo.ListClassInfoSet, ref sb);

            // class
            MakeComponentsCode_Class(tab, false, _selfInfo.ClassInfoSet, ref sb);

            // basic
            MakeComponentsCode_Basic(tab, false, _selfInfo.BasicInfoSet, ref sb);

            // list
            MakeComponentsCode_List(tab, false, _selfInfo.ListInfoSet, ref sb);
            
            return sb.ToString();
        }

        /// <summary>
        /// 오브젝트 연결 코드 변경 시도
        /// </summary>
        private static string TryReplaceAssignObjects(string str)
        {
            var index = str.IndexOf(TOKEN_ASSIGNOBJECTS);
            if (-1 == index)
                return str;

            var tab = string.Empty;
            for (int i = 0; i < index; ++i)
                tab += " ";

            var sb = new StringBuilder();

            // list class
            MakeAssignCode_ListClass(tab, _selfInfo.ListClassInfoSet, ref sb);

            // class
            MakeAssignCode_Class(tab, _selfInfo.ClassInfoSet, ref sb);

            // basic
            MakeAssignCode_Basic(tab, _selfInfo.BasicInfoSet, ref sb);

            // list
            MakeAssignCode_List(tab, _selfInfo.ListInfoSet, ref sb);

            return sb.ToString();
        }

        /// <summary>
        /// 클래스 리스트 컴포넌트 목록의 코드 생성
        /// </summary>
        private static void MakeComponentsCode_ListClass(string tab, bool usePublic, Dictionary<string, List<ClassTokenInfo>> infoListSet, ref StringBuilder sb)
        {
            foreach (var info in infoListSet.Values)
            {
                if (null == info 
                    || 0 == info.Count
                    || null == info[0])
                    continue;

                MakeComponentsCode_Class_Single(tab, usePublic, true, info[0], ref sb);
            }
        }

        /// <summary>
        /// 클래스 컴포넌트 목록의 코드 생성
        /// </summary>
        private static void MakeComponentsCode_Class(string tab, bool usePublic, Dictionary<string, ClassTokenInfo> infoSet, ref StringBuilder sb)
        {
            foreach (var info in infoSet.Values)
                MakeComponentsCode_Class_Single(tab, usePublic, false, info, ref sb);
        }

        /// <summary>
        /// 클래스 컴포넌트 목록의 코드 생성 - 실 부분
        /// </summary>
        private static void MakeComponentsCode_Class_Single(string tab, bool usePublic, bool isList, ClassTokenInfo info, ref StringBuilder sb)
        {
            var tabPP = tab + "    ";

            if (null == info)
                return;

            // class name
            sb.AppendLine($"{tab}[System.Serializable] public class {info.ClassName}");
            sb.AppendLine(tab + "{");

            {
                // self
                sb.AppendLine($"{tabPP}public GameObject GO_Self;");
                sb.AppendLine($"{tabPP}public RectTransform RTF_Self;");
                sb.AppendLine();

                // list class components
                if (0 < info.ListClassInfoSet.Count)
                    MakeComponentsCode_ListClass(tabPP, true, info.ListClassInfoSet, ref sb);

                // class components
                if (0 < info.ClassInfoSet.Count)
                    MakeComponentsCode_Class(tabPP, true, info.ClassInfoSet, ref sb);

                // basic components
                if (0 < info.BasicInfoSet.Count)
                    MakeComponentsCode_Basic(tabPP, true, info.BasicInfoSet, ref sb);

                // list components
                if (0 < info.ListInfoSet.Count)
                    MakeComponentsCode_List(tabPP, true, info.ListInfoSet, ref sb);
            }

            sb.AppendLine(tab + "}");

            // component
            if (isList)
            {
                if (usePublic)
                    sb.AppendLine($"{tab}public List<{info.ClassName}> LIST_CLS_{info.ClassName};");
                else
                    sb.AppendLine($"{tab}[SerializeField] protected List<{info.ClassName}> LIST_CLS_{info.ClassName};");
            }
            else
            {
                if (usePublic)
                    sb.AppendLine($"{tab}public {info.ClassName} CLS_{info.ClassName};");
                else
                    sb.AppendLine($"{tab}[SerializeField] protected {info.ClassName} CLS_{info.ClassName};");
            }

            sb.AppendLine();
        }

        /// <summary>
        /// 일반 컴포넌트 목록의 코드 생성
        /// </summary>
        private static void MakeComponentsCode_Basic(string tab, bool usePublic, Dictionary<string, TokenInfo> infoSet, ref StringBuilder sb)
        {
            // 컴포넌트 별 그룹핑
            var infos = new List<TokenInfo>();

            foreach (var info in infoSet.Values)
                infos.Add(info);

            infos.Sort((l, r) => l.TokenData.ClassName.CompareTo(r.TokenData.ClassName));

            // 코드 생성
            var preClassName = string.Empty;
            for (int i = 0, size = infos.Count; i < size; ++i)
            {
                if (!string.IsNullOrEmpty(preClassName) && preClassName != infos[i].TokenData.ClassName)
                    sb.AppendLine();
                preClassName = infos[i].TokenData.ClassName;
                
                if (usePublic)
                    sb.AppendLine($"{tab}public {infos[i].TokenData.ClassName} {infos[i].Self.name};");
                else
                    sb.AppendLine($"{tab}[SerializeField] protected {infos[i].TokenData.ClassName} {infos[i].Self.name};");
            }
        }

        /// <summary>
        /// 리스트형 컴포넌트 목록의 코드 생성
        /// </summary>
        private static void MakeComponentsCode_List(string tab, bool usePublic, Dictionary<string, List<ListTokenInfo>> infoSet, ref StringBuilder sb)
        {
            // 상단에 컴포넌트 코드가 있으면 한줄 띄움
            if (0 < sb.Length)
                sb.AppendLine();

            // 코드 생성
            foreach (var infoList in infoSet.Values)
            {
                if (null == infoList || 0 == infoList.Count)
                    continue;

                if (usePublic)
                    sb.AppendLine($"{tab}public List<{infoList[0].TokenData.ClassName}> {infoList[0].ListName};");
                else
                    sb.AppendLine($"{tab}[SerializeField] protected List<{infoList[0].TokenData.ClassName}> {infoList[0].ListName};");
            }
        }

        /// <summary>
        /// 리스트 클래스 오브젝트 연결 코드
        /// </summary>
        private static void MakeAssignCode_ListClass(string tab, Dictionary<string, List<ClassTokenInfo>> infoListSet, ref StringBuilder sb)
        {
            foreach (var list in infoListSet.Values)
            {
                if (null == list || 0 == list.Count)
                    continue;

                // new list
                var listName = list[0].VariablePath;
                var index    = listName.IndexOf("[");

                listName = listName.Substring(0, index);
                sb.AppendLine($"{tab}{listName} = new List<{list[0].ClassFullName}>();");
                sb.AppendLine($"{tab}for (int i = 0, size = {list.Count}; i < size; ++i)");
                sb.AppendLine($"{tab}    {listName}.Add(null);");

                for (int i = 0, size = list.Count; i < size; ++i)
                    MakeAssignCode_Class_Single(tab, list[i], ref sb);
            }
        }

        /// <summary>
        /// 클래스 오브젝트 연결 코드
        /// </summary>
        private static void MakeAssignCode_Class(string tab, Dictionary<string, ClassTokenInfo> infoSet, ref StringBuilder sb)
        {
            foreach (var info in infoSet.Values)
                MakeAssignCode_Class_Single(tab, info, ref sb);
        }

        /// <summary>
        /// 클래스 오브젝트 연결 코드 - 실제코드
        /// </summary>
        private static void MakeAssignCode_Class_Single(string tab, ClassTokenInfo info, ref StringBuilder sb)
        {
            if (null == info)
                return;

            // new class
            sb.AppendLine($"{tab}{info.VariablePath} = new {info.ClassFullName}()");
            sb.AppendLine($"{tab}{{");
            sb.AppendLine($"{tab}    GO_Self = UIComponentUtil.FindChild(transform, \"{info.HierarchyPath}\").gameObject,");
            sb.AppendLine($"{tab}    RTF_Self = UIComponentUtil.FindComponent<RectTransform>(transform, \"{info.HierarchyPath}\"),");
            sb.AppendLine($"{tab}}};");

            // list class
            if (0 < info.ListClassInfoSet.Count)
                MakeAssignCode_ListClass(tab, info.ListClassInfoSet, ref sb);

            // class
            if (0 < info.ClassInfoSet.Count)
                MakeAssignCode_Class(tab, info.ClassInfoSet, ref sb);

            // basic
            if (0 < info.BasicInfoSet.Count)
                MakeAssignCode_Basic(tab, info.BasicInfoSet, ref sb);

            // list
            if (0 < info.ListInfoSet.Count)
                MakeAssignCode_List(tab, info.ListInfoSet, ref sb);
        }

        /// <summary>
        /// 일반 오브젝트 연결 코드
        /// </summary>
        private static void MakeAssignCode_Basic(string tab, Dictionary<string, TokenInfo> infoSet, ref StringBuilder sb)
        {
            foreach (var info in infoSet.Values)
                MakeAssignCode_Object(tab, info, ref sb);
        }

        /// <summary>
        /// 리스트 오브젝트 연결 코드
        /// </summary>
        private static void MakeAssignCode_List(string tab, Dictionary<string, List<ListTokenInfo>> infoListSet, ref StringBuilder sb)
        {
            foreach (var list in infoListSet.Values)
            {
                if (null == list || 0 == list.Count)
                    continue;

                var listName = list[0].VariablePath;
                var index    = listName.IndexOf("[");

                listName = listName.Substring(0, index);
                sb.AppendLine($"{tab}{listName} = new List<{list[0].TokenData.ClassName}>();");
                sb.AppendLine($"{tab}for (int i = 0, size = {list.Count}; i < size; ++i)");
                sb.AppendLine($"{tab}    {listName}.Add(null);");

                for (int i = 0, size = list.Count; i < size; ++i)
                    MakeAssignCode_Object(tab, list[i], ref sb);
            }
        }

        private static void MakeAssignCode_Object(string tab, TokenInfo info, ref StringBuilder sb)
        {
            if (null == info)
                return;

            var className = info.TokenData.ClassName;

            if (className == CLASSNAME_GAMEOBJECT)
                sb.AppendLine($"{tab}{info.VariablePath} = UIComponentUtil.FindChild(transform, \"{info.HierarchyPath}\").gameObject;");
            else
            {
                sb.AppendLine($"{tab}{info.VariablePath} = UIComponentUtil.FindComponent<{info.TokenData.ClassName}>(transform, \"{info.HierarchyPath}\");");

                if (CLASSNAME_BUTTON == className)
                    sb.AppendLine($"{tab}Supercent.UIv2.AssignHelper.TryAssignButton({info.VariablePath});");
                else if (CLASSNAME_TOGGLE == className)
                    sb.AppendLine($"{tab}Supercent.UIv2.AssignHelper.TryAssignToggle({info.VariablePath});");
            }
        }
    }
}