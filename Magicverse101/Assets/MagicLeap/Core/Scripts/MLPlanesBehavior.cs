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
using MagicLeap.Core.StarterKit;

namespace MagicLeap.Core
{
    /// <summary>
    /// MLPlanesBehavior encapsulates the functionality to query for planes present in the world.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/MLPlanesBehavior")]
    public class MLPlanesBehavior : MonoBehaviour
    {
        /// <summary>
        /// Flags that determine the what type of plane are included inside the query result.
        /// OrientationFlags are based on if a plane's normal is parallel (horizontal) or perpendicular (vertical) to gravity.
        /// </summary>
        [Flags]
        public enum OrientationFlags
        {
            Vertical = MLPlanes.QueryFlags.Vertical,
            Horizontal = MLPlanes.QueryFlags.Horizontal,
            Arbitrary = MLPlanes.QueryFlags.Arbitrary
        }

        /// <summary>
        /// Flags that determine the what type of plane are included inside the query result.
        /// SemanticFlags are based on what a plane is semantically tagged as.
        /// </summary>
        [Flags]
        public enum SemanticFlags
        {
            Ceiling = MLPlanes.QueryFlags.SemanticCeiling,
            Floor = MLPlanes.QueryFlags.SemanticFloor,
            Wall = MLPlanes.QueryFlags.SemanticWall,
        }

        /// <summary>
        /// Flags that determine the what type of plane are included inside the query result.
        /// SystemFlags are based on properties that the WorldPlanes system will abide by when querying.
        /// </summary>
        [Flags]
        public enum SystemFlags
        {
            // If this flag is set, inner planes will be returned; if it is not set, outer planes will be returned.
            InnerPlanes = MLPlanes.QueryFlags.Inner,
            // Instructs the plane system to ignore holes in planar surfaces.
            IgnoreHoles = MLPlanes.QueryFlags.IgnoreHoles,
            // For non-horizontal planes, setting this flag will result in the plane rectangle being forced to perpendicular to gravity.
            OrientToGravity = MLPlanes.QueryFlags.OrientToGravity,
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// The latest result from the last query to MLPlanesStarterKit.
        /// All planes in the array are ordered from largest first based on it's surface area.
        /// </summary>
        public MLPlanes.Plane[] PlanesResult { get; private set; }
        #endif

        [Header("Query Parameters")]
        [Tooltip("Maximum number of planes to get at the same time.")]
        public uint MaxPlaneCount = 512;

        [SerializeField, MLBitMask(typeof(SemanticFlags)), Space]
        public SemanticFlags semanticFlags;

        [SerializeField, MLBitMask(typeof(OrientationFlags))]
        public OrientationFlags orientationFlags;

        [SerializeField, MLBitMask(typeof(SystemFlags))]
        public SystemFlags systemFlags;

        [Space, Tooltip("Minimum area of searched for planes.")]
        public float minPlaneArea = 0.04f;

        [Tooltip("Perimeter until which to ignore holes and include in plane if IgnoreHoles is not set")]
        public float minHoleLength = 0.0f;

        /// <summary>
        /// Cached query flags.
        /// </summary>
        private MLPlanes.QueryFlags _queryFlags  = MLPlanes.QueryFlags.Vertical;

        #if PLATFORM_LUMIN
        /// <summary>
        /// Cached query parameters.
        /// </summary>
        private MLPlanes.QueryParams _queryParams = new MLPlanes.QueryParams();

        public delegate void QueryPlanesResult(MLPlanes.Plane[] planes, MLPlanes.Boundaries[] boundaries);

        /// <summary>
        /// Event for when a query has completed.
        /// </summary>
        public event QueryPlanesResult OnQueryPlanesResult;
        #endif

        /// <summary>
        /// Validates data after it has been changed in the inspector.
        /// </summary>
        void OnValidate()
        {
            if (minHoleLength < 0.0f)
            {
                Debug.LogWarning("Can't have MinHoleLength less than 0.0f, setting back to default.");
                minHoleLength = 0.0f;
            }
            if (minPlaneArea < 0.04f)
            {
                Debug.LogWarning("Can't have MinPlaneArea less than 0.04, setting back to default.");
                minPlaneArea = 0.04f;
            }
        }

        /// <summary>
        /// Starts up MLPlanesStarterKit.
        /// </summary>
        void Start()
        {
            #if PLATFORM_LUMIN
            MLPlanesStarterKit.Start();
            #endif
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        void OnDestroy()
        {
            MLPlanesStarterKit.Stop();
        }

        /// <summary>
        /// Updates PlanesResult based on the query results.
        /// </summary>
        void Update()
        {
            if(!MLPlanesStarterKit.isQuerying)
            {
                QueryPlanes();
            }
        }

        /// <summary>
        /// Queries for planes via MLPlanesStarterKit with all of the set query flags and parameters
        /// and sets the PlanesResult[] when finished. Based on the query flags that
        /// are passed in, extraction and calculation times may vary.
        /// </summary>
        private void QueryPlanes()
        {
            // Construct flag data.
            _queryFlags = (MLPlanes.QueryFlags)orientationFlags;
            _queryFlags |= (MLPlanes.QueryFlags)semanticFlags;
            _queryFlags |= (MLPlanes.QueryFlags)systemFlags;

            #if PLATFORM_LUMIN
            _queryParams.Flags = _queryFlags;
            _queryParams.BoundsCenter = transform.position;
            _queryParams.MaxResults = MaxPlaneCount;
            _queryParams.BoundsExtents = transform.localScale;
            _queryParams.BoundsRotation = transform.rotation;
            _queryParams.MinHoleLength = minHoleLength;
            _queryParams.MinPlaneArea = minPlaneArea;

            MLPlanesStarterKit.QueryPlanes(_queryParams, HandleOnQueriedPlanes);
            #endif
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Handles the result that is recieved from  MLPlanesStarterKit.QueryPlanes.
        /// <param name="result">The resulting status of the query.</param>
        /// <param name="planes">The planes recieved from the query.</param>
        /// <param name="boundaries">The boundaries recieved from the query.</param>
        /// </summary>
        private void HandleOnQueriedPlanes(MLResult result, MLPlanes.Plane[] planes, MLPlanes.Boundaries[] boundaries)
        {
            if (result.IsOk)
            {
                PlanesResult = planes;
                OnQueryPlanesResult?.Invoke(planes, boundaries);
            }
            else
            {
                Debug.LogErrorFormat("Error: Planes failed to query  MLPlanes. Reason: {0}", result);
            }
        }
        #endif
    }
}
