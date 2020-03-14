// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLWrist.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;

    #if !UNITY_2019_3_OR_NEWER
    using UnityEngine.Experimental.XR.MagicLeap;
    #endif

    [Obsolete("Please use MLHandTracking.Wrist instead.")]
    public sealed class MLWrist
    {
        /// <summary>
        /// The center of the  wrist.
        /// </summary>
        public MLKeyPoint Center { get; private set; }

        /// <summary>
        /// The Ulnar of the wrist.
        /// </summary>
        public MLKeyPoint Ulnar { get; private set; }

        /// <summary>
        /// The Radial of the wrist.
        /// </summary>
        public MLKeyPoint Radial { get; private set; }

        /// <summary>
        /// All valid keypoints combined into a list.
        /// </summary>
        public List<MLKeyPoint> KeyPoints { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MLWrist class.
        /// </summary>
        public MLWrist()
        {
            Center = new MLKeyPoint(true);
            Ulnar = new MLKeyPoint(false);
            Radial = new MLKeyPoint(false);
            // Assign all valid KeyPoints to an easy to access list.
            KeyPoints = new List<MLKeyPoint>();
            if (Center.IsSupported)
            {
                KeyPoints.Add(Center);
            }
            if (Ulnar.IsSupported)
            {
                KeyPoints.Add(Ulnar);
            }
            if (Radial.IsSupported)
            {
                KeyPoints.Add(Radial);
            }
        }
        /// <summary>
        /// Updates the state of the MLWrist.
        /// </summary>
        public void Update(InputDevice device)
        {
            // Center - Wrist center.
            if (Center.IsSupported)
            {
                if (device.TryGetFeatureValue(MagicLeapHandUsages.WristCenter, out Vector3 deviceWristCenter))
                {
                    Center.Position = deviceWristCenter;
                }
            }

            // Ulnar - Ulnar-sided wrist.
            if (Ulnar.IsSupported)
            {
                if (device.TryGetFeatureValue(MagicLeapHandUsages.WristUlnar, out Vector3 deviceWristUlnar))
                {
                    Ulnar.Position = deviceWristUlnar;
                }
            }

            // Radial - Radial-sided wrist.
            if (Radial.IsSupported)
            {
                if (device.TryGetFeatureValue(MagicLeapHandUsages.WristRadial, out Vector3 deviceWristRadial))
                {
                    Radial.Position = deviceWristRadial;
                }
            }
        }
    }
}

#endif
