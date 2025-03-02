using UnityEngine;
using UnityEngine.UI;

namespace Supercent.UI
{
    public class CurrencyParticle : MonoBehaviour
    {
        [SerializeField] RectTransform  _rectTransform  = null;
        [SerializeField] Image          _icon           = null;
        [SerializeField] CanvasGroup    _group          = null;

        bool _isUsed = false;



        public Vector2 AnchoredPosition
        {
            set
            {
                if (null == _rectTransform)
                    return;

                _rectTransform.anchoredPosition = value;
            }
            get
            {
                if (null == _rectTransform)
                    return Vector2.zero;

                return _rectTransform.anchoredPosition;
            }
        }

        public Vector2 SizeDelta
        {
            set
            {
                if (null == _rectTransform)
                    return;

                _rectTransform.sizeDelta = value;
            }
            get
            {
                if (null == _rectTransform)
                    return Vector2.zero;

                return _rectTransform.sizeDelta;
            }
        }

        public Vector3 LocalScale
        {
            set
            {
                if (null == _rectTransform)
                    return;

                _rectTransform.localScale = value;
            }
            get
            {
                if (null == _rectTransform)
                    return Vector3.zero;

                return _rectTransform.localScale;
            }
        }

        public Sprite IconSprite
        {
            set
            {
                if (null == _icon)
                    return;

                _icon.sprite = value;
            }
            get
            {
                if (null == _icon)
                    return null;

                return _icon.sprite;
            }
        }

        public float Alpha
        {
            set
            {
                if (null == _group)
                    return;

                _group.alpha = value;
            }
            get
            {
                if (null == _group)
                    return 0.0f;

                return Mathf.Clamp01(_group.alpha);
            }
        }

        public bool IsUsed => _isUsed;



        public void Init()
        {
            AnchoredPosition    = new Vector2(100_000.0f, 0.0f);
            LocalScale          = Vector3.one;
            Alpha               = 0.0f;
            _isUsed             = false;
        }

        public void Use()
        {
            _isUsed = true;
        }
    }
}
