// 이 파일은 자동 생성되는 파일로 이곳에 추가한 코드는 제거될 수 있습니다.
// 이 파일에 코드를 추가하여 발생하는 버그는 모두 작성자에게 책임이 있습니다.
// 로직은 Base 클래스가 아닌 파일에 작성하세요.

using Supercent.UIv2;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


namespace BabyNightmare.Match
{
    public class MatchFailView_Base : Supercent.UIv2.UIBase
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [System.Serializable] public class Reward
        {
            public GameObject GO_Self;
            public RectTransform RTF_Self;

            public TextMeshProUGUI TMP_Coin;

            public Transform TF_Coin;
        }
        [SerializeField] protected Reward CLS_Reward;

        [System.Serializable] public class Roulette
        {
            public GameObject GO_Self;
            public RectTransform RTF_Self;

            public Transform TF_Arrow;

            public List<GameObject> LIST_GO_Block;
            public List<Image> LIST_IMG_Value;
        }
        [SerializeField] protected Roulette CLS_Roulette;

        [SerializeField] protected Button BTN_RVReward;
        [SerializeField] protected Button BTN_NoThanks;

        [SerializeField] protected GameObject GO_BG;

        [SerializeField] protected Image IMG_Icon;

        [SerializeField] protected ParticleSystem PTC_BG;

        [SerializeField] protected TextMeshProUGUI TMP_RVCoin;


        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
#if UNITY_EDITOR
        protected override void EDITOR_AssignObjects()
        {
            Supercent.UIv2.AssignHelper.MountUI(this);

            CLS_Reward = new Reward()
            {
                GO_Self = UIComponentUtil.FindChild(transform, "RectStretch/CLS_Reward").gameObject,
                RTF_Self = UIComponentUtil.FindComponent<RectTransform>(transform, "RectStretch/CLS_Reward"),
            };
            CLS_Reward.TF_Coin = UIComponentUtil.FindComponent<Transform>(transform, "RectStretch/CLS_Reward/TF_Coin");
            CLS_Reward.TMP_Coin = UIComponentUtil.FindComponent<TextMeshProUGUI>(transform, "RectStretch/CLS_Reward/TF_Coin/TMP_Coin");
            CLS_Roulette = new Roulette()
            {
                GO_Self = UIComponentUtil.FindChild(transform, "RectStretch/CLS_Roulette").gameObject,
                RTF_Self = UIComponentUtil.FindComponent<RectTransform>(transform, "RectStretch/CLS_Roulette"),
            };
            CLS_Roulette.TF_Arrow = UIComponentUtil.FindComponent<Transform>(transform, "RectStretch/CLS_Roulette/TF_Arrow");
            CLS_Roulette.LIST_GO_Block = new List<GameObject>();
            for (int i = 0, size = 5; i < size; ++i)
                CLS_Roulette.LIST_GO_Block.Add(null);
            CLS_Roulette.LIST_GO_Block[0] = UIComponentUtil.FindChild(transform, "RectStretch/CLS_Roulette/BG_Roulette/LIST_GO_Block").gameObject;
            CLS_Roulette.LIST_GO_Block[1] = UIComponentUtil.FindChild(transform, "RectStretch/CLS_Roulette/BG_Roulette/LIST_GO_Block (1)").gameObject;
            CLS_Roulette.LIST_GO_Block[2] = UIComponentUtil.FindChild(transform, "RectStretch/CLS_Roulette/BG_Roulette/LIST_GO_Block (2)").gameObject;
            CLS_Roulette.LIST_GO_Block[3] = UIComponentUtil.FindChild(transform, "RectStretch/CLS_Roulette/BG_Roulette/LIST_GO_Block (3)").gameObject;
            CLS_Roulette.LIST_GO_Block[4] = UIComponentUtil.FindChild(transform, "RectStretch/CLS_Roulette/BG_Roulette/LIST_GO_Block (4)").gameObject;
            CLS_Roulette.LIST_IMG_Value = new List<Image>();
            for (int i = 0, size = 5; i < size; ++i)
                CLS_Roulette.LIST_IMG_Value.Add(null);
            CLS_Roulette.LIST_IMG_Value[0] = UIComponentUtil.FindComponent<Image>(transform, "RectStretch/CLS_Roulette/BG_Roulette/LIST_IMG_Value");
            CLS_Roulette.LIST_IMG_Value[1] = UIComponentUtil.FindComponent<Image>(transform, "RectStretch/CLS_Roulette/BG_Roulette/LIST_IMG_Value (1)");
            CLS_Roulette.LIST_IMG_Value[2] = UIComponentUtil.FindComponent<Image>(transform, "RectStretch/CLS_Roulette/BG_Roulette/LIST_IMG_Value (2)");
            CLS_Roulette.LIST_IMG_Value[3] = UIComponentUtil.FindComponent<Image>(transform, "RectStretch/CLS_Roulette/BG_Roulette/LIST_IMG_Value (3)");
            CLS_Roulette.LIST_IMG_Value[4] = UIComponentUtil.FindComponent<Image>(transform, "RectStretch/CLS_Roulette/BG_Roulette/LIST_IMG_Value (4)");
            GO_BG = UIComponentUtil.FindChild(transform, "RectStretch/GO_BG").gameObject;
            PTC_BG = UIComponentUtil.FindComponent<ParticleSystem>(transform, "RectStretch/GO_BG/PTC_BG");
            IMG_Icon = UIComponentUtil.FindComponent<Image>(transform, "RectStretch/IMG_Icon");
            BTN_RVReward = UIComponentUtil.FindComponent<Button>(transform, "RectStretch/BTN_RVReward");
            Supercent.UIv2.AssignHelper.TryAssignButton(BTN_RVReward);
            TMP_RVCoin = UIComponentUtil.FindComponent<TextMeshProUGUI>(transform, "RectStretch/BTN_RVReward/TMP_RVCoin");
            BTN_NoThanks = UIComponentUtil.FindComponent<Button>(transform, "RectStretch/BTN_NoThanks");
            Supercent.UIv2.AssignHelper.TryAssignButton(BTN_NoThanks);

            _EDITOR_AssignObjectsForUser();
            Supercent.UIv2.AssignHelper.UnmountUI(this);
        }

        protected virtual void _EDITOR_AssignObjectsForUser() {}
#endif
    }
}
