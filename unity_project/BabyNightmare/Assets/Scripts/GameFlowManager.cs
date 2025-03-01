using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BabyNightmare.StaticData;
using BabyNightmare.Character;
using BabyNightmare.Lobby;
using BabyNightmare.Match;

namespace BabyNightmare
{
    public class GameFlowManager
    {
        private static GameFlowManager _instance = null;
        public static GameFlowManager Instance
        {
            get
            {
                if (null == _instance)
                    _instance = new GameFlowManager();

                return _instance;
            }
        }

        public void AppOpen()
        {
            LayerHelper.Init();
            PlayerData.Init();
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