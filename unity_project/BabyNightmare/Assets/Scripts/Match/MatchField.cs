using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.Character;
using BabyNightmare.StaticData;
using BabyNightmare.Util;

namespace BabyNightmare.Match
{
    public class MatchField : MonoBehaviour
    {
        [SerializeField] private Camera _renderCamera;
        [SerializeField] private Transform _playerTF;
        [SerializeField] private Transform _enemySpawnTF;

        private const string PATH_PLAYER = "Match/Player";
        private const string PATH_ENEMY = "Match/Enemy_";

        private RenderTexture _rt = null;
        private Action _onClearWave = null;
        private Action _onFailWave = null;
        private Player _player = null;
        private List<EnemyBase> _aliveEnemies = null;

        public RenderTexture RT => _rt;

        public void Init(Action onClearWave, Action onFailWave)
        {
            _aliveEnemies = new List<EnemyBase>();

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
                var enemy = ObjectUtil.LoadAndInstantiate<EnemyBase>($"{PATH_ENEMY}{data.Name}", _enemySpawnTF);
                var enemyContext = new EnemyContext(data, _player, () => OnDieEnemy(enemy));

                enemy.Init(enemyContext);
                enemy.TF.localPosition += Vector3.right * UnityEngine.Random.Range(-4f, 4f);
                _aliveEnemies.Add(enemy);

                var randomDelay = UnityEngine.Random.Range(1.5f, 3f);
                yield return CoroutineUtil.WaitForSeconds(randomDelay);
            }
        }

        private void OnDiePlayer()
        {
            _onFailWave?.Invoke();
        }

        private void OnDieEnemy(EnemyBase enemy)
        {
            _aliveEnemies.Remove(enemy);
            Destroy(enemy.GO);

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
            _player.UseEquipment(equipmentData, randomEnemy);
        }

        public void MovePlayer(Action doneCallback)
        {
            _player.Move(3f, doneCallback);
        }
    }
}