using System;
using UnityEngine;
using UnityEngine.UI;
using Supercent.UI;
using BabyNightmare.Util;

namespace BabyNightmare.HUD
{
    public class CoinHUD : CurrencyHUDBase, IHUD
    {
        [SerializeField] private Button _shortcutButton;

        public EHUDType Type => EHUDType.Coin;
        public GameObject GO => gameObject;

        protected override void _Start()
        {
            RefreshAll_CurrentType(PlayerData.Instance.Coin);
        }
        protected override void _OnDestroy() { }


        // NOTE: 유저 정보의 재화 정보가 변경되었을 때 HUD를 갱신해주기 위한 이벤트를 등록
        // callback == RefreshAll_CurrentType
        //
        // (ex 1. User.OnChangedCoinEvent.AddListener(RefreshAll_CurrentType))
        // (ex 2. AccountFollower.OnChangeFollower += callback)
        protected override void RegistEvent(Action<int> callback)
        {
            PlayerData.Instance.OnChangedCoinEvent.AddListener(RefreshAll_CurrentType);
        }

        protected override void UnregistEvent(Action<int> callback)
        {
            PlayerData.Instance.OnChangedCoinEvent.RemoveListener(RefreshAll_CurrentType);
        }

        public static void SetSpreadPoint(Vector3 canvasWorldPosition)
        {
            var hud = GetFocusedHUD(typeof(CoinHUD));
            if (null == hud)
                return;

            hud.SetSpreadPoint_FromUI(canvasWorldPosition);
        }

        public static void SetSpreadPoint(Vector3 worldPos, Camera worldCamera, RectTransform uiCanvasRect)
        {
            var hud = GetFocusedHUD(typeof(CoinHUD));
            if (null == hud)
                return;

            hud.SetSpreadPoint_FromWorld(worldPos, worldCamera, uiCanvasRect);
        }

        protected override void PlayHaptic()
        {
            // NOTE: 햅틱 코드
        }
        protected override void _OnAbsorbParticle()
        {
            // NOTE: CurrencyParticle 1개가 최종 AbsorbPoint에 도착했을 때 마다 실행되는 함수
            // * 100개의 파티클이 연출로 출력된다면 해당 파티클이 도착하는 순서대로 함수가 호출되며 총 100번 호출됩니다.
        }



        protected override void _ApplyValue_ForUI(int value)
        {
            // NOTE: UI상의 텍스트(_valueTMP 컴포넌트)가 갱신될 때 마다 호출 되는 함수
        }



        protected override int GetCurrencyValue_FromUserData() => PlayerData.Instance.Coin;
        protected override string ValueToString(int value) => CurrencyUtil.GetUnit(value, true);


        public static void UseFX(bool use)
        {
            var hud = GetFocusedHUD(typeof(CoinHUD));
            if (null == hud)
            {
                //Debug.Log($"{typeof(CoinHUD)}에 맞는 HUD가 없습니다.");
                return;
            }

            hud.UseAbsorbFx = use;
        }

        public void EnableShortcut(bool enable)
        {
            _shortcutButton.gameObject.SetActive(enable);
        }

        public void OnClickCheat()
        {
            PlayerData.Instance.Coin += 10000;
        }
    }
}