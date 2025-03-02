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
        public float Health { get; }
        public Action OnDiePlayer { get; }

        public PlayerContext(float health, Action onDiePlayer)
        {
            this.Health = health;
            this.OnDiePlayer = onDiePlayer;
        }
    }

    public class AttackInfo
    {
        public EquipmentData EquipmentData { get; }
        public ICharacter Character { get; }

        public AttackInfo(EquipmentData equipmentData, ICharacter character)
        {
            EquipmentData = equipmentData;
            Character = character;
        }
    }

    public class Player : CharacterBase
    {
        [SerializeField] private Transform _throwStartTF;
        [SerializeField] private AnimationTrigger _aniTrigger;

        private PlayerContext _context = null;
        private Queue<AttackInfo> _attackInfoQueue = null;

        public override void Init(ICharacterContext context)
        {
            base.Init(context);

            _context = context as PlayerContext;
            _attackInfoQueue = new Queue<AttackInfo>();

            _aniTrigger.AddAction(1, OnAnimationAction);
        }

        public override void Die()
        {
            _context.OnDiePlayer?.Invoke();
        }

        public void Attack(EquipmentData equipmentData, ICharacter enemy)
        {
            Debug.Log($"Attack {_attackInfoQueue.Count}");
            _attackInfoQueue.Enqueue(new AttackInfo(equipmentData, enemy));

            _animator.speed = _attackInfoQueue.Count;

            _animator.Rebind();
            _animator.Play(HASH_ANI_ATTACK);
        }

        private void OnAnimationAction()
        {
            Debug.Log($"OnAnimationAction {_attackInfoQueue.Count}");

            if (_attackInfoQueue.Count == 0)
                return;

            var info = _attackInfoQueue.Dequeue();

            ThrowObj(info);
        }

        private void ThrowObj(AttackInfo info)
        {
            var equipmentData = info.EquipmentData;
            var enemy = info.Character;
            if (null == enemy || null == enemy.GO)
                return;

            var obj = ObjectUtil.LoadAndInstantiate<Projectile>(PATH_PROJECTILE, null);
            obj.transform.position = _throwStartTF.position;

            StartCoroutine(Co_ThrowObj(obj.transform, enemy.TF, equipmentData.ThrowDuration,
            () =>
            {
                var damage = equipmentData.Damage;
                enemy?.ReceiveAttack(damage);
            }));
        }

        private IEnumerator Co_ThrowObj(Transform objTF, Transform targetTF, float duration, Action doneCallback)
        {
            var startPos = objTF.position;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = elapsed / duration;

                if (null == targetTF)
                {
                    Destroy(objTF.gameObject);
                    yield break;
                }

                var targetPos = targetTF.position;
                var midPos = Vector3.Lerp(startPos, targetPos, 0.5f);
                midPos.y *= 5;
                objTF.position = VectorExtensions.CalcBezier(startPos, midPos, targetPos, factor);
                objTF.Rotate(Vector3.forward, 10f * Time.deltaTime, Space.Self);
            }

            objTF.position = targetTF.position;
            Destroy(objTF.gameObject);

            doneCallback?.Invoke();
        }
    }
}