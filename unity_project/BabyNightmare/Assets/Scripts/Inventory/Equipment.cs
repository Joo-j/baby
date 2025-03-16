using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using UnityEngine.EventSystems;
using Supercent.Util;

namespace BabyNightmare.InventorySystem
{
    public class Equipment : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Transform _fxTF;
        [SerializeField] private Image _iconOutline;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _coolImage;
        [SerializeField] private AnimationCurve _moveCurve;
        [SerializeField] private AnimationCurve _swingCurve;

        private const string PATH_EQUIPMENT_ICON_OUTLINE = "Inventory/Equipment_Icon_Outline/";
        private const string PATH_EQUIPMENT_ICON = "Inventory/Equipment_Icon/";
        private const string PATH_EQUIPMENT_FX = "Inventory/FX/FX_";
        private Coroutine _coCoolDown = null;
        private Coroutine _coSwing = null;
        private ParticleSystem _fx = null;

        public EquipmentData Data { get; private set; }
        public Vector2Int Index { get; set; }
        public RectTransform RTF => _rtf;

        public void Refresh(EquipmentData data, bool isMerge)
        {
            this.Data = data;

            var iconOutlinePath = $"{PATH_EQUIPMENT_ICON_OUTLINE}{data.Name}_Outline";
            var icon = Resources.Load<Sprite>(iconOutlinePath);

            _rtf.sizeDelta = icon.rect.size;

            _iconOutline.sprite = icon;
            Debug.Assert(null != _iconOutline.sprite, $"{iconOutlinePath} | no outline icon");

            var iconPath = $"{PATH_EQUIPMENT_ICON}{data.Name}";
            _icon.sprite = Resources.Load<Sprite>(iconPath);
            Debug.Assert(null != _iconOutline.sprite, $"{iconPath} | no icon");


            transform.localScale = Vector3.one;

            if (null != _fx)
            {
                Destroy(_fx.gameObject);
                _fx = null;
            }

            var fxPath = $"{PATH_EQUIPMENT_FX}{data.Name}_LV{data.Level}";
            var fxRes = Resources.Load<ParticleSystem>(fxPath);
            if (null != fxRes)
            {
                _fx = Instantiate(fxRes, _fxTF);
                _fx.transform.localPosition = Vector3.zero;
            }

            if (true == isMerge)
            {
                var mergeFxRes = Resources.Load<ParticleSystem>($"{PATH_EQUIPMENT_FX}Merge");
                var mergeFX = Instantiate(mergeFxRes, transform);
                mergeFX.transform.localPosition = Vector3.zero;

                StartCoroutine(SimpleLerp.Co_Invoke(2, () => Destroy(mergeFX.gameObject)));
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