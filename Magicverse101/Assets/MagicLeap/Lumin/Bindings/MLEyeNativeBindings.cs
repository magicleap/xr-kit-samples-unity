// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLEyeNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

// Disable warnings about missing documentation for native interop.
#pragma warning disable 1591

namespace UnityEngine.XR.MagicLeap.Native
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// See ml_eye_tracking.h for additional comments
    /// </summary>
    public class MLEyeNativeBindings : MagicLeapNativeBindings
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="MLEyeNativeBindings"/> class from being created.
        /// </summary>
        private MLEyeNativeBindings()
        {
        }

        /// <summary>
        /// Returns the active status of the eye tracker.
        /// </summary>
        /// <returns>The active status of the eye tracker.</returns>
        [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_InputGetEyeTrackerActive")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetEyeTrackerActive();

        /// <summary>
        /// Sets the active status of the eye tracker.
        /// This will enable/disable the eye tracker subsystem.
        /// </summary>
        /// <param name="value">The active status of the eye tracker.</param>
        [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_InputSetEyeTrackerActive")]
        public static extern void SetEyeTrackerActive(bool value);
    }
}
#endif
