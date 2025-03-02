using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supercent.UI
{
    public class ImageTextModule : MonoBehaviour
    {
        [SerializeField]
        private GameObject _basePrefab = null;

        public GameObject GetBasePrefab()
        {
            return _basePrefab;
        }

        public void Release()
        {
            _basePrefab = null;
        }
    }
}
