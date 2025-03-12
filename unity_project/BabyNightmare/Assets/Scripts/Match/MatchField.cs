using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.Character;
using BabyNightmare.StaticData;
using Random = UnityEngine.Random;

namespace BabyNightmare.Match
{
    public class MatchField : MonoBehaviour
    {
        [SerializeField] private Camera _renderCamera;
        [SerializeField] private Transform _playerTF;
        [SerializeField] private Transform _nearSpawnTF;
        [SerializeField] private Transform _midSpawnTF;
        [SerializeField] private Transform _farSpawnTF;
        [SerializeField] private Transform _boxTF;
        [SerializeField] private float _attackRadius = 10f;

        private const string PATH_PLAYER = "Match/Player";
        private const string PATH_ENEMY = "Match/Enemy_";
        private const string PATH_EQUIPMENT_BOX = "Match/EquipmentBox_";

        private RenderTexture _rt = null;
        private Transform[] _nearSpawnTFArr = null;
        private Transform[] _midSpawnTFArr = null;
        private Transform[] _farSpawnTFArr = null;
        private Action<int, Vector3> _getCoin = null;
        private Action _onClearWave = null;
        private Action _onFailWave = null;
        private Player _player = null;
        private List<EnemyBase> _aliveEnemies = null;

        public RenderTexture RT => _rt;
        public Camera RenderCamera => _renderCamera;
        public Vector3 CameraForward => _renderCamera.transform.forward;

        public void Init(Action<int, Vector3> getCoin, Action onClearWave, Action onFailWave)
        {
            _nearSpawnTFArr = _nearSpawnTF.GetComponentsInChildren<Transform>();
            _midSpawnTFArr = _midSpawnTF.GetComponentsInChildren<Transform>();
            _farSpawnTFArr = _farSpawnTF.GetComponentsInChildren<Transform>();
            _aliveEnemies = new List<EnemyBase>();

            _rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            _renderCamera.targetTexture = _rt;

            _getCoin = getCoin;
            _onClearWave = onClearWave;
            _onFailWave = onFailWave;

            _player = ObjectUtil.LoadAndInstantiate<Player>(PATH_PLAYER, _playerTF);

            var playerContext = new PlayerContext(PlayerData.Instance.HP, CameraForward, OnDiePlayer);
            _player.Init(playerContext);
        }

        public void Release()
        {
            if (null != _rt)
            {
                _renderCamera.targetTexture = null;
                _rt.Release();
            }

            _rt = null;
        }

        public void StartWave(List<EnemyData> enemyDataList)
        {
            _aliveEnemies.Clear();

            var delay = 0f;
            for (var i = 0; i < enemyDataList.Count; i++)
            {
                var data = enemyDataList[i];
                var spawnOrder = data.SpawnOrder;
                var spawnTF = GetSpawnTF(spawnOrder);

                var enemy = ObjectUtil.LoadAndInstantiate<EnemyBase>($"{PATH_ENEMY}{data.Name}", spawnTF);
                var enemyContext = new EnemyContext(
                                data,
                                _player,
                                () => OnDieEnemy(enemy),
                                CameraForward,
                                delay);

                enemy.Init(enemyContext);
                _aliveEnemies.Add(enemy);
                delay = UnityEngine.Random.Range(1.5f, 4f);
            }
        }

        private void OnDiePlayer()
        {
            _onFailWave?.Invoke();
        }

        private void OnDieEnemy(EnemyBase enemy)
        {
            _getCoin?.Invoke(enemy.GetRandomCoin(), enemy.transform.position);
            _aliveEnemies.Remove(enemy);
            Destroy(enemy.GO);

            if (_aliveEnemies.Count == 0)
            {
                _onClearWave?.Invoke();
            }
        }

        public void AttackEnemy(EquipmentData equipmentData)
        {
            var attackableEnemies = new List<EnemyBase>();

            for (var i = 0; i < _aliveEnemies.Count; i++)
            {
                var enemy = _aliveEnemies[i];
                if (enemy.HP <= 0)
                    continue;

                var dist = Vector3.Distance(_player.TF.position, enemy.TF.position);
                if (dist > _attackRadius)
                    continue;

                attackableEnemies.Add(enemy);
            }

            if (attackableEnemies.Count == 0)
                return;

            var rand = UnityEngine.Random.Range(0, attackableEnemies.Count);
            var randomEnemy = attackableEnemies[rand];

            _player.UseEquipment(equipmentData, randomEnemy);
        }

        public void EncounterBox(EquipmentBoxData boxData, Action doneCallback)
        {
            StartCoroutine(Co_EncounterBox());
            IEnumerator Co_EncounterBox()
            {
                yield return CoroutineUtil.WaitForSeconds(1.5f);

                var waiter = new CoroutineWaiter();
                _player.Move(3f, waiter.Signal);
                yield return waiter.Wait();

                var path = $"{PATH_EQUIPMENT_BOX}{boxData.Type}";
                var box = ObjectUtil.LoadAndInstantiate<EquipmentBox>(path, _boxTF);
                box.Open(doneCallback);
            }
        }
        private Transform GetSpawnTF(ESpawnOrder order)
        {
            switch (order)
            {
                case ESpawnOrder.Near: return _nearSpawnTFArr[Random.Range(0, _nearSpawnTFArr.Length)];
                case ESpawnOrder.Middle: return _midSpawnTFArr[Random.Range(0, _midSpawnTFArr.Length)];
                case ESpawnOrder.Far: return _farSpawnTFArr[Random.Range(0, _farSpawnTFArr.Length)];
                default:
                    var typeCount = Enum.GetValues(typeof(ESpawnOrder)).Length;
                    return GetSpawnTF((ESpawnOrder)(Random.Range(0, typeCount)));
            }
        }
    }
}