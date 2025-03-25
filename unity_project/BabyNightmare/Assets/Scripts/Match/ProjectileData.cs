using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.StaticData
{
    [CreateAssetMenu(fileName = "ProjectileData", menuName = "BabyNightmare/ProjectileData")]
    public class ProjectileData : ScriptableObject
    {
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material[] _materials;
        [SerializeField] private float _throwDuration;
        [SerializeField] private AnimationCurve _throwCurve;
        [SerializeField] private float _bezierHeight;
        [SerializeField] private float _targetAngleZ;

        public Mesh Mesh => _mesh;
        public Material[] Materials => _materials;
        public float Duration => _throwDuration;
        public AnimationCurve Curve => _throwCurve;
        public float BezierHeight => _bezierHeight;
        public float TargetAngleZ => _targetAngleZ;
    }
}
