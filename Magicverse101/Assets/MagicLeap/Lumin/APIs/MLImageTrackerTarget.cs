// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLImageTrackerTarget.cs" company="Magic Leap, Inc">
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
        /// <summary>
        /// Manages the image target settings.
        /// </summary>
        public sealed class Target : IDisposable
        {
            #if PLATFORM_LUMIN
            /// <summary>
            /// Reference to the image target's settings.
            /// </summary>
            private MLImageTracker.Target.Settings targetSettings = new MLImageTracker.Target.Settings();

            /// <summary>
            /// The internal MLHANDLE used to reference this image target.
            /// </summary>
            private ulong targetHandle = MagicLeapNativeBindings.InvalidHandle;

            /// <summary>
            /// The internal MLHANDLE used to reference the tracker for this target.
            /// </summary>
            private ulong trackerHandle = MagicLeapNativeBindings.InvalidHandle;

            /// <summary>
            /// The provided Texture2D encoded to a byte[] format used by the tracker system.
            /// </summary>
            private byte[] imageData;

            /// <summary>
            /// Static data for this image target. Contains the coordinate frame ID.
            /// </summary>
            private NativeBindings.MLImageTrackerTargetStaticDataNative targetStaticData;

            /// <summary>
            /// Tracking result as retrieved from Native bindings directly.
            /// </summary>
            private NativeBindings.MLImageTrackerTargetResultNative nativeTrackingResult;

            /// <summary>
            /// The last known result of this target.
            /// </summary>
            private Result lastTrackingResult = new Result();

            /// <summary>
            /// Initializes a new instance of the <see cref="Target" /> class.
            /// </summary>
            /// <param name="name"> Image target's name. </param>
            /// <param name="image"> Texture2D representing the image target. The size of the texture should not be changed. Set the "Non Power of 2" property of Texture2D to none. </param>
            /// <param name="longerDimension"> Longer dimension of the image target in scene units. Default is meters. </param>
            /// <param name="callback"> Tracking result callback for this image target. </param>
            /// <param name="handle"> Handle for the image tracker. </param>
            /// <param name="isStationary"> Set this to true if the position of this image target in the physical world is fixed and its surroundings are planar (ex: walls, floors, tables, etc). </param>
            public Target(string name, Texture2D image, float longerDimension, OnImageResultDelegate callback, ulong handle, bool isStationary = false)
            {
                this.targetSettings = MLImageTracker.Target.Settings.Create();
                this.targetStaticData = new NativeBindings.MLImageTrackerTargetStaticDataNative();
                this.nativeTrackingResult = new NativeBindings.MLImageTrackerTargetResultNative();

                // It is assumed that all the parameters are valid as this class should only be used by the MLImageTracker,
                // which already has checks for these values before it creates the MLImageTracker.Target.
                this.targetSettings.Name = name;
                this.targetSettings.LongerDimension = longerDimension;
                this.targetSettings.IsStationary = isStationary;
                this.trackerHandle = handle;
                this.OnImageResult = callback;

                this.imageData = MLTextureUtils.ConvertToByteArray(image, out int numChannels);
                this.targetHandle = MagicLeapNativeBindings.InvalidHandle;

                MLResult.Code result = NativeBindings.MLImageTrackerAddTargetFromArray(this.trackerHandle, ref this.targetSettings, this.imageData, (uint)image.width, (uint)image.height, (numChannels == 4) ? MLImageTracker.ImageFormat.RGBA : MLImageTracker.ImageFormat.RGB, ref this.targetHandle);

                if (result == MLResult.Code.Ok && this.IsValid && MagicLeapNativeBindings.MLHandleIsValid(this.trackerHandle))
                {
                    result = NativeBindings.MLImageTrackerGetTargetStaticData(this.trackerHandle, this.targetHandle, ref this.targetStaticData);
                    if (result != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLImageTracker.Target.Target failed to get static data for target. Reason: {0}", result);
                    }
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLImageTracker.Target.Target failed, one or both handles are invalid: Tracker Handle: {0}, Existing Target Handle: {1}. Reason: {2}", this.trackerHandle, this.targetHandle, result);
                }
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="Target" /> class.
            /// </summary>
            ~Target()
            {
                this.Cleanup();
            }

            /// <summary>
            /// Delegate used for the OnImageResult event.
            /// </summary>
            /// <param name="target"> The image target that produced a result. </param>
            /// <param name="result"> The latest result of the given image target. </param>
            public delegate void OnImageResultDelegate(Target target, Result result);

            /// <summary>
            /// The event for when the ImageTracker has an update result for one of it's image targets.
            /// </summary>
            private event OnImageResultDelegate OnImageResult = (Target target, Result result) => { };
            #endif

            /// <summary>
            /// Identifies the status of an image target.
            /// Each MLImageTracker.Target.Result will include a MLImageTracker.Target.TrackingStatus
            /// giving the current status of the target.
            /// </summary>
            public enum TrackingStatus
            {
                /// <summary>
                /// Image target is tracked.
                /// The image tracker system will provide a 6 DOF pose when queried using
                /// MLSnapshotGetTransform function.
                /// </summary>
                Tracked,

                /// <summary>
                /// Image target is tracked with low confidence.
                /// The image tracker system will still provide a 6 DOF pose. But this
                /// pose might be inaccurate and might have jitter. When the tracking is
                /// unreliable one of the following two events will happen quickly : Either the
                /// tracking will recover to MLImageTrackerTargetStatus.Tracked or tracking
                /// will be lost and the status will change to
                /// MLImageTrackerTargetStatus.NotTracked.
                /// </summary>
                Unreliable,

                /// <summary>
                /// Image target is lost.
                /// The image tracker system will not report any pose for this target. Querying
                /// for the pose using MLSnapshotGetTransform will return false.
                /// </summary>
                NotTracked,
            }

            #if PLATFORM_LUMIN
            /// <summary>
            /// Gets a copy of the image target's settings.
            /// </summary>
            public Settings TargetSettings
            {
                get
                {
                    return this.targetSettings;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the image target's handle is valid or not.
            /// </summary>
            public bool IsValid
            {
                get
                {
                    return MagicLeapNativeBindings.MLHandleIsValid(this.targetHandle);
                }
            }
            #endif

            /// <summary>
            /// Cleans up the tracker and target handles and stops the destructor from being called.
            /// </summary>
            public void Dispose()
            {
                #if PLATFORM_LUMIN
                this.Cleanup();
                #endif

                GC.SuppressFinalize(this);
            }

            #if PLATFORM_LUMIN
            /// <summary>
            /// Updates this image target's tracking data, status, and transforms.
            /// OnImageResult is called with the updated result.
            /// </summary>
            public void UpdateTrackingData()
            {
                bool success = false;
                if (MagicLeapNativeBindings.MLHandleIsValid(this.trackerHandle) && this.IsValid)
                {
                    MLResult.Code result = NativeBindings.MLImageTrackerGetTargetResult(this.trackerHandle, this.targetHandle, ref this.nativeTrackingResult);
                    if (result != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLImageTracker.Target.UpdateTrackingData failed getting image tracker target result. Reason: {0}", result);
                    }
                    else
                    {
                        this.lastTrackingResult.Status = (TrackingStatus)this.nativeTrackingResult.Status;
                        if (TrackingStatus.NotTracked == this.lastTrackingResult.Status || !MLImageTracker.GetTrackerStatus())
                        {
                            // If the target is not being tracked then there is no need to query for the transform.
                            success = true;
                        }
                        else
                        {
                            if (MagicLeapNativeBindings.UnityMagicLeap_TryGetPose(this.targetStaticData.CoordFrameTarget, out Pose outputPose))
                            {
                                this.lastTrackingResult.Position = outputPose.position;
                                this.lastTrackingResult.Rotation = outputPose.rotation;
                                this.lastTrackingResult.Rotation *= Quaternion.Euler(270, 0, 0);
                                success = true;
                            }
                            else
                            {
                                MLPluginLog.Error("MLImageTracker.Target.UpdateTrackingData failed getting target transforms.");
                            }
                        }
                    }

                    if (success)
                    {
                        this.OnImageResult?.Invoke(this, this.lastTrackingResult);
                    }
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLImageTracker.Target.UpdateTrackingData failed. Reason: Handle was not valid: Tracker Handle: {0}, Existing Target Handle: {1}.", this.trackerHandle, this.targetHandle);
                }
            }

            /// <summary>
            /// Get the longer dimension of the image target.
            /// </summary>
            /// <returns> longer dimension in scene units.</returns>
            public float GetTargetLongerDimension()
            {
                return this.targetSettings.LongerDimension;
            }

            /// <summary>
            /// Set the longer dimension of the image target.
            /// </summary>
            /// <param name="longerDimension"> Longer dimension in scene units. Default is meters.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// </returns>
            public MLResult SetTargetLongerDimension(float longerDimension)
            {
                if (Mathf.Approximately(this.targetSettings.LongerDimension, longerDimension))
                {
                    return MLResult.Create(MLResult.Code.Ok, "Longer dimension already set to value");
                }

                float oldValue = this.targetSettings.LongerDimension;
                this.targetSettings.LongerDimension = longerDimension;
                MLResult.Code resultCode = NativeBindings.MLImageTrackerUpdateTargetSettings(this.trackerHandle, this.targetHandle, ref this.targetSettings);
                MLResult result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    this.targetSettings.LongerDimension = oldValue;
                }

                return result;
            }

            /// <summary>
            /// Cleans up unmanaged memory.
            /// </summary>
            private void Cleanup()
            {
                if (this.IsValid && MagicLeapNativeBindings.MLHandleIsValid(this.trackerHandle))
                {
                    MLResult.Code result = NativeBindings.MLImageTrackerRemoveTarget(this.trackerHandle, this.targetHandle);
                    if (result != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLImageTracker.Target.Cleanup failed to remove target. Tracker Handle : {0}, Target Name: {1}, Target Handle : {2}. Reason: {3}", this.trackerHandle, this.targetSettings.Name, this.targetHandle, result);
                    }
                }

                // Always invalidate the handles if the user tried to remove this target.
                this.targetHandle = MagicLeapNativeBindings.InvalidHandle;
                this.trackerHandle = MagicLeapNativeBindings.InvalidHandle;
            }

            /// <summary>
            /// Represents the image target result.
            /// </summary>
            public struct Result
            {
                /// <summary>
                /// Position of the target.
                /// This is not valid if the target is not being tracked.
                /// </summary>
                public Vector3 Position;

                /// <summary>
                /// Orientation of the target.
                /// This is not valid if the target is not being tracked.
                /// </summary>
                public Quaternion Rotation;

                /// <summary>
                /// Status of the target.
                /// Every target will have an associated status indicating the current
                /// tracking status.
                /// </summary>
                public TrackingStatus Status;
            }

            /// <summary>
            /// Represents the settings of an Image Target.
            /// All fields are required for an Image Target to be tracked.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct Settings
            {
                /// <summary>
                /// Name of the target.
                /// This name has to be unique across all targets added to the Image Tracker.
                /// Caller should allocate memory for this.
                /// Encoding should be in UTF8.
                /// This will be copied internally.
                /// </summary>
                public string Name;

                /// <summary>
                /// LongerDimension refer to the size of the longer dimension of the physical Image.
                /// Target in Unity scene units.
                /// </summary>
                public float LongerDimension;

                /// <summary>
                /// Set this to \c true to improve detection for stationary targets.
                /// An Image Target is a stationary target if its position in the physical world does not change.
                /// This is best suited for cases where the target is stationary and when the local geometry (environment surrounding the target) is planar.
                /// When in doubt set this to false.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool IsStationary;

                /// <summary>
                /// Set this to \c true to track the image target.
                /// Disabling the target when not needed marginally improves the tracker CPU performance.
                /// This is best suited for cases where the target is temporarily not needed.
                /// If the target no longer needs to be tracked it is best to use remove the target.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool IsEnabled;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static Settings Create()
                {
                    return new Settings
                    {
                        Name = string.Empty,
                        LongerDimension = 0.0f,
                        IsStationary = false,
                        IsEnabled = false
                    };
                }
            }
            #endif
        }
    }
}
