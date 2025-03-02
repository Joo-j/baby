using System;
using UnityEngine;

namespace Supercent.Util
{
    public class SimpleMouseInput
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private Action<Vector3> _onMouseDown = null;
        private Action<Vector3> _onMouseMove = null;
        private Action<Vector3> _onMouseUp   = null;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public Vector3 FirstDownPosition { get; private set; } = Vector3.zero;
        public Vector3 LastMousePosition { get; private set; } = Vector3.zero;
        public Vector3 Delta             { get; private set; } = Vector3.zero;
        public Vector3 MoveDistance      => LastMousePosition - FirstDownPosition;

        public bool IsPressed { get; private set; } = false;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public void Init(Action<Vector3> onMouseDown, Action<Vector3> onMouseMove, Action<Vector3> onMouseUp)
        {
            _onMouseDown = onMouseDown;
            _onMouseMove = onMouseMove;
            _onMouseUp   = onMouseUp;
            IsPressed    = false;
        }

        public void Update()
        {
            if (IsPressed)
            {
                var prvPosition = LastMousePosition;
                LastMousePosition = Input.mousePosition;
                Delta = LastMousePosition - prvPosition;

                if (Input.GetMouseButton(0))
                    _onMouseMove?.Invoke(LastMousePosition);
                else
                {
                    IsPressed = false;
                    _onMouseUp?.Invoke(LastMousePosition);
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                IsPressed = true;
                LastMousePosition = FirstDownPosition = Input.mousePosition;
                Delta = Vector3.zero;
                _onMouseDown?.Invoke(LastMousePosition);
            }
        }

        public void ReleaseMouse()
        {
            IsPressed = false;
        }
    }
}