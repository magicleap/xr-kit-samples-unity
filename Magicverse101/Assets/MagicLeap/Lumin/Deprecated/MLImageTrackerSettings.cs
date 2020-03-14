// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLImageTrackerSettings.cs" company="Magic Leap, Inc">
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
    /// Supported formats when adding Image Targets to the Image Tracker.
    /// </summary>
    [Obsolete("Please use MLImageTracker.ImageFormat instead.", false)]
    public enum MLImageTrackerImageFormat
    {
        /// <summary>
        /// Grayscale format.
        /// </summary>
        Grayscale,

        /// <summary>
        /// RGB format.
        /// </summary>
        RGB,

        /// <summary>
        /// RGBA format.
        /// </summary>
        RGBA
    }

    #if PLATFORM_LUMIN
    /// <summary>
    /// Represents the list of Image Tracker settings.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [Obsolete("Please use MLImageTracker.Settings instead.", true)]
    public struct MLImageTrackerSettings
    {
    }
    #endif
}
