using System;
using System.Collections.Generic;
using UnityEngine;
namespace BabyNightmare.Match
{
    public class MatchFailView : MonoBehaviour
    {
        public void Init(Action doneCallback)
        {
            Destroy(gameObject);
            doneCallback?.Invoke();
        }
    }
}