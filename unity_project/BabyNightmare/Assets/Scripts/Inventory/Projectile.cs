using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Match
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Renderer _render;

        public float Duration { get; private set; }
        public AnimationCurve Curve { get; private set; }
        public float BezierHeight { get; private set; }
        public Vector3 TargetAngle { get; private set; }
        public Transform TF => transform;

        public void Init(ProjectileData data)
        {
            _meshFilter.mesh = data.Mesh;
            _render.materials = data.Materials;

            Duration = data.Duration;
            Curve = data.Curve;
            BezierHeight = data.BezierHeight;
            TargetAngle = new Vector3(0, 0, data.TargetAngleZ);
        }
    }
}
