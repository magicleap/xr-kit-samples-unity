// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHandTrackingThumb.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;

    /// <summary>
    /// MLHandTracking is the entry point for all the hand tracking data
    /// including gestures, hand centers and key points for both hands.
    /// </summary>
    public partial class MLHandTracking
    {
        /// <summary>
        /// Thumb contains joint information about the thumb instance.
        /// </summary>
        public sealed partial class Thumb
        {
            /// <summary>
            /// Used to skip empty bone indexes from Unity.
            /// </summary>
            private int indexOffset = 2;

            /// <summary>
            /// Initializes a new instance of the <see cref="Thumb" /> class.
            /// </summary>
            public Thumb()
            {
                this.MCP = new KeyPoint(true);
                this.IP = new KeyPoint(true);
                this.Tip = new KeyPoint(true);

                // Assign all valid KeyPoints to an easy to access list.
                this.KeyPoints = new List<KeyPoint>();
                if (this.MCP.IsSupported)
                {
                    this.KeyPoints.Add(this.MCP);
                }

                if (this.IP.IsSupported)
                {
                    this.KeyPoints.Add(this.IP);
                }

                if (this.Tip.IsSupported)
                {
                    this.KeyPoints.Add(this.Tip);
                }
            }

            /// <summary>
            /// Gets the <c>metacarpophalangeal</c> joint of the thumb.
            /// </summary>
            public KeyPoint MCP { get; private set; }

            /// <summary>
            /// Gets the <c>interphalangeal</c> joint of the thumb.
            /// </summary>
            public KeyPoint IP { get; private set; }

            /// <summary>
            /// Gets the tip of the thumb.
            /// </summary>
            public KeyPoint Tip { get; private set; }

            /// <summary>
            /// Gets all the valid key points combined into a list.
            /// </summary>
            public List<KeyPoint> KeyPoints { get; private set; }

            /// <summary>
            /// Updates the state of the thumb.
            /// </summary>
            /// <param name="bones">The list of bones to update.</param>
            public void Update(List<UnityEngine.XR.Bone> bones)
            {
                // Make sure the correct number of bones are in the list.
                if (bones.Count != 5)
                {
                    Debug.LogError("Error: Thumb the number of bones returned was invalid.");
                    return;
                }

                // Start at the offset to skip empty bones.
                for (int i = this.indexOffset; i < bones.Count; i++)
                {
                    if (this.KeyPoints.Count > i - this.indexOffset)
                    {
                        if (bones[i].TryGetPosition(out Vector3 bonePosition))
                        {
                            // Negate the offset for our local list.
                            this.KeyPoints[i - this.indexOffset].Position = bonePosition;
                        }
                    }
                }
            }
        }
    }
}

#endif
