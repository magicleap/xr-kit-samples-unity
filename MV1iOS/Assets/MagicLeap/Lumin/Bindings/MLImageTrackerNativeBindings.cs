// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLImageTrackerNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;

    /// <summary>
    /// MLImageTracker manages the Image Tracker system.
    /// Image Tracker enables experiences that recognize 2D planar images
    /// (image targets) in the physical world. It provides the position and
    /// orientation of the image targets in the physical world.
    /// </summary>
    public sealed partial class MLImageTracker
    {
        /// <summary>
        /// The native bindings to the Image Tracking API.
        /// See ml_image_tracking.h for additional comments
        /// </summary>
        private class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// Sets the values of an #MLImageTracker.Settings structure to defaults.
            /// Call this function to get the default MLImageTracker.Settings used by the Image Tracker system.
            /// </summary>
            /// <param name="outSettings">Pointer to MLDispatchPacket struct that will be initialized.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there were invalid outSettings.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if outSettings is successfully initialized with the default values.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed there was an internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLImageTrackerInitSettings(ref MLImageTrackerSettingsNative outSettings);

            /// <summary>
            /// Creates the Image Tracker.
            /// This can be called only after starting the Perception system using MLPerceptionStartup() and HeadTracker using MLHeadTrackingCreate().
            /// This function should be called only once. Do not create multiple Image Trackers.
            /// </summary>
            /// <param name="trackerSettings">List of Image Tracker settings.</param>
            /// <param name="trackerHandle">Handle to the created Image Tracker.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid trackerHandle.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the Image Tracker was created successfully.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLImageTrackerCreate(ref MLImageTrackerSettingsNative trackerSettings, ref ulong trackerHandle);

            /// <summary>
            /// Updates the Image Tracker with new settings.
            /// </summary>
            /// <param name="trackerHandle">Handle to the created Image Tracker created by MLImageTrackerCreate().</param>
            /// <param name="trackerSettings">List of Image Tracker settings.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there were invalid trackerSettings.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the Image Tracker settings were updated successfully.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLImageTrackerUpdateSettings(ulong trackerHandle, ref MLImageTrackerSettingsNative trackerSettings);

            /// <summary>
            /// Destroys the Image Tracker.
            /// </summary>
            /// <param name="trackerHandle">Handle to the created Image Tracker created by MLImageTrackerCreate().</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the Image Tracker was destroyed successfully.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLImageTrackerDestroy(ulong trackerHandle);

            /// <summary>
            /// Creates and adds a new target to the image tracker from an array.
            /// Supported formats: Grayscale, RGB, and RGBA, should be specified using the MLImageTracker.ImageFormat enum.
            /// </summary>
            /// <param name="trackerHandle">Handle to the created Image Tracker created by MLImageTrackerCreate().</param>
            /// <param name="targetSettings">List of the settings to be used for the new target.</param>
            /// <param name="imageData">Pointer to the array of pixel data.</param>
            /// <param name="width">Width of the image.</param>
            /// <param name="height">Height of the image.</param>
            /// <param name="format"> Specifies the image format.</param>
            /// <param name="targetHandle">A handle to the created Image Target.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the target was added to the Image Tracker successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there were any invalid arguments or if there was an internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLImageTrackerAddTargetFromArray(ulong trackerHandle, ref MLImageTracker.Target.Settings targetSettings, [MarshalAs(UnmanagedType.LPArray)] byte[] imageData, uint width, uint height, MLImageTracker.ImageFormat format, ref ulong targetHandle);

            /// <summary>
            /// Removes and destroys a previously added Image Target from the Image Tracker.
            /// The Image Tracker will stop searching for this Image Target once its destroyed.
            /// </summary>
            /// <param name="trackerHandle">Handle to the created Image Tracker created by MLImageTrackerCreate().</param>
            /// <param name="targetHandle">Handle to the Image Target that needs to be removed.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the target was removed from the Image Tracker successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLImageTrackerRemoveTarget(ulong trackerHandle, ulong targetHandle);

            /// <summary>
            /// Updates the settings of an Image Target that is already added to the Image Tracker system.
            /// </summary>
            /// <param name="trackerHandle">Handle to the created Image Tracker created by MLImageTrackerCreate().</param>
            /// <param name="targetHandle">Handle to the Image Target whose settings needs to be updated.</param>
            /// <param name="targetSettings">List of the updated settings to be used for the new target.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there were invalid targetSettings
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the target's settings were updated successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLImageTrackerUpdateTargetSettings(ulong trackerHandle, ulong targetHandle, ref MLImageTracker.Target.Settings targetSettings);

            /// <summary>
            /// Gets the static data for an Image Target in the Image Tracker.
            /// First call MLImageTrackerGetTargetResult() to check to see if the target is being tracked.
            /// </summary>
            /// <param name="trackerHandle">Handle to the created Image Tracker created by MLImageTrackerCreate().</param>
            /// <param name="targetHandle">Handle to the Image Target whose static data needs to be retrieved.</param>
            /// <param name="outData">Pointer to MLImageTrackerTargetStaticData which contains the static data.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was invalid outData.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the static data for the specified target was fetched successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLImageTrackerGetTargetStaticData(ulong trackerHandle, ulong targetHandle, ref MLImageTrackerTargetStaticDataNative outData);

            /// <summary>
            /// Gets the result for a Image Target from the Image Tracker.
            /// This function should always be called after MLPerceptionGetSnapshot().
            /// The result returned is from the result cached during the last call to MLPerceptionGetSnapshot().
            /// </summary>
            /// <param name="trackerHandle">Handle to the created Image Tracker created by MLImageTrackerCreate().</param>
            /// <param name="targetHandle">Handle to the Image Target whose static data needs to be retrieved.</param>
            /// <param name="outData">Pointer to MLImageTrackerTargetStaticData which contains the static data.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was invalid outData.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the specified target result was fetched successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLImageTrackerGetTargetResult(ulong trackerHandle, ulong targetHandle, ref MLImageTrackerTargetResultNative outData);

            /// <summary>
            /// The native structure that holds Image Tracker settings.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MLImageTrackerSettingsNative
            {
                /// <summary>
                /// Max number of targets to track at one time.
                /// Cannot exceed 25.
                /// </summary>
                public uint MaxSimultaneousTargets;

                /// <summary>
                /// Boolean that determines if the tracker is currently enabled.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool EnableTracker;

                /// <summary>
                /// Initializes a new instance of the <see cref="MLImageTrackerSettingsNative" /> struct.
                /// </summary>
                /// <param name="customSettings">The Image Tracker settings to use.</param>
                public MLImageTrackerSettingsNative(MLImageTracker.Settings customSettings)
                {
                    this.MaxSimultaneousTargets = customSettings.MaxSimultaneousTargets;
                    this.EnableTracker = true;
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static MLImageTrackerSettingsNative Create()
                {
                    return new MLImageTrackerSettingsNative()
                    {
                        MaxSimultaneousTargets = 0u,
                        EnableTracker = false
                    };
                }

                /// <summary>
                /// The equality check to be used for comparing two MLImageTrackerSettingsNative structs.
                /// </summary>
                /// <param name="one">The first struct to compare with the second struct. </param>
                /// <param name="two">The second struct to compare with the first struct. </param>
                /// <returns>True if the two provided structs have the same number of MaxSimultaneousTargets and share the same tracker status.</returns>
                public static bool operator ==(MLImageTrackerSettingsNative one, MLImageTrackerSettingsNative two)
                {
                    return (one.MaxSimultaneousTargets == two.MaxSimultaneousTargets) && (one.EnableTracker == two.EnableTracker);
                }

                /// <summary>
                /// The inequality check to be used for comparing two MLImageTrackerSettingsNative structs.
                /// </summary>
                /// <param name="one">The first struct to compare with the second struct. </param>
                /// <param name="two">The second struct to compare with the first struct. </param>
                /// <returns>True if the two provided structs do not have the same number of MaxSimultaneousTargets and share the same tracker status.</returns>
                public static bool operator !=(MLImageTrackerSettingsNative one, MLImageTrackerSettingsNative two)
                {
                    return !((one.MaxSimultaneousTargets == two.MaxSimultaneousTargets) && (one.EnableTracker == two.EnableTracker));
                }

                /// <summary>
                /// The equality check to be used for comparing another object to this one.
                /// </summary>
                /// <param name="obj">The object to compare to this one with. </param>
                /// <returns>
                /// True if the the provided object is of the MLImageTrackerSettingsNative type,
                /// has the same number of MaxSimultaneousTargets,
                /// and shares the same tracker status.
                /// </returns>
                public override bool Equals(object obj)
                {
                    // Check for null values and compare run-time types.
                    if (obj == null || GetType() != obj.GetType())
                    {
                        return false;
                    }

                    MLImageTrackerSettingsNative p = (MLImageTrackerSettingsNative)obj;
                    return (this.MaxSimultaneousTargets == p.MaxSimultaneousTargets) && (this.EnableTracker == p.EnableTracker);
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

            /// <summary>
            /// Native structure that holds the static data of an Image Target.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MLImageTrackerTargetStaticDataNative
            {
                /// <summary>
                /// The unique identifier which expresses a coordinate frame.
                /// </summary>
                public MLCoordinateFrameUID CoordFrameTarget;
            }

            /// <summary>
            /// Native structure that holds the status of an Image Target.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MLImageTrackerTargetResultNative
            {
                /// <summary>
                /// The status of the Image Target.
                /// </summary>
                public MLImageTracker.Target.TrackingStatus Status;
            }
        }
    }
}

#endif
