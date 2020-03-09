// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLPCFNativeBindings.cs" company="Magic Leap, Inc">
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
    using UnityEngine.XR.MagicLeap.Native;

    public partial class MLPersistentCoordinateFrames
    {
        private class NativeBindings : MagicLeapNativeBindings
        {
            /// <summary>
            /// Prevents a default instance of the <see cref="NativeBindings" /> class from being created.
            /// </summary>
            private NativeBindings()
            {
            }

            /// <summary>
            /// Creates a Persistent Coordinate Frame Tracker.
            /// </summary>
            /// <param name="handle">Pointer to an MLHandle.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if a valid handle is returned.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPersistentCoordinateFrameTrackerCreate(ref ulong handle);

            /// <summary>
            /// Returns the count of currently available Persistent Coordinate Frames.
            /// </summary>
            /// <param name="trackerHandle">Valid MLHandle to a Persistent Coordinate Frame Tracker.</param>
            /// <param name="outCount">Number of currently available Persistent Coordinate Frames.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldLowMapQuality</c> if map quality is too low for content persistence. Continue building the map.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldUnableToLocalize</c> if currently unable to localize into any map. Continue building the map.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPersistentCoordinateFrameGetCount(ulong trackerHandle, ref uint outCount);

            /// <summary>
            /// Returns all the Persistent Coordinate Frames currently available.
            /// </summary>
            /// <param name="trackerHandle">Valid MLHandle to a Persistent Coordinate Frame Tracker.</param>
            /// <param name="count">The size of the <c>cfuIds</c> array.</param>
            /// <param name="cfuIds">An array used for writing the Persistent Coordinate Frame's MLCoordinateFrameUID.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldLowMapQuality</c> if map quality is too low for content persistence. Continue building the map.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldUnableToLocalize</c> if currently unable to localize into any map. Continue building the map.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPersistentCoordinateFrameGetAllEx(ulong trackerHandle, uint count, [In, Out] MLCoordinateFrameUID[] cfuIds);

            /// <summary>
            /// Returns the closest Persistent Coordinate Frame to the target point passed in.
            /// </summary>
            /// <param name="trackerHandle">Valid MLHandle to a Persistent Coordinate Frame Tracker.</param>
            /// <param name="target">XYZ of the destination that the nearest Persistent Coordinate Frame is requested for.</param>
            /// <param name="cfuId">Pointer to be used to write the MLCoordinateFrameUID for the nearest Persistent Coordinate Frame to the target destination.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldLowMapQuality</c> if map quality is too low for content persistence. Continue building the map.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldUnableToLocalize</c> if currently unable to localize into any map. Continue building the map.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPersistentCoordinateFrameGetClosest(ulong trackerHandle, ref MLVec3f target, ref MLCoordinateFrameUID cfuId);

            /// <summary>
            /// Destroys a Persistent Coordinate Frame Tracker.
            /// </summary>
            /// <param name="trackerHandle">Valid MLHandle to a Persistent Coordinate Frame Tracker.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the tracker handle is not known.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPersistentCoordinateFrameTrackerDestroy(ulong trackerHandle);

            /// <summary>
            /// Returns the state of the Persistent Coordinate Frame passed in as parameter.
            /// </summary>
            /// <param name="trackerHandle">Valid MLHandle to a Persistent Coordinate Frame Tracker.</param>
            /// <param name="cfuid">Valid MLCoordinateFrameUID of a Persistent Coordinate Frame.</param>
            /// <param name="state">Pointer to be used for writing the Persistent Coordinate Frame's state.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldLowMapQuality</c> if map quality is too low for content persistence. Continue building the map.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldNotFound</c> if the passed cfuid is not available.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldUnableToLocalize</c> if currently unable to localize into any map. Continue building the map.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPersistentCoordinateFramesGetFrameState(ulong trackerHandle, in MLCoordinateFrameUID cfuid, ref FrameStateNative state);

            /// <summary>
            /// Returns filtered set of Persistent Coordinate Frames based on the informed parameters.
            /// </summary>
            /// <param name="trackerHandle">Valid MLHandle to a Persistent Coordinate Frame Tracker.</param>
            /// <param name="queryFilter">Parameters used to curate the returned values.</param>
            /// <param name="cfuids">An array of QueryFilter.MaxResults size used for writing the PCF's MLCoordinateFrameUID.</param>
            /// <param name="cfuidCount">Number of entries populated in cfuidArray. Any number between 0 and QueryFilter.MaxResults.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldLowMapQuality</c> if map quality is too low for content persistence. Continue building the map.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldUnableToLocalize</c> if currently unable to localize into any map. Continue building the map.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldSharedWorldNotEnabled</c> if QueryFilter.TypesMask is only FrameTypeNative.MultiUserMultiSession but user has not enabled shared world in settings.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLPersistentCoordinateFrameQuery(ulong trackerHandle, in QueryFilterNative queryFilter, [In, Out] MLCoordinateFrameUID[] cfuids, out uint cfuidCount);


            /// <summary>
            /// Provides the string value for some PCF related MLResult.Code.
            /// </summary>
            /// <param name="result">The MLResult.Code to get the string value for.</param>
            /// <returns>Pointer to the string value of the given MLResult.Code.</returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MLPersistentCoordinateFrameGetResultString(MLResult.Code result);

            /// <summary>
            /// The native structure for tracking the Persistent Coordinate Frame's state.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct FrameStateNative
            {
                /// <summary>
                /// Version of this struct.
                /// </summary>
                public uint Version;

                /// <summary>
                /// A confidence value (from [0, 1]) representing the confidence in the
                /// Persistent Coordinate Frame error within the valid radius.
                /// </summary>
                public float Confidence;

                /// <summary>
                /// The radius (in meters) within which the confidence is valid.
                /// </summary>
                public float ValidRadiusM;

                /// <summary>
                /// The rotational error (in degrees).
                /// </summary>
                public float RotationErrDeg;

                /// <summary>
                /// The translation error (in meters).
                /// </summary>
                public float TranslationErrM;

                /// <summary>
                /// The PCF type.
                /// PCFs can vary in their types between multiple head pose sessions.
                /// </summary>
                public PCF.Types Type;

                public PCF.State Data()
                {
                    return new PCF.State()
                    {
                        Confidence = this.Confidence,
                        ValidRadiusM = this.ValidRadiusM,
                        RotationErrDeg = this.RotationErrDeg,
                        TranslationErrM = this.TranslationErrM,
                        Type = this.Type
                    };
                }

                /// <summary>
                /// Create an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static FrameStateNative Create()
                {
                    return new FrameStateNative()
                    {
                        Version = 2u,
                        Confidence = 0.0f,
                        ValidRadiusM = 0.0f,
                        RotationErrDeg = 0.0f,
                        TranslationErrM = 0.0f,
                        Type = PCF.Types.SingleUserSingleSession
                    };
                }
            }

            /// <summary>
            /// This represents a collection of filters and modifiers used by
            /// MLPersistentCoordinateFrameQuery to curate the returned values.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct QueryFilterNative
            {
                /// <summary>
                /// Version of this struct.
                /// </summary>
                public uint Version;

                /// <summary>
                /// [X,Y,Z] center query point from where the nearest neighbors will be calculated.
                /// </summary>
                public MLVec3f TargetPoint;

                /// <summary>
                /// Expected types of the results.
                /// This is a bitmask field to specify all expected types.
                /// For example, use:
                /// TypesMask = PCFTypes.SingleUserMultiSession | PCFTypes.MultiUserMultiSession;
                /// to get PCFs of PCFTypes.SingleUserMultiSession and PCFTypes.MultiUserMultiSession types
                /// </summary>
                public uint TypesMask;

                /// <summary>
                /// Upper bound number of expected results.
                /// The implementation may return less entries than requested when total number of
                /// available elements is less than max_results or if internal memory limits are reached.
                /// </summary>
                public uint MaxResults;

                /// <summary>
                /// Return only entries within radius of the sphere from target_point.
                /// Radius is provided in meters. Set to zero for unbounded results.
                /// Filtering by distance will incur a performance penalty.
                /// </summary>
                public float RadiusM;

                /// <summary>
                /// Indicate if the result set should be sorted by distance from target_point.
                /// Sorting the PCFs by distance will incur a performance penalty.
                /// </summary>
                public bool Sorted;

                /// <summary>
                /// Sets the native structures from the user facing properties.
                /// </summary>
                public QueryFilter Data
                {
                    set
                    {
                        this.TargetPoint = MLConvert.FromUnity(value.TargetPoint);
                        this.TypesMask = (uint)value.TypesMask;
                        this.MaxResults = value.MaxResults;
                        this.RadiusM = value.Radius;
                        this.Sorted = value.Sorted;
                    }
                }

                /// <summary>
                /// Initializes default values for QueryFilterNative.
                /// </summary>
                /// <returns> An initialized version of this struct.</returns>
                public static QueryFilterNative Create()
                {
                    return new QueryFilterNative()
                    {
                        Version = 1u,
                        TargetPoint = new MLVec3f(),
                        TypesMask = (uint)PCF.Types.MultiUserMultiSession,
                        MaxResults = 1,
                        RadiusM = 0.0f,
                        Sorted = true
                    };
                }
            }
        }
    }
}


#endif
