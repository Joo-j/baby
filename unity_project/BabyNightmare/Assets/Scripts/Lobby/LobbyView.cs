using System;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Lobby
{
    public class LobbyView : MonoBehaviour
    {
        private Action _startMatch = null;

        public void Init(Action startMatch)
        {
            _startMatch = startMatch;
        }

        public void OnClickStart()
        {
            _startMatch?.Invoke();
        }
    }
}