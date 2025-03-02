using UnityEngine;

namespace Supercent.UI
{
    public class UISimpleScalerWithInterval : MonoBehaviour
    {
        private enum EState
        {
            Scaling,
            Interval,
        }

        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private AnimationCurve _scaleCurve;
        [SerializeField] private float _scalingDuration = 1f;
        [SerializeField] private float _intervalMin = 0f;
        [SerializeField] private float _intervalMax = 0f;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private float _curScale      = 1f;
        private float _scaleTimer    = 0f;
        private float _interval      = 0f;
        private float _intervalTimer = 0f;

        private Vector3 _orgScale = Vector3.one;

        private EState _state = EState.Scaling;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void Start() 
        {
            _scaleTimer    = 0f;
            _interval      = 0f < _intervalMin && 0f < _intervalMax ? UnityEngine.Random.Range(_intervalMin, _intervalMax) : 0f;
            _intervalTimer = 0f;
            _orgScale      = transform.localScale;
        }

        private void Update()
        {
            switch (_state)
            {
            case EState.Scaling:  Update_Scaling();  break;
            case EState.Interval: Update_Interval(); break;
            }
        }

        private void Update_Scaling()
        {
            _curScale = _scaleCurve.Evaluate(_scaleTimer / _scalingDuration);
            transform.localScale = _orgScale * _curScale;

            _scaleTimer += Time.deltaTime;
            if (_scalingDuration <= _scaleTimer)
            {
                _scaleTimer -= _scalingDuration;

                if (0f < _interval)
                    _state = EState.Interval;
            }
        }

        private void Update_Interval()
        {
            _intervalTimer += Time.deltaTime;
            if (_interval <= _intervalTimer)
            {
                _interval = UnityEngine.Random.Range(_intervalMin, _intervalMax);
                _state    = EState.Scaling;
            }
        }
    }
}