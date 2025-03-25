using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Util
{
    public class RotationLocker : MonoBehaviour
    {
        [SerializeField] private Vector3 _eulerAngle;

        public void Init(Vector3 eulerAngle)
        {
            _eulerAngle = eulerAngle;
        }

        void LateUpdate()
        {
            transform.rotation = Quaternion.LookRotation(_eulerAngle);
        }
    }
}