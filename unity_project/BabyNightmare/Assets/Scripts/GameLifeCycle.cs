using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare
{
    public class GameLifeCycle : MonoBehaviour
    {
        private void Awake()
        {

        }

        private void OnEnable()
        {

        }

        private void Start()
        {
            GameFlowManager.Instance.AppOpen();
        }

        private void Update()
        {

        }


        private void OnApplicationPause(bool pause)
        {

        }

        private void OnApplicationQuit()
        {

        }


        private void OnDisable()
        {

        }

        private void OnDestroy()
        {

        }
    }
}

