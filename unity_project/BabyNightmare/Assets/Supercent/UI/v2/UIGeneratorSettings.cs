using System.Collections.Generic;
using UnityEngine;

namespace Supercent.UIv2
{
    [CreateAssetMenu(fileName = "UIGeneratorSettings", menuName = "Supercent/Settings/UIv2_UIGeneratorSettings")]
    public class UIGeneratorSettings : ScriptableObject
    {
        //------------------------------------------------------------------------------
        // singleton
        //------------------------------------------------------------------------------
        private UIGeneratorSettings() {}
        private static UIGeneratorSettings _instance = null;
        public  static UIGeneratorSettings Instance
        {
            get 
            {
                if (null != _instance)
                    return _instance;

                _instance = Resources.Load<UIGeneratorSettings>("UIv2/UIGeneratorSettings");
                if (null == _instance)
                    Debug.LogError("[SkinSettings] .");

                return _instance;
            }
        }

        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private List<TokenData> _tokenDataList = null;
        [SerializeField] private List<string> _ignoreTokenList = null;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public List<TokenData> TokenDataList => _tokenDataList;
        public List<string> IgnoreTokenList => _ignoreTokenList;
    }
}
