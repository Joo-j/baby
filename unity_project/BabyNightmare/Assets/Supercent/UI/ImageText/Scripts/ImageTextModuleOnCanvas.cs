using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.UI
{
    public class ImageTextModuleOnCanvas : MonoBehaviour, IPoolObject
    {
        [SerializeField]
        private Image _imageText = null;

        [SerializeField]
        private CharacterType _characterType = CharacterType.None;

        [SerializeReference]
        private Transform _initParent = null;

        [SerializeField]
        private RectTransform _rtf = null;

        [SerializeField]
        private PunctuationControlModule _punctuationControlModule = null;

        private Vector2 _pivot = Vector2.zero;

        public IPoolBase OwnPool { get; set ; }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        public void Init()
        {
            _rtf = GetComponent<RectTransform>();

            _punctuationControlModule.Init();

            if(_characterType.Equals(CharacterType.Period) || _characterType.Equals(CharacterType.Comma))
            {
                _punctuationControlModule.SetPivot(Vector2.one * 0.5f);
            }
            else
            {
                _punctuationControlModule.SetPivot(_pivot);
            }
        }

        public void SetSpriteToImage(Sprite sprite)
        {
            _imageText.sprite = sprite;

            this.transform.name = _imageText.sprite.name;
        }

        public void SetParent(Transform parent)
        {
            this.transform.SetParent(parent);

            this.transform.localScale = Vector3.one;
        }

        public void SetImageSize(Vector2 size)
        {
            _rtf.sizeDelta = size;

            if(null != _punctuationControlModule)
            {
                _punctuationControlModule.SetSize(size);
            }
        }

        public void SetCharacterType(CharacterType characterType)
        {
            _characterType = characterType;
        }

        public void SetPivot(Vector2 pivot)
        {
            _pivot = pivot;
        }

        public void SetColor(Color color)
        {
            _imageText.color = color;
        }

        public void ReturnToPool()
        {
            this.OwnPool.Return(this);
        }

        public void OnGenerate()
        {         
            _initParent = this.transform.parent;
        }

        public void OnGet()
        {
            this.gameObject.SetActive(true);
        }

        public void OnReturn()
        {
            if(null != this)
            {
                this.transform.SetParent(_initParent);
            }
        }

        public void OnTerminate()
        {
            _imageText = null;
            _initParent = null;

            OwnPool = null;

            _punctuationControlModule?.Release();
            _punctuationControlModule = null;

            _rtf = null;
        }
    }
}
