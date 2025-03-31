using System.Collections;
using System.Collections.Generic;
using BabyNightmare.Match;
using BabyNightmare.Talent;
using BabyNightmare.Util;
using Supercent.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare
{
    public class TalentItemView : MonoBehaviour
    {
        [SerializeField] private Image _bg;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _titleTMP;
        [SerializeField] private TextMeshProUGUI _levelTMP;
        [SerializeField] private Sprite _lockIcon;
        [SerializeField] private Sprite _normalBG;
        [SerializeField] private Sprite _focusBG;

        private const string PATH_ICON = "Talent/Icon/ICN_";
        private TalentData _data = null;
        private Sprite _iconSprite = null;
        private string _titleText = null;
        private int _level = 0;
        private FX _fx = null;

        public void Init(TalentData data)
        {
            _data = data;
            _iconSprite = Resources.Load<Sprite>($"{PATH_ICON}{data.TalentType}");
            _titleText = $"{_data.TalentType}".Replace("Amount", "").Replace("Percentage", "").Replace("_", " ");

            RefreshLevel(0, false);
            Focus(false);
        }

        public void RefreshLevel(int level, bool showFX)
        {
            if (level == 0)
            {
                _icon.sprite = _lockIcon;
                _icon.SetNativeSize();
                _titleTMP.text = "";
                _levelTMP.text = "";
                return;
            }

            _icon.sprite = _iconSprite;
            _icon.SetNativeSize();
            _titleTMP.text = _titleText;
            _levelTMP.text = $"LV.{level}";

            if (null != _fx && null != _fx.gameObject)
            {
                FXPool.Instance.Return(_fx);
                _fx = null;
            }

            if (level > _level && true == showFX)
            {
                StartCoroutine(SimpleLerp.Co_BounceScale(transform, Vector3.one * 1.2f, CurveHelper.Preset.Linear, 0.1f));
                _fx = FXPool.Instance.Get(EFXType.Pop);
                _fx.transform.SetParent(transform);
                _fx.transform.localPosition = Vector3.zero;
                _fx.transform.localScale = Vector3.one * 150f;

                this.Invoke(CoroutineUtil.WaitForSeconds(2.5f), () => FXPool.Instance.Return(_fx));
            }

            _level = level;
        }

        public void Focus(bool on)
        {
            _bg.sprite = on ? _focusBG : _normalBG;
            if (on)
                StartCoroutine(SimpleLerp.Co_BounceScale(transform, Vector3.one * 1.1f, CurveHelper.Preset.Linear, 0.1f));
        }
    }
}