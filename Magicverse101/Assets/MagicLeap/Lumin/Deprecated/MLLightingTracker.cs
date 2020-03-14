// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLLightingTracker.cs" company = "Magic Leap">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;
using System;
using System.Collections;

#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap.Native;
#endif

namespace UnityEngine.XR.MagicLeap
{
    /// <summary/>
    [Obsolete("Please use MLLightingTracking.Camera instead.")]
    public enum MLLightingTrackingCamera
    {
        /// <summary/>
        Left,
        /// <summary/>
        Right,
        /// <summary/>
        FarLeft,
        /// <summary/>
        FarRight
    }

    #if PLATFORM_LUMIN
    /// <summary>
    /// Provides environment lighting information.
    /// Capturing images or video will stop the lighting information update.
    /// </summary>
    [Obsolete("Please use MLLightingTracking instead.")]
    public sealed class MLLightingTracker : MLLightingTracking
    {
    }
    #endif
}
