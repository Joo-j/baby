using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using BabyNightmare.Match;
using BabyNightmare.HUD;

namespace BabyNightmare.Character
{
    public class PlayerContext : ICharacterContext
    {
        public float HP { get; }
        public Vector3 CameraForward { get; }
        public Action OnDiePlayer { get; }
        public Action<int, Vector3> GetCoin { get; }

        public PlayerContext(
        float hp,
        Vector3 cameraForward,
        Action onDiePlayer,
        Action<int, Vector3> getCoin)
        {
            this.HP = Mathf.Max(50, hp);
            this.CameraForward = cameraForward;
            this.OnDiePlayer = onDiePlayer;
            this.GetCoin = getCoin;
        }
    }

    public class EquipmentUseInfo
    {
        public EquipmentData EquipmentData { get; }
        public ICharacter Character { get; }

        public EquipmentUseInfo(EquipmentData equipmentData, ICharacter character)
        {
            EquipmentData = equipmentData;
            Character = character;
        }
    }

    public class Player : CharacterBase
    {
        [SerializeField] private Transform _throwStartTF;
        [SerializeField] private float _hitRadius = 2f;
        [SerializeField] private AnimationTrigger _aniTrigger;
        [SerializeField] private Projectile _projectileRes;

        private PlayerContext _context = null;
        private Queue<EquipmentUseInfo> _useInfoQueue = null;
        private float _def = 0f;
        private Pool<Projectile> _projectilePool = null;

        public override float HitRadius => _hitRadius;

        public override void Init(ICharacterContext context)
        {
            base.Init(context);

            _context = context as PlayerContext;
            _useInfoQueue = new Queue<EquipmentUseInfo>();

            _aniTrigger.AddAction(1, TryUseEquipment);

            _projectilePool = new Pool<Projectile>(() => Instantiate(_projectileRes));
        }

        public void ShowMoveAni()
        {
            _animator.Play(HASH_ANI_MOVE);
        }

        public void ShowIdleAni()
        {
            _animator.Play(HASH_ANI_IDLE);
        }

        public override void Die()
        {
            if (true == _isDead)
                return;

            _context.OnDiePlayer?.Invoke();
            _isDead = true;
        }

        public override void ReceiveAttack(float damage)
        {
            damage -= _def;

            base.ReceiveAttack(damage);
        }

        public void AddHP(float amount)
        {
            if (amount <= 0)
                return;

            _hp = Mathf.Min(_maxHealth, _hp + amount);
            _hpBar.Refresh(_hp, _maxHealth, false);
        }

        public void AddDEF(float amount)
        {
            _def += amount;
        }

        public void UseEquipment(EquipmentData equipmentData, ICharacter enemy)
        {
            _useInfoQueue.Enqueue(new EquipmentUseInfo(equipmentData, enemy));

            if (_useInfoQueue.Count > 1)
            {
                TryUseEquipment();
                return;
            }

            _animator.Rebind();
            _animator.Play(HASH_ANI_ATTACK);
        }

        private void TryUseEquipment()
        {
            if (_useInfoQueue.Count == 0)
                return;

            var info = _useInfoQueue.Dequeue();

            var equipmentData = info.EquipmentData;

            var enemy = info.Character;
            if (null == enemy)
                return;

            if (false == enemy.IsAttackable)
                return;

            var damage = equipmentData.Damage;
            enemy.ReserveDamage(damage);

            var projectile = _projectilePool.Get();
            projectile.TF.position = _throwStartTF.position;
            projectile.TF.rotation = Quaternion.identity;
            projectile.Init(equipmentData.ID);

            if (damage > 0)
            {
                StartCoroutine(Co_ThrowProjectile(projectile, enemy.TF, OnThrow));
                return;
            }

            var heal = equipmentData.Heal;
            if (heal > 0)
            {
                StartCoroutine(Co_ThrowProjectile(projectile, transform, OnThrow));
            }

            var defence = equipmentData.Defence;
            if (defence > 0)
            {
                StartCoroutine(Co_ThrowProjectile(projectile, transform, OnThrow));
            }

            var coin = equipmentData.Coin;
            if (coin > 0)
            {
                StartCoroutine(Co_ThrowProjectile(projectile, transform, OnThrow));
            }

            void OnThrow()
            {
                enemy?.ReceiveAttack(equipmentData.Damage);
                AddHP(equipmentData.Heal);
                AddDEF(equipmentData.Defence);
                _context.GetCoin?.Invoke(equipmentData.Coin, transform.position);
            }
        }

        private IEnumerator Co_ThrowProjectile(Projectile projectile, Transform targetTF, Action doneCallback)
        {
            var duration = projectile.Duration;
            var curve = projectile.Curve;
            var startAngle = Vector3.zero;
            var targetAngle = projectile.TargetAngle;
            var startPos = projectile.TF.position;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (null == targetTF)
                {
                    Destroy(projectile.gameObject);
                    yield break;
                }

                elapsed += Time.deltaTime;
                var factor = curve.Evaluate(elapsed / duration);
                var targetPos = targetTF.position;
                var midPos = Vector3.Lerp(startPos, targetPos, 0.5f);

                midPos.y += projectile.BezierHeight;
                projectile.TF.position = VectorExtensions.CalcBezier(startPos, midPos, targetPos, factor);
                projectile.TF.eulerAngles = Vector3.Lerp(startAngle, targetAngle, factor);
                yield return null;
            }

            projectile.TF.position = targetTF.position;
            Destroy(projectile.gameObject);

            doneCallback?.Invoke();
        }
    }
}