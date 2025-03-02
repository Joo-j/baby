using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supercent.Util.SimpleCC
{
    public class CameraController : MonoBehaviour
    {
        [System.Serializable] public class CameraData
        {
            public string     Key           = string.Empty;
            public Transform  Transform     = null;
            public CameraInfo InfoCollector = null;
        }

        [System.Serializable] public class CurveData
        {
            public string         Key   = string.Empty;
            public AnimationCurve Curve = null;
        }

        //------------------------------------------------------------------------------
        // components
        //------------------------------------------------------------------------------
        [SerializeField] private List<CameraData> _cameraInfos;
        [SerializeField] private List<CurveData>  _curveInfos;

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private Coroutine _coMoveTo = null;

        private Dictionary<string, CameraData> _cameraInfoSet = null;
        private Dictionary<string, CurveData>  _curveInfoSet  = null;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public void CameraMoveToImmediate(string cameraKey)
        {
            var info = GetCameraInfo(cameraKey);
            if (null == info)
            {
                Debug.LogError($"[DalgonaBreaker.SimpleCameraController.CameraMoveToImmediate] 해당 키 의 카메라 정보를 찾을 수 없습니다. {cameraKey.ToString()}");
                return;
            }

            if (null != _coMoveTo)
            {
                StopCoroutine(_coMoveTo);
                _coMoveTo = null;
            }

            var cameraTf = Camera.main.transform;
            var infoTf   = null != info.InfoCollector ? info.InfoCollector.transform : info.Transform;

            cameraTf.position = infoTf.position;
            cameraTf.rotation = infoTf.rotation;

            if (null != info.InfoCollector)
                Camera.main.fieldOfView = info.InfoCollector.FieldOfView;
        }

        public void CameraMoveTo(string cameraKey, string curveType, float duration)
        {
            var info = GetCameraInfo(cameraKey);
            if (null == info)
            {
                Debug.LogError($"[SimpleCameraController.CameraMoveTo] 해당 키 의 카메라 정보를 찾을 수 없습니다. key: {cameraKey}");
                return;
            }

            Transform infoTf = info.Transform;
            float     destFieldOfView = Camera.main.fieldOfView;

            if (null != info.InfoCollector)
            {
                infoTf          = info.InfoCollector.transform;
                destFieldOfView = info.InfoCollector.FieldOfView;
            }

            if (null == infoTf)
            {
                Debug.LogError($"[SimpleCameraController.CameraMoveTo] 해당 키 의 Transform 정보를 찾을 수 없습니다. camera: {cameraKey}");
                return;
            }

            var curve = GetCurve(curveType)?.Curve;
            if (null == curve)
            {
                Debug.LogError($"[SimpleCameraController.CameraMoveTo] 해당 키 의 커브 정보를 찾을 수 없습니다. 카메라를 즉시 이동시킵니다. curve: {curveType.ToString()}");
                CameraMoveToImmediate(cameraKey);
                return;
            }

            if (null != _coMoveTo)
                StopCoroutine(_coMoveTo);

            _coMoveTo = StartCoroutine(Co_CameraMoveTo(infoTf, destFieldOfView, curve, duration));
        }

        private void Awake() 
        {
            if (null == _cameraInfoSet)
            {
                _cameraInfoSet = new Dictionary<string, CameraData>();

                for (int i = 0, size = _cameraInfos.Count; i < size; ++i)
                    _cameraInfoSet[_cameraInfos[i].Key] = _cameraInfos[i];
            }

            if (null == _curveInfoSet)
            {
                _curveInfoSet = new Dictionary<string, CurveData>();

                for (int i = 0, size = _curveInfos.Count; i < size; ++i)
                    _curveInfoSet[_curveInfos[i].Key] = _curveInfos[i];
            }
        }

        private IEnumerator Co_CameraMoveTo(Transform targetTf, float destFieldOfView, AnimationCurve curve, float duration)
        {
            var camera    = Camera.main;
            var cameraTf  = camera.transform;
            var beginPos  = cameraTf.position;
            var beginRot  = cameraTf.rotation;
            var targetPos = targetTf.position;
            var targetRot = targetTf.rotation;
            var curvePos  = curve;
            var curveRot  = curve;
            var timer     = 0f;
            var temp      = 0f;

            var beginFieldOfView = camera.fieldOfView;
            var distFieldOfView  = destFieldOfView - beginFieldOfView;

            while (timer < duration)
            {
                temp = timer / duration;

                cameraTf.position = Vector3.Lerp(beginPos, targetPos, curvePos.Evaluate(temp));
                cameraTf.rotation = Quaternion.Lerp(beginRot, targetRot, curveRot.Evaluate(temp));

                camera.fieldOfView = temp * distFieldOfView + beginFieldOfView;

                yield return null;

                timer += Time.deltaTime;
            }

            cameraTf.position = targetPos;
            cameraTf.rotation = targetRot;

            camera.fieldOfView = destFieldOfView;

            _coMoveTo = null;
        }

        private CameraData GetCameraInfo(string key)
        {
            if (_cameraInfoSet.TryGetValue(key, out var info))
                return info;
            
            return null;
        }

        private CurveData GetCurve(string type)
        {
            if (_curveInfoSet.TryGetValue(type, out var info))
                return info;

            return null;
        }
    }
}