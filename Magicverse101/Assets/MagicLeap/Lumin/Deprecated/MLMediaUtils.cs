// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLMediaUtils.cs" company="Magic Leap, Inc">
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
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// Utility MediaPlayer struct to help get the result string from a media return code.
    /// </summary>
    [Obsolete("Please use MLResult.CodeToString(MLResult.Code) instead.")]
    public partial struct MLMediaUtils
    {
        /// <summary>
        /// Gets a readable version of the result code as an ASCII string.
        /// </summary>
        /// <param name="result">The MLResult.Code that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        [Obsolete("Please use MLResult.CodeToString(MLResult.Code) instead.")]
        public static string GetResultString(MLResultCode result)
        {
            // TODO: Support other media results.
            if (result >= MLResultCode.MediaGenericInvalidOperation &&
                result <= MLResultCode.MediaGenericUnexpectedNull)
            {
                return result.ToString();
            }

            return MagicLeapNativeBindings.MLGetResultString(result);
        }
    }
}

#endif
