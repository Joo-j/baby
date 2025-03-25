using System;
using System.Collections;
using System.Collections.Generic;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using Supercent.Core.Audio;
using Supercent.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare.Talent
{
    public class TalentView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _gridTF;
        [SerializeField] private Button _upgradeBTN;
        [SerializeField] private Sprite _enableButtonSprite;
        [SerializeField] private Sprite _disenableButtonSprite;
        [SerializeField] private TextMeshProUGUI _priceTMP;
        [SerializeField] private AnimationCurve _gachaCurve;
        [SerializeField] private float _delayDuration = 0.5f;
        [SerializeField] private GameObject _guide;

        private const string PATH_TALENT_ITEM_VIEW = "Talent/TalentItemView";

        private Dictionary<ETalentType, TalentItemView> _itemViewDict = null;
        private Action _upgrade = null;

        public void Init(List<TalentData> dataList, Action upgrade)
        {
            _itemViewDict = new Dictionary<ETalentType, TalentItemView>();

            foreach (var data in dataList)
            {
                var itemView = ObjectUtil.LoadAndInstantiate<TalentItemView>(PATH_TALENT_ITEM_VIEW, _gridTF);
                itemView.Init(data);

                _itemViewDict.Add(data.TalentType, itemView);
            }

            _upgrade = upgrade;

            _guide.SetActive(false);
        }

        public void RefreshLevel(Dictionary<ETalentType, int> levelDict, bool showFx)
        {
            foreach (var pair in _itemViewDict)
            {
                var type = pair.Key;
                if (false == levelDict.TryGetValue(type, out var level))
                    continue;

                var itemView = pair.Value;
                itemView.RefreshLevel(level, showFx);
            }
        }

        public void RefreshButton(int gem, int price)
        {
            if (price == 0)
            {
                _priceTMP.text = $"FREE";
            }
            else
            {
                _priceTMP.text = $"{price}";
            }

            var purchasable = gem >= price;
            _upgradeBTN.enabled = purchasable;
            _upgradeBTN.image.sprite = purchasable ? _enableButtonSprite : _disenableButtonSprite;
        }

        public void ShowGacha(Action doneCallback)
        {
            StartCoroutine(Co_Gacha());

            IEnumerator Co_Gacha()
            {
                var touchBlocker = TempViewHelper.GetTouchBlocker();
                _upgradeBTN.enabled = false;
                _upgradeBTN.image.sprite = _disenableButtonSprite;

                var randomPicker = new WeightedRandomPicker<TalentItemView>();

                foreach (var pair in _itemViewDict)
                {
                    randomPicker.Add(pair.Value, 1);
                }

                yield return CoroutineUtil.WaitForSeconds(0.5f);
                AudioManager.PlaySFX("AudioClip/Talent_Gacha");

                TalentItemView preItemView = null;
                var count = _itemViewDict.Count;
                for (var i = 0; i < count; i++)
                {
                    var itemView = randomPicker.RandomPick();
                    if (preItemView == itemView)
                    {
                        --i;
                        continue;
                    }

                    preItemView = itemView;

                    foreach (var pair in _itemViewDict)
                    {
                        pair.Value.Focus(pair.Value == itemView);
                    }

                    var duration = _gachaCurve.Evaluate((float)i / count) * _delayDuration;

                    yield return CoroutineUtil.WaitForSeconds(duration);
                }

                foreach (var pair in _itemViewDict)
                {
                    pair.Value.Focus(false);
                }
                AudioManager.PlaySFX("AudioClip/Talent_Upgrade");

                _upgradeBTN.enabled = true;
                _upgradeBTN.image.sprite = _enableButtonSprite;
                Destroy(touchBlocker.gameObject);
                doneCallback?.Invoke();
            }
        }

        public void OnClickUpgrade()
        {
            _upgrade?.Invoke();
            FocusOverlayHelper.Clear();
            _guide.SetActive(false);
            AudioManager.PlaySFX("AudioClip/Click");
        }

        public void ShowGuide()
        {
            FocusOverlayHelper.Apply(_upgradeBTN.gameObject);
            _guide.SetActive(true);
        }
    }
}
