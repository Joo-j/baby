using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BabyNightmare.StaticData;

namespace BabyNightmare.Character
{
    public class PlayerModel : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private MeshFilter _bagMeshFilter;
        [SerializeField] private MeshRenderer _bagMeshRenderer;

        public void RefreshCustomItem(CustomItemData itemData)
        {
            switch (itemData.Type)
            {
                case ECustomType.Bag:
                    _bagMeshFilter.mesh = itemData.Mesh;
                    _bagMeshRenderer.material = itemData.Material;
                    break;
            }
        }
    }
}