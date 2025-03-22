using System;
using System.Collections;
using System.Collections.Generic;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
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

        private const string PATH_TALENT_ITEM_VIEW = "Talent/TalentItemView";

        private Dictionary<ETalentType, TalentItemView> _itemViewDict = null;
        private Action _upgrade = null;
        private Func<int> _getPrice = null;

        public void Init(List<TalentData> dataList, Action upgrade, Func<int> getPrice)
        {
            _itemViewDict = new Dictionary<ETalentType, TalentItemView>();

            foreach (var data in dataList)
            {
                var itemView = ObjectUtil.LoadAndInstantiate<TalentItemView>(PATH_TALENT_ITEM_VIEW, _gridTF);
                itemView.Init(data);

                _itemViewDict.Add(data.TalentType, itemView);
            }

            _upgrade = upgrade;
            _getPrice = getPrice;
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

        public void RefreshButton(int gem)
        {
            var price = _getPrice.Invoke();
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
                _canvasGroup.interactable = false;
                _upgradeBTN.enabled = false;
                _upgradeBTN.image.sprite = _disenableButtonSprite;

                var randomPicker = new WeightedRandomPicker<TalentItemView>();

                foreach (var pair in _itemViewDict)
                {
                    randomPicker.Add(pair.Value, 1);
                }

                yield return CoroutineUtil.WaitForSeconds(0.5f);

                var count = _itemViewDict.Count;
                for (var i = 0; i < count; i++)
                {
                    var itemView = randomPicker.RandomPick();
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

                _canvasGroup.interactable = true;
                doneCallback?.Invoke();
            }
        }

        public void OnClickUpgrade()
        {
            _upgrade?.Invoke();
        }
    }
}
