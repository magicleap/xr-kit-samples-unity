// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLImageTrackerTargetSettings.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Identifies the status of an image target.
    /// Each MLImageTargetResult will include a ImageTrackerTargetStatus
    /// giving the current status of the target.
    /// </summary>
    [Obsolete("Please use MLImageTracker.Target.TrackingStatus instead.", false)]
    public enum MLImageTargetTrackingStatus
    {
        /// <summary>
        /// Image target is tracked.
        /// The image tracker system will provide a 6 DOF pose when queried using
        /// MLSnapshotGetTransform function.
        /// </summary>
        Tracked = MLImageTracker.Target.TrackingStatus.Tracked,

        /// <summary>
        /// Image target is tracked with low confidence.
        /// The image tracker system will still provide a 6 DOF pose. But this
        /// pose might be inaccurate and might have jitter. When the tracking is
        /// unreliable one of the following two events will happen quickly : Either the
        /// tracking will recover to MLImageTrackerTargetStatus_Tracked or tracking
        /// will be lost and the status will change to
        /// MLImageTrackerTargetStatus_NotTracked.
        /// </summary>
        Unreliable = MLImageTracker.Target.TrackingStatus.Unreliable,

        /// <summary>
        /// Image target is lost.
        /// The image tracker system will not report any pose for this target. Querying
        /// for the pose using MLSnapshotGetTransform will return false.
        /// </summary>
        NotTracked = MLImageTracker.Target.TrackingStatus.NotTracked
    }

    #if PLATFORM_LUMIN
    /// <summary>
    /// Represents the settings of an Image Target.
    /// All fields are required for an Image Target to be tracked.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MLImageTrackerTargetSettings
    {
        /// <summary>
        /// Name of the target.
        /// This name has to be unique across all targets added to the Image Tracker.
        /// Caller should allocate memory for this.
        /// Encoding should be in UTF8.
        /// This will be copied internally.
        /// </summary>
        public string Name;

        /// <summary>
        /// LongerDimension refer to the size of the longer dimension of the physical Image.
        /// Target in Unity scene units.
        /// </summary>
        public float LongerDimension;

        /// <summary>
        /// Set this to \c true to improve detection for stationary targets.
        /// An Image Target is a stationary target if its position in the physical world does not change.
        /// This is best suited for cases where the target is stationary and when the local geometry (environment surrounding the target) is planar.
        /// When in doubt set this to false.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsStationary;

        /// <summary>
        /// Set this to \c true to track the image target.
        /// Disabling the target when not needed marginally improves the tracker CPU performance.
        /// This is best suited for cases where the target is temporarily not needed.
        /// If the target no longer needs to be tracked it is best to use remove the target.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsEnabled;

        /// <summary>
        /// Create and return an initialized version of this struct.
        /// </summary>
        /// <returns>An initialized version of this struct.</returns>
        public static MLImageTrackerTargetSettings Create()
        {
            return new MLImageTrackerTargetSettings
            {
                Name = string.Empty,
                LongerDimension = 0.0f,
                IsStationary = false,
                IsEnabled = false
            };
        }
    }
    #endif
}
