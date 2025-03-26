using UnityEngine;

namespace AllIn1VfxToolkit.Demo.Scripts
{
    public class AllIn1DoShake : MonoBehaviour
    {
        [SerializeField] private float _shakeAmount = 0.15f;
        [SerializeField] private bool doShakeOnStart;
        [SerializeField] private float shakeOnStartDelay;
        
        private void Update() {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                DoShake();
            }
        }

        public void DoShake()
        {
            DoShake(_shakeAmount);
        }
        public void DoShake(float shakeAmount)
        {
            AllIn1Shaker.i.DoCameraShake(shakeAmount);
        }
    }
}