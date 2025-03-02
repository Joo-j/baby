using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using System.Reflection;
#endif //UNITY_EDITOR



namespace Supercent.UI
{
    /// <summary>
    /// 가이드 링크 (@카로_김정근 Karo)
    /// https://www.notion.so/supercent/HUD-bf0a13e74999415ebb2802f209ad5fd9?pvs=4
    /// </summary>
    public abstract class CurrencyHUDBase : MonoBehaviour
    {
        class HUDInfos
        {
            public int                      LastRefreshFrame    = 0;
            public List<CurrencyHUDBase>    List                = new List<CurrencyHUDBase>();
        }

        struct ObtainInfo
        {
            public float    Current;
            public int      Target;
        }

        static Dictionary<Type, HUDInfos> _hudDict = null;



        [SerializeField] bool               _useAbsorbFx            = true;     // 재화 획득 시 연출 사용 여부
        [SerializeField] int                _minFxMaxCountPerObtain = 1;        // 1회 연출 실행 시 생성될 최소 입자 개수
        [SerializeField] int                _maxFxMaxCountPerObtain = 30;       // 1회 연출 실행 시 생성될 최대 입자 개수
        [SerializeField] float              _fxCountPerValue        = 0.8f;     // 재화 획득량 1당 연출에 사용될 입자 개수 비율
        [SerializeField] CurrencyEffector   _fx                     = null;

        [Space]
        [SerializeField] Image              _icon       = null;
        [SerializeField] Animation          _iconAnim   = null;
        [SerializeField] TextMeshProUGUI    _valueTMP   = null;



        int     _value              = 0;
        float   _lastAnimAt         = 0.0f;
        float   _lastHapticAt       = 0.0f;

        Sprite  _fxIcon = null;
        Vector2 _fxSize = Vector2.zero;

        LinkedList<ObtainInfo> _obtainInfos = null;



        public bool UseAbsorbFx
        {
            set => _useAbsorbFx = value;
            get => _useAbsorbFx;
        }
        public int MinFxMaxCountPerObtain => _minFxMaxCountPerObtain;
        public int MaxFxMaxCountPerObtain => _maxFxMaxCountPerObtain;
        public float FxCountPerValue => _fxCountPerValue;
        protected virtual float Interval_Anim   => 0.1f;
        protected virtual float Interval_Haptic => 0.1f;



        void Awake()
        {
            _obtainInfos = new LinkedList<ObtainInfo>();

            if (null != _fx)
            {
                _fx.Init();
                _fx.OnAbsorbParticle.AddListener(OnAbsorbParticle);
            }

            _Awake();
        }
        protected virtual void _Awake() { }

        void OnDestroy()
        {
            if (null != _fx)
                _fx.OnAbsorbParticle.RemoveListener(OnAbsorbParticle);

            _obtainInfos?.Clear();
            _obtainInfos = null;

            _OnDestroy();
        }
        protected abstract void _OnDestroy();



        void OnEnable() 
        {
            _hudDict ??= new Dictionary<Type, HUDInfos>();

            var key     = GetType();
            var info    = AddGetHUDInfos(key);
            if (null == info)
            {
                Debug.LogWarning($"[CurrencyHUDBase - OnEnable] {name}이 비정상적인 상태입니다.");
                return;
            }

            var list = info.List;
            if (null == list)
            {
                Debug.LogWarning($"[CurrencyHUDBase - OnEnable] {name}이 비정상적인 상태입니다.");
                return;
            }

            list.Add(this);

            RegistEvent(RefreshAll_CurrentType);
            Refresh(true);

            _OnEnable();
        }
        protected virtual void _OnEnable() { }

        void OnDisable()
        {
            _OnDisable();

            var key     = GetType();
            var info    = AddGetHUDInfos(key);
            if (null != info)
                info.List?.Remove(this);

            UnregistEvent(RefreshAll_CurrentType);
        }
        protected virtual void _OnDisable() { }



        static HUDInfos AddGetHUDInfos(Type key)
        {
            if (null == _hudDict)
                return null;

            if (_hudDict.TryGetValue(key, out var infos))
                return infos;

            infos           =
            _hudDict[key]   = new HUDInfos();
            return infos;
        }

        public static T GetFocusedHUD<T>() where T : CurrencyHUDBase
        {
            var key = typeof(T);
            return GetFocusedHUD(key) as T;
        }
        public static CurrencyHUDBase GetFocusedHUD(Type key)
        {
            var hudInfos = AddGetHUDInfos(key);
            if (null == hudInfos)
                return null;

            var list = hudInfos.List;
            if (null == list)
                return null;

            if (list.Count <= 0)
                return null;

            return list[list.Count - 1];
        }

        public static IEnumerable<T> GetHUDList<T>() where T : CurrencyHUDBase
        {
            var key = typeof(T);
            var iter = GetHUDList(key);
            if (null == iter)
                yield break;

            foreach (var hud in iter)
                yield return hud as T;
        }
        public static IEnumerable<CurrencyHUDBase> GetHUDList(Type key)
        {
            var hudInfos = AddGetHUDInfos(key);
            if (null == hudInfos)
                return null;

            var list = hudInfos.List;
            if (null == list)
                return null;

            if (list.Count <= 0)
                return null;

            return list;
        }



        void Start() => _Start();
        protected abstract void _Start();



        void OnAbsorbParticle()
        {
            var currentAt = Time.time;
            if (Interval_Anim < currentAt - _lastAnimAt)
            {
                PlayIconAnim();
                _lastAnimAt = currentAt;
            }

            if (Interval_Haptic < currentAt - _lastHapticAt)
            {
                PlayHaptic();
                _lastHapticAt = currentAt;
            }

            _OnAbsorbParticle();
        }
        protected abstract void _OnAbsorbParticle();

        protected virtual void PlayIconAnim()
        {
            if (null == _iconAnim)
                return;

            _iconAnim.Stop();
            _iconAnim.Play();
        }
        protected abstract void PlayHaptic();



        // 유저 정보의 재화 정보가 변경되었을 때 HUD를 갱신해주기 위한 이벤트를 등록
        // callback == RefreshAll_CurrentType
        //
        // (ex 1. User.OnChangedCoinEvent.AddListener(RefreshAll_CurrentType))
        // (ex 2. AccountFollower.OnChangeFollower += callback)
        protected abstract void RegistEvent(Action<int> callback);
        protected abstract void UnregistEvent(Action<int> callback);



        protected void RefreshAll_CurrentType(int value) => RefreshAll(GetType(), value, false);
        public static void RefreshAll(Type key, int value, bool force)
        {
            var info = AddGetHUDInfos(key);
            if (null == info)
                return;

            var currentFrame = Time.frameCount;
            if (!force && info.LastRefreshFrame == currentFrame)
                return;

            info.LastRefreshFrame = currentFrame;

            var list = info.List;
            if (null == list)
                return;

            if (list.Count <= 0)
                return;

            var useSkipAnim = false;
            for (int n = list.Count - 1; 0 <= n; --n)
            {
                var hud = list[n];
                if (null == hud)
                    continue;

                hud.Refresh(value, useSkipAnim);
                useSkipAnim = true;
            }
        }



        public void Refresh(bool useSkipAnim)               => OnRefresh(GetCurrencyValue_FromUserData(), useSkipAnim);
        public void Refresh(int value, bool useSkipAnim)    => OnRefresh(value, useSkipAnim);
        void OnRefresh(int value, bool useSkipAnim)
        {
            if (useSkipAnim)
            {
                StopAllCoroutines();
                if (null != _fx)
                    _fx.HideAllParticle();

                _obtainInfos.Clear();
                _value = value;

                ApplyValue_ForUI();
                return;
            }

            var targetValue = Calc_TargetValue();
            var obtainValue = value - targetValue;
            if (!gameObject.activeSelf || obtainValue <= 0)
            {
                _value += obtainValue;
                ApplyValue_ForUI();
                return;
            }

            var node = _obtainInfos.AddLast(new ObtainInfo
            {
                Current = 0.0f,
                Target  = obtainValue,
            });

            StartCoroutine(Co_PlayFx(node));
        }



        IEnumerator Co_PlayFx(LinkedListNode<ObtainInfo> node)
        {
            if (null == node)
                yield break;

            var obtainInfo = node.Value;
            var timeLength = 0.25f;
            if (_useAbsorbFx && null != _fx)
            {
                var isWait      = true;
                var waitTime    = 0.0f;
                var fxIcon      = default(Sprite);
                var fxSize      = default(Vector2);

                if (null != _fxIcon)
                {
                    fxIcon = _fxIcon;
                    fxSize = _fxSize;
                }
                else
                {
                    if (null != _icon)
                    {
                        fxIcon = _icon.sprite;
                        fxSize = _icon.rectTransform.sizeDelta;
                    }
                }

                var fxCount = Mathf.RoundToInt(obtainInfo.Target * _fxCountPerValue);
                fxCount     = Mathf.Clamp(fxCount, _minFxMaxCountPerObtain, _maxFxMaxCountPerObtain);

                _fx.Play(fxIcon, fxSize, fxCount, (minAbsorbTime, maxAbsorbTime) =>
                {
                    isWait      = false;
                    waitTime    = minAbsorbTime;
                    timeLength  = Mathf.Max(timeLength, maxAbsorbTime - minAbsorbTime);
                });

                while (isWait)
                    yield return null;

                if (0.0f < waitTime)
                {
                    // Spread 연출이 종료된 후 최초로 Absorb되는 재화가 발생할 때 까지 대기
                    var secDone = Time.timeAsDouble + waitTime;
                    while (Time.timeAsDouble < secDone)
                        yield return null;
                }
            }

            var elapsed = 0.0f;
            while (elapsed < timeLength)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor          = elapsed / timeLength;
                obtainInfo.Current  = obtainInfo.Target * factor;

                node.Value = obtainInfo;
                ApplyValue_ForUI();

                if (!_useAbsorbFx)
                    continue;
                
                if (null != _fx)
                    continue;

                OnAbsorbParticle();
            }

            _value += obtainInfo.Target;
            _obtainInfos?.Remove(node);

            ApplyValue_ForUI();
        }



        void ApplyValue_ForUI()
        {
            var currentValue = Mathf.Max(0, Calc_CurrentValue());
            if (null != _valueTMP)
                _valueTMP.text = ValueToString(currentValue);

            _ApplyValue_ForUI(currentValue);
        }
        protected abstract void _ApplyValue_ForUI(int value);



        protected abstract int GetCurrencyValue_FromUserData();
        protected virtual string ValueToString(int value) => $"{value:0}";
        public int Calc_CurrentValue()
        {
            if (null == _obtainInfos)
                return 0;

            var value   = (float)_value;
            var node    = _obtainInfos.First;
            while (null != node)
            {
                value  += node.Value.Current;
                node    = node.Next;
            }

            return Mathf.RoundToInt(value);
        }
        public int Calc_TargetValue()
        {
            if (null == _obtainInfos)
                return 0;

            var value   = _value;
            var node    = _obtainInfos.First;
            while (null != node)
            {
                value  += node.Value.Target;
                node    = node.Next;
            }

            return value;
        }



        /// <summary>
        /// 동일한 캔버스 위에 존재하는 UI의 월드 좌표를 기준으로 연출이 끝나도록 지정
        /// </summary>
        public void SetAbsorbPoint_FromUI(Vector3 worldPosition)
        {
            if (null == _fx)
                return;

            _fx.SetAbsorbPoint_FromUI(worldPosition);
        }



        /// <summary>
        /// 동일한 캔버스 위에 존재하는 UI의 월드 좌표를 기준으로 연출이 시작되도록 지정
        /// </summary>
        public void SetSpreadPoint_FromUI(Vector3 worldPosition)
        {
            if (null == _fx)
                return;

            _fx.SetSpreadPoint_FromUI(worldPosition);
        }


        /// <summary>
        /// 월드 상의 오브젝트의 좌표를 기준으로 연출이 시작되도록 지정
        /// </summary>
        public void SetSpreadPoint_FromWorld(Vector3 worldPos, Camera worldCamera, RectTransform uiCanvasRect)
        {
            // UI 상의 위치 찾기
            var startAnchoredPoint_MainCam = worldCamera.WorldToScreenPoint(worldPos);
            var rateX = startAnchoredPoint_MainCam.x / Screen.width;
            var rateY = startAnchoredPoint_MainCam.y / Screen.height;
            var uiCamRect = uiCanvasRect.rect;
            var startAnchoredPoint = new Vector3(rateX * uiCamRect.width - uiCamRect.width * 0.5f        // MainCam 에서 ViewPort는 좌하단 기준이기 때문에 화면 너비 절반을 뺀다. + UI획득하는 주인공 머리 위로 이동
                                                            , rateY * uiCamRect.height - uiCamRect.height * 0.5f     // MainCam 에서 ViewPort는 좌하단 기준이기 때문에 화면 너비 절반을 뺀다. + UI획득하는 주인공 머리 위로 이동
                                                            , 0f);
            var finalPos = uiCanvasRect.position + startAnchoredPoint * uiCanvasRect.localScale.x;
            SetSpreadPoint_FromUI(finalPos);
        }


        /// <summary>
        /// 메인 아이콘 및 기본 크기를 변경
        /// </summary>
        public void SetIcon(Sprite icon)
        {
            _icon.sprite = icon;
        }

        /// <summary>
        /// 재화 획득 연출 시 입자의 아이콘 및 기본 크기를 변경
        /// </summary>
        public void SetFxInfo(Sprite icon, Vector2 size)
        {
            _fxIcon = icon;
            _fxSize = size;
        }
        
        /// <summary>
        /// 재화 획득 연출 시 입자 개수 결정과 관련된 값들 변경
        /// </summary>
        /// <param name="min">1회 연출 실행 시 생성될 최소 입자 개수</param>
        /// <param name="max">1회 연출 실행 시 생성될 최대 입자 개수</param>
        /// <param name="countPerValue">재화 획득량 1당 연출에 사용될 입자 개수 비율</param>
        public void SetFxCountInfo(int min, int max, float countPerValue)
        {
            Debug.Assert(min <= max, $"[CurrencyHUDBase.SetFxCountRange] min({min})은 max({max})보다 클 수 없습니다.");
            Debug.Assert(min >= 0, $"[CurrencyHUDBase.SetFxCountRange] min({min})은 0보다 클 수 없습니다.");
            Debug.Assert(countPerValue >= 0.0f, $"[CurrencyHUDBase.SetFxCountRange] CountPerValue({countPerValue})는 0보다 작을 수 없습니다.");

            _minFxMaxCountPerObtain = min;
            _maxFxMaxCountPerObtain = max;

            _fxCountPerValue = countPerValue;
        }

        /// <summary>
        /// 재화 획득 연출 시 입자의 아이콘 및 기본 크기를 초기화
        /// </summary>
        public void ClearFxInfo()
        {
            _fxIcon = null;
            _fxSize = Vector2.zero;
        }



#if UNITY_EDITOR
        [ContextMenu("Bind Serialized Field")]
        protected void BindSerializedField()
        {
            UnityEditor.Undo.RecordObject(this, "Bind Serialized Field");
            OnBindSerializedField();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        protected virtual void OnBindSerializedField() { }
        public virtual void OnInspectorGUI() { }
        public virtual void OnSceneGUI() { }
#endif //UNITY_EDITOR
    }


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(CurrencyHUDBase), true)]
    [UnityEditor.CanEditMultipleObjects]
    [ExecuteInEditMode]
    public class CurrencyHUDBaseEditor : UnityEditor.Editor
    {
        public static readonly MethodInfo BIND_SERIALIZED_FIELD = typeof(CurrencyHUDBase).GetMethod("BindSerializedField", BindingFlags.NonPublic | BindingFlags.Instance);



        public override void OnInspectorGUI()
        {
            var targetList = serializedObject.targetObjects;
            if (null == targetList)
                return;

            if (GUILayout.Button("Bind Serialized Field"))
            {
                for (int n = 0, cnt = targetList.Length; n < cnt; ++n)
                {
                    var target = targetList[n] as CurrencyHUDBase;
                    if (null == target)
                        continue;

                    BIND_SERIALIZED_FIELD.Invoke(target, null);
                }
            }

            CustomInspectorGUI();

            for (int n = 0, cnt = targetList.Length; n < cnt; ++n)
            {
                var target = targetList[n] as CurrencyHUDBase;
                if (null == target)
                    continue;

                target.OnInspectorGUI();
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 유니티에서 기본적으로 제공되는 Inspector GUI부분을 커스텀하는 메서드
        /// </summary>
        protected virtual void CustomInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        void OnSceneGUI() 
        {
            if (null == target)
                return;

            var bb = target as CurrencyHUDBase;
            bb.OnSceneGUI();
        }
    }
#endif // UNITY_EDITOR
}
