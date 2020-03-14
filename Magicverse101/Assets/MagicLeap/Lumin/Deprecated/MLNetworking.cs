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
    using System;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// MLNetworking provides network connectivity status as well as current connection data.
    /// Connection data is only retrieved if device is connected and will be all zeros otherwise.
    /// </summary>
    public static partial class MLNetworking
    {
        /// <summary>
        /// Gets a readable version of the result code as an ASCII string.
        /// </summary>
        /// <param name="resultCode">The MLResult that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        [Obsolete("Please use MLResult.CodeToString(MLResult.Code) instead.", false)]
        public static string GetResultString(MLResultCode resultCode)
        {
            string finalResult = "MLNetworking.GetResultString failed due to internal error";

            try
            {
                finalResult = "Use MLResult.CodeToString(MLResult.Code) instead.";
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
        [Obsolete("Please use MLNetworking.GetWifiData with the MLNetworking.WifiData struct instead.", false)]
        public static MLResult GetWifiData(ref MLNetworkingWifiData wifiData)
        {
            MLResult finalResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLNetworking.GetWifiData failed due to internal error");
            try
            {
                MLNetworkingNativeBindings.MLNetworkingWifiDataNative wifiDataInternal = MLNetworkingNativeBindings.MLNetworkingWifiDataNative.Create();
                MLResult.Code result = MLNetworkingNativeBindings.MLNetworkingGetWiFiData(ref wifiDataInternal);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLNetworking.GetWifiData failed to get Wi-Fi data. Reason: {0}", result);
                    wifiData = new MLNetworkingWifiData();
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
    }
}

#endif
