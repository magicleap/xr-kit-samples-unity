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

#if PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// MLMovement class is the entry point for the Movement API.
    /// </summary>
    public sealed partial class MLMovement
    {
        /// <summary>
        /// This populates the provided MLMovementSettings struct with all of the current system
        /// movement settings.
        /// </summary>
        /// <param name="settings">Settings struct to fill out.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if input parameter is invalid.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        [Obsolete("Please use MLMovement.Settings.Create() instead.")]
        public static MLResult GetDefaultSettings(out MLMovementSettings settings)
        {
            errorMsg = "MLMovement.GetDefaultSettings failed due to internal error";

            MLMovementSettings tempSettings = default;

            MLResult GetDefaultSettingsFunc()
            {
                NativeBindings.SettingsNative internalSettings = NativeBindings.SettingsNative.Create();
                MLResult.Code result = NativeBindings.MLMovementGetDefaultSettings(out internalSettings);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.GetDefaultSettings failed to get default movement settings. Reason: {0}", result);
                }
                else
                {
                    tempSettings = internalSettings.DataEx;
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(GetDefaultSettingsFunc);

            settings = tempSettings;

            return finalResult;
        }

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
        [Obsolete("Please use MLMovement.Start3Dof(MLMovement.Settings, MLMovement.Settings3Dof, MLMovement.Controls3Dof, MLMovement.MovementObject, ulong) instead.")]
        public static MLResult Start3Dof(in MLMovementSettings settings, in MLMovement3DofSettings dofSettings, in MLMovement3DofControls controls, in MLMovementObject obj, out ulong sessionHandle)
        {
            errorMsg = "MLMovement.Start3Dof failed due to internal error";

            NativeBindings.SettingsNative internalSettings = NativeBindings.SettingsNative.Create();
            internalSettings.DataEx = settings;
            NativeBindings.Settings3DofNative internalDofSettings = NativeBindings.Settings3DofNative.Create();
            internalDofSettings.DataEx = dofSettings;
            NativeBindings.Controls3DofNative internalControls = NativeBindings.Controls3DofNative.Create();
            internalControls.DataEx = controls;
            NativeBindings.MovementObjectNative internalObj = NativeBindings.MovementObjectNative.Create();
            internalObj.DataEx = obj;

            ulong tempHandle = MagicLeapNativeBindings.InvalidHandle;

            MLResult Start3DofFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementStart3Dof(internalSettings, internalDofSettings, internalControls, internalObj, out tempHandle);
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
        [Obsolete("Please use MLMovement.Start6Dof(MLMovement.Settings, MLMovement.Controls6Dof, MLMovement.MovementObject, ulong) instead.")]
        public static MLResult Start6Dof(in MLMovementSettings settings, in MLMovement6DofSettings dofSettings, in MLMovement6DofControls controls, in MLMovementObject obj, out ulong sessionHandle)
        {
            errorMsg = "MLMovement.Start6Dof failed due to internal error";

            NativeBindings.SettingsNative internalSettings = NativeBindings.SettingsNative.Create();
            internalSettings.DataEx = settings;
            NativeBindings.Settings6DofNative internalDofSettings = NativeBindings.Settings6DofNative.Create();
            internalDofSettings.DataEx = dofSettings;
            NativeBindings.Controls6DofNative internalControls = NativeBindings.Controls6DofNative.Create();
            internalControls.DataEx = controls;
            NativeBindings.MovementObjectNative internalObj = NativeBindings.MovementObjectNative.Create();
            internalObj.DataEx = obj;

            ulong tempHandle = MagicLeapNativeBindings.InvalidHandle;

            MLResult Start6DofFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementStart6Dof(internalSettings, internalDofSettings, internalControls, internalObj, out tempHandle);
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
        [Obsolete("Please use MLMovement.Update3Dof(ulong, MLMovement.Controls3Dof, float, MLMovement.MovementObject) instead.")]
        public static MLResult Update3Dof(ulong sessionHandle, in MLMovement3DofControls controls, float deltaTime, ref MLMovementObject inOutObj)
        {
            errorMsg = "MLMovement.Update3Dof failed due to internal error";

            NativeBindings.Controls3DofNative internalControls = NativeBindings.Controls3DofNative.Create();
            internalControls.DataEx = controls;
            NativeBindings.MovementObjectNative internalInOutObj = NativeBindings.MovementObjectNative.Create();
            internalInOutObj.DataEx = inOutObj;

            MLResult Update3DofFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementUpdate3Dof(sessionHandle, internalControls, deltaTime, ref internalInOutObj);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.Update3Dof failed to update 3Dof movement session. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(Update3DofFunc);

            inOutObj = internalInOutObj.DataEx;

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
        [Obsolete("Please use MLMovement.Update6Dof(ulong, MLMovement.Controls6Dof, float, MLMovement.MovementObject) instead.")]
        public static MLResult Update6Dof(ulong sessionHandle, in MLMovement6DofControls controls, float deltaTime, ref MLMovementObject inOutObj)
        {
            errorMsg = "MLMovement.Update6Dof failed due to internal error";

            NativeBindings.Controls6DofNative internalControls = NativeBindings.Controls6DofNative.Create();
            internalControls.DataEx = controls;
            NativeBindings.MovementObjectNative internalInOutObj = NativeBindings.MovementObjectNative.Create();
            internalInOutObj.DataEx = inOutObj;

            MLResult Update6DofFunc()
            {
                MLResult.Code result = NativeBindings.MLMovementUpdate6Dof(sessionHandle, internalControls, deltaTime, ref internalInOutObj);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMovement.Update6Dof failed to update 6Dof movement session. Reason: {0}", result);
                }

                return MLResult.Create(result);
            }

            MLResult finalResult = TryExecute(Update6DofFunc);

            inOutObj = internalInOutObj.DataEx;

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
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if the movement session was unable to resolve and was forcefully ended..
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if input parameters were invalid.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MovementInvalidMovementHandle</c> if session handle is invalid.
        /// </returns>
        [Obsolete("Please use MLMovement.End(ulong, float, MLMovement.MovementObject) instead.")]
        public static MLResult End(ulong sessionHandle, float deltaTime, out MLMovementObject obj)
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

            obj = internalObj.DataEx;

            return finalResult;
        }

        /// <summary>
        /// Gets a readable version of the result code as an ASCII string.
        /// </summary>
        /// <param name="resultCode">The MLResult that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        [Obsolete("Please use MLResult.CodeToString(MLResult.Code) instead.", true)]
        public static string GetResultString(MLResultCode resultCode)
        {
            return "This function is deprecated. Use MLMovement.CodeToString(MLResult.Code) instead.";
        }
    }
}

#endif
