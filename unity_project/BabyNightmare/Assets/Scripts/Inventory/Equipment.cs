using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BabyNightmare.StaticData;

namespace BabyNightmare.InventorySystem
{
    public class Equipment : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Image _image;
        [SerializeField] private Image _coolImage;

        public EquipmentData Data { get; private set; }
        public Action Reset { get; set; }
        public Vector2Int Index { get; set; }
        public Vector2 AnchoredPos
        {
            get => _rtf.anchoredPosition;
            set => _rtf.anchoredPosition = value;
        }
        public bool IsShake { private get; set; }
        public RectTransform RTF => _rtf;

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

        public void StartCoolDownLoop(float coolTime, Action onCoolDown)
        {
            StartCoroutine(Co_CoolDown(true, coolTime, onCoolDown));
        }

        private IEnumerator Co_CoolDown(bool isLoop, float coolTime, Action onCoolDown)
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

                if (false == isLoop)
                    break;
            }
        }

        public void ResetCool()
        {
            StopAllCoroutines();
            _coolImage.fillAmount = 0;
        }
    }
}