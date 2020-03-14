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
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using MagicLeap.Core.StarterKit;

namespace MagicLeap.Core
{
    /// <summary>
    /// KeyPoseTypes flags enumeration. This enumeration lists the MLHandTracking.HandKeyPose enumerations as Flags so that
    /// more than one keyposes can be easily selected from the inspector.
    /// </summary>
    [Flags]
    public enum KeyPoseTypes
    {
        Finger = (1 << MLHandTracking.HandKeyPose.Finger),
        Fist = (1 << MLHandTracking.HandKeyPose.Fist),
        Pinch = (1 << MLHandTracking.HandKeyPose.Pinch),
        Thumb = (1 << MLHandTracking.HandKeyPose.Thumb),
        L = (1 << MLHandTracking.HandKeyPose.L),
        OpenHand = (1 << MLHandTracking.HandKeyPose.OpenHand),
        Ok = (1 << MLHandTracking.HandKeyPose.Ok),
        C = (1 <<  MLHandTracking.HandKeyPose.C),
        NoPose = (1 <<  MLHandTracking.HandKeyPose.NoPose)
    }

    /// <summary>
    /// Component used to call Start on MLHandTrackingStarterKit and manage
    /// which key poses are currently being tracked by each hand.
    /// key poses can be added and removed from the tracker during runtime.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/MLHandTrackingBehavior")]
    public class MLHandTrackingBehavior : MonoBehaviour
    {
        [Space, SerializeField, MLBitMask(typeof(KeyPoseTypes)), Tooltip("All KeyPoses to be tracked")]
        private KeyPoseTypes _trackedKeyPoses;

        [SerializeField, Tooltip("Configured level for filtering of keypoints and hand centers.")]
        private MLHandTracking.KeyPointFilterLevel _keyPointFilterLevel = MLHandTracking.KeyPointFilterLevel.ExtraSmoothed;

        [SerializeField, Tooltip("Configured level for filtering of hand poses.")]
        private MLHandTracking.PoseFilterLevel _poseFilterLevel = MLHandTracking.PoseFilterLevel.ExtraRobust;

        public KeyPoseTypes TrackedKeyPoses { get; private set; }

        void Start()
        {
            #if PLATFORM_LUMIN
            MLResult result = MLHandTrackingStarterKit.Start();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLHandTrackingBehavior failed on MLHandTrackingStarterKit.Start, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }
            #endif

            UpdateKeyPoseStates();

            MLHandTrackingStarterKit.SetKeyPointsFilterLevel(_keyPointFilterLevel);
            MLHandTrackingStarterKit.SetPoseFilterLevel(_poseFilterLevel);
        }

        /// <summary>
        /// Disables all key poses and calls Stop on MLHandTrackingStarterKit.
        /// </summary>
        void OnDestroy()
        {
            // Disable all key poses if MLHandTrackingStarterKit was started
            _trackedKeyPoses &= 0;
            UpdateKeyPoseStates();
            MLHandTrackingStarterKit.Stop();
        }

        /// <summary>
        /// Update key poses tracked if enum value changed.
        /// </summary>
        void Update()
        {
            if ((_trackedKeyPoses ^ TrackedKeyPoses) != 0)
            {
                UpdateKeyPoseStates();
            }
        }

        /// <summary>
        /// Adds KeyPose if it's not there already.
        /// </summary>
        /// <param name="keyPose"> KeyPose to add.</param>
        public void AddKeyPose(KeyPoseTypes keyPose)
        {
            if ((keyPose & _trackedKeyPoses) != keyPose)
            {
                _trackedKeyPoses |= keyPose;
                UpdateKeyPoseStates();
            }
        }

        /// <summary>
        /// Removes KeyPose if it's there.
        /// </summary>
        /// <param name="keyPose"> KeyPose to remove.</param>
        public void RemoveKeyPose(KeyPoseTypes keyPose)
        {
            if ((keyPose & _trackedKeyPoses) == keyPose)
            {
                _trackedKeyPoses ^= keyPose;
                UpdateKeyPoseStates();
            }
        }

        /// <summary>
        /// Get the key poses enabled.
        /// </summary>
        private MLHandTracking.HandKeyPose[] GetKeyPoseTypes()
        {
            int[] enumValues = (int[])Enum.GetValues(typeof(KeyPoseTypes));
            List<MLHandTracking.HandKeyPose> keyPoses = new List<MLHandTracking.HandKeyPose>();

            TrackedKeyPoses = 0;
            KeyPoseTypes current;
            for (int i = 0; i < enumValues.Length; ++i)
            {
                current = (KeyPoseTypes)enumValues[i];
                if ((_trackedKeyPoses & current) == current)
                {
                    TrackedKeyPoses |= current;
                    keyPoses.Add((MLHandTracking.HandKeyPose)i);
                }
            }

            return keyPoses.ToArray();
        }

        /// <summary>
        /// Updates the list of key poses internal to the magic leap device,
        /// enabling or disabling key poses that should be tracked.
        /// </summary>
        private void UpdateKeyPoseStates()
        {
            MLHandTracking.HandKeyPose[] keyPoseTypes = GetKeyPoseTypes();

            // Early out in case there are no key poses to enable.
            if (keyPoseTypes.Length == 0)
            {
                MLHandTrackingStarterKit.DisableKeyPoses();
                return;
            }

            bool status = MLHandTrackingStarterKit.EnableKeyPoses(true, keyPoseTypes);
            if (!status)
            {
                Debug.LogError("Error: MLHandTrackingBehavior failed enabling tracked key poses, disabling script.");
                enabled = false;
                return;
            }
        }
    }
}
