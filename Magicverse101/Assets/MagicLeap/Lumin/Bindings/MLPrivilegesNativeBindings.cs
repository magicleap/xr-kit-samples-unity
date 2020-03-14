// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLPrivilegesNativeBindings.cs" company="Magic Leap, Inc">
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
    /// Functionality to validate or query privileges from the system.
    /// </summary>
    public sealed partial class MLPrivileges : MLAPISingleton<MLPrivileges>
    {
        /// <summary>
        /// See ml_privileges.h for additional comments.
        /// </summary>
        private class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// MLPrivileges library name.
            /// </summary>
            private const string MLPrivilegesDll = "ml_privileges";

            /// <summary>
            /// Prevents a default instance of the <see cref="NativeBindings"/> class from being created.
            /// </summary>
            private NativeBindings()
            {
            }

            /// <summary>
            /// Start the privilege-checking system. This function should be called
            /// before any privilege-checking functions are called.
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the privilege system startup succeeded.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the privilege system failed to startup.
            /// </returns>
            [DllImport(MLPrivilegesDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPrivilegesStartup();

            /// <summary>
            /// Shut down and clean all resources used by the privilege-checking
            /// system.This function should be called prior to exiting the program if
            /// a call to MLPrivilegesStartup() was made.
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the privilege system shutdown succeeded.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the privilege system failed to shutdown.
            /// </returns>
            [DllImport(MLPrivilegesDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPrivilegesShutdown();

            /// <summary>
            /// Check whether the application has the specified privileges.
            /// This does not solicit consent from the end-user.
            /// </summary>
            /// <param name="privilegeId">Privilege to check.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeGranted</c> if the privilege is granted.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeNotGranted</c> if the privilege is denied.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the privilege system was not started.
            /// </returns>
            [DllImport(MLPrivilegesDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPrivilegesCheckPrivilege(MLPrivileges.Id privilegeId);

            /// <summary>
            /// Request the specified privilege. This may solicit consent from the end-user.
            /// When waiting for user consent, this function blocks.
            /// </summary>
            /// <param name="privilegeId">Privilege to request.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeGranted</c> if the privilege is granted.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeNotGranted</c> if the privilege is denied.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the privilege system was not started.
            /// </returns>
            [DllImport(MLPrivilegesDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPrivilegesRequestPrivilege(MLPrivileges.Id privilegeId);

            /// <summary>
            /// Request the specified privileges. This may solicit consent from the end-user.
            /// </summary>
            /// <param name="privilegeId">Privilege to request.</param>
            /// <param name="request">The request object.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the privilege request is in progress.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the request is a null pointer.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the privilege system was not started.
            /// </returns>
            [DllImport(MLPrivilegesDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPrivilegesRequestPrivilegeAsync(MLPrivileges.Id privilegeId, ref IntPtr request);

            /// <summary>
            /// Try to get the result from a privilege request.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeGranted</c> if the privilege is granted.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeNotGranted</c> if the privilege is denied.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the privilege request is in progress.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the request is a null pointer.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the privilege system was not started.
            /// </returns>
            [DllImport(MLPrivilegesDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPrivilegesRequestPrivilegeTryGet(IntPtr request);

            /// <summary>
            /// Gets the result string for a MLResult.Code.
            /// </summary>
            /// <param name="result">The MLResult.Code to be requested.</param>
            /// <returns>A pointer to the result string.</returns>
            [DllImport(MLPrivilegesDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MLPrivilegesGetResultString(MLResult.Code result);
        }
    }
}

#endif
