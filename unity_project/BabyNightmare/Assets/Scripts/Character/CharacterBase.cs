using System.Collections;
using System.Collections.Generic;
using BabyNightmare.Util;
using Supercent.Util;
using UnityEngine;

namespace BabyNightmare.Character
{
    public interface ICharacterContext
    {
        public float Health { get; }
    }

    public abstract class CharacterBase : BehaviourBase
    {
        [SerializeField] protected Animator _animator;
        [SerializeField] protected MeshRenderer _renderer;
        [SerializeField] private Transform _hpTF;

        private const string PATH_SIMPLE_PROGRESS = "Util/SimpleProgress";
        protected const string PATH_PROJECTILE = "Inventory/Projectile";
        protected static readonly int HASH_ANI_ATTACK = Animator.StringToHash("Attack");
        private SimpleProgress _hpBar = null;
        protected float _currentHealth = 0;
        protected float _maxHealth = 0;
        protected Coroutine _coAct = null;

        public Transform TF => transform;

        public virtual void Init(ICharacterContext context)
        {
            _currentHealth = _maxHealth = context.Health;

            _hpBar = ObjectUtil.LoadAndInstantiate<SimpleProgress>(PATH_SIMPLE_PROGRESS, _hpTF);
            _hpBar.transform.rotation = Quaternion.Euler(0, 0, 0);
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