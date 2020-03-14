// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLDispatch.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using UnityEngine;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// This interface is to let an application query the platform to handle things
    /// that the app itself cannot or wants someone else to handle.
    /// For example, if an application comes across a schema tag that it doesn't know
    /// what to do with, it can query the platform to see if some other application might.
    /// This can be useful for handling http://, https:// or other HTML Entity Codes.
    /// Apart from handling schema tags in URIs, this interface can also be used
    /// to query the platform to handle a type of file based on file-extension or mime-type
    /// </summary>
    public sealed partial class MLDispatch
    {
        /// <summary>
        /// Gets a readable version of the result code as an ASCII string.
        /// </summary>
        /// <param name="result">The MLResult that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        [Obsolete("Please use MLResult.CodeToString(MLResult.Code) instead.", true)]
        public static string GetResultString(MLResultCode result)
        {
            return "This function is deprecated. Use MLResult.CodeToString(MLResult.Code) instead.";
        }
    }
}

#endif
