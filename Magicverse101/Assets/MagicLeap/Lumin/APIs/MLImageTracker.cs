// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLImageTracker.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// MLImageTracker manages the Image Tracker system.
    /// Image Tracker enables experiences that recognize 2D planar images
    /// (image targets) in the physical world. It provides the position and
    /// orientation of the image targets in the physical world.
    /// </summary>
    public sealed partial class MLImageTracker : MLAPISingleton<MLImageTracker>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// The internal MLHANDLE used to reference this tracker.
        /// </summary>
        private ulong handle = MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// The list of targets currently being tracked by this image tracker.
        /// </summary>
        private List<Target> targetList;

        /// <summary>
        /// The list of settings for this image tracker.
        /// </summary>
        private NativeBindings.MLImageTrackerSettingsNative trackerSettings;

        /// <summary>
        /// Prevents a default instance of the <see cref="MLImageTracker" /> class from being created.
        /// </summary>
        private MLImageTracker()
        {
            this.trackerSettings = NativeBindings.MLImageTrackerSettingsNative.Create();
            MLResult.Code result = NativeBindings.MLImageTrackerInitSettings(ref this.trackerSettings);
            if (result != MLResult.Code.Ok)
            {
                MLPluginLog.ErrorFormat("MLImageTracker.Constructor failed initializing the ImageTrackerSettings. Reason: {0}", result);
            }

            this.targetList = new List<Target>();
        }
        #endif

        /// <summary>
        /// Supported formats when adding Image Targets to the Image Tracker.
        /// </summary>
        public enum ImageFormat
        {
            /// <summary>
            /// Grayscale format.
            /// </summary>
            Grayscale,

            /// <summary>
            /// RGB format.
            /// </summary>
            RGB,

            /// <summary>
            /// RGBA format.
            /// </summary>
            RGBA
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Starts the image tracker with the defined settings.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if image tracker was not created due to lack of privilege(s).
        /// </returns>
        public static MLResult Start()
        {
            var settings = NativeBindings.MLImageTrackerSettingsNative.Create();
            MLResult.Code resultCode = NativeBindings.MLImageTrackerInitSettings(ref settings);
            MLResult result = MLResult.Create(resultCode);
            if (result.IsOk)
            {
                MLImageTracker.Settings defaultTrackerSettings = MLImageTracker.Settings.Create(settings.MaxSimultaneousTargets);
                return Start(defaultTrackerSettings);
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Starts the image tracker with the specified settings.
        /// </summary>
        /// <param name="customSettings">The settings to start the image tracker with.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if image tracker was not created due to lack of privilege(s).
        /// </returns>
        public static MLResult Start(MLImageTracker.Settings customSettings)
        {
            var settings = new NativeBindings.MLImageTrackerSettingsNative(customSettings);
            bool hasInstanceWithDifferentSettings = false;

            if ((MLImageTracker._instance != null) && (settings != MLImageTracker.Instance.trackerSettings))
            {
                MLPluginLog.Warning("MLImageTracker.Start, starting image tracking multiple times with different settings. New settings will be ignored.");
                hasInstanceWithDifferentSettings = true;
            }

            CreateInstance();

            if (!hasInstanceWithDifferentSettings)
            {
                MLImageTracker.Instance.trackerSettings = settings;
            }

            return MLImageTracker.BaseStart(true);
        }

        /// <summary>
        /// Gets the current MLImageTracker.Settings
        /// </summary>
        /// <returns> A copy of the current MLImageTracker.Settings object. </returns>
        public static MLImageTracker.Settings GetCurrentTrackerSettings()
        {
            if (MLImageTracker.IsValidInstance() && Instance.trackerSettings != null)
            {
                return MLImageTracker.Settings.Create(Instance.trackerSettings.MaxSimultaneousTargets);
            }
            else
            {
                MLPluginLog.ErrorFormat("MLImageTracker.Settings.GetCurrentTrackerSettings failed. Reason: No Instance for MLImageTracker");
                return MLImageTracker.Settings.Create(0);
            }
        }

        /// <summary>
        /// Sets the Image Tracker's settings to match the provided settings.
        /// </summary>
        /// <param name="customSettings">The new MLImageTracker.Settings wanted.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully updated the image tracker settings.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to update the settings due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if failed to update the settings due to lack of privilege(s).
        /// </returns>
        public static MLResult UpdateTrackerSettings(MLImageTracker.Settings customSettings)
        {
            var currentSettings = GetCurrentTrackerSettings();

            if (currentSettings == customSettings)
            {
                return MLResult.Create(MLResult.Code.Ok, "No change needed");
            }

            Instance.trackerSettings = new NativeBindings.MLImageTrackerSettingsNative(customSettings);

            MLResult.Code resultCode = NativeBindings.MLImageTrackerUpdateSettings(Instance.handle, ref Instance.trackerSettings);
            MLResult result = MLResult.Create(resultCode);
            if (!result.IsOk)
            {
                Instance.trackerSettings = new NativeBindings.MLImageTrackerSettingsNative(currentSettings);
            }

            return result;
        }

        /// <summary>
        /// Gets the enabled status of the Image Tracker
        /// </summary>
        /// <returns>True if currently enabled, false if disabled.</returns>
        public static bool GetTrackerStatus()
        {
            return Instance.trackerSettings.EnableTracker;
        }

        /// <summary>
        /// Enables the Image Tracker
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        public static MLResult Enable()
        {
            return Instance.SetTrackerStatusInternal(true);
        }

        /// <summary>
        /// Disable the Image Tracker
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        public static MLResult Disable()
        {
            return Instance.SetTrackerStatusInternal(false);
        }

        /// <summary>
        /// Adds an Image Target to the image tracker system.
        /// </summary>
        /// <param name="name">The unique name of this target.</param>
        /// <param name="image">
        /// Texture2D representing the Image Target.
        /// The aspect ration of the target should not be changed. Set the "Non Power of 2" property of Texture2D to none.
        /// </param>
        /// <param name="longerDimension">Size of the longer dimension in scene units.</param>
        /// <param name="callback">The function that will be triggered with target info.</param>
        /// <param name="isStationary">
        /// Set this to true if the position of this Image Target in the physical world is fixed and the local
        /// geometry is planar.
        /// </param>
        /// <returns>MLImageTracker.Target if the target was created and added successfully, null otherwise.</returns>
        public static Target AddTarget(string name, Texture2D image, float longerDimension, MLImageTracker.Target.OnImageResultDelegate callback, bool isStationary = false)
        {
            return Instance.AddTargetInternal(name, image, longerDimension, callback, isStationary);
        }

        /// <summary>
        /// External call used to remove a target whose name matches the passed in name.
        /// </summary>
        /// <param name="name">The name of the target to remove.</param>
        /// <returns>True if the target was found and removed, false otherwise.</returns>
        public static bool RemoveTarget(string name)
        {
            for (int i = 0; i < Instance.targetList.Count; i++)
            {
                if (Instance.targetList[i].TargetSettings.Name.Equals(name))
                {
                    Instance.targetList[i].Dispose();
                    Instance.targetList.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        #if !DOXYGENSHOULDSKIPTHIS
        /// <summary>
        /// Initializes the image tracker.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        protected override MLResult StartAPI()
        {
            this.handle = MagicLeapNativeBindings.InvalidHandle;
            MLResult.Code resultCode = NativeBindings.MLImageTrackerCreate(ref this.trackerSettings, ref this.handle);
            MLResult result = MLResult.Create(resultCode);
            if (result.IsOk)
            {
                bool success = MagicLeapNativeBindings.MLHandleIsValid(this.handle);
                if (!success)
                {
                    MLPluginLog.Error("MLImageTracker.StartApi failed. Reason: Invalid image tracker this.handle.");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLImageTracker.StartApi failed to create image tracker this.handle. Reason: {0}", result);
            }

            return result;
        }
        #endif // DOXYGENSHOULDSKIPTHIS

        /// <summary>
        /// Cleanup the image targets first (if it safe) and the cleanup the
        /// image tracker.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Determines if the target's should be disposed of or not.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            if (isSafeToAccessManagedObjects)
            {
                if (this.targetList != null)
                {
                    foreach (Target target in this.targetList)
                    {
                        target.Dispose();
                    }

                    this.targetList.Clear();
                }
            }

            this.DestroyNativeTracker();
        }

        /// <summary>
        /// Handles the update callback.
        /// Updates the tracking results for all the image targets.
        /// </summary>
        protected override void Update()
        {
            // Updates the tracking status for all the image targets.
            foreach (Target target in this.targetList)
            {
                target.UpdateTrackingData();
            }
        }

        /// <summary>
        /// Handles the application pause callback.
        /// </summary>
        /// <param name="pause"> True if the app is paused.</param>
        protected override void OnApplicationPause(bool pause)
        {
            MLResult result = pause ? Disable() : Enable();
            if (!result.IsOk)
            {
                MLPluginLog.ErrorFormat("MLImageTracker.OnApplicationPause failed to {0} the image tracker. Reason: {1}", pause ? "disable" : "enable", result);
            }
        }

        /// <summary>
        /// Static instance of the MLImageTracker class.
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLImageTracker.IsValidInstance())
            {
                MLImageTracker._instance = new MLImageTracker();
            }
        }

        /// <summary>
        /// Destroys the native tracker if the handle is valid.
        /// </summary>
        private void DestroyNativeTracker()
        {
            if (!MagicLeapNativeBindings.MLHandleIsValid(this.handle))
            {
                return;
            }

            MLResult.Code result = NativeBindings.MLImageTrackerDestroy(this.handle);
            if (result != MLResult.Code.Ok)
            {
                MLPluginLog.ErrorFormat("MLImageTracker.DestroyNativeTracker failed to destroy image tracker. Reason: {0}", result);
            }

            this.handle = MagicLeapNativeBindings.InvalidHandle;
        }

        /// <summary>
        /// Creates an Image Target based on the provided parameters.
        /// </summary>
        /// <param name="name">The name to give the Image Target.</param>
        /// <param name="image">The image that represents the target.</param>
        /// <param name="width">The width of the Image Target.</param>
        /// <param name="callback">The callback to use for any status updates of the new Image Target.</param>
        /// <param name="isStationary">Set this to true if the position of this Image Target in the physical world is fixed and the local geometry is planar.</param>
        /// <returns>The newly created Image Target.</returns>
        private Target CreateTarget(string name, Texture2D image, float width, MLImageTracker.Target.OnImageResultDelegate callback, bool isStationary)
        {
            if (string.IsNullOrEmpty(name))
            {
                MLPluginLog.Error("MLImageTracker.CreateTarget failed to add MLImageTracker.Target to ImageTracker. Reason: The unique name provided is null or empty.");
                return null;
            }

            if (image == null)
            {
                MLPluginLog.ErrorFormat("MLImageTracker.CreateTarget failed to add MLImageTracker.Target \"{0}\" to ImageTracker. Reason: The Texture2D image provided is null.", name);
                return null;
            }

            if (callback == null)
            {
                MLPluginLog.ErrorFormat("MLImageTracker.CreateTarget failed to add MLImageTracker.Target \"{0}\" to ImageTracker. Reason: The callback function provided is null.", name);
                return null;
            }

            // Check to see if a version of this image is already tracked.
            // Currently this only checks for unique names.
            if (this.targetList.FindIndex((Target target) => target.TargetSettings.Name.Equals(name)) > -1)
            {
                MLPluginLog.ErrorFormat("MLImageTracker.CreateTarget failed to add MLImageTracker.Target \"{0}\" to ImageTracker. Reason: A target with the same name is already added to this tracker.", name);
                return null;
            }

            return new Target(name, image, width, callback, this.handle, isStationary);
        }

        /// <summary>
        /// Internal call used to add an Image Target with the provided parameters to the Image Tracker.
        /// </summary>
        /// <param name="name">The name to give the Image Target.</param>
        /// <param name="image">The image that represents the target.</param>
        /// <param name="longerDimension">Size of the longer dimension in scene units.</param>
        /// <param name="callback">The callback to use for any status updates of the new Image Target.</param>
        /// <param name="isStationary">Set this to true if the position of this Image Target in the physical world is fixed and the local geometry is planar.</param>
        /// <returns>The newly created Image Target.</returns>
        private Target AddTargetInternal(string name, Texture2D image, float longerDimension, MLImageTracker.Target.OnImageResultDelegate callback, bool isStationary = false)
        {
            Target newTarget = this.CreateTarget(name, image, longerDimension, callback, isStationary);

            if (newTarget == null)
            {
                MLPluginLog.ErrorFormat("MLImageTracker.AddTargetInternal failed to add MLImageTarget \"{0}\" to ImageTracker. Tracker Handle: {1}", name, this.handle);
                return null;
            }

            if (!newTarget.IsValid)
            {
                MLPluginLog.ErrorFormat("MLImageTracker.AddTargetInternal failed to add MLImageTracker.Target \"{0}\" to ImageTracker. Tracker Handle: {1}", name, this.handle);
                newTarget.Dispose();
                return null;
            }

            this.targetList.Add(newTarget);
            return newTarget;
        }

        /// <summary>
        /// Internal call used to set the status of the Image Tracker.
        /// </summary>
        /// <param name="enabled">Used to enabled or disable Image Tracker to be enabled or disabled.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there were invalid trackerSettings when setting the Image Tracker status.
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the Image Tracker status was updated successfully or if no change was needed.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
        /// </returns>
        private MLResult SetTrackerStatusInternal(bool enabled)
        {
            if (this.trackerSettings.EnableTracker == enabled)
            {
                return MLResult.Create(MLResult.Code.Ok, "No change needed");
            }

            bool prevStatus = this.trackerSettings.EnableTracker;
            this.trackerSettings.EnableTracker = enabled;

            MLResult.Code resultCode = NativeBindings.MLImageTrackerUpdateSettings(this.handle, ref this.trackerSettings);
            MLResult result = MLResult.Create(resultCode);
            if (!result.IsOk)
            {
                this.trackerSettings.EnableTracker = prevStatus;
            }

            return result;
        }

        /// <summary>
        /// Represents the list of Image Tracker settings.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Settings
        {
            /// <summary>
            /// Maximum number of Image Targets that can be tracked at any given time.
            /// If the tracker is already tracking the maximum number of targets
            /// possible then it will stop searching for new targets which helps
            /// reduce the load on the CPU. For example, if you are interested in
            /// tracking a maximum of x targets simultaneously from a list y (x &lt; y)
            /// targets then set this parameter to x.
            /// The valid range for this parameter is from 1 through 25.
            /// </summary>
            public uint MaxSimultaneousTargets;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <param name="maxTargets">The max number of targets to track at once.</param>
            /// <returns>An initialized version of this struct.</returns>
            public static Settings Create(uint maxTargets)
            {
                return new Settings
                {
                    MaxSimultaneousTargets = maxTargets
                };
            }

            /// <summary>
            /// The equality check to be used for comparing two MLImageTracker.Settings structs.
            /// </summary>
            /// <param name="one">The first struct to compare with the second struct. </param>
            /// <param name="two">The second struct to compare with the first struct. </param>
            /// <returns>True if the two provided structs have the same number of MaxSimultaneousTargets.</returns>
            public static bool operator ==(Settings one, MLImageTracker.Settings two)
            {
                return one.MaxSimultaneousTargets == two.MaxSimultaneousTargets;
            }

            /// <summary>
            /// The inequality check to be used for comparing two MLImageTracker.Settings structs.
            /// </summary>
            /// <param name="one">The first struct to compare with the second struct. </param>
            /// <param name="two">The second struct to compare with the first struct. </param>
            /// <returns>True if the two provided structs do not have the same number of MaxSimultaneousTargets.</returns>
            public static bool operator !=(Settings one, Settings two)
            {
                return one.MaxSimultaneousTargets != two.MaxSimultaneousTargets;
            }

            /// <summary>
            /// The equality check to be used for comparing another object to this one.
            /// </summary>
            /// <param name="obj">The object to compare to this one with. </param>
            /// <returns>True if the the provided object is of the MLImageTracker.Settings type and has the same number of MaxSimultaneousTargets.</returns>
            public override bool Equals(object obj)
            {
                // Check for null values and compare run-time types.
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                Settings p = (Settings)obj;
                return this.MaxSimultaneousTargets == p.MaxSimultaneousTargets;
            }

            /// <summary>
            /// Gets the hash code to use from MaxSimultaneousTargets.
            /// </summary>
            /// <returns>The hash code returned by MaxSimultaneousTargets.</returns>
            public override int GetHashCode()
            {
                return this.MaxSimultaneousTargets.GetHashCode();
            }
        }
        #endif
    }
}
