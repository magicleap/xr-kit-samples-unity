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

    /// <summary>
    /// MLLocation provides device location data including; Latitude, Longitude, and approximate Postal Code.
    /// Currently location is only received on wi-fi connect. If connection is lost, a new location will be
    /// provided on connect.
    /// </summary>
    public sealed partial class MLLocation : MLAPISingleton<MLLocation>
    {
        /// <summary>
        /// Cached reference to the last successfully queried coarse location.
        /// </summary>
        private Location? lastValidCoarseLocation = null;

        /// <summary>
        /// Cached reference to the last successfully queried fine location.
        /// </summary>
        private Location? lastValidFineLocation = null;

        /// <summary>
        /// Prevents a default instance of the <see cref="MLLocation" /> class from being created.
        /// </summary>
        private MLLocation()
        {
            this.DllNotFoundError = "MLLocation API is currently available only on device.";
        }

        /// <summary>
        /// Starts the MLLocation API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLLocation.BaseStart();
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
        public static MLResult GetLastFineLocation(out Location fineLocation)
        {
            try
            {
                if (MLLocation.IsValidInstance())
                {
                    MLResult result = NativeBindings.MLLocationGetLastLocation(out fineLocation, true);

                    if (result.IsOk)
                    {
                        _instance.lastValidFineLocation = fineLocation;
                    }
                    else
                    {
                        MLPluginLog.ErrorFormat("MLLocation.GetLastFineLocation failed to get location. Reason: {0}", result);
                    }

                    return result;
                }
                else
                {
                    fineLocation = new Location();
                    MLPluginLog.ErrorFormat("MLLocation.GetLastFineLocation failed. Reason: No Instance for MLLocation.");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLLocation.GetLastFineLocation failed. Reason: No Instance for MLLocation.");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                fineLocation = new Location();
                MLPluginLog.Error("MLLocation.GetLastFineLocation failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLLocation.GetLastFineLocation failed. Reason: API symbols not found.");
            }
        }

        /// <summary>
        /// External call for querying the last known coarse location.
        /// Coarse location provides latitude and longitude estimate from block to city accuracy.
        /// Returns the last known coarse location data on success and returns the last queried coarse location data on failure. Latitude and Longitude of
        /// 0 should be assumed an Invalid Location.
        /// </summary>
        /// <param name="coarseLocation">Where to store the last known coarse location. Only updates when getting the location succeeds. Latitude and Longitude of 0 should be assumed an Invalid Location.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the location was queried successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid location.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privilege(s).
        /// MLResult.Result will be <c>MLResult.Code.LocationProviderNotFound</c> if there was no provider or an invalid request was made.
        /// MLResult.Result will be <c>MLResult.Code.LocationNetworkConnection</c> if there was no internet connection.
        /// MLResult.Result will be <c>MLResult.Code.LocationNoLocation</c> if the location could not be found.
        /// </returns>
        public static MLResult GetLastCoarseLocation(out Location coarseLocation)
        {
            try
            {
                if (MLLocation.IsValidInstance())
                {
                    MLResult result = NativeBindings.MLLocationGetLastLocation(out coarseLocation, false);

                    if (result.IsOk)
                    {
                        _instance.lastValidCoarseLocation = coarseLocation;
                    }
                    else
                    {
                        MLPluginLog.ErrorFormat("MLLocation.GetLastCoarseLocation failed to get location. Reason: {0}", result);
                    }

                    return result;
                }
                else
                {
                    coarseLocation = new Location();
                    MLPluginLog.ErrorFormat("MLLocation.GetLastCoarseLocation failed. Reason: No Instance for MLLocation.");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLLocation.GetLastCoarseLocations failed. Reason: No Instance for MLLocation.");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                coarseLocation = new Location();
                MLPluginLog.Error("MLLocation.GetLastCoarseLocation failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLLocation.GetLastCoarseLocation failed. Reason: API symbols not found.");
            }
        }

        /// <summary>
        /// Gets the result string for a MLResult.Code.
        /// </summary>
        /// <param name="result">The MLResult.Code to be requested.</param>
        /// <returns>A pointer to the result string.</returns>
        internal static IntPtr GetResultString(MLResult.Code result)
        {
            try
            {
                if (MLLocation.IsValidInstance())
                {
                    return NativeBindings.MLLocationGetResultString(result);
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLLocation.GetResultString failed. Reason: No Instance for MLLocation.");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLLocation.GetResultString failed. Reason: API symbols not found");
            }

            return IntPtr.Zero;
        }

        #if !DOXYGENSHOULDSKIPTHIS
        /// <summary>
        /// Start MLLocation API and set up callbacks.
        /// </summary>
        /// <returns>MLResult.Result will be <c>MLResult.Code.Ok</c>.</returns>
        protected override MLResult StartAPI()
        {
            return MLResult.Create(MLResult.Code.Ok);
        }
        #endif // DOXYGENSHOULDSKIPTHIS

        /// <summary>
        /// Called by MLAPISingleton on destruction.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Determines if any managed objects should be disposed of or not.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
        }

        /// <summary>
        /// Called every device frame.
        /// </summary>
        protected override void Update()
        {
        }

        /// <summary>
        /// Static instance of the MLLocation class
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLLocation.IsValidInstance())
            {
                MLLocation._instance = new MLLocation();
            }
        }

        /// <summary>
        /// Location request result.
        /// </summary>
        public struct Location
        {
            /// <summary>
            /// Location latitude.
            /// </summary>
            public float Latitude;

            /// <summary>
            /// Location longitude.
            /// </summary>
            public float Longitude;

            /// <summary>
            /// Approximate postal code.
            /// </summary>
            public string PostalCode;

            /// <summary>
            /// Used to determine the validity of Location.
            /// </summary>
            public bool HasPostalCode;

            /// <summary>
            /// Epoch timestamp in milliseconds of the location data.
            /// </summary>
            public ulong Timestamp;

            /// <summary>
            /// The radius in meters.
            /// </summary>
            public float Accuracy;

            /// <summary>
            /// Used to determine the accuracy of Location.
            /// </summary>
            public bool HasAccuracy;

            /// <summary>
            /// The equality check to be used for comparing two MLImageTracker.Settings structs.
            /// </summary>
            /// <param name="one">The first struct to compare with the second struct. </param>
            /// <param name="two">The second struct to compare with the first struct. </param>
            /// <returns>True if the two provided structs have the same field values.</returns>
            public static bool operator ==(Location one, Location two)
            {
                return one.Equals(two);
            }

            /// <summary>
            /// The inequality check to be used for comparing two MLImageTracker.Settings structs.
            /// </summary>
            /// <param name="one">The first struct to compare with the second struct. </param>
            /// <param name="two">The second struct to compare with the first struct. </param>
            /// <returns>True if the two provided structs do not have the same field values.</returns>
            public static bool operator !=(Location one, Location two)
            {
                return !(one == two);
            }

            /// <summary>
            /// The equality check to be used for comparing another object to this one.
            /// </summary>
            /// <param name="obj">The object to compare to this one with. </param>
            /// <returns>True if the the provided object is of the MLLocation.Location type and has matching field values.</returns>
            public override bool Equals(object obj)
            {
                if (obj is Location)
                {
                    Location c = (Location)obj;
                    return this.Latitude == c.Latitude && this.Longitude == c.Longitude && this.PostalCode == c.PostalCode && this.HasPostalCode == c.HasPostalCode && this.Timestamp == c.Timestamp && this.Accuracy == c.Accuracy && this.HasAccuracy == c.HasAccuracy;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Gets the hash code to use from MaxSimultaneousTargets.
            /// </summary>
            /// <returns>The hash code returned by XORing the hash codes of all the fields.</returns>
            public override int GetHashCode()
            {
                return this.Latitude.GetHashCode() ^ this.Longitude.GetHashCode() ^ this.PostalCode.GetHashCode() ^ this.HasPostalCode.GetHashCode() ^ this.HasAccuracy.GetHashCode() ^ this.Timestamp.GetHashCode() ^ this.Accuracy.GetHashCode();
            }

            /// <summary>
            /// ToString method for turning this struct into a readable format.
            /// </summary>
            /// <returns>A string with all of the struct fields and values.</returns>
            public override string ToString()
            {
                return string.Format("Latitude: {0}, Longitude: {1}, PostalCode: {2}, HasPostalCode: {3}, Timestamp: {4}, Accuracy: {5}, HasAccuracy: {6}", this.Latitude, this.Longitude, this.PostalCode, this.HasPostalCode, this.Timestamp, this.Accuracy, this.HasAccuracy);
            }
        }
    }
}

#endif
