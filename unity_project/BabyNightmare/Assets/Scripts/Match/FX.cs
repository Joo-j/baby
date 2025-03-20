using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Match
{

    public class FX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _ps;
        [SerializeField] private EFXType _type;
        [SerializeField] private float _appearDuration;

        public EFXType Type => _type;
        public float AppearDuration => _appearDuration;

        public void ChangeStartColor(Color color)
        {
            var fxMain = _ps.main;
            fxMain.startColor = color;
        }
    }
}
