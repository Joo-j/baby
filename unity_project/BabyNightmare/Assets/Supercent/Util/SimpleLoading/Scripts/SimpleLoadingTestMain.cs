using UnityEngine;

namespace Supercent.Util
{
    public class SimpleLoadingTestMain : MonoBehaviour
    {
        private void Update() 
        {
#if UNITY_EDITOR
            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    SimpleLoading.ShowFullOfScreen(null);
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    SimpleLoading.HideFullOfScreen(null);
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    SimpleLoading.ShowPartsOfScreen();
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                    SimpleLoading.HidePartsOfScreen();
            }
#endif
        }
    }
}