using System;
using System.Collections;
using UnityEngine;
using Supercent.Core.Audio;
using Supercent.Util;

namespace BabyNightmare.Match
{
    public class EquipmentBox : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private const string HASH_ANI_OPEN = "Box_Open";
        private bool _isOpened = false;

        public Transform TF => transform;

        public void Open(Action doneCallback)
        {
            if (true == _isOpened)
                return;

            _isOpened = true;
            StartCoroutine(Co_Open(doneCallback));
        }

        IEnumerator Co_Open(Action doneCallback)
        {
            AudioManager.PlaySFX("AudioClip/Field_Box_Open");
            _animator.Rebind();
            _animator.Play(HASH_ANI_OPEN);
            yield return null;

            yield return CoroutineUtil.WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
            yield return CoroutineUtil.WaitForSeconds(0.5f);

            doneCallback?.Invoke();
            Destroy(gameObject);
        }
    }
}