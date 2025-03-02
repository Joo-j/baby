using System;
using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Supercent.UI
{
    public class CurrencyEffector : MonoBehaviour
    {
        struct SpreadInfo
        {
            public CurrencyParticle Partial;
            public Vector2          SpreadEndPoint;
            public float            SpraedTimeLength;
            public float            AbsorbDist;
            public Vector2          AbosrbEndPoint;
        }

        public delegate void OnPlayAbsorb(float minAbsorbTime, float maxAbsorbTime);



        [Header("Partial Settings")]
        [SerializeField] bool                   _autoInitOnStart        = false;    // Start 호출 시 자동으로 Init 함수를 호출 할 것인지 여부
        [SerializeField] int                    _particleCount          = 100;      // 입자 최초 생성 개수
        [SerializeField] bool                   _autoGenerate           = true;     // 입자가 부족할 경우 자동 생성 여부
        [SerializeField] int                    _autoGenerateCount      = 50;       // 자동 생성 시 입자 생성 개수
        [SerializeField] int                    _autoGenerateMaxCount   = 300;      // 자동 생성 시 입자 생성 한계값
        [SerializeField] CurrencyParticle       _particleOrigin         = null;
        [SerializeField] List<CurrencyParticle> _particleList           = null;

        [Header("Spread Settings")]
        [SerializeField] float          _spread_MinDistance     = 30.0f;    // 입자가 퍼지는 최소 거리
        [SerializeField] float          _spread_MaxDistance     = 130.0f;   // 입자가 퍼지는 최대 거리
        [SerializeField] float          _spread_MinTimeLength   = 0.2f;     // 입자가 최소 거리까지 퍼지는 시간
        [SerializeField] float          _spread_MaxTimeLength   = 0.45f;    // 입자가 최대 거리까지 퍼지는 시간
        [SerializeField] float          _spread_StartScale      = 0.7f;     // 입자 퍼짐 연출 시작 시 크기
        [SerializeField] float          _spread_EndScale        = 0.7f;     // 입자 퍼짐 연출 종료 시 크기
        [SerializeField] float          _spread_FinishWaitTime  = 0.0f;     // 입자 퍼짐 연출 완료 후 대기 시간
        [SerializeField] AnimationCurve _spread_MoveCurve       = null;
        [SerializeField] AnimationCurve _spread_ScaleCurve      = null;
        [SerializeField] RectTransform  _spread_CenterPoint     = null;

        [Header("Absorb Settings")]
        [SerializeField] float          _absorb_MinTimeLength   = 0.3f;     // 입자 흡수 시 최소 거리에서 흡수되는 시간
        [SerializeField] float          _absorb_MaxTimeLength   = 0.7f;     // 입자 흡수 시 최대 거리에서 흡수되는 시간
        [SerializeField] float          _absorb_EndScale        = 1.0f;     // 입자 흡수 연출 종료 시 입자의 크기
        [SerializeField] AnimationCurve _absorb_MoveCurve       = null;
        [SerializeField] AnimationCurve _absorb_ScaleCurve      = null;
        [SerializeField] RectTransform  _absorb_CenterPoint     = null;

        int _lastUsedPartialIndex = 0;

        RectTransform   _rtf                = null;
        RectTransform   _calcSupportRTF     = null;
        UnityEvent      _onAbsorbPartial    = new UnityEvent();



        public UnityEvent OnAbsorbParticle => _onAbsorbPartial;



        void Start()
        {
            if (!_autoInitOnStart)
                return;

            Init();
        }
        public void Init()
        {
            GenerateCalcSupportRTF();

            if (null == _particleOrigin)
                return;

            if (null != _particleList)  _particleList = _particleList.FindAll(fx => null != fx);
            else                        _particleList = new List<CurrencyParticle>(_particleCount);

            var generateCount = Mathf.Max(0, _particleCount - _particleList.Count);
            if (generateCount <= 0)
                return;

            GeneratePartial(generateCount);
        }
        void GenerateCalcSupportRTF()
        {
            var go          = new GameObject("CalcSupport");
            go.hideFlags    = HideFlags.HideInHierarchy;
            _calcSupportRTF = go.AddComponent<RectTransform>();

            _calcSupportRTF.SetParent(transform);

            _calcSupportRTF.pivot           = 
            _calcSupportRTF.anchorMin       =
            _calcSupportRTF.anchorMax       = Vector2.one * 0.5f;
            _calcSupportRTF.localRotation   = Quaternion.identity;
            _calcSupportRTF.localScale      = Vector3.one;

            go.SetActive(false);
        }
        void GeneratePartial(int count)
        {
            while (0 < count)
            {
                var inst = Instantiate(_particleOrigin, transform);
                inst.Init();

                _particleList.Add(inst);

                --count;
            }
        }



        public void HideAllParticle()
        {
            StopAllCoroutines();

            for (int n = 0, cnt = _particleList.Count; n < cnt; ++n)
            {
                var partial = _particleList[n];
                if (null == partial)
                {
                    Debug.LogWarning($"[CurrencyEffector - HideAllParticle] {name}'s ParticleList[{n}] is null !");
                    continue;
                }

                partial.Init();
            }
        }



        /// <summary>
        /// 동일한 캔버스 위에 존재하는 UI의 월드 좌표를 기준으로 연출이 시작되도록 지정
        /// </summary>
        public void SetSpreadPoint_FromUI(Vector3 worldPosition)
        {
            if (null == _spread_CenterPoint)
                return;

            _spread_CenterPoint.position            = worldPosition;
            var anchoredPosition                    = _spread_CenterPoint.anchoredPosition3D;
            anchoredPosition.z                      = 0.0f;
            _spread_CenterPoint.anchoredPosition3D  = anchoredPosition;
        }

        /// <summary>
        /// 동일한 캔버스 위에 존재하는 UI의 월드 좌표를 기준으로 연출이 종료되도록 지정
        /// </summary>
        public void SetAbsorbPoint_FromUI(Vector3 worldPosition)
        {
            if (null == _absorb_CenterPoint)
                return;

            _absorb_CenterPoint.position            = worldPosition;
            var anchoredPosition                    = _absorb_CenterPoint.anchoredPosition3D;
            anchoredPosition.z                      = 0.0f;
            _absorb_CenterPoint.anchoredPosition3D  = anchoredPosition;
        }



        public void Play(int count, OnPlayAbsorb onPlayAbsorb, Transform spreadPoint = null)                                        => Play(null, Vector2.zero, count, onPlayAbsorb, spreadPoint);   
        public void Play(int count, OnPlayAbsorb onPlayAbsorb, Action onFinishAbsorb, Transform spreadPoint = null)                 => Play(null, Vector2.zero, count, onPlayAbsorb, onFinishAbsorb, spreadPoint);   
        public void Play(Sprite iconSprite, Vector2 iconSize, int count, OnPlayAbsorb onPlayAbsorb, Transform spreadPoint = null)   => Play(iconSprite, iconSize, count, onPlayAbsorb, null, spreadPoint);
        public void Play(Sprite iconSprite, Vector2 iconSize, int count, OnPlayAbsorb onPlayAbsorb, Action onFinishAbsorb, Transform spreadPoint = null)
        {
            if (count <= 0)
                return;

            if (null != spreadPoint)
                SetSpreadPoint_FromUI(spreadPoint.position);

            StartCoroutine(CoroutinePlay(iconSprite, iconSize, count, onPlayAbsorb, onFinishAbsorb));
        }
        IEnumerator CoroutinePlay(Sprite iconSprite, Vector2 iconSize, int count, OnPlayAbsorb onPlayAbsorb, Action onFinishAbsorb)
        {
            ResetRTF();
            Calc_PlayInfos(iconSprite, iconSize, count, out var spreadInfos, 
                                                        out var maxSpreadTime, 
                                                        out var minAbsorbDist, 
                                                        out var maxAbsorbDist);

            for (int n = 0, cnt = spreadInfos.Count; n < cnt; ++n)
                StartCoroutine(CoroutineSpread(spreadInfos[n]));

            if (null == spreadInfos || spreadInfos.Count <= 0)
            {
                onPlayAbsorb?.Invoke(0.0f, 0.0f);
                onFinishAbsorb?.Invoke();
                yield break;
            }

            var secDone = Time.timeAsDouble + maxSpreadTime;
            while (Time.timeAsDouble < secDone)
                yield return null;

            if (0.0f < _spread_FinishWaitTime)
                yield return Supercent.Util.CoroutineUtil.WaitForSeconds(_spread_FinishWaitTime);

            var minAbsorbTime = float.MaxValue;
            var maxAbsorbTime = float.MinValue;
            for (int n = 0, cnt = spreadInfos.Count; n < cnt; ++n)
            {
                var info    = spreadInfos[n];
                var partial = info.Partial;
                var absorbTime = MathUtil.Lerp_Percent_Between_A_and_B(minAbsorbDist, maxAbsorbDist,
                                                                       info.AbsorbDist,
                                                                       _absorb_MinTimeLength, _absorb_MaxTimeLength);

                if (absorbTime < minAbsorbTime) minAbsorbTime = absorbTime;
                if (maxAbsorbTime < absorbTime) maxAbsorbTime = absorbTime;

                StartCoroutine(CoroutineAbsorb(partial, info.AbosrbEndPoint, absorbTime));
            }

            if (Mathf.Approximately(_absorb_MinTimeLength, float.MaxValue)) minAbsorbTime = _absorb_MinTimeLength;
            if (Mathf.Approximately(_absorb_MaxTimeLength, float.MinValue)) maxAbsorbTime = _absorb_MaxTimeLength;

            onPlayAbsorb?.Invoke(minAbsorbTime, maxAbsorbTime);

            if (0.0f < _absorb_MaxTimeLength)
                yield return Supercent.Util.CoroutineUtil.WaitForSeconds(_absorb_MaxTimeLength);

            onFinishAbsorb?.Invoke();
        }



        void Calc_PlayInfos(Sprite iconSprite, Vector2 iconSize, int count, out List<SpreadInfo> finalSpreadInfos, out float finalMaxSpreadTime, out float finalMinAbsorbDist, out float finalMaxAbsorbDist)
        {
            var maxSpreadTime       = float.MinValue;
            var minAbsorbDist       = float.MaxValue;
            var maxAbsorbDist       = float.MinValue;
            var spreadInfos         = new List<SpreadInfo>(count);
            var centerPoint         = Calc_WorldToAnchoredPos(_spread_CenterPoint);
            var absorbPoint         = Calc_WorldToAnchoredPos(_absorb_CenterPoint);
            var totalRadian         = UnityEngine.Random.Range(0.0f, 360.0f);
            var additionalMinRadian = Mathf.PI * 0.100f;
            var additionalMaxRadian = Mathf.PI * 0.225f;
            var partialCount        = _particleList.Count;
            var index               = _lastUsedPartialIndex;
            var checkCount          = partialCount;
            var remainCount         = count;

            Calc_Infos();

            finalSpreadInfos        = spreadInfos;
            finalMaxSpreadTime      = maxSpreadTime;
            finalMinAbsorbDist      = minAbsorbDist;
            finalMaxAbsorbDist      = maxAbsorbDist;
            _lastUsedPartialIndex   = index;

            if (remainCount <= 0)                       return;
            if (!_autoGenerate)                         return;
            if (_autoGenerateMaxCount <= partialCount)  return;

            var generateCount = Mathf.CeilToInt(remainCount / (float)_autoGenerateCount) * _autoGenerateCount;
            if (_autoGenerateMaxCount < partialCount + generateCount)
                generateCount = _autoGenerateMaxCount - partialCount;

            GeneratePartial(generateCount);

            partialCount    = _particleList.Count;
            index           = partialCount - generateCount;
            checkCount      = generateCount;

            Calc_Infos();

            finalSpreadInfos        = spreadInfos;
            finalMaxSpreadTime      = maxSpreadTime;
            finalMinAbsorbDist      = minAbsorbDist;
            finalMaxAbsorbDist      = maxAbsorbDist;
            _lastUsedPartialIndex   = index;



            void Calc_Infos()
            {
                while (0 < checkCount && 0 < remainCount)
                {
                    ++index;
                    --checkCount;
                    if (partialCount <= index)
                        index = 0;

                    var partial = _particleList[index];
                    if (null == partial)
                        continue;

                    if (partial.IsUsed)
                        continue;

                    totalRadian += UnityEngine.Random.Range(additionalMinRadian, additionalMaxRadian);

                    var r               = UnityEngine.Random.Range(_spread_MinDistance, _spread_MaxDistance);
                    var spreadEndPoint  = new Vector2(centerPoint.x + Mathf.Cos(totalRadian) * r,
                                                      centerPoint.y + Mathf.Sin(totalRadian) * r);
                    var absorbDist      = Vector2.Distance(spreadEndPoint, absorbPoint);
                    var spreadTime      = MathUtil.Lerp_Percent_Between_A_and_B(_spread_MinDistance, _spread_MaxDistance, 
                                                                                r, 
                                                                                _spread_MinTimeLength, _spread_MaxTimeLength);

                    if (maxSpreadTime < spreadTime) maxSpreadTime = spreadTime;
                    if (absorbDist < minAbsorbDist) minAbsorbDist = absorbDist;
                    if (maxAbsorbDist < absorbDist) maxAbsorbDist = absorbDist;

                    if (null != iconSprite)
                    {
                        partial.IconSprite  = iconSprite;
                        partial.SizeDelta   = iconSize;
                    }

                    partial.AnchoredPosition    = centerPoint;
                    partial.LocalScale          = Vector3.zero;
                    partial.Alpha               = 0.0f;
                    partial.Use();

                    spreadInfos.Add(new SpreadInfo
                    {
                        Partial             = partial,
                        SpreadEndPoint      = spreadEndPoint,
                        SpraedTimeLength    = spreadTime,
                        AbsorbDist          = absorbDist,
                        AbosrbEndPoint      = absorbPoint,
                    });

                    --remainCount;
                }
            }
        }

        IEnumerator CoroutineSpread(SpreadInfo info)
        {
            var partial     = info.Partial;
            var startPoint  = partial.AnchoredPosition;
            var endPoint    = info.SpreadEndPoint;
            var elapsed     = 0.0f;
            var timeLength  = info.SpraedTimeLength;

            partial.LocalScale  = _spread_StartScale * Vector3.one;
            partial.Alpha       = 1.0f;
            
            while (elapsed < timeLength)
            {
                yield return null;
                elapsed += Time.deltaTime;

                if (!partial.IsUsed)
                    yield break;

                var factor      = elapsed / timeLength;
                var moveFactor  = _spread_MoveCurve .Evaluate(factor);
                var scaleFactor = _spread_ScaleCurve.Evaluate(factor);

                partial.AnchoredPosition    = Vector2.Lerp(startPoint, endPoint, moveFactor);
                partial.LocalScale          = Mathf.Lerp(_spread_StartScale, _spread_EndScale, scaleFactor) * Vector3.one;
            }
        }

        IEnumerator CoroutineAbsorb(CurrencyParticle partial, Vector2 endPoint, float timeLength)
        {
            var elapsed     = 0.0f;
            var startPoint  = partial.AnchoredPosition;
            while (elapsed < timeLength)
            {
                yield return null;
                elapsed += Time.deltaTime;

                if (!partial.IsUsed)
                    yield break;

                var factor      = elapsed / timeLength;
                var moveFactor  = _absorb_MoveCurve .Evaluate(factor);
                var scaleFactor = _absorb_ScaleCurve.Evaluate(factor);

                partial.AnchoredPosition    = Vector2.Lerp(startPoint, endPoint, moveFactor);
                partial.LocalScale          = Mathf.Lerp(_spread_EndScale, _absorb_EndScale, scaleFactor) * Vector3.one;
            }

            partial.Init();
            _onAbsorbPartial?.Invoke();
        }
    
    
    
        // 파티클 연출 계산을 단순화 하기 위한 함수
        void ResetRTF()
        {
            if (null == _rtf)
            {
                _rtf = transform as RectTransform;
                if (null == _rtf)
                    return;
            }

            _rtf.sizeDelta = Vector2.zero;
        }
        Vector2 Calc_WorldToAnchoredPos(RectTransform targetRTF)
        {
            if (null == _calcSupportRTF)
            {
                Debug.LogWarning($"[CurrencyEffector - Calc_WorldToAnchoredPos] _calcSupportRTF is null!");
                return Vector2.zero;
            }

            if (null == targetRTF)
            {
                Debug.LogWarning($"[CurrencyEffector - Calc_WorldToAnchoredPos] target is null!");
                return Vector2.zero;
            }

            _calcSupportRTF.position = targetRTF.position;

            var targetSize                      = targetRTF.sizeDelta;
            var targetPivot                     = targetRTF.pivot;
            _calcSupportRTF.anchoredPosition   += new Vector2(targetSize.x * 0.5f - targetSize.x * targetPivot.x,
                                                              targetSize.y * 0.5f - targetSize.y * targetPivot.y);

            return _calcSupportRTF.anchoredPosition;
        }
    }
}
