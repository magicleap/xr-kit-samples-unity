// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHandTracking.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// MLHandTracking is the entry point for all the hand tracking data
    /// including gestures, hand centers and key points for both hands.
    /// </summary>
    public partial class MLHandTracking : MLAPISingleton<MLHandTracking>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// The object to manage when different key poses start and end.
        /// </summary>
        private KeyposeManager keyposeManager;

        /// <summary>
        /// Right hand class used to get right hand specific data.
        /// </summary>
        private Hand right;

        /// <summary>
        /// Left hand class used to get left hand specific data.
        /// </summary>
        private Hand left;
        #endif

        /// <summary>
        /// Static key pose types which are available when both hands are separated.
        /// </summary>
        public enum HandKeyPose
        {
            /// <summary>
            /// Index finger.
            /// </summary>
            Finger,

            /// <summary>A
            /// A closed fist.
            /// </summary>
            Fist,

            /// <summary>
            /// A pinch.
            /// </summary>
            Pinch,

            /// <summary>
            /// A closed fist with the thumb pointed up.
            /// </summary>
            Thumb,

            /// <summary>
            /// An L shape
            /// </summary>
            L,

            /// <summary>
            /// An open hand.
            /// </summary>
            OpenHand = 5,

            /// <summary>
            /// A pinch with all fingers, except the index finger and the thumb, extended out.
            /// </summary>
            Ok,

            /// <summary>
            /// A rounded 'C' alphabet shape.
            /// </summary>
            C,

            /// <summary>
            /// No pose was recognized.
            /// </summary>
            NoPose,

            /// <summary>
            /// No hand was detected. Should be the last pose.
            /// </summary>
            NoHand
        }

        /// <summary>
        /// Configured level for key points filtering of key points and hand centers.
        /// </summary>
        public enum KeyPointFilterLevel
        {
            /// <summary>
            /// Default value, no filtering is done, the points are raw.
            /// </summary>
            Raw,

            /// <summary>
            /// Some smoothing at the cost of latency.
            /// </summary>
            Smoothed,

            /// <summary>
            /// Predictive smoothing, at higher cost of latency.
            /// </summary>
            ExtraSmoothed
        }

        /// <summary>
        /// Configured level of filtering for static poses.
        /// </summary>
        public enum PoseFilterLevel
        {
            /// <summary>
            /// Default value, no filtering, the poses are raw.
            /// </summary>
            Raw,

            /// <summary>
            /// Some robustness to flicker at some cost of latency.
            /// </summary>
            Robust,

            /// <summary>
            /// More robust to flicker at higher latency cost.
            /// </summary>
            ExtraRobust
        }

        /// <summary>
        /// Represents if a hand is the right or left hand.
        /// </summary>
        public enum HandType
        {
            /// <summary>
            /// Left hand.
            /// </summary>
            Left,

            /// <summary>
            /// Right hand.
            /// </summary>
            Right
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Gets the key pose manager of the instance.
        /// </summary>
        public static KeyposeManager KeyPoseManager
        {
            get
            {
                if (MLHandTracking.IsValidInstance())
                {
                    return _instance.keyposeManager;
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLHandTracking.KeyPoseManager failed. Reason: No Instance for MLHandTracking.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the left hand.
        /// </summary>
        public static Hand Left
        {
            get
            {
                if (MLHandTracking.IsValidInstance())
                {
                    return _instance.left;
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLHandTracking.Left failed. Reason: No Instance for MLHandTracking.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the right hand.
        /// </summary>
        public static Hand Right
        {
            get
            {
                if (MLHandTracking.IsValidInstance())
                {
                    return _instance.right;
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLHandTracking.Right failed. Reason: No Instance for MLHandTracking.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Starts the HandTracking API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLHandTracking.BaseStart(true);
        }

        #if !DOXYGENSHOULDSKIPTHIS
        /// <summary>
        /// Start tracking hands with all key poses disabled.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed to initialize the native hand tracker.
        /// </returns>
        protected override MLResult StartAPI()
        {
            this.left = new Hand(MLHandTracking.HandType.Left);
            this.right = new Hand(MLHandTracking.HandType.Right);

            // Initialize KeyPoseManager, to register the gesture subsystem.
            this.keyposeManager = new KeyposeManager(Left, Right);

            try
            {
                // Attempt to start the tracker & validate.
                NativeBindings.SetHandGesturesEnabled(true);
                if (!NativeBindings.IsHandGesturesEnabled())
                {
                    MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLHandTracking.StartAPI failed to initialize the native hand tracker.");
                    MLPluginLog.Error("MLHandTracking.StartAPI failed to initialize the native hand tracker.");
                    return result;
                }
            }
            catch (EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLHandTracking.StartAPI failed. Reason: API symbols not found.");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLHandTracking.StartAPI failed. Reason: API symbols not found.");
            }

            return MLResult.Create(MLResult.Code.Ok);
        }

        /// <summary>
        /// Cleans up API and unmanaged memory.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Allow complete cleanup of the API.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            if (isSafeToAccessManagedObjects)
            {
                // The KeyPoseManager object will not receive any more updates from Left or Right hands.
                this.keyposeManager = null;
                this.left = null;
                this.right = null;
            }

            try
            {
                // Attempt to stop the tracker.
                NativeBindings.SetHandGesturesEnabled(false);
            }
            catch (EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLHandTracking.CleanupAPI failed. Reason: API symbols not found");
            }
        }
        #endif // DOXYGENSHOULDSKIPTHIS

        /// <summary>
        /// Updates the key pose state based on the provided snapshot.
        /// </summary>
        protected override void Update()
        {

            List<InputDevice> leftHandDevices = new List<InputDevice>();

            #if UNITY_2019_3_OR_NEWER
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Left, leftHandDevices);
            #else
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
            #endif

            if (leftHandDevices.Count > 0 && leftHandDevices[0].isValid)
            {
                this.left.Update(leftHandDevices[0]);
            }

            List<InputDevice> rightHandDevices = new List<InputDevice>();

            #if UNITY_2019_3_OR_NEWER
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Right, rightHandDevices);
            #else
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);
            #endif

            if (rightHandDevices.Count > 0 && rightHandDevices[0].isValid)
            {
                this.right.Update(rightHandDevices[0]);
            }
        }

        /// <summary>
        /// Static instance of the MLHandTracking class.
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLHandTracking.IsValidInstance())
            {
                MLHandTracking._instance = new MLHandTracking();
            }
        }
        #endif
    }
}
