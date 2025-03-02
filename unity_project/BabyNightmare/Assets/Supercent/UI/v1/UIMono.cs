using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Supercent.UI
{
    public class UIMono : MonoBehaviour
    {

        [SerializeField] protected Text[] m_Txt;
        [SerializeField] protected TextMeshProUGUI[] m_TMP;
        [SerializeField] protected Image[] m_Img;
        [SerializeField] protected Button[] m_Btn;
        [SerializeField] protected RectTransform[] m_Rt;
        [SerializeField] protected InputField[] m_If;
        [SerializeField] protected GameObject[] m_Go;


        [HideInInspector] public RectTransform m_Transform;

        public virtual void OnButtonEvent(int buttonId)
        {
        }

        public virtual void OnInputFieldEnd(InputField inputField)
        {
            ;
        }

        public virtual void OnToggleEvent(Toggle toggle)
        {
        }

        public delegate void Event_UIVisible();
        public void NULL_Event_UIVisible() { }

        public delegate void Event_UISetWithInt(int v);
        public void NULL_Event_UISetWithInt(int v) { }

        public delegate void Event_UISetWithInt2(int v1, int v2);
        public void NULL_Event_UISetWithInt2(int v1, int v2) { }

        public void NULL_Event_ButtonClickWithIB(int type, bool bFlag) {; }


        public delegate void Event_UISetWithBool(bool v);
        public void NULL_Event_UISetWithBool(bool v) { }

        public delegate void Event_ButtonClick();
        public delegate void Event_ButtonClickWithBool(bool bFlag);
        public delegate void Event_ButtonClickWithInt(int type);
        public delegate void Event_ButtonClickWithInt2(int type1, int type2);
        public delegate void Event_ButtonClickWithIB(int type, bool bFlag);
        public delegate void Event_ButtonClickWithString(string str);
        public delegate void Event_ButtonClickWithString2(string str, string str2);
        public void NULL_Event_ButtonClickWithString(string str) { }

#if UNITY_EDITOR

        [ContextMenu("Assign Button Event")]
        public void AssignButtonEvent()
        {
            if (m_Btn == null)
                return;
            var nameList = "";
            int idx = 0;
            foreach (Button unit in m_Btn)
            {
                while (unit.onClick.GetPersistentEventCount() > 0)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(unit.onClick, 0);
                }
                UnityEditor.Events.UnityEventTools.AddIntPersistentListener(unit.onClick, OnButtonEvent, idx++);
                Navigation nav = new Navigation();
                nav.mode = Navigation.Mode.None;
                unit.navigation = nav;
                nameList += unit.name + "\n";
            }
        }
        [ContextMenu("Assign InputField Event")]
        public void AssignInputFieldEvent()
        {
            if (m_If == null)
                return;
            var nameList = "";
            foreach (InputField unit in m_If)
            {
                while (unit.onEndEdit.GetPersistentEventCount() > 0)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(unit.onEndEdit, 0);
                }
                UnityEditor.Events.UnityEventTools.AddObjectPersistentListener(unit.onEndEdit, OnInputFieldEnd, unit);

                nameList += unit.name + "\n";
            }
        }
#endif
    }
}