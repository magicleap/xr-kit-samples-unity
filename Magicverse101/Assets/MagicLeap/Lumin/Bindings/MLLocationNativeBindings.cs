// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLLocationNativeBindings.cs" company="Magic Leap, Inc">
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
    /// Currently location is only received on wi-fi connect. If connection is lost, a new location will be
    /// provided on connect.
    /// </summary>
    public sealed partial class MLLocation : MLAPISingleton<MLLocation>
    {
        /// <summary>
        /// See ml_location.h for additional comments.
        /// </summary>
        private partial class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// Maximum size that a given postal code can be.
            /// </summary>
            private const int MaxPostalCodeSize = 64;

            /// <summary>
            /// MLLocation library name.
            /// </summary>
            private const string MLLocationDLL = "ml_location";

            /// <summary>
            /// Mask values to determine the validity of MLLocation.Location data.
            /// </summary>
            [Flags]
            public enum LocationMask : uint
            {
                /// <summary>
                /// Mask value that confirms a postal code is available.
                /// </summary>
                HasPostalCode = 1 << 0,

                /// <summary>
                /// Mask value that confirms an estimated accuracy radius is available.
                /// </summary>
                HasAccuracy = 1 << 1
            }

            /// <summary>
            /// Internal call for querying the last known coarse location.
            /// Coarse location provides latitude and longitude estimate from block to city accuracy.
            /// Returns the last known coarse location data on success and returns the last queried coarse location data on failure. Latitude and Longitude of
            /// 0 should be assumed an Invalid Location.
            /// </summary>
            /// <param name="outLocation">Pointer for where to store the last known coarse location. Only updates when getting the location succeeds. Latitude and Longitude of 0 should be assumed an Invalid Location.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the location was queried successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid location.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privilege(s).
            /// MLResult.Result will be <c>MLResult.Code.LocationProviderNotFound</c> if there was no provider or an invalid request was made.
            /// MLResult.Result will be <c>MLResult.Code.LocationNetworkConnection</c> if there was no internet connection.
            /// MLResult.Result will be <c>MLResult.Code.LocationNoLocation</c> if the location could not be found.
            /// </returns>
            [DllImport(MLLocationDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLLocationGetLastCoarseLocation(IntPtr outLocation);

            /// <summary>
            /// This call returns strings for all of the MLResult codes having to do with location.
            /// Developers should be using MLResult.CodeToString(MLResult.Code) to get string values of MLResult codes.
            /// </summary>
            /// <param name="result">The MLResult.Code to turn into a string.</param>
            /// <returns>
            /// A pointer to the ASCII string containing readable version of result code.
            /// </returns>
            [DllImport(MLLocationDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MLLocationGetResultString(MLResult.Code result);

            /// <summary>
            /// External call for querying the last known fine location.
            /// The accuracy field of the MLLocation.Location provides the estimate accuracy radius in meters.
            /// Returns the last known fine location data on success and returns the last queried fine location data on failure. Latitude and Longitude of
            /// 0 should be assumed an Invalid Location.
            /// </summary>
            /// <param name="outLocation">Pointer for where to store the last known fine location. Only updates when getting the location succeeds. Latitude and Longitude of 0 should be assumed an Invalid Location.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the location was queried successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid location.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privilege(s).
            /// MLResult.Result will be <c>MLResult.Code.LocationProviderNotFound</c> if there was no provider or an invalid request was made.
            /// MLResult.Result will be <c>MLResult.Code.LocationNetworkConnection</c> if there was no internet connection.
            /// MLResult.Result will be <c>MLResult.Code.LocationNoLocation</c> if the location could not be found.
            /// </returns>
            [DllImport(MLLocationDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLLocationGetLastFineLocation(IntPtr outLocation);

            /// <summary>
            /// Call for querying the last known coarse/fine location.
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
            public static MLResult MLLocationGetLastLocation(out MLLocation.Location outLocation, bool isFine)
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
                        MLPluginLog.ErrorFormat("MLLocation.NativeBindings.MLLocationGetLastLocation failed to get location. Reason: {0}", resultCode);
                    }
                }
                catch (EntryPointNotFoundException)
                {
                    MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLLocation API symbols not found.");
                    MLPluginLog.ErrorFormat("MLLocation.NativeBindings.MLLocationGetLastLocation failed to get location. Reason: {0}", result);
                    outLocation = new MLLocation.Location();
                    Marshal.FreeHGlobal(myPointer);
                    return result;
                }

                location = (NativeBindings.LocationNative)Marshal.PtrToStructure(myPointer, typeof(NativeBindings.LocationNative));

                Marshal.FreeHGlobal(myPointer);

                MLResult finalResult = MLResult.Create(resultCode);

                outLocation = location.Data;

                return finalResult;
            }

            /// <summary>
            /// Location request result.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public partial struct LocationNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                [MarshalAs(UnmanagedType.U4)]
                public uint Version;

                /// <summary>
                /// Location latitude.
                /// </summary>
                [MarshalAs(UnmanagedType.R4)]
                public float Latitude;

                /// <summary>
                /// Location longitude.
                /// </summary>
                [MarshalAs(UnmanagedType.R4)]
                public float Longitude;

                /// <summary>
                /// Location mask value.
                /// </summary>
                [MarshalAs(UnmanagedType.U4)]
                public LocationMask Mask;

                /// <summary>
                /// Approximate postal code.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxPostalCodeSize)]
                public string PostalCode;

                /// <summary>
                /// Epoch timestamp in milliseconds of the location data.
                /// </summary>
                [MarshalAs(UnmanagedType.U8)]
                public ulong Timestamp;

                /// <summary>
                /// The radius in meters.
                /// </summary>
                [MarshalAs(UnmanagedType.R4)]
                public float Accuracy;

                /// <summary>
                /// Gets an easy conversion from the native structure to an external one.
                /// </summary>
                public MLLocation.Location Data
                {
                    get
                    {
                        MLLocation.Location location = new MLLocation.Location();
                        location.Latitude = this.Latitude;
                        location.Longitude = this.Longitude;
                        location.PostalCode = this.PostalCode;
                        location.HasPostalCode = this.Mask.HasFlag(NativeBindings.LocationMask.HasPostalCode);
                        location.Timestamp = this.Timestamp;
                        location.Accuracy = this.Accuracy;
                        location.HasAccuracy = this.Mask.HasFlag(NativeBindings.LocationMask.HasAccuracy);

                        return location;
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static LocationNative Create()
                {
                    return new LocationNative()
                    {
                        Version = 2u,
                        Latitude = 0.0f,
                        Longitude = 0.0f,
                        Mask = 0u,
                        PostalCode = string.Empty,
                        Timestamp = 0ul,
                        Accuracy = 0.0f
                    };
                }
            }
        }
    }
}

#endif
