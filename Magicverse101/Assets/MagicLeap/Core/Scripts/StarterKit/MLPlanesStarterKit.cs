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
    /// Starter kit class for practical use of MLPlanes.
    /// </summary>
    public static class MLPlanesStarterKit
    {
        /// <summary>
        /// Use this to determine when you are allowed to Query for planes again.
        /// </summary>
        public static bool isQuerying
        {
            get;
            private set;
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Used to set isQuerying to false.
        /// </summary>
        private static MLPlanes.QueryResultsDelegate _queryCallback = (MLResult result, MLPlanes.Plane[] planes, MLPlanes.Boundaries[] boundaries) => { isQuerying = false; };

        private static MLPlanes.QueryParams _parameters = new MLPlanes.QueryParams();

        private static MLResult _result;

        /// <summary>
        /// Starts up MLPlanes.
        /// </summary>
        public static MLResult Start()
        {
            _result = MLPlanes.Start();
            if (!_result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLPlanesStarterKit failed starting MLPlanes. Reason: {0}", _result);
            }
            return _result;
        }
        #endif

        /// <summary>
        /// Stops MLPlanes if it has been started.
        /// </summary>
        public static void Stop()
        {
            #if PLATFORM_LUMIN
            if (MLPlanes.IsStarted)
            {
                MLPlanes.Stop();
            }
            #endif
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Utility function used to determine if some MLPlanes.Plane object contains a certain flag.
        /// </summary>
        /// <param name="plane">The MLPlanes.Plane object to be checked.</param>
        /// <param name="flag">The MLPlanes.QueryFlags to be checked.</param>
        public static bool DoesPlaneHaveFlag(MLPlanes.Plane plane, MLPlanes.QueryFlags flag)
        {
            return (plane.Flags & (uint)flag) == (uint)flag;
        }

        /// <summary>
        /// Function used to query for planes present in the real world.
        /// </summary>
        /// <param name="parameters">The parameters to use for this query.</param>
        /// <param name="callback">The function to call when the query is done.</param>
        public static MLResult QueryPlanes(MLPlanes.QueryParams parameters, MLPlanes.QueryResultsDelegate callback)
        {
            if (MLPlanes.IsStarted)
            {
                if (isQuerying)
                {
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "A previous query is still in progress.");
                }

                // Required flag by the CAPI, has to be set or errors will occur.
                parameters.Flags |= MLPlanes.QueryFlags.Polygons;

                // Planes can't have MinHoleLength less than 0.0.
                parameters.MinHoleLength = Mathf.Clamp(parameters.MinHoleLength, 0.0f, parameters.MinHoleLength);

                // Planes can't have MinPlaneArea less than 0.04.
                parameters.MinPlaneArea = Mathf.Clamp(parameters.MinHoleLength, 0.04f, parameters.MinPlaneArea);

                callback += _queryCallback;
                _result = MLPlanes.GetPlanes(parameters, callback);
                isQuerying = _result.IsOk;

                if (!_result.IsOk)
                {
                    callback = null;
                    Debug.LogErrorFormat("Error: MLPlanesStarterKit.QueryPlanes failed. Reason: {0}", _result);
                }
            }

            else
            {
                Debug.LogError("Error: MLPlanesStarterKit.QueryPlanes failed because MLPlanes was not started.");
                _result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPlanes was not started");
            }

            return _result;
        }

        /// <summary>
        /// Function used to call QueryPlanes with just a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to use for the query parameters.</param>
        /// <param name="callback">The function to call when the query is done.</param>
        public static MLResult QueryPlanes(GameObject gameObject, MLPlanes.QueryResultsDelegate callback)
        {
            if(gameObject == null)
            {
                Debug.LogError("MLPlanesStarterKit.QueryPlanes failed because the gameObject parameter was null.");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "gameObject parameter was null");
            }

            _parameters.BoundsCenter = gameObject.transform.position;
            _parameters.BoundsRotation = gameObject.transform.rotation;
            _parameters.BoundsExtents = gameObject.transform.localScale;
            _parameters.MaxResults = 512;
            _parameters.MinHoleLength = 0.0f;
            _parameters.MinPlaneArea = 0.04f;
            _parameters.Flags = MLPlanes.QueryFlags.SemanticAll | MLPlanes.QueryFlags.AllOrientations;

            return QueryPlanes(_parameters, callback);
        }

        /// <summary>
        /// QueryPlanes by using boundsCenter, boundsRotation, and boundsExtents.
        /// </summary>
        /// <param name="boundsCenter">The center position to use for the bounding box which defines where planes extraction should occur.</param>
        /// <param name="boundsRotation">The rotation to use for the bounding box where planes extraction will occur.</param>
        /// <param name="boundsExtents">The size to use for the bounding box where planes extraction will occur.</param>
        public static MLResult QueryPlanes(Vector3 boundsCenter, Quaternion boundsRotation, Vector3 boundsExtents, MLPlanes.QueryResultsDelegate callback)
        {
            _parameters.BoundsCenter = boundsCenter;
            _parameters.BoundsRotation = boundsRotation;
            _parameters.BoundsExtents = boundsExtents;
            _parameters.MaxResults = 512;
            _parameters.MinHoleLength = 0.0f;
            _parameters.MinPlaneArea = 0.04f;
            _parameters.Flags = MLPlanes.QueryFlags.SemanticAll | MLPlanes.QueryFlags.AllOrientations;

            return QueryPlanes(_parameters, callback);
        }
        #endif
    }
}
