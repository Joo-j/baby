using System;
using System.Collections;
using System.Collections.Generic;
using BabyNightmare.Util;
using Supercent.Util;
using UnityEngine;

namespace BabyNightmare.Match
{
    public class Coin : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _appearDuration = 3;
        [SerializeField] private AnimationCurve _scaleCurve;
        [SerializeField] private float _hideDuration = 2f;

        public void Init(Vector3 dir, float amount, Action returnPool)
        {
            transform.localScale = Vector3.one;
            _rigidbody.AddForce(dir * amount);
            _rigidbody.AddTorque(dir * amount);

            StartCoroutine(Co_Hide(returnPool));
        }

        private IEnumerator Co_Hide(Action doneCallback)
        {
            yield return CoroutineUtil.WaitForSeconds(_appearDuration);
            
            yield return SimpleLerp.Co_LerpScale(transform, Vector3.one, Vector3.zero, _scaleCurve, _hideDuration);

            doneCallback?.Invoke();
        }
    }
}