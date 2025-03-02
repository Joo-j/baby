using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace Supercent.UIv2
{
    public static class AssignHelper
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private static IClickEventable _mountedScript = null;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        /// <summary>
        /// 헬퍼에 IClickEventable 를 마운트합니다. 이 후 연결을 시도하는 버튼등은 해당 IClickEventable 의 함수에 연결됩니다.
        /// </summary>
        /// <param name="script">마운트 할 script</param>
        /// <returns>마운트 성공시 true, 실패시 false</returns>
        public static bool MountUI(IClickEventable script)
        {
            if (null == script)
            {
                Debug.LogError($"[Supercent.UIv2.AssignHelper.MountUI] UI 가 null 이거나 gameobject 가 null 입니다.");
                return false;
            }

            _mountedScript = script;
            return true;
        }

        /// <summary>
        /// 해당 IClickEventable 의 마운트를 끊습니다. 현재 마운트 된 IClickEventable 가 아닐경우 무시합니다.
        /// </summary>
        /// <param name="script">끊을 script</param>
        public static void UnmountUI(IClickEventable script)
        {
            if (script == _mountedScript)
                _mountedScript = null;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// 마운트한 IClickEventable 의 OnButtonEvent 함수에 해당 버튼을 연결합니다.
        /// </summary>
        /// <param name="button">연결할 Button</param>
        /// <returns>연결 성공시 true, 실패시 false</returns>
        public static bool TryAssignButton(Button button)
        {
            if (null == button)
            {
                Debug.LogError($"[Supercent.UIv2.AssignHelper.TryAssignButton] 연결할 Button 을 찾을 수 없습니다.");
                return false;
            }

            if (null == _mountedScript)
            {
                Debug.LogError($"[Supercent.UIv2.AssignHelper.TryAssignButton] Button 이 연결될 IClickEventable 가 없습니다.");
                return false;
            }

            // 기존연결 제거
            while (0 < button.onClick.GetPersistentEventCount())
                UnityEventTools.RemovePersistentListener(button.onClick, 0);

            // 새 연결 추가
            UnityEventTools.AddObjectPersistentListener(button.onClick, _mountedScript.OnButtonEvent, button);

            // click animation 연결
            button.GetComponent<SimpleClickAnimation>()?.EDT_ONLY_TryAssignButton();
            button.GetComponent<SimpleOpenURLBtn>()?.EDT_ONLY_TryAssignButton();

            return true;
        }

        /// <summary>
        /// 마운트한 IClickEventable 의 OnToggleEvent 함수에 해당 토글을 연결합니다.
        /// </summary>
        /// <param name="toggle">연결할 Toggle</param>
        /// <returns>연결 성공시 true, 실패시 false</returns>
        public static bool TryAssignToggle(Toggle toggle)
        {
            if (null == toggle)
            {
                Debug.LogError($"[Supercent.UIv2.AssignHelper.TryAssignToggle] 연결할 Toggle 을 찾을 수 없습니다.");
                return false;
            }

            if (null == _mountedScript)
            {
                Debug.LogError($"[Supercent.UIv2.AssignHelper.TryAssignToggle] Toggle 이 연결될 IClickEventable 가 없습니다.");
                return false;
            }

            // 기존연결 제거
            while (0 < toggle.onValueChanged.GetPersistentEventCount())
                UnityEventTools.RemovePersistentListener(toggle.onValueChanged, 0);

            // 새 연결 추가
            UnityEventTools.AddObjectPersistentListener(toggle.onValueChanged, _mountedScript.OnToggleEvent, toggle);

            return true;
        }
#endif
    }
}
