using System;
using System.Collections;
using UnityEngine;
using Supercent.Util;

namespace BabyNightmare.Match
{
    public class EquipmentBox : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private const string HASH_ANI_OPEN = "Open";
        private bool _isOpened = false;

        public void Open(Action doneCallback)
        {
            if (true == _isOpened)
                return;

            _isOpened = true;
            StartCoroutine(Co_Open(doneCallback));
        }

        IEnumerator Co_Open(Action doneCallback)
        {
            _animator.Play(HASH_ANI_OPEN);
            yield return null;

            yield return CoroutineUtil.WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

            doneCallback?.Invoke();
            Destroy(gameObject);
        }
    }
}