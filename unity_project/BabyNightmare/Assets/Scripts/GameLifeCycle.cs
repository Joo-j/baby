using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare
{
    public class GameLifeCycle : MonoBehaviour
    {
        private static GameLifeCycle _singleton = null;

        private void Awake()
        {
            _singleton = this;
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

        public static Coroutine Start_Coroutine(IEnumerator coroutine)
        {
            return _singleton.StartCoroutine(coroutine);
        }

        public static void Stop_Coroutine(IEnumerator coroutine)
        {
            _singleton.StopCoroutine(coroutine);
        }
    }
}

