// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLImageTarget.cs" company="Magic Leap, Inc">
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
    using UnityEngine;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Manages the image target settings.
    /// </summary>
    [Obsolete("Please use MLImageTracker.Target instead.", true)]
    public sealed class MLImageTarget
    {
    }
}
