// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHandTrackingNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    /// MLHandTracking is the entry point for all the hand tracking data
    /// including gestures, hand centers and key points for both hands.
    /// </summary>
    public partial class MLHandTracking : MLAPISingleton<MLHandTracking>
    {
        /// <summary>
        /// Class that deals with the native calls and configurations of the MLHandTracking API.
        /// </summary>
        private class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// The max number of key points to track.
            /// </summary>
            public const int MaxKeyPoints = 24;

            /// <summary>
            /// The max number of key poses to track.
            /// </summary>
            public const int MaxKeyPoses = (int)(MLHandTracking.HandKeyPose.NoHand + 1);

            /// <summary>
            /// Prevents a default instance of the <see cref="NativeBindings" /> class from being created.
            /// </summary>
            private NativeBindings()
            {
            }

            /// <summary>
            /// Native call for updating the configuration object.
            /// </summary>
            /// <param name="config">The configuration object to update</param>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_GesturesUpdateConfiguration")]
            public static extern void UpdateConfiguration(ref ConfigurationNative config);

            /// <summary>
            /// Native call for checking if hand gestures are enabled.
            /// </summary>
            /// <returns>True if hand gestures are enabled.</returns>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_GesturesIsHandGesturesEnabled")]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool IsHandGesturesEnabled();

            /// <summary>
            /// Native call for setting hand gestures to enabled.
            /// </summary>
            /// <param name="value">Value indicating to enable or disable hand gestures.</param>
            [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_GesturesSetHandGesturesEnabled")]
            public static extern void SetHandGesturesEnabled(bool value);

            /// <summary>
            /// The native configuration object used to hold key poses, key pose statuses, and key pose/point tracking fidelity.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ConfigurationNative
            {
                /// <summary>
                /// Array length excludes [NoHand], since we do not allow it to be disabled.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MLHandTracking.HandKeyPose.NoHand)]
                public byte[] KeyposeConfig;

                /// <summary>
                /// Determines if the hand tracking pipeline is currently enabled.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool HandTrackingPipelineEnabled;

                /// <summary>
                /// The fidelity to track key points with.
                /// </summary>
                public MLHandTracking.KeyPointFilterLevel KeyPointsFilterLevel;

                /// <summary>
                /// The fidelity to track key poses with.
                /// </summary>
                public MLHandTracking.PoseFilterLevel PoseFilterLevel;

                /// <summary>
                /// Creates and return an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static ConfigurationNative Create()
                {
                    return new ConfigurationNative()
                    {
                        KeyposeConfig = Enumerable.Repeat<byte>(0, (int)MLHandTracking.HandKeyPose.NoHand).ToArray(),
                        HandTrackingPipelineEnabled = false,
                        KeyPointsFilterLevel = MLHandTracking.KeyPointFilterLevel.Raw,
                        PoseFilterLevel = MLHandTracking.PoseFilterLevel.Raw
                    };
                }
            }
        }
    }
}
#endif
