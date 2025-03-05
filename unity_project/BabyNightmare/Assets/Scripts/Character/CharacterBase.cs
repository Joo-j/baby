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
        public float Health { get; }
    }

    public abstract class CharacterBase : BehaviourBase, ICharacter
    {
        [SerializeField] protected Animator _animator;
        [SerializeField] protected Renderer _renderer;
        [SerializeField] private Transform _hpTF;

        private const string PATH_SIMPLE_PROGRESS = "Util/SimpleProgress";
        protected const string PATH_PROJECTILE = "Inventory/Projectile";
        protected static readonly int HASH_ANI_IDLE = Animator.StringToHash("Idle");
        protected static readonly int HASH_ANI_ATTACK = Animator.StringToHash("Attack");
        protected static readonly int HASH_ANI_MOVE = Animator.StringToHash("Move");

        private SimpleProgress _hpBar = null;
        protected float _heath = 0;
        protected float _maxHealth = 0;
        protected Coroutine _coAct = null;
        private Coroutine _coFlash = null;
        private Color _originColor;

        public GameObject GO => gameObject;
        public Transform TF => transform;
        public float Health => _heath;
        public abstract float HitRadius { get; }

        public virtual void Init(ICharacterContext context)
        {
            _originColor = _renderer.material.color;

            _heath = _maxHealth = context.Health;

            _hpBar = ObjectUtil.LoadAndInstantiate<SimpleProgress>(PATH_SIMPLE_PROGRESS, _hpTF);
            _hpBar.transform.rotation = Quaternion.Euler(0, 0, 0);
            _hpBar.Refresh(_heath, _maxHealth, true);
        }

        public abstract void Die();

        public void ReceiveAttack(float damage)
        {
            if (damage <= 0)
                return;

            _heath = Mathf.Max(0, _heath - damage);

            _hpBar.Refresh(_heath, _maxHealth, false);

            if (null != _coFlash)
                StopCoroutine(_coFlash);

            _coFlash = StartCoroutine(Co_FlashRed());

            if (_heath <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0)
                return;

            _heath = Mathf.Min(_maxHealth, _heath + amount);
            _hpBar.Refresh(_heath, _maxHealth, false);
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
                mat.color = Color.Lerp(_originColor, targetColor, factor);
            }

            while (elapsed > 0)
            {
                yield return null;
                elapsed -= Time.deltaTime;

                var factor = elapsed / duration;
                mat.color = Color.Lerp(_originColor, targetColor, factor);
            }

            mat.color = _originColor;
            _coFlash = null;
        }
    }
}