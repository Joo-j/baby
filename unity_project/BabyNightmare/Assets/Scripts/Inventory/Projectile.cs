using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BabyNightmare.StaticData;

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

        private FX _levelFX = null;

        public void Init(ProjectileData data, int level)
        {
            _meshFilter.mesh = data.Mesh;
            _render.materials = data.Materials;

            Duration = data.Duration;
            Curve = data.Curve;
            BezierHeight = data.BezierHeight;
            TargetAngle = new Vector3(0, 0, data.TargetAngleZ);

            if (level >= 2)
            {
                var levelFXType = level == 2 ? EFXType.Projectile_Level_2 : EFXType.Projectile_Level_3;

                _levelFX = FXPool.Instance.Get(levelFXType);
                _levelFX.transform.SetParent(transform);
                _levelFX.transform.localPosition = Vector3.zero;
                _levelFX.ChangeShapeMesh(data.Mesh);
            }
        }

        void OnDisable()
        {
            if (null != _levelFX)
            {
                FXPool.Instance.Return(_levelFX);
                _levelFX = null;
            }
        }
    }
}
