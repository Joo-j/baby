using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BabyNightmare.StaticData;
using BabyNightmare.Util;

namespace BabyNightmare.InventorySystem
{
    public class Equipment : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Image _image;
        [SerializeField] private Image _coolImage;
        [SerializeField] private AnimationCurve _moveCurve;

        private Coroutine _coMove = null;
        private Coroutine _coShake = null;
        private Coroutine _coCoolDown = null;

        public EquipmentData Data { get; private set; }
        public Action Reset { get; set; }
        public Vector2Int Index { get; set; }
        public Vector2 AnchoredPos
        {
            get => _rtf.anchoredPosition;
            set => _rtf.anchoredPosition = value;
        }

        public void Refresh(EquipmentData data, bool showFX)
        {
            this.Data = data;

            var rect = data.Sprite.rect;
            _image.sprite = data.Sprite;
            _image.SetNativeSize();
            _image.raycastTarget = false;
            _image.type = Image.Type.Simple;
            transform.localScale = Vector3.one;

            if (true == showFX)
            {

            }
        }

        public void Move(Vector2 targetPos, Action callback)
        {
            var startPos = _rtf.anchoredPosition;
            StartCoroutine(SimpleLerp.Co_LerpAnchoredPosition(_rtf, startPos, targetPos, _moveCurve, 0.1f, callback));
        }

        public void StartShake()
        {
            var startRot = Quaternion.Euler(0, 0, -30);
            var targetRot = Quaternion.Euler(0, 0, 30);
            _coShake = StartCoroutine(SimpleLerp.Co_BounceRotation_Loop(transform, startRot, targetRot, 0.2f, _moveCurve));
        }

        public void StopShake()
        {
            transform.rotation = Quaternion.identity;

            if (null != _coShake)
                StopCoroutine(_coShake);

            _coShake = null;
        }

        public void StartCoolDown(float coolTime, Action onCoolDown)
        {
            _coCoolDown = StartCoroutine(Co_CoolDown(coolTime, onCoolDown));
            IEnumerator Co_CoolDown(float coolTime, Action onCoolDown)
            {
                while (true)
                {
                    var elapsed = 0f;
                    while (elapsed < coolTime)
                    {
                        yield return null;
                        elapsed += Time.deltaTime;
                        _coolImage.fillAmount = elapsed / coolTime;
                    }

                    onCoolDown?.Invoke();
                }
            }
        }

        public void StopCoolDown()
        {
            _coolImage.fillAmount = 0;

            if (null != _coCoolDown)
                StopCoroutine(_coCoolDown);

            _coCoolDown = null;
        }
    }
}