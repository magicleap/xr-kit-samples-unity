// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLLocationData.cs" company="Magic Leap, Inc">
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
    /// Location request result.
    /// </summary>
    [Obsolete("Please use MLLocation.Location instead.")]
    public struct MLLocationData
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
        /// Used to determine the validity of MLLocationData.
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
        /// Used to determine the accuracy of MLLocationData.
        /// </summary>
        public bool HasAccuracy;

        /// <summary>
        /// The equality check to be used for comparing two MLImageTracker.Settings structs.
        /// </summary>
        /// <param name="one">The first struct to compare with the second struct. </param>
        /// <param name="two">The second struct to compare with the first struct. </param>
        /// <returns>True if the two provided structs have the same field values.</returns>
        public static bool operator ==(MLLocationData one, MLLocationData two)
        {
            return one.Equals(two);
        }

        /// <summary>
        /// The inequality check to be used for comparing two MLImageTracker.Settings structs.
        /// </summary>
        /// <param name="one">The first struct to compare with the second struct. </param>
        /// <param name="two">The second struct to compare with the first struct. </param>
        /// <returns>True if the two provided structs do not have the same field values.</returns>
        public static bool operator !=(MLLocationData one, MLLocationData two)
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
            if (obj is MLLocationData)
            {
                MLLocationData c = (MLLocationData)obj;
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

#endif
