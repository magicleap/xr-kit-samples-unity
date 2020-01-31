// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// MLUIInputModule. Requires TrackedDeviceRaycaster on Canvas.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/Input/MLInputModule")]
    public class MLInputModule : BaseInputModule
    {
        /// <summary>
        /// The controller button that should register as a click event.
        /// </summary>
        public enum ButtonClickType : int
        {
            /// <summary/>
            Trigger = 0,
            /// <summary/>
            Bumper = 1,
        }

        /// <summary>
        /// The Input to represent the Pointer.
        /// </summary>
        public enum PointerInputType : int
        {
            /// <summary>
            /// Use headpose to represent Pointer.
            /// </summary>
            Headpose = 0,

            /// <summary>
            /// Use eyes to represent Pointer.
            /// </summary>
            EyeTracking,

            /// <summary>
            /// Use controller to represent Pointer.
            /// </summary>
            Controller,
        }

        /// <summary>
        /// Contains the world space point information about a line segment.
        /// </summary>
        public class LineSegment
        {
            /// <summary/>
            public Vector3 Start { get; internal set; }
            /// <summary/>
            public Vector3? End { get; internal set; }
            /// <summary/>
            public Vector3 Normal { get; internal set; }

            /// <summary/>
            public void Set(Vector3 start, Vector3? end, Vector3 normal)
            {
                Start = start;
                End = end;
                Normal = normal;
            }
        }

        #region Private Variables
        [SerializeField, Tooltip("The input that should be used as a pointer.")]
        private PointerInputType _pointerInput = PointerInputType.Controller;

        [SerializeField, Tooltip("The controller button that should register as a click event.")]
        private ButtonClickType _buttonClick = ButtonClickType.Trigger;

        [SerializeField, Range(1.0f, 10.0f), Tooltip("The scroll speed of content within a scroll view or navigation container.")]
        private float _scrollSpeed = 5.0f;

        [SerializeField, Range(0.1f, 1.0f), Tooltip("The maximum time (in seconds) between two presses for it to be a consecutive click.")]
        private float _clickSpeed = 0.3f;

        private MLInputDevice _pointerInputDevice;
        private List<MLInputController> _controllers = new List<MLInputController>();

        // Used to interact with the UI elements.
        private MLInputDeviceEventData _cachedTrackedPointerEventData;

        // Used to determine hit events.
        private Camera _headpose;

        private LineSegment _pointerLineSegment = new LineSegment();
        #endregion

        #region Public Properties
        /// <summary>
        /// The input source to be used as pointer.
        /// </summary>
        public PointerInputType PointerInput
        {
            get
            {
                return _pointerInput;
            }
        }

        /// <summary>
        /// The line segment between the pointer origin and the hit point, if it exists.
        /// </summary>
        public LineSegment PointerLineSegment
        {
            get
            {
                return _pointerLineSegment;
            }
            private set
            {
                _pointerLineSegment = value;
            }
        }
        #endregion

        #region Unity Methods
        /// <summary/>
        protected override void Start()
        {
            base.Start();

            _headpose = Camera.main;
            if (_headpose == null)
            {
                Debug.LogError("Error: MLInputModule, no camera found tagged as MainCamera, disabling script.");
                enabled = false;
                return;
            }

            // Eye Tracking
            if (_pointerInput == PointerInputType.EyeTracking)
            {
                MLResult result = MLEyes.Start();
                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("Error: MLInputModule failed starting MLEyes, disabling script. Reason: {0}", result);
                    enabled = false;
                    return;
                }
            }

            // Controllers
            if (!MLInput.IsStarted)
            {
                MLInputConfiguration config = new MLInputConfiguration(true);
                MLResult result = MLInput.Start(config);
                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("Error: MLInputModule failed starting MLInput, disabling script. Reason: {0}", result);
                    enabled = false;
                    return;
                }
            }

            AddController(0);
            AddController(1);

            // Track connect / disconnect
            MLInput.OnControllerConnected += HandleOnControllerConnected;
            MLInput.OnControllerDisconnected += HandleOnControllerDisconnected;

            _pointerInputDevice = new MLInputDevice();
            _pointerInputDevice.Initialize();
            _cachedTrackedPointerEventData = new MLInputDeviceEventData(eventSystem);
        }

        /// <summary/>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(MLInput.IsStarted)
            {
                MLInput.Stop();
            }

            MLInput.OnControllerConnected -= HandleOnControllerConnected;
            MLInput.OnControllerDisconnected -= HandleOnControllerDisconnected;

            if(MLEyes.IsStarted)
            {
                MLEyes.Stop();
            }
        }

        /// <summary/>
        protected override void OnEnable()
        {
            // Try to set input override
            BaseInput MLInput = GetComponent<BaseInput>();
            if (MLInput != null)
            {
                m_InputOverride = MLInput;
            }

            base.OnEnable();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Process the input for headpose, eye tracking, controls and standard Unity Editor input (Keyboard, Mouse, Joystick).
        /// </summary>
        public override void Process()
        {
            switch (_pointerInput)
            {
            case PointerInputType.Headpose:
                _pointerInputDevice.Position = _headpose.transform.position;
                _pointerInputDevice.Orientation = _headpose.transform.rotation;
                break;
            case PointerInputType.EyeTracking:
                _pointerInputDevice.Position = _headpose.transform.position;
                if (MLEyes.IsStarted)
                {
                    _pointerInputDevice.Orientation = Quaternion.LookRotation(MLEyes.FixationPoint - _headpose.transform.position);
                }
                break;
            case PointerInputType.Controller:
                if (_controllers.Count > 0)
                {
                    MLInputController controller = _controllers[0];
                    _pointerInputDevice.Position = controller.Position;
                    _pointerInputDevice.Orientation = controller.Orientation;
                }
                break;
            }

            ProcessLine(_pointerLineSegment, ref _pointerInputDevice);

            if (_controllers.Count > 0)
            {
                MLInputController controller = _controllers[0];
                _pointerInputDevice.Select = IsClicked(controller);
                _pointerInputDevice.UpdateTouch(controller, _scrollSpeed);
            }

            ProcessTrackedDevice(ref _pointerInputDevice);
        }
        #endregion

        #region Private Methods
        private bool IsClicked(MLInputController controller)
        {
            if (controller != null && controller.Connected)
            {
                if (_buttonClick == ButtonClickType.Bumper)
                {
                    return controller.IsBumperDown;
                }

                if(_buttonClick == ButtonClickType.Trigger)
                {
                    return controller.TriggerValue >= MLInput.TriggerDownThreshold;
                }
            }

            return false;
        }

        private void ProcessLine(LineSegment line, ref MLInputDevice inputDevice)
        {
            Vector3? endPoint = null;
            Vector3 normal = Vector3.zero;

            MLInputDeviceEventData pointerEventData = _cachedTrackedPointerEventData;
            pointerEventData.Reset();
            inputDevice.CopyTo(ref pointerEventData);
            pointerEventData.pointerCurrentRaycast = PerformRaycast(pointerEventData);
            GameObject currentRaycastObject = pointerEventData.pointerCurrentRaycast.gameObject;

            // enable cursor for any canvas UI or any game object with an Event Trigger (assuming camera has physics raycaster on it)
            if (currentRaycastObject != null &&
                (currentRaycastObject.GetComponent<RectTransform>() != null || currentRaycastObject.GetComponent<EventTrigger>() != null))
            {
                // Relies on TrackedDeviceRaycaster and 3D world position to work with event system
                RaycastResult result = pointerEventData.pointerCurrentRaycast;
                endPoint = result.worldPosition;
                normal = result.worldNormal;
            }

            inputDevice.CopyFrom(pointerEventData);
            inputDevice.OnFrameFinished();

            line.Set(inputDevice.Position, endPoint, normal);
        }

        private RaycastResult PerformRaycast(PointerEventData eventData)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException("eventData");
            }

            eventSystem.RaycastAll(eventData, m_RaycastResultCache);
            var result = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();

            return result;
        }

        private void ProcessMovement(PointerEventData eventData)
        {
            var currentPointerTarget = eventData.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(eventData, currentPointerTarget);
        }

        private void ProcessButton(ButtonDeltaState mouseButtonChanges, ref MLInputDeviceEventData eventData)
        {
            var currentOverGo = eventData.pointerCurrentRaycast.gameObject;

            if (mouseButtonChanges == ButtonDeltaState.Pressed)
            {
                eventData.eligibleForClick = true;
                eventData.delta = Vector2.zero;
                eventData.dragging = false;
                eventData.pressPosition = eventData.position;
                eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;

                var selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);

                // If we have clicked something new, deselect the old thing
                // and leave 'selection handling' up to the press event.
                if (selectHandlerGO != eventSystem.currentSelectedGameObject)
                {
                    eventSystem.SetSelectedGameObject(null, eventData);
                }

                // search for the control that will receive the press.
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, eventData, ExecuteEvents.pointerDownHandler);

                // We didn't find a press handler, so we search for a click handler.
                if (newPressed == null)
                {
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
                }

                var time = Time.unscaledTime;

                if (newPressed == eventData.lastPress && ((time - eventData.clickTime) < _clickSpeed))
                {
                    ++eventData.clickCount;
                }
                else
                {
                    eventData.clickCount = 1;
                }

                eventData.clickTime = time;
                eventData.pointerPress = newPressed;
                eventData.rawPointerPress = currentOverGo;

                // Save the drag handler for drag events during this mouse down.
                eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (eventData.pointerDrag != null)
                {
                    ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.initializePotentialDrag);
                }
            }
            else if (mouseButtonChanges == ButtonDeltaState.Released)
            {
                ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                if (eventData.pointerPress == pointerUpHandler && eventData.eligibleForClick)
                {
                    ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerClickHandler);
                }
                else if (eventData.dragging && eventData.pointerDrag != null)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, eventData, ExecuteEvents.dropHandler);
                    ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.endDragHandler);
                }

                eventData.eligibleForClick = eventData.dragging = false;
                eventData.pointerPress = eventData.rawPointerPress = eventData.pointerDrag = null;
            }
        }

        private void ProcessButtonDrag(PointerEventData eventData, float pixelDragThresholdMultiplier = 1.0f)
        {
            if (!eventData.IsPointerMoving() || Cursor.lockState == CursorLockMode.Locked || eventData.pointerDrag == null)
            {
                return;
            }

            // TODO: this is questionable
            // PointerEventData.position is a Vector2 in screen coordinates... we don't have a screen!!!

            if (!eventData.dragging)
            {
                if ((eventData.pressPosition - eventData.position).sqrMagnitude >= ((eventSystem.pixelDragThreshold * eventSystem.pixelDragThreshold) * pixelDragThresholdMultiplier))
                {
                    ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.beginDragHandler);
                    eventData.dragging = true;
                }
            }

            if (eventData.dragging)
            {
                // If we moved from our initial press object, process an up for that object.
                if (eventData.pointerPress != eventData.pointerDrag)
                {
                    ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

                    eventData.eligibleForClick = false;
                    eventData.pointerPress = null;
                    eventData.rawPointerPress = null;
                }

                ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);
            }
        }

        private void ProcessScroll(PointerEventData eventData)
        {
            var scrollDelta = eventData.scrollDelta;

            if (!Mathf.Approximately(scrollDelta.sqrMagnitude, 0.0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.pointerEnter);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, eventData, ExecuteEvents.scrollHandler);
            }
        }

        private void ProcessTrackedDevice(ref MLInputDevice deviceState)
        {
            var eventData = _cachedTrackedPointerEventData;
            eventData.Reset();
            deviceState.CopyTo(ref eventData);

            eventData.button = PointerEventData.InputButton.Left;
            eventData.pointerCurrentRaycast = PerformRaycast(eventData);

            ProcessButton(deviceState.SelectDelta, ref eventData);
            ProcessMovement(eventData);
            ProcessScroll(eventData);

            deviceState.CopyFrom(eventData);
            deviceState.OnFrameFinished();
        }

        /// <summary>
        /// Adds the connected controller if not yet added.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        private void AddController(byte controllerId)
        {
            MLInputController newController = MLInput.GetController(controllerId);
            if (_controllers.Exists((device) => device.Id == controllerId))
            {
                Debug.LogWarning(string.Format("Connected controller with id {0} already connected.", controllerId));
                return;
            }

            if (newController.Connected)
            {
                _controllers.Add(newController);
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event when a controller connects. If the connected controller
        /// is valid, we add it to the _controllers list.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        private void HandleOnControllerConnected(byte controllerId)
        {
            AddController(controllerId);
        }

        /// <summary>
        /// Handles the event when a controller disconnects. If the disconnected
        /// controller happens to be in the _controllers list, we
        /// remove it from the list and invalidate the associated line.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        private void HandleOnControllerDisconnected(byte controllerId)
        {
            int controllerIndex = _controllers.FindIndex((device) => device.Id == controllerId);
            if(controllerIndex != -1)
            {
                _controllers.RemoveAt(controllerIndex);
            }
        }
        #endregion
    }
}
