using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Supercent.UI
{
    public class FastButton : Button
    {

        float sec = 0;
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (Time.realtimeSinceStartup - sec < 0.5f)
                return;
            this.onClick.Invoke();
            sec = Time.realtimeSinceStartup;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            //		base.OnPointerClick (eventData);
        }
    }
}