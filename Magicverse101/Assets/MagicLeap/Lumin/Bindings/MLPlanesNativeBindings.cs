// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLPlanesNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;

    /// <summary>
    /// Creates planes requests and delegates their result.
    /// </summary>
    public partial class MLPlanes : MLAPISingleton<MLPlanes>
    {
        /// <summary>
        /// See ml_planes.h for additional comments
        /// </summary>
        private class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// Prevents a default instance of the <see cref="NativeBindings"/> class from being created.
            /// </summary>
            private NativeBindings()
            {
            }

            /// <summary>
            /// Create a planes tracker.
            /// </summary>
            /// <param name="handle">A reference to the handle to the planes tracker.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPlanesCreate(ref ulong handle);

            /// <summary>
            /// Initiates a plane query.
            /// </summary>
            /// <param name="planesTracker">Handle produced by MLPlanesCreate().</param>
            /// <param name="query">A reference to a QueryNative structure containing query parameters.</param>
            /// <param name="handle">A pointer to a handle which will contain the handle to the query.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPlanesQueryBegin(ulong planesTracker, ref QueryParamsNative query, ref ulong handle);

            /// <summary>
            /// Destroy a planes tracker.
            /// </summary>
            /// <param name="planesTracker">Handle to planes tracker to destroy.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPlanesDestroy(ulong planesTracker);

            /// <summary>
            /// Gets the result of a plane query with boundaries on each plane.
            /// After this function has returned successfully, the handle is invalid and should be discarded.
            /// Also check MLPlanes.QueryFlag.Polygons description for this API's further behavior.
            /// </summary>
            /// <param name="planesTracker">Handle produced by MLPlanesCreate().</param>
            /// <param name="queryHandle">Handle produced by MLPlanesQueryBegin().</param>
            /// <param name="results">An array of PlaneNative structures.</param>
            /// <param name="numResults">The count of results pointed to by results.</param>
            /// <param name="planeBoundaries">A pointer to MLPlaneBoundariesList for the returned polygons.
            /// If boundaries is NULL, the function call will not return any polygons, otherwise boundaries must be zero initialized.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if query has not yet completed.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPlanesQueryGetResultsWithBoundaries(ulong planesTracker, ulong queryHandle, IntPtr results, out uint numResults, ref BoundariesListNative planeBoundaries);

            /// <summary>
            /// Release the polygons data owned by the #MLPlaneBoundariesList.
            /// Also, check MLPlanes.QueryFlag.Polygons description for this API's further behavior.
            /// </summary>
            /// <param name="planesTracker">Handle produced by MLPlanesCreate().</param>
            /// <param name="planeBoundaries">BoundariesListNative to release.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if query has not yet completed.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPlanesReleaseBoundariesList(ulong planesTracker, ref BoundariesListNative planeBoundaries);

            /// <summary>
            /// Type used to represent a plane query.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct QueryParamsNative
            {
                /// <summary>
                /// The flags to apply to this query.
                /// </summary>
                public MLPlanes.QueryFlags Flags;

                /// <summary>
                /// The center of the bounding box which defines where planes extraction should occur.
                /// </summary>
                public MLVec3f BoundsCenter;

                /// <summary>
                /// The rotation of the bounding box where planes extraction will occur.
                /// </summary>
                public MLQuaternionf BoundsRotation;

                /// <summary>
                /// The size of the bounding box where planes extraction will occur.
                /// </summary>
                public MLVec3f BoundsExtents;

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
                /// Sets the native structure from the external one.
                /// </summary>
                public MLPlanes.QueryParams Data
                {
                    set
                    {
                        this.Flags = value.Flags;
                        this.BoundsCenter = Native.MLConvert.FromUnity(value.BoundsCenter);
                        this.BoundsRotation = Native.MLConvert.FromUnity(value.BoundsRotation);
                        this.BoundsExtents = Native.MLConvert.FromUnity(value.BoundsExtents, false, true);
                        this.MaxResults = value.MaxResults;
                        this.MinHoleLength = value.MinHoleLength;
                        this.MinPlaneArea = value.MinPlaneArea;
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>New instance of <c>QueryParamsNative</c>.</returns>
                public static QueryParamsNative Create()
                {
                    return new QueryParamsNative()
                    {
                        Flags = MLPlanes.QueryFlags.AllOrientations | MLPlanes.QueryFlags.SemanticAll,
                        BoundsCenter = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        BoundsRotation = new MLQuaternionf() { X = 0.0f, Y = 0.0f, Z = 0.0f, W = 1.0f },
                        BoundsExtents = new MLVec3f() { X = 10.0f, Y = 10.0f, Z = 10.0f },
                        MaxResults = 10,
                        MinHoleLength = 0.0f,
                        MinPlaneArea = 0.04f
                    };
                }
            }

            /// <summary>
            /// A plane with width and height.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PlaneNative
            {
                /// <summary>
                /// Plane center.
                /// </summary>
                public MLVec3f Position;

                /// <summary>
                /// Plane rotation.
                /// </summary>
                public MLQuaternionf Rotation;

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

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>New instance of PlaneNative.</returns>
                public static PlaneNative Create()
                {
                    return new PlaneNative()
                    {
                        Position = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        Rotation = new MLQuaternionf() { X = 0.0f, Y = 0.0f, Z = 0.0f, W = 1.0f },
                        Width = 0.0f,
                        Height = 0.0f,
                        Flags = 0u,
                        Id = 0,
                    };
                }
            }

            /// <summary>
            /// Coplanar connected line segments representing the outer boundary of a polygon,
            /// an n sided polygon where n is the number of vertices.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PolygonNative
            {
                /// <summary>
                /// Vertices of all line segments.
                /// </summary>
                public IntPtr Vertices;

                /// <summary>
                /// Number of vertices.
                /// </summary>
                public uint VerticesCount;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>New instance of PolygonNative.</returns>
                public static PolygonNative Create()
                {
                    return new PolygonNative()
                    {
                        Vertices = IntPtr.Zero,
                        VerticesCount = 0u,
                    };
                }
            }

            /// <summary>
            /// Type to represent multiple regions on a 2D plane.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct BoundaryNative
            {
                /// <summary>
                /// The polygon that defines the region, the boundary vertices in MLPolygon will be in CCW order.
                /// </summary>
                public IntPtr Polygon;

                /// <summary>
                /// A polygon may contains multiple holes, the boundary vertices in MLPolygon will be in CW order.
                /// </summary>
                public IntPtr Holes;

                /// <summary>
                /// Count of the holes.
                /// </summary>
                public uint HolesCount;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>New instance of BoundaryNative.</returns>
                public static BoundaryNative Create()
                {
                    return new BoundaryNative()
                    {
                        Polygon = IntPtr.Zero,
                        Holes = IntPtr.Zero,
                        HolesCount = 0u,
                    };
                }
            }

            /// <summary>
            /// Type to represent multiple regions on a 2D plane.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct BoundariesNative
            {
                /// <summary>
                /// Plane ID, the same value associating to the ID in #MLPlane if they belong to the same plane.
                /// </summary>
                public ulong Id;

                /// <summary>
                /// The boundaries in a plane.
                /// </summary>
                public IntPtr Boundaries;

                /// <summary>
                /// Count of boundaries. A plane may contain multiple boundaries each of which defines a region.
                /// </summary>
                public uint BoundariesCount;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>New instance of BoundariesNative.</returns>
                public static BoundariesNative Create()
                {
                    return new BoundariesNative()
                    {
                        Id = 0,
                        Boundaries = IntPtr.Zero,
                        BoundariesCount = 0u,
                    };
                }
            }

            /// <summary>
            /// Type to represent polygons of all returned planes.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct BoundariesListNative
            {
                /// <summary>
                /// Version number of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// List of BoundariesNative.
                /// </summary>
                public IntPtr PlaneBoundaries;

                /// <summary>
                /// Count of BoundariesNative in the array.
                /// </summary>
                public uint PlaneBoundariesCount;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>New instance of BoundariesListNative.</returns>
                public static BoundariesListNative Create()
                {
                    return new BoundariesListNative()
                    {
                        Version = 1u,
                        PlaneBoundaries = IntPtr.Zero,
                        PlaneBoundariesCount = 0u,
                    };
                }
            }

            /// <summary>
            /// Helper structure to store plane query data.
            /// </summary>
            public struct Query
            {
                /// <summary>
                /// Query result callback.
                /// </summary>
                public readonly MLPlanes.QueryResultsDelegate Callback;

                /// <summary>
                /// The max results.
                /// </summary>
                public readonly uint MaxResults;

                /// <summary>
                /// The planes results unmanaged.
                /// </summary>
                public readonly IntPtr PlanesResultsUnmanaged;

                /// <summary>
                /// Gets or sets the planes.
                /// </summary>
                public MLPlanes.Plane[] Planes;

                /// <summary>
                /// The boundaries results unmanaged.
                /// </summary>
                public NativeBindings.BoundariesListNative PlaneBoundariesList;

                /// <summary>
                /// Gets or sets the boundaries.
                /// </summary>
                public MLPlanes.Boundaries[] PlaneBoundaries;

                /// <summary>
                /// Gets or sets the result.
                /// </summary>
                public MLResult Result;

                /// <summary>
                /// Initializes a new instance of the <see cref="Query"/> struct.
                /// </summary>
                /// <param name="callback">The callback to trigger when query ends.</param>
                /// <param name="maxResults">Maximum amount of results desired.</param>
                /// <param name="requestBoundaries">Determines whether or not boundaries should be requested.</param>
                public Query(MLPlanes.QueryResultsDelegate callback, uint maxResults, bool requestBoundaries)
                {
                    this.Callback = callback;
                    this.MaxResults = maxResults;
                    this.Planes = default;
                    this.PlanesResultsUnmanaged = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PlaneNative)) * (int)this.MaxResults);
                    this.PlaneBoundariesList = BoundariesListNative.Create();
                    this.PlaneBoundaries = default;
                    this.Result = MLResult.Create(MLResult.Code.Ok);

                    // Allows proper API selection based on query parameters and assigned callback.
                    this.IsRequestingBoundaries = requestBoundaries;
                }

                /// <summary>
                /// Gets a value indicating whether polygon boundaries have been requested.
                /// </summary>
                public bool IsRequestingBoundaries { get; private set; }

                /// <summary>
                /// Cleans unmanaged memory.
                /// </summary>
                public void Dispose()
                {
                    Marshal.FreeHGlobal(this.PlanesResultsUnmanaged);
                }

                /// <summary>
                /// Extract unmanaged plane data and convert to managed plane data.
                /// </summary>
                /// <param name="numResults">Number of plane results returned.</param>
                public void ExtractPlanesFromQueryResults(uint numResults)
                {
                    this.ExtractRectangularPlanes(numResults);

                    if (this.IsRequestingBoundaries)
                    {
                        this.ExtractPolygonalPlanes();
                    }
                }

                /// <summary>
                /// Extracts user facing planes from native plane data.
                /// </summary>
                /// <param name="numResults">The number of results.</param>
                private void ExtractRectangularPlanes(uint numResults)
                {
                    this.Planes = new MLPlanes.Plane[numResults];
                    int sizeMLPlane = Marshal.SizeOf(typeof(NativeBindings.PlaneNative));
                    IntPtr planeResultPointer = this.PlanesResultsUnmanaged;
                    for (var i = 0; i < numResults; ++i)
                    {
                        // Read unmanaged structure and convert to our managed representation.
                        var planeResult = (NativeBindings.PlaneNative)Marshal.PtrToStructure(planeResultPointer, typeof(NativeBindings.PlaneNative));

                        this.Planes[i] = new MLPlanes.Plane()
                        {
                            Height = Native.MLConvert.ToUnity(planeResult.Height),
                            Width = Native.MLConvert.ToUnity(planeResult.Width),
                            Center = Native.MLConvert.ToUnity(planeResult.Position),
                            Rotation = Native.MLConvert.ToUnity(planeResult.Rotation),
                            Flags = planeResult.Flags,
                            Id = planeResult.Id
                        };

                        // Move to next entry in pointer array.
                        planeResultPointer = new IntPtr(planeResultPointer.ToInt64() + sizeMLPlane);
                    }
                }

                /// <summary>
                /// Extracts polygonal plane boundaries data from native plane data.
                /// </summary>
                private void ExtractPolygonalPlanes()
                {
                    IntPtr ptr = this.PlaneBoundariesList.PlaneBoundaries;
                    uint count = this.PlaneBoundariesList.PlaneBoundariesCount;
                    int size = Marshal.SizeOf(typeof(NativeBindings.BoundariesNative));
                    MLPlanes.Boundaries[] array = new MLPlanes.Boundaries[count];

                    for (var i = 0; i < count; ++i)
                    {
                        // Read unmanaged structure and convert to our managed representation.
                        var result = (NativeBindings.BoundariesNative)Marshal.PtrToStructure(ptr, typeof(NativeBindings.BoundariesNative));

                        array[i].Id = result.Id;
                        array[i].PlaneBoundaries = this.GetBoundaryArray(result.Boundaries, result.BoundariesCount);

                        // Advance the pointer address.
                        ptr = new IntPtr(ptr.ToInt64() + size);
                    }

                    this.PlaneBoundaries = array;
                }

                /// <summary>
                /// Extracts boundary data from native pointer.
                /// </summary>
                /// <param name="ptr">Pointer with the data.</param>
                /// <param name="count">Number of results.</param>
                /// <returns>The user facing boundaries array.</returns>
                private MLPlanes.Boundary[] GetBoundaryArray(IntPtr ptr, uint count)
                {
                    int size = Marshal.SizeOf(typeof(NativeBindings.BoundaryNative));
                    MLPlanes.Boundary[] array = new MLPlanes.Boundary[count];

                    for (var i = 0; i < count; ++i)
                    {
                        // Read unmanaged structure and convert to our managed representation.
                        var result = (NativeBindings.BoundaryNative)Marshal.PtrToStructure(ptr, typeof(NativeBindings.BoundaryNative));

                        // Marshal the Polygon, and get the vertices.
                        var polygon = (NativeBindings.PolygonNative)Marshal.PtrToStructure(result.Polygon, typeof(NativeBindings.PolygonNative));
                        array[i].Polygon = new MLPlanes.Polygon()
                        {
                            Vertices = this.GetVector3Array(polygon.Vertices, polygon.VerticesCount)
                        };

                        // Marshal the Holes array, and get the vertices.
                        array[i].Holes = this.GetPolygonArray(result.Holes, result.HolesCount);

                        // Advance the pointer address.
                        ptr = new IntPtr(ptr.ToInt64() + size);
                    }

                    return array;
                }

                /// <summary>
                /// Extracts polygon data from native pointer.
                /// </summary>
                /// <param name="ptr">Pointer with the data.</param>
                /// <param name="count">Number of results.</param>
                /// <returns>The user facing polygon array.</returns>
                private MLPlanes.Polygon[] GetPolygonArray(IntPtr ptr, uint count)
                {
                    int size = Marshal.SizeOf(typeof(NativeBindings.PolygonNative));
                    MLPlanes.Polygon[] array = new MLPlanes.Polygon[count];

                    for (var i = 0; i < count; ++i)
                    {
                        // Read unmanaged structure and convert to our managed representation.
                        var result = (NativeBindings.PolygonNative)Marshal.PtrToStructure(ptr, typeof(NativeBindings.PolygonNative));

                        array[i].Vertices = this.GetVector3Array(result.Vertices, result.VerticesCount);

                        // Advance the pointer address.
                        ptr = new IntPtr(ptr.ToInt64() + size);
                    }

                    return array;
                }

                /// <summary>
                /// Extracts vertex data from native pointer.
                /// </summary>
                /// <param name="ptr">Pointer with the data.</param>
                /// <param name="count">Number of results.</param>
                /// <returns>The user facing vertex array.</returns>
                private Vector3[] GetVector3Array(IntPtr ptr, uint count)
                {
                    int size = Marshal.SizeOf(typeof(Native.MagicLeapNativeBindings.MLVec3f));
                    Vector3[] array = new Vector3[count];

                    for (var i = 0; i < count; ++i)
                    {
                        // Read unmanaged structure and convert to our managed representation.
                        var result = (Native.MagicLeapNativeBindings.MLVec3f)Marshal.PtrToStructure(ptr, typeof(Native.MagicLeapNativeBindings.MLVec3f));

                        array[i] = Native.MLConvert.ToUnity(result);

                        // Advance the pointer address.
                        ptr = new IntPtr(ptr.ToInt64() + size);
                    }

                    return array;
                }
            }
        }
    }
}

#endif
