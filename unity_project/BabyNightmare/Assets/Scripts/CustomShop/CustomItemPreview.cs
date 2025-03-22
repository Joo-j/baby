using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using UnityEngine;
using BabyNightmare.Character;
using BabyNightmare.StaticData;

namespace BabyNightmare.CustomShop
{
    public class CustomItemPreview : MonoBehaviour
    {
        [SerializeField] protected Camera _renderCamera;
        [SerializeField] private PlayerModel _playerModel;

        private RenderTexture _rt = null;

        public RenderTexture RT
        {
            get
            {
                if (null == _rt)
                {
                    _rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
                    _renderCamera.targetTexture = _rt;
                }

                return _rt;
            }
        }

        private void OnEnable()
        {
            _playerModel.PlayMoveAni();
        }

        public void RefreshCustomItem(CustomItemData itemData)
        {
            _playerModel.RefreshCustomItem(itemData);
        }

        public void Release()
        {
            var rt = _renderCamera.targetTexture;
            _renderCamera.targetTexture = null;
            rt.Release();

            Destroy(gameObject);
        }
    }
}