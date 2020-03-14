// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHands.cs" company="Magic Leap, Inc">
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
    /// MLHands is the entry point for all the hand tracking data
    /// including gestures, hand centers and key points for both hands.
    /// </summary>
    [Obsolete("Please use the MLHandTracking class instead.")]
    public sealed class MLHands : MLHandTracking
    {
    }
}
#endif
