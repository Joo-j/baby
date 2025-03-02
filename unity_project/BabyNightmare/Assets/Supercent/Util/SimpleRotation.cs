using UnityEngine;

namespace Supercent.Util
{
    public class SimpleRotation : MonoBehaviour
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private bool _x = false;
        [SerializeField] private bool _y = false;
        [SerializeField] private bool _z = false;

        [Space]
        [SerializeField] private float _speedX;
        [SerializeField] private float _speedY;
        [SerializeField] private float _speedZ;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private bool _enableRot = false;
        private Vector3 _rot = Vector3.zero;


        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public bool EnableX
        {
            get => _x;
            set => _x = value;
        }

        public bool EnableY
        {
            get => _y;
            set => _y = value;
        }

        public bool EnableZ
        {
            get => _z;
            set => _z = value;
        }

        public float SpeedX
        {
            get => _speedX;
            set => _speedX = value;
        }

        public float SpeedY
        {
            get => _speedY;
            set => _speedY = value;
        }

        public float SpeedZ
        {
            get => _speedZ;
            set => _speedZ = value;
        }


        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void Start() 
        {
            _rot = transform.localEulerAngles;    

            _enableRot = true;
            if ((!_x && !_y & !_z) || (0f == _speedX && 0f == _speedY && 0 == _speedZ))
                _enableRot = false;
        }

        private void Update() 
        {
            if (!_enableRot)
                return;

            if (_x)
                _rot.x += _speedX * Time.deltaTime;

            if (_y)
                _rot.y += _speedY * Time.deltaTime;

            if (_z)
                _rot.z += _speedZ * Time.deltaTime;

            transform.localRotation = Quaternion.Euler(_rot);
        }
    }
}