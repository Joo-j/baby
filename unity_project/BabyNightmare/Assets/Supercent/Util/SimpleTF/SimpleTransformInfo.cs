using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Supercent.Util.SimpleTF
{
    public class SimpleTransformInfo : MonoBehaviour
    {
        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public string Key = string.Empty;


#if UNITY_EDITOR
        [ContextMenu("Name as key")]
        void EDITOR_NameAsKey()
        {
            Key = this.name;
        }
#endif
    }
}