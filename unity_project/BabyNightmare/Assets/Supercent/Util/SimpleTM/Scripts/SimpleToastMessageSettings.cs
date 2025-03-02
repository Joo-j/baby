using TMPro;
using UnityEngine;

namespace Supercent.Util.STM
{
    public class SimpleToastMessageSettings : ScriptableObject
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private Sprite         _bgSprite     = null;
        [SerializeField] private Color          _bgColor      = new Color(0f, 0f, 0f, 0.5f);
        [SerializeField] private Vector2        _posOffset    = Vector3.zero;
        [SerializeField] private TMP_FontAsset  _fontAsset    = null;
        [SerializeField] private Material       _fontMaterial = null;
        [SerializeField] private int            _sortingOrder = 20;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public Sprite        BgSprite       => _bgSprite;
        public Color         BgColor        => _bgColor;
        public Vector2       PositionOffset => _posOffset;
        public TMP_FontAsset FontAsset      => _fontAsset;
        public Material      FontMaterial   => _fontMaterial;
        public int           SortingOrder   => _sortingOrder;


#if UNITY_EDITOR
        //------------------------------------------------------------------------------
        // create asset file
        //------------------------------------------------------------------------------
        [UnityEditor.MenuItem("Supercent/Util/토스트 메시지 셋팅 파일 생성하기")]
        private static void _EDT_CreateAsset()
        {
            var folder = "Assets/Resources/Supercent";
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            var filePath  = $"Resources/Supercent/SimpleToastMessageSettings.asset";
            var assetPath = $"Assets/{filePath}";

            SimpleToastMessageSettings asset = null;

            if (!System.IO.File.Exists($"{Application.dataPath}/{filePath}"))
            {
                asset = ScriptableObject.CreateInstance<SimpleToastMessageSettings>();

                UnityEditor.AssetDatabase.CreateAsset(asset, assetPath);
                UnityEditor.AssetDatabase.SaveAssets();
            }
            else
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath<SimpleToastMessageSettings>(assetPath);

            UnityEditor.EditorUtility.FocusProjectWindow();
            UnityEditor.Selection.activeObject = asset;
        }
#endif
    }
}