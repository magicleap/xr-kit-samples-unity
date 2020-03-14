// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLLocation.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;

    /// <summary>
    /// MLLocation provides device location data including; Latitude, Longitude, and approximate Postal Code.
    /// Currently location is only received on Wi-Fi connect. If connection is lost, a new location will be
    /// provided on connect.
    /// </summary>
    public sealed partial class MLLocation : MLAPISingleton<MLLocation>
    {
        /// <summary>
        /// Gets a readable version of the result code as an ASCII string.
        /// </summary>
        /// <param name="resultCode">The MLResult that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        [Obsolete("Please use MLResult.CodeToString(MLResult.Code) instead.", true)]
        public static string GetResultString(MLResultCode resultCode)
        {
            return "This function is obsolete. Use MLResult.CodeToString(MLResult.Code) instead.";
        }

        /// <summary>
        /// External call for querying the last known fine location.
        /// The accuracy field of the MLLocation.Location provides the estimate accuracy radius in meters.
        /// Returns the last known fine location data on success and returns the last queried fine location data on failure. Latitude and Longitude of
        /// 0 should be assumed an Invalid Location.
        /// </summary>
        /// <param name="fineLocation">Where to store the last known fine location. Only updates when getting the location succeeds. Latitude and Longitude of 0 should be assumed an Invalid Location.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the location was queried successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid location.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privilege(s).
        /// MLResult.Result will be <c>MLResult.Code.LocationProviderNotFound</c> if there was no provider or an invalid request was made.
        /// MLResult.Result will be <c>MLResult.Code.LocationNetworkConnection</c> if there was no internet connection.
        /// MLResult.Result will be <c>MLResult.Code.LocationNoLocation</c> if the location could not be found.
        /// </returns>
        [Obsolete("Please use MLLocation.GetLastFineLocation with the MLLocation.Location struct instead.")]
        public static MLResult GetLastFineLocation(out MLLocationData fineLocation)
        {
            MLResult result = Instance.GetLastLocationInternal(out fineLocation, false);
            return result;
        }

        /// <summary>
        /// External call for querying the last known coarse location.
        /// Coarse location provides latitude and longitude estimate from block to city accuracy.
        /// Returns the last known coarse location data on success and returns the last queried coarse location data on failure. Latitude and Longitude of
        /// 0 should be assumed an Invalid Location.
        /// </summary>
        /// <param name="coarseLocation">Where to store the last known coarse location.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c>  because this function is deprecated.
        /// </returns>
        [Obsolete("Please use MLLocation.GetLastCoarseLocation with the MLLocation.Location struct instead.")]
        public static MLResult GetLastCoarseLocation(out MLLocationData coarseLocation)
        {
            MLResult result = Instance.GetLastLocationInternal(out coarseLocation, true);
            return result;
        }

        /// <summary>
        /// Internal call for querying the last known coarse/fine location.
        /// The accuracy field of the MLLocation.Location provides the estimate accuracy radius in meters.
        /// Returns the last known data on success and returns the last queried location data on failure. Latitude and Longitude of
        /// 0 should be assumed an Invalid Location.
        /// </summary>
        /// <param name="outLocation">Where to store the last known fine location. Only updates when getting the location succeeds. Latitude and Longitude of 0 should be assumed an Invalid Location.</param>
        /// <param name="isFine">Determines whether to query for fine location or coarse location.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the location was queried successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid location.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privilege(s).
        /// MLResult.Result will be <c>MLResult.Code.LocationProviderNotFound</c> if there was no provider or an invalid request was made.
        /// MLResult.Result will be <c>MLResult.Code.LocationNetworkConnection</c> if there was no internet connection.
        /// MLResult.Result will be <c>MLResult.Code.LocationNoLocation</c> if the location could not be found.
        /// </returns>
        [Obsolete("Please use MLLocation.GetLastLocationInternal with the MLLocation.Location struct instead.")]
        private MLResult GetLastLocationInternal(out MLLocationData outLocation, bool isFine)
        {
            // The automatic marshaling was not working properly for this structure. TimeStamp was always 0. To solve this
            // we use an IntPtr that has the proper default values set.
            NativeBindings.LocationNative location = NativeBindings.LocationNative.Create();
            MLResult.Code resultCode;

            IntPtr myPointer = Marshal.AllocHGlobal(Marshal.SizeOf(location));

            Marshal.StructureToPtr(location, myPointer, false);

            try
            {
                resultCode = isFine ? NativeBindings.MLLocationGetLastFineLocation(myPointer) : NativeBindings.MLLocationGetLastCoarseLocation(myPointer);

                if (resultCode != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLLocation.GetLastLocationInternal failed to get location. Reason: {0}", resultCode);
                    location.Latitude = 0.0000f;
                    location.Longitude = 0.0000f;
                }

                location = (NativeBindings.LocationNative)Marshal.PtrToStructure(myPointer, typeof(NativeBindings.LocationNative));

                Marshal.FreeHGlobal(myPointer);

                MLResult finalResult = MLResult.Create(resultCode);

                outLocation = location.DataEx;

                if (finalResult.IsOk)
                {
                    if (isFine)
                    {
                        this.lastValidFineLocation = location.Data;
                    }
                    else
                    {
                        this.lastValidCoarseLocation = location.Data;
                    }
                }

                return finalResult;
            }
            catch (EntryPointNotFoundException)
            {
                outLocation = new MLLocationData();
                MLPluginLog.Error("MLLocation.GetLastLocationInternal failed. Reason: API symbols not found.");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLLocation.GetLastLocationInternal failed. Reason: API symbols not found.");
            }
        }
    }
}

#endif
