using System;
using System.Collections;
using UnityEngine;
using BabyNightmare.StaticData;
using Supercent.Util;

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
        [SerializeField] private AnimationCurve _moveCurve;

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

            _coAct = StartCoroutine(Co_IntervalMove());
        }

        private IEnumerator Co_IntervalMove()
        {
            var targetTF = _context.TargetTF;
            var moveStepDuration = _context.EnemyData.Move_Step_Duration;
            var moveStepSpeed = _context.EnemyData.Move_Step_Speed;
            var stopStepDuration = _context.EnemyData.Stop_Step_Duration;

            while (true)
            {
                var elapsed = 0f;
                var startPos = transform.position;
                var dir = (targetTF.position - startPos).normalized;

                var moveDist = moveStepSpeed * moveStepDuration;
                var targetPos = startPos + dir * moveDist;

                var dist = Vector3.Distance(startPos, targetTF.position);
                if (dist < moveDist)
                {
                    targetPos = targetTF.position;
                }

                while (elapsed < moveStepDuration)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                    var factor = _moveCurve.Evaluate(elapsed / moveStepDuration);
                    transform.position = Vector3.Lerp(startPos, targetPos, factor);
                }

                yield return CoroutineUtil.WaitForSeconds(stopStepDuration);
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