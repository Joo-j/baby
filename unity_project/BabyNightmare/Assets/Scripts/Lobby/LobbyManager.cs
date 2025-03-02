using System;
using System.Collections.Generic;
using BabyNightmare.HUD;
using UnityEngine;

namespace BabyNightmare.Lobby
{
    public class LobbyManager : SingletoneBase<LobbyManager>
    {
        private const string PATH_LOBBY_VIEW = "Lobby/LobbyView";
        private LobbyView _lobbyView = null;
        private Action _startMatch = null;

        public void Init(Action startMatch)
        {
            _startMatch = startMatch;
            HUDManager.Instance.SetState(EHUDState.Show_Shortcut_On, "Lobby");
        }

        private void CreateView()
        {
            if (null != _lobbyView)
                return;

            var res = Resources.Load<LobbyView>(PATH_LOBBY_VIEW);
            if (null == res)
            {
                Debug.LogError($"{PATH_LOBBY_VIEW} no prefab");
                return;
            }

            _lobbyView = GameObject.Instantiate(res);

            _lobbyView.Init(_startMatch);
        }

        public void Enter()
        {
            CreateView();
            _lobbyView.gameObject.SetActive(true);
        }

        public void Exit()
        {
            _lobbyView.gameObject.SetActive(false);
        }
    }
}