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
        public ChapterData ChapterData { get; }
        public Action<int, Vector3> GetCoin { get; }
        public Action<float, bool> RefreshProgress { get; }
        public Action OnClearWave { get; }
        public Action OnFailWave { get; }

        public MatchFieldContext
        (
            ChapterData chapterData,
            Action<int, Vector3> getCoin,
            Action<float, bool> refreshProgress,
            Action onClearWave,
            Action onFailWave
        )
        {
            this.ChapterData = chapterData;
            this.GetCoin = getCoin;
            this.RefreshProgress = refreshProgress;
            this.OnClearWave = onClearWave;
            this.OnFailWave = onFailWave;
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
        [SerializeField] private float _playerAttackRadius = 10f;
        [SerializeField] private float _areaAttackRadius = 3f;
        [SerializeField] private float _cameraMoveDuration = 0.01f;
        [SerializeField] private float _cameraShakeAmount = 0.2f;
        [SerializeField] private float _cameraShakeDuraton = 0.2f;

        private const string PATH_PLAYER = "Match/Character/Player";
        private const string PATH_ENEMY = "Match/Character/Enemy_";
        private const string PATH_EQUIPMENT_BOX = "Match/EquipmentBox/EquipmentBox_";
        private const string PATH_COIN = "Match/Coin";
        private const float COIN_DAMP = 0.5f;

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
        private int _waveCoin = 0;
        private Coroutine _coMoveCamera = null;
        private Coroutine _coShakeCamera = null;

        public RenderTexture RT => _rt;
        public Camera RenderCamera => _renderCamera;
        public Vector3 CameraForward => _renderCamera.transform.forward;

        public void Init(MatchFieldContext context)
        {
            _context = context;

            _fieldTF = ObjectUtil.Instantiate<Transform>(context.ChapterData.Field, transform);

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
                                    GetEnemiesInArea,
                                    ShakeCamera);
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
            _waveCoin = 0;

            _aliveEnemies.Clear();

            _enemySpawnCount = enemyDataList.Count;
            var delay = 0f;
            for (var i = 0; i < enemyDataList.Count; i++)
            {
                var data = enemyDataList[i];
                var spawnOrder = data.SpawnOrder;
                var spawnTF = GetSpawnTF(spawnOrder);

                var enemy = ObjectUtil.LoadAndInstantiate<EnemyBase>($"{PATH_ENEMY}{data.Type}", spawnTF);
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
            _waveCoin += dieCoin;

            _aliveEnemies.Remove(enemy);
            Destroy(enemy.GO);

            var progressFactor = 1 - ((float)_aliveEnemies.Count / _enemySpawnCount);
            _context.RefreshProgress?.Invoke(progressFactor, false);

            if (_aliveEnemies.Count == 0)
            {
                _context.RefreshProgress?.Invoke(0, true);
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
                if (dist > _playerAttackRadius)
                    continue;

                attackableEnemies.Add(enemy);
            }

            if (attackableEnemies.Count == 0)
                return;

            var rand = Random.Range(0, attackableEnemies.Count);
            var randomEnemy = attackableEnemies[rand];

            _player.UseEquipment(equipmentData, randomEnemy);
        }

        private List<EnemyBase> GetEnemiesInArea(Vector3 areaPos)
        {
            var enemies = new List<EnemyBase>();
            for (var i = 0; i < _aliveEnemies.Count; i++)
            {
                var enemy = _aliveEnemies[i];
                var playerPos = _player.TF.position;
                var enemyPos = enemy.TF.position;
                var dist = Vector3.Distance(playerPos, enemyPos);
                if (dist > _playerAttackRadius)
                    continue;

                dist = Vector3.Distance(areaPos, enemyPos);
                if (dist > _areaAttackRadius)
                    continue;

                enemies.Add(enemy);
            }

            return enemies;
        }

        public void EncounterBox(EBoxType boxType, Action doneCallback)
        {
            StartCoroutine(Co_EncounterBox());
            IEnumerator Co_EncounterBox()
            {
                var path = $"{PATH_EQUIPMENT_BOX}{boxType}";
                var box = ObjectUtil.LoadAndInstantiate<EquipmentBox>(path, _boxSpawnTF);
                box.TF.SetParent(_fieldTF);

                yield return CoroutineUtil.WaitForSeconds(1.5f);

                _player.ReadyNextWave();

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
                _player.ReadyOpenBox();

                box.Open(doneCallback);
            }
        }

        public int GetWaveCoin()
        {
            var totalCoin = Mathf.CeilToInt(_waveCoin * COIN_DAMP);
            var talentCoin = TalentManager.Instance.GetValue(ETalentType.Coin_Earn_Percentage);
            totalCoin += Mathf.CeilToInt(_waveCoin * talentCoin);

            return _waveCoin;
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

            if (null != _coMoveCamera)
                StopCoroutine(_coMoveCamera);

            _coMoveCamera = StartCoroutine(SimpleLerp.Co_LerpPosition(tf, startPos, targetPos, CurveHelper.Preset.EaseIn, _cameraMoveDuration, () => _coMoveCamera = null));
        }

        private void ShakeCamera()
        {
            if (null != _coShakeCamera)
                return;

            _coShakeCamera = StartCoroutine(Co_ShakeCamera());

            IEnumerator Co_ShakeCamera()
            {
                var originPos = _renderCamera.transform.position;

                float elapsed = 0f;
                while (elapsed < _cameraMoveDuration)
                {
                    yield return null;
                    elapsed += Time.deltaTime;

                    float x = Random.Range(-1f, 1f) * _cameraShakeAmount;
                    float y = Random.Range(-1f, 1f) * _cameraShakeAmount;

                    _renderCamera.transform.position += new Vector3(x, y, 0);
                }

                _renderCamera.transform.position = originPos;
                _coShakeCamera = null;
            }
        }
    }
}