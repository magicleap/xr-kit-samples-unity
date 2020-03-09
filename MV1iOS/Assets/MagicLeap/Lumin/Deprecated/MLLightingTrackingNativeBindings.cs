// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLLightingTrackingNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

using System;
using System.Linq;
using System.Runtime.InteropServices;

// Disable warnings about missing documentation for native interop.
#pragma warning disable 1591

namespace UnityEngine.XR.MagicLeap.Native
{
    /// <summary>
    /// See ml_lighting_tracking.h for additional comments.
    /// </summary>
    public partial class MLLightingTrackingNativeBindings : MagicLeapNativeBindings
    {
        /// <summary>
        /// Information about the ambient light sensor global state.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [Obsolete("Please use MLLightingTrackingNativeBindings.AmbientGlobalStateNative instead.", false)]
        public struct MLLightingTrackingAmbientGlobalStateNative
        {
            /// <summary>
            /// Array stores values for each world camera, ordered left, right, far left, far right.
            /// Luminance estimate is in nits (cd/m^2).
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CameraCount)]
            public ushort[] cameraLuminance;

            /// <summary>
            /// Time when captured in nanoseconds since the Epoch.
            /// </summary>
            public long TimestampInNanoSeconds;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <returns>Information of the ambient light's global state.</returns>
            public static MLLightingTrackingAmbientGlobalStateNative Create()
            {
                return new MLLightingTrackingAmbientGlobalStateNative
                {
                    cameraLuminance = Enumerable.Repeat<ushort>(0, CameraCount).ToArray(),
                    TimestampInNanoSeconds = 0
                };
            }
        }

        /// <summary>
        /// Information about the color temperature state.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [Obsolete("Please use MLLightingTrackingNativeBindings.ColorTemperatureStateNative instead.", false)]
        public struct MLLightingTrackingColorTemperatureStateNative
        {
            /// <summary>
            /// Color Temperature in Kelvin.
            /// </summary>
            public ushort ColorTemperature;

            /// <summary>
            /// Average red representation of the kelvin color temperature.
            /// </summary>
            public ushort RawPixelAverageR;

            /// <summary>
            ///  Average green representation of the kelvin color temperature.
            /// </summary>
            public ushort RawPixelAverageG;

            /// <summary>
            ///  Average blue representation of the kelvin color temperature.
            /// </summary>
            public ushort RawPixelAverageB;

            /// <summary>
            ///  CIE <c>tristimulus</c> X value.
            /// </summary>
            public float XCIE;

            /// <summary>
            /// CIE <c>tristimulus</c> Y value.
            /// </summary>
            public float YCIE;

            /// <summary>
            /// CIE <c>tristimulus</c> Z value.
            /// </summary>
            public float ZCIE;

            /// <summary>
            /// Time when captured in nanoseconds since the Epoch.
            /// </summary>
            public long TimestampInNanoSeconds;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <returns>Initialized version of this struct.</returns>
            public static MLLightingTrackingColorTemperatureStateNative Create()
            {
                return new MLLightingTrackingColorTemperatureStateNative
                {
                    ColorTemperature = 0,
                    RawPixelAverageR = 0,
                    RawPixelAverageG = 0,
                    RawPixelAverageB = 0,
                    XCIE = 0.0f,
                    YCIE = 0.0f,
                    ZCIE = 0.0f,
                    TimestampInNanoSeconds = 0
                };
            }
        }

        [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
        [Obsolete("Please use MLLightingTrackingNativeBindings.MLLightingTrackingGetColorTemperatureState with MLLightingTrackingNativeBindings.ColorTemperatureStateNative instead.", false)]
        public static extern MLResult.Code MLLightingTrackingGetColorTemperatureState(ulong handle, ref MLLightingTrackingColorTemperatureStateNative outState);

        [Obsolete("Please use MLLightingTrackingNativeBindings.MLLightingTrackingGetAmbientGlobalState with MLLightingTrackingNativeBindings.AmbientGlobalStateNative instead.", false)]
        [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLLightingTrackingGetAmbientGlobalState(ulong handle, ref MLLightingTrackingAmbientGlobalStateNative outState);
    }
}
    #endif
