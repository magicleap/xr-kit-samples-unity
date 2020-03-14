// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLTokenAgentClientCredentials.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// A class for retrieving client credentials.
    /// </summary>
    public sealed partial class MLTokenAgent
    {
        /// <summary>
        /// Public representation of MLTokenAgentClientCredentials.
        /// MLTokenAgentClientCredentials represents the credentials and tokens of the User-Audience pair
        /// that is associated with the calling service.
        /// </summary>
        [Serializable]
        public partial class ClientCredentials
        {
            /// <summary>
            /// Structure containing the credentials can be used to for a user to access a particular service.
            /// </summary>
            private Credentials credentials;

            /// <summary>
            /// Structure containing the tokens that are used to read and modify the user profile.
            /// </summary>
            private Tokens tokens;

            /// <summary>
            /// The current request for this clientCredentials object.
            /// </summary>
            private Request request;

            /// <summary>
            /// Gets the client credentials.
            /// </summary>
            public Credentials Credentials
            {
                get
                {
                    return this.credentials;
                }
            }

            /// <summary>
            /// Gets the tokens for the client credentials.
            /// </summary>
            public Tokens Tokens
            {
                get
                {
                    return this.tokens;
                }
            }

            /// <summary>
            /// Gets the current request attached to this clientCredentials object.
            /// A clientCredentials object may only handle one request at a time.
            /// </summary>
            public Request CurrentRequest
            {
                get
                {
                    return this.request;
                }

                private set
                {
                    if (this.request == null)
                    {
                        this.request = value;
                    }
                    else
                    {
                        MLPluginLog.Warning("A request is active and currently fetching credentials.");
                    }
                }
            }

            /// <summary>
            /// Fetches client credentials, can be used with a callback or as a blocking call.
            /// </summary>
            /// <param name="callback">The callback to notify when the CurrentRequest is complete.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the out_credentials was 0 (null).
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if the caller does not have the ClientCredentialsRead privilege.
            /// MLResult.Result will be <c>MLResult.Code.TokenAgent*</c> if a token specific failure occurred during the operation.
            /// </returns>
            public MLResult Fetch(MLTokenAgent.ClientCredentials.Request.RequestAttibutesDelegate callback = null)
            {
                if (callback == null)
                {
                    MLResult.Code resultCode = MLTokenAgent.NativeBindings.GetClientCredentials(this, ref this.credentials, ref this.tokens);
                    if (!MLResult.IsOK(resultCode))
                    {
                        MLPluginLog.ErrorFormat("MLTokenAgent.Fetch failed. Reason: {0}", MLResult.CodeToString(resultCode));
                    }

                    return MLResult.Create(resultCode);
                }
                else
                {
                    this.CurrentRequest = new Request
                    {
                        Callback = callback,
                        ResultCode = MLResult.Code.Pending,
                        RequestState = Request.State.REQUEST_CREDENTIALS
                    };

                    MLTokenAgent.AddClientCredentials(this);

                    return MLResult.Create(MLResult.Code.Ok);
                }
            }

            /// <summary>
            /// Handles when a request queries client credentials.
            /// </summary>
            public void ProcessRequest()
            {
                if (this.request == null)
                {
                    return;
                }

                switch (this.request.RequestState)
                {
                    case Request.State.REQUEST_CREDENTIALS:
                        this.RequestClientCredentialsAsync();
                        break;

                    case Request.State.LISTENING_CREDENTIALS:
                        this.ListenClientCredentialsResponse();
                        break;

                    case Request.State.DONE:
                        this.request?.Callback?.Invoke(MLResult.Create(this.request.ResultCode));

                        // Removes the clientCredentials object if something when wrong with the request.
                        if (!MLResult.IsOK(this.request.ResultCode))
                        {
                            this.request = null;
                            MLTokenAgent.RemoveClientCredentials(this);
                        }

                        this.request = null;
                        break;

                    default:
                        break;
                }
            }

            /// <summary>
            /// Queries for the client credentials of this user.
            /// </summary>
            private void RequestClientCredentialsAsync()
            {
                MLResult.Code resultCode = MLTokenAgent.NativeBindings.RequestClientCredentialsAsync(this);

                if (MLResult.IsOK(resultCode))
                {
                    this.request.ResultCode = MLResult.Code.Pending;
                    this.request.RequestState = MLTokenAgent.ClientCredentials.Request.State.LISTENING_CREDENTIALS;
                }
                else
                {
                    this.request.ResultCode = resultCode;
                    this.request.RequestState = MLTokenAgent.ClientCredentials.Request.State.DONE;
                }
            }

            /// <summary>
            /// Listens for client credentials previously requested by RequestClientCredentialsAsync.
            /// </summary>
            private void ListenClientCredentialsResponse()
            {
                MLResult.Code resultCode = MLTokenAgent.NativeBindings.ListenClientCredentialsResponse(this, ref this.credentials, ref this.tokens);

                if (MLResult.IsOK(resultCode))
                {
                    this.request.RequestState = Request.State.DONE;
                }
                else if (!MLResult.IsPending(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLIdentity.clientCredentials.ListenAttributeNamesResponse failed to retrieve attribute names. Reason: {0}", resultCode);
                    this.request.RequestState = Request.State.DONE;
                }

                this.request.ResultCode = resultCode;
            }
        }
    }
}
#endif
