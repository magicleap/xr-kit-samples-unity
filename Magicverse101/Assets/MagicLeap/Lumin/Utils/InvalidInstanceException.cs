// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "InvalidInstanceException.cs" company="Magic Leap, Inc">
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
    /// Simple custom exception for instance error checking.
    /// </summary>
    public class InvalidInstanceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInstanceException"/> class.
        /// </summary>
        /// <param name="message">Message to provide in the exception</param>
        public InvalidInstanceException(string message)
            : base(message)
        {
        }
    }
}

#endif
