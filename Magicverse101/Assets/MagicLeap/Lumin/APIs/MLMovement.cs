// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMovement.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Runtime.InteropServices;

    using UnityEngine;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// MLMovement class is the entry point for the Movement API.
    /// </summary>
    public sealed partial class MLMovement
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// Contains the string to print in case the lambda function fails.
        /// </summary>
        private static string errorMsg;

        /// <summary>
        /// Starts a new 3 degrees of freedom movement session.
        /// A 3 degrees of freedom movement session relies on the orientation of the user's pointing device, but
        /// ignores the position, when moving the target object.
        /// </summary>
        /// <param name="settings">Settings to use for new movement session.</param>
        /// <param name="dofSettings">3 degrees of freedom settings for movement sessions.</param>
        /// <param name="controls">3 degrees of freedom control settings for movement session.</param>
        /// <param name="obj">Object for the movement session.</param>
        /// <param name="sessionHandle">Handle to fill in for new movement session.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if input parameter is invalid.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
        /// </returns>
        public static MLResult Start3Dof(in Settings settings, in Settings3Dof dofSettings, in Controls3Dof controls, in MovementObject obj, out ulong sessionHandle)
        {
            errorMsg = "MLMovement.Start3Dof failed due to internal error";

            NativeBindings.SettingsNative internalSettings = NativeBindings.SettingsNative.Create();
            internalSettings.Data = settings;
            NativeBindings.Settings3DofNative internalDofSettings = NativeBindings.Settings3DofNative.Create();
            internalDofSettings.Data = dofSettings;
            NativeBindings.Controls3DofNative internalControls = NativeBindings.Controls3DofNative.Create();
            internalControls.Data = controls;
            NativeBindings.MovementObjectNative internalObj = NativeBindings.MovementObjectNative.Create();
            internalObj.Data = obj;

            ulong tempHandle = MagicLeapNativeBindings.InvalidHandle;

            MLResult Start3DofFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementStart3Dof(in internalSettings, in internalDofSettings, in internalControls, in internalObj, out tempHandle);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.Start3Dof failed to start new 3Dof movement session. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(Start3DofFunc);

            sessionHandle = tempHandle;

            return finalResult;
        }

        /// <summary>
        /// Starts a new 6 degrees of freedom movement session.
        /// A 6 degrees of freedom movement session relies on both the position and orientation of the user's pointing device
        /// when moving the target object.
        /// </summary>
        /// <param name="settings">Settings to use for new movement session.</param>
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
        public static MLResult Start6Dof(in Settings settings, in Settings6Dof dofSettings, in Controls6Dof controls, in MovementObject obj, out ulong sessionHandle)
        {
            errorMsg = "MLMovement.Start6Dof failed due to internal error";

            NativeBindings.SettingsNative internalSettings = NativeBindings.SettingsNative.Create();
            internalSettings.Data = settings;
            NativeBindings.Settings6DofNative internalDofSettings = NativeBindings.Settings6DofNative.Create();
            internalDofSettings.Data = dofSettings;
            NativeBindings.Controls6DofNative internalControls = NativeBindings.Controls6DofNative.Create();
            internalControls.Data = controls;
            NativeBindings.MovementObjectNative internalObj = NativeBindings.MovementObjectNative.Create();
            internalObj.Data = obj;

            ulong tempHandle = MagicLeapNativeBindings.InvalidHandle;

            MLResult Start6DofFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementStart6Dof(in internalSettings, in internalDofSettings, in internalControls, in internalObj, out tempHandle);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.Start6Dof failed to start new 6Dof movement session. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(Start6DofFunc);

            sessionHandle = tempHandle;

            return finalResult;
        }

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
        public static MLResult ChangeDepth(ulong sessionHandle, float deltaDepth)
        {
            errorMsg = "MLMovement.ChangeDepth failed due to internal error";

            MLResult ChangeDepthFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementChangeDepth(sessionHandle, deltaDepth);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.ChangeDepth failed to change depth of movement session. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(ChangeDepthFunc);

            return finalResult;
        }

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
        public static MLResult ChangeRotation(ulong sessionHandle, float deltaRadians)
        {
            errorMsg = "MLMovement.ChangeRotation failed due to internal error";

            MLResult ChangeRotationFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementChangeRotation(sessionHandle, deltaRadians);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.ChangeRotation failed to change depth of movement session. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(ChangeRotationFunc);

            return finalResult;
        }

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
        public static MLResult Update3Dof(ulong sessionHandle, in Controls3Dof controls, float deltaTime, ref MovementObject inOutObj)
        {
            errorMsg = "MLMovement.Update3Dof failed due to internal error";

            NativeBindings.Controls3DofNative internalControls = NativeBindings.Controls3DofNative.Create();
            internalControls.Data = controls;
            NativeBindings.MovementObjectNative internalInOutObj = NativeBindings.MovementObjectNative.Create();
            internalInOutObj.Data = inOutObj;

            MLResult Update3DofFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementUpdate3Dof(sessionHandle, in internalControls, deltaTime, ref internalInOutObj);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.Update3Dof failed to update 3Dof movement session. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(Update3DofFunc);

            inOutObj = internalInOutObj.Data;

            return finalResult;
        }

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
        public static MLResult Update6Dof(ulong sessionHandle, in Controls6Dof controls, float deltaTime, ref MovementObject inOutObj)
        {
            errorMsg = "MLMovement.Update6Dof failed due to internal error";

            NativeBindings.Controls6DofNative internalControls = NativeBindings.Controls6DofNative.Create();
            internalControls.Data = controls;
            NativeBindings.MovementObjectNative internalInOutObj = NativeBindings.MovementObjectNative.Create();
            internalInOutObj.Data = inOutObj;

            MLResult Update6DofFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementUpdate6Dof(sessionHandle, in internalControls, deltaTime, ref internalInOutObj);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.Update6Dof failed to update 6Dof movement session. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(Update6DofFunc);

            inOutObj = internalInOutObj.Data;

            return finalResult;
        }

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
        public static MLResult End(ulong sessionHandle, float deltaTime, out MovementObject obj)
        {
            errorMsg = "MLMovement.End failed due to internal error";

            NativeBindings.MovementObjectNative internalObj = NativeBindings.MovementObjectNative.Create();

            MLResult EndFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementEnd(sessionHandle, deltaTime, out internalObj);
                if (result != MLResult.Code.Ok && result != MLResult.Code.Pending && result != MLResult.Code.Timeout)
                {
                    MLPluginLog.ErrorFormat("MLMovement.End failed to end movement session. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(EndFunc);

            obj = internalObj.Data;

            return finalResult;
        }

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
        public static MLResult StartHardCollision(ulong sessionHandle, in Vector3 contactNormal, out ulong collisionHandle)
        {
            errorMsg = "MLMovement.StartHardCollision failed due to internal error";

            MagicLeapNativeBindings.MLVec3f nativeNormal = MLConvert.FromUnity(contactNormal);

            ulong tempHandle = MagicLeapNativeBindings.InvalidHandle;

            MLResult StartHardCollisionFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementStartHardCollision(sessionHandle, in nativeNormal, out tempHandle);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.StartHardCollision failed to start tracking hard collision session. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(StartHardCollisionFunc);

            collisionHandle = tempHandle;

            return finalResult;
        }

        /// <summary>
        /// Start tracking a new soft collision for a movement session.
        /// Soft collisions will allow a degree of interpenetration during movement. Collisions will resolve over time
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
        public static MLResult StartSoftCollision(ulong sessionHandle, in Vector3 otherPosition, float closestDistance, float maxDistance, out ulong collisionHandle)
        {
            errorMsg = "MLMovement.StartSoftCollision failed due to internal error";

            MagicLeapNativeBindings.MLVec3f nativeOtherPosition = MLConvert.FromUnity(otherPosition);
            float nativeClosestDistance = MLConvert.FromUnity(closestDistance);
            float nativeMaxDistance = MLConvert.FromUnity(maxDistance);

            ulong tempHandle = MagicLeapNativeBindings.InvalidHandle;

            MLResult StartSoftCollisionFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementStartSoftCollision(sessionHandle, in nativeOtherPosition, nativeClosestDistance, nativeMaxDistance, out tempHandle);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.StartSoftCollision failed to start tracking soft collision session. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(StartSoftCollisionFunc);

            collisionHandle = tempHandle;

            return finalResult;
        }

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
        public static MLResult UpdateHardCollision(ulong sessionHandle, ulong collisionHandle, in Vector3 contactNormal)
        {
            errorMsg = "MLMovement.UpdateHardCollision failed due to internal error";

            MagicLeapNativeBindings.MLVec3f nativeContactNormal = MLConvert.FromUnity(contactNormal);

            MLResult UpdateHardCollisionFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementUpdateHardCollision(sessionHandle, collisionHandle, in nativeContactNormal);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.UpdateHardCollision failed to update hard collision normal. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(UpdateHardCollisionFunc);

            return finalResult;
        }

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
        public static MLResult EndCollision(ulong sessionHandle, ulong collisionHandle)
        {
            errorMsg = "MLMovement.EndCollision failed due to internal error";

            MLResult EndCollisionFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementEndCollision(sessionHandle, collisionHandle);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.EndCollision failed to finish tracking a collision. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(EndCollisionFunc);

            return finalResult;
        }

        /// <summary>
        /// Gets the result string for a MLResult.Code.
        /// </summary>
        /// <param name="result">The MLResult.Code to be requested.</param>
        /// <returns>A pointer to the result string.</returns>
        internal static IntPtr GetResultString(MLResult.Code resultCode)
        {
            try
            {
                return NativeBindings.MLMovementGetResultString(resultCode);
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error("MLMovement.GetResultString failed. Reason: MLMovement API is currently available only on device.");
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLMovement.GetResultString failed. Reason: API symbols not found");
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Executes a function passed in and checks for exceptions.
        /// </summary>
        /// <param name="func">Function to execute.</param>
        /// <returns>MLResult with the return value of the function or <c>MLResult.Code.UnspecifiedFailure</c> in case of an exception.</returns>
        private static MLResult TryExecute(Func<MLResult> func)
        {
            MLResult finalResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, errorMsg);
            try
            {
                if (func != null)
                {
                    finalResult = func();
                }
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error("MLMovement API is currently available only on device.");
                finalResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Dll not found");
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("API symbols not found");
                finalResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, "API symbols not found");
            }

            return finalResult;
        }
        #endif

        /// <summary>
        /// Settings for the movement session.
        /// </summary>
        [Serializable]
        public struct Settings
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

            /// <summary>
            /// Creates a new MLMovement.Settings struct with all of the current system movement settings.
            /// </summary>
            /// <returns>The new MLMovement.Settings struct.</returns>
            public static Settings Create()
            {
                #if PLATFORM_LUMIN
                return NativeBindings.SettingsNative.Create().Data;
                #else
                return new Settings();
                #endif
            }
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Parameters that define the input controls for 3 degrees of freedom movement.
        /// Under 3 degrees of freedom movement mode, only the orientation of the user's pointing device
        /// is taken into account for moving the target object and the pointing device's position is not used.
        /// Head pose position is also required by the algorithm, and is used as the center point around which
        /// the target object moves.
        /// When the movement session starts, the initial orientation of the control forms a baseline, from
        /// which further changes in the orientation will cause the object to move around the user.
        /// </summary>
        public struct Controls3Dof
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
        public struct Controls6Dof
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
        public struct MovementObject
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
        public struct Settings3Dof
        {
            /// <summary>
            /// Determines if the object should automatically center on the control direction when beginning movement.
            /// </summary>
            public bool AutoCenter;
        }

        /// <summary>
        /// 6 degrees of freedom specific movement settings that must be provided when starting a 6 degrees
        /// of freedom movement session.
        /// A 6 degrees of freedom movement session relies on both the position and orientation of the user's
        /// pointing device when moving the target object.
        /// </summary>
        public struct Settings6Dof
        {
            /// <summary>
            /// Determines if the object should automatically center on the control direction when beginning movement.
            /// </summary>
            public bool AutoCenter;
        }
        #endif
    }
}
