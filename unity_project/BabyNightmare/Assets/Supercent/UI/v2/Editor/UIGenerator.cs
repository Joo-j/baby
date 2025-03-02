using System;
using System.Collections.Generic;
using UnityEngine;

namespace Supercent.UIv2.EDT
{
    public static partial class UIGenerator
    {
        private class TokenInfo
        {
            public Transform Self          = null;
            public TokenData TokenData     = null;
            public string    HierarchyPath = string.Empty;
            public string    VariablePath  = string.Empty;
        }

        private class ListTokenInfo : TokenInfo
        {
            public string ListName = string.Empty;
        }

        private class ClassTokenInfo : TokenInfo
        {
            public string ClassName = string.Empty;
            public string ClassFullName = string.Empty;

            public Dictionary<string, TokenInfo>            BasicInfoSet     = new Dictionary<string, TokenInfo>();
            public Dictionary<string, List<ListTokenInfo>>  ListInfoSet      = new Dictionary<string, List<ListTokenInfo>>();
            public Dictionary<string, ClassTokenInfo>       ClassInfoSet     = new Dictionary<string, ClassTokenInfo>();
            public Dictionary<string, List<ClassTokenInfo>> ListClassInfoSet = new Dictionary<string, List<ClassTokenInfo>>();
        }

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private static string _codeNamespace = string.Empty;
        private static string _outputFolder  = string.Empty;
        private static bool   _useStop       = false; 


        private static ClassTokenInfo  _selfInfo        = null;
        private static List<TokenData> _tokenDataList   = null;
        private static List<string>    _ignoreTokenList = null;
        private static HashSet<string> _usingSet        = null;

        private static string _baseClassCodes = string.Empty;
        private static string _userClassCodes = string.Empty;

        private const string TOKEN_UIv2_USING = "Supercent.UIv2";

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public static bool Generate(GameObject targetGo, string codeNamespace, string outputFolder, bool useStop)
        {
            if (null == targetGo)
                return false;

            _codeNamespace = codeNamespace;
            _outputFolder  = outputFolder;
            _useStop       = useStop;

            // Hierarchy 분석
            _tokenDataList   = UIGeneratorSettings.Instance.TokenDataList;
            _ignoreTokenList = UIGeneratorSettings.Instance.IgnoreTokenList;

            _usingSet = new HashSet<string>() { TOKEN_UIv2_USING };            

            _selfInfo = new ClassTokenInfo() 
            { 
                Self      = targetGo.transform,
                ClassName = targetGo.name,
            };

            AnalyzeHierarchy(_selfInfo, targetGo.transform, string.Empty);

            // 코드 생성
            MakeCode();

            // 파일 생성
            SaveFile(outputFolder, _selfInfo.ClassName + "_Base.cs", ref _baseClassCodes);

            var fileName = _selfInfo.ClassName + ".cs";
            if (!System.IO.File.Exists(outputFolder + fileName))
                SaveFile(outputFolder, fileName, ref _userClassCodes);

            // 정리
            _tokenDataList  = null;
            _baseClassCodes = string.Empty;
            _userClassCodes = string.Empty;

            return true;
        }

        private static void SaveFile(string folder, string filename, ref string codes)
        {
            if (System.IO.File.Exists(folder + filename))
                System.IO.File.Delete(folder + filename);

            System.IO.File.WriteAllText(folder + filename, codes);
        }
    }
}