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

        private PlayerContext _context = null;
        private Queue<EquipmentUseInfo> _useInfoQueue = null;
        private float _def = 0f;

        public override float HitRadius => _hitRadius;

        public override void Init(ICharacterContext context)
        {
            base.Init(context);

            _context = context as PlayerContext;
            _useInfoQueue = new Queue<EquipmentUseInfo>();

            _animationTrigger.AddAction(1, TryUseEquipment);
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
            if (null == gameObject)
                return;
                
            damage -= _def;

            PopupTextPool.Instance.ShowTemporary(transform.position, Quaternion.Euler(_context.CameraForward), $"{damage}", Color.white);

            base.ReceiveAttack(damage);
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

            var pj = ProjectilePool.Instance.Get();
            pj.TF.position = _throwStartTF.position;
            pj.TF.rotation = Quaternion.identity;
            pj.Init(equipmentData.ID);

            if (damage > 0)
            {
                StartCoroutine(Co_ThrowProjectile(pj, enemy.TF, OnThrow));
            }
            else
            {
                StartCoroutine(Co_ThrowProjectile(pj, transform, OnThrow));
            }

            void OnThrow()
            {
                enemy?.ReceiveAttack(equipmentData.Damage);
                _hp = Mathf.Min(_maxHealth, _hp + equipmentData.Heal);
                _hpBar.Refresh(_hp, _maxHealth, false);
                _def += equipmentData.Defence;
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
                    ProjectilePool.Instance.Return(projectile);
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
            ProjectilePool.Instance.Return(projectile);

            doneCallback?.Invoke();
        }
    }
}