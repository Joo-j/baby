using System;
using System.Collections;
using UnityEngine;
using BabyNightmare.Util;
using BabyNightmare.StaticData;
using Supercent.Util;
using BabyNightmare.HUD;
using BabyNightmare.Match;
using Random = UnityEngine.Random;

namespace BabyNightmare.Character
{
    public class EnemyContext
    {
        public EnemyData EnemyData { get; }
        public ICharacter Player { get; }
        public Action OnDie { get; }

        public float HP { get; }
        public Vector3 CameraForward { get; }
        public float Delay { get; }

        public EnemyContext
        (
            EnemyData enemyData,
            ICharacter player,
            Action onDie,
            Vector3 cameraForward,
            float delay
        )
        {
            EnemyData = enemyData;
            Player = player;
            OnDie = onDie;

            this.HP = enemyData.Health;
            this.CameraForward = cameraForward;
            this.Delay = delay;
        }
    }

    public class EnemyBase : CharacterBase
    {
        [SerializeField] private AnimationCurve _moveCurve;

        private EnemyContext _context = null;

        public override float HitRadius => 2f;

        public void Init(EnemyContext context)
        {
            _context = context;

            _originEmissionColor = _mainRenderer.material.GetColor(KEY_EMISSION_COLOR);

            _hp = _maxHealth = context.HP;

            _hpBar.transform.rotation = Quaternion.LookRotation(context.CameraForward);
            _hpBar.Refresh(_hp, _maxHealth, true);

            for (var i = 0; i < _allRenderers.Length; i++)
                _allRenderers[i].enabled = false;

            _hpBar.gameObject.SetActive(false);

            StartCoroutine(SimpleLerp.Co_Invoke(_context.Delay, () =>
            {
                for (var i = 0; i < _allRenderers.Length; i++)
                    _allRenderers[i].enabled = true;

                _hpBar.gameObject.SetActive(true);
                StartMove();
            }));
        }

        private void StartMove()
        {
            _animator.Play(HASH_ANI_MOVE);

            if (null != _coAct)
                StopCoroutine(_coAct);

            _coAct = StartCoroutine(Co_IntervalMove());
        }

        private IEnumerator Co_IntervalMove()
        {
            var player = _context.Player;
            var targetPos = player.TF.position;
            targetPos.z = transform.position.z;
            var moveStepDuration = _context.EnemyData.Move_Step_Duration;
            var moveStepSpeed = _context.EnemyData.Move_Step_Speed;
            var stopStepDuration = _context.EnemyData.Stop_Step_Duration;
            var attackRadius = _context.EnemyData.Attack_Radius;

            while (true)
            {
                var elapsed = 0f;
                var startPos_interval = transform.position;
                var dir = (targetPos - startPos_interval).normalized;

                var stepDist = moveStepSpeed * moveStepDuration;
                var targetPos_interval = startPos_interval + dir * stepDist;

                var totalDist = Vector3.Distance(startPos_interval, targetPos);
                if (totalDist - attackRadius < stepDist)// 공격 범위 바깥에서 멈추도록 보정
                {
                    targetPos_interval = targetPos - dir * attackRadius;
                }

                while (elapsed < moveStepDuration)
                {
                    elapsed += Time.deltaTime;
                    var factor = _moveCurve.Evaluate(elapsed / moveStepDuration);
                    transform.position = Vector3.Lerp(startPos_interval, targetPos_interval, factor);

                    if (Vector3.Distance(transform.position, targetPos) <= player.HitRadius)
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
            if (player.HP <= 0)
                return;

            _coAct = StartCoroutine(Co_Attack());

            IEnumerator Co_Attack()
            {
                while (true)
                {
                    _animationTrigger.Clear();
                    _animationTrigger.AddAction(1, () => player.ReceiveAttack(_context.EnemyData.Damage, false));

                    _animator.SetTrigger(HASH_ANI_ATTACK);

                    yield return new WaitForSeconds(interval);
                }
            }
        }

        public override void ReceiveAttack(float damage, bool isCritical)
        {
            if (null == gameObject)
                return;

            PopupTextPool.Instance.ShowTemporary(transform.position, Quaternion.Euler(_context.CameraForward), $"{damage}", Color.white);

            base.ReceiveAttack(damage, isCritical);
        }

        public override void Die()
        {
            if (true == _isDead)
                return;

            _context.OnDie?.Invoke();
            _isDead = true;
        }

        public int GetRandomCoin()
        {
            return UnityEngine.Random.Range(_context.EnemyData.Coin_Min, _context.EnemyData.Coin_Max);
        }
    }
}