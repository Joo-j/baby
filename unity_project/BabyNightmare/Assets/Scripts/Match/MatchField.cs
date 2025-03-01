using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BabyNightmare.Character;
using BabyNightmare.StaticData;
using System;

namespace BabyNightmare.Match
{
    public class MatchField : MonoBehaviour
    {
        [SerializeField] private Transform _playerTF;
        [SerializeField] private Transform _enemySpawnTF;

        private const string PATH_PLAYER = "Match/Player";
        private const string PATH_ENEMY = "Match/Enemy_";
        private readonly WaitForSeconds _spawnInterval = new WaitForSeconds(1f);

        private Action _onClearWave = null;
        private Action _onFailWave = null;
        private Player _player = null;
        private List<EnemyBase> _aliveEnemies = null;

        public void Init(Action onClearWave, Action onFailWave)
        {
            _onClearWave = onClearWave;
            _onFailWave = onFailWave;

            var res = Resources.Load<Player>(PATH_PLAYER);
            _player = GameObject.Instantiate(res, _playerTF);

            var playerContext = new PlayerContext(PlayerData.Health, OnDiePlayer);
            _player.Init(playerContext);
        }

        public void StartWave(List<EnemyData> enemyDataList)
        {
            _aliveEnemies = new List<EnemyBase>();

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