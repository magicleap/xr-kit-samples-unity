// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLTokenAgent.cs" company="Magic Leap, Inc">
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
    /// A class for retrieving the client credentials.
    /// </summary>
    public sealed partial class MLTokenAgent
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
