// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLImageTargetResult.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// Represents the image target result.
    /// </summary>
    [Obsolete("Please use MLImageTracker.Target.Result instead.", true)]
    public struct MLImageTargetResult
    {
        /// <summary>
        /// Position of the target.
        /// This is not valid if the target is not being tracked.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Orientation of the target.
        /// This is not valid if the target is not being tracked.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Status of the target.
        /// Every target will have an associated status indicating the current
        /// tracking status.
        /// </summary>
        public MLImageTargetTrackingStatus Status;
    }
}

#endif
