using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "BabyNightmare/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material[] _materials;

    public Mesh Mesh => _mesh;
    public Material[] Materials => _materials;
}
