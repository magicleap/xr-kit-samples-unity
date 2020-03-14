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
            /// Location request result.
            /// </summary>
            public partial struct LocationNative
            {
                /// <summary>
                /// Gets an easy conversion from the native structure to an external one.
                /// </summary>
                [Obsolete("Please use LocationNative.Data instead.")]
                public MLLocationData DataEx
                {
                    get
                    {
                        MLLocationData location = new MLLocationData();
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
            }
        }
    }
}

#endif
