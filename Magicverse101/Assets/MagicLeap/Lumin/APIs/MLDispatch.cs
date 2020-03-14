// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLDispatch.cs" company="Magic Leap, Inc">
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

    using AOT;
    using UnityEngine.XR.MagicLeap.Native;

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
        /// This string will be thrown in case of the library containing this API's symbols not being found.
        /// </summary>
        private static readonly string DllNotFoundExceptionError = "MLDispatch API is currently available only on device.";

        /// <summary>
        /// Internal dictionary to control <c>OAuth</c> callbacks.
        /// </summary>
        private static Dictionary<int, OAuthPair> oAuthCallbacks = new Dictionary<int, OAuthPair>();

        /// <summary>
        /// Internal dictionary key for <c>OAuth</c>.
        /// </summary>
        private static int uniqueID = 0;

        /// <summary>
        /// Delegate for <c>OAuth</c> response callbacks.
        /// </summary>
        /// <param name="response">The response url from authorization service.</param>
        /// <param name="schema">The schema this response is for.</param>
        public delegate void OAuthHandler(string response, string schema);

        /// <summary>
        /// Try to open the application that is registered to support the given URI.
        /// </summary>
        /// <param name="uri"> The URI to pass into the dispatch service.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.Dispatch*</c> if a dispatch specific error occurred.
        /// </returns>
        public static MLResult TryOpenAppropriateApplication(string uri)
        {
            try
            {
                IntPtr packet = default;

                MLResult.Code resultCode = NativeBindings.MLDispatchAllocateEmptyPacket(ref packet);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLDispatch.TryOpenAppropriateApplication failed to allocate an empty packet. Reason: {0}", result);
                    return result;
                }

                resultCode = NativeBindings.MLDispatchSetUri(packet, uri);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLDispatch.TryOpenAppropriateApplication failed to set URI. Reason: {0}", result);
                    return result;
                }

                resultCode = NativeBindings.MLDispatchTryOpenApplication(packet);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLDispatch.TryOpenAppropriateApplication failed to open application. Reason: {0}", result);
                    return result;
                }

                resultCode = NativeBindings.MLDispatchReleasePacket(ref packet, true, false);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLDispatch.TryOpenAppropriateApplication failed to release the packet. Reason: {0}", result);
                }

                return MLResult.Create(MLResult.Code.Ok);
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(DllNotFoundExceptionError);
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, DllNotFoundExceptionError);
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLDispatch API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLDispatch API symbols not found");
            }
        }

        /// <summary>
        /// Register a unique schema for <c>OAuth</c> redirect handler. The caller needs to ensure that the schema is unique.
        /// If the schema is already registered the function will return an error. The handler
        /// will be called once the authorization procedure has been completed.
        /// The caller should register two schema callbacks. The first will be for
        /// authorization redirect and the second schema will in case the user cancels
        /// the authentication.
        /// </summary>
        /// <param name="schema">A unique string that will match the redirect uri schema</param>
        /// <param name="callback">MLDispatch <c>OAuth</c> callback function</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the new schema has been registered correctly.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if SecureBrowserWindow privilege is denied.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// MLResult.Result will be <c>MLResult.Code.SchemaAlreadyRegistered</c> if the schema already is registered.
        /// MLResult.Result will be <c>MLResult.Code.Dispatch*</c> if a dispatch specific error occurred.
        /// </returns>
        public static MLResult OAuthRegisterSchema(string schema, ref OAuthHandler callback)
        {
            try
            {
                NativeBindings.OAuthCallbacksNative newSchema = NativeBindings.OAuthCallbacksNative.Create();
                newSchema.OnReplyComplete = OAuthOnReplyNative;

                int newID = uniqueID + 1;

                MLResult.Code resultCode = NativeBindings.MLDispatchOAuthRegisterSchemaEx(schema, ref newSchema, new IntPtr(newID));

                if (MLResult.IsOK(resultCode))
                {
                    OAuthPair newEntry = new OAuthPair(schema, callback);
                    oAuthCallbacks.Add(newID, newEntry);
                    uniqueID = newID;
                }

                return MLResult.Create(resultCode);
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(DllNotFoundExceptionError);
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, DllNotFoundExceptionError);
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLDispatch API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLDispatch API symbols not found");
            }
        }

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
        public static MLResult OAuthUnRegisterSchema(string schema)
        {
            try
            {
                MLResult.Code resultCode = NativeBindings.MLDispatchOAuthUnregisterSchema(schema);

                if (MLResult.IsOK(resultCode))
                {
                    oAuthCallbacks.Remove(schema.GetHashCode());
                }

                return MLResult.Create(resultCode);
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(DllNotFoundExceptionError);
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, DllNotFoundExceptionError);
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLDispatch API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLDispatch API symbols not found");
            }
        }

        /// <summary>
        /// Open a secure browser window to perform an <c>oauth</c> authentication.
        /// Will open a special browser window that will be <c>laz</c> head locked to
        /// the user's head movement. The browser window will close once the
        /// authentication procedure has been completed.
        /// </summary>
        /// <param name="url">The <c>oauth</c> url to navigate to.</param>
        /// <param name="cancelUri">The cancel uri called by the browser when users cancels the window.</param>
        /// <param name="disallowHttp">Flag to set if http requests are not allowed. Default: false. If this is set to true and url points to http,
        /// the request will fail and the cancel uri callback will return the failure response.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the browser <c>oauth</c> window opened correctly.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied*</c> if SecureBrowserWindow privilege is denied.
        /// MLResult.Result will be <c>MLResult.Code.Dispatch*</c> if a dispatch specific error occurred.
        /// </returns>
        public static MLResult OAuthOpenWindow(string url, string cancelUri, bool disallowHttp = false)
        {
            try
            {
                NativeBindings.OAuthOpenWindowRequest newRequest = NativeBindings.OAuthOpenWindowRequest.Create();
                newRequest.Url = url;
                newRequest.CancelUri = cancelUri;
                newRequest.DisallowHttp = disallowHttp;

                MLResult.Code resultCode = NativeBindings.MLDispatchOAuthOpenWindowEx(ref newRequest);

                return MLResult.Create(resultCode);
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(DllNotFoundExceptionError);
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, DllNotFoundExceptionError);
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLDispatch API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLDispatch API symbols not found");
            }
        }

        /// <summary>
        /// Gets the result string for a MLResult.Code.
        /// </summary>
        /// <param name="result">The MLResult.Code to be requested.</param>
        /// <returns>A pointer to the result string.</returns>
        internal static IntPtr GetResultString(MLResult.Code result)
        {
            try
            {
                return NativeBindings.MLDispatchGetResultString(result);
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLDispatch.GetResultString failed. Reason: API symbols not found");
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Native <c>OAuth</c> callback.
        /// </summary>
        /// <param name="response">Pointer to the MLDispatch <c>OAuth</c> response.</param>
        [MonoPInvokeCallback(typeof(NativeBindings.OAuthSchemaHandler))]
        private static void OAuthOnReplyNative(ref NativeBindings.OAuthResponseNative response)
        {
            NativeBindings.OAuthResponseNative newResponse = response;

            int contextHash = newResponse.Context.ToInt32();

            if (oAuthCallbacks.TryGetValue(contextHash, out OAuthPair currentPair))
            {
                MLThreadDispatch.Call(response.Response, currentPair.Schema, currentPair.Callback);
                return;
            }

            MLPluginLog.ErrorFormat("MLDispatch OAuth callback received with no context");
        }

        /// <summary>
        /// Struct to connect a Schema to a Callback.
        /// </summary>
        private struct OAuthPair
        {
            /// <summary>
            /// Schema to pair with a Callback.
            /// </summary>
            public string Schema;

            /// <summary>
            /// Callback to pair with a Schema.
            /// </summary>
            public OAuthHandler Callback;

            /// <summary>
            /// Initializes a new instance of the <see cref="OAuthPair" /> struct.
            /// </summary>
            /// /// <param name="schema">Schema to pair with a Callback.</param>
            /// <param name="callback">Callback to pair with a Schema.</param>
            public OAuthPair(string schema, OAuthHandler callback)
            {
                this.Schema = schema;
                this.Callback = callback;
            }
        }
    }
}

#endif
