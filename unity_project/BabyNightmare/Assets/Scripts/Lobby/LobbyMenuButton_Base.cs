// 이 파일은 자동 생성되는 파일로 이곳에 추가한 코드는 제거될 수 있습니다.
// 이 파일에 코드를 추가하여 발생하는 버그는 모두 작성자에게 책임이 있습니다.
// 로직은 Base 클래스가 아닌 파일에 작성하세요.

using Supercent.UIv2;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


namespace BabyNightmare.Lobby
{
    public class LobbyMenuButton_Base : Supercent.UIv2.UIBase
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] protected Button BTN_Click;

        [SerializeField] protected GameObject GO_ChoiceBG;
        [SerializeField] protected GameObject GO_Line;
        [SerializeField] protected GameObject GO_Lock;
        [SerializeField] protected GameObject GO_Guide;

        [SerializeField] protected Image IMG_RedDot;

        [SerializeField] protected ParticleSystem PTC_Paper;

        [SerializeField] protected TextMeshProUGUI TMP_Desc;


        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
#if UNITY_EDITOR
        protected override void EDITOR_AssignObjects()
        {
            Supercent.UIv2.AssignHelper.MountUI(this);

            GO_ChoiceBG = UIComponentUtil.FindChild(transform, "GO_ChoiceBG").gameObject;
            GO_Line = UIComponentUtil.FindChild(transform, "GO_Line").gameObject;
            GO_Lock = UIComponentUtil.FindChild(transform, "GO_Lock").gameObject;
            PTC_Paper = UIComponentUtil.FindComponent<ParticleSystem>(transform, "LobbyIcon/PTC_Paper");
            TMP_Desc = UIComponentUtil.FindComponent<TextMeshProUGUI>(transform, "TMP_Desc");
            IMG_RedDot = UIComponentUtil.FindComponent<Image>(transform, "IMG_RedDot");
            GO_Guide = UIComponentUtil.FindChild(transform, "GO_Guide").gameObject;
            BTN_Click = UIComponentUtil.FindComponent<Button>(transform, "BTN_Click");
            Supercent.UIv2.AssignHelper.TryAssignButton(BTN_Click);

            _EDITOR_AssignObjectsForUser();
            Supercent.UIv2.AssignHelper.UnmountUI(this);
        }

        protected virtual void _EDITOR_AssignObjectsForUser() {}
#endif
    }
}
