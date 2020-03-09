// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHeadTrackingMapEventsExtension.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// A class to provide an extension for the MLHeadTrackingMapEvent enum.
    /// Provides an alternative to binary operator use in order to find if a
    /// specific event has occurred given a MLHeadTrackingMapEvent bitmask.
    /// </summary>
    public static class MLHeadTrackingMapEventsExtension
    {
        /// <summary>
        /// Indicates if the lost event has been triggered.
        /// </summary>
        /// <param name="events">The bitmask of all map events that have occurred.</param>
        /// <returns>True if the MLHeadTracking.MapEvents.Lost flag is true.</returns>
        public static bool IsLost(this MLHeadTracking.MapEvents events)
        {
            return (int)(events & MLHeadTracking.MapEvents.Lost) != 0;
        }

        /// <summary>
        /// Indicates if the recovered event has been triggered.
        /// </summary>
        /// <param name="events">The bitmask of all map events that have occurred.</param>
        /// <returns>True if the MLHeadTracking.MapEvents.Recovered flag is true.</returns>
        public static bool IsRecovered(this MLHeadTracking.MapEvents events)
        {
            return (int)(events & MLHeadTracking.MapEvents.Recovered) != 0;
        }

        /// <summary>
        /// Indicates if the recovery failed event has been triggered.
        /// </summary>
        /// <param name="events">The bitmask of all map events that have occurred.</param>
        /// <returns>True if the MLHeadTracking.MapEvents.RecoveryFailed flag is true.</returns>
        public static bool IsRecoveryFailed(this MLHeadTracking.MapEvents events)
        {
            return (int)(events & MLHeadTracking.MapEvents.RecoveryFailed) != 0;
        }

        /// <summary>
        /// Indicates if the new session event has been triggered.
        /// </summary>
        /// <param name="events">The bitmask of all map events that have occurred.</param>
        /// <returns>True if the MLHeadTracking.MapEvents.NewSession flag is true.</returns>
        public static bool IsNewSession(this MLHeadTracking.MapEvents events)
        {
            return (int)(events & MLHeadTracking.MapEvents.NewSession) != 0;
        }
    }
}
