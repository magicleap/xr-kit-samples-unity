// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLAPISingleton.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Internal;
    #endif

    /// <summary>
    /// MLAPISingleton class contains a template for singleton APIs
    /// </summary>
    /// <typeparam name="T">The type of the class to create a singleton for.</typeparam>
    public abstract class MLAPISingleton<T> where T : MLAPISingleton<T>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// Stores the number of "instances" for an API.
        /// </summary>
        private static uint startCount = 0;

        /// <summary>
        /// Handle to the perception system.
        /// </summary>
        private PerceptionHandle perceptionHandle;

        /// <summary>
        /// Stores if the perception layer has been started.
        /// </summary>
        private bool perceptionHasStarted = false;

        /// <summary>
        /// Finalizes an instance of the <see cref="MLAPISingleton{T}"/> class.
        /// </summary>
        ~MLAPISingleton()
        {
            // Clean only if instance matches the instance the finalizer is getting called from.
            if (this == _instance)
            {
                MLPluginLog.ErrorFormat("MLAPISingleton.Finalizer failed, there was a Start() and Stop() call mismatch in {0}, Stop() was called {1} times less.", typeof(T).Name, startCount);
                this.CleanupAPI(false);

                if (this.perceptionHasStarted)
                {
                    this.perceptionHandle.Dispose();
                    this.perceptionHasStarted = false;
                }

                startCount = 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the API was started successfully.
        /// </summary>
        /// <returns> Returns false if no instance exists</returns>
        public static bool IsStarted
        {
            get
            {
                return IsValidInstance();
            }
        }

        /// <summary>
        /// Gets the singleton object instance.
        /// </summary>
        /// <returns>The singleton object instance.</returns>
        protected static T Instance
        {
            get
            {
                CheckValidInstance();
                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets the singleton object instance.
        /// </summary>
        protected static T _instance { get; set; } = null;

        /// <summary>
        /// Gets or sets the error message displayed to user when API library was not found
        /// </summary>
        protected string DllNotFoundError { get; set; } = "MLAPISingleton.BaseStart failed, {0} is only available on device or when running inside the Unity editor with Zero iteration enabled.";

        /// <summary>
        /// Stop the API
        /// </summary>
        public static void Stop()
        {
            if (IsValidInstance())
            {
                Instance.StopAPI();
            }
        }

        /// <summary>
        /// Utility function returns true is the instance exists.
        /// </summary>
        /// <returns>If the instance is valid.</returns>
        protected static bool IsValidInstance()
        {
            return _instance != null;
        }

        /// <summary>
        /// Utility function checks for valid instance and throws an exception if not found
        /// </summary>
        /// <throw> InvalidInstanceException </throw>
        protected static void CheckValidInstance()
        {
            if (!IsValidInstance())
            {
                throw new InvalidInstanceException(string.Format("No Instance for: {0}", typeof(T).Name));
            }
        }

        /// <summary>
        /// Utility function set up instance and tracks successful _startCount
        /// </summary>
        /// <param name="requiresXRLoader">Flag to determine if this API requires the XR Loader being initialized.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error (MagicLeap XR Loader not loaded, no device, DLL not found, no API symbols).
        /// MLResult.Result will otherwise be return value specific API's StartAPI function.
        /// </returns>
        protected static MLResult BaseStart(bool requiresXRLoader = false)
        {
            MLResult result;
            try
            {
                // Check to see if we have already successfully initialized a valid instance
                if (startCount > 0)
                {
                    startCount++;
                    result = MLResult.Create(MLResult.Code.Ok);
                }
                else
                {
                    if (requiresXRLoader && !MLDevice.IsReady())
                    {
                        MLPluginLog.ErrorFormat("MLAPISingleton.BaseStart failed to start {0} API. Reason: MagicLeap XR Loader is not initialized. Please wait to start API until Monobehavior.Start and if issue persists make sure ProjectSettings/XR/Initialize On Startup is enabled.", typeof(T).Name);
                        return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MagicLeap XR Loader not initialized");
                    }

                    result = Instance.StartAPI();
                    if (result.IsOk)
                    {
                        // Everything started correctly register the update and increament _startCount
                        MLDevice.Register(Instance.Update);
                        MLDevice.RegisterOnApplicationPause(Instance.OnApplicationPause);
                        startCount++;

                        Instance.perceptionHandle = PerceptionHandle.Acquire();
                        Instance.perceptionHasStarted = true;
                    }
                    else
                    {
                        MLPluginLog.ErrorFormat("MLAPISingleton.BaseStart failed to start {0} API. Reason: {1}", typeof(T).Name, result);
                        _instance = null;
                    }
                }

                return result;
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.ErrorFormat(_instance.DllNotFoundError, typeof(T).Name);
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Dll not found");
                MLPluginLog.ErrorFormat("MLAPISingleton.BaseStart failed to start {0} API. Reason: {1}", typeof(T).Name, result);
                _instance = null;
            }
            catch (System.EntryPointNotFoundException)
            {
                string errorMessage = string.Format("{0} API symbols not found", typeof(T).Name);
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, errorMessage);
                MLPluginLog.ErrorFormat("MLAPISingleton.BaseStart failed to start {0} API. Reason: {1}", typeof(T).Name, result);
                _instance = null;
            }

            return result;
        }

        #if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        ///  Do API specific creation/initialization of ML resources for this API
        /// such as create trackers.
        /// </summary>
        /// <returns>MLResult from the API start.</returns>
        protected abstract MLResult StartAPI();
        #endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Cleans up unmanaged memory
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Boolean that tells if it is safe to clear managed memory</param>
        protected abstract void CleanupAPI(bool isSafeToAccessManagedObjects);

        /// <summary>
        /// Update function registered with MLDevice to do API specific per frame work.
        /// </summary>
        protected abstract void Update();

        /// <summary>
        /// Callback sent to all MagicLeap APIs on application pause.
        /// </summary>
        /// <param name="pauseStatus">True if the application is paused, else False. </param>
        protected virtual void OnApplicationPause(bool pauseStatus)
        {
        }

        /// <summary>
        /// Decrement API instance count and cleanup if this is the last one.
        /// </summary>
        private void StopAPI()
        {
            if (startCount > 0)
            {
                --startCount;
                if (startCount == 0)
                {
                    MLDevice.Unregister(this.Update);
                    MLDevice.UnregisterOnApplicationPause(this.OnApplicationPause);
                    this.CleanupAPI(true);

                    if (this.perceptionHasStarted)
                    {
                        this.perceptionHandle.Dispose();
                        this.perceptionHasStarted = false;
                    }

                    _instance = null;
                }
            }
        }
        #endif
    }
}
