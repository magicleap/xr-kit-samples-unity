// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://auth.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeap.Core
{
    /// <summary>
    /// This class is meant to work as a collider component for the MLMovement
    /// API. Contains information like the type of collision for this object
    /// as well as related variables for these collisions.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/Movement/MLMovementColliderBehavior")]
    [RequireComponent(typeof(Collider)), RequireComponent(typeof(Rigidbody))]
    public class MLMovementColliderBehavior : MonoBehaviour
    {
        /// <summary>
        /// Type of collider for collisions.
        /// </summary>
        public enum MovementColliderType
        {
            Hard,
            Soft
        }

        private int maxDepth = 50;

        /// <summary>
        /// Type of movement collider.
        /// </summary>
        public MovementColliderType ColliderType = MovementColliderType.Hard;

        /// <summary>
        /// Maximum depth percentage into the object the collider object will be able to penetrate
        /// in case of this object being a Soft MovementColliderType.
        /// </summary>
        public int MaxDepth
        {
            get
            {
                return maxDepth;
            }
            set
            {
                maxDepth = Mathf.Clamp(value, 0, 100);
            }
        }

        /// <summary>
        /// Ensures proper RigidBody and Collider values based on the Collider type.
        /// </summary>
        void OnValidate()
        {
            Collider collider = this.GetComponent<Collider>();

            if (ColliderType == MovementColliderType.Soft && collider.isTrigger == false)
            {
                Debug.LogWarning("Warning: MLMovementColliderBehavior's object Collider.isTrigger must be enabled for soft collisions. Enabling.");
                collider.isTrigger = true;
            }
            else if (ColliderType == MovementColliderType.Hard && collider.isTrigger == true)
            {
                Debug.LogWarning("Warning: MLMovementColliderBehavior's object Collider.isTrigger must be disabled for hard collisions. Disabling.");
                collider.isTrigger = false;
            }
        }
    }
}
