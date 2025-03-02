using UnityEngine.UI;

namespace Supercent.UIv2
{
    public interface IClickEventable
    {
        void OnButtonEvent(Button button);
        void OnToggleEvent(Toggle toggle);
    }
}