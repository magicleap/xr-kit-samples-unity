// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLMediaErrorNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;

    /// <summary>
    /// Media player script that allows playback of a streaming video (either from file or web URL)
    /// This script will update the main texture parameter of the Renderer attached as a sibling
    /// with the video frame from playback. Audio is also handled through this class and will
    /// playback audio from the file.
    /// </summary>
    public sealed partial class MLMediaPlayer
    {
        /// <summary>
        /// See ml_media_error.h for additional comments.
        /// </summary>
        private partial class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// MLMediaError library name
            /// </summary>
            private const string MLMediaErrorDLL = "ml_mediaerror";

            /// <summary>
            /// Gets a readable version of the result code as an ASCII string.
            /// </summary>
            /// <param name="result">The MLResult.Code that should be converted.</param>
            /// <returns>ASCII string containing a readable version of the result code.</returns>
            [DllImport(MLMediaErrorDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MLMediaResultGetString(MLResult.Code result);
        }
    }
}

#endif
