// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLNetworkingNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap.Native
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine.XR.MagicLeap;

    /// <summary>
    /// The native bindings to the Networking API.
    /// See ml_networking.h for additional comments.
    /// </summary>
    public partial class MLNetworkingNativeBindings : MagicLeapNativeBindings
    {
        /// <summary>
        /// MLNetworking library name.
        /// </summary>
        private const string MLNetworkingDll = "ml_networking";

        /// <summary>
        /// Prevents a default instance of the <see cref="MLNetworkingNativeBindings" /> class from being created.
        /// </summary>
        private MLNetworkingNativeBindings()
        {
        }

        /// <summary>
        /// Returns whether the internet is connected.
        /// </summary>
        /// <param name="isConnected">Indicates whether the internet is up and accessible.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the call was successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
        /// MLResult.Result will be <c>MLResult.Code.NetworkingServiceNotAvailable</c> if the corresponding service was not available.
        /// MLResult.Result will be <c>MLResult.Code.NetworkingServiceError</c> if the corresponding service returned with an error.
        /// MLResult.Result will be <c>MLResult.Code.NetworkingWiFiDataStructureVersionError</c> if the version number in the struct is not recognized.
        /// MLResult.Result will be <c>MLResult.Code.NetworkingWiFiServiceInvalidState</c> if the Wi-Fi service was not in the right state.
        /// </returns>
        [DllImport(MLNetworkingDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLNetworkingIsInternetConnected([MarshalAs(UnmanagedType.I1)] ref bool isConnected);

        /// <summary>
        /// Gets device Wi-Fi related data.
        /// The Wi-Fi data object is only valid if the API returns MLResult_Ok.
        /// </summary>
        /// <param name="wifiData">Holds Wi-Fi related data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if Wi-Fi data query was successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
        /// MLResult.Result will be <c>MLResult.Code.NetworkingServiceNotAvailable</c> if the corresponding service was not available.
        /// MLResult.Result will be <c>MLResult.Code.NetworkingServiceError</c> if the corresponding service returned with an error.
        /// MLResult.Result will be <c>MLResult.Code.NetworkingWiFiDataStructureVersionError</c> if the version number in the struct is not recognized.
        /// MLResult.Result will be <c>MLResult.Code.NetworkingWiFiServiceInvalidState</c> if the Wi-Fi service was not in the right state.
        /// </returns>
        [DllImport(MLNetworkingDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLNetworkingGetWiFiData(ref WifiDataNative wifiData);

        /// <summary>
        /// Returns an ASCII string for each result code.
        /// This call returns strings for all of the MLResult codes having to do with networking.
        /// Developers should be using MLResult.CodeToString(MLResult.Code) to get string values of MLResult codes.
        /// </summary>
        /// <param name="result">The MLResult.Code to turn into a string.</param>
        /// <returns> A pointer to the ASCII string containing readable version of result code.</returns>
        [DllImport(MLNetworkingDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MLNetworkingGetResultString(MLResult.Code result);

        /// <summary>
        /// The native structure for Wi-Fi data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct WifiDataNative
        {
            /// <summary>
            /// Version of this structure.
            /// </summary>
            public uint Version;

            /// <summary>
            /// Wi-Fi RSSI in <c>decibel-milliwatts</c>.
            /// </summary>
            public int Rssi;

            /// <summary>
            /// Wi-Fi link speed in megabytes per second.
            /// </summary>
            public int LinkSpeed;

            /// <summary>
            /// Wi-Fi frequency in megahertz.
            /// </summary>
            public float Frequency;

            /// <summary>
            /// Gets an easy conversion from the native structure to the external one.
            /// </summary>
            public MLNetworking.WifiData Data
            {
                get
                {
                    MLNetworking.WifiData wifiData = new MLNetworking.WifiData();
                    wifiData.Rssi = this.Rssi;
                    wifiData.LinkSpeed = this.LinkSpeed;
                    wifiData.Frequency = this.Frequency;
                    return wifiData;
                }
            }

            /// <summary>
            /// Creates and returns an initialized version of this struct.
            /// </summary>
            /// <returns>An initialized version of this struct.</returns>
            public static WifiDataNative Create()
            {
                return new WifiDataNative()
                {
                    Version = 1u,
                    Rssi = 0,
                    LinkSpeed = 0,
                    Frequency = 0.0f
                };
            }
        }
    }
}

#endif
