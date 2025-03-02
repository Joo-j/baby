using UnityEngine;

namespace Supercent.UI
{
    public class UISimpleScaler : MonoBehaviour
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private AnimationCurve _scaleCurve;
        [SerializeField] private float _minScale;
        [SerializeField] private float _maxScale;
        [SerializeField] private float _scalingDuration = 1f;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private float _curScale = 1f;
        private float _scaleDist = 0f;
        private float _timer = 0f;

        private Vector3 _orgScale = Vector3.one;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public float MinScale
        {
            get { return _minScale; }
            set { _minScale = value; }
        }

        public float MaxScale
        {
            get { return _maxScale; }
            set { _maxScale = value; }
        }

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void Start() 
        {
            _curScale  = _minScale;
            _scaleDist = _maxScale - _minScale;
            _timer     = 0f;
            _orgScale  = transform.localScale;
        }

        private void Update()
        {
            _curScale = _scaleCurve.Evaluate(_timer / _scalingDuration) * _scaleDist + _minScale;
            transform.localScale = _orgScale * _curScale;
            
            _timer += Time.deltaTime;
            if (_scalingDuration < _timer)
                _timer -= _scalingDuration;
        }
    }
}