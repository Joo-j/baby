using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.Character;
using BabyNightmare.StaticData;

namespace BabyNightmare.Match
{
    public class MatchField : MonoBehaviour
    {
        [SerializeField] private Camera _renderCamera;
        [SerializeField] private Transform _playerTF;
        [SerializeField] private Transform _enemySpawnTF;

        private const string PATH_PLAYER = "Match/Player";
        private const string PATH_ENEMY = "Match/Enemy_";
        private readonly WaitForSeconds _spawnInterval = new WaitForSeconds(1f);

        private RenderTexture _rt = null;
        private Action _onClearWave = null;
        private Action _onFailWave = null;
        private Player _player = null;
        private List<EnemyBase> _aliveEnemies = new List<EnemyBase>();

        public RenderTexture RT => _rt;

        public void Init(Action onClearWave, Action onFailWave)
        {
            _rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            _renderCamera.targetTexture = _rt;

            _onClearWave = onClearWave;
            _onFailWave = onFailWave;

            _player = ObjectUtil.LoadAndInstantiate<Player>(PATH_PLAYER, _playerTF);

            var playerContext = new PlayerContext(PlayerData.Instance.Health, OnDiePlayer);
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

            StartCoroutine(Co_SpawnEnemy(enemyDataList));
        }

        private IEnumerator Co_SpawnEnemy(List<EnemyData> enemyDataList)
        {
            for (var i = 0; i < enemyDataList.Count; i++)
            {
                var data = enemyDataList[i];
                var res = Resources.Load<EnemyBase>($"{PATH_ENEMY}{data.Name}");
                var enemy = GameObject.Instantiate(res, _enemySpawnTF);
                var enemyContext = new EnemyContext(data, _playerTF, () => OnDieEnemy(enemy));

                enemy.Init(enemyContext);
                _aliveEnemies.Add(enemy);

                yield return _spawnInterval;
            }
        }

        private void OnDiePlayer()
        {
            _onFailWave?.Invoke();
        }

        private void OnDieEnemy(EnemyBase enemy)
        {
            _aliveEnemies.Remove(enemy);
            Destroy(enemy.gameObject);

            if (_aliveEnemies.Count == 0)
            {
                _onClearWave?.Invoke();
            }
        }

        public void AttackEnemy(EquipmentData equipmentData)
        {
            if (_aliveEnemies.Count == 0)
                return;

            var rand = UnityEngine.Random.Range(0, _aliveEnemies.Count);
            var randomEnemy = _aliveEnemies[rand];
            _player.Attack(equipmentData, randomEnemy);
        }
    }
}