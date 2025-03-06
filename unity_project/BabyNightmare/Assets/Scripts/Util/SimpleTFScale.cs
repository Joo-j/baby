using System.Collections;
using System.Collections.Generic;
using BabyNightmare.Util;
using UnityEngine;

public class SimpleTFScale : MonoBehaviour
{
    [SerializeField] private Vector3 _targetScale;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _duration = 2f;

    private void OnEnable()
    {
        StartCoroutine(SimpleLerp.Co_BounceScale(transform, _targetScale, _curve, _duration, true));
    }
}
