using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using UnityEngine.EventSystems;

namespace BabyNightmare.InventorySystem
{
    public class Equipment : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Image _image;
        [SerializeField] private Image _coolImage;
        [SerializeField] private AnimationCurve _moveCurve;
        [SerializeField] private AnimationCurve _swingCurve;

        private Coroutine _coCoolDown = null;
        private Coroutine _coSwing = null;

        public EquipmentData Data { get; private set; }
        public Vector2Int Index { get; set; }
        public RectTransform RTF => _rtf;

        public void Refresh(EquipmentData data, bool showFX)
        {
            this.Data = data;

            _image.sprite = data.Sprite;
            _image.SetNativeSize();
            _image.raycastTarget = false;
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

        public void Swing()
        {
            if (null != _coSwing)
                return;

            _coSwing = StartCoroutine(Co_Swing());
        }

        private IEnumerator Co_Swing()
        {
            yield return SimpleLerp.Co_BounceRotation(transform, Quaternion.Euler(0, 0, -25), 0.1f, _swingCurve);
            yield return SimpleLerp.Co_BounceRotation(transform, Quaternion.Euler(0, 0, 25), 0.1f, _swingCurve);

            _coSwing = null;
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