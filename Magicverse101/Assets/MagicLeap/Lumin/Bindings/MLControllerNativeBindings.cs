// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLControllerNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

// Disable warnings about missing documentation for native interop.
#pragma warning disable 1591

#if PLATFORM_LUMIN
namespace UnityEngine.XR.MagicLeap
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Bindings to access for direct access to the Input system.
    /// </summary>
    public sealed partial class MLInput : MLAPISingleton<MLInput>
    {
        /// <summary>
        /// The native bindings to the Input API.
        /// See ml_controller.h for additional comments.
        /// </summary>
        private partial class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// The stream size for controller information.
            /// </summary>
            private const int StreamSize = (int)MLInput.Controller.ControlMode.Fused6DOF + 1;

            /// <summary>
            /// Updates the configuration for the controllers.
            /// This will Stop/Start the API when a new configuration is applied.
            /// </summary>
            /// <param name="config">The configuration file that should be applied.</param>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_InputSetControllerConfiguration")]
            public static extern void UpdateConfiguration(ref MLControllerConfigurationNative config);

            /// <summary>
            /// Checks the controller tracker active status.
            /// </summary>
            /// <returns>The status of the controller tracker.</returns>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_InputGetControllerTrackerActive")]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool GetControllerTrackerActive();

            /// <summary>
            /// Sets the active status of the controller tracker.
            /// </summary>
            /// <param name="value">The active status of the controller tracker.</param>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_InputSetControllerTrackerActive")]
            public static extern void SetControllerTrackerActive(bool value);

            /// <summary>
            /// Checks the controller gestures active status.
            /// </summary>
            /// <returns>Returns the status of controller gestures.</returns>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_GesturesIsControllerGesturesEnabled")]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool IsControllerGesturesEnabled();

            /// <summary>
            /// Sets the active status for controller gestures.
            /// </summary>
            /// <param name="value">The active status of the controller gestures.</param>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_GesturesSetControllerGesturesEnabled")]
            public static extern void SetControllerGesturesEnabled(bool value);

            /// <summary>
            /// Starts a vibe feedback pattern on the specified controller.
            /// </summary>
            /// <param name="controllerId">The id of the controller.</param>
            /// <param name="pattern">The haptic feedback pattern to apply.</param>
            /// <param name="intensity">The intensity of the haptic feedback pattern.</param>
            /// <returns>Returns true if the operation completed successfully.</returns>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_StartControllerFeedbackPatternVibe")]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool MLInputStartControllerFeedbackPatternVibe(byte controllerId, MLInput.Controller.FeedbackPatternVibe pattern, MLInput.Controller.FeedbackIntensity intensity);

            /// <summary>
            /// Starts a LED feedback pattern on the specified controller.
            /// </summary>
            /// <param name="controllerId">The id of the controller.</param>
            /// <param name="pattern">The LED pattern to apply.</param>
            /// <param name="color">The LED color value to apply.</param>
            /// <param name="duration">The duration for the LED pattern.</param>
            /// <returns>Returns true if the operation completed successfully.</returns>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_StartControllerFeedbackPatternLED")]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool MLInputStartControllerFeedbackPatternLED(byte controllerId, MLInput.Controller.FeedbackPatternLED pattern, MLInput.Controller.FeedbackColorLED color, uint duration);

            /// <summary>
            /// Starts a LED feedback effect using the specified pattern on the specified controller.
            /// </summary>
            /// <param name="controllerId">The id of the controller.</param>
            /// <param name="effect">The LED effect to apply.</param>
            /// <param name="speed">The speed of the LED effect.</param>
            /// <param name="pattern">The LED pattern to apply.</param>
            /// <param name="color">The LED color value to apply.</param>
            /// <param name="duration">The duration for the LED pattern.</param>
            /// <returns>Returns true if the operation completed successfully.</returns>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_StartControllerFeedbackPatternEffectLED")]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool MLInputStartControllerFeedbackPatternEffectLED(byte controllerId, MLInput.Controller.FeedbackEffectLED effect, MLInput.Controller.FeedbackEffectSpeedLED speed, MLInput.Controller.FeedbackPatternLED pattern, MLInput.Controller.FeedbackColorLED color, uint duration);

            /// <summary>
            /// Struct to configure controller's tracking-modes.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MLControllerConfigurationNative
            {
                /// <summary>
                /// <c>IMU only 3 degree of freedom tracking (orientation only).</c>
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool IMU3DOF;

                /// <summary>
                /// <c>EM only 6 degrees of freedom tracking (position and orientation).</c>
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool EM6DOF;

                /// <summary>
                /// <c>High quality EM fused with IMU 6 degrees of freedom tracking (position and orientation).</c>
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool Fused6DOF;
            }
        }
    }
}
#endif
