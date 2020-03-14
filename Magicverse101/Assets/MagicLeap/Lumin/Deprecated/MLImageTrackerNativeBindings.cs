// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLImageTrackerNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap.Native
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine.XR.MagicLeap;

    /// <summary>
    /// The native bindings to the Image Tracking API.
    /// See ml_image_tracking.h for additional comments
    /// </summary>
    [Obsolete("Please use MLImageTracker.NativeBindings instead.", true)]
    public partial class MLImageTrackerNativeBindings : MagicLeapNativeBindings
    {
    }
}

#endif
