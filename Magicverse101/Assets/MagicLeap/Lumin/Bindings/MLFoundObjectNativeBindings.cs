// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLFoundObjectNativeBindings.cs" company="Magic Leap, Inc">
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
    /// Manages calls to the native MLFoundObjects bindings.
    /// </summary>
    public sealed partial class MLFoundObjects : MLAPISingleton<MLFoundObjects>
    {
        /// <summary>
        /// This class defines the C# interface to the C functions/structures in "ml_camera.h".
        /// </summary>
        private class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// MLFoundObject library name.
            /// </summary>
            private const string MLFoundObjectDLL = "ml_perception_client";

            /// <summary>
            /// The maximum label size.
            /// </summary>
            private const uint MaxLabelSize = 64;

            /// <summary>
            /// The maximum property size.
            /// </summary>
            private const uint MaxPropertyKeySize = 64;

            /// <summary>
            /// The maximum property value size.
            /// </summary>
            private const uint MaxPropertyValueSize = 64;

            /// <summary>
            /// Initializes a new instance of the <see cref="NativeBindings"/> class.
            /// </summary>
            protected NativeBindings()
            {
            }

            /// <summary>
            /// The type of found object.
            /// </summary>
            public enum FoundObjectType
            {
                /// <summary>
                /// It's Invalid
                /// </summary>
                None = -1,

                /// <summary>
                /// It's an Object
                /// </summary>
                Object,
            }

            /// <summary>
            /// Create a found object query tracker.
            /// </summary>
            /// <param name="handle">A pointer to the handle of the found object query tracker.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if a valid handle was returned.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLFoundObjectDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLFoundObjectTrackerCreate(out ulong handle);

            /// <summary>
            /// Update the tracker settings.
            /// </summary>
            /// <param name="handle">A handle to the FoundObject query tracker.</param>
            /// <param name="settings">A pointer of the found object tracker settings.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if a valid handle was returned.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLFoundObjectDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLFoundObjectTrackerUpdateSettings(ulong handle, ref TrackerSettings settings);

            /// <summary>
            /// Create a new Found Object Query.
            /// Creates a new query for requesting found objects. Query criteria is
            /// specified by filling out the MLFoundObjectQueryFilter.
            /// </summary>
            /// <param name="handle">A handle to the FoundObject query tracker.</param>
            /// <param name="query">A pointer to the query filter applied to the search.</param>
            /// <param name="queryHandle">A pointer to the handle of the query.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if a valid handle was returned.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a privilege is missing or was denied.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLFoundObjectDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLFoundObjectQuery(ulong handle, ref QueryFilterNative query, out ulong queryHandle);

            /// <summary>
            /// Gets the result count of a query.
            /// </summary>
            /// <param name="handle">A handle to the FoundObject query tracker.</param>
            /// <param name="queryHandle">The handle to the active query.</param>
            /// <param name="numResults">A pointer to the number of max results from the query.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if a valid handle was returned.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a privilege is missing or was denied.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLFoundObjectDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLFoundObjectGetResultCount(ulong handle, ulong queryHandle, out uint numResults);

            /// <summary>
            /// Get the result of a submitted query.
            /// </summary>
            /// <param name="handle">A handle to the FoundObject query tracker.</param>
            /// <param name="queryHandle">The handle to the active query.</param>
            /// <param name="index">The index of the found object result.</param>
            /// <param name="foundObject">A pointer to the structure that will contain the found object information.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if a valid handle was returned.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLFoundObjectDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLFoundObjectGetResult(ulong handle, ulong queryHandle, uint index, ref FoundObjectNative foundObject);

            /// <summary>
            /// Gets the property information for a found object ID by index.
            /// This is not the data for a property. This is a MLFoundObjectProperty. If the
            /// property has a data size greater than zero and you would like to get the data you
            /// will have to call MLFoundObjectRequestPropertyData and then MLFoundObjectGetPropertyData.
            /// </summary>
            /// <param name="handle">A handle to the FoundObject query tracker.</param>
            /// <param name="id">The universally unique identifier of the found object.</param>
            /// <param name="index">The index of the found object.</param>
            /// <param name="property">A pointer to the property of the found object.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if a valid handle was returned.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLFoundObjectDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLFoundObjectGetProperty(ulong handle, MLUUID id, uint index, ref PropertyNative property);

            /// <summary>
            /// Returns the count for all the unique labels for your current area.
            /// </summary>
            /// <param name="handle">A handle to the FoundObject query tracker.</param>
            /// <param name="uniqueLabelCount">A count of all the unique labels in the area.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if a valid handle was returned.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLFoundObjectDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLFoundObjectGetAvailableLabelsCount(ulong handle, out uint uniqueLabelCount);

            /// <summary>
            /// Returns the unique label by index for your current area.
            /// Each found object has an array of labels. To facilitate better understanding of the
            /// environment, you can get all the unique labels in the area.This is used for
            /// discovering what is available in the users area.Unique labels have the
            /// potential to change and expand as the area is explored.
            /// </summary>
            /// <param name="handle">A handle to the FoundObject query tracker.</param>
            /// <param name="uniqueLabelIndex">The index of the unique label you are fetching.</param>
            /// <param name="bufferSize">The size of the buffer for the label.</param>
            /// <param name="label">A pointer to the label of the found object.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if a valid handle was returned.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLFoundObjectDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern MLResult.Code MLFoundObjectGetUniqueLabel(ulong handle, uint uniqueLabelIndex, uint bufferSize, [MarshalAs(UnmanagedType.LPStr)] out string label);

            /// <summary>
            /// Releases the resources assigned to the tracker.
            /// </summary>
            /// <param name="handle">A handle to the found object query tracker.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if a valid handle was returned.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLFoundObjectDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLFoundObjectTrackerDestroy(ulong handle);

            /// <summary>
            /// Contains property information for the found object.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct PropertyNative
            {
                /// <summary>
                /// Key for an objects property. Type is string. Max size is defined by MLFoundObject_MaxPropertyKeySize.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MaxPropertyKeySize)]
                public char[] Key;

                /// <summary>
                /// Value for an objects property. Type is string. Max size is defined by MLFoundObject_MaxPropertyValueSize.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MaxPropertyValueSize)]
                public char[] Value;

                /// <summary>
                /// Last time this object was updated in UTC time. Not filled out when creating a query.
                /// </summary>
                public ulong LastUpdateEpochTimeNs;
            }

            /// <summary>
            /// Struct used to compose a query.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct QueryFilterNative
            {
                /// <summary>
                /// Version of the structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Valid ID for a found object. This can be obtained from sources such as a prior session.
                /// </summary>
                public MLUUID Id;

                /// <summary>
                /// Properties to be used as filters for the query.
                /// </summary>
                /// const FoundObjectProperty *properties;
                public IntPtr Properties;

                /// <summary>
                /// Number of attributes.
                /// </summary>
                public uint PropertiesCount;

                /// <summary>
                /// Vector3 float of where you want the spatial query to originate.
                /// </summary>
                public MLVec3f Center;

                /// <summary>
                /// Vector3 float of the max distance you want the spatial query to span relative to the center of the query.
                /// </summary>
                public MLVec3f MaxDistance;

                /// <summary>
                /// Maximum number of results. Used to allocate memory.
                /// </summary>
                public uint MaxResults;

                /// <summary>
                /// Initializes a FoundObjectQueryFilter with the default values.
                /// </summary>
                /// <param name="filter">A pointer to the found object query filter.</param>
                public void Create(ref QueryFilterNative filter)
                {
                    filter.Version = 1;
                    filter.Properties = IntPtr.Zero;
                    filter.PropertiesCount = 0;
                    filter.Center.X = 0.0f;
                    filter.Center.Y = 0.0f;
                    filter.Center.Z = 0.0f;
                    filter.MaxDistance.X = 0.0f;
                    filter.MaxDistance.Y = 0.0f;
                    filter.MaxDistance.Z = 0.0f;
                    filter.MaxResults = 0;
                }
            }

            /// <summary>
            /// Struct to represent a Found Object.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct FoundObjectNative
            {
                /// <summary>
                /// Gets the identifier of the Found Object.
                /// </summary>
                public MLUUID Id { get; private set; }

                /// <summary>
                /// Gets the number of properties.
                /// </summary>
                public uint PropertyCount { get; private set; }

                /// <summary>
                /// Gets the center position of found object.
                /// </summary>
                public MLVec3f Position { get; private set; }

                /// <summary>
                /// Gets the rotation of found object.
                /// </summary>
                public MLQuaternionf Rotation { get; private set; }

                /// <summary>
                /// Gets the Vector3 extents of the object where each dimension is defined as max-min.
                /// </summary>
                public MLVec3f Size { get; private set; }
            }

            /// <summary>
            /// Settings for the found object tracker.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TrackerSettings
            {
                /// <summary>
                /// Gets the version of the structure.
                /// </summary>
                public uint Version { get; private set; }

                /// <summary>
                /// Gets the max result returned by a query.
                /// </summary>
                public uint MaxQueryResult { get; private set; }

                /// <summary>
                ///  Gets the maximum number of found objects to be stored.
                /// </summary>
                public uint MaxObjectCache { get; private set; }

                /// <summary>
                /// Initializes a MLFoundObjectTrackerSettings with the default values.
                /// </summary>
                /// <param name="settings">A pointer to the found object tracker settings.</param>
                public void Create(ref TrackerSettings settings)
                {
                    settings.Version = 1;
                    settings.MaxQueryResult = 256;
                    settings.MaxObjectCache = 1024;
                }
            }

            /// <summary>
            /// Helper structure to store found object query data.
            /// </summary>
            public struct Query
            {
                /// <summary>
                /// Query result callback.
                /// </summary>
                public readonly QueryResultsDelegate Callback;

                /// <summary>
                /// The filter applied to the query.
                /// </summary>
                public readonly QueryFilterNative QueryFilter;

                /// <summary>
                /// Gets or sets the result.
                /// </summary>
                public MLResult Result;

                /// <summary>
                /// Initializes a new instance of the <see cref="Query"/> struct.
                /// </summary>
                /// <param name="callback">The callback that should be invoked.</param>
                /// <param name="queryFilter">The filter applied to the query.</param>
                /// <param name="result">The result of the query.</param>
                public Query(QueryResultsDelegate callback, NativeBindings.QueryFilterNative queryFilter, MLResult result)
                {
                    this.Callback = callback;
                    this.QueryFilter = queryFilter;
                    this.Result = result;
                }
            }
        }
    }
}

#endif
