// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMovementSettings.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// Settings for the movement session.
    /// </summary>
    [Serializable]
    [Obsolete("Please use MLMovement.Settings instead.")]
    public struct MLMovementSettings
    {
        /// <summary>
        /// Number of frames of sway history to track.
        /// Increase to improve smoothing.
        /// Minimum value of 3.
        /// MLMovement.GetDefaultSettings will set this value to 30.
        /// </summary>
        public uint SwayHistorySize;

        /// <summary>
        /// Maximum angle, in radians, between the oldest and newest head pose to object vector.
        /// Increasing this will increase the maximum speed of movement.
        /// Must be greater than zero.
        /// MLMovement.GetDefaultSettings will set this value to 0.5.
        /// </summary>
        public float MaxDeltaAngle;

        /// <summary>
        /// A unit-less number that governs the smoothing of Control input.
        /// Larger values will make the movement more twitchy, smaller values will make it smoother by
        /// increasing latency between Control input and object movement response by averaging multiple
        /// frames of input values.
        /// Must be greater than zero. Typical values would be between 0.5 and 10.
        /// MLMovement.GetDefaultSettings will set this value to 3.5.
        /// </summary>
        public float ControlDampeningFactor;

        /// <summary>
        /// The maximum angle, in radians, that the object will be tilted left/right and front/back.
        /// Cannot be a negative value, but may be zero.
        /// MLMovement.GetDefaultSettings will set this value to M_PI / 6.0.
        /// </summary>
        public float MaxSwayAngle;

        /// <summary>
        /// The speed of rotation that will stop implicit depth translation from happening.
        /// The speed of rotation about the head pose Y-axis, in radians per second, that if exceeded,
        /// stops implicit depth translation from happening.
        /// Must be greater than zero.
        /// MLMovement.GetDefaultSettings will set this value to M_PI * 5.0 / 3.0.
        /// </summary>
        public float MaximumHeadposeRotationSpeed;

        /// <summary>
        /// The maximum speed that head pose can move, in meters per second, that will stop implicit depth translation.
        /// If the head pose is moving faster than this speed (meters per second) implicit depth translation doesn't happen.
        /// Must be greater than zero.
        /// MLMovement.GetDefaultSettings will set this value to 0.75.
        /// </summary>
        public float MaximumHeadposeMovementSpeed;

        /// <summary>
        /// Distance in meters the object must move in depth since the last frame to cause maximum push/pull sway.
        /// Must be greater than zero.
        /// MLMovement.GetDefaultSettings will set this value to 0.1.
        /// </summary>
        public float MaximumDepthDeltaForSway;

        /// <summary>
        /// The minimum distance in meters the object can be moved in depth relative to the head pose.
        /// Must be greater than zero and less than MaximumDistance.
        /// MLMovement.GetDefaultSettings will set this value to 0.5.
        /// </summary>
        public float MinimumDistance;

        /// <summary>
        /// The maximum distance in meters the object can be moved in depth relative to the head pose.
        /// Must be greater than zero and MinimumDistance.
        /// MLMovement.GetDefaultSettings will set this value to 15.0.
        /// </summary>
        public float MaximumDistance;

        /// <summary>
        /// Maximum length of time, in seconds, that lateral sway should take to decay.
        /// Maximum length of time (in seconds) lateral sway should take to decay back to an upright
        /// orientation once lateral movement stops.
        /// Must be greater than zero.
        /// MLMovement.GetDefaultSettings will set this value to 0.4.
        /// </summary>
        public float MaximumSwayTimeSeconds;

        /// <summary>
        /// Maximum length of time, in seconds, to allow MLMovementEnd to resolve before forcefully aborting.
        /// This serves as a fail-safe for instances where the object is in a bad position and can't resolve to a safe
        /// position.
        /// Must be greater than or equal to zero.
        /// MLMovement.GetDefaultSettings will set this value to 10.0.
        /// </summary>
        public float EndResolveTimeoutSeconds;
    }

    /// <summary>
    /// Parameters that define the input controls for 3 degrees of freedom movement.
    /// Under 3 degrees of freedom movement mode, only the orientation of the user's pointing device
    /// is taken into account for moving the target object and the pointing device's position is not used.
    /// Head pose position is also required by the algorithm, and is used as the center point around which
    /// the target object moves.
    /// When the movement session starts, the initial orientation of the control forms a baseline, from
    /// which further changes in the orientation will cause the object to move around the user.
    /// </summary>
    [Obsolete("Please use MLMovement.Controls3Dof instead.")]
    public struct MLMovement3DofControls
    {
        /// <summary>
        /// The world space position of the device's head pose.
        /// </summary>
        public Vector3 HeadposePosition;

        /// <summary>
        /// The world space orientation of the user's pointing device.
        /// </summary>
        public Quaternion ControlRotation;
    }

    /// <summary>
    /// Parameters that define the input controls for 6 degrees of freedom movement.
    /// Under 6 degrees of freedom movement mode, both the position and orientation of the user's pointing
    /// device is taken into account when moving the target object. Head pose position and orientation is
    /// also required by the algorithm.
    /// When the movement session starts, the initial orientation and position of the pointing device
    /// form a baseline, from which further changes in orientation or position will cause the object to
    /// move relative to the pointing device.
    /// </summary>
    [Obsolete("Please use MLMovement.Controls6Dof instead.")]
    public struct MLMovement6DofControls
    {
        /// <summary>
        /// The world space position of the device's head pose.
        /// </summary>
        public Vector3 HeadposePosition;

        /// <summary>
        /// The world space orientation of the device's head pose.
        /// </summary>
        public Quaternion HeadposeRotation;

        /// <summary>
        /// The world space position of the user's pointing device.
        /// </summary>
        public Vector3 ControlPosition;

        /// <summary>
        /// The world space orientation of the user's pointing device.
        /// </summary>
        public Quaternion ControlRotation;
    }

    /// <summary>
    /// The relevant state of the object being moved.
    /// </summary>
    [Obsolete("Please use MLMovement.MovementObject instead.")]
    public struct MLMovementObject
    {
        /// <summary>
        /// The world space translation of the object to move.
        /// </summary>
        public Vector3 ObjectPosition;

        /// <summary>
        /// The world space orientation of the object to move.
        /// </summary>
        public Quaternion ObjectRotation;
    }

    /// <summary>
    /// 3 degrees of freedom specific movement settings that must be provided when starting a 3 degrees
    /// of freedom movement session.
    /// A 3 degrees of freedom movement session relies on the orientation of the user's pointing device,
    /// but ignores the position, when moving the target object.
    /// </summary>
    [Obsolete("Please use MLMovement.Settings3Dof instead.")]
    public struct MLMovement3DofSettings
    {
        /// <summary>
        /// If the object should automatically center on the control direction when beginning movement.
        /// </summary>
        public bool AutoCenter;
    }

    /// <summary>
    /// 6 degrees of freedom specific movement settings that must be provided when starting a 6 degrees
    /// of freedom movement session.
    /// A 6 degrees of freedom movement session relies on both the position and orientation of the user's
    /// pointing device when moving the target object.
    /// </summary>
    [Obsolete("Please use MLMovement.Settings6Dof instead.")]
    public struct MLMovement6DofSettings
    {
        /// <summary>
        /// If the object should automatically center on the control direction when beginning movement.
        /// </summary>
        public bool AutoCenter;
    }
}

#endif
