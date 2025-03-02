using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Supercent.UI.Culling
{
    public abstract class UICuller : UIBehaviour
    {
        private bool _isVisible = false;


        public abstract RectTransform RTF { get; }
        protected abstract RectTransform Viewport { get; }


        protected abstract void OnChangeVisible(bool isVisible);

        protected virtual void OnLateUpdate()
        {

        }

        protected void LateUpdate()
        {
            UICullingHelper.CheckVisibility(RTF, Viewport, ChangeVisible);

            OnLateUpdate();
        }


        private void ChangeVisible(bool visible)
        {
            if (_isVisible == visible)
                return;

            _isVisible = visible;

            OnChangeVisible(visible);
        }

    }
}