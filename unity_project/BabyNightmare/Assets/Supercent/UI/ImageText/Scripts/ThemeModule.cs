using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supercent.UI
{
    public class ThemeModule : MonoBehaviour
    {
        [SerializeField]
        private ImageTextTheme _theme = ImageTextTheme.None;
        public ImageTextTheme Theme => _theme;


        // Start is called before the first frame update
        void Start()
        {
            
        }

        public void SetTheme(ImageTextTheme theme)
        {
            _theme = theme;
        }
    }
}
