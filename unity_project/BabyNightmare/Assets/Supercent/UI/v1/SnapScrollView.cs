using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace Supercent.UI
{
    public class SnapScrollView : ScrollRect
    {
        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private HorizontalLayoutGroup HLG_Contents;
        [SerializeField] private RectTransform RTF_Item;
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private float _snapLimitVelocity;
        [SerializeField] private float _snapLimitDuration;
        [SerializeField] private float _snapThreshold = 10.0f;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private float _itemWidth = 0f;
        private float _spacing = 0f;
        private float _padding = 0f;

        private List<float> _itemNormlas = null;

        private bool _isOnDrag = false;
        private Coroutine _coSnap = null;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public bool IsInited { get; private set; } = false;

        public int ItemIndex { get; private set; } = 0;

        public int TotalItemCount => _itemNormlas.Count;

        public UnityEvent OnBeginScrollEvent { get; private set; } = new UnityEvent();
        public UnityEvent OnEndedScrollEvent { get; private set; } = new UnityEvent();
        public UnityEvent<int> OnChangeItemIndexEvent { get; private set; } = new UnityEvent<int>();

        public bool DragLock { get; set; } = false;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public void Init()
        {
            if (IsInited)
                return;
            IsInited = true;

            _itemWidth = RTF_Item.sizeDelta.x;
            _padding   = (viewport.rect.width - _itemWidth) * 0.5f;
            _spacing   = HLG_Contents?.spacing ?? 0f;

            // var temp = Mathf.FloorToInt(_padding);
            // HLG_Contents.padding.left  = temp;
            // HLG_Contents.padding.right = temp;   

            this.onValueChanged.AddListener((v) =>
            { 
                // Debug.Log(velocity.x);

                if (null != _coSnap || _isOnDrag)
                    return;

                var x = velocity.x;
                if (-_snapLimitVelocity <= x && x <= _snapLimitVelocity)
                    _coSnap = StartCoroutine(Co_Snap());

            });
        }

        public void ResetItemNormals()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());

            var contentsWidth = content.rect.width - viewport.rect.width;
            var limit         = content.rect.width - _padding;

            _itemNormlas?.Clear();
            _itemNormlas = new List<float>();

            var temp = 0f;
            var add  = _itemWidth + _spacing;

            while (temp < limit)
            {
                _itemNormlas.Add(temp / contentsWidth);
                temp += add;
            }
        }

        public void GoItemPos(int itemIndex, bool immediate = true)
        {
            if (null == _itemNormlas || itemIndex < 0 || _itemNormlas.Count <= itemIndex)
                return;

            if (immediate)
            {
                _isOnDrag = true;
                horizontalNormalizedPosition = _itemNormlas[itemIndex];
                _isOnDrag = false;

                SetItemIndex(itemIndex);
            }
            else
                StartCoroutine(Co_GoItemPos(itemIndex));
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (DragLock)
                return;

            OnBeginScrollEvent.Invoke();

            _isOnDrag = true;
    
            if (null != _coSnap)
            {
                StopCoroutine(_coSnap);
                _coSnap = null;
            }

            base.OnBeginDrag(eventData);
        }
        
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (DragLock)
                return;
                
            _isOnDrag = false;

            base.OnEndDrag(eventData);

            // Debug.Log(velocity.x);

            if (-_snapThreshold <= velocity.x && velocity.x < _snapThreshold)
                _coSnap = StartCoroutine(Co_Snap());
        }

        private IEnumerator Co_Snap()
        {
            // 가까운 곳을 찾는다.
            var index  = 0;
            var min    = float.MaxValue;
            var dist   = 0f;
            var normal = this.horizontalNormalizedPosition;

            for (int i = 0, size = _itemNormlas.Count; i < size; ++i)
            {
                dist = _itemNormlas[i] - normal;
                if (dist < 0f)
                    dist *= -1f;

                if (dist < min)
                {
                    min  = dist;
                    index = i;
                }
            }

            SetItemIndex(index);
            // Debug.Log(index);

            // 위치 이동
            var timer  = 0f;
            var target = _itemNormlas[index];
            var temp   = 0f;
            
            dist = target - normal;

            while (timer < _snapLimitDuration)
            {
                temp = _curve.Evaluate(timer / _snapLimitDuration);
                this.horizontalNormalizedPosition = dist * temp + normal;

                yield return null;
                timer += Time.deltaTime;
            }

            this.horizontalNormalizedPosition = target;

            _coSnap = null;


       
            OnEndedScrollEvent.Invoke();
        }

        //protected override void Start()
        //{
        //    Init();
        //    ResetItemNormals();
        //}


        private IEnumerator Co_GoItemPos(int newItemIndex)
        {
            var timer  = 0f;
            var limit  = 0.25f;
            var begin  = _itemNormlas[ItemIndex];
            var target = _itemNormlas[newItemIndex];
            var dist   = target - begin;

            while (timer < limit)
            {
                this.horizontalNormalizedPosition = _curve.Evaluate(timer / limit) * dist + begin;
                yield return null;
                timer += Time.deltaTime;
            }

            this.horizontalNormalizedPosition = target;
            SetItemIndex(newItemIndex);
        }

        private void SetItemIndex(int index)
        {
            if (ItemIndex == index)
                return;

            ItemIndex = index;
            OnChangeItemIndexEvent.Invoke(index);

            //Debug.Log($"Cur Index is {ItemIndex}");
        }

        //------------------------------------------------------------------------------
        // editor
        //------------------------------------------------------------------------------
#if UNITY_EDITOR
        [CustomEditor(typeof(SnapScrollView))]
        private class EDITOR_SnapScrollView : ScrollRectEditor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                var t = (SnapScrollView)target;

                EditorGUILayout.Space();
                t.HLG_Contents = (HorizontalLayoutGroup)EditorGUILayout.ObjectField("HLG Contents", t.HLG_Contents, typeof(HorizontalLayoutGroup), true);
                t.RTF_Item = (RectTransform)EditorGUILayout.ObjectField("RTF Item", t.RTF_Item, typeof(RectTransform), true);
                t._curve = EditorGUILayout.CurveField("Smoother Curve", t._curve);
                t._snapLimitVelocity = EditorGUILayout.FloatField("Snap Limit Velocity", t._snapLimitVelocity);
                t._snapLimitDuration = EditorGUILayout.FloatField("Snap Limit Duration", t._snapLimitDuration);
                t._snapThreshold = EditorGUILayout.FloatField("Snap Threshold", t._snapThreshold);

                UnityEditor.EditorUtility.SetDirty(t);
            }
        }
#endif
    }
}