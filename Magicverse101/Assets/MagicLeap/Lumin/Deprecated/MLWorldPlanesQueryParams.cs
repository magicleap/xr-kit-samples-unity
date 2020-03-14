// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLWorldPlanesQueryParams.cs" company="Magic Leap, Inc">
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
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// Control flags for plane queries.
    /// </summary>
    [Obsolete("Please use MLPlanes.QueryFlags instead.")]
    [Flags]
    public enum MLWorldPlanesQueryFlags
    {
        /// <summary>
        /// Include planes whose normal is perpendicular to gravity.
        /// </summary>
        Vertical = MLPlanes.QueryFlags.Vertical,

        /// <summary>
        /// Include planes whose normal is parallel to gravity.
        /// </summary>
        Horizontal = MLPlanes.QueryFlags.Horizontal,

        /// <summary>
        /// Include planes with arbitrary normal vectors that are not parallel or perpendicular to gravity.
        /// </summary>
        Arbitrary = MLPlanes.QueryFlags.Arbitrary,

        /// <summary>
        /// Include planes of all possible orientations.
        /// </summary>
        AllOrientations = MLPlanes.QueryFlags.AllOrientations,

        /// <summary>
        /// For non-horizontal planes, setting this flag will result in
        /// the plane rectangle being forced to perpendicular to gravity.
        /// </summary>
        OrientToGravity = MLPlanes.QueryFlags.OrientToGravity,

        /// <summary>
        /// If this flag is set, inner planes will be returned; if it is not set,
        /// outer planes will be returned.
        /// </summary>
        Inner = MLPlanes.QueryFlags.Inner,

        /// <summary>
        /// Instructs the plane system to ignore holes in planar surfaces.
        /// </summary>
        IgnoreHoles = MLPlanes.QueryFlags.IgnoreHoles,

        /// <summary>
        /// Include planes semantically tagged as ceiling.
        /// </summary>
        SemanticCeiling = MLPlanes.QueryFlags.SemanticCeiling,

        /// <summary>
        /// Include planes semantically tagged as floor.
        /// </summary>
        SemanticFloor = MLPlanes.QueryFlags.SemanticFloor,

        /// <summary>
        /// Include planes semantically tagged as wall.
        /// </summary>
        SemanticWall = MLPlanes.QueryFlags.SemanticWall,

        /// <summary>
        /// Include all planes that are semantically tagged.
        /// </summary>
        SemanticAll = MLPlanes.QueryFlags.SemanticAll,

        /// <summary>
        /// If this flag is set, polygons will be returned along with applicable rectangular planes;
        /// if it's not set, only rectangular planes will be returned.
        /// </summary>
        Polygons = MLPlanes.QueryFlags.Polygons
    }

    /// <summary>
    /// Plane Startup Configuration Parameters. Pass these to the Start function
    /// </summary>
    [Obsolete("Please use MLPlanes.QueryParams instead.")]
    public struct MLWorldPlanesQueryParams
    {
        /// <summary>
        /// The flags to apply to this query.
        /// </summary>
        public MLWorldPlanesQueryFlags Flags;

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
    }

    /// <summary>
    /// A plane with width and height.
    /// </summary>
    [Obsolete("Please use MLPlanes.Plane instead.")]
    public struct MLWorldPlane
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
    [Obsolete("Please use MLPlanes.Boundaries instead.")]
    public struct MLWorldPlaneBoundaries
    {
        /// <summary>
        /// Plane ID, the same value associating to the ID in #MLPlane if they belong to the same plane.
        /// </summary>
        public ulong Id;

        /// <summary>
        /// The boundaries in a plane.
        /// </summary>
        public MLWorldPlaneBoundary[] Boundaries;
    }

    /// <summary>
    /// Type used to represent a region boundary on a 2D plane.
    /// </summary>
    [Obsolete("Please use MLPlanes.Boundary instead.")]
    public struct MLWorldPlaneBoundary
    {
        /// <summary>
        /// The polygon that defines the region, the boundary vertices in MLPolygon will be in CCW order.
        /// </summary>
        public MLWorldPolygon Polygon;

        /// <summary>
        /// A polygon may contains multiple holes, the boundary vertices in MLPolygon will be in CW order.
        /// </summary>
        public MLWorldPolygon[] Holes;
    }

    /// <summary>
    /// Coplanar connected line segments representing the outer boundary of a polygon,
    /// an n sided polygon where n is the number of vertices.
    /// </summary>
    [Obsolete("Please use MLPlanes.Polygon instead.")]
    public struct MLWorldPolygon
    {
        /// <summary>
        /// Vertices of all line segments.
        /// </summary>
        public Vector3[] Vertices;
    }

    /// <summary>
    /// Creates planes requests and delegates their result.
    /// </summary>
    [Obsolete("Please use MLPlanes instead.")]
    public sealed class MLWorldPlanes : MLPlanes
    {
        /// <summary>
        /// Delegate to handle plane queries results.
        /// </summary>
        /// <param name="result">Result of the query.</param>
        /// <param name="planes">Planes found on the query.</param>
        /// <param name="boundaries">Plane boundaries.</param>
        [Obsolete("Please use MLPlanes instead.", true)]
        public delegate void CallbackDelegate(MLResult result, MLWorldPlane[] planes, MLWorldPlaneBoundaries[] boundaries);

        /// <summary>
        /// Request real world quad surfaces.
        /// Callback will never be called while request is still pending.
        /// </summary>
        /// <param name="queryParams">All values are required, omitting values may result in unexpected behavior.</param>
        /// <param name="callback">
        /// Callback used to report query results.
        /// Callback MLResult code will never be MLResult.Code.Pending.
        /// </param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        [Obsolete("Please use MLPlanes.GetPlanes(MLPlanes.QueryParams, QueryResultsDelegate) instead.", true)]
        public static MLResult GetPlanes(MLWorldPlanesQueryParams queryParams, CallbackDelegate callback)
        {
            return MLResult.Create(MLResult.Code.UnspecifiedFailure);
        }
    }
}

#endif
