// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLThumb.cs" company="Magic Leap, Inc">
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
    /// MLThumb contains joint information about the thumb instance.
    /// </summary>
    [Obsolete("Please use MLHandTracking.Thumb instead.")]
    public sealed class MLThumb
    {
        /// <summary>
        /// The Metacarpophalangeal joint of the thumb.
        /// </summary>
        public MLKeyPoint MCP { get; private set; }

        /// <summary>
        /// The Interphalangeal joint of the thumb.
        /// </summary>
        public MLKeyPoint IP { get; private set; }

        /// <summary>
        /// The Tip of the thumb.
        /// </summary>
        public MLKeyPoint Tip { get; private set; }

        /// <summary>
        /// All valid keypoints combined into a list.
        /// </summary>
        public List<MLKeyPoint> KeyPoints { get; private set; }

        // Used to skip empty bone indexes from Unity.
        private int _indexOffset = 2;

        /// <summary>
        /// Initializes a new instance of the MLThumb class.
        /// </summary>
        public MLThumb()
        {
            MCP = new MLKeyPoint(true);
            IP = new MLKeyPoint(true);
            Tip = new MLKeyPoint(true);
            // Assign all valid KeyPoints to an easy to access list.
            KeyPoints = new List<MLKeyPoint>();
            if (MCP.IsSupported)
            {
                KeyPoints.Add(MCP);
            }
            if (IP.IsSupported)
            {
                KeyPoints.Add(IP);
            }
            if (Tip.IsSupported)
            {
                KeyPoints.Add(Tip);
            }
        }

        /// <summary>
        /// Updates the state of the thumb.
        /// </summary>
        public void Update(List<UnityEngine.XR.Bone> bones)
        {
            // Make sure the correct number of bones are in the list.
            if (bones.Count != 5)
            {
                Debug.LogError("Error: MLThumb the number of bones returned was invalid.");
                return;
            }

            // Start at the offset to skip empty bones.
            for (int i = _indexOffset; i < bones.Count; i++)
            {
                if (KeyPoints.Count > i - _indexOffset)
                {
                    if (bones[i].TryGetPosition(out Vector3 bonePosition))
                    {
                        // Negate the offset for our local list.
                        KeyPoints[i - _indexOffset].Position = bonePosition;
                    }
                }
            }
        }
    }
}
#endif
