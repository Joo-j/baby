using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supercent.Util
{
    public class VisibleFollower : MonoBehaviour
    {
        //--------------------------------------------------------------------------------
        // components
        //--------------------------------------------------------------------------------
        [SerializeField] private List<GameObject> _followers;

        //--------------------------------------------------------------------------------
        // functions
        //--------------------------------------------------------------------------------
        private void OnEnable() 
        {
            IEnumerator Co_Enable()
            {
                yield return null;

                SetVisibleFollowers(true);
            }

            StartCoroutine(Co_Enable());
        }

        private void OnDisable() 
        {
            StopAllCoroutines();
            SetVisibleFollowers(false);
        }

        private void SetVisibleFollowers(bool visible)
        {
            for (int i = 0, size = _followers?.Count ?? 0; i < size; ++i)
                _followers[i]?.SetActive(visible);
        }
    }
}