using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Supercent.UIv2
{
    public class UIBase : MonoBehaviour, IAssignable, IClickEventable
    {
        public virtual void OnButtonEvent(Button button)
        {
        }

        public virtual void OnToggleEvent(Toggle toggle)
        {
        }

#if UNITY_EDITOR
        protected virtual void EDITOR_AssignObjects()
        {
        }
#endif
    }
}