using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BabyNightmare.Util
{
    public class AnimationTrigger : MonoBehaviour
    {
        private Dictionary<int, Action> _actionDict = new Dictionary<int, Action>();

        public void AddAction(int index, Action action)
        {
            if (true == _actionDict.ContainsKey(index))
                return;

            _actionDict.Add(index, action);
        }

        public void RemoveAction(int index)
        {
            if (false == _actionDict.ContainsKey(index))
                return;

            _actionDict.Remove(index);
        }

        public void InvokeAction(int index)
        {
            if (false == _actionDict.TryGetValue(index, out var action))
                return;

            action?.Invoke();
        }

        public void Clear()
        {
            _actionDict.Clear();
        }
    }
}