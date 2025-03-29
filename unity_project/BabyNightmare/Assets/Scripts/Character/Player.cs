using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Supercent.Util;
using Supercent.Core.Audio;
using BabyNightmare.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Match;
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
        public Func<Vector3, float, List<EnemyBase>> GetEnemiesInArea { get; }
        public Action ShakeCamera { get; }

        public PlayerContext(
        float hp,
        Vector3 cameraForward,
        Action onDiePlayer,
        Action<int, Vector3> getCoin,
        Func<Vector3, float, List<EnemyBase>> getEnemiesInArea,
        Action shakeCamera)
        {
            this.HP = hp;
            this.CameraForward = cameraForward;
            this.OnDiePlayer = onDiePlayer;
            this.GetCoin = getCoin;
            this.GetEnemiesInArea = getEnemiesInArea;
            this.ShakeCamera = shakeCamera;
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
        [SerializeField] private int _startHP = 200;
        [SerializeField] private Transform _throwStartTF;
        [SerializeField] private Image _shieldImage;
        [SerializeField] private TextMeshProUGUI _shieldText;

        private const string PATH_PROJECTILE_DATA = "StaticData/ProjectileData/ProjectileData_";
        private PlayerContext _context = null;
        private Queue<EquipmentUseInfo> _useInfoQueue = null;
        private float _def = 0f;

        public override bool IsAttackable
        {
            get
            {
                if (_isDead)
                    return false;
                if (_hp <= 0)
                    return false;
                if (_hp - _reserveDamage <= 0)
                    return false;

                return true;
            }
        }

        public void Init(PlayerContext context)
        {
            _context = context;

            _originEmissionColor = _mainRenderer.material.GetColor(KEY_EMISSION_COLOR);

            var hp = Mathf.Max(context.HP, _startHP);
            var talentHP = TalentManager.Instance.GetValue(ETalentType.Max_HP_Amount);
            hp += talentHP;

            _hp = _maxHealth = hp;

            var rotationLocker = _hpBar.GetComponent<RotationLocker>();
            rotationLocker.Init(context.CameraForward);
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
            transform.localRotation = Quaternion.identity;
            _animator.Play(HASH_ANI_MOVE);
        }

        public void ReadyOpenBox()
        {
            _animator.Play(HASH_ANI_IDLE);
        }

        public override void Die()
        {
            if (true == _isDead)
                return;

            _isDead = true;

            gameObject.SetActive(false);

            AudioManager.PlaySFX("AudioClip/Player_Die");
            HapticHelper.Haptic(Lofelt.NiceVibrations.HapticPatterns.PresetType.HeavyImpact);

            _context.OnDiePlayer?.Invoke();
        }

        private void AddDef(float value)
        {
            _def += value;
            _def = Mathf.RoundToInt(_def);

            _shieldImage.gameObject.SetActive(_def > 0);
            _shieldText.gameObject.SetActive(_def > 0);
            StartCoroutine(SimpleLerp.Co_BounceScale(_shieldText.transform, Vector3.one * 1.2f, CurveHelper.Preset.EaseOut, 0.1f, () => _shieldText.text = $"{_def}"));
        }

        public override void ReceiveAttack(float damage, bool isCritical)
        {
            if (false == IsAttackable)
                return;

            var blocked = Mathf.Min(_def, damage);
            AddDef(-blocked);
            damage -= blocked;

            if (blocked > 0)
            {
                FXPool.Instance.ShowTemporary(EFXType.Defense, transform.position);
                AudioManager.PlaySFX("AudioClip/Player_Defense");
            }

            var message = damage <= 0 ? "Block" : $"{Mathf.RoundToInt(damage)}";

            PopupTextPool.Instance.ShowTemporary(
                                                EPopupTextType.Damage,
                                                 transform.position,
                                                Quaternion.Euler(_context.CameraForward),
                                                Vector3.one,
                                                message
                                                );

            base.ReceiveAttack(damage, isCritical);

            if (Random.value > 0.5)
                AudioManager.PlaySFX("AudioClip/Player_ReceiveAttack_1");
            else
                AudioManager.PlaySFX("AudioClip/Player_ReceiveAttack_2");
        }

        public void UseEquipment(EquipmentData equipmentData, ICharacter enemy)
        {
            _useInfoQueue.Enqueue(new EquipmentUseInfo(equipmentData, enemy));

            if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == HASH_ANI_ATTACK)
            {
                TryUseEquipment();
            }

            if (_useInfoQueue.Count > 0)
            {
                var enemyPos = enemy.TF.position;
                enemyPos.y = transform.position.y;
                transform.LookAt(enemyPos);
                _animator.Play(HASH_ANI_ATTACK);
                AudioManager.PlaySFX("AudioClip/Player_Throw");
            }
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

                            if (equipmentData.DamageType == EDamageType.Direct)
                                enemy.ReserveDamage(value);

                            switch (equipmentData.Type)
                            {
                                case EEquipmentType.Horse:
                                    AudioManager.PlaySFX("AudioClip/Projectile_Horse");
                                    break;
                            }

                            if (i == 0)
                                StartCoroutine(Co_ThrowProjectile(equipmentData, enemy.HitPoint, OnThrow));
                            else
                                OnThrow();

                            void OnThrow()
                            {
                                var pos = enemy.HitPoint.position;

                                switch (equipmentData.DamageType)
                                {
                                    case EDamageType.Direct:
                                        enemy?.ReceiveAttack(value, isCritical);
                                        break;
                                    case EDamageType.Area:
                                        pos.y = 0;
                                        var enemies = _context.GetEnemiesInArea?.Invoke(pos, equipmentData.Radius);
                                        for (var j = 0; j < enemies.Count; j++)
                                        {
                                            enemies[j]?.ReceiveAttack(value, isCritical);
                                        }
                                        _context.ShakeCamera?.Invoke();
                                        break;
                                }

                                switch (equipmentData.Type)
                                {
                                    case EEquipmentType.Bomb:
                                        FXPool.Instance.ShowTemporary(EFXType.Projectile_Bomb, pos);
                                        AudioManager.PlaySFX("AudioClip/Projectile_Bomb");
                                        break;
                                    case EEquipmentType.Missile:
                                        FXPool.Instance.ShowTemporary(EFXType.Projectile_Missle, pos);
                                        AudioManager.PlaySFX("AudioClip/Projectile_Missile");
                                        break;
                                    case EEquipmentType.WaterGun:
                                        FXPool.Instance.ShowTemporary(EFXType.Projectile_WaterGun, enemy.HitPoint.position);
                                        AudioManager.PlaySFX("AudioClip/Projectile_WaterGun");
                                        break;
                                }
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
                                PopupTextPool.Instance.ShowTemporary(
                                                                    EPopupTextType.Heal,
                                                                    _hitPoint.position + Vector3.up * 3,
                                                                    Quaternion.Euler(_context.CameraForward),
                                                                    Vector3.one,
                                                                    $"+{value}"
                                                                    );
                                AudioManager.PlaySFX("AudioClip/Player_Heal");
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
                                AudioManager.PlaySFX("AudioClip/Player_Defense");
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
                                AudioManager.PlaySFX("AudioClip/Coin");
                            }
                            break;
                        }
                    default: throw new Exception($"{statData.Type}의 행동이 정해지지 않았습니다.");
                }
            }
        }

        private IEnumerator Co_ThrowProjectile(EquipmentData data, Transform targetTF, Action doneCallback)
        {
            var ptPath = $"{PATH_PROJECTILE_DATA}{data.ID}";
            var ptData = Resources.Load<ProjectileData>(ptPath);

            var pt = ProjectilePool.Instance.Get();
            pt.TF.position = _throwStartTF.position;
            pt.TF.rotation = Quaternion.identity;
            pt.Init(ptData, data.Level);

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

            if (null == targetTF)
            {
                ProjectilePool.Instance.Return(pt);
                yield break;
            }

            pt.TF.position = targetTF.position;
            ProjectilePool.Instance.Return(pt);

            doneCallback?.Invoke();
        }
    }
}