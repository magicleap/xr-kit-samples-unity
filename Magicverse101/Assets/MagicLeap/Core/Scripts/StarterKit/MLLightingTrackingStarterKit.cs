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
    /// Starter kit class for MLLightingTracking.
    /// </summary>
    public static class MLLightingTrackingStarterKit
    {
        public const float LUMINANCE_MIN = 0.0f;
        public const float LUMINANCE_MAX = 8.0f;

        /// <summary>
        /// Gets the Temperature Color.
        /// </summary>
        public static Color TemperatureColor
        {
            get
            {
                #if PLATFORM_LUMIN
                if (MLLightingTracking.IsStarted)
                {
                    return MLLightingTracking.GlobalTemperatureColor;
                }
                else
                {
                    Debug.LogErrorFormat("Error: MLLightingTrackingStarterKit.TemperatureColor failed because MLLightingTracking was not started.");
                    return Color.black;
                }
                #else
                return Color.black;
                #endif
            }
        }

        /// <summary>
        /// Gets the Normalized Luminance.
        /// </summary>
        public static float NormalizedLuminance
        {
            get
            {
                #if PLATFORM_LUMIN
                if (MLLightingTracking.IsStarted)
                {
                    return (float)(System.Math.Min(System.Math.Max((double)MLLightingTracking.AverageLuminance, LUMINANCE_MIN), LUMINANCE_MAX) / LUMINANCE_MAX);
                }
                else
                {
                    Debug.LogErrorFormat("Error: MLLightingTrackingStarterKit.NormalizedLuminance failed because MLLightingTracking was not started.");
                    return LUMINANCE_MIN;
                }
                #else
                return LUMINANCE_MIN;
                #endif
            }
        }

        /// <summary>
        /// Start the Lighting Tracker API.
        /// </summary>
        public static MLResult Start()
        {
            #if PLATFORM_LUMIN
            MLResult result =  MLLightingTracking.Start();

            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLLightingTrackingStarterKit failed to start MLLightingTracking. Reason: {0}", result);
            }

            return result;
            #else
            return new MLResult();
            #endif
        }

        /// <summary>
        /// Stops the Lighting Tracker API.
        /// </summary>
        public static void Stop()
        {
            #if PLATFORM_LUMIN
            if (MLLightingTracking.IsStarted)
            {
                MLLightingTracking.Stop();
            }
            #endif
        }
    }
}
