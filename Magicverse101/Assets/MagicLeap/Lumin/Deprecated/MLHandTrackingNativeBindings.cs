// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHandTrackingNativeBindings.cs" company="Magic Leap, Inc">
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
    /// Static key pose types which are available when both hands are separated.
    /// </summary>
    [Obsolete("Please use MLHandTracking.HandKeyPose instead.")]
    public enum MLHandKeyPose
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
    [Obsolete("Please use MLHandTracking.KeyPointFilterLevel instead.")]
    public enum MLKeyPointFilterLevel
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
    [Obsolete("Please use MLHandTracking.PoseFilterLevel instead.")]
    public enum MLPoseFilterLevel
    {
        /// <summary>
        /// Default value, No filtering, the poses are raw.
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
    [Obsolete("Please use MLHandTracking.HandType instead.")]
    public enum MLHandType
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
}
#endif
