// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLTokenAgentNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

// Disable warnings about missing documentation for native interop.
#pragma warning disable 1591

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// A class for retrieving the client credentials.
    /// </summary>
    public sealed partial class MLTokenAgent
    {
        /// <summary>
        /// See ml_token_agent.h for additional comments
        /// </summary>
        private class NativeBindings : MagicLeapNativeBindings
        {
            /// <summary>
            /// MLIdentity library name.
            /// </summary>
            private const string MLIdentityDll = "ml_identity";

            /// <summary>
            /// Map of MLTokenAgent.ClientCredentials class references to their unmanaged memory pointers.
            /// </summary>
            private static Dictionary<MLTokenAgent.ClientCredentials, IntPtr> clientCredentialsPtrMap = new Dictionary<MLTokenAgent.ClientCredentials, IntPtr>();

            /// <summary>
            /// Map of MLTokenAgent.ClientCredentials class references to their associated MLInvokeFuture pointers used for polling.
            /// </summary>
            private static Dictionary<MLTokenAgent.ClientCredentials, IntPtr> clientCredentialsFuturePtrMap = new Dictionary<MLTokenAgent.ClientCredentials, IntPtr>();

            /// <summary>
            /// Initializes a new instance of the <see cref="NativeBindings" /> class.
            /// </summary>
            protected NativeBindings()
            {
            }

            /// <summary>
            /// ReleaseClientCredentials releases all resources associated with the
            /// MLTokenAgentClientCredentials structure that was returned by the library.
            /// </summary>
            /// <param name="clientCredentials">Reference to the clientCredentials object.</param>
            public static void CleanupClientCredentialsMemory(MLTokenAgent.ClientCredentials clientCredentials)
            {
                if (clientCredentials.CurrentRequest != null)
                {
                    MLPluginLog.Warning("This client has an ongoing request and cannot have it's memory released.");
                }

                IntPtr clientCredentialsPtr = clientCredentialsPtrMap.ContainsKey(clientCredentials) ? clientCredentialsPtrMap[clientCredentials] : IntPtr.Zero;

                if (clientCredentialsPtr != IntPtr.Zero)
                {
                    try
                    {
                        MLResult.Code resultCode = MLTokenAgent.NativeBindings.MLTokenAgentReleaseClientCredentials(clientCredentialsPtr);

                        if (resultCode != MLResult.Code.Ok)
                        {
                            MLPluginLog.ErrorFormat("MLTokenAgent.CleanupClientCredentialsMemory failed. Reason: {0}", resultCode);
                        }
                    }
                    catch (EntryPointNotFoundException)
                    {
                        MLPluginLog.Error("MLTokenAgent.CleanupClientCredentialsMemory failed. Reason: API symbols not found.");
                    }
                }
            }

            /// <summary>
            /// GetClientCredentials is a blocking function that accesses the cloud and
            /// returns a MLTokenAgentClientCredentials structure containing the users credentials and
            /// tokens for a particular service (Audience).
            /// The library deduces the Audience being requested from the name of the calling service.
            /// </summary>
            /// <param name="clientCredentials">Reference to the clientCredentials object.</param>
            /// <param name="credentials">Reference to the credentials struct of the clientCredentials object.</param>
            /// <param name="tokens">Reference to the tokens struct of the clientCredentials object.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
            /// MLResult.Result will be <c>MLResult.Code.TokenAgent*</c> if a token specific failure occurred during the operation.
            /// </returns>
            public static MLResult.Code GetClientCredentials(MLTokenAgent.ClientCredentials clientCredentials, ref MLTokenAgent.Credentials credentials, ref MLTokenAgent.Tokens tokens)
            {
                try
                {
                    IntPtr clientCredentialsPtr = clientCredentialsPtrMap.ContainsKey(clientCredentials) ? clientCredentialsPtrMap[clientCredentials] : IntPtr.Zero;
                    MLResult.Code resultCode = MLTokenAgent.NativeBindings.MLTokenAgentGetClientCredentials(ref clientCredentialsPtr);
                    if (MLResult.IsOK(resultCode))
                    {
                        clientCredentialsPtrMap.Remove(clientCredentials);
                        clientCredentialsPtrMap.Add(clientCredentials, clientCredentialsPtr);

                        MLTokenAgent.NativeBindings.ClientCredentialsNative clientCredentialsNative = (MLTokenAgent.NativeBindings.ClientCredentialsNative)Marshal.PtrToStructure(clientCredentialsPtr, typeof(MLTokenAgent.NativeBindings.ClientCredentialsNative));
                        credentials = clientCredentialsNative.Credentials;
                        tokens = clientCredentials.Tokens;
                    }
                    else
                    {
                        credentials = new Credentials();
                        tokens = new Tokens();
                    }

                    return resultCode;
                }
                catch (EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLTokenAgent.NativeBindings.GetClientCredentials failed. Reason: API symbols not found.");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }

            /// <summary>
            /// Invokes the MLTokenAgentGetClientCredentials() function asynchronously (in a different thread).
            /// </summary>
            /// <param name="clientCredentials">Reference to the clientCredentials object.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the profile or out_future were 0 (null).
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if the caller does not have the ClientCredentialsRead privilege.
            /// MLResult.Result will be <c>MLResult.Code.TokenAgent*</c> if a token specific failure occurred during the operation.
            /// </returns>
            public static MLResult.Code RequestClientCredentialsAsync(MLTokenAgent.ClientCredentials clientCredentials)
            {
                IntPtr clientCredentialsFuturePtr = clientCredentialsFuturePtrMap.ContainsKey(clientCredentials) ? clientCredentialsFuturePtrMap[clientCredentials] : IntPtr.Zero;

                try
                {
                    MLResult.Code resultCode = MLTokenAgent.NativeBindings.MLTokenAgentGetClientCredentialsAsync(ref clientCredentialsFuturePtr);

                    if (MLResult.IsOK(resultCode))
                    {
                        clientCredentialsFuturePtrMap.Remove(clientCredentials);
                        clientCredentialsFuturePtrMap.Add(clientCredentials, clientCredentialsFuturePtr);
                    }
                    else if (resultCode == MLResult.Code.PrivilegeDenied)
                    {
                        MLPluginLog.Warning("MLTokenAgent.NativeBindings.RequestClientCredentialsAsync failed. Reason: Caller does not have IdentityRead Privilege.");
                    }
                    else
                    {
                        MLPluginLog.ErrorFormat("MLTokenAgent.NativeBindings.RequestClientCredentialsAsync failed. Reason: {0}", resultCode);
                    }

                    return resultCode;
                }
                catch (EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLTokenAgent.NativeBindings.RequestClientCredentialsAsync failed. Reason: API symbols not found.");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }

            /// <summary>
            /// Having made a call to MLTokenAgentGetClientCredentialsAsync(), the user can call MLTokenAgentGetClientCredentialsWait()
            /// to detect whether the asynchronous call completed and if so, to retrieve the credentials in out_credentials.
            /// </summary>
            /// <param name="clientCredentials">Reference to the clientCredentials object.</param>
            /// <param name="credentials">Reference to the credentials struct of the clientCredentials object.</param>
            /// <param name="tokens">Reference to the tokens struct of the clientCredentials object.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully before the timeout elapsed.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the timeout elapsed before the asynchronous call completed.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the future or out_credentials were 0 (null).
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if the caller does not have the ClientCredentialsRead privilege.
            /// MLResult.Result will be <c>MLResult.Code.TokenAgent*</c> if a token specific failure occurred during the operation.
            /// </returns>
            public static MLResult.Code ListenClientCredentialsResponse(MLTokenAgent.ClientCredentials clientCredentials, ref MLTokenAgent.Credentials credentials, ref MLTokenAgent.Tokens tokens)
            {
                try
                {
                    IntPtr clientCredentialsPtr = clientCredentialsPtrMap.ContainsKey(clientCredentials) ? clientCredentialsPtrMap[clientCredentials] : IntPtr.Zero;
                    IntPtr clientCredentialsFuturePtr = clientCredentialsFuturePtrMap.ContainsKey(clientCredentials) ? clientCredentialsFuturePtrMap[clientCredentials] : IntPtr.Zero;

                    if (clientCredentialsFuturePtr == IntPtr.Zero)
                    {
                        MLPluginLog.Warning("MLTokenAgent.NativeBindings.ListenClientCredentialsResponse failed because a valid future pointer could not be found with the passed request.");
                        return MLResult.Code.UnspecifiedFailure;
                    }

                    //// Attempt to get data if available, 0 is passed as a timeout to immediately return and never wait for results.
                    MLResult.Code resultCode = MLTokenAgent.NativeBindings.MLTokenAgentGetClientCredentialsWait(clientCredentialsFuturePtr, 0, ref clientCredentialsPtr);

                    // If it succeeded, copy any modifications made to the profile in unmanaged memory by the Identity API to managed memory.
                    if (MLResult.IsOK(resultCode))
                    {
                        clientCredentialsFuturePtrMap.Remove(clientCredentials);

                        clientCredentialsPtrMap.Remove(clientCredentials);
                        clientCredentialsPtrMap.Add(clientCredentials, clientCredentialsPtr);

                        MLTokenAgent.NativeBindings.ClientCredentialsNative clientCredentialsNative = (MLTokenAgent.NativeBindings.ClientCredentialsNative)Marshal.PtrToStructure(clientCredentialsPtr, typeof(MLTokenAgent.NativeBindings.ClientCredentialsNative));
                        credentials = clientCredentialsNative.Credentials;
                        tokens = clientCredentials.Tokens;
                    }
                    else
                    {
                        credentials = new Credentials();
                        tokens = new Tokens();
                    }

                    return resultCode;
                }
                catch (EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLTokenAgent.NativeBindings.ListenClientCredentialsResponse failed. Reason: API symbols not found.");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }

            /// <summary>
            /// A blocking function that accesses the cloud and returns a pointer to a structure
            /// containing the users credentials and tokens for a particular service(Audience).
            /// </summary>
            /// <param name="outCredentials">A pointer to the client credentials.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the out_credentials was 0 (null).
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if the caller does not have the ClientCredentialsRead privilege.
            /// MLResult.Result will be <c>MLResult.Code.TokenAgent*</c> if a token specific failure occurred during the operation.
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLTokenAgentGetClientCredentials(ref IntPtr outCredentials);

            /// <summary>
            /// Invokes the MLTokenAgentGetClientCredentials() function asynchronously (in a different thread).
            /// </summary>
            /// <param name="outFuture">A pointer to a MLInvokeFuture pointer which provides the ability to poll for completion and
            /// to retrieve the credentials returned by MLTokenAgentGetClientCredentials().</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the profile or out_future were 0 (null).
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if the caller does not have the ClientCredentialsRead privilege.
            /// MLResult.Result will be <c>MLResult.Code.TokenAgent*</c> if a token specific failure occurred during the operation.
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLTokenAgentGetClientCredentialsAsync(ref IntPtr outFuture);

            /// <summary>
            /// Having made a call to MLTokenAgentGetClientCredentialsAsync(), the user can call MLTokenAgentGetClientCredentialsWait()
            /// to detect whether the asynchronous call completed and if so, to retrieve the credentials in out_credentials.
            /// </summary>
            /// <param name="future">The pointer returned by the MLTokenAgentGetClientCredentialsAsync() function.</param>
            /// <param name="msecTimeout">The timeout in milliseconds.</param>
            /// <param name="outCredentials">The location in memory where the pointer to the credentials
            /// structure allocated by the library will be copied, if the asynchronous call completed,
            /// or 0 (null) if not.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully before the timeout elapsed.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the timeout elapsed before the asynchronous call completed.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the future or out_credentials were 0 (null).
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if the caller does not have the ClientCredentialsRead privilege.
            /// MLResult.Result will be <c>MLResult.Code.TokenAgent*</c> if a token specific failure occurred during the operation.
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLTokenAgentGetClientCredentialsWait(
                IntPtr future,
                uint msecTimeout,
                ref IntPtr outCredentials);

            /// <summary>
            /// MLTokenAgentReleaseClientCredentials() releases all resources associated with the
            /// client credentials structure that was returned by the library.
            /// </summary>
            /// <param name="credentials">A pointer to the client credentials to release.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the credentials was 0 (null).
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLTokenAgentReleaseClientCredentials(IntPtr credentials);

            /// <summary>
            /// Gets the result string for a MLResult.Code.
            /// </summary>
            /// <param name="result">The MLResult.Code to be requested.</param>
            /// <returns>A pointer to the result string.</returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MLTokenAgentGetResultString(MLResult.Code result);

            /// <summary>
            /// Internal raw representation of C API's MLTokenAgentClientCredentials.
            /// MLTokenAgentClientCredentials represents the credentials and tokens of the User-Audience pair
            /// that is associated with the calling service.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            internal struct ClientCredentialsNative
            {
                /// <summary>
                /// The credentials that can be used to access a particular service (Audience).
                /// </summary>
                public MLTokenAgent.Credentials Credentials;

                /// <summary>
                /// The tokens that can be used to manage the user profile for a particular Audience.
                /// </summary>
                public MLTokenAgent.Tokens Tokens;
            }
        }
    }
}
#endif
