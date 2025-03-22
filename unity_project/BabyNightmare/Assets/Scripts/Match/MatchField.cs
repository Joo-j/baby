using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using BabyNightmare.Character;
using BabyNightmare.StaticData;
using Random = UnityEngine.Random;
using BabyNightmare.Util;
using BabyNightmare.Talent;

namespace BabyNightmare.Match
{
    public class MatchFieldContext
    {
        public int Chapter { get; }
        public Action<int, Vector3> GetCoin { get; }
        public Action<float> RefreshProgress { get; }
        public Action OnClearWave { get; }
        public Action OnFailWave { get; }
        public Func<int, ProjectileData> GetProjectileData { get; }

        public MatchFieldContext
        (
            int chapter,
            Action<int, Vector3> getCoin,
            Action<float> refreshProgress,
            Action onClearWave,
            Action onFailWave,
            Func<int, ProjectileData> getProjectileData
        )
        {
            this.Chapter = chapter;
            this.GetCoin = getCoin;
            this.RefreshProgress = refreshProgress;
            this.OnClearWave = onClearWave;
            this.OnFailWave = onFailWave;
            this.GetProjectileData = getProjectileData;
        }
    }

    public enum ECameraPosType
    {
        Low,
        Mid,
        High
    }

    public class MatchField : MonoBehaviour
    {
        [SerializeField] private Camera _renderCamera;
        [SerializeField] private Transform _playerTF;
        [SerializeField] private Transform _nearSpawnTF;
        [SerializeField] private Transform _midSpawnTF;
        [SerializeField] private Transform _farSpawnTF;
        [SerializeField] private Transform _boxSpawnTF;
        [SerializeField] private float _groundMoveAmount = 5f;
        [SerializeField] private float _attackRadius = 10f;
        [SerializeField] private float _cameraMoveDuration = 0.01f;

        private const string PATH_FIELD = "Match/Field/Field_";
        private const string PATH_PLAYER = "Match/Character/Player";
        private const string PATH_ENEMY = "Match/Character/Enemy_";
        private const string PATH_EQUIPMENT_BOX = "Match/EquipmentBox/EquipmentBox_";
        private const string PATH_COIN = "Match/Coin";

        private Transform _fieldTF;
        private RenderTexture _rt = null;
        private Transform[] _nearSpawnTFArr = null;
        private Transform[] _midSpawnTFArr = null;
        private Transform[] _farSpawnTFArr = null;
        private Player _player = null;
        private List<EnemyBase> _aliveEnemies = null;
        private MatchFieldContext _context = null;
        private int _enemySpawnCount = 0;
        private Pool<Coin> _coinPool = null;
        private int _totalCoin = 0;
        private Coroutine _coMoveCamera = null;

        public RenderTexture RT => _rt;
        public Camera RenderCamera => _renderCamera;
        public Vector3 CameraForward => _renderCamera.transform.forward;

        public void Init(MatchFieldContext context)
        {
            _context = context;

            _fieldTF = ObjectUtil.LoadAndInstantiate<Transform>($"{PATH_FIELD}{_context.Chapter}", transform);

            _nearSpawnTFArr = _nearSpawnTF.GetComponentsInChildren<Transform>();
            _midSpawnTFArr = _midSpawnTF.GetComponentsInChildren<Transform>();
            _farSpawnTFArr = _farSpawnTF.GetComponentsInChildren<Transform>();
            _aliveEnemies = new List<EnemyBase>();

            _rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            _renderCamera.targetTexture = _rt;

            _player = ObjectUtil.LoadAndInstantiate<Player>(PATH_PLAYER, _playerTF);

            var playerContext = new PlayerContext(
                                    PlayerData.Instance.HP,
                                    CameraForward,
                                    OnDiePlayer,
                                    _context.GetCoin,
                                    _context.GetProjectileData);
            _player.Init(playerContext);

            _coinPool = new Pool<Coin>(() => ObjectUtil.LoadAndInstantiate<Coin>(PATH_COIN, transform), 10);
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

            _enemySpawnCount = enemyDataList.Count;
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
                delay += UnityEngine.Random.Range(data.Spawn_Delay_Min, data.Spawn_Delay_Max);
            }
        }

        private void OnDiePlayer()
        {
            _context.OnFailWave?.Invoke();
        }

        private void OnDieEnemy(EnemyBase enemy)
        {
            var pos = enemy.transform.position;

            var dieCoin = enemy.GetRandomCoin();
            StartCoroutine(Co_SpawnCoin(pos, dieCoin));
            _totalCoin += dieCoin;

            _aliveEnemies.Remove(enemy);
            Destroy(enemy.GO);

            var progressFactor = 1 - ((float)_aliveEnemies.Count / _enemySpawnCount);
            _context.RefreshProgress?.Invoke(progressFactor);

            if (_aliveEnemies.Count == 0)
            {
                _context.OnClearWave?.Invoke();
            }
        }


        private IEnumerator Co_SpawnCoin(Vector3 pos, int count)
        {
            var randDir = new Vector3(Random.Range(0f, 0.5f), 1f, Random.Range(0f, 0.5f));
            var randForce = Random.Range(30f, 60f);
            for (var i = 0; i < count; i++)
            {
                var coin = _coinPool.Get();
                coin.transform.position = pos;
                coin.Init(randDir, randForce, () => _coinPool.Return(coin));
                yield return null;
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

            var rand = Random.Range(0, attackableEnemies.Count);
            var randomEnemy = attackableEnemies[rand];

            _player.UseEquipment(equipmentData, randomEnemy);
        }

        public void OnClearWave(Transform coinGenTF)
        {
            var talentCoin = TalentManager.Instance.GetValue(ETalentType.Coin_Earn_Percentage);
            _totalCoin += Mathf.CeilToInt(_totalCoin * talentCoin);

            _context.GetCoin?.Invoke(_totalCoin, coinGenTF.position);
            _totalCoin = 0;
        }

        public void EncounterBox(EquipmentBoxData boxData, Action doneCallback)
        {
            StartCoroutine(Co_EncounterBox());
            IEnumerator Co_EncounterBox()
            {
                var path = $"{PATH_EQUIPMENT_BOX}{boxData.Type}";
                var box = ObjectUtil.LoadAndInstantiate<EquipmentBox>(path, _boxSpawnTF);
                box.TF.SetParent(_fieldTF);

                yield return CoroutineUtil.WaitForSeconds(1.5f);

                _player.ShowMoveAni();

                var startPos = _fieldTF.localPosition;
                var targetPos = startPos + Vector3.left * _groundMoveAmount;
                var moveDuration = 3f;
                var elapsed = 0f;
                while (elapsed < moveDuration)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                    var factor = elapsed / moveDuration;
                    _fieldTF.localPosition = Vector3.Lerp(startPos, targetPos, factor);
                }

                _fieldTF.localPosition = targetPos;
                _player.ShowIdleAni();

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

        public void MoveCamera(ECameraPosType type)
        {
            var tf = _renderCamera.transform;
            var startPos = tf.position;
            var targetPos = tf.position;

            switch (type)
            {
                case ECameraPosType.Low:
                    targetPos = new Vector3(0, 7f, -10);
                    break;
                case ECameraPosType.Mid:
                    targetPos = new Vector3(0, 9, -10);
                    break;
                case ECameraPosType.High:
                    targetPos = new Vector3(0, 11f, -10);
                    break;
            }

            if (startPos == targetPos)
                return;

            tf.position = targetPos;

            // if (null != _coMoveCamera)
            //     StopCoroutine(_coMoveCamera);

            // _coMoveCamera = StartCoroutine(SimpleLerp.Co_LerpPosition(tf, startPos, targetPos, CurveHelper.Preset.EaseIn, _cameraMoveDuration, () => _coMoveCamera = null));
        }
    }
}