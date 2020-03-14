// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHandTrackingFinger.cs" company="Magic Leap, Inc">
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
        /// Finger contains joint information about the finger instance.
        /// </summary>
        public sealed partial class Finger
        {
            /// <summary>
            /// Initializes a new instance of the Finger class.
            /// </summary>
            /// <param name="bones">The number of bone key points to track. Essentially the fidelity of the finger.</param>
            public Finger(uint bones = 3)
            {
                this.MCP = new KeyPoint(bones >= 2);
                this.PIP = new KeyPoint(bones >= 3); // Only valid for: Index/Middle Finger
                this.Tip = new KeyPoint(bones >= 1);

                // Assign all valid KeyPoints to an easy to access list.
                this.KeyPoints = new List<KeyPoint>();

                if (this.MCP.IsSupported)
                {
                    this.KeyPoints.Add(this.MCP);
                }

                if (this.PIP.IsSupported)
                {
                    this.KeyPoints.Add(this.PIP);
                }

                if (this.Tip.IsSupported)
                {
                    this.KeyPoints.Add(this.Tip);
                }
            }

            /// <summary>
            /// Gets the Metacarpal phalangeal joint of the finger.
            /// </summary>
            public KeyPoint MCP { get; private set; }

            /// <summary>
            /// Gets the <c>proximal-interphalangeal</c> joint of the finger.
            /// </summary>
            public KeyPoint PIP { get; private set; }

            /// <summary>
            /// Gets the tip of the finger.
            /// </summary>
            public KeyPoint Tip { get; private set; }

            /// <summary>
            /// Gets all valid key points combined into a list.
            /// </summary>
            public List<KeyPoint> KeyPoints { get; private set; }

            /// <summary>
            /// Updates the state of the finger.
            /// </summary>
            /// <param name="bones">The list of bones to update with their corresponding key points.</param>
            public void Update(List<UnityEngine.XR.Bone> bones)
            {
                // Make sure the correct number of bones are in the list.
                if (bones.Count != 5)
                {
                    Debug.LogError("Error: Finger the number of bones returned was invalid.");
                    return;
                }

                // Index [2] - MCP
                // Index [3] - PIP
                // Index [4] - TIP
                if (this.KeyPoints.Count == 3)
                {
                    // PIP - Only exist for fingers with 3 keypoints.
                    this.PIP.Position = this.GetBonePosition(bones[3]);
                }

                if (this.KeyPoints.Count >= 2)
                {
                    // MCP,Tip - exist for all fingers.
                    this.MCP.Position = this.GetBonePosition(bones[2]);
                    this.Tip.Position = this.GetBonePosition(bones[4]);
                }
            }

            /// <summary>
            /// Returns the position of the bone, from the Unity XR Input.
            /// </summary>
            /// <param name="bone">The XR.Bone to be examined.</param>
            /// <returns>The position of the bone provided.</returns>
            private Vector3 GetBonePosition(UnityEngine.XR.Bone bone)
            {
                bone.TryGetPosition(out Vector3 bonePosition);

                return bonePosition;
            }
        }
    }
}

#endif
