// 이 파일은 자동 생성되는 파일로 이곳에 추가한 코드는 제거될 수 있습니다.
// 이 파일에 코드를 추가하여 발생하는 버그는 모두 작성자에게 책임이 있습니다.
// 로직은 Base 클래스가 아닌 파일에 작성하세요.

using Supercent.UIv2;
using UnityEngine.UI;
using UnityEngine;
using TMPro;


namespace BabyNightmare.Match
{
    public class SkinRewardView_Base : Supercent.UIv2.UIBase
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] protected Button BTN_TapToClaim;
        [SerializeField] protected Button BTN_Equip;

        [SerializeField] protected GameObject GO_Title;
        [SerializeField] protected GameObject GO_ConfettisSET;

        [SerializeField] protected Image IMG_Halo;

        [SerializeField] protected RawImage RIMG_Preview;

        [SerializeField] protected TextMeshProUGUI TMP_Desc;
        [SerializeField] protected TextMeshProUGUI TMP_TaptoclaimInfo;
        [SerializeField] protected TextMeshProUGUI TMP_EquipInfo;


        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
#if UNITY_EDITOR
        protected override void EDITOR_AssignObjects()
        {
            Supercent.UIv2.AssignHelper.MountUI(this);

            IMG_Halo = UIComponentUtil.FindComponent<Image>(transform, "IMG_Halo");
            GO_Title = UIComponentUtil.FindChild(transform, "GO_Title").gameObject;
            TMP_Desc = UIComponentUtil.FindComponent<TextMeshProUGUI>(transform, "GO_Title/TMP_Desc");
            RIMG_Preview = UIComponentUtil.FindComponent<RawImage>(transform, "RIMG_Preview");
            BTN_TapToClaim = UIComponentUtil.FindComponent<Button>(transform, "BTN_TapToClaim");
            Supercent.UIv2.AssignHelper.TryAssignButton(BTN_TapToClaim);
            GO_ConfettisSET = UIComponentUtil.FindChild(transform, "GO_ConfettisSET").gameObject;
            BTN_Equip = UIComponentUtil.FindComponent<Button>(transform, "BTN_Equip");
            Supercent.UIv2.AssignHelper.TryAssignButton(BTN_Equip);
            TMP_TaptoclaimInfo = UIComponentUtil.FindComponent<TextMeshProUGUI>(transform, "TMP_TaptoclaimInfo");
            TMP_EquipInfo = UIComponentUtil.FindComponent<TextMeshProUGUI>(transform, "TMP_EquipInfo");

            _EDITOR_AssignObjectsForUser();
            Supercent.UIv2.AssignHelper.UnmountUI(this);
        }

        protected virtual void _EDITOR_AssignObjectsForUser() {}
#endif
    }
}
