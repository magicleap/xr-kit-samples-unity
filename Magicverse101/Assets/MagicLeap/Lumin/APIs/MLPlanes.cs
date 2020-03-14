// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLPlanes.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Creates planes requests and delegates their result.
    /// </summary>
    public partial class MLPlanes : MLAPISingleton<MLPlanes>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// Stores all pending plane queries.
        /// </summary>
        private readonly Dictionary<ulong, NativeBindings.Query> pendingQueries = new Dictionary<ulong, NativeBindings.Query>();

        /// <summary>
        /// Keeps the queries that were completed on a specific frame.
        /// </summary>
        private Dictionary<ulong, NativeBindings.Query> completedQueries = new Dictionary<ulong, NativeBindings.Query>();

        /// <summary>
        /// Keeps the queries that failed on a specific frame.
        /// </summary>
        private List<ulong> errorQueries = new List<ulong>();

        /// <summary>
        /// Stores the planes system tracker.
        /// </summary>
        private ulong planesTracker = MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// Delegate to handle plane queries results.
        /// </summary>
        /// <param name="result">Result of the query.</param>
        /// <param name="planes">Planes found on the query.</param>
        /// <param name="boundaries">Plane boundaries.</param>
        public delegate void QueryResultsDelegate(MLResult result, Plane[] planes, Boundaries[] boundaries);
        #endif

        /// <summary>
        /// Control flags for plane queries.
        /// </summary>
        [Flags]
        public enum QueryFlags
        {
            /// <summary>
            /// Include planes whose normal is perpendicular to gravity.
            /// </summary>
            Vertical = 1 << 0,

            /// <summary>
            /// Include planes whose normal is parallel to gravity.
            /// </summary>
            Horizontal = 1 << 1,

            /// <summary>
            /// Include planes with arbitrary normal vectors that are not parallel or perpendicular to gravity.
            /// </summary>
            Arbitrary = 1 << 2,

            /// <summary>
            /// Include planes of all possible orientations.
            /// </summary>
            AllOrientations = Vertical | Horizontal | Arbitrary,

            /// <summary>
            /// For non-horizontal planes, setting this flag will result in
            /// the plane rectangle being forced to perpendicular to gravity.
            /// </summary>
            OrientToGravity = 1 << 3,

            /// <summary>
            /// If this flag is set, inner planes will be returned; if it is not set,
            /// outer planes will be returned.
            /// </summary>
            Inner = 1 << 4,

            /// <summary>
            /// Instructs the plane system to ignore holes in planar surfaces.
            /// </summary>
            IgnoreHoles = 1 << 5,

            /// <summary>
            /// Include planes semantically tagged as ceiling.
            /// </summary>
            SemanticCeiling = 1 << 6,

            /// <summary>
            /// Include planes semantically tagged as floor.
            /// </summary>
            SemanticFloor = 1 << 7,

            /// <summary>
            /// Include planes semantically tagged as wall.
            /// </summary>
            SemanticWall = 1 << 8,

            /// <summary>
            /// Include all planes that are semantically tagged.
            /// </summary>
            SemanticAll = SemanticCeiling | SemanticFloor | SemanticWall,

            /// <summary>
            /// If this flag is set, polygons will be returned along with applicable rectangular planes;
            /// if it's not set, only rectangular planes will be returned.
            /// </summary>
            Polygons = 1 << 9
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Gets or sets the default query flags.
        /// </summary>
        public QueryFlags DefaultQueryFlags { get; set; } = QueryFlags.AllOrientations;

        /// <summary>
        /// Starts the MLPlanes API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLPlanes.BaseStart(true);
        }

        /// <summary>
        /// Request real world quad surfaces.
        /// Callback will never be called while request is still pending.
        /// </summary>
        /// <param name="queryParams">All values are required, omitting values may result in unexpected behavior.</param>
        /// <param name="callback">
        /// Callback used to report query results.
        /// Callback MLResult code will never be <c>MLResult.Code.Pending</c>.
        /// </param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        public static MLResult GetPlanes(QueryParams queryParams, QueryResultsDelegate callback)
        {
            if (MLPlanes.IsValidInstance())
            {
                _instance.ValidateQueryParams(ref queryParams);

                // Required flag by the CAPI, has to be set or errors will occur.
                queryParams.Flags |= QueryFlags.Polygons;

                // Don't allow null callbacks to be registered.
                if (callback == null)
                {
                    MLPluginLog.Error("MLPlanes.GetPlanes failed. Reason: Passed input callback is null.");
                    return MLResult.Create(MLResult.Code.InvalidParam);
                }

                return _instance.BeginPlaneQuery(queryParams, callback);
            }
            else
            {
                MLPluginLog.ErrorFormat("MLPlanes.GetPlanes failed. Reason: No Instance for MLPlanes");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPlanes.GetPlanes failed. Reason: No Instance for MLPlanes");
            }
        }

        #if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Starts the plane object requests, Must be called to start receiving plane results from
        /// the underlying system.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        protected override MLResult StartAPI()
        {
            _instance.planesTracker = MagicLeapNativeBindings.InvalidHandle;

            return _instance.CreatePlanesTracker();
        }
        #endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Cleans up unmanaged memory.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Allow complete cleanup of the API.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            if (isSafeToAccessManagedObjects)
            {
                foreach (var query in _instance.pendingQueries.Values)
                {
                    query.Dispose();
                }

                _instance.pendingQueries.Clear();
            }

            _instance.DestroyNativeTracker();
        }

        /// <summary>
        /// Polls for the result of pending planes requests.
        /// </summary>
        protected override void Update()
        {
            _instance.ProcessPendingQueries();
        }

        /// <summary>
        /// static instance of the MLPlanes class
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLPlanes.IsValidInstance())
            {
                MLPlanes._instance = new MLPlanes();
            }
        }

        /// <summary>
        /// Create a new planes native tracker.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        private MLResult CreatePlanesTracker()
        {
            try
            {
                MLResult.Code resultCode = NativeBindings.MLPlanesCreate(ref _instance.planesTracker);
                if (resultCode != MLResult.Code.Ok || !MagicLeapNativeBindings.MLHandleIsValid(_instance.planesTracker))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLPlanes.CreatePlanesTracker failed to initialize native planes tracking. Reason: {0}", result);
                }

                return MLResult.Create(MLResult.Code.Ok);
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLPlanes.CreatePlanesTracker failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPlanes.CreatePlanesTracker failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Destroy the planes native tracker.
        /// </summary>
        private void DestroyNativeTracker()
        {
            try
            {
                if (!MagicLeapNativeBindings.MLHandleIsValid(_instance.planesTracker))
                {
                    return;
                }

                MLResult.Code resultCode = NativeBindings.MLPlanesDestroy(_instance.planesTracker);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLPlanes.DestroyNativeTracker failed to destroy planes tracker. Reason: {0}", NativeBindings.MLGetResultString(resultCode));
                }

                _instance.planesTracker = MagicLeapNativeBindings.InvalidHandle;
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLPlanes.DestroyNativeTracker failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Process pending requests and call the callback specified in the startup config.
        /// </summary>
        private void ProcessPendingQueries()
        {
            try
            {
                if (_instance.pendingQueries.Count > 0)
                {
                    // Process each individual pending query to get updated status.
                    foreach (ulong handle in _instance.pendingQueries.Keys)
                    {
                        NativeBindings.Query query = _instance.pendingQueries[handle];

                        // Request the update.
                        MLResult.Code resultCode = NativeBindings.MLPlanesQueryGetResultsWithBoundaries(_instance.planesTracker, handle, query.PlanesResultsUnmanaged, out uint numResults, ref query.PlaneBoundariesList);

                        // If it is no longer in pending state, continue to process further.
                        if (resultCode != MLResult.Code.Pending)
                        {
                            if (resultCode == MLResult.Code.Ok)
                            {
                                query.ExtractPlanesFromQueryResults(numResults);

                                resultCode = NativeBindings.MLPlanesReleaseBoundariesList(_instance.planesTracker, ref query.PlaneBoundariesList);
                                if (resultCode != MLResult.Code.Ok)
                                {
                                    MLPluginLog.ErrorFormat("MLPlanes.ProcessPendingQueries failed to release boundaries list. Reason: {0}", resultCode);
                                }
                                else
                                {
                                    query.Result = MLResult.Create(resultCode);

                                    if (query.Planes == null)
                                    {
                                        query.Planes = new Plane[] { };
                                    }

                                    if (query.PlaneBoundaries == null)
                                    {
                                        query.PlaneBoundaries = new Boundaries[] { };
                                    }

                                    _instance.completedQueries.Add(handle, query);
                                }
                            }
                            else
                            {
                                MLPluginLog.ErrorFormat("MLPlanes.ProcessPendingQueries failed to query planes. Reason: {0}", resultCode);
                                _instance.errorQueries.Add(handle);
                            }
                        }
                    }

                    foreach (ulong handle in _instance.errorQueries)
                    {
                        _instance.pendingQueries.Remove(handle);
                    }

                    _instance.errorQueries.Clear();

                    foreach (KeyValuePair<ulong, NativeBindings.Query> handle in _instance.completedQueries)
                    {
                        handle.Value.Callback(handle.Value.Result, handle.Value.Planes, handle.Value.PlaneBoundaries);

                        _instance.pendingQueries[handle.Key].Dispose();
                        _instance.pendingQueries.Remove(handle.Key);
                    }

                    _instance.completedQueries.Clear();
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLPlanes.ProcessPendingQueries failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Begin querying for planes.
        /// </summary>
        /// <param name="queryParams">All values are required, omitting values may result in unexpected behavior.</param>
        /// <param name="callback">Callback used to report query results.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        private MLResult BeginPlaneQuery(QueryParams queryParams, QueryResultsDelegate callback)
        {
            try
            {
                if (!NativeBindings.MLHandleIsValid(_instance.planesTracker))
                {
                    MLPluginLog.Error("MLPlanes.BeginPlaneQuery failed to request planes. Reason: Tracker handle is invalid");
                    return MLResult.Create(MLResult.Code.InvalidParam);
                }

                // Convert to native plane query parameters.
                NativeBindings.QueryParamsNative planeQuery = NativeBindings.QueryParamsNative.Create();
                planeQuery.Data = queryParams;

                // Register the query with the native library and store native handle.
                ulong handle = MagicLeapNativeBindings.InvalidHandle;
                MLResult.Code resultCode = NativeBindings.MLPlanesQueryBegin(_instance.planesTracker, ref planeQuery, ref handle);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLPlanes.BeginPlaneQuery failed to request planes. Reason: {0}", result);
                    return result;
                }

                // Create query object to prepresent this newly registered plane query.
                NativeBindings.Query query = new NativeBindings.Query((QueryResultsDelegate)callback, planeQuery.MaxResults, _instance.IsRequestingBoundaries(planeQuery.Flags));
                _instance.pendingQueries.Add(handle, query);
                return MLResult.Create(MLResult.Code.Ok);
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLPlanes.BeginPlaneQuery failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPlanes.BeginPlaneQuery failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Determine if the query flags contain a request for polygon boundaries.
        /// </summary>
        /// <param name="flags">Query flags to check against.</param>
        /// <returns>true if polygons flag was requested and false otherwise.</returns>
        private bool IsRequestingBoundaries(QueryFlags flags)
        {
            // Check if the flag is set to return Polygons (Boundaries).
            return (flags & QueryFlags.Polygons) == QueryFlags.Polygons;
        }

        /// <summary>
        /// Validates that <c>QueryParams</c> passed in are valid and fixes them otherwise.
        /// </summary>
        /// <param name="queryParams">The <c>QueryParams</c> to verify</param>
        private void ValidateQueryParams(ref QueryParams queryParams)
        {
            if (queryParams.BoundsRotation.x == 0 && queryParams.BoundsRotation.y == 0 &&
                queryParams.BoundsRotation.z == 0 && queryParams.BoundsRotation.w == 0)
            {
                MLPluginLog.Warning("MLPlanes.ValidateQueryParams contained an uninitialized Quaternion for BoundsRotation, setting it to (Quaternion.identity).");
                queryParams.BoundsRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Plane Startup Configuration Parameters. Pass these to the Start function
        /// </summary>
        public struct QueryParams
        {
            /// <summary>
            /// The flags to apply to this query.
            /// </summary>
            public QueryFlags Flags;

            /// <summary>
            /// The center of the bounding box which defines where planes extraction should occur.
            /// </summary>
            public Vector3 BoundsCenter;

            /// <summary>
            /// The rotation of the bounding box where planes extraction will occur.
            /// </summary>
            public Quaternion BoundsRotation;

            /// <summary>
            /// The size of the bounding box where planes extraction will occur.
            /// </summary>
            public Vector3 BoundsExtents;

            /// <summary>
            /// The maximum number of results that should be returned.
            /// </summary>
            public uint MaxResults;

            /// <summary>
            /// If MLPlanesQueryFlags.IgnoreHoles is not set, holes with a perimeter
            /// (in meters) smaller than this value will be ignored, and can be part of
            /// the plane. This value cannot be lower than 0 (lower values will be
            /// capped to this minimum).
            /// </summary>
            public float MinHoleLength;

            /// <summary>
            /// The minimum area (in squared meters) of planes to be returned. This value
            /// cannot be lower than 0.04 (lower values will be capped to this minimum).
            /// </summary>
            public float MinPlaneArea;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <returns>New instance of <c>QueryParams</c>.</returns>
            public static QueryParams Create()
            {
                return new QueryParams()
                {
                    Flags = QueryFlags.AllOrientations | QueryFlags.SemanticAll,
                    BoundsCenter = Vector3.zero,
                    BoundsRotation = Quaternion.identity,
                    BoundsExtents = new Vector3(10.0f, 10.0f, 10.0f),
                    MaxResults = 10,
                    MinHoleLength = 0.0f,
                    MinPlaneArea = 0.04f
                };
            }
        }

        /// <summary>
        /// A plane with width and height.
        /// </summary>
        public struct Plane
        {
            /// <summary>
            /// Plane center.
            /// </summary>
            public Vector3 Center;

            /// <summary>
            /// Plane rotation.
            /// </summary>
            public Quaternion Rotation;

            /// <summary>
            /// Plane width.
            /// </summary>
            public float Width;

            /// <summary>
            /// Plane height.
            /// </summary>
            public float Height;

            /// <summary>
            /// Flags which describe this plane.
            /// </summary>
            public uint Flags;

            /// <summary>
            /// Plane ID. All inner planes within an outer plane will have the
            /// same ID(outer plane's ID). These IDs are persistent across
            /// plane queries unless a map merge occurs. On a map merge, IDs
            /// could be different.
            /// </summary>
            public ulong Id;
        }

        /// <summary>
        /// Type to represent multiple regions on a 2D plane.
        /// </summary>
        public struct Boundaries
        {
            /// <summary>
            /// Plane ID, the same value associating to the ID in #MLPlane if they belong to the same plane.
            /// </summary>
            public ulong Id;

            /// <summary>
            /// The boundaries in a plane.
            /// </summary>
            public Boundary[] PlaneBoundaries;
        }

        /// <summary>
        /// Type used to represent a region boundary on a 2D plane.
        /// </summary>
        public struct Boundary
        {
            /// <summary>
            /// The polygon that defines the region, the boundary vertices in MLPolygon will be in CCW order.
            /// </summary>
            public Polygon Polygon;

            /// <summary>
            /// A polygon may contains multiple holes, the boundary vertices in MLPolygon will be in CW order.
            /// </summary>
            public Polygon[] Holes;
        }

        /// <summary>
        /// Coplanar connected line segments representing the outer boundary of a polygon,
        /// an n sided polygon where n is the number of vertices.
        /// </summary>
        public struct Polygon
        {
            /// <summary>
            /// Vertices of all line segments.
            /// </summary>
            public Vector3[] Vertices;
        }
        #endif
    }
}
