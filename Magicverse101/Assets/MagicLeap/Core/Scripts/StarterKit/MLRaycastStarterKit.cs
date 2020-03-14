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
    /// Starter kit class for practical use of MLRaycast
    /// </summary>
    public static class MLRaycastStarterKit
    {
        #if PLATFORM_LUMIN
        private static MLRaycast.QueryParams _parameters = new MLRaycast.QueryParams();
        #endif

        #pragma warning disable 649
        private static MLResult _result;
        #pragma warning restore 649

        /// <summary>
        /// Starts up MLRaycast
        /// </summary>
        public static MLResult Start()
        {
            #if PLATFORM_LUMIN
            _result = MLRaycast.Start();
            if (!_result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLRaycastStarterKit failed starting MLRaycast. Reason: {0}", _result);
            }
            #endif

            return _result;
        }

        /// <summary>
        /// Stops MLRaycast if it has been started
        /// </summary>
        public static void Stop()
        {
            #if PLATFORM_LUMIN
            if (MLRaycast.IsStarted)
            {
                MLRaycast.Stop();
            }
            #endif
        }

        #if PLATFORM_LUMIN
        /// <summary>
        // Raycast using full QueryParams object
        /// </summary>
        public static MLResult Raycast(MLRaycast.QueryParams parameters, MLRaycast.OnRaycastResultDelegate callback)
        {
            if (MLRaycast.IsStarted)
            {
                _result = MLRaycast.Raycast(parameters, callback);

                if (!_result.IsOk)
                {
                    Debug.LogErrorFormat("Error: MLRaycastStarterKit.Raycast failed. Reason: {0}", _result);
                }
            }

            else
            {
                Debug.LogError("Error: MLRaycastStarterKit.Raycast failed because MLRaycast was not started.");
                _result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLRaycast was not started");
            }

            return _result;
        }

        /// <summary>
        // Raycast using just rayOrigin, rayDirection, and rayUpwardVector
        /// </summary>
        public static MLResult Raycast(Vector3 rayOrigin, Vector3 rayDirection, Vector3 rayUpwardDirection, MLRaycast.OnRaycastResultDelegate callback)
        {
            _parameters.Position = rayOrigin;
            _parameters.Direction = rayDirection;
            _parameters.UpVector = rayUpwardDirection;
            _parameters.Width = 1;
            _parameters.Height = 1;
            _parameters.HorizontalFovDegrees = 0;
            _parameters.CollideWithUnobserved = false;

            return Raycast(_parameters, callback);
        }

        /// <summary>
        // Raycast using just a transform reference
        /// </summary>
        public static MLResult Raycast(Transform transform, MLRaycast.OnRaycastResultDelegate callback)
        {
            _parameters.Position = transform.position;
            _parameters.Direction = transform.forward;
            _parameters.UpVector = transform.up;
            _parameters.Width = 1;
            _parameters.Height = 1;
            _parameters.HorizontalFovDegrees = 0;
            _parameters.CollideWithUnobserved = false;

            return Raycast(_parameters, callback);
        }
        #endif
    }
}
