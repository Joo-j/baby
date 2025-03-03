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
        [SerializeField] private float _hitRadius = 2f;
        [SerializeField] private AnimationTrigger _aniTrigger;

        private PlayerContext _context = null;
        private Queue<AttackInfo> _throwInfoQueue = null;

        public override float HitRadius => _hitRadius;

        public override void Init(ICharacterContext context)
        {
            base.Init(context);

            _context = context as PlayerContext;
            _throwInfoQueue = new Queue<AttackInfo>();

            _aniTrigger.AddAction(1, TryThrowObj);
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

        public void Attack(EquipmentData equipmentData, ICharacter enemy)
        {
            _throwInfoQueue.Enqueue(new AttackInfo(equipmentData, enemy));

            if (_throwInfoQueue.Count > 1)
            {
                TryThrowObj();
                return;
            }

            _animator.Rebind();
            _animator.Play(HASH_ANI_ATTACK);
        }

        private void TryThrowObj()
        {
            if (_throwInfoQueue.Count == 0)
                return;

            var info = _throwInfoQueue.Dequeue();

            var enemy = info.Character;
            if (null == enemy)
                return;

            if (enemy.Health <= 0)
                return;

            var equipmentData = info.EquipmentData;
            var damage = equipmentData.Damage;

            var obj = ObjectUtil.LoadAndInstantiate<Projectile>(PATH_PROJECTILE, null);
            obj.transform.position = _throwStartTF.position;

            StartCoroutine(Co_ThrowObj(obj.transform, enemy.TF, equipmentData.ThrowDuration, () => enemy?.ReceiveAttack(damage)));
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