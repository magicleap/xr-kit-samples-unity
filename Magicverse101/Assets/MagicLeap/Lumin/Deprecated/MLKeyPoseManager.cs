// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLKeyPoseManager.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// Manages what key poses are enabled and exposes the events.
    /// </summary>
    [Obsolete("Please use MLHandTracking.KeyposeManager instead.")]
    public sealed class MLKeyPoseManager : MLHandTracking.KeyposeManager
    {
        /// <summary>
        /// Gets the currently enabled hand key poses.
        /// </summary>
        [Obsolete("Please use MLKeyPoseManager.EnabledKeyPoses instead.", true)]
        public List<MLHandKeyPose> EnabledHandKeyPoses
        {
            get
            {
                return new List<MLHandKeyPose>();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MLKeyPoseManager" /> class.
        /// </summary>
        /// <param name="leftHand">Left hand to which KeyPoseManager will subscribe for events.</param>
        /// <param name="rightHand">Right hand to which KeyPoseManager will subscribe for events.</param>
        public MLKeyPoseManager(MLHandTracking.Hand leftHand, MLHandTracking.Hand rightHand) : base(leftHand, rightHand)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MLKeyPoseManager" /> class.
        /// </summary>
        /// <param name="leftHand">Left hand to which KeyPoseManager will subscribe for events.</param>
        /// <param name="rightHand">Right hand to which KeyPoseManager will subscribe for events.</param>
        [Obsolete("Please use MLKeyPoseManager(MLHandTracking.Hand, MLHandTracking.Hand) instead.", true)]
        public MLKeyPoseManager(MLHand leftHand, MLHand rightHand) : base(null, null)
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="KeyposeManager" /> class.
        /// </summary>
        ~MLKeyPoseManager()
        {
        }

        /// <summary>
        /// Enables or disables an array of KeyPoses.
        /// </summary>
        /// <param name="keyPoses"/>
        /// <param name="enable">Enable or disable KeyPoses.</param>
        /// <param name="exclusive">
        /// When enabling and this is true, only the list of provided KeyPoses
        /// are enabled, all other previously-enabled KeyPoses get disabled. No effect if
        /// parameter enable is false.
        /// </param>
        /// <remarks>
        /// Enabling too many KeyPoses will currently lead to decreased sensitivity to each
        /// individual KeyPose.
        /// </remarks>
        [Obsolete("Please use MLKeyPoseManager.EnableKeyPoses(MLHandTracking.HandKeyPose[], bool, bool) instead.", true)]
        public bool EnableKeyPoses(MLHandKeyPose[] keyPoses, bool enable, bool exclusive = false)
        {
            return false;
        }

        /// <summary>
        /// Sets the keypoints filter level.
        /// </summary>
        /// <param name="filterLevel">The desired filter level.</param>
        /// <returns>true if the filter level was successfully applied and false otherwise.</returns>
        [Obsolete("Please use MLKeyPoseManager.SetKeyPointsFilterLevel(MLHandTracking.KeyPointFilterLevel) instead.", true)]
        public bool SetKeyPointsFilterLevel(MLKeyPointFilterLevel filterLevel)
        {
            return false;
        }

        /// <summary>
        /// Sets the pose filter level.
        /// </summary>
        /// <param name="filterLevel">The desired filter level.</param>
        /// <returns>true if the filter level was successfully applied and false otherwise.</returns>
        [Obsolete("Please use MLKeyPoseManager.SetPoseFilterLevel(MLHandTracking.KeyPointFilterLevel) instead.", true)]
        public bool SetPoseFilterLevel(MLPoseFilterLevel filterLevel)
        {
            return false;
        }
    }
}
#endif
