using System.Collections;
using System.Collections.Generic;
using BabyNightmare.Util;
using Supercent.Util;
using UnityEngine;

namespace BabyNightmare.Character
{
    public interface ICharacter
    {
        public GameObject GO { get; }
        public Transform TF { get; }
        public float Health { get; }
        public float HitRadius { get; }
        public void ReceiveAttack(float damage);
    }

    public interface ICharacterContext
    {
        public float HP { get; }
        public Vector3 CameraForward { get; }
    }

    public abstract class CharacterBase : BehaviourBase, ICharacter
    {
        [SerializeField] protected Animator _animator;
        [SerializeField] protected Renderer _renderer;
        [SerializeField] private Transform _hpTF;

        private const string PATH_SIMPLE_PROGRESS = "Util/SimpleProgress";
        private static readonly int KEY_EMISSION = Shader.PropertyToID("_EmissionColor");
        protected static readonly int HASH_ANI_IDLE = Animator.StringToHash("Idle");
        protected static readonly int HASH_ANI_ATTACK = Animator.StringToHash("Attack");
        protected static readonly int HASH_ANI_MOVE = Animator.StringToHash("Move");

        protected SimpleProgress _hpBar = null;
        protected float _hp = 0;
        protected float _maxHealth = 0;
        protected Coroutine _coAct = null;
        private Coroutine _coFlash = null;
        private Color _originColor;

        public GameObject GO => gameObject;
        public Transform TF => transform;
        public float Health => _hp;
        public abstract float HitRadius { get; }

        public virtual void Init(ICharacterContext context)
        {
            _originColor = _renderer.material.color;

            _hp = _maxHealth = context.HP;

            _hpBar = ObjectUtil.LoadAndInstantiate<SimpleProgress>(PATH_SIMPLE_PROGRESS, _hpTF);
            _hpBar.transform.rotation = Quaternion.LookRotation(context.CameraForward);
            _hpBar.Refresh(_hp, _maxHealth, true);
        }

        public abstract void Die();

        public virtual void ReceiveAttack(float damage)
        {
            if (damage <= 0)
                return;

            _hp = Mathf.Max(0, _hp - damage);

            _hpBar.Refresh(_hp, _maxHealth, false);

            if (null != _coFlash)
                StopCoroutine(_coFlash);

            _coFlash = StartCoroutine(Co_FlashRed());

            if (_hp <= 0)
            {
                Die();
            }
        }

        private IEnumerator Co_FlashRed()
        {
            var mat = _renderer.material;
            var targetColor = Color.red;
            var duration = 0.1f;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor = elapsed / duration;
                mat.SetColor(KEY_EMISSION, Color.Lerp(_originColor, targetColor, factor));
            }

            while (elapsed > 0)
            {
                yield return null;
                elapsed -= Time.deltaTime;

                var factor = elapsed / duration;

                mat.SetColor(KEY_EMISSION, Color.Lerp(_originColor, targetColor, factor));
                mat.color = Color.Lerp(_originColor, targetColor, factor);
            }

            mat.SetColor(KEY_EMISSION, _originColor);
            _coFlash = null;
        }
    }
}