// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHandTrackingKeyPoint.cs" company="Magic Leap, Inc">
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
    /// <summary>
    /// MLHandTracking is the entry point for all the hand tracking data
    /// including gestures, hand centers and key points for both hands.
    /// </summary>
    public partial class MLHandTracking
    {
        /// <summary>
        /// This class contains the valid state and current position of the KeyPoint.
        /// </summary>
        public sealed partial class KeyPoint
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="KeyPoint" /> class.
            /// </summary>
            /// <param name="supported">The valid state of the KeyPoint.</param>
            public KeyPoint(bool supported)
            {
                this.IsSupported = supported;
            }

            /// <summary>
            /// Gets a value indicating whether the key point is supported by the hand tracking framework.
            /// This value won't change after initialization and does not update every frame.
            /// </summary>
            public bool IsSupported { get; private set; }

            /// <summary>
            /// Gets or sets a value indicating whether the position of the KeyPoint.
            /// </summary>
            public Vector3 Position { get; set; }
        }
    }
}

#endif
