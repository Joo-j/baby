#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using Supercent.Util;


namespace Supercent.UI.Editor
{
    [CustomEditor(typeof(UIMono), true)]
    public class UIMonoEditor : UnityEditor.Editor
    {
        const string TAP = "    ";
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        const string AUTO_BEGIN = "//[Auto generate code begin]";
        const string AUTO_END = "//[Auto generate code end]";

        [MenuItem("Supercent/UI/Creat UIMono Script")]
        static void CreatScript()
        {
            string reference = File.ReadAllText($"{Application.dataPath}/Supercent/UI/Template/ui_mono.txt");
            string fileName = Selection.activeTransform.name.Replace("_", string.Empty);
            string result = "";

            string[] imgList = Selection.activeTransform.GetChildNames("IMG_",true);
            string[] btnList = Selection.activeTransform.GetChildNames("BTN_",true);
            string[] txtList = Selection.activeTransform.GetChildNames("TXT_",true);
            string[] tmpList = Selection.activeTransform.GetChildNames("TMP_",true);
            string[] rtfList = Selection.activeTransform.GetChildNames("RT_",true);
            string[] ifList = Selection.activeTransform.GetChildNames("IF_",true);
            string[] goList = Selection.activeTransform.GetChildNames("GO_",true);

            string imgS = "\n";
            string btnS = "\n";
            string txtS = "\n";
            string tmpS = "\n";
            string rtfS = "\n";
            string ifS = "\n";
            string goS = "\n";

            for (int i = 0; i < imgList.Length; i++)
            {
                imgS += ($"{TAP}{TAP}" + imgList[i] + ",\n");
            }
            imgS += $"{TAP}{TAP}Max\n";

            for (int i = 0; i < btnList.Length; i++)
            {
                btnS += ($"{TAP}{TAP}" + btnList[i] + ",\n");
            }
            btnS += $"{TAP}{TAP}Max\n";

            for (int i = 0; i < txtList.Length; i++)
            {
                txtS += ($"{TAP}{TAP}" + txtList[i] + ",\n");
            }
            txtS += $"{TAP}{TAP}Max\n";

            for (int i = 0; i < tmpList.Length; i++)
            {
                tmpS += ($"{TAP}{TAP}" + tmpList[i] + ",\n");
            }
            tmpS += $"{TAP}{TAP}Max\n";

            for (int i = 0; i < rtfList.Length; i++)
            {
                rtfS += ($"{TAP}{TAP}" + rtfList[i] + ",\n");
            }
            rtfS += $"{TAP}{TAP}Max\n";

            for (int i = 0; i < ifList.Length; i++)
            {
                ifS += ($"{TAP}{TAP}" + ifList[i] + ",\n");
            }
            ifS += $"{TAP}{TAP}Max\n";

            for (int i = 0; i < goList.Length; i++)
            {
                goS += ($"{TAP}{TAP}" + goList[i] + ",\n");
            }
            goS += $"{TAP}{TAP}Max\n";

            if (reference.LastIndexOf("[ClassName]") != -1) { reference = reference.Replace("[ClassName]", fileName); }
            if (reference.LastIndexOf("[IMG_Names]") != -1) { reference = reference.Replace("[IMG_Names]", imgS); }
            if (reference.LastIndexOf("[BTN_Names]") != -1) { reference = reference.Replace("[BTN_Names]", btnS); }
            if (reference.LastIndexOf("[TXT_Names]") != -1) { reference = reference.Replace("[TXT_Names]", txtS); }
            if (reference.LastIndexOf("[TMP_Names]") != -1) { reference = reference.Replace("[TMP_Names]", tmpS); }
            if (reference.LastIndexOf("[RT_Names]") != -1) { reference = reference.Replace("[RT_Names]", rtfS); }
            if (reference.LastIndexOf("[IF_Names]") != -1) { reference = reference.Replace("[IF_Names]", ifS); }
            if (reference.LastIndexOf("[GO_Names]") != -1) { reference = reference.Replace("[GO_Names]", goS); }

            string path = EditorUtility.SaveFilePanel("Save Script", Application.dataPath, fileName, "cs");
            if (File.Exists(path))
            {
                string origin = File.ReadAllText(path);
                int s = origin.LastIndexOf(AUTO_BEGIN);
                int e = origin.LastIndexOf(AUTO_END);
                string f = origin.Substring(0, s);
                string b = origin.Substring(e, origin.Length - e);

                s = reference.LastIndexOf(AUTO_BEGIN);
                e = reference.LastIndexOf(AUTO_END);

                result = f + reference.Substring(s, e - s) + b;
            }
            else
            {
                result = reference;
            }
            var buttonName="";
            for (int i = 0; i < btnList.Length; i++)
            {
                buttonName += $"            case BTN.{btnList[i]}:\n                break;\n";
            }
            result = result.Replace("***BUTTON_EVENTS***",buttonName);
            if(path!=null && path.Length>0)
                File.WriteAllText(path, result);

            AssetDatabase.Refresh();
            var window = EditorWindow.GetWindow(typeof(UIMonoEditorWindow)) as UIMonoEditorWindow;            
            window.FileName = fileName;
            window.FilePath = path;
            window.Show();  
        }
    }
}
#endif