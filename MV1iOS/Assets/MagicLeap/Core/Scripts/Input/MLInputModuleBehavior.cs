// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://auth.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// MLUIInputModule. Requires TrackedDeviceRaycaster on Canvas.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/Input/MLInputModuleBehavior")]
    public class MLInputModuleBehavior : BaseInputModule
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

        [SerializeField]
        private Canvas _mainCanvas = null;

        #pragma warning disable 414
        [SerializeField, Tooltip("The input that should be used as a pointer.")]
        private PointerInputType _pointerInput = PointerInputType.Controller;

        [SerializeField, Tooltip("The controller button that should register as a click event.")]
        private ButtonClickType _buttonClick = ButtonClickType.Trigger;

        [SerializeField, Range(1.0f, 10.0f), Tooltip("The scroll speed of content within a scroll view or navigation container.")]
        private float _scrollSpeed = 5.0f;

        [SerializeField, Range(0.1f, 3.0f), Tooltip("The maximum time (in seconds) between the press and release of the trigger for a click event to occur.")]
        private float _triggerClickSpeed = 3.0f;

        [SerializeField, Range(0, 0.99f), Tooltip("The threshold value the trigger must achieve before issuing a click event.")]
        private float _triggerDownThreshold = 0.25f;

        [SerializeField, Range(0.1f, 1.0f), Tooltip("The distance threshold value required before issuing a drag event.")]
        private float _dragThreshold = 0.025f;
        #pragma warning restore 414

        #if PLATFORM_LUMIN
        private MLInputDevice _pointerInputDevice;
        #endif

        private List<MLInput.Controller> _controllers = new List<MLInput.Controller>();

        #if PLATFORM_LUMIN
        // Used to interact with the UI elements.
        private MLInputDeviceEventData _cachedTrackedPointerEventData;
        #endif

        // Used to determine hit events.
        private Camera _mainCamera;

        private LineSegment _pointerLineSegment = new LineSegment();

        private Vector3 _previousRaycastPosition;

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

        /// <summary/>
        protected override void Start()
        {
            base.Start();

            if (_mainCanvas == null)
            {
                Debug.LogError("Error: MLInputModule, _mainCanvas field is empty, disabling script.");
                enabled = false;
                return;
            }

            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogError("Error: MLInputModule, no camera found tagged as MainCamera, disabling script.");
                enabled = false;
                return;
            }

            _mainCanvas.worldCamera = _mainCamera;

            SceneManager.activeSceneChanged += ChangedActiveScene;

            // Eye Tracking
            if (_pointerInput == PointerInputType.EyeTracking)
            {
                #if PLATFORM_LUMIN
                MLResult result = MLEyes.Start();
                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("Error: MLInputModule failed starting MLEyes, disabling script. Reason: {0}", result);
                    enabled = false;
                    return;
                }
                #endif
            }

            #if PLATFORM_LUMIN
            // Controllers
            if (!MLInput.IsStarted)
            {
                MLInput.Configuration config = new MLInput.Configuration(true);
                MLResult result = MLInput.Start(config);
                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("Error: MLInputModule failed starting MLInput, disabling script. Reason: {0}", result);
                    enabled = false;
                    return;
                }
            }
            #endif

            AddController(0);
            AddController(1);

            #if PLATFORM_LUMIN
            // Track connect / disconnect
            MLInput.OnControllerConnected += HandleOnControllerConnected;
            MLInput.OnControllerDisconnected += HandleOnControllerDisconnected;

            _pointerInputDevice = new MLInputDevice();
            _pointerInputDevice.Initialize();
            _cachedTrackedPointerEventData = new MLInputDeviceEventData(eventSystem);
            #endif
        }

        /// <summary/>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            #if PLATFORM_LUMIN
            if (MLInput.IsStarted)
            {
                MLInput.Stop();
            }

            MLInput.OnControllerConnected -= HandleOnControllerConnected;
            MLInput.OnControllerDisconnected -= HandleOnControllerDisconnected;
            SceneManager.activeSceneChanged -= ChangedActiveScene;

            if (MLEyes.IsStarted)
            {
                MLEyes.Stop();
            }
            #endif
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

        /// <summary>
        /// Process the input for headpose, eye tracking, controls and standard Unity Editor input (Keyboard, Mouse, Joystick).
        /// </summary>
        public override void Process()
        {
            switch (_pointerInput)
            {
                case PointerInputType.Headpose:
                {
                    #if PLATFORM_LUMIN
                    // Convert from Local position, to world position.
                    _pointerInputDevice.Position = transform.TransformPoint(_mainCamera.transform.position);
                    _pointerInputDevice.Orientation = _mainCamera.transform.rotation;
                    #endif

                    break;
                }
                case PointerInputType.EyeTracking:
                {
                    #if PLATFORM_LUMIN
                    // Convert from Local position, to world position.
                    _pointerInputDevice.Position = transform.TransformPoint(_mainCamera.transform.position);
                    if (MLEyes.IsStarted)
                    {
                        _pointerInputDevice.Orientation = Quaternion.LookRotation(MLEyes.FixationPoint - _mainCamera.transform.position);
                    }
                    #endif

                    break;
                }
                case PointerInputType.Controller:
                {
                    if (_controllers.Count > 0)
                    {
                        MLInput.Controller controller = _controllers[0];

                        #if PLATFORM_LUMIN
                        // Convert from Local position, to world position.
                        _pointerInputDevice.Position = transform.TransformPoint(controller.Position);
                        _pointerInputDevice.Orientation = controller.Orientation;
                        #endif
                    }
                    break;
                }
            }

            #if PLATFORM_LUMIN
            ProcessLine(_pointerLineSegment, ref _pointerInputDevice);

            if (_controllers.Count > 0)
            {
                MLInput.Controller controller = _controllers[0];
                _pointerInputDevice.Select = IsClicked(controller);
                _pointerInputDevice.UpdateTouch(controller, _scrollSpeed);
            }

            ProcessTrackedDevice(ref _pointerInputDevice);
            #endif
        }

        private bool IsClicked(MLInput.Controller controller)
        {
            #if PLATFORM_LUMIN
            if (controller != null && controller.Connected)
            {
                if (_buttonClick == ButtonClickType.Bumper)
                {
                    return controller.IsBumperDown;
                }

                if (_buttonClick == ButtonClickType.Trigger)
                {
                    return controller.TriggerValue >= _triggerDownThreshold;
                }
            }
            #endif

            return false;
        }

        #if PLATFORM_LUMIN
        private void ProcessLine(LineSegment line, ref MLInputDevice inputDevice)
        {
            Vector3? endPoint = null;
            Vector3 normal = Vector3.zero;

            MLInputDeviceEventData pointerEventData = _cachedTrackedPointerEventData;
            if (pointerEventData == null)
            {
                return;
            }

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
        #endif

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

        #if PLATFORM_LUMIN
        private void ProcessButton(ButtonDeltaState mouseButtonChanges, ref MLInputDeviceEventData eventData)
        {
            var currentOverGo = eventData.pointerCurrentRaycast.gameObject;

            if (mouseButtonChanges == ButtonDeltaState.Pressed)
            {
                eventData.eligibleForClick = true;
                eventData.dragging = false;
                eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;

                eventData.position = _mainCamera.WorldToScreenPoint(eventData.pointerCurrentRaycast.worldPosition);
                eventData.pressPosition = eventData.position;

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

                if (newPressed == eventData.lastPress && ((time - eventData.clickTime) < _triggerClickSpeed))
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
                var canPointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (canPointerDrag != null && (newPressed == null || canPointerDrag == newPressed))
                {
                    eventData.pointerDrag = canPointerDrag;
                    ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.initializePotentialDrag);
                }
            }
            else if (mouseButtonChanges == ButtonDeltaState.Released)
            {
                ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

                var clickHandlerGO = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                if (eventData.pointerPress == clickHandlerGO && eventData.eligibleForClick)
                {
                    ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerClickHandler);
                }

                if (eventData.dragging && eventData.pointerDrag != null)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, eventData, ExecuteEvents.dropHandler);
                    ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.endDragHandler);
                }

                eventData.eligibleForClick = eventData.dragging = false;
                eventData.pointerPress = eventData.rawPointerPress = eventData.pointerDrag = null;
            }
            else if(eventData.pointerPress != null)
            {
                // Check to see if we've dragged over a press handler
                var newPress = ExecuteEvents.GetEventHandler<IPointerDownHandler>(currentOverGo);

                // We didn't find a press handler, so we search for a click handler.
                if (newPress == null)
                {
                    newPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
                }

                // If we moved from our initial press object, process an up for that object.
                if (eventData.pointerPress != newPress)
                {
                    var pointerUpHandlerGO = ExecuteEvents.ExecuteHierarchy(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

                    if (pointerUpHandlerGO == null)
                    {
                        pointerUpHandlerGO = ExecuteEvents.GetEventHandler<IPointerUpHandler>(eventData.pointerPress);
                        ExecuteEvents.Execute(pointerUpHandlerGO, eventData, ExecuteEvents.pointerUpHandler);
                    }

                    eventData.eligibleForClick = false;
                    eventData.pointerPress = null;
                    eventData.rawPointerPress = null;
                }
            }
        }
        #endif

        private void ProcessButtonDrag(PointerEventData eventData, float pixelDragThresholdMultiplier = 1.0f)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            eventData.position = _mainCamera.WorldToScreenPoint(eventData.pointerCurrentRaycast.worldPosition);

            if (!eventData.dragging)
            {
                var dragStart = ExecuteEvents.ExecuteHierarchy(eventData.pointerDrag, eventData, ExecuteEvents.beginDragHandler);

                if (dragStart == null)
                {
                    dragStart = ExecuteEvents.GetEventHandler<IBeginDragHandler>(eventData.pointerDrag);
                    ExecuteEvents.Execute(dragStart, eventData, ExecuteEvents.beginDragHandler);
                }

                eventData.dragging = true;
                eventData.delta = Vector2.zero;
                _previousRaycastPosition = eventData.pointerCurrentRaycast.worldPosition;
            }

            if (eventData.dragging)
            {
                eventData.delta = eventData.position - (Vector2)_mainCamera.WorldToScreenPoint(_previousRaycastPosition);

                var newDrag = ExecuteEvents.ExecuteHierarchy(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);

                if (newDrag == null)
                {
                    newDrag = ExecuteEvents.GetEventHandler<IDragHandler>(eventData.pointerDrag);
                    ExecuteEvents.Execute(newDrag, eventData, ExecuteEvents.dragHandler);
                }

                _previousRaycastPosition = eventData.pointerCurrentRaycast.worldPosition;
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

        #if PLATFORM_LUMIN
        private void ProcessTrackedDevice(ref MLInputDevice deviceState)
        {
            var eventData = _cachedTrackedPointerEventData;
            if (eventData == null)
            {
                return;
            }

            eventData.Reset();
            deviceState.CopyTo(ref eventData);

            eventData.button = PointerEventData.InputButton.Left;
            eventData.pointerCurrentRaycast = PerformRaycast(eventData);

            ProcessButton(deviceState.SelectDelta, ref eventData);
            ProcessMovement(eventData);
            ProcessScroll(eventData);
            ProcessButtonDrag(eventData, _dragThreshold);

            deviceState.CopyFrom(eventData);
            deviceState.OnFrameFinished();
        }
        #endif

        /// <summary>
        /// Adds the connected controller if not yet added.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        private void AddController(byte controllerId)
        {
            #if PLATFORM_LUMIN
            MLInput.Controller newController = MLInput.GetController(controllerId);
            if (_controllers.Exists((device) => device.Id == controllerId))
            {
                Debug.LogWarning(string.Format("Connected controller with id {0} already connected.", controllerId));
                return;
            }

            if (newController.Connected)
            {
                _controllers.Add(newController);
            }
            #endif
        }

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
            #if PLATFORM_LUMIN
            int controllerIndex = _controllers.FindIndex((device) => device.Id == controllerId);
            if (controllerIndex != -1)
            {
                _controllers.RemoveAt(controllerIndex);
            }
            #endif
        }

        private void ChangedActiveScene(Scene current, Scene next)
        {
            if (_mainCanvas == null)
            {
                Debug.LogError("Error: MLInputModule, _mainCanvas field is empty, disabling script.");
                enabled = false;
                return;
            }

            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogError("Error: MLInputModule, no camera found tagged as MainCamera, disabling script.");
                enabled = false;
                return;
            }

            _mainCanvas.worldCamera = _mainCamera;
        }
    }
}
