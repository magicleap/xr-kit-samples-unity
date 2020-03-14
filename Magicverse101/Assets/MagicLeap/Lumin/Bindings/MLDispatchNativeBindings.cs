// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLDispatchNativeBindings.cs" company="Magic Leap, Inc">
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
    /// This interface is to let an application query the platform to handle things
    /// that the app itself cannot or wants someone else to handle.
    /// For example, if an application comes across a schema tag that it doesn't know
    /// what to do with, it can query the platform to see if some other application might.
    /// This can be useful for handling http://, https:// or other HTML Entity Codes.
    /// Apart from handling schema tags in URIs, this interface can also be used
    /// to query the platform to handle a type of file based on file-extension or mime-type.
    /// </summary>
    public sealed partial class MLDispatch
    {
        /// <summary>
        /// See ml_dispatch.h for additional comments.
        /// </summary>
        private class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// MLDispatch library name.
            /// </summary>
            private const string MLDispatchDLL = "ml_dispatch";

            /// <summary>
            /// Prevents a default instance of the <see cref="NativeBindings"/> class from being created.
            /// </summary>
            private NativeBindings()
            {
            }

            /// <summary>
            /// OnReplyComplete delegate.
            /// </summary>
            /// <param name="response">Pointer to the MLDispatch <c>OAuth</c> response.</param>
            public delegate void OAuthSchemaHandler(ref OAuthResponseNative response);

            /// <summary>
            /// Create empty dispatch packet.
            /// </summary>
            /// <param name="outPacket">A pointer to MLDispatchPacket structure on success and null on failure.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate dispatch packet.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if dispatch packet was allocated successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLDispatchDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLDispatchAllocateEmptyPacket(ref IntPtr outPacket);

            /// <summary>
            /// Release the MLDispatchPacket that is allocated by MLDispatchAllocateEmptyPacket and all its resources.
            /// The pointer to the MLDispatchPacket struct will point to NULL after this call.
            /// </summary>
            /// <param name="packet">Pointer to MLDispatchPacket struct.</param>
            /// <param name="releaseMembers">If true, function will attempt to release/free MLFileInfo array and uri members from the MLDispatchPacket.</param>
            /// <param name="closeFDs">If true, function will attempt to close the file descriptors in MLFileInfo. If false, caller will have to close file descriptors.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLDispatchDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLDispatchReleasePacket(ref IntPtr packet, bool releaseMembers, bool closeFDs);

            /// <summary>
            /// Allocate and assign URI in the MLDispatchPacket.
            /// The caller can release/free by calling MLDispatchReleaseUri().
            /// </summary>
            /// <param name="packet">Pointer to MLDispatchPacket whose uri member will be allocated and populated.</param>
            /// <param name="uri">Value assigned to MLDispatchPacket's uri member.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate uri.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLDispatchDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLDispatchSetUri(IntPtr packet, [MarshalAsAttribute(UnmanagedType.LPStr)] string uri);

            /// <summary>
            /// Release uri that is allocated by MLDispatchSetUri().
            /// The caller will have to call MLDispatchSetUri() before calling this function.
            /// The char pointer uri in MLDispatchPacket will point to NULL after this call.
            /// </summary>
            /// <param name="packet">Pointer to MLDispatchPacket struct.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLDispatchDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLDispatchReleaseUri(IntPtr packet);

            /// <summary>
            /// Try to open the application that supports a given mime type or schema type.
            /// If the caller does not specify a mime-type or schema type in the dispatch packet,
            /// dispatch service will try to open an application which supports the file extension
            /// specified in the file name.
            /// </summary>
            /// <param name="packet">Pointer to MLDispatchPacket struct.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// MLResult.Result will be <c>MLResult.Code.Dispatch*</c> if a dispatch specific error occurred.
            /// </returns>
            [DllImport(MLDispatchDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLDispatchTryOpenApplication(IntPtr packet);

            /// <summary>
            /// Returns an ASCII string for MLDispatchResult and MLResultGlobal codes.
            /// </summary>
            /// <param name="result">The input MLResult.Code enum from MLDispatch functions.</param>
            /// <returns>ASCII string containing readable version of result code.</returns>
            [DllImport(MLDispatchDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MLDispatchGetResultString(MLResult.Code result);

            /// <summary>
            /// Register a unique schema for <c>OAuth</c> redirect handler.
            /// The caller needs to ensure that the schema is unique. If the schema
            /// is already registered the function will return an error. The handler
            /// will be called once the authorization procedure has been completed.
            /// The caller should register two schema callbacks. The first will be for
            /// authorization redirect and the second schema will be in case the user cancels
            /// or an error happens in the authentication procedure.
            /// </summary>
            /// <param name="schema">A unique string that will match the redirect uri schema.</param>
            /// <param name="callbacks">Pointer to an <c>MLDispatchOAuthCallbacksNative</c> structure.</param>
            /// <param name="context">Pointer to the context that the application wants a reference to during handler callback.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the new schema has been registered correctly.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if SecureBrowserWindow privilege is denied.
            /// MLResult.Result will be <c>MLResult.Code.SchemaAlreadyRegistered</c> if the schema already is registered.
            /// MLResult.Result will be <c>MLResult.Code.Dispatch*</c> if a dispatch specific error occurred.
            /// </returns>
            [DllImport(MLDispatchDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLDispatchOAuthRegisterSchemaEx([MarshalAsAttribute(UnmanagedType.LPStr)] string schema, ref OAuthCallbacksNative callbacks, IntPtr context);

            /// <summary>
            /// Unregister a unique schema for <c>OAuth</c> redirect handler.
            /// </summary>
            /// <param name="schema">A unique string that will match the redirect uri schema.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the new schema has been registered correctly.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if SecureBrowserWindow privilege is denied.
            /// MLResult.Result will be <c>MLResult.Code.Dispatch*</c> if a dispatch specific error occurred.
            /// </returns>
            [DllImport(MLDispatchDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLDispatchOAuthUnregisterSchema([MarshalAsAttribute(UnmanagedType.LPStr)] string schema);

            /// <summary>
            /// Open a secure browser window to perform an <c>oauth</c> authentication.
            /// Will open a special browser window that will be lazy head locked to
            /// the user's head movement. The browser window will close once the
            /// authentication procedure has been completed.
            /// </summary>
            /// <param name="request">Pointer to an <c>OAuthOpenWindowRequest</c> structure.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the browser <c>oauth</c> window opened correctly.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if SecureBrowserWindow privilege is denied.
            /// MLResult.Result will be <c>MLResult.Code.Dispatch*</c> if a dispatch specific error occurred.
            /// </returns>
            [DllImport(MLDispatchDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLDispatchOAuthOpenWindowEx(ref OAuthOpenWindowRequest request);

            /// <summary>
            /// MLDispatch <c>OAuth</c> response. This response is provided via the <c>MLDispatchOAuthCallbacks</c> when the reply from the <c>OAuth</c> authorization step has been completed.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct OAuthResponseNative
            {
                /// <summary>
                /// Pointer to the context passed during call to <c>MLDispatchOAuthRegisterSchemaEx</c>.
                /// </summary>
                public IntPtr Context;

                /// <summary>
                /// The response url from authorization service.
                /// </summary>
                [MarshalAsAttribute(UnmanagedType.LPStr)]
                public string Response;
            }

            /// <summary>
            /// MLDispatch <c>OAuth</c> callback functions. This structure must be initialized by calling Create() before use.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct OAuthCallbacksNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Invoked when the reply from <c>oauth</c> authorization step has been completed.
                /// </summary>
                public OAuthSchemaHandler OnReplyComplete;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>
                /// Initialized version of this struct.
                /// </returns>
                public static OAuthCallbacksNative Create()
                {
                    return new OAuthCallbacksNative()
                    {
                        Version = 1u,
                        OnReplyComplete = null
                    };
                }
            }

            /// <summary>
            /// The <c>OAuth</c>  window open request parameters.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct OAuthOpenWindowRequest
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// The <c>oauth</c> url to navigate to.
                /// </summary>
                [MarshalAsAttribute(UnmanagedType.LPStr)]
                public string Url;

                /// <summary>
                /// The cancel uri called by the browser when users cancels the window.
                /// </summary>
                [MarshalAsAttribute(UnmanagedType.LPStr)]
                public string CancelUri;

                /// <summary>
                /// <para>Flag to set if http requests are not allowed. Default: false.</para>
                /// <para>If this is set to true and url points to http, the request will fail
                /// and the cancel uri callback will return the failure response.</para>
                /// </summary>
                public bool DisallowHttp;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>
                /// Initialized version of this struct.
                /// </returns>
                public static OAuthOpenWindowRequest Create()
                {
                    return new OAuthOpenWindowRequest()
                    {
                        Version = 1u,
                        Url = null,
                        CancelUri = null,
                        DisallowHttp = false
                    };
                }
            }
        }
    }
}

#endif
