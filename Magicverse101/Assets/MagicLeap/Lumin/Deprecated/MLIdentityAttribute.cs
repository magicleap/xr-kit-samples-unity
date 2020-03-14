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
    using System.Runtime.InteropServices;

    #if PLATFORM_LUMIN
    /// <summary>
    /// MLIdentityAttribute represents an attribute of a user's profile
    /// (for instance: name, address, email). Each attribute has a name (represented by key and value).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    [Obsolete("Please use MLIdentity.Profile.Attribute instead.", true)]
    public struct MLIdentityAttribute
    {
    }

    /// <summary>
    /// Internal raw representation of C API's MLIdentityProfile.
    /// MLIdentityProfile represents a set of attribute of a user's profile.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [Obsolete("Please use MLIdentity.NativeBindings.ProfileNative instead.", true)]
    internal struct InternalMLIdentityProfile
    {
    }


    /// <summary>
    /// MLInvokeFuture represents a type which is opaque (incomplete) to users of this library.
    /// A pointer to an MLInvokeFuture is returned by the Async function.
    /// Users pass it to the Wait function to determine if the asynchronous method has
    /// completed and to retrieve the result if it has.
    /// </summary>
    [Obsolete("This class is now represented as an IntPtr and is no longer used.", true)]
    public class MLInvokeFuture
    {
    }
    #endif

    /// <summary>
    /// MLIdentityAttributeKey identifies an attribute in a user profile.
    /// Attributes that were not known at the time the library was built, are marked as Unknown.
    /// </summary>
    [Obsolete("Please use MLIdentity.Profile.Attribute.Keys instead.", true)]
    public enum MLIdentityAttributeKey
    {
    }
}
