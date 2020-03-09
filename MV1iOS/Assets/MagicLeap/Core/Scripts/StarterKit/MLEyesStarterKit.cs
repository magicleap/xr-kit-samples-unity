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

using System;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Core.StarterKit
{
    /// <summary>
    /// Starter kit class for practical use of MLEyes
    /// </summary>
    public static class MLEyesStarterKit
    {
        #pragma warning disable 414, 649
        private static MLResult _result;
        #pragma warning restore 414, 649

        /// <summary>
        // Gets the direction the user is looking at
        /// </summary>
        public static Vector3 GazeDirection
        {
            get
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    return (FixationPoint - mainCamera.transform.position).normalized;
                }

                else
                {
                    Debug.LogError("Error: MLEyesStarterKit.GazeDirection failed because _mainCamera is null.");
                    return Vector3.zero;
                }

            }
        }

        /// <summary>
        // Gets the point that the user is looking at
        /// </summary>
        public static Vector3 FixationPoint
        {
            get
            {
                #if PLATFORM_LUMIN
                if (MLEyes.IsStarted)
                {
                    return MLEyes.FixationPoint;
                }
                else
                {
                    Debug.LogError("Error: MLEyesStarterKit.FixationPoint failed because MLEyes was not started.");
                    return Vector3.zero;
                }
                #else
                return Vector3.zero;
                #endif
            }
        }

        /// <summary>
        // Gets the string value of the current eye calibration status
        /// </summary>
        public static string CalibrationStatus
        {
            get
            {
                #if PLATFORM_LUMIN
                if (MLEyes.IsStarted)
                {
                    return MLEyes.CalibrationStatus.ToString();
                }
                else
                {
                    Debug.LogError("Error: MLEyesStarterKit.CalibrationStatus failed because MLEyes was not started.");
                    return "";
                }
                #else
                return string.Empty;
                #endif
            }
        }

        /// <summary>
        // Starts up MLEyes
        /// </summary>
        public static MLResult Start()
        {
            #if PLATFORM_LUMIN
            _result = MLEyes.Start();

            if (!_result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLEyesStarterKit failed starting MLEyes. Reason: {0}", _result);
            }
            #endif

            return _result;
        }

        /// <summary>
        /// Stops MLEyes if it has been started
        /// </summary>
        public static void Stop()
        {
            #if PLATFORM_LUMIN
            if (MLEyes.IsStarted)
            {
                MLEyes.Stop();
            }
            #endif
        }
    }
}
