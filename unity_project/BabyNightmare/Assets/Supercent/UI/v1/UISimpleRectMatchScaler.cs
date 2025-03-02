using UnityEngine;

namespace Supercent.UI
{
    public class UISimpleRectMatchScaler : MonoBehaviour
    {
        public enum EMatchMode
        {
            Width,
            Height,
            Expand,
        }

        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private EMatchMode _matchMode = EMatchMode.Expand;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void Start() 
        {
            if (null == transform.parent)
            {
                Debug.LogError($"[UISimpleRectMatchScaler.Start] 부모를 찾을 수 없습니다. name: {name}");
                return;
            }

            var parentRtf = transform.parent.GetComponent<RectTransform>();
            if (null == parentRtf)
            {
                Debug.LogError($"[UISimpleRectMatchScaler.Start] 부모가 RectTransform 을 가지고 있지 않습니다. name: {name}");
                return;
            }

            var selfRtf = GetComponent<RectTransform>();
            if (null == selfRtf)
            {
                Debug.LogError($"[UISimpleRectMatchScaler.Start] 이 오브젝트가 RectTransform 을 가지고 있지 않습니다. name: {name}");
                return;
            }

            var parentSize = new Vector2(parentRtf.rect.width, parentRtf.rect.height);
            var selfSize   = new Vector2(selfRtf.rect.width, selfRtf.rect.height);
            var scale      = 1f;

            switch (_matchMode)
            {
            case EMatchMode.Width:
                scale = parentSize.x / selfSize.x;
                break;

            case EMatchMode.Height:
                scale = parentSize.y / selfSize.y;
                break;

            case EMatchMode.Expand:
                if ((selfSize.x - parentSize.x) < (selfSize.y - parentSize.y))
                    scale = parentSize.y / selfSize.y;
                else
                    scale = parentSize.x / selfSize.x;
                break;
            }

            transform.localScale = Vector3.one * scale;
        }
    }
}