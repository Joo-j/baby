using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Match
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Renderer _render;

        private const string PATH_PROJECTILE_MESH = "Match/Projectile/ProjectileData_";
        private int _id = -1;
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
        }

        public void Release()
        {
            _meshFilter = null;
            _render = null;
        }
    }
}
