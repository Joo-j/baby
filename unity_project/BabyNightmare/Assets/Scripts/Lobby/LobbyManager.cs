using System;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Lobby
{
    public class LobbyManager
    {
        private static LobbyManager _instance = null;
        public static LobbyManager Instance
        {
            get
            {
                if (null == _instance)
                    _instance = new LobbyManager();

                return _instance;
            }
        }

        private const string PATH_LOBBY_VIEW = "Lobby/LobbyView";
        private LobbyView _lobbyView = null;
        private Action _startMatch = null;

        public void Init(Action startMatch)
        {
            _startMatch = startMatch;
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