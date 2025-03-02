using UnityEngine;

namespace Supercent.UI
{
    public class UISimpleRotation : MonoBehaviour
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private float _rotationSpeed;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private float _rot = 0f;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public float RotationSpeed
        {
            get { return _rotationSpeed; }
            set { _rotationSpeed = value; }
        }

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void Update() 
        {
            _rot += Time.deltaTime * _rotationSpeed;
            if (_rot < 0f)
                _rot += 360f;
            if (360f < _rot)
                _rot -= 360f;

            transform.localRotation = Quaternion.Euler(0f, 0f, _rot);
        }
    }
}