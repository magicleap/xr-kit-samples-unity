// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLImageTracker.cs" company="Magic Leap, Inc">
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
    /// MLImageTracker manages the Image Tracker system.
    /// Image Tracker enables experiences that recognize 2D planar images
    /// (image targets) in the physical world. It provides the position and
    /// orientation of the image targets in the physical world.
    /// </summary>
    public sealed partial class MLImageTracker
    {
        /// <summary>
        /// Adds an Image Target to the image tracker system.
        /// </summary>
        /// <param name="name">The unique name of this target.</param>
        /// <param name="image">
        /// Texture2D representing the Image Target.
        /// The aspect ration of the target should not be changed. Set the "Non Power of 2" property of Texture2D to none.
        /// </param>
        /// <param name="longerDimension">Size of the longer dimension in scene units.</param>
        /// <param name="callback">The function that will be triggered with target info.</param>
        /// <param name="isStationary">
        /// Set this to true if the position of this Image Target in the physical world is fixed and the local
        /// geometry is planar.
        /// </param>
        /// <returns>MLImageTracker.Target if the target was created and added successfully, null otherwise.</returns>
        [Obsolete("Please use MLImageTracker.AddTarget with MLImageTracker.Target.OnImageResultDelegate instead.", true)]
        public static MLImageTarget AddTarget(string name, Texture2D image, float longerDimension, Action<MLImageTarget, MLImageTargetResult> callback, bool isStationary = false)
        {
            return null;
        }
    }
}

#endif
