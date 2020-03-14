// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLInput.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Collections.Generic;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Manages the input state for controllers, MCA and tablet devices.
    /// </summary>
    public partial class MLInput : MLAPISingleton<MLInput>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// The unofficial maximum amount of tablets allowed to be connected to the device at once.
        /// </summary>
        private const int UnofficialSupportedTablets = 2;

        /// <summary>
        /// Cached value of are we in the Editor for later use on shutdown
        /// </summary>
        private bool isInEditor = false;

        /// <summary>
        /// The internal handle attached to this instance of MLInput
        /// </summary>
        private ulong inputHandle = MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// The two controller objects currently being tracked.
        /// [0] = Left
        /// [1] = Right
        /// </summary>
        private MLInput.Controller[] controllers;

        /// <summary>
        /// The current pressed state of the controller triggers.
        /// </summary>
        private bool[] triggerPressed;

        /// <summary>
        /// The input configuration to send to MLInputCreate
        /// </summary>
        private Configuration config;

        /// <summary>
        /// The configuration for the controllers.
        /// </summary>
        private NativeBindings.MLControllerConfigurationNative controllerConfig;

        /// <summary>
        /// Stored a cached list of the device controllers.
        /// </summary>
        private List<InputDevice> cachedDevices = new List<InputDevice>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MLInput"/> class.
        /// </summary>
        /// <param name="config">An instance of the configuration file to apply.</param>
        private MLInput(Configuration config)
        {
            this.config = config;
            this.isInEditor = Application.isEditor;
        }

        /// <summary>
        /// A delegate for the touchpad gesture events.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        /// <param name="touchpadGesture">The touchpad gesture attached to this event.</param>
        public delegate void ControllerTouchpadGestureDelegate(byte controllerId, MLInput.Controller.TouchpadGesture touchpadGesture);

        /// <summary>
        /// A delegate for controller button events.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        /// <param name="button">The button attached to this event.</param>
        /// <summary/>
        public delegate void ControllerButtonDelegate(byte controllerId, Controller.Button button);

        /// <summary>
        /// A delegate for controller connect events.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        public delegate void ControllerConnectionDelegate(byte controllerId);

        /// <summary>
        /// A delegate for controller trigger events.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        /// <param name="triggerValue">The current amount of depression of the trigger from 0 to 1.</param>
        public delegate void TriggerDelegate(byte controllerId, float triggerValue);

        /// <summary>
        /// A delegate for pen touch events.
        /// </summary>
        /// <param name="tabletId">ID of the tablet device.</param>
        /// <param name="tabletState">State of the tablet device</param>
        public delegate void OnPenTouchDelegate(byte tabletId, TabletState tabletState);

        /// <summary>
        /// A delegate for tablet connect events.
        /// </summary>
        /// <param name="tabletId">ID of the tablet device.</param>
        public delegate void TabletConnectionDelegate(byte tabletId);

        /// <summary>
        /// A delegate for tablet ring touch events.
        /// </summary>
        /// <param name="tabletId">ID of the tablet device.</param>
        /// <param name="touchRingValue">Touch ring value. (0 - 71)</param>
        /// <param name="timestamp">Time at which this event occurred.</param>
        public delegate void OnRingTouchDelegate(byte tabletId, int touchRingValue, ulong timestamp);

        /// <summary>
        /// A delegate for tablet button events.
        /// </summary>
        /// <param name="tabletId">ID of the tablet device.</param>
        /// <param name="tabletButton">Value of the tablet device button.</param>
        /// <param name="timestamp">Time at which this event occurred.</param>
        public delegate void TabletButtonDelegate(byte tabletId, TabletDeviceButton tabletButton, ulong timestamp);

        /// <summary>
        /// A delegate for native controller button events.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        /// <param name="button">The controller button.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        private delegate void OnControllerButtonNativeCallbackPrivate(byte controllerId, Controller.Button button, System.IntPtr data);

        /// <summary>
        /// A delegate for native controller connect events.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        private delegate void OnControllerConnectNativeCallbackPrivate(byte controllerId);

        /// <summary>
        /// A delegate for native controller disconnect events.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        private delegate void OnControllerDisconnectNativeCallbackPrivate(byte controllerId);

        /// <summary>
        /// A delegate for native tablet pen touch events.
        /// </summary>
        /// <param name="tabletId">The id of the tablet.</param>
        /// <param name="tabletState">The state of the tablet.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        private delegate void OnTabletPenTouchNativeCallbackPrivate(byte tabletId, ref NativeBindings.TabletDeviceStateNative tabletState, System.IntPtr data);

        /// <summary>
        /// A delegate for native tablet ring touch events.
        /// </summary>
        /// <param name="tabletId">The id of the tablet.</param>
        /// <param name="touchRingValue">The touch ring value.</param>
        /// <param name="timeStamp">The time the button was pressed.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        private delegate void OnTabletRingTouchNativeCallbackPrivate(byte tabletId, int touchRingValue, ulong timeStamp, System.IntPtr data);

        /// <summary>
        /// A delegate for native tablet button down events.
        /// </summary>
        /// <param name="tabletId">The id of the tablet.</param>
        /// <param name="tabletButton">The tablet button that was pressed.</param>
        /// <param name="timeStamp">The time the button was pressed.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        private delegate void OnTabletButtonDownNativeCallbackPrivate(byte tabletId, TabletDeviceButton tabletButton, ulong timeStamp, System.IntPtr data);

        /// <summary>
        /// A delegate for native tablet button up events.
        /// </summary>
        /// <param name="tabletId">The id of the tablet.</param>
        /// <param name="tabletButton">The tablet button that was released.</param>
        /// <param name="timeStamp">The time the button was released.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        private delegate void OnTabletButtonUpNativeCallbackPrivate(byte tabletId, TabletDeviceButton tabletButton, ulong timeStamp, System.IntPtr data);

        /// <summary>
        /// A delegate for native tablet connect events.
        /// </summary>
        /// <param name="tabletId">The id of the tablet.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        private delegate void OnTabletConnectNativeCallbackPrivate(byte tabletId, System.IntPtr data);

        /// <summary>
        /// A delegate for native tablet disconnect events.
        /// </summary>
        /// <param name="tabletId">The id of the tablet.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        private delegate void OnTabletDisconnectNativeCallbackPrivate(byte tabletId, System.IntPtr data);

        /// <summary>
        /// This callback will be invoked whenever a detected touchpad gesture begins.
        /// </summary>
        public static event ControllerTouchpadGestureDelegate OnControllerTouchpadGestureStart = delegate { };

        /// <summary>
        /// This callback will be invoked as a detected touchpad gesture continues.
        /// </summary>
        public static event ControllerTouchpadGestureDelegate OnControllerTouchpadGestureContinue = delegate { };

        /// <summary>
        /// This callback will be invoked whenever a detected touchpad gesture ends.
        /// </summary>
        public static event ControllerTouchpadGestureDelegate OnControllerTouchpadGestureEnd = delegate { };

        /// <summary>
        /// This callback will be invoked whenever a button press is detected.
        /// </summary>
        public static event ControllerButtonDelegate OnControllerButtonDown = delegate { };

        /// <summary>
        /// This callback will be invoked whenever a button release is detected.
        /// </summary>
        public static event ControllerButtonDelegate OnControllerButtonUp = delegate { };

        /// <summary>
        /// This callback will be invoked when a controller is connected.
        /// </summary>
        public static event ControllerConnectionDelegate OnControllerConnected = delegate { };

        /// <summary>
        /// This callback will be invoked when a controller is disconnected.
        /// </summary>
        public static event ControllerConnectionDelegate OnControllerDisconnected = delegate { };

        /// <summary>
        /// This callback will be invoked as the trigger passes the TriggerDownThreshold.
        /// </summary>
        public static event TriggerDelegate OnTriggerDown = delegate { };

        /// <summary>
        /// This callback will be invoked as the trigger passes the TriggerUpThreshold.
        /// </summary>
        public static event TriggerDelegate OnTriggerUp = delegate { };

        /// <summary>
        /// This callback will be invoked whenever a pen touch event is detected.
        /// </summary>
        public static event OnPenTouchDelegate OnTabletPenTouch = delegate { };

        /// <summary>
        /// This callback will be invoked whenever a touch ring event is detected.
        /// touchRingValue for Wacom has 72 levels and goes from 0 to 71. Values are absolute, not
        /// relative to start position.
        /// </summary>
        public static event OnRingTouchDelegate OnTabletRingTouch = delegate { };

        /// <summary>
        /// This callback will be invoked whenever a tablet device button press is detected.
        /// </summary>
        public static event TabletButtonDelegate OnTabletButtonDown = delegate { };

        /// <summary>
        /// This callback will be invoked whenever a tablet device button release is detected.
        /// </summary>
        public static event TabletButtonDelegate OnTabletButtonUp = delegate { };

        /// <summary>
        /// This callback will be invoked whenever a tablet device is connected.
        /// </summary>
        public static event TabletConnectionDelegate OnTabletConnected = delegate { };

        /// <summary>
        /// This callback will be invoked whenever a tablet device is disconnected. This tabletID is no longer valid.
        /// </summary>
        public static event TabletConnectionDelegate OnTabletDisconnected = delegate { };
        #endif

        /// <summary>
        /// Standardized enumeration of handedness
        /// </summary>
        public enum Hand
        {
            /// <summary>
            /// The left hand.
            /// </summary>
            Left = 0,

            /// <summary>
            /// The right hand.
            /// </summary>
            Right
        }

        /// <summary>
        /// Types of input tablet devices recognized. Links to MLInputTabletDeviceType in ml_input.h.
        /// </summary>
        public enum TabletDeviceType : uint
        {
            /// <summary>
            /// Unknown tablet.
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Wacom tablet.
            /// </summary>
            Wacom = 1
        }

        /// <summary>
        /// Types of tools used with the tablet device. Links to MLInputTabletDeviceToolType in ml_input.h.
        /// </summary>
        public enum TabletDeviceToolType : uint
        {
            /// <summary>
            /// Unknown tool type.
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Pen tool type.
            /// </summary>
            Pen = 1,

            /// <summary>
            /// Eraser tool type.
            /// </summary>
            Eraser = 2
        }

        /// <summary>
        /// Buttons on input tablet device. Links to MLInputTabletDeviceButton in ml_input.h.
        /// </summary>
        public enum TabletDeviceButton : uint
        {
            /// <summary>
            /// An unknown tablet button.
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Tablet button 1.
            /// </summary>
            Button1,

            /// <summary>
            /// Tablet button 2.
            /// </summary>
            Button2,

            /// <summary>
            /// Tablet button 3.
            /// </summary>
            Button3,

            /// <summary>
            /// Tablet button 4.
            /// </summary>
            Button4,

            /// <summary>
            /// Tablet button 5.
            /// </summary>
            Button5,

            /// <summary>
            /// Tablet button 6.
            /// </summary>
            Button6,

            /// <summary>
            /// Tablet button 7.
            /// </summary>
            Button7,

            /// <summary>
            /// Tablet button 8.
            /// </summary>
            Button8,

            /// <summary>
            /// Tablet button 9.
            /// </summary>
            Button9,

            /// <summary>
            /// Tablet button 10.
            /// </summary>
            Button10,

            /// <summary>
            /// Tablet button 11.
            /// </summary>
            Button11,

            /// <summary>
            /// Tablet button 12.
            /// </summary>
            Button12
        }

        /// <summary>
        /// Mask value to determine the validity of variables in MLInputTabletDeviceState.
        /// Links to MLInputTabletDeviceStateMask in ml_input.h.
        /// </summary>
        [Flags]
        public enum TabletDeviceStateMask : uint
        {
            /// <summary>
            /// Type mask value.
            /// </summary>
            HasType = 1 << 0,

            /// <summary>
            /// ToolType mask value.
            /// </summary>
            HasToolType = 1 << 1,

            /// <summary>
            /// <c>PenTouchPosAndForce</c> mask value.
            /// </summary>
            HasPenTouchPosAndForce = 1 << 2,

            /// <summary>
            /// AdditionalPenTouchData mask value.
            /// </summary>
            HasAdditionalPenTouchData = 1 << 3,

            /// <summary>
            /// PenTouchActive mask value.
            /// </summary>
            HasPenTouchActive = 1 << 4,

            /// <summary>
            /// ConnectionState mask value.
            /// </summary>
            HasConnectionState = 1 << 5,

            /// <summary>
            /// PenDistance mask value.
            /// </summary>
            HasPenDistance = 1 << 6,

            /// <summary>
            /// TimeStamp mask value.
            /// </summary>
            HasTimeStamp = 1 << 7
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Gets a list of currently connected Tablet Devices. Will be empty if no Tablets are connected.
        /// Currently Unity is officially supporting one tablet connected, this setup will support
        /// more in the future when an official Maximum Supported Tablets is set.
        /// </summary>
        public static List<byte> TabletDevices
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the threshold used to determine when the trigger has been squeezed enough
        /// to be considered pressed.
        /// The trigger will remain in the pressed state until its reading goes below TriggerUpThreshold.
        /// TriggerDownThreshold must be larger than TriggerUpThreshold.
        /// </summary>
        public static float TriggerDownThreshold { get; set; }

        /// <summary>
        /// Gets or sets the threshold used to determine when the trigger has been released
        /// enough to be considered released.
        /// The trigger will remain in the released state until its reading goes above TriggerDownThreshold.
        /// TriggerUpThreshold must be smaller than TriggerDownThreshold.
        /// </summary>
        public static float TriggerUpThreshold { get; set; }

        /// <summary>
        /// List of MLInput.TabletStates since last query up to a max of 20 MLInput.TabletStates
        /// as specified in the CAPI. Will be empty if device has just been connected and no
        /// updates have happened yet or if a Tablet Pen is not actively touching or hovering above the touchpad.
        /// </summary>
        /// <param name="tabletId">The Id of the tablet you want to get the states for. Can be retrieved from TabletDevices.</param>
        /// <param name="tabletStates">Returns the list of MLInput.TabletStates since previous update for specific Tablet Device. Null for invalid tabletID or error on getting states.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully fetched the tablet device state.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        public static MLResult GetTabletStates(byte tabletId, out TabletState[] tabletStates)
        {
            tabletStates = null;

            if (!MLInput.IsValidInstance())
            {
                MLPluginLog.ErrorFormat("MLInput.GetTabletStates failed. Reason: No Instance for MLInput");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLInput.GetTabletStates failed. Reason: No Instance for MLInput");
            }

            if (TabletDevices.Contains(tabletId))
            {
                NativeBindings.TabletDeviceStatesListNative currentTabletStates = NativeBindings.TabletDeviceStatesListNative.Create();

                MLResult.Code resultCode = NativeBindings.MLInputGetTabletDeviceStates(_instance.inputHandle, tabletId, ref currentTabletStates);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLInput.GetTabletStates failed to get tablet device states for tabletId {0}. Reason: {1}", tabletId, MLResult.CodeToString(resultCode));
                }
                else
                {
                    tabletStates = NativeBindings.DeviceStatesToArray(currentTabletStates);

                    resultCode = NativeBindings.MLInputReleaseTabletDeviceStates(_instance.inputHandle, ref currentTabletStates);
                    if (!MLResult.IsOK(resultCode))
                    {
                        MLPluginLog.ErrorFormat("MLInput.GetTabletStates failed to release tablet device states list for tabletId {0}. Reason: {1}", tabletId, MLResult.CodeToString(resultCode));
                    }
                }

                return MLResult.Create(resultCode);
            }
            else
            {
                MLPluginLog.ErrorFormat("MLInput.GetTabletStates failed to get tablet device states for tabletId {0}. Reason: Invalid tabletId", tabletId);
                return MLResult.Create(MLResult.Code.InvalidParam);
            }
        }

        /// <summary>
        /// Starts the Input API.
        /// </summary>
        /// <param name="config">The configuration applied to input upon initialization.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        public static MLResult Start(Configuration config = null)
        {
            if (config == null)
            {
                config = new MagicLeap.MLInput.Configuration(true);
            }

            CreateInstance(config);

            return MLInput.BaseStart(true);
        }

        /// <summary>
        /// Gets a reference to a Controller object via hand mapping
        /// </summary>
        /// <param name="hand">The hand to check for a controller.</param>
        /// <returns> The first MLInput.Controller mapped to the specified hand. </returns>
        public static MLInput.Controller GetController(Hand hand)
        {
            if (MLInput.IsValidInstance())
            {
                for (int i = 0; i < _instance.controllers.Length; ++i)
                {
                    if (_instance.controllers[i].Hand == hand)
                    {
                        return _instance.controllers[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a reference to a Controller object via index.
        /// </summary>
        /// <param name="controllerIndex">The index of an active controller.</param>
        /// <returns>MLInput.Controller corresponding to the specified index.</returns>
        public static MLInput.Controller GetController(int controllerIndex)
        {
            if (MLInput.IsValidInstance())
            {
                if (controllerIndex >= 0 && controllerIndex < _instance.controllers.Length)
                {
                    return _instance.controllers[controllerIndex];
                }
            }

            return null;
        }

        /// <summary>
        /// Maps a controller reference to a specific hand
        /// </summary>
        /// <param name="controllerIndex">The controller index.</param>
        /// <param name="hand">The hand to assign to the controller index.</param>
        public static void SetControllerHand(int controllerIndex, Hand hand)
        {
            if (MLInput.IsValidInstance())
            {
                if (controllerIndex >= 0 && controllerIndex < _instance.controllers.Length)
                {
                    _instance.controllers[controllerIndex].Hand = hand;
                }
            }
        }

        /// <summary>
        /// Retrieves a controller index mapped to a specific hand
        /// </summary>
        /// <param name="hand">The hand to check for a controller.</param>
        /// <returns> Index of controller mapped to the specified hand. </returns>
        public static int GetControllerIndexFromHand(Hand hand)
        {
            if (MLInput.IsValidInstance())
            {
                for (int i = 0; i < _instance.controllers.Length; ++i)
                {
                    if (_instance.controllers[i].Hand == hand)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Retrieves the hand mapped to the specified controller index
        /// </summary>
        /// /// <param name="controllerIndex">The index of an active controller.</param>
        /// <returns> Hand corresponding to the specified index. </returns>
        public static Hand GetHandFromControllerIndex(int controllerIndex)
        {
            if (!MLInput.IsValidInstance())
            {
                MLPluginLog.ErrorFormat("MLInput.GetHandFromControllerIndex failed. Reason: No Instance for MLInput");

                // return the default hand.
                return Hand.Left;
            }

            if (controllerIndex >= 0 && controllerIndex < _instance.controllers.Length)
            {
                return _instance.controllers[controllerIndex].Hand;
            }

            return _instance.controllers[0].Hand;
        }

        /// <summary>
        /// Cleans up this input tracker and unsubscribes from the MagicLeap device's update loop.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Whether it is safe for the cleanup routine to access managed objects.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            // Disable controller gesture subsystem and tracker.
            NativeBindings.SetControllerGesturesEnabled(false);
            NativeBindings.SetControllerTrackerActive(false);

            this.DestroyNativeTracker();
            this.CleanupStaticEvents();
        }

#if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Start the Input API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed to obtain transform due to internal error.
        /// </returns>
        protected override MLResult StartAPI()
        {
            // Default configuration, MLInput no longer handles 6DOF.
            MLResult.Code resultCode = NativeBindings.MLInputCreate(IntPtr.Zero, ref this.inputHandle);
            if (!MLResult.IsOK(resultCode))
            {
                // TODO: Return failed result
                MLPluginLog.ErrorFormat("MLInput.StartAPI failed to create input tracker. Reason: {0}", MLResult.CodeToString(resultCode));
            }

            if (!MagicLeapNativeBindings.MLHandleIsValid(this.inputHandle))
            {
                MLPluginLog.Error("MLInput.StartAPI failed. Reason: Invalid handle returned when initializing an instance of MLInput.");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure);
            }

            // catch already connected devices and trigger connect event
            TabletDevices = new List<byte>();

            NativeBindings.ConnectedDevicesListNative tabletList = NativeBindings.ConnectedDevicesListNative.Create();

            resultCode = NativeBindings.MLInputGetConnectedDevices(this.inputHandle, ref tabletList);
            if (resultCode == MLResult.Code.NotImplemented)
            {
                MLPluginLog.WarningFormat("MLInput.StartAPI failed to get connected devices. Reason: {0}", MLResult.CodeToString(resultCode));
            }
            else if (!MLResult.IsOK(resultCode))
            {
                MLPluginLog.ErrorFormat("MLInput.StartAPI failed to get connected devices. Reason: {0}", MLResult.CodeToString(resultCode));
            }
            else
            {
                byte[] connectedTablets = NativeBindings.GetConnectedTabletIds(tabletList);
                for (int i = 0; i < connectedTablets.Length; i++)
                {
                    OnTabletConnectNative(connectedTablets[i], System.IntPtr.Zero);
                }

                resultCode = NativeBindings.MLInputReleaseConnectedDevicesList(this.inputHandle, ref tabletList);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLInput.StartAPI failed to release connected devices lists. Reason: {0}", MLResult.CodeToString(resultCode));
                }
            }

            this.InitControllers();
            return this.InitNativeCallbacks();
        }
#endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Update the controllers and process input.
        /// </summary>
        protected override void Update()
        {
            this.PollState();
            this.ProcessTriggerReadings();
        }

        /// <summary>
        /// static instance of the MLInput class
        /// </summary>
        /// <param name="config">An instance of the configuration file to apply.</param>
        private static void CreateInstance(Configuration config)
        {
            if (!MLInput.IsValidInstance())
            {
                MLInput._instance = new MLInput(config);
            }
        }

        /// <summary>
        /// Dispatches an event for controller touchpad gesture start.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        /// <param name="touchpadGesture">The touchpad gesture.</param>
        private static void OnControllerTouchpadGestureStartNative(byte controllerId, MLInput.Controller.TouchpadGesture touchpadGesture)
        {
            if (MLInput.IsValidInstance())
            {
                // Create an instance of MLInput.Controller.TouchpadGesture, since we queue it.
                var unityTouchpadGesture = new MLInput.Controller.TouchpadGesture(touchpadGesture);
                MLThreadDispatch.Call(controllerId, unityTouchpadGesture, OnControllerTouchpadGestureStart);
            }
        }

        /// <summary>
        /// Dispatches an event for controller touchpad gesture continue.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        /// <param name="touchpadGesture">The touchpad gesture.</param>
        private static void OnControllerTouchpadGestureContinueNative(byte controllerId, MLInput.Controller.TouchpadGesture touchpadGesture)
        {
            if (MLInput.IsValidInstance())
            {
                // Create an instance of MLInput.Controller.TouchpadGesture, since we queue it.
                var unityTouchpadGesture = new MLInput.Controller.TouchpadGesture(touchpadGesture);
                MLThreadDispatch.Call(controllerId, unityTouchpadGesture, OnControllerTouchpadGestureContinue);
            }
        }

        /// <summary>
        /// Dispatches an event for controller touchpad gesture end.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        /// <param name="touchpadGesture">The touchpad gesture.</param>
        private static void OnControllerTouchpadGestureEndNative(byte controllerId, MLInput.Controller.TouchpadGesture touchpadGesture)
        {
            if (MLInput.IsValidInstance())
            {
                // Create an instance of MLInput.Controller.TouchpadGesture, since we queue it.
                var unityTouchpadGesture = new MLInput.Controller.TouchpadGesture(touchpadGesture);
                MLThreadDispatch.Call(controllerId, unityTouchpadGesture, OnControllerTouchpadGestureEnd);
            }
        }

        /// <summary>
        /// Dispatches an event for controller button down.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        [AOT.MonoPInvokeCallback(typeof(OnControllerButtonNativeCallbackPrivate))]
        private static void OnControllerButtonDownNative(byte controllerId, Controller.Button button)
        {
            if (MLInput.IsValidInstance())
            {
                MLThreadDispatch.Call(controllerId, button, OnControllerButtonDown);
            }
        }

        /// <summary>
        /// Dispatches an event for controller button up.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        [AOT.MonoPInvokeCallback(typeof(OnControllerButtonNativeCallbackPrivate))]
        private static void OnControllerButtonUpNative(byte controllerId, Controller.Button button)
        {
            if (MLInput.IsValidInstance())
            {
                MLThreadDispatch.Call(controllerId, button, OnControllerButtonUp);
            }
        }

        /// <summary>
        /// Dispatches an event for controller connect.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        [AOT.MonoPInvokeCallback(typeof(OnControllerConnectNativeCallbackPrivate))]
        private static void OnControllerConnectNative(byte controllerId)
        {
            if (MLInput.IsValidInstance())
            {
                MLThreadDispatch.Call(controllerId, OnControllerConnected);
            }
        }

        /// <summary>
        /// Dispatches an event for controller disconnect.
        /// </summary>
        /// <param name="controllerId">The zero-based index of the controller.</param>
        [AOT.MonoPInvokeCallback(typeof(OnControllerDisconnectNativeCallbackPrivate))]
        private static void OnControllerDisconnectNative(byte controllerId)
        {
            if (MLInput.IsValidInstance())
            {
                MLThreadDispatch.Call(controllerId, OnControllerDisconnected);
            }
        }

        /// <summary>
        /// Dispatches an event for tablet pen touch.
        /// </summary>
        /// <param name="tabletId">The id of the tablet.</param>
        /// <param name="tabletState">The state of the tablet.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        [AOT.MonoPInvokeCallback(typeof(OnTabletPenTouchNativeCallbackPrivate))]
        private static void OnTabletPenTouchNative(byte tabletId, ref NativeBindings.TabletDeviceStateNative tabletState, System.IntPtr data)
        {
            if (MLInput.IsValidInstance())
            {
                if (TabletDevices.Contains(tabletId))
                {
                    TabletState newState = tabletState.Data;
                    MLThreadDispatch.Call(tabletId, newState, OnTabletPenTouch);
                }
                else
                {
                    MLPluginLog.Warning("MLInput.OnTabletPenTouch event recieved from unregistered tablet device.");
                }
            }
        }

        /// <summary>
        /// Dispatches an event for tablet ring touch.
        /// </summary>
        /// <param name="tabletId">The tablet id.</param>
        /// <param name="touchRingValue">The touch ring value.</param>
        /// <param name="timeStamp">The time the touch occurred.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        [AOT.MonoPInvokeCallback(typeof(OnTabletRingTouchNativeCallbackPrivate))]
        private static void OnTabletRingTouchNative(byte tabletId, int touchRingValue, ulong timeStamp, System.IntPtr data)
        {
            if (MLInput.IsValidInstance())
            {
                if (TabletDevices.Contains(tabletId))
                {
                    MLThreadDispatch.Call(tabletId, touchRingValue, timeStamp, OnTabletRingTouch);
                }
                else
                {
                    MLPluginLog.Warning("MLInput.OnTabletRingTouch event recieved from unregistered tablet device.");
                }
            }
        }

        /// <summary>
        /// Dispatches an event for tablet button down.
        /// </summary>
        /// <param name="tabletId">The tablet id.</param>
        /// <param name="tabletButton">The button being pressed.</param>
        /// <param name="timeStamp">The time the press occurred.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        [AOT.MonoPInvokeCallback(typeof(OnTabletButtonDownNativeCallbackPrivate))]
        private static void OnTabletButtonDownNative(byte tabletId, TabletDeviceButton tabletButton, ulong timeStamp, System.IntPtr data)
        {
            if (MLInput.IsValidInstance())
            {
                if (TabletDevices.Contains(tabletId))
                {
                    MLThreadDispatch.Call(tabletId, tabletButton, timeStamp, OnTabletButtonDown);
                }
                else
                {
                    MLPluginLog.Warning("MLInput.OnTabletButtonDown event recieved from unregistered tablet device.");
                }
            }
        }

        /// <summary>
        /// Dispatches an event for tablet button up.
        /// </summary>
        /// <param name="tabletId">The tablet id.</param>
        /// <param name="tabletButton">The tablet button that was released.</param>
        /// <param name="timeStamp">The time the release occurred.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        [AOT.MonoPInvokeCallback(typeof(OnTabletButtonUpNativeCallbackPrivate))]
        private static void OnTabletButtonUpNative(byte tabletId, TabletDeviceButton tabletButton, ulong timeStamp, System.IntPtr data)
        {
            if (MLInput.IsValidInstance())
            {
                if (TabletDevices.Contains(tabletId))
                {
                    MLThreadDispatch.Call(tabletId, tabletButton, timeStamp, OnTabletButtonUp);
                }
                else
                {
                    MLPluginLog.Warning("MLInput.OnTabletButtonUp event recieved from unregistered tablet device.");
                }
            }
        }

        /// <summary>
        /// Dispatches an event for tablet connect.
        /// </summary>
        /// <param name="tabletId">The id of the tablet.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        [AOT.MonoPInvokeCallback(typeof(OnTabletConnectNativeCallbackPrivate))]
        private static void OnTabletConnectNative(byte tabletId, System.IntPtr data)
        {
            if (MLInput.IsValidInstance())
            {
                bool keyFound = TabletDevices.Contains(tabletId);
                if (!keyFound && (TabletDevices.Count < UnofficialSupportedTablets))
                {
                    MLThreadDispatch.Call(tabletId, OnTabletConnected);

                    TabletDevices.Add(tabletId);
                }
                else if (keyFound)
                {
                    MLPluginLog.Warning("MLInput.OnTabletConnected event getting called for already connected tablet device.");
                }
                else
                {
                    MLPluginLog.Warning("MLInput.OnTabletConnected event unable to register new tablet connection. Reason: maximum tablet connections has been reached.");
                }
            }
        }

        /// <summary>
        /// Dispatches an event for tablet disconnect.
        /// </summary>
        /// <param name="tabletId">The id of the tablet.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        [AOT.MonoPInvokeCallback(typeof(OnTabletDisconnectNativeCallbackPrivate))]
        private static void OnTabletDisconnectNative(byte tabletId, System.IntPtr data)
        {
            if (MLInput.IsValidInstance())
            {
                if (TabletDevices.Contains(tabletId))
                {
                    MLThreadDispatch.Call(tabletId, OnTabletDisconnected);
                    TabletDevices.Remove(tabletId);
                }
                else
                {
                    MLPluginLog.Warning("MLInput.OnTabletDisconnected event getting called for not currently registered tablet device.");
                }
            }
        }

        /// <summary>
        /// Initialize the controls.
        /// </summary>
        private void InitControllers()
        {
            // Default is to map controller 0 to left hand and controller 1 to right hand
            this.controllers = new MLInput.Controller[Enum.GetNames(typeof(Hand)).Length];
            this.controllers[0] = new MLInput.Controller(this.inputHandle, 0, Hand.Left);
            this.controllers[1] = new MLInput.Controller(this.inputHandle, 1, Hand.Right);

            if (this.config.EnableCFUIDTracking)
            {
                // Enable 6DOF (ControllerPose) for the controller.
                NativeBindings.SetControllerTrackerActive(true);
                if (NativeBindings.GetControllerTrackerActive())
                {
                    // Apply Configuration
                    this.SetControllerConfiguration();
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLInput.InitControllers failed to initialize controller tracker. Reason: {0}", MLResult.CodeToString(MLResult.Code.UnspecifiedFailure));
                }
            }

            // Enable Controller Gestures
            NativeBindings.SetControllerGesturesEnabled(true);
            if (!NativeBindings.IsControllerGesturesEnabled())
            {
                MLPluginLog.ErrorFormat("MLInput.InitControllers failed to enable controller gestures. Reason: {0}", MLResult.CodeToString(MLResult.Code.UnspecifiedFailure));
            }

            // Register Callbacks
            for (int i = 0; i < this.controllers.Length; i++)
            {
                this.controllers[i].OnConnect += OnControllerConnectNative;
                this.controllers[i].OnDisconnect += OnControllerDisconnectNative;
                this.controllers[i].OnButtonDown += OnControllerButtonDownNative;
                this.controllers[i].OnButtonUp += OnControllerButtonUpNative;

                // Touchpad Events
                this.controllers[i].OnTouchpadGestureStart += OnControllerTouchpadGestureStartNative;
                this.controllers[i].OnTouchpadGestureContinue += OnControllerTouchpadGestureContinueNative;
                this.controllers[i].OnTouchpadGestureEnd += OnControllerTouchpadGestureEndNative;
            }

            this.triggerPressed = new bool[this.controllers.Length];

            if (this.config == null)
            {
                this.config = new Configuration(true);
            }

            TriggerDownThreshold = this.config.TriggerDownThreshold;
            TriggerUpThreshold = this.config.TriggerUpThreshold;
        }

        /// <summary>
        /// Sets the controller configuration.
        /// </summary>
        private void SetControllerConfiguration()
        {
            this.controllerConfig = new NativeBindings.MLControllerConfigurationNative
            {
                IMU3DOF = false,
                EM6DOF = false,
                Fused6DOF = true
            };

            NativeBindings.UpdateConfiguration(ref this.controllerConfig);
        }

        /// <summary>
        /// Assign the native callbacks and begin listening for events.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        private MLResult InitNativeCallbacks()
        {
            NativeBindings.TabletDeviceCallbacksNative tabletCallbacks = NativeBindings.TabletDeviceCallbacksNative.Create();
            tabletCallbacks.OnPenTouchEvent = OnTabletPenTouchNative;
            tabletCallbacks.OnTouchRingEvent = OnTabletRingTouchNative;
            tabletCallbacks.OnButtonDown = OnTabletButtonDownNative;
            tabletCallbacks.OnButtonUp = OnTabletButtonUpNative;
            tabletCallbacks.OnConnect = OnTabletConnectNative;
            tabletCallbacks.OnDisconnect = OnTabletDisconnectNative;

            MLResult.Code resultCode = NativeBindings.MLInputSetTabletDeviceCallbacks(this.inputHandle, ref tabletCallbacks, IntPtr.Zero);
            if (resultCode == MLResult.Code.NotImplemented)
            {
                MLPluginLog.WarningFormat("MLInput.InitNativeCallbacks failed to set tablet callbacks. Reason: {0}", MLResult.CodeToString(resultCode));

                #if UNITY_EDITOR
                resultCode = MLResult.Code.Ok;
                #endif
            }
            else if (!MLResult.IsOK(resultCode))
            {
                MLPluginLog.ErrorFormat("MLInput.InitNativeCallbacks failed to set tablet callbacks. Reason: {0}", MLResult.CodeToString(resultCode));
            }

            return MLResult.Create(resultCode);
        }

        /// <summary>
        /// Poll all available controllers.
        /// </summary>
        private void PollState()
        {
            List<InputDevice> devices = new List<InputDevice>();

            #if UNITY_2019_3_OR_NEWER
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand, devices);
            #else
            InputDevices.GetDevicesAtXRNode(XRNode.GameController, devices);
            #endif

            foreach (var device in devices)
            {
                // If the device doesn't exist yet, cache it.
                if (!this.cachedDevices.Contains(device))
                {
                    this.cachedDevices.Add(device);
                }
            }

            int i = 0; // Used to filter out non MagicLeapControllers.
            foreach (var device in this.cachedDevices)
            {
                // Controller/EyeTracking - use the same definition, so we must check.
                if (device.name.Replace(" ", string.Empty) == "MagicLeapController")
                {
                    // isValid is checked within the ControllerUpdate.
                    this.controllers[i].Update(device);
                    i++;
                }

                // TODO: Use this in the future once InputFeatureUsage struct casting is supported
                //_deviceUsages.Clear();
                //if(device.TryGetFeatureUsages(_deviceUsages) && _deviceUsages.Contains(MagicLeapControllerUsages.ControllerType))
                //{
                    // isValid is checked within the ControllerUpdate.
                //    _controllers[i].Update(device);
                //    i++;
                //}
            }
        }

        /// <summary>
        /// Check threshold values and issue events for the trigger.
        /// </summary>
        private void ProcessTriggerReadings()
        {
            for (byte i = 0; i < this.controllers.Length; ++i)
            {
                float triggerReading = this.controllers[i].TriggerValue;
                if (this.triggerPressed[i])
                {
                    if (triggerReading <= TriggerUpThreshold)
                    {
                        this.triggerPressed[i] = false;
                        MLInput.OnTriggerUp(i, triggerReading);
                    }
                }
                else
                {
                    if (triggerReading >= TriggerDownThreshold)
                    {
                        this.triggerPressed[i] = true;
                        MLInput.OnTriggerDown(i, triggerReading);
                    }
                }
            }
        }

        /// <summary>
        /// Destroy the native tracker.
        /// </summary>
        private void DestroyNativeTracker()
        {
            if (!MagicLeapNativeBindings.MLHandleIsValid(this.inputHandle))
            {
                return;
            }

            // Clear out the native event connections before we destroy the tracker
            this.CleanupNativeCallbacks();

            // Use the cached value of Application.isEditor here to avoid a crash if this logic is reached
            // from the destructor upon editor close.
            this.inputHandle = MLInputNativeBindings.InvalidHandle;
        }

        /// <summary>
        /// Clear the event listeners.
        /// </summary>
        private void CleanupStaticEvents()
        {
            OnControllerTouchpadGestureStart = delegate { };
            OnControllerTouchpadGestureContinue = delegate { };
            OnControllerTouchpadGestureEnd = delegate { };

            OnControllerButtonDown = delegate { };
            OnControllerButtonUp = delegate { };
            OnControllerConnected = delegate { };
            OnControllerDisconnected = delegate { };
            OnTriggerDown = delegate { };
            OnTriggerUp = delegate { };

            OnTabletPenTouch = delegate { };
            OnTabletRingTouch = delegate { };
            OnTabletButtonDown = delegate { };
            OnTabletButtonUp = delegate { };
            OnTabletConnected = delegate { };
            OnTabletDisconnected = delegate { };
        }

        /// <summary>
        /// Clear the native callbacks.
        /// </summary>
        private void CleanupNativeCallbacks()
        {
            NativeBindings.TabletDeviceCallbacksNative tabletCallbacks = NativeBindings.TabletDeviceCallbacksNative.Create();

            MLResult.Code resultCode = NativeBindings.MLInputSetTabletDeviceCallbacks(this.inputHandle, ref tabletCallbacks, IntPtr.Zero);
            if (resultCode == MLResult.Code.NotImplemented)
            {
                MLPluginLog.WarningFormat("MLInput.CleanupNativeCallbacks failed to unset tablet callbacks. Reason: {0}", MLResult.CodeToString(resultCode));
            }
            else if (!MLResult.IsOK(resultCode))
            {
                MLPluginLog.ErrorFormat("MLInput.CleanupNativeCallbacks failed to unset tablet callbacks. Reason: {0}", MLResult.CodeToString(resultCode));
            }
        }

        /// <summary>
        /// A structure containing information about the state of the tablet device.
        /// </summary>
        public struct TabletState
        {
            /// <summary>
            /// Type of this tablet device.
            /// </summary>
            public MLInput.TabletDeviceType Type;

            /// <summary>
            /// Type of tool used with the tablet.
            /// </summary>
            public MLInput.TabletDeviceToolType ToolType;

            /// <summary>
            /// Current touch position (x,y) and force (z).
            /// Position is in the [-1.0,1.0] range and force is in the [0.0,1.0] range.
            /// </summary>
            public Vector3 PenTouchPosAndForce;

            /// <summary>
            /// Additional coordinate values (x, y, z)
            /// It could contain data specific to the device type.
            /// AdditionalPenTouchData for Wacom holds pen tilt data (x, y), in degrees from -64 to 64. Straight up an down is 0.
            /// </summary>
            public int[] AdditionalPenTouchData;

            /// <summary>
            /// Is touch active.
            /// </summary>
            public bool IsPenTouchActive;

            /// <summary>
            /// If this tablet is connected.
            /// </summary>
            public bool IsConnected;

            /// <summary>
            /// Distance between pen and tablet.
            /// </summary>
            public float PenDistance;

            /// <summary>
            /// Time stamp of the event.
            /// </summary>
            public ulong TimeStamp;

            /// <summary>
            /// Used to determine what data in this structure is valid.
            /// Example: Before using AdditionalPenTouchData check if that variable is valid in the ValidityCheck.
            /// </summary>
            public TabletDeviceStateMask ValidityCheck;

            /// <summary>
            /// Override the ToString. Does not print the ValidityCheck.
            /// </summary>
            /// <returns>A string with the tablet information.</returns>
            public override string ToString()
            {
                string penTouch = string.Format("({0},{1},{2})", this.PenTouchPosAndForce.x, this.PenTouchPosAndForce.y, this.PenTouchPosAndForce.z);
                string penData = string.Format("({0},{1},{2})", this.AdditionalPenTouchData[0], this.AdditionalPenTouchData[1], this.AdditionalPenTouchData[2]);
                return string.Format("Tablet Device Type: {0} \nTool Type: {1} \nPen Position and Force: {2} \nAdditional Pen Touch Data: {3} \nIs Pen Touching: {4} \nIs Tablet Connected: {5} \nPen Distance: {6} \nTimeStamp: {7}", this.Type, this.ToolType, penTouch, penData, this.IsPenTouchActive, this.IsConnected, this.PenDistance, this.TimeStamp);
            }
        }

        /// <summary>
        /// The configuration class for MLInput.
        /// Updating settings requires a stop/start of the input system, which is done automatically.
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// The default trigger reading threshold for emitting OnTriggerDown.
            /// </summary>
            public const float DefaultTriggerDownThreshold = 0.8f;

            /// <summary>
            /// The default trigger reading threshold for emitting OnTriggerUp.
            /// </summary>
            public const float DefaultTriggerUpThreshold = 0.2f;

            /// <summary>
            /// The default value for CFUID tracking if not specified.
            /// </summary>
            private const bool DefaultCFUIDTrackingEnabled = true;

            /// <summary>
            /// Initializes a new instance of the <see cref="Configuration"/> class.
            /// </summary>
            /// <param name="enableCFUIDTracking">A flag that indicates if 6DOF will be used.</param>
            /// <param name="triggerDownThreshold">The threshold before the trigger down event occurs.</param>
            /// <param name="triggerUpThreshold">The threshold before the trigger up event occurs.</param>
            public Configuration(
                bool enableCFUIDTracking,
                float triggerDownThreshold = DefaultTriggerDownThreshold,
                float triggerUpThreshold = DefaultTriggerUpThreshold)
            {
                this.Dof = new MLInput.Controller.ControlDof[NativeBindings.MaxControllers];

                this.EnableCFUIDTracking = enableCFUIDTracking;
                this.TriggerDownThreshold = triggerDownThreshold;
                this.TriggerUpThreshold = triggerUpThreshold;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the (Coordinate Frame Unique ID) based tracking should be initialized or not.
            /// </summary>
            public bool EnableCFUIDTracking { get; set; } = DefaultCFUIDTrackingEnabled;

            /// <summary>
            /// Gets the degrees-of-freedom mode for the control.
            /// </summary>
            public Controller.ControlDof[] Dof { get; private set; }

            /// <summary>
            /// Gets the trigger reading threshold for emitting OnTriggerUp.
            /// </summary>
            public float TriggerDownThreshold { get; private set; }

            /// <summary>
            /// Gets the trigger reading threshold for emitting OnTriggerUp.
            /// </summary>
            public float TriggerUpThreshold { get; private set; }
        }
        #endif
    }
}
