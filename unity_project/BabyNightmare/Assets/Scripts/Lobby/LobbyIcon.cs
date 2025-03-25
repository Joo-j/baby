using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using TMPro;
using Supercent.Core.Audio;

namespace BabyNightmare.Lobby
{
    public class LobbyIcon : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _bounceAniTF;
        [SerializeField] private TextMeshProUGUI _desc;
        [SerializeField] private AnimationTrigger _aniTrigger;

        private readonly int ANI_UNLOCK = Animator.StringToHash("Unlock");
        private readonly int ANI_UNLOCK_IDLE = Animator.StringToHash("Unlock_Idle");
        private readonly int ANI_BOUNCE = Animator.StringToHash("Bounce");
        private Animator _bounceAni;
        private bool _unlocked = false;

        public RectTransform RTF => _rtf;

        public void Init(ELobbyButtonType type)
        {
            _bounceAni = LobbyUtil.GetLobbyIconAni(type, _bounceAniTF);

            _aniTrigger.AddAction(1, () => AudioManager.PlaySFX("AudioClip/Unlock_2"));
            _aniTrigger.AddAction(2, () => AudioManager.PlaySFX("AudioClip/Lobby_Menu_Unlock"));
        }

        public void RefreshDesc(ELobbyButtonType type)
        {
            _desc.text = LobbyUtil.GetDesc(type);
        }

        private void OnEnable()
        {
            if (true == _unlocked)
                _animator.Play(ANI_UNLOCK_IDLE);
        }

        public void Unlock(bool immediate, Action doneCallback = null)
        {
            _unlocked = true;

            if (false == immediate)
            {
                StartCoroutine(Co_Unlock(doneCallback));
                return;
            }

            _animator.Play(ANI_UNLOCK_IDLE);
            doneCallback?.Invoke();
        }

        private IEnumerator Co_Unlock(Action doneCallback)
        {
            yield return CoroutineUtil.WaitForSeconds(0.1f);
            _animator.Play(ANI_UNLOCK);
            yield return null;

            yield return CoroutineUtil.WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

            HapticHelper.Haptic(Lofelt.NiceVibrations.HapticPatterns.PresetType.Success);
            _animator.Play(ANI_UNLOCK_IDLE);

            doneCallback?.Invoke();
        }

        public void Bounce() => _bounceAni.Play(ANI_BOUNCE);
    }
}