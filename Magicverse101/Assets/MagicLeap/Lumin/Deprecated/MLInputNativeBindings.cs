// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLInputNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using UnityEngine.XR.MagicLeap.Native;

    namespace Native
    {
        /// <summary>
        /// See ml_input.h for additional comments.
        /// </summary>
        public partial class MLInputNativeBindings : MagicLeapNativeBindings
        {
            /// <summary>
            /// The maximum number of controllers that are supported.
            /// </summary>
            public const uint MaxControllers = 2;

            /// <summary>
            /// Tablet additional pen data size.
            /// </summary>
            public const int TabletAdditionalPenDataSize = 3;

            /// <summary>
            /// The native input configuration settings.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MLInputConfigurationNative
            {
                /// <summary>
                /// Gets the degrees-of-freedom mode for the control.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MaxControllers)]
                [Obsolete("This structure is obsolete, 6DOF is now handled through MLControllerNativeBindings.", true)]
                public MLInput.Controller.ControlDof[] Dof;

                /// <summary>
                /// Sets the configuration data. (Obsolete, nothing is set.)
                /// </summary>
                [Obsolete("This property is obsolete and is no longer functions.", true)]
                public MLInput.Configuration Data
                {
                    set { }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>Returns an instance of the MLInputConfigurationNative structure.</returns>
                [Obsolete("This method is obsolete and is no longer functions.", true)]
                public static MLInputConfigurationNative Create()
                {
                    return new MLInputConfigurationNative { };
                }
            }

            /// <summary>
            /// Links to MLInputTabletDeviceState in ml_input.h.
            /// </summary>
            [Obsolete("Please use MLInput.TabletDeviceStateNative instead.", true)]
            [StructLayout(LayoutKind.Sequential)]
            public struct TabletDeviceStateNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Type of this tablet device.
                /// </summary>
                public MLInput.TabletDeviceType Type;

                /// <summary>
                /// Type of tool used with the tablet.
                /// </summary>
                public MLInput.TabletDeviceToolType ToolType;

                /// <summary>
                /// Current touch position (X,Y) and force (Z).
                /// Position is in the [-1.0,1.0] range and force is in the [0.0,1.0] range.
                /// </summary>
                public MLVec3f PenTouchPosAndForce;

                /// <summary>
                /// Additional coordinate values (X,Y,Z)
                /// It could contain data specific to the device type.
                /// Example: It could hold tilt values while using pen
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = TabletAdditionalPenDataSize)]
                public int[] AdditionalPenTouchData;

                /// <summary>
                /// Is touch active.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool IsPenTouchActive;

                /// <summary>
                /// If this tablet is connected.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
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
                /// Flags to denote which of the above fields are valid.
                /// </summary>
                public uint ValidFieldsFlag;

                /// <summary>
                /// Gets the tablet state from the internal format.
                /// </summary>
                public MLInput.TabletState Data
                {
                    get
                    {
                        MLInput.TabletState state = new MLInput.TabletState();

                        state.Type = this.Type;
                        state.ToolType = this.ToolType;
                        state.PenTouchPosAndForce = new Vector3(this.PenTouchPosAndForce.X, this.PenTouchPosAndForce.Y, this.PenTouchPosAndForce.Z);

                        state.AdditionalPenTouchData = new int[TabletAdditionalPenDataSize];
                        if (this.AdditionalPenTouchData != null && this.AdditionalPenTouchData.Length > 0)
                        {
                            for (int i = 0; i < TabletAdditionalPenDataSize; i++)
                            {
                                state.AdditionalPenTouchData[i] = this.AdditionalPenTouchData[i];
                            }
                        }

                        state.IsPenTouchActive = this.IsPenTouchActive;
                        state.IsConnected = this.IsConnected;
                        state.PenDistance = this.PenDistance;
                        state.TimeStamp = this.TimeStamp;
                        state.ValidityCheck = (MLInput.TabletDeviceStateMask)this.ValidFieldsFlag;
                        return state;
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>Returns a new instance of the MLInputTabletDeviceStateNative structure.</returns>
                public static TabletDeviceStateNative Create()
                {
                    return new TabletDeviceStateNative
                    {
                        Version = 1u,
                        Type = MLInput.TabletDeviceType.Unknown,
                        ToolType = MLInput.TabletDeviceToolType.Unknown,
                        PenTouchPosAndForce = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        AdditionalPenTouchData = Enumerable.Repeat<int>(0, TabletAdditionalPenDataSize).ToArray(),
                        IsPenTouchActive = false,
                        IsConnected = false,
                        PenDistance = 0.0f,
                        TimeStamp = 0u,
                        ValidFieldsFlag = 0u
                    };
                }
            }
        }
    }

    /// <summary>
    /// Gesture state. Links to MLInputControllerTouchpadGestureState in ml_input.h.
    /// </summary>
    [Obsolete("Please use MLInput.Controller.TouchpadGesture.State instead.", false)]
    public enum MLInputControllerTouchpadGestureState : uint
    {
        /// <summary>
        /// State: End
        /// </summary>
        End,

        /// <summary>
        /// State: Continue
        /// </summary>
        Continue,

        /// <summary>
        /// State: Start
        /// </summary>
        Start
    }

    /// <summary>
    /// Recognized touchpad gesture types. Links to MLInputControllerTouchpadGestureType in ml_input.h.
    /// </summary>
    [Obsolete("Please use MLInput.Controller.TouchpadGesture.GestureType instead.", false)]
    public enum MLInputControllerTouchpadGestureType : uint
    {
        /// <summary>
        /// Type: None
        /// </summary>
        None,

        /// <summary>
        /// Type: Tap
        /// </summary>
        Tap,

        /// <summary>
        /// Type: Force Tap Down
        /// </summary>
        ForceTapDown,

        /// <summary>
        /// Type: Force Tap Up
        /// </summary>
        ForceTapUp,

        /// <summary>
        /// Type: Force Dwell
        /// </summary>
        ForceDwell,

        /// <summary>
        /// Type: Second Force Down
        /// </summary>
        SecondForceDown,

        /// <summary>
        /// Type: Long Hold
        /// </summary>
        LongHold,

        /// <summary>
        /// Type: Radial Scroll
        /// </summary>
        RadialScroll,

        /// <summary>
        /// Type: Swipe
        /// </summary>
        Swipe,

        /// <summary>
        /// Type: Scroll
        /// </summary>
        Scroll,

        /// <summary>
        /// Type: Pinch
        /// </summary>
        Pinch
    }

    /// <summary>
    /// Buttons on device, controller, and Mobile Companion App. Links to MLInputControllerButton in ml_input.h.
    /// </summary>
    [Obsolete("Please use MLInput.Controller.Button instead.", false)]
    public enum MLInputControllerButton : uint
    {
        /// <summary>
        /// Button: None
        /// </summary>
        None,

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
    [Obsolete("Please use MLInput.Controller.ControlType instead.", false)]
    public enum MLInputControllerType : uint
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
    /// Degrees-of-freedom mode of controller. Links to MLInputControllerDof in ml_input.h.
    /// </summary>
    [Obsolete("Please use MLInput.Controller.ControlDof instead.", true)]
    public enum MLInputControllerDof : uint
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
    /// Direction of touchpad gesture. Links to MLInputControllerTouchpadGestureDirection in ml_input.h.
    /// </summary>
    [Obsolete("Please use MLInput.Controller.TouchpadGesture.GestureDirection instead.", false)]
    public enum MLInputControllerTouchpadGestureDirection : uint
    {
        /// <summary>
        /// Direction: None
        /// </summary>
        None,

        /// <summary>
        /// Direction: Up
        /// </summary>
        Up,

        /// <summary>
        /// Direction: Down
        /// </summary>
        Down,

        /// <summary>
        /// Direction: Left
        /// </summary>
        Left,

        /// <summary>
        /// Direction: Right
        /// </summary>
        Right,

        /// <summary>
        /// Direction: In
        /// </summary>
        In,

        /// <summary>
        /// Direction: Out
        /// </summary>
        Out,

        /// <summary>
        /// Direction: Clockwise
        /// </summary>
        Clockwise,

        /// <summary>
        /// Direction: Counter Clockwise
        /// </summary>
        CounterClockwise
    }

    /// <summary>
    /// Controller LED pattern. Links to MLInputControllerFeedbackPatternLED in ml_input.h.
    /// </summary>
    [Obsolete("Please use MLInput.Controller.FeedbackPatternLED instead.", false)]
    public enum MLInputControllerFeedbackPatternLED : uint
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
    [Obsolete("Please use MLInput.Controller.FeedbackEffectLED instead.", false)]
    public enum MLInputControllerFeedbackEffectLED : uint
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
    [Obsolete("Please use MLInput.Controller.FeedbackColorLED instead.", false)]
    public enum MLInputControllerFeedbackColorLED : uint
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
    [Obsolete("Please use MLInput.Controller.FeedbackEffectSpeedLED instead.", false)]
    public enum MLInputControllerFeedbackEffectSpeedLED : uint
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
    [Obsolete("Please use MLInput.Controller.FeedbackPatternVibe instead.", false)]
    public enum MLInputControllerFeedbackPatternVibe : uint
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
    [Obsolete("Please use MLInput.Controller.FeedbackIntensity instead.", false)]
    public enum MLInputControllerFeedbackIntensity : uint
    {
        /// <summary>
        /// Intensity: Low
        /// </summary>
        Low,

        /// <summary>
        /// Intensity: Medium
        /// </summary>
        Medium,

        /// <summary>
        /// Intensity: High
        /// </summary>
        High
    }

    /// <summary>
    /// Types of input tablet devices recognized. Links to MLInputTabletDeviceType in ml_input.h.
    /// </summary>
    [Obsolete("Please use MLInput.TabletDeviceType instead.", true)]
    public enum MLInputTabletDeviceType : uint
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
    [Obsolete("Please use MLInput.TabletDeviceToolType instead.", true)]
    public enum MLInputTabletDeviceToolType : uint
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
    [Obsolete("Please use MLInput.TabletDeviceButton instead.", true)]
    public enum MLInputTabletDeviceButton : uint
    {
        /// <summary>
        /// An unknown tablet button.
        /// </summary>
        Unknown,

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
    /// Mask value to determine the validity of variables in MLInputTabletDeviceState. Links to MLInputTabletDeviceStateMask in ml_input.h.
    /// </summary>
    [Flags]
    [Obsolete("Please use MLInput.TabletDeviceStateMask instead.", false)]
    public enum MLInputTabletDeviceStateMask : uint
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

    /// <summary>
    /// Links to MLInputConnectedDevicesList in ml_input.h.
    /// </summary>
    [Obsolete("Please use MLInput.NativeBindings.ConnectedDevicesListNative instead.", true)]
    [StructLayout(LayoutKind.Sequential)]
    public struct MLInputConnectedDevicesListNative
    {
        /// <summary>
        /// Version of this structure.
        /// </summary>
        public uint Version;

        /// <summary>
        /// Number of connected controllers.
        /// </summary>
        public uint ControllerCount;

        /// <summary>
        /// Pointer to the array of connected controller IDs(byte).
        /// </summary>
        public IntPtr ControllerIds;

        /// <summary>
        /// Number of connected tablet devices.
        /// </summary>
        public uint TabletDeviceCount;

        /// <summary>
        /// Pointer to the array of connected tablet device IDs(byte).
        /// </summary>
        public IntPtr TabletDeviceIds;

        /// <summary>
        /// Create and return an initialized version of this struct.
        /// </summary>
        /// <returns>Returns a new instance of the MLInputConnectedDevicesListNative structure.</returns>
        public static MLInputConnectedDevicesListNative Create()
        {
            return new MLInputConnectedDevicesListNative
            {
                Version = 1u,
                ControllerCount = 0u,
                ControllerIds = IntPtr.Zero,
                TabletDeviceCount = 0u,
                TabletDeviceIds = IntPtr.Zero
            };
        }
    }

    /// <summary>
    /// Links to MLInputTabletDeviceCallbacks in ml_input.h.
    /// </summary>
    [Obsolete("Please use MLInput.NativeBindings.TabletDeviceCallbacksNative instead.", true)]
    [StructLayout(LayoutKind.Sequential)]
    public struct MLInputTabletDeviceCallbacksNative
    {
        /// <summary>
        /// Version of this structure.
        /// </summary>
        public uint Version;

        /// <summary>
        /// This callback occurs when the pen touches the tablet.
        /// </summary>
        public OnPenTouchEventCallback OnPenTouchEvent;

        /// <summary>
        /// This callback occurs when the tablet ring is touched.
        /// </summary>
        public OnTouchRingEventCallback OnTouchRingEvent;

        /// <summary>
        /// This callback occurs when buttons on the tablet are pressed.
        /// </summary>
        public OnButtonDownCallback OnButtonDown;

        /// <summary>
        /// This callback occurs when buttons on the tablet are released.
        /// </summary>
        public OnButtonUpCallback OnButtonUp;

        /// <summary>
        /// This callback occurs when the tablet connects.
        /// </summary>
        public OnConnectCallback OnConnect;

        /// <summary>
        /// This callback occurs when the tablet disconnects.
        /// </summary>
        public OnDisconnectCallback OnDisconnect;

        /// <summary>
        /// A delegate for when a pen touches the tablet.
        /// </summary>
        /// <param name="tablet_device_id">The id of the tablet.</param>
        /// <param name="tablet_device_state">The state of the tablet.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        public delegate void OnPenTouchEventCallback(byte tablet_device_id, ref MLInputNativeBindings.TabletDeviceStateNative tablet_device_state, IntPtr data);

        /// <summary>
        /// A delegate for when the tablet ring is touched.
        /// </summary>
        /// <param name="tablet_device_id">The id of the tablet.</param>
        /// <param name="touch_ring_value">The value of the tablet touch ring.</param>
        /// <param name="timestamp">The time the callback occurred.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        public delegate void OnTouchRingEventCallback(byte tablet_device_id, int touch_ring_value, ulong timestamp, IntPtr data);

        /// <summary>
        /// A delegate for when a tablet button is pressed.
        /// </summary>
        /// <param name="tablet_device_id">The id of the tablet.</param>
        /// <param name="tablet_device_button">The tablet button that was pressed.</param>
        /// <param name="timestamp">The time the callback occurred.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        public delegate void OnButtonDownCallback(byte tablet_device_id, MLInput.TabletDeviceButton tablet_device_button, ulong timestamp, IntPtr data);

        /// <summary>
        /// A delegate for when a tablet button is released.
        /// </summary>
        /// <param name="tablet_device_id">The id of the tablet.</param>
        /// <param name="tablet_device_button">The tablet button that was released.</param>
        /// <param name="timestamp">The time the callback occurred.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        public delegate void OnButtonUpCallback(byte tablet_device_id, MLInput.TabletDeviceButton tablet_device_button, ulong timestamp, IntPtr data);

        /// <summary>
        /// A delegate for when a tablet connects.
        /// </summary>
        /// <param name="tablet_device_id">The id of the tablet.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        public delegate void OnConnectCallback(byte tablet_device_id, IntPtr data);

        /// <summary>
        /// A delegate for when a tablet disconnects.
        /// </summary>
        /// <param name="tablet_device_id">The id of the tablet.</param>
        /// <param name="data">A pointer to user payload data (can be NULL).</param>
        public delegate void OnDisconnectCallback(byte tablet_device_id, IntPtr data);

        /// <summary>
        /// Create and return an initialized version of this struct.
        /// </summary>
        /// <returns>Returns a new instance of the MLInputTabletDeviceCallbacksNative structure.</returns>
        public static MLInputTabletDeviceCallbacksNative Create()
        {
            return new MLInputTabletDeviceCallbacksNative
            {
                Version = 1u,
                OnPenTouchEvent = null,
                OnTouchRingEvent = null,
                OnButtonDown = null,
                OnButtonUp = null,
                OnConnect = null,
                OnDisconnect = null
            };
        }
    }

    /// <summary>
    /// Links to MLInputTabletDeviceStatesList in ml_input.h.
    /// </summary>
    [Obsolete("Please use MLInput.NativeBindings.TabletDeviceStatesListNative instead.", true)]
    [StructLayout(LayoutKind.Sequential)]
    public struct MLInputTabletDeviceStatesListNative
    {
        /// <summary>
        /// Version of this structure.
        /// </summary>
        public uint Version;

        /// <summary>
        /// Number of tablet device states in this list.
        /// </summary>
        public uint Count;

        /// <summary>
        /// Pointer referring to the array of MLInputTabletDeviceState.
        /// </summary>
        public IntPtr Data;

        /// <summary>
        /// Create and return an initialized version of this struct.
        /// </summary>
        /// <returns>Returns a new instance of the MLInputTabletDeviceStatesListNative structure.</returns>
        public static MLInputTabletDeviceStatesListNative Create()
        {
            return new MLInputTabletDeviceStatesListNative
            {
                Version = 1u,
                Count = 0u,
                Data = IntPtr.Zero
            };
        }
    }
}

#endif
