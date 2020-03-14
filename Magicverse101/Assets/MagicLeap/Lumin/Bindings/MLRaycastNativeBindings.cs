// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLRaycastNativeBindings.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// Sends requests to create Rays intersecting world geometry and returns results through callbacks.
    /// </summary>
    public partial class MLRaycast : MLAPISingleton<MLRaycast>
    {
        /// <summary>
        /// See <c>ml_raycast.h</c> for additional comments
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
            /// Create the ray cast system. This function must be called with the the required settings prior to <c>MLRaycastRequest()</c>.
            /// </summary>
            /// <param name="trackerHandle">Handle to the created ray cast system. Only valid if the return value is MLResult_Ok.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLRaycastCreate(ref ulong trackerHandle);

            /// <summary>
            /// Begin a query to a ray cast.
            /// </summary>
            /// <param name="trackerHandle">Handle to the tracker created by <c>MLRaycastRequest()</c>.</param>
            /// <param name="request">Query parameters for the ray cast.</param>
            /// <param name="queryHandle">A handle to an ongoing request.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLRaycastRequest(ulong trackerHandle, ref MLRaycastQueryNative request, ref ulong queryHandle);

            /// <summary>
            /// Get the result of a call to <c>MLRaycastRequest()</c>.
            /// After this function has returned successfully, the handle is invalid.
            /// </summary>
            /// <param name="trackerHandle">Handle to the tracker created by <c>MLRaycastRequest()</c>.</param>
            /// <param name="raycastRequest">A handle to the ray cast request.</param>
            /// <param name="result">The target to populate the result.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if request has not completed. This does not indicate a failure.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLRaycastGetResult(ulong trackerHandle, ulong raycastRequest, ref MLRaycastResultNative result);

            /// <summary>
            /// Destroy a ray cast tracker.
            /// </summary>
            /// <param name="trackerHandle">Handle to the tracker created by <c>MLRaycastRequest()</c>.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLRaycastDestroy(ulong trackerHandle);

            /// <summary>
            /// Request information for a ray cast.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MLRaycastQueryNative
            {
                /// <summary>
                /// Origin of ray, in world space.
                /// </summary>
                public MLVec3f Position;

                /// <summary>
                /// Direction of ray, in world space.
                /// </summary>
                public MLVec3f Direction;

                /// <summary>
                /// Up vector, in world space.
                /// If multiple rays are to be fired, this is used to determine the coordinate system used to
                /// calculate the directions of those rays; therefore must be orthogonal to the direction vector.
                /// Use MLTransform.rotation* (0, 1, 0) to use the up vector of the rig frame in world space.
                /// This parameter has no effect on a single-point ray cast.
                /// </summary>
                public MLVec3f UpVector;

                /// <summary>
                /// The number of horizontal rays. For single point ray cast, set this to 1.
                /// </summary>
                public uint Width;

                /// <summary>
                /// The number of vertical rays. For single point ray cast, set this to 1.
                /// </summary>
                public uint Height;

                /// <summary>
                /// The horizontal field of view, in degrees.
                /// </summary>
                public float HorizontalFovDegrees;

                /// <summary>
                /// If true, a ray will terminate when encountering an unobserved area and return a surface
                /// or the ray will continue until it ends or hits a observed surface.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool CollideWithUnobserved;

                /// <summary>
                /// Create an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static MLRaycastQueryNative Create()
                {
                    return new MLRaycastQueryNative()
                    {
                        Position = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        Direction = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        UpVector = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        Width = 0u,
                        Height = 0u,
                        HorizontalFovDegrees = 0.0f,
                        CollideWithUnobserved = false,
                    };
                }
            }

            /// <summary>
            /// Result of a ray cast.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MLRaycastResultNative
            {
                /// <summary>
                /// Where in the world the collision happened.
                /// This field is only valid if the state is either HitUnobserved or HitObserved.
                /// </summary>
                public MLVec3f Hitpoint;

                /// <summary>
                /// Normal to the surface where the ray collided.
                /// This field is only valid if the state is either HitUnobserved or HitObserved.
                /// </summary>
                public MLVec3f Normal;

                /// <summary>
                /// Confidence of the ray cast result. Confidence is a non-negative value from 0 to 1 where closer
                /// to 1 indicates a higher quality.It is an indication of how confident we are about ray cast result
                /// and underlying 3D shape.
                /// This field is only valid if the state is either HitUnobserved or HitObserved.
                /// </summary>
                public float Confidence;

                /// <summary>
                /// The ray cast result. If this field is either RequestFailed or NoCollision, fields in this structure are invalid.
                /// </summary>
                public MLRaycast.ResultState State;

                /// <summary>
                /// Create an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static MLRaycastResultNative Create()
                {
                    return new MLRaycastResultNative()
                    {
                        Hitpoint = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        Normal = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        Confidence = 0.0f,
                        State = MLRaycast.ResultState.NoCollision,
                    };
                }
            }
        }
    }
}

#endif
