using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BabyNightmare.InventorySystem;
using BabyNightmare.StaticData;
using Supercent.Util;
using BabyNightmare.Util;

namespace BabyNightmare.Character
{
    public class PlayerContext : ICharacterContext
    {
        public float HP { get; }
        public Action OnDiePlayer { get; }

        public PlayerContext(
        float hp,
        Action onDiePlayer)
        {
            this.HP = hp;
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

        private PlayerContext _context = null;
        private Queue<EquipmentUseInfo> _useInfoQueue = null;
        private float _def = 0f;

        public override float HitRadius => _hitRadius;

        public override void Init(ICharacterContext context)
        {
            base.Init(context);

            _context = context as PlayerContext;
            _useInfoQueue = new Queue<EquipmentUseInfo>();

            _aniTrigger.AddAction(1, TryUseEquipment);
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

            if (enemy.Health <= 0)
                return;

            var damage = equipmentData.Damage;

            if (damage > 0 && equipmentData.ThrowDuration > 0)
            {
                var obj = ObjectUtil.LoadAndInstantiate<Projectile>(PATH_PROJECTILE, null);
                obj.transform.position = _throwStartTF.position;

                StartCoroutine(Co_ThrowObj(obj.transform, enemy.TF, equipmentData.ThrowDuration, () => enemy?.ReceiveAttack(damage)));
            }
        }

        private IEnumerator Co_ThrowObj(Transform objTF, Transform targetTF, float duration, Action doneCallback)
        {
            var startPos = objTF.position;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (null == targetTF)
                {
                    Destroy(objTF.gameObject);
                    yield break;
                }

                elapsed += Time.deltaTime;
                var factor = elapsed / duration;
                var targetPos = targetTF.position;
                var midPos = Vector3.Lerp(startPos, targetPos, 0.5f);
                midPos.y *= 5;
                objTF.position = VectorExtensions.CalcBezier(startPos, midPos, targetPos, factor);
                objTF.Rotate(Vector3.forward, 10f * Time.deltaTime, Space.Self);
                yield return null;
            }

            objTF.position = targetTF.position;
            Destroy(objTF.gameObject);

            doneCallback?.Invoke();
        }
    }
}