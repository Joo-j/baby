using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using BabyNightmare.Match;

namespace BabyNightmare.Character
{
    public class PlayerContext : ICharacterContext
    {
        public float HP { get; }
        public Vector3 CameraForward { get; }
        public Action OnDiePlayer { get; }

        public PlayerContext(
        float hp,
        Vector3 cameraForward,
        Action onDiePlayer)
        {
            this.HP = hp;
            this.CameraForward = cameraForward;
            this.OnDiePlayer = onDiePlayer;
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

        public void Move(float duration, Action doneCallback)
        {
            _animator.Play(HASH_ANI_MOVE);

            this.Invoke(CoroutineUtil.WaitForSeconds(duration), () =>
            {
                _animator.Play(HASH_ANI_IDLE);
                doneCallback?.Invoke();
            });
        }

        public override void Die()
        {
            _context.OnDiePlayer?.Invoke();
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

            AddHP(equipmentData.Heal);
            AddDEF(equipmentData.Defence);

            var enemy = info.Character;
            if (null == enemy)
                return;

            if (enemy.HP <= 0)
                return;

            var damage = equipmentData.Damage;

            if (damage > 0)
            {
                var projectile = _projectilePool.Get();
                projectile.TF.position = _throwStartTF.position;
                projectile.TF.rotation = Quaternion.identity;
                projectile.Init(equipmentData.ID);

                StartCoroutine(Co_ThrowProjectile(projectile, enemy.TF, () => enemy?.ReceiveAttack(damage)));
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