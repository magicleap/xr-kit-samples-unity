// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLEyes.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using System.Collections.Generic;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;

    #if !UNITY_2019_3_OR_NEWER
    using UnityEngine.Experimental.XR.MagicLeap;
    #endif
    #endif

    /// <summary>
    /// MLEyes class contains all Eye tracking data for both left and right eyes.
    /// </summary>
    public sealed partial class MLEyes : MLAPISingleton<MLEyes>
    {
        /// <summary>
        /// Enumeration for the eye calibration status.
        /// </summary>
        public enum Calibration
        {
            /// <summary>
            /// Eye calibration has not been performed.
            /// </summary>
            None = 0,

            /// <summary>
            /// The eye calibration data is bad.
            /// </summary>
            Bad = 1,

            /// <summary>
            /// The eye calibration data is good.
            /// </summary>
            Good = 2
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Gets the MLEye data for the left eye.
        /// </summary>
        /// <returns>Returns a reference to left eye.</returns>
        public static MLEye LeftEye { get; private set; }

        /// <summary>
        /// Gets the MLEye data for the right eye.
        /// </summary>
        /// <returns>Returns a reference to right eye.</returns>
        public static MLEye RightEye { get; private set; }

        /// <summary>
        /// Gets a quality metric to indicate the accuracy of the gaze.
        /// </summary>
        /// <returns>Returns a normalized value for the confidence of the fixation point.</returns>
        public static float FixationConfidence { get; private set; }

        /// <summary>
        /// Gets the calibration status for eye tracking.
        /// </summary>
        /// <returns>Returns the status of eye calibration.</returns>
        public static Calibration CalibrationStatus { get; private set; }

        /// <summary>
        /// Gets the current fixation point.
        /// </summary>
        /// <returns>Returns the fixtion point of a users gaze.</returns>
        public static Vector3 FixationPoint { get; private set; }

        /// <summary>
        /// Starts the Eyes API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c>
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to an internal error.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLEyes.BaseStart(true);
        }

        #if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Starts the eye object requests, Must be called to start receiving eye tracker data from the underlying system.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c>
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to an internal error.
        /// </returns>
        protected override MLResult StartAPI()
        {
            // Attempt to start the tracker & validate.
            MLEyeNativeBindings.SetEyeTrackerActive(true);
            if (!MLEyeNativeBindings.GetEyeTrackerActive())
            {
                MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "UnityMagicLeap - SetEyeTrackerActive() failed to start the tracker.");
                MLPluginLog.ErrorFormat("MLEyes.StartAPI failed to initialize native eye tracker. Reason: {0}", result);

                return result;
            }

            LeftEye = new MLEye(MLEye.EyeType.Left);
            RightEye = new MLEye(MLEye.EyeType.Right);

            return MLResult.Create(MLResult.Code.Ok);
        }
        #endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Cleans up unmanaged memory
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Informs the cleanup process it's safe to clear the initialized MLEye(s).</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            if (isSafeToAccessManagedObjects)
            {
                LeftEye = null;
                RightEye = null;
            }

            // Attempt to stop the tracker.
            MLEyeNativeBindings.SetEyeTrackerActive(false);
        }

        /// <summary>
        /// Update all eye data.
        /// </summary>
        protected override void Update()
        {
            List<InputDevice> devices = new List<InputDevice>();

            #if UNITY_2019_3_OR_NEWER
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking, devices);
            #else
            InputDevices.GetDevicesAtXRNode(XRNode.Head, devices);
            #endif

            foreach (var device in devices)
            {
                // Controller/EyeTracking - use the same definition, so we must check.
                if (device.isValid)
                {
                    // Eyes
                    if (device.TryGetFeatureValue(CommonUsages.eyesData, out UnityEngine.XR.Eyes deviceEyes))
                    {
                        if (LeftEye != null)
                        {
                            this.UpdateEye(device, deviceEyes, MLEye.EyeType.Left);
                        }

                        if (RightEye != null)
                        {
                            this.UpdateEye(device, deviceEyes, MLEye.EyeType.Right);
                        }

                        // Fixation Point
                        if (deviceEyes.TryGetFixationPoint(out Vector3 deviceFixationPoint))
                        {
                            MLEyes.FixationPoint = deviceFixationPoint;
                        }

                        // Fixation Confidence
                        if (device.TryGetFeatureValue(MagicLeapHeadUsages.FixationConfidence, out float deviceFixationConfidence))
                        {
                            MLEyes.FixationConfidence = deviceFixationConfidence;
                        }

                        // Calibration Status
                        if (device.TryGetFeatureValue(MagicLeapHeadUsages.EyeCalibrationStatus, out uint deviceCalibrationStatus))
                        {
                            MLEyes.CalibrationStatus = (Calibration)deviceCalibrationStatus;
                        }
                    }
                }
                else
                {
                    MLPluginLog.Error("MLEyes.Update failed to locate a valid tracker.");
                }
            }
        }

        /// <summary>
        /// Creates a new instance of MLEyes.
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLEyes.IsValidInstance())
            {
                MLEyes._instance = new MLEyes();
            }
        }

        /// <summary>
        /// Updates an individual MLEye with the latest information from the XR.Eyes.
        /// </summary>
        /// <param name="device">The Unity XR input device.</param>
        /// <param name="deviceEyes">The Unity XR eyes.</param>
        /// <param name="type">The eye type to update.</param>
        private void UpdateEye(InputDevice device, UnityEngine.XR.Eyes deviceEyes, MLEye.EyeType type)
        {
            // Early exist if any of the request methods fail.

            // Center
            Vector3 deviceEyeCenter;
            if ((type == MLEye.EyeType.Left) ?
                !deviceEyes.TryGetLeftEyePosition(out deviceEyeCenter) :
                !deviceEyes.TryGetRightEyePosition(out deviceEyeCenter))
            {
                return;
            }

            // Gaze
            Quaternion deviceEyeGaze;
            if ((type == MLEye.EyeType.Left) ?
                !deviceEyes.TryGetLeftEyeRotation(out deviceEyeGaze) :
                !deviceEyes.TryGetRightEyeRotation(out deviceEyeGaze))
            {
                return;
            }

            // Blink
            float deviceEyeBlink;
            if ((type == MLEye.EyeType.Left) ?
                !deviceEyes.TryGetLeftEyeOpenAmount(out deviceEyeBlink) :
                !deviceEyes.TryGetRightEyeOpenAmount(out deviceEyeBlink))
            {
                return;
            }

            // Center Confidence
            // Note: These values are not exposed via UnityEngine.XR.Eyes.
            // Instead they are pulled through a custom implementation.
            float deviceCenterConfidence;
            if ((type == MLEye.EyeType.Left) ?
                !device.TryGetFeatureValue(MagicLeapHeadUsages.EyeLeftCenterConfidence, out deviceCenterConfidence) :
                !device.TryGetFeatureValue(MagicLeapHeadUsages.EyeRightCenterConfidence, out deviceCenterConfidence))
            {
                return;
            }

            // Update the requested eye.
            if (type == MLEye.EyeType.Left)
            {
                LeftEye.Update(deviceEyeCenter, deviceEyeGaze, deviceCenterConfidence, (deviceEyeBlink == 0f) ? true : false);
            }
            else
            {
                RightEye.Update(deviceEyeCenter, deviceEyeGaze, deviceCenterConfidence, (deviceEyeBlink == 0f) ? true : false);
            }
        }
        #endif

        /// <summary>
        /// Class used to represents a single eye.
        /// </summary>
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
    }
}
