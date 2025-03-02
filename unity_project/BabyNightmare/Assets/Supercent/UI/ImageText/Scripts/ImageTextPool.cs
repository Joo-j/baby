using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supercent.UI
{
    public class ImageTextPool : BaseObjectPool<ImageTextModuleOnCanvas, CharacterType>
    {
        [SerializeField]
        private Sprite _targetSprite = null;

        [SerializeField]
        private Vector2 _pivot = Vector2.zero;

        public override void Init()
        {
            base.Init();
        }

        public override void OnInit()
        {
            var tmp = base._pool.GetInactives();

            while(tmp.MoveNext())
            {
                var current = tmp.Current;

                if(null == current)
                {
                    Debug.LogError($"ImageTextPool, OnInit(), current is null -> {transform.name}");

                    continue;
                }

                current.SetCharacterType(_enumType);
                current.SetPivot(_pivot);

                current.Init();
                current.SetSpriteToImage(_targetSprite);
            }
        }

        public void SetCharacterType()
        {

        }

        public void SetSprite(Sprite sprite)
        {
            _targetSprite = sprite;
        }

        public void SetPivot(Vector2 pivot)
        {
            _pivot = pivot;
        }

        public Sprite GetSprite()
        {
            return _targetSprite;
        }
    }
}

