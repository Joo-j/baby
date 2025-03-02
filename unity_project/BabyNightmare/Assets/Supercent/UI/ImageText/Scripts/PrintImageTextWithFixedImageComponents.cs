using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.UI
{
    public class PrintImageTextWithFixedImageComponents : MonoBehaviour
    {
        [SerializeField]
        private Image[] _printImages = null;

        private Stack<int> _indexStack = new Stack<int>();

        // Start is called before the first frame update
        void Start()
        {
            
        }

        public void PrintWithFixedImageComponent(int damage)
        {
            for (int i = 0, cnt = _printImages.Length; i < cnt; i++)
            {
                _printImages[i].gameObject.SetActive(false);
            }

            InitIndexStack(damage);

            var stackCount = _indexStack.Count;

            if(stackCount > 0)
            {
                for (int i = 0; i < stackCount; i++)
                {
                    var tmpSprite = ImageTextManager.Instance.GetTargetSprite( ImageTextTheme.Theme01, _indexStack.Pop());

                    var targetImage = _printImages[i];

                    targetImage.gameObject.SetActive(true);
                    targetImage.sprite = tmpSprite;
                }
            }
        }

        private void InitIndexStack(int damage)
        {
            var length = (int)(Mathf.Log10(damage) +1);

            var tmp = damage;

            for(var i = 0; i < length; i++)
            {
                var share = tmp / 10;
                var rest = tmp % 10;

                _indexStack.Push(rest);

                tmp = share;
            }
        }

        #if UNITY_EDITOR

        private void Update() {
            
            // if(Input.GetKeyDown(KeyCode.Q))
            // {
            //     var rndValue = Random.Range(99, 99999);

            //     PrintWithFixedImageComponent(rndValue);
            // }
        }
        #endif
    }
}
