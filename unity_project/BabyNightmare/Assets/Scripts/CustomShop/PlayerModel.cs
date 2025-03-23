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
        [SerializeField] private SkinnedMeshRenderer _clothesMeshRenderer;
        [SerializeField] private SkinnedMeshRenderer _shoesMeshRenderer;

        public void RefreshCustomItem(CustomItemData itemData)
        {
            switch (itemData.Type)
            {
                case ECustomItemType.Bag:
                    _bagMeshFilter.mesh = itemData.Mesh;
                    _bagMeshRenderer.material = itemData.Material;
                    break;

                case ECustomItemType.Clothes:
                    _clothesMeshRenderer.sharedMesh = itemData.Mesh;
                    _clothesMeshRenderer.material = itemData.Material;
                    break;

                case ECustomItemType.Shoes:
                    _shoesMeshRenderer.sharedMesh = itemData.Mesh;
                    _shoesMeshRenderer.material = itemData.Material;
                    break;
            }
        }

        public void PlayAni(EAniType aniType)
        {
            switch (aniType)
            {
                case EAniType.Move: _animator.Play("Move"); break;
                case EAniType.Idle: _animator.Play("Idle"); break;
                case EAniType.Attack: _animator.Play("Attack"); break;
            }
        }
    }
}