using UnityEngine;

namespace Supercent.Util
{
    public class SimpleLoadingSettings : ScriptableObject
    {
        public enum EPartsOfScreenIconDirectingStyle
        {
            Rotation,
            HeartBeat,
        }

        //------------------------------------------------------------------------------
        // Full of screen
        //------------------------------------------------------------------------------
        [System.Serializable]
        public class FullOfScreen
        {
            public Color  DimdColor = new Color(0f, 0f, 0f, 1f);
            public Sprite Thumbnail = null;

            public float  Duration_show = 0.25f;
            public float  Duration_hide = 0.25f;
        }
        
        //------------------------------------------------------------------------------
        // Parts Of screen
        //------------------------------------------------------------------------------
        [System.Serializable]
        public class PartsOfScreen
        {
            public Color  DimdColor   = new Color(0f, 0f, 0f, 0f);
            public Sprite LoadingIcon = null;

            public EPartsOfScreenIconDirectingStyle DirectingStyle = EPartsOfScreenIconDirectingStyle.Rotation;

            public float DirectingSpeed = 1f;
        }

        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private FullOfScreen  _fullOfScreen;
        [SerializeField] private PartsOfScreen _partsOfScreen;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public FullOfScreen  FullOfScreenInfo  => _fullOfScreen;
        public PartsOfScreen PartsOfScreenInfo => _partsOfScreen;
    }
}