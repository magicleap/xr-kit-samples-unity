// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHandTrackingWrist.cs" company="Magic Leap, Inc">
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
        /// Wrist contains joint information about the wrist instance.
        /// </summary>
        public sealed class Wrist
        {
            /// <summary>
            /// Initializes a new instance of the Wrist class.
            /// </summary>
            public Wrist()
            {
                this.Center = new KeyPoint(true);
                this.Ulnar = new KeyPoint(false);
                this.Radial = new KeyPoint(false);

                // Assign all valid KeyPoints to an easy to access list.
                this.KeyPoints = new List<KeyPoint>();
                if (this.Center.IsSupported)
                {
                    this.KeyPoints.Add(this.Center);
                }

                if (this.Ulnar.IsSupported)
                {
                    this.KeyPoints.Add(this.Ulnar);
                }

                if (this.Radial.IsSupported)
                {
                    this.KeyPoints.Add(this.Radial);
                }
            }

            /// <summary>
            /// Gets the center of the wrist.
            /// </summary>
            public KeyPoint Center { get; private set; }

            /// <summary>
            /// Gets the ulnar of the wrist.
            /// </summary>
            public KeyPoint Ulnar { get; private set; }

            /// <summary>
            /// Gets the radial of the wrist.
            /// </summary>
            public KeyPoint Radial { get; private set; }

            /// <summary>
            /// Gets all the valid key points combined into a list.
            /// </summary>
            public List<KeyPoint> KeyPoints { get; private set; }

            /// <summary>
            /// Updates the state of the Wrist.
            /// </summary>
            /// <param name="device">The device to use for obtaining wrist features.</param>
            public void Update(InputDevice device)
            {
                // Center - Wrist center.
                if (this.Center.IsSupported)
                {
                    if (device.TryGetFeatureValue(MagicLeapHandUsages.WristCenter, out Vector3 deviceWristCenter))
                    {
                        this.Center.Position = deviceWristCenter;
                    }
                }

                // Ulnar - Ulnar-sided wrist.
                if (this.Ulnar.IsSupported)
                {
                    if (device.TryGetFeatureValue(MagicLeapHandUsages.WristUlnar, out Vector3 deviceWristUlnar))
                    {
                        this.Ulnar.Position = deviceWristUlnar;
                    }
                }

                // Radial - Radial-sided wrist.
                if (this.Radial.IsSupported)
                {
                    if (device.TryGetFeatureValue(MagicLeapHandUsages.WristRadial, out Vector3 deviceWristRadial))
                    {
                        this.Radial.Position = deviceWristRadial;
                    }
                }
            }
        }
    }
}

#endif
