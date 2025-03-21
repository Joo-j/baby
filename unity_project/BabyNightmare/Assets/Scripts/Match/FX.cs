using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using UnityEngine;

namespace BabyNightmare.Match
{

    public class FX : BehaviourBase
    {
        [SerializeField] private ParticleSystem[] _psArr;
        [SerializeField] private EFXType _type;
        [SerializeField] private float _appearDuration;

        public EFXType Type => _type;
        public float AppearDuration => _appearDuration;

        public void ChangeStartColor(Color color)
        {
            for (var i = 0; i < _psArr.Length; i++)
            {
                var main = _psArr[i].main;
                main.startColor = color;
            }
        }

        public void ChangeShapeMesh(Mesh mesh)
        {
            for (var i = 0; i < _psArr.Length; i++)
            {
                var shape = _psArr[i].shape;
                shape.mesh = mesh;
            }
        }

#if UNITY_EDITOR

        protected override void OnBindSerializedField()
        {
            base.OnBindSerializedField();

            _psArr = GetComponentsInChildren<ParticleSystem>();
        }

#endif
    }
}
