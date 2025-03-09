using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Match
{
    public class MatchCompleteView : MonoBehaviour
    {
        public void Init(Action doneCallback)
        {
            Destroy(gameObject);
            doneCallback?.Invoke();
        }
    }
}
