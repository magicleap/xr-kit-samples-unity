// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://auth.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Core.StarterKit
{
    /// <summary>
    /// Starter kit class for MLLocation.
    /// </summary>
    public static class MLLocationStarterKit
    {
        /// <summary>
        /// Start the MLLocation API.
        /// </summary>
       public static MLResult Start()
        {
            MLResult result = MLPrivilegesStarterKit.Start();

            #if PLATFORM_LUMIN
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLLocationStarterKit failed starting MLPrivilegesStarterKit, Reason {0} ", result);
                return result;
            }
            result = MLPrivilegesStarterKit.RequestPrivileges(MLPrivileges.Id.FineLocation, MLPrivileges.Id.CoarseLocation);

            if (result.Result != MLResult.Code.PrivilegeGranted)
            {
                Debug.LogErrorFormat("Error: MLLocationStarterKit failed requesting privileges, reason {0} ", result);
                return result;
            }

            MLPrivilegesStarterKit.Stop();

            result = MLLocation.Start();

            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLLocationStarterKit failed to start, Reason: {0}", result);
            }
            #endif

            return result;
        }

        /// <summary>
        /// Start the MLLocation API.
        /// </summary>
        public static void Stop()
        {
            #if PLATFORM_LUMIN
            if(MLLocation.IsStarted)
            {
                MLLocation.Stop();
            }
            #endif
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Gets the last valid location information.
        /// </summary>
        /// <param name="newLocation"></param>
        /// <param name="useFineLocation"></param>
        /// <returns></returns>
        public static MLResult GetLocation( out MLLocation.Location newLocation, bool useFineLocation = false)
        {
            MLResult result = useFineLocation ? MLLocation.GetLastFineLocation(out MLLocation.Location lastLocation) : MLLocation.GetLastCoarseLocation(out lastLocation);          
            newLocation = lastLocation;
            return result;
        }
        #endif

        /// <summary>
        /// Convert the given latitude and longitude to a Vector3
        /// This converts spherical coordinates to cartesian coordinates
        /// </summary>
        /// <param name="lat">Latitude of the given spherical coordinates</param>
        /// <param name="lon">Longitude of the given spherical coordinates</param>
        /// <returns>Vector3 of cartesian coordinates</returns>
        public static Vector3 GetWorldCartesianCoords(float lat, float lon, float radius)
        {
            // Convert Latitude and Longitude to radians
            float latitude = Mathf.Deg2Rad * lat;
            float longitude = Mathf.Deg2Rad * lon;
            // adjust position by radians
            latitude -= 1.570795765134f; // subtract 90 degrees (in radians)

            // and switch z and y (since z is forward)
            float xPos = (radius) * Mathf.Sin(latitude) * Mathf.Cos(longitude);
            float zPos = (radius) * Mathf.Sin(latitude) * Mathf.Sin(longitude);
            float yPos = (radius) * Mathf.Cos(latitude);

            return new Vector3(xPos, yPos, zPos);
        }
    }
}
