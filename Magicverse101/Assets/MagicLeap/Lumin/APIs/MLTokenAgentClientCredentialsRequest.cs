// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLTokenAgentClientCredentialsRequest.cs" company="Magic Leap, Inc">
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
    /// <summary>
    /// A class for retrieving client credentials.
    /// </summary>
    public sealed partial class MLTokenAgent
    {
        /// <summary>
        /// Represents client credentials.
        /// </summary>
        public partial class ClientCredentials
        {
            /// <summary>
            /// Represents a request for ClientCredentials properties.
            /// </summary>
            public class Request
            {
                #if PLATFORM_LUMIN
                /// <summary>
                /// Gets or sets the delegate for the callback to notify when the request has finished.
                /// </summary>
                /// <param name="result">The MLResult of the request.</param>
                public delegate void RequestAttibutesDelegate(MLResult result);
                #endif

                /// <summary>
                /// The different states a request can be in.
                /// </summary>
                public enum State
                {
                    /// <summary>
                    /// The request is currently querying for credentials.
                    /// </summary>
                    REQUEST_CREDENTIALS,

                    /// <summary>
                    /// The request is currently listening for credentials.
                    /// </summary>
                    LISTENING_CREDENTIALS,

                    /// <summary>
                    /// The request has finished.
                    /// </summary>
                    DONE
                }

                #if PLATFORM_LUMIN
                /// <summary>
                /// Gets or sets the current state of the request.
                /// </summary>
                public State RequestState { get; set; }

                /// <summary>
                /// Gets or sets the callback to notify when the request has finished.
                /// </summary>
                public RequestAttibutesDelegate Callback { get; set; }

                /// <summary>
                /// Gets or sets the current result code of this request.
                /// </summary>
                public MLResult.Code ResultCode { get; set; }
                #endif
            }
        }
    }
}
#endif
