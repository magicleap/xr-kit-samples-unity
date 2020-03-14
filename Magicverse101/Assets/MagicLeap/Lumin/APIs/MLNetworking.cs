// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLNetworking.cs" company="Magic Leap, Inc">
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
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// MLNetworking provides network connectivity status as well as current connection data.
    /// Connection data is only retrieved if device is connected and will be all zeros otherwise.
    /// </summary>
    public static partial class MLNetworking
    {
        /// <summary>
        /// Retrieves data from the current Wi-Fi network.
        /// </summary>
        /// <param name="wifiData">Reference to the struct to populate.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the given parameter is invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.ServiceNotAvailable</c> if the corresponding service is not available.
        /// MLResult.Result will be <c>MLResult.Code.ServiceError</c> if the corresponding service returned with error.
        /// MLResult.Result will be <c>MLResult.Code.WiFiDataStructureVersionError</c> if the version number in the given struct is not recognized.
        /// MLResult.Result will be <c>MLResult.Code.WiFiServiceInvalidState</c> if the Wi-Fi service is not in the right state.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        public static MLResult GetWifiData(ref MLNetworking.WifiData wifiData)
        {
            MLResult finalResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLNetworking.GetWifiData failed due to internal error");
            try
            {
                MLNetworkingNativeBindings.WifiDataNative wifiDataInternal = MLNetworkingNativeBindings.WifiDataNative.Create();
                MLResult.Code result = MLNetworkingNativeBindings.MLNetworkingGetWiFiData(ref wifiDataInternal);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLNetworking.GetWifiData failed to get Wi-Fi data. Reason: {0}", result);
                    wifiData = new MLNetworking.WifiData();
                }
                else
                {
                    wifiData = wifiDataInternal.Data;
                }

                finalResult = MLResult.Create(result);
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error("MLNetworking API is currently available only on device.");
                finalResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Dll not found");
            }
            catch (System.EntryPointNotFoundException)
            {
                string errorMessage = string.Format("MLNetworking API symbols not found");
                finalResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, errorMessage);
            }

            return finalResult;
        }

        /// <summary>
        /// Retrieves if there's currently internet connection.
        /// </summary>
        /// <param name="isConnected">Reference to populate with whether or not there's internet connectivity.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if isConnected parameter is invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.ServiceNotAvailable</c> if the corresponding service is not available.
        /// MLResult.Result will be <c>MLResult.Code.ServiceError </c> if the corresponding service returned with error.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        public static MLResult IsInternetConnected(ref bool isConnected)
        {
            MLResult finalResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLNetworking.IsInternetConnected failed due to internal error");
            try
            {
                isConnected = false;
                MLResult.Code result = MLNetworkingNativeBindings.MLNetworkingIsInternetConnected(ref isConnected);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLNetworking.IsInternetConnected failed to check if device is connected to internet. Reason: {0}", result);
                    isConnected = false;
                }

                finalResult = MLResult.Create(result);
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error("MLNetworking API is currently available only on device.");
                finalResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Dll not found");
            }
            catch (System.EntryPointNotFoundException)
            {
                string errorMessage = string.Format("MLNetworking API symbols not found");
                finalResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, errorMessage);
            }

            return finalResult;
        }

        /// <summary>
        /// Gets a readable version of the result code as an ASCII string.
        /// </summary>
        /// <param name="resultCode">The MLResult that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        public static string GetResultString(MLResult.Code resultCode)
        {
            string finalResult = "MLNetworking.GetResultString failed due to internal error";

            try
            {
                finalResult = Marshal.PtrToStringAnsi(MLNetworkingNativeBindings.MLNetworkingGetResultString(resultCode));
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error("MLNetworking API is currently available only on device.");
                finalResult = "MLNetworking API is currently available only on device.";
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLNetworking API symbols not found");
                finalResult = "MLNetworking API symbols not found";
            }

            return finalResult;
        }

        /// <summary>
        /// Contains Wi-Fi related data.
        /// </summary>
        public struct WifiData
        {
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
        }
    }
}

#endif
