using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BabyNightmare.InventorySystem;
using BabyNightmare.StaticData;
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

    public class Player : CharacterBase
    {
        private PlayerContext _context = null;
        private HashSet<EnemyBase> _detectedEnemies = null;

        public override void Init(ICharacterContext context)
        {
            base.Init(context);
            
            _context = context as PlayerContext;
            _detectedEnemies = new HashSet<EnemyBase>();
        }

        public override void Die()
        {
            _context.OnDiePlayer?.Invoke();
        }

        public void Attack(EquipmentData equipmentData, CharacterBase character)
        {
            _animator.Play(HASH_ANI_ATTACK);

            var res = Resources.Load<Projectile>($"{PATH_PROJECTILE}{equipmentData.Name}");
            var EquipmentObj = GameObject.Instantiate(res);
            EquipmentObj.Init();

            StartCoroutine(Co_ThrowObj(EquipmentObj.transform, character.TF,
            () =>
            {
                //fx
                Destroy(EquipmentObj.gameObject);
                var damage = equipmentData.Damage;
                character?.ReceiveAttack(damage);
            }));
        }

        private IEnumerator Co_ThrowObj(Transform objTF, Transform targetTF, Action doneCallback)
        {
            var startPos = objTF.position;
            var targetPos = targetTF.position;
            var midPos = Vector3.Lerp(startPos, targetPos, 0.5f);
            midPos.y *= 2;

            var elapsed = 0f;
            while (elapsed < THROW_EQUIPMENT_DRUATION)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = elapsed / THROW_EQUIPMENT_DRUATION;
                objTF.position = VectorUtil.CalcBezier(startPos, midPos, targetPos, factor);
            }

            objTF.position = targetPos;

            doneCallback?.Invoke();
        }


        private void OnTriggerEnter(Collider other)
        {
            if (false == other.TryGetComponent<EnemyBase>(out var enemy))
                return;

            _detectedEnemies.Add(enemy);
        }
    }
}