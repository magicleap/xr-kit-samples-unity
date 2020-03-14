// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLHeadTrackingNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// The MLHeadTracking API used to get head tracking state and triggered map events.
    /// </summary>
    public sealed partial class MLHeadTracking
    {
        /// <summary>
        /// Native bindings for the MLHeadTracking API.
        /// See ml_head_tracking.h for additional comments.
        /// </summary>
        private partial class NativeBindings : MagicLeapNativeBindings
        {
            /// <summary>
            ///  Prevents a default instance of the <see cref="NativeBindings" /> class from being created.
            /// </summary>
            private NativeBindings()
            {
            }

            /// <summary>
            /// Returns the most recent Head Tracking state.
            /// </summary>
            /// <param name="handle">A handle to the tracker.</param>
            /// <param name="outState">Pointer to valid NativeBindings.StateNative object to be filled with current state information.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the head tracking state was successfully received.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the outState parameter was not valid (null).
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed to receive head tracking state.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLHeadTrackingGetState(ulong handle, ref StateNative outState);

            /// <summary>
            /// Creates a Head Tracker.
            /// </summary>
            /// <param name="handle">A pointer to the handle of the head tracker. If this operation fails, handle will be MagicLeapNativeBindings.InvalidHandle.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the head tracker was created successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the head tracker failed to create successfully.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLHeadTrackingCreate(ref ulong handle);

            /// <summary>
            /// Destroys a Head Tracker.
            /// </summary>
            /// <param name="handle">A handle to a Head Tracker created by MLHeadTrackingCreate().</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the head tracker was destroyed successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the head tracker was not destroyed successfully.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLHeadTrackingDestroy(ulong handle);

            /// <summary>
            /// A developer must be aware of certain events that can occur under degenerative conditions
            /// in order to cleanly handle it. The most important event to be aware of is when a map changes.
            /// In the case that a new map session begins, or recovery fails, all formerly cached transform
            /// and world reconstruction data <c>(raycast, planes, mesh)</c> is invalidated and must be updated.
            /// </summary>
            /// <param name="handle">A handle to the tracker.</param>
            /// <param name="outMapEvents">Pointer to a bitmask of MLHeadTrackingMapEvent, allocated by the caller.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if map events were successfully received.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the outMapEvents parameter was not valid (null).
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed to map events.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLHeadTrackingGetMapEvents(ulong handle, ref ulong outMapEvents);

            /// <summary>
            /// Returns static information about the Head Tracker.
            /// </summary>
            /// <param name="handle">A handle to the tracker.</param>
            /// <param name="outStaticData">Target to populate the data about that Head Tracker.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the static data was successfully received.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the outStaticData pointer was not valid (null).
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the static data could not be received.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLHeadTrackingGetStaticData(ulong handle, ref StaticDataNative outStaticData);

            /// <summary>
            /// A structure containing information on the current state of the
            /// Head Tracking system.
            /// </summary>
            public struct StateNative
            {
                /// <summary>
                /// What tracking mode the Head Tracking system is currently in.
                /// </summary>
                public MLHeadTracking.TrackingMode Mode;

                /// <summary>
                /// A confidence value (from 0..1) representing the confidence in the
                /// current pose estimation.
                /// </summary>
                public float Confidence;

                /// <summary>
                /// Represents what tracking error (if any) is present.
                /// </summary>
                public MLHeadTracking.TrackingError Error;
            }

            /// <summary>
            /// Holds static data about the head tracker.
            /// </summary>
            public struct StaticDataNative
            {
                /// <summary>
                /// The coordinate frame id of the head tracker.
                /// </summary>
                public MLCoordinateFrameUID CUID;
            }
        }
    }
}
#endif
