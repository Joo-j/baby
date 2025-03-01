using System.Collections;
using System.Collections.Generic;
using BabyNightmare.Util;
using UnityEngine;

namespace BabyNightmare.Character
{
    public interface ICharacterContext
    {
        public float Health { get; }
    }

    public abstract class CharacterBase : MonoBehaviour
    {
        [SerializeField] protected Animator _animator;
        [SerializeField] protected MeshRenderer _renderer;

        protected const float THROW_EQUIPMENT_DRUATION = 1f;
        private const string PATH_SIMPLE_PROGRESS = "Util/SimpleProgress";
        protected const string PATH_PROJECTILE = "Equipment/Projectile/Projectile_";
        protected static readonly int HASH_ANI_ATTACK = Animator.StringToHash("Attack");
        private SimpleProgress _hpBar = null;
        protected float _currentHealth = 0;
        protected float _maxHealth = 0;
        protected Coroutine _coAct = null;

        public Transform TF => transform;

        public virtual void Init(ICharacterContext context)
        {
            _currentHealth = _maxHealth = context.Health;

            var res = Resources.Load<SimpleProgress>(PATH_SIMPLE_PROGRESS);
            _hpBar = GameObject.Instantiate(res, transform);
            _hpBar.transform.localPosition = new Vector3(0, 1.4f, 0);
            _hpBar.Refresh(_currentHealth, _maxHealth, true);
        }

        public abstract void Die();

        public void ReceiveAttack(float damage)
        {
            _currentHealth = Mathf.Max(0, _currentHealth - damage);

            _hpBar.Refresh(_currentHealth, _maxHealth, false);

            if (_currentHealth == 0)
            {
                Die();
            }
        }
    }
}

