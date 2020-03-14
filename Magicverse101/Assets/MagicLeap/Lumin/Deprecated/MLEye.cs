// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLEye.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// Class used to represents a single eye.
    /// </summary>
    [Obsolete("Please use MLEyes.MLEye instead.", true)]
    public sealed class MLEye
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// Initializes a new instance of the <see cref="MLEye"/> class.
        /// </summary>
        /// <param name="eyeType">The type of eye to initialize.</param>
        public MLEye(EyeType eyeType)
        {
            this.Type = eyeType;

            // Initialize
            this.Center = Vector3.zero;
            this.IsBlinking = false;
            this.CenterConfidence = 0;
        }
        #endif

        /// <summary>
        /// Enumeration to specify which eye.
        /// </summary>
        public enum EyeType
        {
            /// <summary>
            /// Left Eye
            /// </summary>
            Left,

            /// <summary>
            /// Right Eye
            /// </summary>
            Right
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Gets the eye type.
        /// </summary>
        public EyeType Type { get; private set; }

        /// <summary>
        /// Gets the eye center.
        /// </summary>
        public Vector3 Center { get; private set; }

        /// <summary>
        /// Gets the eye rotation.
        /// </summary>
        public Quaternion Gaze { get; private set; }

        /// <summary>
        /// Gets the forward direction of the eye gaze.
        /// </summary>
        public Vector3 ForwardGaze
        {
            get
            {
                return this.Gaze * Vector3.forward;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the eye is blinking.
        /// Set to false before initial update.
        /// </summary>
        public bool IsBlinking { get; private set; }

        /// <summary>
        /// Gets the confidence value for eye center.
        /// 0 - no eye detected (when not wearing the device or closed eye.)
        /// Initial value is set to 0 before the first update.
        /// </summary>
        public float CenterConfidence { get; private set; }

        /// <summary>
        /// Update the eye properties with the provided values.
        /// </summary>
        /// <param name="center">The center of the eye.</param>
        /// <param name="gaze">The gaze rotation of the eye.</param>
        /// <param name="centerConfidence">The confidence value of the center position.</param>
        /// <param name="isBlinking">The blinking state of the eye.</param>
        internal void Update(Vector3 center, Quaternion gaze, float centerConfidence, bool isBlinking)
        {
            this.Center = center;
            this.Gaze = gaze;
            this.CenterConfidence = centerConfidence;
            this.IsBlinking = isBlinking;
        }
        #endif
    }

    /// <summary>
    /// MLEyes class contains all Eye tracking data for both left and right eyes.
    /// </summary>
    public sealed partial class MLEyes : MLAPISingleton<MLEyes>
    {
        /// <summary>
        /// Enumeration for the eye calibration status.
        /// </summary>
        [Obsolete("Please use MLEyes.Calibration instead.", true)]
        public enum MLEyeTrackingCalibrationStatus
        {
            /// <summary>
            /// Eye calibration has not been performed.
            /// </summary>
            MLEyeTrackingCalibrationStatus_None = 0,

            /// <summary>
            /// The eye calibration data is bad.
            /// </summary>
            MLEyeTrackingCalibrationStatus_Bad = 1,

            /// <summary>
            /// The eye calibration data is good.
            /// </summary>
            MLEyeTrackingCalibrationStatus_Good = 2
        }
    }
}
