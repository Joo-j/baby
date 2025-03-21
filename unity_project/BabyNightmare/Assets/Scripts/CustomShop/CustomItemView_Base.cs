// 이 파일은 자동 생성되는 파일로 이곳에 추가한 코드는 제거될 수 있습니다.
// 이 파일에 코드를 추가하여 발생하는 버그는 모두 작성자에게 책임이 있습니다.
// 로직은 Base 클래스가 아닌 파일에 작성하세요.

using Supercent.UIv2;
using UnityEngine.UI;
using UnityEngine;
using TMPro;


namespace BabyNightmare.CustomShop
{
    public class CustomItemView_Base : Supercent.UIv2.UIBase
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] protected Button BTN_Select;

        [SerializeField] protected GameObject GO_Equip;
        [SerializeField] protected GameObject GO_Cost;
        [SerializeField] protected GameObject GO_Select;
        [SerializeField] protected GameObject GO_RedDot;

        [SerializeField] protected Image IMG_Icon;
        [SerializeField] protected Image IMG_EquipCheckBox;
        [SerializeField] protected Image IMG_Cost;

        [SerializeField] protected TextMeshProUGUI TMP_Cost;
        [SerializeField] protected TextMeshProUGUI TMP_FeedID;


        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
#if UNITY_EDITOR
        protected override void EDITOR_AssignObjects()
        {
            Supercent.UIv2.AssignHelper.MountUI(this);

            BTN_Select = UIComponentUtil.FindComponent<Button>(transform, "BTN_Select");
            Supercent.UIv2.AssignHelper.TryAssignButton(BTN_Select);
            IMG_Icon = UIComponentUtil.FindComponent<Image>(transform, "IMG_Icon");
            IMG_EquipCheckBox = UIComponentUtil.FindComponent<Image>(transform, "IMG_EquipCheckBox");
            GO_Equip = UIComponentUtil.FindChild(transform, "GO_Equip").gameObject;
            GO_Cost = UIComponentUtil.FindChild(transform, "GO_Cost").gameObject;
            IMG_Cost = UIComponentUtil.FindComponent<Image>(transform, "GO_Cost/IMG_Cost");
            TMP_Cost = UIComponentUtil.FindComponent<TextMeshProUGUI>(transform, "GO_Cost/TMP_Cost");
            GO_Select = UIComponentUtil.FindChild(transform, "GO_Select").gameObject;
            GO_RedDot = UIComponentUtil.FindChild(transform, "ReddotPos/GO_RedDot").gameObject;
            TMP_FeedID = UIComponentUtil.FindComponent<TextMeshProUGUI>(transform, "TMP_FeedID");

            _EDITOR_AssignObjectsForUser();
            Supercent.UIv2.AssignHelper.UnmountUI(this);
        }

        protected virtual void _EDITOR_AssignObjectsForUser() {}
#endif
    }
}
