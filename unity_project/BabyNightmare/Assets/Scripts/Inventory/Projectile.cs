using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Match
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Renderer _render;

        private const string PATH_PROJECTILE_MESH = "StaticData/ProjectileData/ProjectileData_";
        private int _id = -1;
        public float Duration { get; private set; }
        public AnimationCurve Curve { get; private set; }
        public float BezierHeight { get; private set; }
        public Vector3 TargetAngle { get; private set; }
        public Transform TF => transform;

        public void Init(int id)
        {
            if (_id == id)
                return;

            _id = id;

            var path = $"{PATH_PROJECTILE_MESH}{id}";
            var data = Resources.Load<ProjectileData>(path);

            _meshFilter.mesh = data.Mesh;
            _render.materials = data.Materials;

            Duration = data.Duration;
            Curve = data.Curve;
            BezierHeight = data.BezierHeight;
            TargetAngle = new Vector3(0, 0, data.TargetAngleZ);
        }
    }
}
