// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLHeadTrackingNativeBindings.cs" company="Magic Leap, Inc">
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
    /// A set of possible error conditions that can cause Head Tracking to
    /// be less than ideal.
    /// </summary>
    [Obsolete("Please use MLHeadTracking.TrackingError instead.", true)]
    public enum MLHeadTrackingError
    {
        /// <summary>
        /// No error, tracking is nominal.
        /// </summary>
        None,

        /// <summary>
        /// There are not enough features in the environment.
        /// </summary>
        NotEnoughFeatures,

        /// <summary>
        /// Lighting in the environment is not sufficient to track accurately.
        /// </summary>
        LowLight,

        /// <summary>
        /// Head tracking failed for an unkown reason.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// A set of possible tracking modes the Head Tracking system can be in.
    /// </summary>
    [Obsolete("Please use MLHeadTracking.TrackingMode instead.", true)]
    public enum MLHeadTrackingMode
    {
        /// <summary>
        /// Full 6 degrees of freedom tracking (position and orientation).
        /// </summary>
        Mode6DOF,

        /// <summary>
        /// Head tracking is unavailable.
        /// </summary>
        ModeUnavailable,
    }

    /// <summary>
    /// A set of all types of map events that can occur that a developer
    /// may have to handle.
    /// </summary>
    [Flags]
    [Obsolete("Please use MLHeadTracking.MapEvents instead.", true)]
    public enum MLHeadTrackingMapEvent : uint
    {
        /// <summary>
        /// Map was lost. It could possibly recover.
        /// </summary>
        Lost = (1 << 0),

        /// <summary>
        /// Previous map was recovered.
        /// </summary>
        Recovered = (1 << 1),

        /// <summary>
        /// Failed to recover previous map.
        /// </summary>
        RecoveryFailed = (1 << 2),

        /// <summary>
        /// New map session created.
        /// </summary>
        NewSession = (1 << 3)
    }

    #if PLATFORM_LUMIN
    /// <summary>
    /// A structure containing information on the current state of the
    /// Head Tracking system.
    /// </summary>
    [Obsolete("Please use MLHeadTracking.State instead.", true)]
    public struct MLHeadTrackingState
    {
        /// <summary>
        /// What tracking mode the Head Tracking system is currently in.
        /// </summary>
        public MLHeadTrackingMode Mode;

        /// <summary>
        /// A confidence value (from 0..1) representing the confidence in the
        /// current pose estimation.
        /// </summary>
        public float Confidence;

        /// <summary>
        /// Represents what tracking error (if any) is present.
        /// </summary>
        public MLHeadTrackingError Error;
    }

    /// <summary>
    /// A class to provide an extension for the MLHeadTrackingMapEvent enum.
    /// Provides an alternative to binary operator use in order to find if a
    /// specific event has occured given a MLHeadTrackingMapEvent bitmask.
    /// </summary>
    [Obsolete("Please use MLHeadTracking.MapEvents instead.", true)]
    public static class MapEventsExtension
    {
        public static bool IsLost(this MLHeadTrackingMapEvent events)
        {
            return (int)(events & MLHeadTrackingMapEvent.Lost) != 0;
        }

        public static bool IsRecovered(this MLHeadTrackingMapEvent events)
        {
            return (int)(events & MLHeadTrackingMapEvent.Recovered) != 0;
        }

        public static bool IsRecoveryFailed(this MLHeadTrackingMapEvent events)
        {
            return (int)(events & MLHeadTrackingMapEvent.RecoveryFailed) != 0;
        }

        public static bool IsNewSession(this MLHeadTrackingMapEvent events)
        {
            return (int)(events & MLHeadTrackingMapEvent.NewSession) != 0;
        }
    }
#endif
}
