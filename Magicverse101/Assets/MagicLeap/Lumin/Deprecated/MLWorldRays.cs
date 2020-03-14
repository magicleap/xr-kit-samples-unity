// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLWorldRays.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// Sends requests to create Rays intersecting world geometry and returns results through callbacks.
    /// </summary>
    [Obsolete("Please use MLRaycast instead.")]
    public sealed class MLWorldRays : MLRaycast
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// Delegate used to convey the result of a ray cast.
        /// </summary>
        /// <param name="state">The state of the ray cast result.</param>
        /// <param name="hitpoint">Where in the world the collision happened.</param>
        /// <param name="normal">Normal to the surface where the ray collided.</param>
        /// <param name="confidence">The confidence of the ray cast result. Confidence is a non-negative value from 0 to 1 where closer to 1 indicates a higher quality.</param>
        [Obsolete("Please use MLRaycast.OnRaycastResultDelegate(MLRaycast.ResultState, Vector3, Vector3, float) instead.", true)]
        public delegate void ResultCallback(MLWorldRaycastResultState state, Vector3 hitpoint, Vector3 normal, float confidence);
        #endif

        /// <summary>
        /// Enumeration of ray cast result states.
        /// </summary>
        [Obsolete("Please use MLRaycast.ResultState instead.")]
        public enum MLWorldRaycastResultState
        {
            /// <summary>
            /// The ray cast request failed.
            /// </summary>
            RequestFailed = -1,

            /// <summary>
            /// The ray passed beyond maximum ray cast distance and it doesn't hit any surface.
            /// </summary>
            NoCollision,

            /// <summary>
            /// The ray hit unobserved area. This will on occur when collide_with_unobserved is set to true.
            /// </summary>
            HitUnobserved,

            /// <summary>
            /// The ray hit only observed area.
            /// </summary>
            HitObserved,
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Requests a ray cast with the given query parameters.
        /// </summary>
        /// <param name="query">Query parameters describing ray being cast.</param>
        /// <param name="callback">Delegate which will be called when the result of the ray cast is ready.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        [Obsolete("Please use MLRaycast.Raycast(QueryParams, MLRaycast.OnRaycastResultDelegate) instead.", true)]
        public static MLResult GetWorldRays(QueryParams query, ResultCallback callback)
        {
            return MLResult.Create(MLResult.Code.UnspecifiedFailure);
        }
        #endif
    }
}
