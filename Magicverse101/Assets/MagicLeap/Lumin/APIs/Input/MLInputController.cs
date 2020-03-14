// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLInputController.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using UnityEngine.XR.InteractionSubsystems;

    #if PLATFORM_LUMIN

    #if !UNITY_2019_3_OR_NEWER
    using UnityEngine.Experimental.XR.MagicLeap;
    #endif

    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Manages the input state for controllers, MCA and tablet devices.
    /// </summary>
    public partial class MLInput : MLAPISingleton<MLInput>
    {
        /// <summary>
        /// Contains state information for a controller.
        /// </summary>
        public partial class Controller
        {
            #if PLATFORM_LUMIN
            /// <summary>
            /// A handle to the input tracker.
            /// </summary>
            private ulong inputTrackerHandle = MagicLeapNativeBindings.InvalidHandle;

            /// <summary>
            /// The byte id of the controller.
            /// 0 - Controller 1,
            /// 1 - Controller 2,
            /// 255 - MCA
            /// </summary>
            private byte controllerId;

            /// <summary>
            /// Controller native transform.
            /// </summary>
            private MagicLeapNativeBindings.MLTransform transform = MagicLeapNativeBindings.MLTransform.Identity();

            /// <summary>
            /// Used to invoke an event, when pressed/released.
            /// </summary>
            private bool isMenuButtonDown = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="Controller"/> class.
            /// </summary>
            /// <param name="inputTrackerHandle">The input tracker handle.</param>
            /// <param name="controllerId">The controller id.</param>
            /// <param name="inputHand">The hand that should be assigned.</param>
            public Controller(ulong inputTrackerHandle, byte controllerId, MLInput.Hand inputHand)
            {
                // InputTracker is still required, for LED Patterns & Control Vibrations.
                this.inputTrackerHandle = inputTrackerHandle;
                this.controllerId = controllerId;

                this.Hand = inputHand;
                this.CurrentTouchpadGesture = new MLInput.Controller.TouchpadGesture();

                // Start the Unity gesture subsystem.
                MLDevice.RegisterGestureSubsystem();
                if (MLDevice.GestureSubsystem != null)
                {
                    MLDevice.GestureSubsystem.onTouchpadGestureChanged += this.HandleOnTouchpadGestureChanged;
                }
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="Controller"/> class.
            /// </summary>
            ~Controller()
            {
                if (MLDevice.GestureSubsystem != null)
                {
                    MLDevice.GestureSubsystem.onTouchpadGestureChanged -= this.HandleOnTouchpadGestureChanged;
                    MLDevice.UnregisterGestureSubsystem();
                }
            }

            /// <summary>
            /// This event will be invoked whenever a detected touchpad gesture begins.
            /// </summary>
            public event MLInput.ControllerTouchpadGestureDelegate OnTouchpadGestureStart;

            /// <summary>
            /// This event will be invoked as a detected touchpad gesture continues.
            /// </summary>
            public event MLInput.ControllerTouchpadGestureDelegate OnTouchpadGestureContinue;

            /// <summary>
            /// This event will be invoked whenever a detected touchpad gesture ends.
            /// </summary>
            public event MLInput.ControllerTouchpadGestureDelegate OnTouchpadGestureEnd;

            /// <summary>
            /// This event will be invoked whenever a button press is detected.
            /// </summary>
            public event MLInput.ControllerButtonDelegate OnButtonDown;

            /// <summary>
            /// This event will be invoked whenever a button release is detected.
            /// </summary>
            public event MLInput.ControllerButtonDelegate OnButtonUp;

            /// <summary>
            /// This event will be invoked when a controller is connected.
            /// </summary>
            public event MLInput.ControllerConnectionDelegate OnConnect;

            /// <summary>
            /// This event will be invoked when a controller is disconnected.
            /// </summary>
            public event MLInput.ControllerConnectionDelegate OnDisconnect;
            #endif

            /// <summary>
            /// Buttons on device, controller, and Mobile Companion App. Links to MLInputControllerButton in ml_input.h.
            /// </summary>
            public enum Button : uint
            {
                /// <summary>
                /// Button: None
                /// </summary>
                None = 0,

                /// <summary>
                /// Button: Bumper
                /// </summary>
                Bumper = 3,

                /// <summary>
                /// Button: Home Tap
                /// </summary>
                HomeTap
            }

            /// <summary>
            /// Types of controllers recognized by Magic Leap platform. Links to MLInputControllerType in ml_input.h.
            /// </summary>
            public enum ControlType : uint
            {
                /// <summary>
                /// Type: None
                /// </summary>
                None = 0,

                /// <summary>
                /// Type: Control
                /// </summary>
                Control,

                /// <summary>
                /// Type: Mobile App
                /// </summary>
                MobileApp
            }

            /// <summary>
            /// Degrees-of-freedom mode of controller. Links to <c>MLInputControllerDof</c> in ml_input.h.
            /// </summary>
            public enum ControlDof : uint
            {
                /// <summary>
                /// Depth Of Field: None
                /// </summary>
                None = 0,

                /// <summary>
                /// Depth Of Field: 3
                /// </summary>
                Dof3,

                /// <summary>
                /// Depth Of Field: 6
                /// </summary>
                Dof6
            }

            /// <summary>
            /// The tracking mode used for the controller.
            /// </summary>
            public enum ControlMode : uint
            {
                /// <summary>
                /// <c>IMU 3 degree of freedom tracking (orientation only).</c>
                /// </summary>
                IMU3DOF = 0,

                /// <summary>
                /// <c>EM 6 degrees of freedom tracking (position and orientation).</c>
                /// </summary>
                EM6DOF = 1,

                /// <summary>
                /// <c>High quality EF Fused with IMU 6 degrees of freedom tracking (position and orientation).</c>
                /// </summary>
                Fused6DOF = 2,
            }

            /// <summary>
            /// The calibration accuracy levels for controller.
            /// </summary>
            public enum ControlCalibrationAccuracy : uint
            {
                /// <summary>
                /// The calibration accuracy is bad.
                /// </summary>
                Bad = 0,

                /// <summary>
                /// The calibration accuracy is low.
                /// </summary>
                Low = 1,

                /// <summary>
                /// The calibration accuracy is medium.
                /// </summary>
                Medium = 2,

                /// <summary>
                /// The calibration accuracy is high.
                /// </summary>
                High = 3,
            }

            /// <summary>
            /// Controller LED pattern. Links to MLInputControllerFeedbackPatternLED in ml_input.h.
            /// </summary>
            public enum FeedbackPatternLED : uint
            {
                /// <summary>
                /// Pattern: None
                /// </summary>
                None = 0,

                /// <summary>
                /// Pattern: Clock - points to 1:00
                /// </summary>
                Clock1,

                /// <summary>
                /// Pattern: Clock - points to 2:00
                /// </summary>
                Clock2,

                /// <summary>
                /// Pattern: Clock - points to 3:00
                /// </summary>
                Clock3,

                /// <summary>
                /// Pattern: Clock - points to 4:00
                /// </summary>
                Clock4,

                /// <summary>
                /// Pattern: Clock - points to 5:00
                /// </summary>
                Clock5,

                /// <summary>
                /// Pattern: Clock - points to 6:00
                /// </summary>
                Clock6,

                /// <summary>
                /// Pattern: Clock - points to 7:00
                /// </summary>
                Clock7,

                /// <summary>
                /// Pattern: Clock - points to 8:00
                /// </summary>
                Clock8,

                /// <summary>
                /// Pattern: Clock - points to 9:00
                /// </summary>
                Clock9,

                /// <summary>
                /// Pattern: Clock - points to 10:00
                /// </summary>
                Clock10,

                /// <summary>
                /// Pattern: Clock - points to 11:00
                /// </summary>
                Clock11,

                /// <summary>
                /// Pattern: Clock - points to 12:00
                /// </summary>
                Clock12,

                /// <summary>
                /// Pattern: Clock - points to 1:00 and 7:00
                /// </summary>
                Clock1And7,

                /// <summary>
                /// Pattern: Clock - points to 2:00 and 8:00
                /// </summary>
                Clock2And8,

                /// <summary>
                /// Pattern: Clock - points to 3:00 and 9:00
                /// </summary>
                Clock3And9,

                /// <summary>
                /// Pattern: Clock - points to 4:00 and 10:00
                /// </summary>
                Clock4And10,

                /// <summary>
                /// Pattern: Clock - points to 5:00 and 11:00
                /// </summary>
                Clock5And11,

                /// <summary>
                /// Pattern: Clock - points to 6:00 and 12:00
                /// </summary>
                Clock6And12
            }

            /// <summary>
            /// Feedback effects for LED target. Links to MLInputControllerFeedbackEffectLED in ml_input.h.
            /// </summary>
            public enum FeedbackEffectLED : uint
            {
                /// <summary>
                /// Feedback Effect: Rotate Clockwise
                /// </summary>
                RotateCW = 0,

                /// <summary>
                /// Feedback Effect: Rotate Counter Clockwise
                /// </summary>
                RotateCCW,

                /// <summary>
                /// Feedback Effect: Pulse
                /// </summary>
                Pulse,

                /// <summary>
                /// Feedback Effect: Paint Clockwise
                /// </summary>
                PaintCW,

                /// <summary>
                /// Feedback Effect: Paint Counter Clockwise
                /// </summary>
                PaintCCW,

                /// <summary>
                /// Feedback Effect: Blink
                /// </summary>
                Blink
            }

            /// <summary>
            /// Feedback colors for LED target. Links to MLInputControllerFeedbackColorLED in ml_input.h.
            /// </summary>
            public enum FeedbackColorLED : uint
            {
                /// <summary>
                /// Color: Bright Mission Red
                /// </summary>
                BrightMissionRed = 0,

                /// <summary>
                /// Color: Pastel Mission Red
                /// </summary>
                PastelMissionRed,

                /// <summary>
                /// Color: Bright Florida Orange
                /// </summary>
                BrightFloridaOrange,

                /// <summary>
                /// Color: Pastel Florida Orange
                /// </summary>
                PastelFloridaOrange,

                /// <summary>
                /// Color: Bright Luna Yellow
                /// </summary>
                BrightLunaYellow,

                /// <summary>
                /// Color: Pastel Luna Yellow
                /// </summary>
                PastelLunaYellow,

                /// <summary>
                /// Color: Bright Nebula Pink
                /// </summary>
                BrightNebulaPink,

                /// <summary>
                /// Color: Pastel Nebula Pink
                /// </summary>
                PastelNebulaPink,

                /// <summary>
                /// Color: Bright Cosmic Purple
                /// </summary>
                BrightCosmicPurple,

                /// <summary>
                /// Color: Pastel Cosmic Purple
                /// </summary>
                PastelCosmicPurple,

                /// <summary>
                /// Color: Bright Mystic Blue
                /// </summary>
                BrightMysticBlue,

                /// <summary>
                /// Color: Pastel Mystic Blue
                /// </summary>
                PastelMysticBlue,

                /// <summary>
                /// Color: Bright Celestial Blue
                /// </summary>
                BrightCelestialBlue,

                /// <summary>
                /// Color: Pastel Celestial Blue
                /// </summary>
                PastelCelestialBlue,

                /// <summary>
                /// Color: Bright <c>Shaggle</c> Green
                /// </summary>
                BrightShaggleGreen,

                /// <summary>
                /// Color: Pastel <c>Shaggle</c> Green
                /// </summary>
                PastelShaggleGreen
            }

            /// <summary>
            /// Feedback effect speed for LED target. Links to MLInputControllerFeedbackEffectSpeedLED in ml_input.h.
            /// </summary>
            public enum FeedbackEffectSpeedLED : uint
            {
                /// <summary>
                /// Speed: Slow
                /// </summary>
                Slow = 0,

                /// <summary>
                /// Speed: Medium
                /// </summary>
                Medium,

                /// <summary>
                /// Speed: Fast
                /// </summary>
                Fast
            }

            /// <summary>
            /// Controller vibration pattern. Links to MLInputControllerFeedbackPatternVibe in ml_input.h.
            /// </summary>
            public enum FeedbackPatternVibe : uint
            {
               /// <summary>
               /// Pattern: None
               /// </summary>
                None = 0,

                /// <summary>
                /// Pattern: Click
                /// </summary>
                Click,

                /// <summary>
                /// Pattern: Bump
                /// </summary>
                Bump,

                /// <summary>
                /// Pattern: Double Click
                /// </summary>
                DoubleClick,

                /// <summary>
                /// Pattern: Buzz
                /// </summary>
                Buzz,

                /// <summary>
                /// Pattern: Tick
                /// </summary>
                Tick,

                /// <summary>
                /// Pattern: Force Down
                /// </summary>
                ForceDown,

                /// <summary>
                /// Pattern: Force Up
                /// </summary>
                ForceUp,

                /// <summary>
                /// Pattern: Force Dwell
                /// </summary>
                ForceDwell,

                /// <summary>
                /// Pattern: Second Force Down
                /// </summary>
                SecondForceDown
            }

            /// <summary>
            /// Controller vibration intensity. Links to MLInputControllerFeedbackIntensity in ml_input.h.
            /// </summary>
            public enum FeedbackIntensity : uint
            {
                /// <summary>
                /// Intensity: Low
                /// </summary>
                Low = 0,

                /// <summary>
                /// Intensity: Medium
                /// </summary>
                Medium,

                /// <summary>
                /// Intensity: High
                /// </summary>
                High
            }

            #if PLATFORM_LUMIN
            /// <summary>
            /// Gets the hand assigned this controller.
            /// </summary>
            public MLInput.Hand Hand { get; internal set; }

            /// <summary>
            /// Gets a value indicating whether there is a controller connected to the device.
            /// </summary>
            public bool Connected { get; private set; }

            /// <summary>
            /// Gets the current 3DoF position.
            /// </summary>
            public Vector3 Position { get; private set; }

            /// <summary>
            /// Gets the current 3DoF orientation.
            /// </summary>
            public Quaternion Orientation { get; private set; }

            /// <summary>
            /// Gets the current touch 1 position (x,y) and force (z). Force is [0-1].
            /// </summary>
            public Vector3 Touch1PosAndForce { get; private set; }

            /// <summary>
            /// Gets a value indicating whether touch 1 is active.
            /// </summary>
            public bool Touch1Active { get; private set; }

            /// <summary>
            /// Gets the current touch 2 position (x,y) and force (z). Force is [0-1].
            /// </summary>
            public Vector3 Touch2PosAndForce { get; private set; }

            /// <summary>
            /// Gets a value indicating whether touch 2 is active.
            /// </summary>
            public bool Touch2Active { get; private set; }

            /// <summary>
            /// Gets current trigger value [0.0-1.0]
            /// </summary>
            public float TriggerValue { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the bumper is down.
            /// </summary>
            public bool IsBumperDown { get; private set; }

            /// <summary>
            /// Gets the current touchpad gesture state.
            /// </summary>
            public TouchpadGesture.State TouchpadGestureState { get; private set; }

            /// <summary>
            /// Gets the controller type.
            /// </summary>
            public ControlType Type { get; private set; }

            /// <summary>
            /// Gets the current degrees of freedom mode.
            /// </summary>
            public ControlDof Dof { get; private set; }

            /// <summary>
            /// Gets the calibration accuracy levels.
            /// </summary>
            public ControlCalibrationAccuracy CalibrationAccuracy { get; private set; }

            /// <summary>
            /// Gets the current touchpad gesture.
            /// </summary>
            public TouchpadGesture CurrentTouchpadGesture { get; private set; }

            /// <summary>
            /// Gets the controller id.
            /// </summary>
            public byte Id
            {
                get
                {
                    return this.controllerId;
                }
            }

            /// <summary>
            /// Updates the control state using the MLController API.
            /// </summary>
            /// <param name="device">The current device for the frame.</param>
            /// <returns>Returns the result from either MLControllerGetState or MLSnapshotGetTransform.</returns>
            public MLResult Update(InputDevice device)
            {
                MLResult.Code resultCode = MLResult.Code.Ok;

                // Always return OK?
                if (device.isValid)
                {
                    // CalibrationAccuracy
                    if (device.TryGetFeatureValue(MagicLeapControllerUsages.ControllerCalibrationAccuracy, out uint calibrationAccuracy))
                    {
                        this.CalibrationAccuracy = (ControlCalibrationAccuracy)calibrationAccuracy;
                    }

                    // Position
                    if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePosition))
                    {
                        this.Position = devicePosition;
                    }

                    // Rotation
                    if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
                    {
                        this.Orientation = deviceRotation;
                    }

                    // TriggerValue
                    if (device.TryGetFeatureValue(CommonUsages.trigger, out float deviceTriggerValue))
                    {
                        this.TriggerValue = deviceTriggerValue;
                    }

                    // IsBumperDown
                    if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool deviceIsBumperDown))
                    {
                        if (this.IsBumperDown != deviceIsBumperDown)
                        {
                            if (deviceIsBumperDown)
                            {
                                this.OnButtonDown?.Invoke(this.controllerId, Button.Bumper);
                            }
                            else
                            {
                                this.OnButtonUp?.Invoke(this.controllerId, Button.Bumper);
                            }
                        }

                        this.IsBumperDown = deviceIsBumperDown;
                    }

                    // IsMenuDown
                    if (device.TryGetFeatureValue(CommonUsages.menuButton, out bool deviceIsMenuDown))
                    {
                        // This is not a "real" button, it fires both the down/up event upon release.
                        if (this.isMenuButtonDown && this.isMenuButtonDown != deviceIsMenuDown)
                        {
                            this.OnButtonDown?.Invoke(this.controllerId, Button.HomeTap);

                            this.OnButtonUp?.Invoke(this.controllerId, Button.HomeTap);
                        }

                        this.isMenuButtonDown = deviceIsMenuDown;
                    }

                    // Touch1Active
                    if (device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool deviceIsTouch1Active))
                    {
                        this.Touch1Active = deviceIsTouch1Active;
                    }

                    // Touch2Active
                    if (device.TryGetFeatureValue(MagicLeapControllerUsages.secondary2DAxisTouch, out bool deviceIsTouch2Active))
                    {
                        this.Touch2Active = deviceIsTouch2Active;
                    }

                    // Touch1PosAndForce
                    if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 deviceTouch1PosAndForce))
                    {
                        if (device.TryGetFeatureValue(MagicLeapControllerUsages.ControllerTouch1Force, out float force))
                        {
                            this.Touch1PosAndForce = new Vector3(deviceTouch1PosAndForce.x, deviceTouch1PosAndForce.y, force);
                        }
                    }

                    // Touch2PosAndForce
                    if (device.TryGetFeatureValue(CommonUsages.secondary2DAxis, out Vector2 deviceTouch2PosAndForce))
                    {
                        if (device.TryGetFeatureValue(MagicLeapControllerUsages.ControllerTouch2Force, out float force))
                        {
                            this.Touch2PosAndForce = new Vector3(deviceTouch2PosAndForce.x, deviceTouch2PosAndForce.y, force);
                        }
                    }

                    // Type
                    if (device.TryGetFeatureValue(MagicLeapControllerUsages.ControllerType, out uint deviceControllerType))
                    {
                        this.Type = (ControlType)deviceControllerType;
                    }

                    // DOF
                    if (device.TryGetFeatureValue(MagicLeapControllerUsages.ControllerDOF, out uint deviceDOF))
                    {
                        this.Dof = (ControlDof)deviceDOF;
                    }

                    if (device.TryGetFeatureValue(CommonUsages.isTracked, out bool deviceIsTracked))
                    {
                        // Check for a state change.
                        if (deviceIsTracked && deviceIsTracked != this.Connected)
                        {
                            this.OnConnect?.Invoke(this.controllerId);

                            this.Connected = deviceIsTracked;
                        }
                    }
                }
                else
                {
                    // Handle Disconnected event.
                    // Once a device is turned off, it becomes invalid, the isTracked value never gets set to false.
                    if (this.Connected)
                    {
                        this.OnDisconnect?.Invoke(this.controllerId);

                        this.Connected = false;
                    }
                }

                return MLResult.Create(resultCode);
            }

            /// <summary>
            /// Start the controller's LED in the specified pattern and set the duration (in seconds)
            /// </summary>
            /// <param name="pattern">The pattern of the LED effect.</param>
            /// <param name="color">The color of the LED effect.</param>
            /// <param name="duration">The duration of the LED effect.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// </returns>
            public MLResult StartFeedbackPatternLED(FeedbackPatternLED pattern, FeedbackColorLED color, float duration)
            {
                duration = Mathf.Round(duration * 1000.0f);
                if (NativeBindings.MLInputStartControllerFeedbackPatternLED(this.controllerId, pattern, color, (uint)duration))
                {
                    return MLResult.Create(MLResult.Code.Ok);
                }

                MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "UnityMagicLeap - MLInput.Controller.StartFeedbackPatternLED() returned an error.");
                MLPluginLog.ErrorFormat("MLInput.Controller.StartFeedbackPatternLED failed. Reason: {0}", result);

                return result;
            }

            /// <summary>
            /// Start the controller's LED performing the specified effect at the specified speed with the specified pattern for the specified duration (in seconds)
            /// </summary>
            /// <param name="effect">The LED effect.</param>
            /// <param name="speed">The speed of the LED effect.</param>
            /// <param name="pattern">The pattern of the LED effect.</param>
            /// <param name="color">The color of the LED effect.</param>
            /// <param name="duration">The duration of the LED effect.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// </returns>
            public MLResult StartFeedbackPatternEffectLED(FeedbackEffectLED effect, FeedbackEffectSpeedLED speed, FeedbackPatternLED pattern, FeedbackColorLED color, float duration)
            {
                duration = Mathf.Round(duration * 1000.0f);
                if (NativeBindings.MLInputStartControllerFeedbackPatternEffectLED(this.controllerId, effect, speed, pattern, color, (uint)duration))
                {
                    return MLResult.Create(MLResult.Code.Ok);
                }

                MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "UnityMagicLeap - MLInputStartControllerFeedbackPatternEffectLED() returned an error.");
                MLPluginLog.ErrorFormat("MLInputController.StartFeedbackPatternEffectLED failed. Reason: {0}", result);

                return result;
            }

            /// <summary>
            /// Stop on-going LED patterns for the controller
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// </returns>
            public MLResult StopFeedbackPatternLED()
            {
                // color and duration are irrelevant
                if (NativeBindings.MLInputStartControllerFeedbackPatternLED(this.controllerId, FeedbackPatternLED.None, 0, 0))
                {
                    return MLResult.Create(MLResult.Code.Ok);
                }

                MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "UnityMagicLeap - MLInputStartControllerFeedbackPatternLED() returned an error.");
                MLPluginLog.ErrorFormat("MLInputController.StopFeedbackPatternLED failed. Reason: {0}", result);

                return result;
            }

            /// <summary>
            /// Vibrate the controller in the specified pattern
            /// </summary>
            /// <param name="pattern">The feedback vibration pattern.</param>
            /// <param name="intensity">The intensity of the vibration.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// </returns>
            public MLResult StartFeedbackPatternVibe(FeedbackPatternVibe pattern, FeedbackIntensity intensity)
            {
                if (NativeBindings.MLInputStartControllerFeedbackPatternVibe(this.controllerId, pattern, intensity))
                {
                    return MLResult.Create(MLResult.Code.Ok);
                }

                MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "UnityMagicLeap - MLInput.Controller.StartFeedbackPatternVibe() returned an error.");
                MLPluginLog.ErrorFormat("MLInput.Controller.StartFeedbackPatternVibe failed. Reason: {0}", result);

                return result;
            }

            /// <summary>
            /// Stop on-going Vibration patterns for the controller
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// </returns>
            public MLResult StopFeedbackPatternVibe()
            {
                // intensity is irrelevant
                if (NativeBindings.MLInputStartControllerFeedbackPatternVibe(this.controllerId, FeedbackPatternVibe.None, 0))
                {
                    return MLResult.Create(MLResult.Code.Ok);
                }

                MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "UnityMagicLeap - MLInputStartControllerFeedbackPatternVibe() returned an error.");
                MLPluginLog.ErrorFormat("MLInputController.StopFeedbackPatternVibe failed. Reason: {0}", result);

                return result;
            }

            /// <summary>
            /// Occurs when a touchpad gesture changes.
            /// </summary>
            /// <param name="gestureEvent">The touchpad gesture that changed.</param>
            private void HandleOnTouchpadGestureChanged(MagicLeapTouchpadGestureEvent gestureEvent)
            {
                // Only update when the received gesture is for the matching controller.
                if (gestureEvent.controllerId == this.controllerId)
                {
                    this.CurrentTouchpadGesture.Update(gestureEvent);

                    switch (gestureEvent.state)
                    {
                        case GestureState.Started:
                            {
                                this.TouchpadGestureState = TouchpadGesture.State.Start;
                                this.OnTouchpadGestureStart?.Invoke(this.controllerId, this.CurrentTouchpadGesture);
                                break;
                            }

                        case GestureState.Updated:
                            {
                                this.TouchpadGestureState = TouchpadGesture.State.Continue;
                                this.OnTouchpadGestureContinue?.Invoke(this.controllerId, this.CurrentTouchpadGesture);
                                break;
                            }

                        case GestureState.Completed:
                            {
                                this.TouchpadGestureState = TouchpadGesture.State.End;
                                this.OnTouchpadGestureEnd?.Invoke(this.controllerId, this.CurrentTouchpadGesture);
                                break;
                            }
                    }
                }
            }
            #endif
        }
    }
}
