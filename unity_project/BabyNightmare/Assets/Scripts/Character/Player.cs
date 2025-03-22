using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Supercent.Util;
using BabyNightmare.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Match;
using BabyNightmare.HUD;
using BabyNightmare.Talent;
using Random = UnityEngine.Random;
using BabyNightmare.CustomShop;
using TMPro;

namespace BabyNightmare.Character
{
    public class PlayerContext
    {
        public float HP { get; }
        public Vector3 CameraForward { get; }
        public Action OnDiePlayer { get; }
        public Action<int, Vector3> GetCoin { get; }
        public Func<int, ProjectileData> GetProjectileData { get; }

        public PlayerContext(
        float hp,
        Vector3 cameraForward,
        Action onDiePlayer,
        Action<int, Vector3> getCoin,
        Func<int, ProjectileData> getProjectileData)
        {
            this.HP = hp;
            this.CameraForward = cameraForward;
            this.OnDiePlayer = onDiePlayer;
            this.GetCoin = getCoin;
            this.GetProjectileData = getProjectileData;
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
        [SerializeField] private PlayerModel _model;
        [SerializeField] private Transform _throwStartTF;
        [SerializeField] private float _hitRadius = 2f;
        [SerializeField] private Image _shieldImage;
        [SerializeField] private TextMeshProUGUI _shieldText;

        private PlayerContext _context = null;
        private Queue<EquipmentUseInfo> _useInfoQueue = null;
        private float _def = 0f;

        public override float HitRadius => _hitRadius;

        public void Init(PlayerContext context)
        {
            _context = context;

            _originEmissionColor = _mainRenderer.material.GetColor(KEY_EMISSION_COLOR);

            var hp = Mathf.Max(context.HP, 50);
            var talentHP = TalentManager.Instance.GetValue(ETalentType.Max_HP_Amount);
            hp += talentHP;

            _hp = _maxHealth = hp;

            _hpBar.transform.rotation = Quaternion.LookRotation(context.CameraForward);
            _hpBar.Refresh(_hp, _maxHealth, true);

            _shieldImage.gameObject.SetActive(false);
            _shieldText.gameObject.SetActive(false);

            var equipItemListData = CustomShopManager.Instance.GetEquipItemListData();
            for (var i = 0; i < equipItemListData.Count; i++)
                _model.RefreshCustomItem(equipItemListData[i]);

            _useInfoQueue = new Queue<EquipmentUseInfo>();

            _animationTrigger.AddAction(1, TryUseEquipment);
        }

        public void ReadyNextWave()
        {
            AddDef(-_def);
            _animator.Play(HASH_ANI_MOVE);
        }

        public void ReadyOpenBox()
        {
            _animator.SetTrigger(HASH_ANI_IDLE);
        }

        public override void Die()
        {
            if (true == _isDead)
                return;

            _context.OnDiePlayer?.Invoke();
            _isDead = true;
        }

        private void AddDef(float value)
        {
            _def += value;

            _shieldImage.gameObject.SetActive(_def > 0);
            _shieldText.gameObject.SetActive(_def > 0);
            StartCoroutine(SimpleLerp.Co_BounceScale(_shieldText.transform, Vector3.one * 1.1f, CurveHelper.Preset.EaseOut, 0.05f, () => _shieldText.text = $"{_def}"));
        }

        public override void ReceiveAttack(float damage, bool isCritical)
        {
            if (null == gameObject)
                return;

            var blocked = Mathf.Min(_def, damage);
            AddDef(-blocked);
            damage -= blocked;

            PopupTextPool.Instance.ShowTemporary(transform.position, Quaternion.Euler(_context.CameraForward), $"{damage}", Color.white);

            base.ReceiveAttack(damage, isCritical);
        }

        public void UseEquipment(EquipmentData equipmentData, ICharacter enemy)
        {
            _useInfoQueue.Enqueue(new EquipmentUseInfo(equipmentData, enemy));

            if (_useInfoQueue.Count > 1)
            {
                TryUseEquipment();
                return;
            }

            _animator.SetTrigger(HASH_ANI_ATTACK);
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

                            var talentDamage = TalentManager.Instance.GetValue(ETalentType.Damage_Percentage);
                            value += value * talentDamage;

                            var isCritical = false;
                            var criticalProb = TalentManager.Instance.GetValue(ETalentType.Critical_Prob_Percentage);
                            var rand = Random.value;
                            if (rand < criticalProb)
                            {
                                var criticalDamage = TalentManager.Instance.GetValue(ETalentType.Critical_Damage_Percentage);
                                value += value * criticalDamage;

                                isCritical = true;
                            }

                            enemy.ReserveDamage(value);

                            if (i == 0)
                                StartCoroutine(Co_ThrowProjectile(equipmentData, enemy.HitPoint, OnThrow));
                            else
                                OnThrow();

                            void OnThrow()
                            {
                                enemy?.ReceiveAttack(value, isCritical);
                            }
                            break;
                        }

                    case EStatType.HP:
                        {
                            if (i == 0)
                                StartCoroutine(Co_ThrowProjectile(equipmentData, transform, OnThrow));
                            else
                                OnThrow();

                            void OnThrow()
                            {
                                _hp = Mathf.Min(_maxHealth, _hp + value);
                                _hpBar.Refresh(_hp, _maxHealth, false);

                                var pos = transform.position;
                                pos.y = 0.01f;
                                FXPool.Instance.ShowTemporary(EFXType.Heal, pos);
                            }
                            break;
                        }

                    case EStatType.DEF:
                        {
                            if (i == 0)
                                StartCoroutine(Co_ThrowProjectile(equipmentData, transform, OnThrow));
                            else
                                OnThrow();

                            StartCoroutine(Co_ThrowProjectile(equipmentData, transform, OnThrow));

                            void OnThrow()
                            {
                                var talentDef = TalentManager.Instance.GetValue(ETalentType.Defense_Percentage);
                                value += value * talentDef;

                                AddDef(value);
                            }
                            break;
                        }
                    case EStatType.Coin:
                        {
                            if (i == 0)
                                StartCoroutine(Co_ThrowProjectile(equipmentData, transform, OnThrow));
                            else
                                OnThrow();

                            void OnThrow()
                            {
                                _context.GetCoin?.Invoke((int)value, transform.position);
                            }
                            break;
                        }
                    default: throw new Exception($"{statData.Type}의 행동이 정해지지 않았습니다.");
                }
            }
        }

        private IEnumerator Co_ThrowProjectile(EquipmentData data, Transform targetTF, Action doneCallback)
        {
            var ptData = _context.GetProjectileData(data.ID);
            var pt = ProjectilePool.Instance.Get();
            pt.TF.position = _throwStartTF.position;
            pt.TF.rotation = Quaternion.identity;
            pt.Init(ptData);

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