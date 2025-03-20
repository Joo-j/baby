using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;
using BabyNightmare.Match;
using BabyNightmare.HUD;
using Unity.VisualScripting;

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

            var statDataList = equipmentData.StatDataList;
            for (var i = 0; i < statDataList.Count; i++)
            {
                var statData = statDataList[i];
                var value = statData.Value;
                switch (statData.Type)
                {
                    case EStatType.ATK:
                        {
                            var enemy = info.Character;
                            if (null == enemy)
                                return;

                            if (false == enemy.IsAttackable)
                                return;

                            enemy.ReserveDamage(value);

                            StartCoroutine(Co_ThrowProjectile(equipmentData, enemy.HitPoint, () => enemy?.ReceiveAttack(value)));
                            break;
                        }

                    case EStatType.HP:
                        {
                            StartCoroutine(Co_ThrowProjectile(equipmentData, transform, () =>
                            {
                                _hp = Mathf.Min(_maxHealth, _hp + value);
                                _hpBar.Refresh(_hp, _maxHealth, false);
                            }));
                            break;
                        }

                    case EStatType.DEF:
                        {
                            StartCoroutine(Co_ThrowProjectile(equipmentData, transform, () => _def += value));
                            break;
                        }
                    case EStatType.Coin:
                        {
                            StartCoroutine(Co_ThrowProjectile(equipmentData, transform, () => _context.GetCoin?.Invoke((int)value, transform.position)));
                            break;
                        }
                    default: throw new Exception($"{statData.Type}의 행동이 정해지지 않았습니다.");
                }
            }
        }
        
        private IEnumerator Co_ThrowProjectile(EquipmentData data, Transform targetTF, Action doneCallback)
        {
            var pt = ProjectilePool.Instance.Get();
            pt.TF.position = _throwStartTF.position;
            pt.TF.rotation = Quaternion.identity;
            pt.Init(data.ID);

            var duration = pt.Duration;
            var curve = pt.Curve;
            var startAngle = Vector3.zero;
            var targetAngle = pt.TargetAngle;
            var startPos = pt.TF.position;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (null == targetTF)
                {
                    ProjectilePool.Instance.Return(pt);
                    yield break;
                }

                elapsed += Time.deltaTime;
                var factor = curve.Evaluate(elapsed / duration);
                var targetPos = targetTF.position;
                var midPos = Vector3.Lerp(startPos, targetPos, 0.5f);

                midPos.y += pt.BezierHeight;
                pt.TF.position = VectorExtensions.CalcBezier(startPos, midPos, targetPos, factor);
                pt.TF.eulerAngles = Vector3.Lerp(startAngle, targetAngle, factor);
                yield return null;
            }

            pt.TF.position = targetTF.position;
            ProjectilePool.Instance.Return(pt);

            doneCallback?.Invoke();
        }
    }
}