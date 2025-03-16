using UnityEngine;
using UnityEngine.UI;
using Supercent.Util;

namespace BabyNightmare.Util
{
    public static class FocusOverlayHelper
    {
        private const int DIMD_ORDER = 9;
        private const int CANVAS_ORDER = 10;
        private static DimmedView _dimmedView = null;
        private static Canvas _targetCanvas = null;
        private static Canvas _tempCanvas = null;
        private static GraphicRaycaster _targetRaycaster = null;
        private static GraphicRaycaster _tempRaycaster = null;
        private static RenderMode _originRenderMode;
        private static int _originOrder = 0;
        private static GameObject _targetGameObject = null;


        public static bool IsActive => _tempCanvas != null;

        public static bool IsTarget(GameObject gameObject)
        {
            return _targetGameObject == gameObject;
        }

        public static void Apply(GameObject gameObject, float dimAlpha = 0.5f)
        {
            Debug.Log($"FocusOverlayHelper Apply");

            if (true == IsActive)
            {
                Debug.Log($"FocusOverlayHelper 이미 사용 중으로 끄고 재시작합니다.");
                Clear();
            }

            if (_dimmedView != null)
            {
                GameObject.Destroy(_dimmedView.gameObject);
                _dimmedView = null;
            }

            _dimmedView = ObjectUtil.LoadAndInstantiate<DimmedView>("Util/DimmedView", null);
            _dimmedView.SetAlpha(dimAlpha);
            _dimmedView.SetSortingOrder(DIMD_ORDER);

            if (false == gameObject.TryGetComponent<Canvas>(out _targetCanvas))
            {
                _targetCanvas = gameObject.AddComponent<Canvas>();
                _tempCanvas = _targetCanvas;

                _targetRaycaster = gameObject.AddComponent<GraphicRaycaster>();
                _tempRaycaster = _targetRaycaster;
            }
            else
            {
                _originRenderMode = _targetCanvas.renderMode;
                _originOrder = _targetCanvas.sortingOrder;

                if (false == gameObject.TryGetComponent<GraphicRaycaster>(out _targetRaycaster))
                {
                    _targetRaycaster = gameObject.AddComponent<GraphicRaycaster>();
                }
            }

            _targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _targetCanvas.overrideSorting = true;
            _targetCanvas.sortingOrder = CANVAS_ORDER;
            _targetGameObject = gameObject;
        }

        public static void Clear()
        {
            if (false == IsActive)
                return;

            Debug.Log($"FocusOverlayHelper Clear");

            _targetCanvas.renderMode = _originRenderMode;
            _targetCanvas.sortingOrder = _originOrder;
            _targetCanvas = null;

            if (_dimmedView != null && _dimmedView.gameObject != null)
            {
                GameObject.Destroy(_dimmedView.gameObject);
                _dimmedView = null;
            }

            if (_tempRaycaster != null)
            {
                GameObject.DestroyImmediate(_tempRaycaster);
                _tempRaycaster = null;
            }

            if (_tempCanvas != null)
            {
                GameObject.DestroyImmediate(_tempCanvas);
                _tempCanvas = null;
            }

            _originOrder = 0;
            _targetGameObject = null;
        }

        public static void SetDimdViewAlpha(float alpha)
        {
            if (false == IsActive)
                return;

            _dimmedView.SetAlpha(alpha);
        }
    }
}