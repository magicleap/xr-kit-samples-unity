// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHandTrackingHand.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;

    #if !UNITY_2019_3_OR_NEWER && PLATFORM_LUMIN
    using UnityEngine.Experimental.XR.MagicLeap;
    #endif

    /// <summary>
    /// MLHandTracking is the entry point for all the hand tracking data
    /// including gestures, hand centers and key points for both hands.
    /// </summary>
    public partial class MLHandTracking
    {
        /// <summary>
        /// Hand contains the hand tracking data including gestures,
        /// hand centers and key points for a specific hand.
        /// </summary>
        public partial class Hand
        {
            /// <summary>
            /// Raw byte array representing the confidence values of each key pose.
            /// </summary>
            private byte[] rawKeyposeConfidence = new byte[NativeBindings.MaxKeyPoses * sizeof(float)];

            /// <summary>
            /// Raw byte array representing the filtered confidence values of each key pose.
            /// </summary>
            private byte[] rawKeyposeConfidenceFiltered = new byte[NativeBindings.MaxKeyPoses * sizeof(float)];

            /// <summary>
            /// Float array for the confidence values of each key pose.
            /// </summary>
            private float[] handKeyposeConfidence = new float[NativeBindings.MaxKeyPoses];

            /// <summary>
            /// Float array for the filtered confidence values of each key pose.
            /// </summary>
            private float[] keyposeConfidenceFiltered = new float[NativeBindings.MaxKeyPoses];

            /// <summary>
            /// Initializes a new instance of the <see cref="Hand" /> class.
            /// </summary>
            /// <param name="handType">The hand type of this hand.</param>
            public Hand(MLHandTracking.HandType handType)
            {
                this.Type = handType;

                this.Index = new Finger(3);
                this.Middle = new Finger(3);
                this.Ring = new Finger(2);
                this.Pinky = new Finger(2);
                this.Thumb = new Thumb();
                this.Wrist = new Wrist();

                this.Center = Vector3.zero;
                this.NormalizedCenter = Vector3.zero;

                this.KeyPose = MLHandTracking.HandKeyPose.NoHand;
            }

            /// <summary>
            /// The delegate for when a key pose has been first detected.
            /// </summary>
            /// <param name="pose">The pose that has been detected.</param>
            public delegate void OnKeyPoseBeginDelegate(MLHandTracking.HandKeyPose pose);

            /// <summary>
            /// The delegate for when a previously detected key pose is no longer detected.
            /// </summary>
            /// <param name="pose">The pose that has no longer been detected.</param>
            public delegate void OnKeyPoseEndDelegate(MLHandTracking.HandKeyPose pose);

            /// <summary>
            /// Event is raised whenever a key pose starts being recognized for this hand.
            /// </summary>
            public event OnKeyPoseBeginDelegate OnHandKeyPoseBegin = delegate { };

            /// <summary>
            /// Event is raised whenever a key pose stops being recognized for this hand.
            /// </summary>
            public event OnKeyPoseEndDelegate OnHandKeyPoseEnd = delegate { };

            /// <summary>
            /// Gets currently recognized key pose.
            /// </summary>
            public MLHandTracking.HandKeyPose KeyPose { get; private set; }

            /// <summary>
            /// Gets the confidence value of the requested key pose, between [0, 1].
            /// </summary>
            public float HandKeyPoseConfidence
            {
                get
                {
                    return this.handKeyposeConfidence[(int)this.KeyPose];
                }
            }

            /// <summary>
            /// Gets the filtered confidence value of the requested key pose, between [0, 1].
            /// </summary>
            public float HandKeyPoseConfidenceFiltered
            {
                get
                {
                    return this.keyposeConfidenceFiltered[(int)this.KeyPose];
                }
            }

            /// <summary>
            /// Gets the Hand Type (left or right).
            /// </summary>
            public MLHandTracking.HandType Type { get; private set; }

            /// <summary>
            /// Gets the Thumb.
            /// </summary>
            public Thumb Thumb { get; private set; }

            /// <summary>
            /// Gets the index finger.
            /// </summary>
            public Finger Index { get; private set; }

            /// <summary>
            /// Gets the middle finger.
            /// </summary>
            public Finger Middle { get; private set; }

            /// <summary>
            /// Gets the ring finger.
            /// </summary>
            public Finger Ring { get; private set; }

            /// <summary>
            /// Gets the pinky finger.
            /// </summary>
            public Finger Pinky { get; private set; }

            /// <summary>
            /// Gets the wrist.
            /// </summary>
            public Wrist Wrist { get; private set; }

            /// <summary>
            /// Gets hand center.
            /// Remains in same location if hand is not detected.
            /// </summary>
            public Vector3 Center { get; private set; }

            /// <summary>
            /// Gets normalized center of the hand.
            /// Remains in same location if hand is not detected.
            /// </summary>
            public Vector3 NormalizedCenter { get; private set; }

            /// <summary>
            /// Gets the confidence value of the hand, between [0, 1].
            /// </summary>
            public float HandConfidence { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the hand is "visible".
            /// Visibility is based on if the HandConfidence is above 0.8, and the key pose is not NoHand.
            /// </summary>
            public bool IsVisible
            {
                get
                {
                    return this.HandConfidence > 0.8f && this.KeyPose != MLHandTracking.HandKeyPose.NoHand;
                }
            }

            /// <summary>
            /// Method to call when a pose is detected for the first time.
            /// </summary>
            /// <param name="pose">The pose that has no longer been detected.</param>
            public void BeginKeyPose(MLHandTracking.HandKeyPose pose)
            {
                this.KeyPose = pose;
                this.OnHandKeyPoseBegin?.Invoke(pose);
            }

            /// <summary>
            /// Method to call when a pose is no longer detected.
            /// </summary>
            /// <param name="pose">The pose that has no longer been detected.</param>
            public void EndKeyPose(MLHandTracking.HandKeyPose pose)
            {
                this.KeyPose = pose;
                this.OnHandKeyPoseEnd?.Invoke(pose);
            }

            /// <summary>
            /// Updates Hand state variables. Note: It's not advisable
            /// to call this outside the context of the MLHandTracking and MLHandTracking.KeyPoseManager.
            /// </summary>
            /// <param name="device">Input device containing latest input data.</param>
            public void Update(InputDevice device)
            {
                // Hand Confidence
                if (device.TryGetFeatureValue(MagicLeapHandUsages.Confidence, out float deviceHandConfidence))
                {
                    this.HandConfidence = deviceHandConfidence;
                }

                this.UpdateKeyPoints(device);
                this.UpdateConfidenceValues(device);
            }

            /// <summary>
            /// Updates bones based on the current poses of key points.
            /// </summary>
            /// <param name="device">Input device containing latest input data.</param>
            private void UpdateKeyPoints(InputDevice device)
            {
                // Center
                if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 deviceCenter))
                {
                    this.Center = deviceCenter;
                }

                // Normalized Center
                if (device.TryGetFeatureValue(MagicLeapHandUsages.NormalizedCenter, out Vector3 deviceNormalizedCenter))
                {
                    this.NormalizedCenter = deviceNormalizedCenter;
                }

                // Hand Key Points
                if (device.TryGetFeatureValue(CommonUsages.handData, out UnityEngine.XR.Hand deviceHand))
                {
                    List<UnityEngine.XR.Bone> indexBones = new List<UnityEngine.XR.Bone>();
                    if (deviceHand.TryGetFingerBones(UnityEngine.XR.HandFinger.Index, indexBones))
                    {
                        this.Index.Update(indexBones);
                    }

                    List<UnityEngine.XR.Bone> middleBones = new List<UnityEngine.XR.Bone>();
                    if (deviceHand.TryGetFingerBones(UnityEngine.XR.HandFinger.Middle, middleBones))
                    {
                        this.Middle.Update(middleBones);
                    }

                    List<UnityEngine.XR.Bone> ringBones = new List<UnityEngine.XR.Bone>();
                    if (deviceHand.TryGetFingerBones(UnityEngine.XR.HandFinger.Ring, ringBones))
                    {
                        this.Ring.Update(ringBones);
                    }

                    List<UnityEngine.XR.Bone> pinkyBones = new List<UnityEngine.XR.Bone>();
                    if (deviceHand.TryGetFingerBones(UnityEngine.XR.HandFinger.Pinky, pinkyBones))
                    {
                        this.Pinky.Update(pinkyBones);
                    }

                    List<UnityEngine.XR.Bone> thumbBones = new List<UnityEngine.XR.Bone>();
                    if (deviceHand.TryGetFingerBones(UnityEngine.XR.HandFinger.Thumb, thumbBones))
                    {
                        this.Thumb.Update(thumbBones);
                    }

                    // Not supported with XR.Bone, process manually.
                    this.Wrist.Update(device);
                }
            }

            /// <summary>
            /// Updates the confidence values for each key pose.
            /// </summary>
            /// <param name="device">Input device containing latest input data.</param>
            private void UpdateConfidenceValues(InputDevice device)
            {
                if (device.TryGetFeatureValue(MagicLeapHandUsages.KeyPoseConfidence, this.rawKeyposeConfidence))
                {
                    this.handKeyposeConfidence = MagicLeapInputUtility.ParseData(this.rawKeyposeConfidence);
                }

                if (device.TryGetFeatureValue(MagicLeapHandUsages.KeyPoseConfidenceFiltered, this.rawKeyposeConfidenceFiltered))
                {
                    this.keyposeConfidenceFiltered = MagicLeapInputUtility.ParseData(this.rawKeyposeConfidenceFiltered);
                }
            }
        }
    }
}

#endif
