using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

namespace Supercent.UIv2
{
    public class SimpleClickAnimation : MonoBehaviour
    {
        public enum EAnimationStyle
        {
            Bounce,
        }

        static readonly Keyframe[] BasicCurve = new Keyframe[]
        {
            new Keyframe(0f, 1f, 1.485121f, 1.485121f, 0f, 0.1287549f) { weightedMode = WeightedMode.None },
            new Keyframe(0.2984212f, 1.200003f, 0.01557483f, 0.01557483f, 0.3333333f, 0.05951206f) { weightedMode = WeightedMode.None },
            new Keyframe(1f, 1f, -0.5665656f, -0.5665656f, 0.09043368f, 0f) { weightedMode = WeightedMode.None },
        };

        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private EAnimationStyle _style    = EAnimationStyle.Bounce;
        [SerializeField] private float           _duration = 0.25f;
        [SerializeField] private AnimationCurve  _curve    = new AnimationCurve(BasicCurve);

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private Coroutine _coPlayAnimation = null;
        private Vector3   _orgScale = Vector3.zero;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        private void Awake() 
        {
            _orgScale = transform.localScale;
        }

        private void OnEnable()
        {
            transform.localScale = _orgScale;
        }

        private void OnDisable() 
        {
            if (null != _coPlayAnimation)
                StopCoroutine(_coPlayAnimation);
            _coPlayAnimation = null;
        }

        private void OnClickButton(Button button)
        {
            if (false == gameObject.activeInHierarchy)
                return;

            switch (_style)
            {
            case EAnimationStyle.Bounce:
                if (null != _coPlayAnimation)
                    StopCoroutine(_coPlayAnimation);
                _coPlayAnimation = StartCoroutine(Co_PlayBounce());
                break;
            }
        }

        private IEnumerator Co_PlayBounce()
        {
            var timer = 0f;
            var limit = _duration;
            
            while (timer < limit)
            {
                transform.localScale = _orgScale * _curve.Evaluate(timer / limit);

                yield return null;
                timer += Time.deltaTime;
            }

            transform.localScale = _orgScale;

            _coPlayAnimation = null;
        }

#if UNITY_EDITOR
        public void EDT_ONLY_TryAssignButton()
        {
            var button = GetComponent<Button>();
            if (null == button)
            {
                Debug.LogError("[UIv2.SimpleClickAnimation] 버튼을 찾을 수 없습니다.");
                return;
            }

            var eventName  = nameof(OnClickButton);
            var addedEvent = false;

            for (int i = 0, size = button.onClick.GetPersistentEventCount(); i < size; ++i)
            {
                if (button.onClick.GetPersistentMethodName(i) == eventName)
                {
                    addedEvent = true;
                    break;
                }
            }

            if (false == addedEvent)
                UnityEventTools.AddObjectPersistentListener(button.onClick, OnClickButton, button);

            EditorUtility.SetDirty(gameObject);
        }

        public void EDT_SetBasicCurve()
        {
            if (_curve == null)
                _curve = new AnimationCurve();
            _curve.keys = BasicCurve;

            EditorUtility.SetDirty(gameObject);
        }
#endif

        //------------------------------------------------------------------------------
        // editor
        //------------------------------------------------------------------------------
#if UNITY_EDITOR
        [CustomEditor(typeof(SimpleClickAnimation), true)]
        private class _EDITOR_SimpleClickAnimation : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                if (true == GUILayout.Button("버튼 연결하기", GUILayout.Height(25f)))
                {
                    var temp = target as SimpleClickAnimation;
                    if (null != temp)
                        temp.EDT_ONLY_TryAssignButton();
                }

                if (true == GUILayout.Button("기본 커브로", GUILayout.Height(25f)))
                {
                    var temp = target as SimpleClickAnimation;
                    if (null != temp)
                        temp.EDT_SetBasicCurve();
                }
            }
        }
#endif
    }
}