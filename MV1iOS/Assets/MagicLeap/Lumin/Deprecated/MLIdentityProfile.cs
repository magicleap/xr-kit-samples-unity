
// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLLocation.cs" company="Magic Leap, Inc">
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

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Public represenation of MLIdentityProfile.
    /// MLIdentityProfile represents a set of attribute of a user's profile.
    /// </summary>
    [Obsolete("Please use MLIdentity.Profile instead.", true)]
    public class MLIdentityProfile : MonoBehaviour
    {
    }
}
