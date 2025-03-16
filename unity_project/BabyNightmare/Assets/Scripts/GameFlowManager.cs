using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BabyNightmare.StaticData;
using BabyNightmare.Character;
using BabyNightmare.Lobby;
using BabyNightmare.Match;
using BabyNightmare.HUD;
using BabyNightmare.Talent;

namespace BabyNightmare
{
    public class GameFlowManager : SingletonBase<GameFlowManager>
    {
        public void AppOpen()
        {
            LayerHelper.Init();
            PlayerData.Instance.Init();
            DevManager.Instance.Init();
            HUDManager.Instance.Init();
            HUDManager.Instance.SetState(EHUDState.Show_Shortcut_Off, "AppOpen");
            StaticDataManager.Instance.Init();
            TalentManager.Instance.Init();
            LobbyManager.Instance.Init(StartMatch);
            MatchManager.Instance.Init(EnterLobby);

            EnterLobby();
        }

        public void EnterLobby()
        {
            HUDManager.Instance.ActiveHUD(EHUDType.Gem, true);
            LobbyManager.Instance.Enter();
        }

        private void StartMatch()
        {
            HUDManager.Instance.ActiveHUD(EHUDType.Gem, false);

            LobbyManager.Instance.Exit();
            MatchManager.Instance.StartMatch();
        }
    }
}