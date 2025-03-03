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
        public ICharacter Player { get; }
        public Action OnDie { get; }

        public float Health { get; }

        public EnemyContext
        (
            EnemyData enemyData,
            ICharacter player,
            Action onDie
        )
        {
            EnemyData = enemyData;
            Player = player;
            OnDie = onDie;

            Health = enemyData.Health;
        }
    }

    public class EnemyBase : CharacterBase
    {
        [SerializeField] private AnimationCurve _moveCurve;

        private EnemyContext _context = null;

        public override float HitRadius => 2f;

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
            var player = _context.Player;
            var moveStepDuration = _context.EnemyData.Move_Step_Duration;
            var moveStepSpeed = _context.EnemyData.Move_Step_Speed;
            var stopStepDuration = _context.EnemyData.Stop_Step_Duration;
            var attackRadius = _context.EnemyData.Attack_Radius;

            while (true)
            {
                var elapsed = 0f;
                var startPos = transform.position;
                var dir = (player.TF.position - startPos).normalized;

                var stepDist = moveStepSpeed * moveStepDuration;
                var targetPos = startPos + dir * stepDist;

                var totalDist = Vector3.Distance(startPos, player.TF.position);
                if (totalDist - attackRadius < stepDist)// 공격 범위 바깥에서 멈추도록 보정
                {
                    targetPos = player.TF.position - dir * attackRadius;
                }

                while (elapsed < moveStepDuration)
                {
                    elapsed += Time.deltaTime;
                    var factor = _moveCurve.Evaluate(elapsed / moveStepDuration);
                    transform.position = Vector3.Lerp(startPos, targetPos, factor);

                    if (Vector3.Distance(transform.position, player.TF.position) <= player.HitRadius)
                    {
                        Attack();
                        break;
                    }

                    yield return null;
                }

                yield return CoroutineUtil.WaitForSeconds(stopStepDuration);
            }
        }
        private void Attack()
        {
            if (null != _coAct)
                StopCoroutine(_coAct);

            var interval = _context.EnemyData.Attack_Interval;
            var player = _context.Player;
            if (player.Health <= 0)
                return;

            _coAct = StartCoroutine(Co_Attack());

            IEnumerator Co_Attack()
            {
                while (true)
                {
                    _animator.Play(HASH_ANI_ATTACK);

                    var damage = _context.EnemyData.Damage;
                    player.ReceiveAttack(damage);

                    yield return new WaitForSeconds(interval);
                }
            }
        }

        public override void Die()
        {
            _context.OnDie?.Invoke();
        }
    }
}