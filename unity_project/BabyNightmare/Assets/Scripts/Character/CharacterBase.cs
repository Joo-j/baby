using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Match;
using BabyNightmare.Util;
using UnityEngine;

namespace BabyNightmare.Character
{
    public interface ICharacter
    {
        public GameObject GO { get; }
        public Transform TF { get; }
        public float HP { get; }
        public Transform HitPoint { get; }
        public bool IsAttackable { get; }
        public void ReserveDamage(float damage);
        public void ReceiveAttack(float damage, bool isCritical);
    }
    public enum EAniType
    {
        Idle,
        Move,
        Attack,
    }


    public abstract class CharacterBase : BehaviourBase, ICharacter
    {
        [SerializeField] protected Renderer _mainRenderer;
        [SerializeField] protected Renderer[] _allRenderers;
        [SerializeField] protected Animator _animator;
        [SerializeField] protected AnimationTrigger _animationTrigger;
        [SerializeField] protected SimpleProgress _hpBar = null;
        [SerializeField] private Transform _hitPoint;
        [SerializeField] private Color _ownColor;
        [SerializeField] protected ESizeType _sizeType = ESizeType.Mid;

        protected static readonly int KEY_EMISSION_COLOR = Shader.PropertyToID("_EmissionColor");
        protected static readonly int HASH_ANI_IDLE = Animator.StringToHash("Idle");
        protected static readonly int HASH_ANI_ATTACK = Animator.StringToHash("Attack");
        protected static readonly int HASH_ANI_MOVE = Animator.StringToHash("Move");

        protected float _hp = 0;
        protected float _maxHealth = 0;
        protected Coroutine _coAct = null;
        private Coroutine _coFlash = null;
        protected Color _originEmissionColor;
        private float _reserveDamage = 0f;
        protected bool _isDead = false;

        public GameObject GO => gameObject;
        public Transform TF => transform;
        public Transform HitPoint => _hitPoint;
        public float HP => _hp;
        public bool IsAttackable => _hp - _reserveDamage > 0;

        public abstract void Die();

        public void ReserveDamage(float damage) => _reserveDamage += damage;

        public virtual void ReceiveAttack(float damage, bool isCritical)
        {
            if (damage <= 0)
                return;

            _reserveDamage = 0;

            _hp = Mathf.Max(0, _hp - damage);

            var fxColor = _ownColor;
            if (isCritical)
                fxColor = Color.yellow;

            FXPool.Instance.ShowTemporary(EFXType.Receive_Damage, HitPoint.position, fxColor);

            if (_hp <= 0)
            {
                Die();
                return;
            }

            _hpBar.Refresh(_hp, _maxHealth, false);

            if (null != _coFlash)
                return;

            _coFlash = StartCoroutine(Co_Flash());
        }

        private IEnumerator Co_Flash()
        {
            var originEmissionColorList = new List<Color>();

            for (var i = 0; i < _allRenderers.Length; i++)
                originEmissionColorList.Add(_allRenderers[i].material.GetColor(KEY_EMISSION_COLOR));

            var targetColor = Color.white;
            var duration = 0.1f;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor = elapsed / duration;
                for (var i = 0; i < _allRenderers.Length; i++)
                    _allRenderers[i].material.SetColor(KEY_EMISSION_COLOR, Color.Lerp(originEmissionColorList[i], targetColor, factor));
            }

            while (elapsed > 0)
            {
                yield return null;
                elapsed -= Time.deltaTime;

                var factor = elapsed / duration;
                for (var i = 0; i < _allRenderers.Length; i++)
                    _allRenderers[i].material.SetColor(KEY_EMISSION_COLOR, Color.Lerp(originEmissionColorList[i], targetColor, factor));
            }

            for (var i = 0; i < _allRenderers.Length; i++)
                _allRenderers[i].material.SetColor(KEY_EMISSION_COLOR, originEmissionColorList[i]);

            _coFlash = null;
        }

#if UNITY_EDITOR
        protected override void OnBindSerializedField()
        {
            base.OnBindSerializedField();

            _mainRenderer = this.GetComponentInChildren<Renderer>();
            _allRenderers = this.GetComponentsInChildren<Renderer>();
            _animator = this.GetComponentInChildren<Animator>();
            _animationTrigger = _animator.gameObject.GetComponent<AnimationTrigger>();
            _hpBar = this.FindComponent<SimpleProgress>("HPBar");
            _hitPoint = this.FindComponent<Transform>("HitPoint");
        }

#endif
    }
}