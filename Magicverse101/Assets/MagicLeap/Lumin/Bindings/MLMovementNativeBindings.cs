// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMovementNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;

    /// <summary>
    /// MLMovement class is the entry point for the Movement API.
    /// </summary>
    public sealed partial class MLMovement
    {
        /// <summary>
        /// See ml_movement.h for additional comments.
        /// </summary>
        private partial class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// MLMovement library name.
            /// </summary>
            private const string MLMovementDll = "ml_movement";

            /// <summary>
            /// Stores the default system movement settings.
            /// </summary>
            private static SettingsNative? defaultSettings;

            /// <summary>
            /// Prevents a default instance of the <see cref="NativeBindings"/> class from being created.
            /// </summary>
            private NativeBindings()
            {
            }

            /// <summary>
            /// This populates the provided SettingsNative struct with all of the current system
            /// movement settings.
            /// </summary>
            /// <param name="settings">Settings struct to fill out.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if input parameter is invalid.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementGetDefaultSettings(out SettingsNative settings);

            /// <summary>
            /// Starts a new 3 degrees of freedom movement session.
            /// A 3 degrees of freedom movement session relies on the orientation of the user's pointing device, but
            /// ignores the position, when moving the target object.
            /// </summary>
            /// <param name="sessionSettings">Settings to use for new movement session.</param>
            /// <param name="dofSettings">3 degrees of freedom settings for movement sessions.</param>
            /// <param name="controls">3 degrees of freedom control settings for movement session .</param>
            /// <param name="obj">Object for the movement session.</param>
            /// <param name="sessionHandle">Handle to fill in for new movement session.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if input parameter is invalid.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementStart3Dof(in SettingsNative sessionSettings, in Settings3DofNative dofSettings, in Controls3DofNative controls, in MovementObjectNative obj, out ulong sessionHandle);

            /// <summary>
            /// Starts a new 6 degrees of freedom movement session.
            /// A 6 degrees of freedom movement session relies on both the position and orientation of the user's pointing device
            /// when moving the target object.
            /// </summary>
            /// <param name="sessionSettings">Settings to use for new movement session.</param>
            /// <param name="dofSettings">6 degrees of freedom settings for movement session.</param>
            /// <param name="controls">6 degrees of freedom control settings for movement session.</param>
            /// <param name="obj">Object for the movement session.</param>
            /// <param name="sessionHandle">Handle to fill in for new movement session.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if input parameter is invalid.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementStart6Dof(in SettingsNative sessionSettings, in Settings6DofNative dofSettings, in Controls6DofNative controls, in MovementObjectNative obj, out ulong sessionHandle);

            /// <summary>
            /// Change the depth offset of the object from the user's head pose. (when using a 3DoF movement
            /// session) or from the pointing device (when using a 6DoF movement session).
            /// </summary>
            /// <param name="sessionHandle">Handle for the movement session.</param>
            /// <param name="deltaDepth">Depth delta change.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementChangeDepth(ulong sessionHandle, float deltaDepth);

            /// <summary>
            /// Change the rotation about the up-axis of the object being moved.
            /// This rotation applies to the object in its local space, and not to the rotation relative to
            /// the user's head pose (when using a 3DoF movement session) or relative to the pointing device
            /// (when using a 6DoF movement session).
            /// </summary>
            /// <param name="sessionHandle">Handle for the movement session.</param>
            /// <param name="deltaRadians">Rotation delta change.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementChangeRotation(ulong sessionHandle, float deltaRadians);

            /// <summary>
            /// Update 3 degrees of freedom movement with new data.
            /// This function is intended to be called once per frame per active 3 degrees of freedom movement session.
            /// </summary>
            /// <param name="sessionHandle">Handle for the movement session.</param>
            /// <param name="controls">New input data.</param>
            /// <param name="deltaTime">Time since last update.</param>
            /// <param name="inOutObj">Object to update.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if input parameters were invalid.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementUpdate3Dof(ulong sessionHandle, in Controls3DofNative controls, float deltaTime, ref MovementObjectNative inOutObj);

            /// <summary>
            /// Update 6 degrees of freedom movement with new data.
            /// This function is intended to be called once per frame per active 6 degrees of freedom movement session.
            /// </summary>
            /// <param name="sessionHandle">Handle for the movement session.</param>
            /// <param name="controls">New input data.</param>
            /// <param name="deltaTime">Time since last update.</param>
            /// <param name="inOutObj">Object to update.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if input parameters were invalid.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementUpdate6Dof(ulong sessionHandle, in Controls6DofNative controls, float deltaTime, ref MovementObjectNative inOutObj);

            /// <summary>
            /// End a movement session (either 3 degrees of freedom or 6 degrees of freedom).
            /// This function is intended to be called once per frame per active movement session while it returns
            /// MLResult_Pending. This will allow the session to resolve any remaining soft collisions.
            /// </summary>
            /// <param name="sessionHandle">Handle for the movement session.</param>
            /// <param name="deltaTime">Time since last update.</param>
            /// <param name="obj">Object to fill out with the final data.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the movement session is still resolving.
            /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if the movement session was unable to resolve and was forcefully ended.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if input parameters were invalid.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementEnd(ulong sessionHandle, float deltaTime, out MovementObjectNative obj);

            /// <summary>
            /// Start tracking a new hard collision for a movement session.
            /// Call this to inform the movement library that the moving object has collided with something impenetrable.
            /// The movement library will not allow the moving object to penetrate the other object in the direction
            /// opposite the contact normal. You can update the contact normal for the returned collision instance handle
            /// each frame by calling MLMovementUpdateHardCollision. This is useful if the moving object is colliding with a
            /// curved surface so the contact normal is changing as the moving object slides against the collision surface.
            /// </summary>
            /// <param name="sessionHandle">Handle for the movement session.</param>
            /// <param name="contactNormal">Normalized Vector3 surface normal of object that movement is colliding with.</param>
            /// <param name="collisionHandle">Handle for the collision session.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if contactNormal was invalid.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidCollisionHandle</c> if collision handle is invalid.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementStartHardCollision(ulong sessionHandle, in MLVec3f contactNormal, out ulong collisionHandle);

            /// <summary>
            /// Start tracking a new soft collision for a movement session.
            /// Soft collisions will allow a degree of interpenetration during movement.  Collisions will resolve over time
            /// giving the colliding objects a softness to them.
            /// </summary>
            /// <param name="sessionHandle">Handle for the movement session.</param>
            /// <param name="otherPosition">Position of the center of the thing we are soft-colliding with, the source of the repulsion force.</param>
            /// <param name="closestDistance">The position of the the object being moved will never be closer to other_position than this distance.</param>
            /// <param name="maxDistance">The moving object will not be affected by this soft collision once its position is this distance from other_position.</param>
            /// <param name="collisionHandle">Handle for the collision session.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if otherPosition was invalid.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidCollisionHandle</c> if collision handle is invalid.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementStartSoftCollision(ulong sessionHandle, in MLVec3f otherPosition, float closestDistance, float maxDistance, out ulong collisionHandle);

            /// <summary>
            /// Update the collision normal for an existing hard collision session in an existing movement session.
            /// This function is intended to be called once per hard collision per movement session per frame.
            /// </summary>
            /// <param name="sessionHandle">Handle for the movement session.</param>
            /// <param name="collisionHandle">Handle for the collision session.</param>
            /// <param name="contactNormal">Normalized Vector3 surface normal of object that movement is colliding with.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if contactNormal was invalid.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidCollisionHandle</c> if collision handle is invalid.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementUpdateHardCollision(ulong sessionHandle, ulong collisionHandle, in MLVec3f contactNormal);

            /// <summary>
            /// End the tracking of a soft or hard collision session for a movement session.
            /// </summary>
            /// <param name="sessionHandle">Handle for the movement session.</param>
            /// <param name="collisionHandle">Handle for the collision session.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
            /// MLResult.Result will be <c>MLResult.Code.MovementInvalidCollisionHandle</c> if collision handle is invalid.
            /// </returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLMovementEndCollision(ulong sessionHandle, ulong collisionHandle);

            /// <summary>
            /// Gets a readable version of the result code as an ASCII string.
            /// </summary>
            /// <param name="result">The MLResult.Code that should be converted.</param>
            /// <returns>ASCII string containing a readable version of the result code.</returns>
            [DllImport(MLMovementDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MLMovementGetResultString(MLResult.Code result);

            /// <summary>
            /// Links to <c>MLMovementSettings</c> in ml_movement.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public partial struct SettingsNative
            {
                /// <summary>
                /// Version number of this structure.
                /// </summary>
                public uint Version;

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

                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                public MLMovement.Settings Data
                {
                    get
                    {
                        return new MLMovement.Settings()
                        {
                            SwayHistorySize = this.SwayHistorySize,
                            MaxDeltaAngle = this.MaxDeltaAngle,
                            ControlDampeningFactor = this.ControlDampeningFactor,
                            MaxSwayAngle = this.MaxSwayAngle,
                            MaximumHeadposeRotationSpeed = this.MaximumHeadposeRotationSpeed,
                            MaximumHeadposeMovementSpeed = this.MaximumHeadposeMovementSpeed,
                            MaximumDepthDeltaForSway = this.MaximumDepthDeltaForSway,
                            MinimumDistance = this.MinimumDistance,
                            MaximumDistance = this.MaximumDistance,
                            MaximumSwayTimeSeconds = this.MaximumSwayTimeSeconds,
                            EndResolveTimeoutSeconds = this.EndResolveTimeoutSeconds,
                        };
                    }

                    set
                    {
                        this.SwayHistorySize = value.SwayHistorySize;
                        this.MaxDeltaAngle = value.MaxDeltaAngle;
                        this.ControlDampeningFactor = value.ControlDampeningFactor;
                        this.MaxSwayAngle = value.MaxSwayAngle;
                        this.MaximumHeadposeRotationSpeed = value.MaximumHeadposeRotationSpeed;
                        this.MaximumHeadposeMovementSpeed = value.MaximumHeadposeMovementSpeed;
                        this.MaximumDepthDeltaForSway = value.MaximumDepthDeltaForSway;
                        this.MinimumDistance = value.MinimumDistance;
                        this.MaximumDistance = value.MaximumDistance;
                        this.MaximumSwayTimeSeconds = value.MaximumSwayTimeSeconds;
                        this.EndResolveTimeoutSeconds = value.EndResolveTimeoutSeconds;
                    }
                }

                /// <summary>
                /// Create an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static SettingsNative Create()
                {
                    if (!defaultSettings.HasValue)
                    {
                        SettingsNative settings = new SettingsNative()
                        {
                            Version = 1u,
                            SwayHistorySize = 0u,
                            MaxDeltaAngle = 0.0f,
                            ControlDampeningFactor = 0.0f,
                            MaxSwayAngle = 0.0f,
                            MaximumHeadposeRotationSpeed = 0.0f,
                            MaximumHeadposeMovementSpeed = 0.0f,
                            MaximumDepthDeltaForSway = 0.0f,
                            MinimumDistance = 0.0f,
                            MaximumDistance = 0.0f,
                            MaximumSwayTimeSeconds = 0.0f,
                            EndResolveTimeoutSeconds = 0.0f,
                        };

                        try
                        {
                            MLResult.Code result = NativeBindings.MLMovementGetDefaultSettings(out settings);
                            if (result != MLResult.Code.Ok)
                            {
                                MLPluginLog.ErrorFormat("MLMovement.NativeBindings.SettingsNative.Create failed to get default movement settings. Reason: {0}", result);
                            }
                        }
                        catch (System.DllNotFoundException)
                        {
                            MLPluginLog.Error("MLMovement API is currently available only on device.");
                        }
                        catch (System.EntryPointNotFoundException)
                        {
                            MLPluginLog.Error("MLMovement API symbols not found");
                        }

                        defaultSettings = settings;
                    }

                    return defaultSettings.Value;
                }
            }

            /// <summary>
            /// Links to <c>MLMovement3DofControls</c> in ml_movement.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public partial struct Controls3DofNative
            {
                /// <summary>
                /// Version number of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// The world space position of the device's head pose.
                /// </summary>
                public MLVec3f HeadposePosition;

                /// <summary>
                /// The world space orientation of the user's pointing device.
                /// </summary>
                public MLQuaternionf ControlRotation;

                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                public MLMovement.Controls3Dof Data
                {
                    get
                    {
                        return new MLMovement.Controls3Dof()
                        {
                            HeadposePosition = Native.MLConvert.ToUnity(this.HeadposePosition),
                            ControlRotation = Native.MLConvert.ToUnity(this.ControlRotation),
                        };
                    }

                    set
                    {
                        this.HeadposePosition = Native.MLConvert.FromUnity(value.HeadposePosition);
                        this.ControlRotation = Native.MLConvert.FromUnity(value.ControlRotation);
                    }
                }

                /// <summary>
                /// Create an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static Controls3DofNative Create()
                {
                    return new Controls3DofNative()
                    {
                        Version = 1u,
                        HeadposePosition = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        ControlRotation = new MLQuaternionf() { X = 0.0f, Y = 0.0f, Z = 0.0f, W = 1.0f },
                    };
                }
            }

            /// <summary>
            /// Links to <c>MLMovement6DofControls</c> in ml_movement.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public partial struct Controls6DofNative
            {
                /// <summary>
                /// Version number of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// The world space position of the device's head pose.
                /// </summary>
                public MLVec3f HeadposePosition;

                /// <summary>
                /// The world space orientation of the device's head pose.
                /// </summary>
                public MLQuaternionf HeadposeRotation;

                /// <summary>
                /// The world space position of the user's pointing device.
                /// </summary>
                public MLVec3f ControlPosition;

                /// <summary>
                /// The world space orientation of the user's pointing device.
                /// </summary>
                public MLQuaternionf ControlRotation;

                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                public MLMovement.Controls6Dof Data
                {
                    get
                    {
                        return new MLMovement.Controls6Dof()
                        {
                            HeadposePosition = Native.MLConvert.ToUnity(this.HeadposePosition),
                            HeadposeRotation = Native.MLConvert.ToUnity(this.HeadposeRotation),
                            ControlPosition = Native.MLConvert.ToUnity(this.ControlPosition),
                            ControlRotation = Native.MLConvert.ToUnity(this.ControlRotation),
                        };
                    }

                    set
                    {
                        this.HeadposePosition = Native.MLConvert.FromUnity(value.HeadposePosition);
                        this.HeadposeRotation = Native.MLConvert.FromUnity(value.HeadposeRotation);
                        this.ControlPosition = Native.MLConvert.FromUnity(value.ControlPosition);
                        this.ControlRotation = Native.MLConvert.FromUnity(value.ControlRotation);
                    }
                }

                /// <summary>
                /// Create an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static Controls6DofNative Create()
                {
                    return new Controls6DofNative()
                    {
                        Version = 1u,
                        HeadposePosition = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        HeadposeRotation = new MLQuaternionf() { X = 0.0f, Y = 0.0f, Z = 0.0f, W = 1.0f },
                        ControlPosition = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        ControlRotation = new MLQuaternionf() { X = 0.0f, Y = 0.0f, Z = 0.0f, W = 1.0f },
                    };
                }
            }

            /// <summary>
            /// Links to <c>MLMovementObject</c> in ml_movement.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public partial struct MovementObjectNative
            {
                /// <summary>
                /// Version number of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// The world space translation of the object to move.
                /// </summary>
                public MLVec3f ObjectPosition;

                /// <summary>
                /// The world space orientation of the object to move.
                /// </summary>
                public MLQuaternionf ObjectRotation;

                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                public MLMovement.MovementObject Data
                {
                    get
                    {
                        return new MLMovement.MovementObject()
                        {
                            ObjectPosition = Native.MLConvert.ToUnity(this.ObjectPosition),
                            ObjectRotation = Native.MLConvert.ToUnity(this.ObjectRotation),
                        };
                    }

                    set
                    {
                        this.ObjectPosition = Native.MLConvert.FromUnity(value.ObjectPosition);
                        this.ObjectRotation = Native.MLConvert.FromUnity(value.ObjectRotation);
                    }
                }

                /// <summary>
                /// Create an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static MovementObjectNative Create()
                {
                    return new MovementObjectNative()
                    {
                        Version = 1u,
                        ObjectPosition = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        ObjectRotation = new MLQuaternionf() { X = 0.0f, Y = 0.0f, Z = 0.0f, W = 1.0f },
                    };
                }
            }

            /// <summary>
            /// Links to <c>MLMovement3DofSettings</c> in ml_movement.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public partial struct Settings3DofNative
            {
                /// <summary>
                /// Version number of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// If the object should automatically center on the control direction when beginning movement.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool AutoCenter;

                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                public MLMovement.Settings3Dof Data
                {
                    get
                    {
                        return new MLMovement.Settings3Dof()
                        {
                            AutoCenter = this.AutoCenter,
                        };
                    }

                    set
                    {
                        this.AutoCenter = value.AutoCenter;
                    }
                }

                /// <summary>
                /// Create an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static Settings3DofNative Create()
                {
                    return new Settings3DofNative()
                    {
                        Version = 1u,
                        AutoCenter = false,
                    };
                }
            }

            /// <summary>
            /// Links to <c>MLMovement6DofSettings</c> in ml_movement.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public partial struct Settings6DofNative
            {
                /// <summary>
                /// Version number of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// If the object should automatically center on the control direction when beginning movement.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool AutoCenter;

                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                public MLMovement.Settings6Dof Data
                {
                    get
                    {
                        return new MLMovement.Settings6Dof()
                        {
                            AutoCenter = this.AutoCenter,
                        };
                    }

                    set
                    {
                        this.AutoCenter = value.AutoCenter;
                    }
                }

                /// <summary>
                /// Create an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static Settings6DofNative Create()
                {
                    return new Settings6DofNative()
                    {
                        Version = 1u,
                        AutoCenter = false,
                    };
                }
            }
        }
    }
}

#endif
