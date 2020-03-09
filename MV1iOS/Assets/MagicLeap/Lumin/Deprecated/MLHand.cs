// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHand.cs" company="Magic Leap, Inc">
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
    /// MLHand contains the hand tracking data including gestures,
    /// hand centers and key points for a specific hand.
    /// </summary>
    [Obsolete("Please use MLHandTracking.Hand instead.")]
    public sealed class MLHand : MLHandTracking.Hand
    {
        /// <summary>
        /// The confidence value of the requested KeyPose, between [0, 1].
        /// </summary>
        [Obsolete("Please use MLHandTracking.Hand.HandKeyPoseConfidence instead.", true)]
        public float KeyPoseConfidence
        {
            get
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// The filtered confidence value of the requested KeyPose, between [0, 1].
        /// </summary>
        [Obsolete("Please use MLHandTracking.Hand.HandKeyPoseConfidenceFiltered instead.", true)]
        public float KeyPoseConfidenceFiltered
        {
            get
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// Event is raised whenever a KeyPose starts being recognized for this hand.
        /// </summary>
        [Obsolete("Please use MLHandTracking.Hand.OnHandKeyPoseBegin instead.", true)]
        public event OnKeyPoseBeginDelegate OnKeyPoseBegin = delegate { };

        /// <summary>
        /// Event is raised whenever a KeyPose stops being recognized for this hand.
        /// </summary>
        [Obsolete("Please use MLHandTracking.Hand.OnHandKeyPoseEnd instead.", true)]
        public event OnKeyPoseEndDelegate OnKeyPoseEnd = delegate { };

        /// <summary>
        /// Gets the Hand Type (left or right)
        /// </summary>
        [Obsolete("Please use MLHandTracking.Hand.Type instead.", true)]
        public MLHandType HandType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MLHand" class.
        /// </summary>
        /// <param name="handType">Hand type.</param>
        public MLHand(MLHandTracking.HandType handType) : base(handType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MLHand" class.
        /// </summary>
        /// <param name="handType">Hand type.</param>
        [Obsolete("Please use MLHandTracking.Hand.Hand(MLHandTracking.HandType) instead.", true)]
        public MLHand(MLHandType handType) : base (MLHandTracking.HandType.Left)
        {
        }

        [Obsolete("Please use MLHandTracking.Hand.BeginKeyPose(MLHandTracking.HandKeyPose) instead.", true)]
        public void BeginKeyPose(MLHandKeyPose pose)
        {
        }

        [Obsolete("Please use MLHandTracking.Hand.EndKeyPose(MLHandTracking.HandKeyPose) instead.", true)]
        public void EndKeyPose(MLHandKeyPose pose)
        {
        }
    }
}

#endif
