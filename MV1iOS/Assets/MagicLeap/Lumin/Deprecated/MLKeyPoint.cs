// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLKeyPoint.cs" company="Magic Leap, Inc">
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
    /// MLThumb contains joint information about the thumb instance.
    /// </summary>
    [Obsolete("Please use MLHandTracking.KeyPoint instead.")]
    public sealed class MLKeyPoint
    {
        /// <summary>
        /// Initialize the KeyPoint.
        /// </summary>
        /// <param name="supported">The valid state of the KeyPoint.</param>
        public MLKeyPoint(bool supported)
        {
#pragma warning disable 618
            IsValid = supported;
#pragma warning restore 618
            IsSupported = supported;
        }

        /// <summary>
        /// Returns if the keypoint is supported by the hand tracking framework. This is static data and does not update every frame.
        /// </summary>
        [Obsolete("Please use MLHandTracking.KeyPoint.IsSupported instead.", true)]
        public bool IsValid { get; set; }

        /// <summary>
        /// Returns if the keypoint is supported by the hand tracking framework. This is static data and does not update every frame.
        /// </summary>
        public bool IsSupported { get; set; }

        /// <summary>
        /// Returns the position of the KeyPoint.
        /// </summary>
        public Vector3 Position { get; set; }
    }
}
#endif
