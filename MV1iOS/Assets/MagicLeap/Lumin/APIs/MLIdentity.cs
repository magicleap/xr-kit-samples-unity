// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLIdentity.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;

    /// <summary>
    /// Functionality to query for user profiles.
    /// </summary>
    public partial class MLIdentity : MLAPISingleton<MLIdentity>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// List of profiles that have been fetched.
        /// </summary>
        private List<Profile> profiles = new List<Profile>();

        /// <summary>
        /// Gets a copy of the list of fetched profiles.
        /// </summary>
        public List<Profile> Profiles
        {
            get
            {
                return new List<Profile>(this.profiles);
            }
        }

        /// <summary>
        /// Initializes an instance of MLIdentity.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to an internal error.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLIdentity.BaseStart();
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
                if (MLIdentity.IsValidInstance())
                {
                    try
                    {
                        return NativeBindings.MLIdentityGetResultString(result);
                    }
                    catch (EntryPointNotFoundException)
                    {
                        MLPluginLog.Error("MLIdentity.GetResultString failed. Reason: API symbols not found.");
                    }

                    return IntPtr.Zero;
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLIdentity.GetResultString failed. Reason: No Instance for MLIdentity.");
                }
            }
            catch (EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLIdentity.GetResultString failed. Reason: API symbols not found.");
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
                foreach (Profile profile in this.profiles)
                {
                    MLIdentity.NativeBindings.CleanupProfileMemory(profile);
                }
            }
        }

        /// <summary>
        /// Processes any available profile attribute requests.
        /// </summary>
        protected override void Update()
        {
            for(int i = 0; i < this.profiles.Count; i++)
            {
                Profile profile = this.profiles[i];
                if (profile.CurrentRequest != null)
                {
                    profile.ProcessRequest();
                }
            }
        }

        /// <summary>
        /// Creates an instance of MLIdentity if there is no valid instance.
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLIdentity.IsValidInstance())
            {
                MLIdentity._instance = new MLIdentity();
            }
        }

        /// <summary>
        /// Adds a profile to the cached list of profiles that have their requests processed every frame.
        /// </summary>
        /// <param name="profile">The profile to add.</param>
        private static void AddProfile(Profile profile)
        {
            if (MLIdentity.IsValidInstance())
            {
                if (!_instance.profiles.Contains(profile))
                {
                    _instance.profiles.Add(profile);
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLIdentity.AddProfile failed. Reason: No Instance for MLIdentity.");
            }
        }

        /// <summary>
        /// Removes a profile to the cached list of profiles that have their requests processed every frame.
        /// </summary>
        /// <param name="profile">The profile to remove.</param>
        private static void RemoveProfile(Profile profile)
        {
            if (MLIdentity.IsValidInstance())
            {
                _instance.profiles.Remove(profile);
                MLIdentity.NativeBindings.CleanupProfileMemory(profile);
            }
            else
            {
                MLPluginLog.ErrorFormat("MLIdentity.RemoveProfile failed. Reason: No Instance for MLIdentity.");
            }
        }
        #endif
    }
}
