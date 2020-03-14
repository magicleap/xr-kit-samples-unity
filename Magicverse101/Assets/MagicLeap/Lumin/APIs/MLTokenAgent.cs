// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLTokenAgent.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// A class for retrieving the client credentials.
    /// </summary>
    public sealed partial class MLTokenAgent : MLAPISingleton<MLTokenAgent>
    {
        /// <summary>
        /// Only available on device error message.
        /// </summary>
        private const string DLLNotFoundError = "MLTokenAgent API is currently available only on device.";

        /// <summary>
        /// List of profiles that have been fetched
        /// </summary>
        private List<ClientCredentials> clientsCredentials = new List<ClientCredentials>();

        /// <summary>
        /// Gets a copy of the list of fetched profiles.
        /// </summary>
        public List<ClientCredentials> ClientsCredentials
        {
            get
            {
                return new List<ClientCredentials>(this.clientsCredentials);
            }
        }

        /// <summary>
        /// GetClientCredentialsAsync invokes the GetClientCredentials
        /// function asynchronously (in a different thread).
        /// </summary>
        /// <param name="outFuture">
        /// A pointer to an MLInvokeFuture pointer which provides the means to poll for completion and
        /// to retrieve the credentials returned by MLTokenAgentGetClientCredentials.
        /// This pointer will be freed by the library before returning from the first (and last)
        /// call to MLTokenAgentGetClientCredentialsWait, after the asynchronous call completed, that is
        /// after MLTokenAgentGetClientCredentialsWait returns any value that is not MLResult.Pending.
        /// </param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.TokenAgent*</c> if a token specific failure occurred during the operation.
        /// </returns>
        public static MLResult GetClientCredentialsAsync(ref IntPtr outFuture)
        {
            try
            {
                MLResult.Code resultCode = MLTokenAgent.NativeBindings.MLTokenAgentGetClientCredentialsAsync(ref outFuture);
                return MLResult.Create(resultCode);
            }
            catch (DllNotFoundException)
            {
                MLPluginLog.Error(DLLNotFoundError);
                throw;
            }
            catch (EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLTokenAgent.GetClientCredentialsAsync failed. Reason: API symbols not found.");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLTokenAgent.GetClientCredentialsAsync failed. Reason: API symbols not found.");
            }
        }

        /// <summary>
        /// Gets the result string for a MLResult.Code. Use MLResult.CodeToString(MLResult.Code) to get the string value of any MLResult.Code.
        /// </summary>
        /// <param name="result">The MLResult.Code to be requested.</param>
        /// <returns>A pointer to the result string.</returns>
        internal static IntPtr GetResultString(MLResult.Code result)
        {
            try
            {
                if (MLTokenAgent.IsValidInstance())
                {
                    try
                    {
                        return NativeBindings.MLTokenAgentGetResultString(result);
                    }
                    catch (EntryPointNotFoundException)
                    {
                        MLPluginLog.Error("MLTokenAgent.GetResultString failed. Reason: API symbols not found.");
                    }

                    return IntPtr.Zero;
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLTokenAgent.GetResultString failed. Reason: No Instance for MLTokenAgent.");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLTokenAgent.GetResultString failed. Reason: API symbols not found");
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Returns <c>MLResult.Code.Ok</c> because no API is started.
        /// </summary>
        /// <returns><c>MLResult.Code.Ok</c> because no API is started. </returns>
        protected override MLResult StartAPI()
        {
            return MLResult.Create(MLResult.Code.Ok);
        }

        /// <summary>
        /// Cleans up unmanaged memory.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Allow complete cleanup of the API.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            if (isSafeToAccessManagedObjects)
            {
                foreach (ClientCredentials clientCredentials in this.clientsCredentials)
                {
                    MLTokenAgent.NativeBindings.CleanupClientCredentialsMemory(clientCredentials);
                }
            }
        }

        /// <summary>
        /// Processes any available profile attribute requests.
        /// </summary>
        protected override void Update()
        {
            for (int i = 0; i < this.clientsCredentials.Count; i++)
            {
                ClientCredentials clientCredentials = this.clientsCredentials[i];
                if (clientCredentials.CurrentRequest != null)
                {
                    clientCredentials.ProcessRequest();
                }
            }
        }

        /// <summary>
        /// Creates an instance of MLTokenAgent if there is no valid instance.
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLTokenAgent.IsValidInstance())
            {
                MLTokenAgent._instance = new MLTokenAgent();
            }
        }

        /// <summary>
        /// Adds a profile to the cached list of profiles that have their requests processed every frame.
        /// </summary>
        /// <param name="clientCredentials">The clientCredentials object to add.</param>
        private static void AddClientCredentials(ClientCredentials clientCredentials)
        {
            if (MLTokenAgent.IsValidInstance())
            {
                if (!_instance.clientsCredentials.Contains(clientCredentials))
                {
                    _instance.clientsCredentials.Add(clientCredentials);
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLTokenAgent.AddProfile failed. Reason: No Instance for MLTokenAgent.");
            }
        }

        /// <summary>
        /// Removes a profile to the cached list of profiles that have their requests processed every frame.
        /// </summary>
        /// <param name="clientCredentials">The clientCredentials object to remove.</param>
        private static void RemoveClientCredentials(ClientCredentials clientCredentials)
        {
            if (MLTokenAgent.IsValidInstance())
            {
                _instance.clientsCredentials.Remove(clientCredentials);
                MLTokenAgent.NativeBindings.CleanupClientCredentialsMemory(clientCredentials);
            }
            else
            {
                MLPluginLog.ErrorFormat("MLTokenAgent.RemoveProfile failed. Reason: No Instance for MLTokenAgent.");
            }
        }

        /// <summary>
        /// The credentials that can be used to for a user to access a particular service (Audience).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Credentials
        {
            /// <summary>
            /// Null terminated string allocated and released by the library.
            /// </summary>
            public string AccessKeyId;

            /// <summary>
            /// Null terminated string allocated and released by the library.
            /// </summary>
            public string SecretKey;

            /// <summary>
            /// Null terminated string allocated and released by the library.
            /// </summary>
            public string SessionToken;
        }

        /// <summary>
        /// MLTokenAgentTokens contains tokens that are used to read and modify the user profile.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Tokens
        {
            /// <summary>
            /// The idToken contains information from the user profile.
            /// It is a null terminated string that is allocated and released by the library.
            /// </summary>
            public string IdToken;

            /// <summary>
            /// The accessToken is the token that can be used to modify attributes of the user profile.
            /// It is a null terminated string that is allocated and released by the library.
            /// </summary>
            public string AccessToken;
        }
    }
}
#endif
