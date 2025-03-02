using UnityEngine;
using UnityEngine.UI;

namespace Supercent.UI.Util
{
    public class FlowingRawImage : MonoBehaviour
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private RawImage _rawImage;

        [Space]
        [SerializeField] private float _flowingSpeed_X = -0.1f;
        [SerializeField] private float _flowingSpeed_Y = 0.1f;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private Rect _uvRect;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void Start()
        {
            if (null == _rawImage)
                return;
                
            _uvRect = _rawImage.uvRect;
        }

        private void Update() 
        {
            if (null == _rawImage)
                return;

            _uvRect.x += _flowingSpeed_X * Time.deltaTime;
            _uvRect.y += _flowingSpeed_Y * Time.deltaTime;

            _rawImage.uvRect = _uvRect;
        }
    }
}