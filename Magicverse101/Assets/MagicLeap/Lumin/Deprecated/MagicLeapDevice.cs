// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MagicLeapDevice.cs" company="Magic Leap, Inc">
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
    /// MagicLeap device class responsible for updating all trackers when they register and are enabled.
    /// </summary>
    [Obsolete("Please use MLDevice instead.", true)]
    public class MagicLeapDevice : MLDevice
    {
        [Obsolete("Please use MLHeadTracking.OnHeadTrackingModeChangedDelegate instead.", true)]
        public delegate void OnHeadTrackingModeChangedDelegate(MLHeadTrackingState state);

        [Obsolete("Please use MLHeadTracking.OnHeadTrackingMapEventDelegate instead.", true)]
        public delegate void OnHeadTrackingMapEventDelegate(MLHeadTrackingMapEvent mapEvent);

        [Obsolete("Please use MLHeadTracking.TrackingState instead.", true)]
        public static MLHeadTrackingState HeadTrackingState
        {
            get
            {
                return new MLHeadTrackingState();
            }
        }

        /// <summary>
        /// Registers a callback to be called when only rotation head pose is available,
        /// or when full head pose is available again.
        /// </summary>
        [Obsolete("Please use MLHeadTracking.RegisterHeadTrackingModeChanged instead.", true)]
        public static void RegisterHeadTrackingModeChanged(OnHeadTrackingModeChangedDelegate callback)
        {
        }

        /// <summary>
        /// Unregisters a previously registered head tracking mode change callback.
        /// </summary>
        [Obsolete("Please use MLHeadTracking.UnregisterHeadTrackingModeChanged instead.", true)]
        public static void UnregisterHeadTrackingModeChanged(OnHeadTrackingModeChangedDelegate callback)
        {
        }

        /// <summary>
        /// Registers a callback to be called when map events occur.
        /// This functionality is only supported on platform api level 2 and onwards.
        /// </summary>
        [Obsolete("Please use MLHeadTracking.RegisterOnHeadTrackingMapEvent instead.", true)]
        public static void RegisterOnHeadTrackingMapEvent(OnHeadTrackingMapEventDelegate callback)
        {
        }

        /// <summary>
        /// Unregisters a previously registered map event callback.
        /// This functionality is only supported on platform api level 2 and onwards.
        /// </summary>
        [Obsolete("Please use MLHeadTracking.UnregisterOnHeadTrackingMapEvent instead.", true)]
        public static void UnregisterOnHeadTrackingMapEvent(OnHeadTrackingMapEventDelegate callback)
        {
        }
        /// <summary>
        /// MagicLeap platform Unity name.
        /// </summary>
        [Obsolete("Please use MLDevice.MagicLeapDeviceName instead.", true)]
        public const string MAGIC_LEAP_DEVICE_NAME = "Lumin";

        /// <summary>
        /// Unregister a previously registered MagicLeap API Update callback.
        /// </summary>
        /// <param name="updateCallback"></param>
        [Obsolete("Please use MLDevice.Unregister(MLDevice.OnUpdateActionsDelegate callback) instead.", true)]
        public static void Unregister(Action<float> updateCallback)
        {
        }

        /// <summary>
        /// Register a MagicLeap API Update callback to be called on Update of this MonoBehaviour.
        /// </summary
        [Obsolete("Please use MLDevice.Register(MLDevice.OnUpdateActionsDelegate callback) instead.", true)]
        public static void Register(Action<float> callback)
        {
        }
    }
}

#endif
