using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BabyNightmare.StaticData;
using BabyNightmare.Character;
using BabyNightmare.Lobby;
using BabyNightmare.Match;
using BabyNightmare.HUD;

namespace BabyNightmare
{
    public class GameFlowManager : SingletonBase<GameFlowManager>
    {
        public void AppOpen()
        {
            LayerHelper.Init();
            PlayerData.Instance.Init();
            HUDManager.Instance.Init();
            HUDManager.Instance.SetState(EHUDState.Show_Shortcut_Off, "AppOpen");
            StaticDataManager.Instance.Init();
            LobbyManager.Instance.Init(StartMatch);
            MatchManager.Instance.Init(EnterLobby);

            EnterLobby();
        }

        private void EnterLobby()
        {
            LobbyManager.Instance.Enter();
        }

        private void StartMatch()
        {
            LobbyManager.Instance.Exit();
            MatchManager.Instance.StartMatch();
        }
    }
}