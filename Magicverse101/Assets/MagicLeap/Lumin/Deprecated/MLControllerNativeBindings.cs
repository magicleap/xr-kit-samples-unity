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

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Runtime.InteropServices;

    #if PLATFORM_LUMIN
    namespace Native
    {
        /// <summary>
        /// Bindings to access for direct access to the Controller system.
        /// </summary>
        public partial class MLControllerNativeBindings : MagicLeapNativeBindings
        {
            /// <summary>
            /// Starts a vibe feedback pattern on the specified controller.
            /// </summary>
            /// <param name="controllerId">The id of the controller.</param>
            /// <param name="pattern">The haptic feedback pattern to apply.</param>
            /// <param name="intensity">The intensity of the haptic feedback pattern.</param>
            /// <returns>Returns true if the operation completed successfully.</returns>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_StartControllerFeedbackPatternVibe")]
            [return: MarshalAs(UnmanagedType.I1)]
            [Obsolete("Please use MLInputStartControllerFeedbackPatternVibe(byte, MLInput.Controller.FeedbackPatternVibe, MLInput.Controller.FeedbackIntensity) instead.", false)]
            public static extern bool MLInputStartControllerFeedbackPatternVibe(byte controllerId, MLInputControllerFeedbackPatternVibe pattern, MLInputControllerFeedbackIntensity intensity);

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
            [Obsolete("Please use MLInputStartControllerFeedbackPatternLED(byte, MLInput.Controller.FeedbackPatternLED, MLInput.Controller.FeedbackColorLED, uint) instead.", false)]
            public static extern bool MLInputStartControllerFeedbackPatternLED(byte controllerId, MLInputControllerFeedbackPatternLED pattern, MLInputControllerFeedbackColorLED color, uint duration);

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
            [Obsolete("Please use MLInputStartControllerFeedbackPatternEffectLED(byte, MLInput.Controller.FeedbackEffectLED, MLInput.Controller.FeedbackEffectSpeedLED, MLInput.Controller.FeedbackPatternLED, MLInput.Controller.FeedbackColorLED, uint) instead.", false)]
            public static extern bool MLInputStartControllerFeedbackPatternEffectLED(byte controllerId, MLInputControllerFeedbackEffectLED effect, MLInputControllerFeedbackEffectSpeedLED speed, MLInputControllerFeedbackPatternLED pattern, MLInputControllerFeedbackColorLED color, uint duration);
        }
    }
    #endif

    /// <summary>
    /// The tracking mode used for the controller.
    /// </summary>
    [Obsolete("Please use MLInput.Controller.ControlMode instead.", true)]
    public enum MLControllerMode : uint
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
    [Obsolete("Please use MLInput.Controller.ControlCalibrationAccuracy instead.", true)]
    public enum MLControllerCalibAccuracy : uint
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
}
