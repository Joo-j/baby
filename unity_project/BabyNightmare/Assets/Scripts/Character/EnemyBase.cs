using System;
using System.Collections;
using UnityEngine;
using BabyNightmare.StaticData;

namespace BabyNightmare.Character
{
    public class EnemyContext : ICharacterContext
    {
        public EnemyData EnemyData { get; }
        public Transform TargetTF { get; }
        public Action OnDie { get; }

        public float Health { get; }

        public EnemyContext
        (
            EnemyData enemyData,
            Transform targetTF,
            Action onDie
        )
        {
            EnemyData = enemyData;
            TargetTF = targetTF;
            OnDie = onDie;

            Health = enemyData.Health;
        }
    }

    public class EnemyBase : CharacterBase
    {
        private EnemyContext _context = null;

        public override void Init(ICharacterContext context)
        {
            base.Init(context);

            _context = context as EnemyContext;

            StartMove();
        }

        private void StartMove()
        {
            if (null != _coAct)
                StopCoroutine(_coAct);

            var targetTF = _context.TargetTF;
            var speed = _context.EnemyData.Move_Speed;
            _coAct = StartCoroutine(Co_Move(targetTF, speed));
        }

        private IEnumerator Co_Move(Transform targetTF, float speed)
        {
            while (true)
            {
                var dir = (targetTF.position - transform.position).normalized;
                transform.Translate(dir * Time.deltaTime * speed, Space.World);
                yield return null;
            }
        }

        private void StartAttack(Player player)
        {
            if (null != _coAct)
                StopCoroutine(_coAct);

            var interval = _context.EnemyData.Attack_Interval;

            _coAct = StartCoroutine(Co_Attack(player, interval));
        }

        private IEnumerator Co_Attack(Player player, float interval)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);

                var damage = _context.EnemyData.Damage;
                player.ReceiveAttack(damage);
            }
        }

        public override void Die()
        {
            _context.OnDie?.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (false == other.TryGetComponent<Player>(out var player))
                return;

            StartAttack(player);
        }
    }
}