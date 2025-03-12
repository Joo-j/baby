// 이 파일은 자동 생성되는 파일로 이곳에 추가한 코드는 제거될 수 있습니다.
// 이 파일에 코드를 추가하여 발생하는 버그는 모두 작성자에게 책임이 있습니다.
// 로직은 Base 클래스가 아닌 파일에 작성하세요.

using Supercent.UIv2;
using UnityEngine;


namespace BabyNightmare.Lobby
{
    public class LobbyView_Base : Supercent.UIv2.UIBase
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] protected GameObject GO_LoadingIcon;

        [SerializeField] protected RectTransform RTF_Banner;
        [SerializeField] protected RectTransform RTF_Screen;


        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
#if UNITY_EDITOR
        protected override void EDITOR_AssignObjects()
        {
            Supercent.UIv2.AssignHelper.MountUI(this);

            RTF_Screen = UIComponentUtil.FindComponent<RectTransform>(transform, "RTF_Screen");
            RTF_Banner = UIComponentUtil.FindComponent<RectTransform>(transform, "StretchRect/RTF_Banner");
            GO_LoadingIcon = UIComponentUtil.FindChild(transform, "StretchRect/RTF_Banner/GO_LoadingIcon").gameObject;

            _EDITOR_AssignObjectsForUser();
            Supercent.UIv2.AssignHelper.UnmountUI(this);
        }

        protected virtual void _EDITOR_AssignObjectsForUser() {}
#endif
    }
}
