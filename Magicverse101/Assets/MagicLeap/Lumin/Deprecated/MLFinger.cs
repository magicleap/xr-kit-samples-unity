// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLFinger.cs" company="Magic Leap, Inc">
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
    [Obsolete("Please use MLHandTracking.Finger instead.")]
    public sealed class MLFinger
    {
        /// <summary>
        /// The Metacarpal phalangeal joint of the finger.
        /// </summary>
        public MLKeyPoint MCP { get; private set; }

        /// <summary>
        /// The Proximal-interphalangeal joint of the finger.
        /// </summary>
        public MLKeyPoint PIP { get; private set; }

        /// <summary>
        /// The Tip of the finger.
        /// </summary>
        public MLKeyPoint Tip { get; private set; }

        /// <summary>
        /// All valid keypoints combined into a list.
        /// </summary>
        public List<MLKeyPoint> KeyPoints { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MLFinger class.
        /// </summary>
        public MLFinger(uint bones = 3)
        {
            MCP = new MLKeyPoint(bones >= 2);
            PIP = new MLKeyPoint(bones >= 3); // Only valid for: Index/Middle Finger
            Tip = new MLKeyPoint(bones >= 1);
            // Assign all valid KeyPoints to an easy to access list.
            KeyPoints = new List<MLKeyPoint>();
            if (MCP.IsSupported)
            {
                KeyPoints.Add(MCP);
            }
            if (PIP.IsSupported)
            {
                KeyPoints.Add(PIP);
            }
            if (Tip.IsSupported)
            {
                KeyPoints.Add(Tip);
            }
        }

        /// <summary>
        /// Updates the state of the finger.
        /// </summary>
        public void Update(List<UnityEngine.XR.Bone> bones)
        {
            // Make sure the correct number of bones are in the list.
            if (bones.Count != 5)
            {
                Debug.LogError("Error: MLFinger the number of bones returned was invalid.");
                return;
            }
            // Index [2] - MCP
            // Index [3] - PIP
            // Index [4] - TIP
            if (KeyPoints.Count == 3)
            {
                // PIP - Only exist for fingers with 3 keypoints.
                PIP.Position = GetBonePosition(bones[3]);
            }
            if (KeyPoints.Count >= 2)
            {
                // MCP,Tip - exist for all fingers.
                MCP.Position = GetBonePosition(bones[2]);
                Tip.Position = GetBonePosition(bones[4]);
            }
        }

        /// <summary>
        /// Returns the position of the bone, from the Unity XR Input.
        /// </summary>
        /// <param name="bones">The XR.Bone to be examined.</param>
        /// <returns></returns>
        private Vector3 GetBonePosition(UnityEngine.XR.Bone bone)
        {
            bone.TryGetPosition(out Vector3 bonePosition);
            return bonePosition;
        }
    }
}
#endif
