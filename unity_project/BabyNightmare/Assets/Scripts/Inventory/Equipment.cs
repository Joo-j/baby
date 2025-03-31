using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using BabyNightmare.Match;
using BabyNightmare.Talent;
using TMPro;

namespace BabyNightmare.InventorySystem
{
    public class Equipment : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Transform _fxTF;
        [SerializeField] private Image _iconOutline;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _coolImage;
        [SerializeField] private TextMeshProUGUI _levelTMP;
        [SerializeField] private AnimationCurve _moveCurve;
        [SerializeField] private AnimationCurve _swingCurve;

        private const string PATH_EQUIPMENT_ICON_OUTLINE = "Inventory/Equipment_Icon_Outline/";
        private const string PATH_EQUIPMENT_ICON = "Inventory/Equipment_Icon/";
        private readonly Vector2 MERGE_FX_SCALE = Vector3.one * 80f;
        private readonly Vector2 LEVEL_FX_SCALE_1X1 = Vector3.one * 55f;
        private readonly Vector2 LEVEL_FX_SCALE_2X2 = Vector3.one * 70f;
        private Action<EquipmentData> _useEquipment = null;
        private Coroutine _coCoolDown = null;
        private Coroutine _coSwing = null;
        private FX _levelFX = null;
        private FX _mergeFX = null;

        public EquipmentData Data { get; private set; }
        public Vector2Int Index { get; set; }
        public RectTransform RTF => _rtf;

        public void Init(Action<EquipmentData> useEquipment)
        {
            _useEquipment = useEquipment;
        }

        private void OnDestroy()
        {
            if (null != _levelFX)
            {
                FXPool.Instance.Return(_levelFX);
                _levelFX = null;
            }

            if (null != _mergeFX)
            {
                FXPool.Instance.Return(_mergeFX);
                _mergeFX = null;
            }
        }

        public void Refresh(EquipmentData data, bool isMerge)
        {
            this.Data = data;

            var iconOutlinePath = $"{PATH_EQUIPMENT_ICON_OUTLINE}{data.Type}_Outline";
            var icon_outline = Resources.Load<Sprite>(iconOutlinePath);
            Debug.Assert(null != icon_outline, $"{iconOutlinePath} | no outline icon");

            var iconPath = $"{PATH_EQUIPMENT_ICON}{data.Type}";
            var icon = Resources.Load<Sprite>(iconPath);
            Debug.Assert(null != icon, $"{iconPath} | no icon");

            _rtf.sizeDelta = icon.rect.size;

            _iconOutline.sprite = icon_outline;
            _icon.sprite = Resources.Load<Sprite>(iconPath);
            _levelTMP.text = $"LV.{Data.Level}";

            if (null != _levelFX)
            {
                FXPool.Instance.Return(_levelFX);
                _levelFX = null;
            }

            if (null != _mergeFX)
            {
                FXPool.Instance.Return(_mergeFX);
                _mergeFX = null;
            }

            if (data.Level >= 2)
            {
                var levelFXType = data.Level == 2 ? EFXType.Equipment_Level_2 : EFXType.Equipment_Level_3;

                _levelFX = FXPool.Instance.Get(levelFXType);
                _levelFX.transform.SetParent(_fxTF);
                _levelFX.transform.localPosition = Vector3.zero;
                _levelFX.ChangeShapeMesh(Data.Mesh);

                if (data.Shape.Column >= 2 && data.Shape.Row >= 2)
                    _levelFX.transform.localScale = LEVEL_FX_SCALE_2X2;
                else
                    _levelFX.transform.localScale = LEVEL_FX_SCALE_1X1;
            }

            if (true == isMerge)
            {
                _mergeFX = FXPool.Instance.Get(EFXType.Equipment_Merge);
                _mergeFX.transform.SetParent(transform);
                _mergeFX.transform.localPosition = Vector3.zero;
                _mergeFX.transform.localScale = MERGE_FX_SCALE;

                StartCoroutine(SimpleLerp.Co_Invoke(2, () => FXPool.Instance.Return(_mergeFX)));
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

        public void StartCoolDown()
        {
            if (null != _coCoolDown)
                return;

            var coolTime = Data.CoolTime;
            var speed = TalentManager.Instance.GetValue(ETalentType.Attack_Speed_Percentage);
            coolTime -= coolTime * speed;

            _coCoolDown = StartCoroutine(Co_CoolDown());
            IEnumerator Co_CoolDown()
            {
                var rand = UnityEngine.Random.Range(0f, 0.2f);
                yield return CoroutineUtil.WaitForSeconds(rand);

                while (true)
                {
                    var elapsed = 0f;
                    while (elapsed < coolTime)
                    {
                        yield return null;
                        elapsed += Time.deltaTime;
                        _coolImage.fillAmount = elapsed / coolTime;
                    }

                    StartCoroutine(SimpleLerp.Co_BounceScale(transform, Vector3.one * 1.2f, _swingCurve, 0.15f));
                    _useEquipment?.Invoke(Data);
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