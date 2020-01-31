// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is meant to work as a collider component for the MLMovement
/// API. Contains information like the type of collision for this object
/// as well as related variables for these collisions.
/// </summary>
[AddComponentMenu("XR/MagicLeap/Movement/MLMovementCollider")]
[RequireComponent(typeof(Collider)), RequireComponent(typeof(Rigidbody))]
public class MLMovementCollider : MonoBehaviour
{
    #region Public Enums
    /// <summary>
    /// Type of collider for collisions.
    /// </summary>
    public enum MovementColliderType
    {
        Hard,
        Soft
    }
    #endregion

    #region Private Variables
    private int maxDepth = 50;
    #endregion

    #region Public Variables
    /// <summary>
    /// Type of movement collider.
    /// </summary>
    public MovementColliderType ColliderType = MovementColliderType.Hard;
    #endregion

    #region Public Properties
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
    #endregion

    #region Unity Methods
    /// <summary>
    /// Ensures proper RigidBody and Collider values based of the Collider type.
    /// </summary>
    void OnValidate()
    {
        Collider collider = this.GetComponent<Collider>();

        if (ColliderType == MovementColliderType.Soft && collider.isTrigger == false)
        {
            Debug.LogWarning("Warning: MLMovementCollider's object Collider.isTrigger must be enabled for soft collisions. Enabling.");
            collider.isTrigger = true;
        }
        else if (ColliderType == MovementColliderType.Hard && collider.isTrigger == true)
        {
            Debug.LogWarning("Warning: MLMovementCollider's object Collider.isTrigger must be disabled for hard collisions. Disabling.");
            collider.isTrigger = false;
        }
    }
    #endregion
}
