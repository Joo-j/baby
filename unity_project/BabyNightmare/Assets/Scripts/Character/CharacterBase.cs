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
        public float HP { get; }
        public float HitRadius { get; }
        public bool IsAttackable { get; }
        public void ReserveDamage(float damage);
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
        [SerializeField] protected SimpleProgress _hpBar = null;

        private const string PATH_SIMPLE_PROGRESS = "Util/SimpleProgress";
        private static readonly int KEY_EMISSION = Shader.PropertyToID("_EmissionColor");
        protected static readonly int HASH_ANI_IDLE = Animator.StringToHash("Idle");
        protected static readonly int HASH_ANI_ATTACK = Animator.StringToHash("Attack");
        protected static readonly int HASH_ANI_MOVE = Animator.StringToHash("Move");

        protected float _hp = 0;
        protected float _maxHealth = 0;
        protected Coroutine _coAct = null;
        private Coroutine _coFlash = null;
        private Color _originColor;
        private float _reserveDamage = 0f;
        protected bool _isDead = false;

        public abstract float HitRadius { get; }
        public GameObject GO => gameObject;
        public Transform TF => transform;
        public float HP => _hp;
        public bool IsAttackable => _hp - _reserveDamage > 0;

        public virtual void Init(ICharacterContext context)
        {
            _originColor = _renderer.material.GetColor(KEY_EMISSION);

            _hp = _maxHealth = context.HP;

            _hpBar.transform.rotation = Quaternion.LookRotation(context.CameraForward);
            _hpBar.Refresh(_hp, _maxHealth, true);
        }

        public abstract void Die();

        public void ReserveDamage(float damage) => _reserveDamage += damage;

        public virtual void ReceiveAttack(float damage)
        {
            if (damage <= 0)
                return;

            _reserveDamage = 0;

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
            var targetColor = Color.white;
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
            }

            mat.SetColor(KEY_EMISSION, _originColor);
            _coFlash = null;
        }
    }
}