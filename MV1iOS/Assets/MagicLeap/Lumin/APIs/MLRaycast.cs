// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLRaycast.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using System.Collections.Generic;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Sends requests to create Rays intersecting world geometry and returns results through callbacks.
    /// </summary>
    public partial class MLRaycast : MLAPISingleton<MLRaycast>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// Contains all pending queries.
        /// </summary>
        private readonly Dictionary<ulong, OnRaycastResultDelegate> pendingQueries = new Dictionary<ulong, OnRaycastResultDelegate>();

        /// <summary>
        /// Tracks the latest query added.
        /// </summary>
        private NativeBindings.MLRaycastQueryNative currentQuery = NativeBindings.MLRaycastQueryNative.Create();

        /// <summary>
        /// Stores the ray cast system tracker.
        /// </summary>
        private ulong trackerHandle = MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// Keeps the queries that were completed on a specific frame.
        /// </summary>
        private Dictionary<ulong, NativeBindings.MLRaycastResultNative> completedQueries = new Dictionary<ulong, NativeBindings.MLRaycastResultNative>();

        /// <summary>
        /// Keeps the queries that failed on a specific frame.
        /// </summary>
        private List<ulong> errorQueries = new List<ulong>();

        /// <summary>
        /// Delegate used to convey the result of a ray cast.
        /// </summary>
        /// <param name="state">The state of the ray cast result.</param>
        /// <param name="hitpoint">Where in the world the collision happened.</param>
        /// <param name="normal">Normal to the surface where the ray collided.</param>
        /// <param name="confidence">The confidence of the ray cast result. Confidence is a non-negative value from 0 to 1 where closer to 1 indicates a higher quality.</param>
        /// \internal
        /// CAPI has custom result MLWorldRaycastResultState, thus we expose it instead of MLResult.
        /// \endinternal
        public delegate void OnRaycastResultDelegate(ResultState state, Vector3 hitpoint, Vector3 normal, float confidence);
        #endif

        /// <summary>
        /// Enumeration of ray cast result states.
        /// </summary>
        public enum ResultState
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
        /// Starts the World Rays API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLRaycast.BaseStart(true);
        }

        /// <summary>
        /// Requests a ray cast with the given query parameters.
        /// </summary>
        /// <param name="query">Query parameters describing ray being cast.</param>
        /// <param name="callback">Delegate which will be called when the result of the ray cast is ready.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        public static MLResult Raycast(QueryParams query, OnRaycastResultDelegate callback)
        {
            try
            {
                if (MLRaycast.IsValidInstance())
                {
                    if (query == null || callback == null)
                    {
                        MLPluginLog.ErrorFormat("MLRaycast.Raycast failed. Reason: Invalid input parameters.");
                        return MLResult.Create(MLResult.Code.InvalidParam);
                    }

                    _instance.currentQuery.Position = MLConvert.FromUnity(query.Position);
                    _instance.currentQuery.Direction = MLConvert.FromUnity(query.Direction, true, false);
                    _instance.currentQuery.UpVector = MLConvert.FromUnity(query.UpVector, true, false);
                    _instance.currentQuery.Width = query.Width;
                    _instance.currentQuery.Height = query.Height;
                    _instance.currentQuery.HorizontalFovDegrees = query.HorizontalFovDegrees;
                    _instance.currentQuery.CollideWithUnobserved = query.CollideWithUnobserved;

                    ulong queryHandle = MagicLeapNativeBindings.InvalidHandle;
                    MLResult.Code resultCode = NativeBindings.MLRaycastRequest(_instance.trackerHandle, ref _instance.currentQuery, ref queryHandle);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLRaycast.Raycast failed to request a new ray cast. Reason: {0}", result);
                        return result;
                    }

                    _instance.pendingQueries.Add(queryHandle, callback);
                    return MLResult.Create(MLResult.Code.Ok);
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLRaycast.Raycast failed. Reason: No Instance for MLRaycast");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLRaycast.Raycast failed. Reason: No Instance for MLRaycast");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLRaycast.Raycast failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLRaycast.Raycast failed. Reason: API symbols not found");
            }
        }

        #if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Creates a new ray cast tracker.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        protected override MLResult StartAPI()
        {
            _instance.trackerHandle = MagicLeapNativeBindings.InvalidHandle;

            MLResult.Code resultCode = NativeBindings.MLRaycastCreate(ref _instance.trackerHandle);
            if (resultCode != MLResult.Code.Ok)
            {
                MLResult result = MLResult.Create(resultCode);
                MLPluginLog.ErrorFormat("MLRaycast.StartAPI failed to create input tracker. Reason: {0}", result);
                return result;
            }

            if (!MagicLeapNativeBindings.MLHandleIsValid(_instance.trackerHandle))
            {
                MLPluginLog.Error("MLRaycast.StartAPI failed. Reason: Invalid handle returned when initializing an instance of MLRaycast");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure);
            }

            return MLResult.Create(MLResult.Code.Ok);
        }
        #endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Polls for the result of pending ray cast requests.
        /// </summary>
        protected override void Update()
        {
            try
            {
                if (_instance.pendingQueries.Count > 0)
                {
                    foreach (ulong handle in _instance.pendingQueries.Keys)
                    {
                        NativeBindings.MLRaycastResultNative raycastResult = NativeBindings.MLRaycastResultNative.Create();

                        MLResult.Code resultCode = NativeBindings.MLRaycastGetResult(_instance.trackerHandle, handle, ref raycastResult);
                        if (resultCode == MLResult.Code.Ok)
                        {
                            _instance.completedQueries.Add(handle, raycastResult);
                        }
                        else if(resultCode != MLResult.Code.Pending)
                        {
                            MLPluginLog.ErrorFormat("MLRaycast.Update failed to get raycast result. Reason: {0}", MLResult.CodeToString(resultCode));
                            _instance.errorQueries.Add(handle);
                        }
                    }

                    foreach (ulong handle in _instance.errorQueries)
                    {
                        _instance.pendingQueries.Remove(handle);
                    }
                    _instance.errorQueries.Clear();

                    foreach (KeyValuePair<ulong, NativeBindings.MLRaycastResultNative> handle in _instance.completedQueries)
                    {
                        // Check if there is a valid hit result.
                        bool didHit = handle.Value.State != ResultState.RequestFailed && handle.Value.State != ResultState.NoCollision;

                        _instance.pendingQueries[handle.Key](
                             handle.Value.State,
                            didHit ? MLConvert.ToUnity(handle.Value.Hitpoint) : Vector3.zero,
                            didHit ? MLConvert.ToUnity(handle.Value.Normal, true, false) : Vector3.zero,
                             handle.Value.Confidence);

                        _instance.pendingQueries.Remove(handle.Key);
                    }
                    _instance.completedQueries.Clear();
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLRaycast.Update failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Cleans up memory and destroys the ray cast tracker.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Allow complete cleanup of the API.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            if (isSafeToAccessManagedObjects)
            {
                _instance.pendingQueries.Clear();
            }

            _instance.DestroyNativeTracker();
        }

        /// <summary>
        /// static instance of the <c>MLRaycast</c> class
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLRaycast.IsValidInstance())
            {
                MLRaycast._instance = new MLRaycast();
            }
        }

        /// <summary>
        /// Destroys the native ray cast tracker.
        /// </summary>
        private void DestroyNativeTracker()
        {
            try
            {
                if (NativeBindings.MLHandleIsValid(_instance.trackerHandle))
                {
                    MLResult.Code resultCode = NativeBindings.MLRaycastDestroy(_instance.trackerHandle);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLRaycast.DestroyNativeTracker failed to destroy raycast tracker. Reason: {0}", NativeBindings.MLGetResultString(resultCode));
                    }

                    _instance.trackerHandle = MagicLeapNativeBindings.InvalidHandle;
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLRaycast.DestroyNativeTracker failed. Reason: API symbols not found");
                _instance.trackerHandle = MagicLeapNativeBindings.InvalidHandle;
            }
        }

        /// <summary>
        /// Parameters for a ray cast request.
        /// </summary>
        public class QueryParams
        {
            /// <summary>
            /// Gets or sets where the ray is cast from.
            /// </summary>
            public Vector3 Position { get; set; } = Vector3.zero;

            /// <summary>
            /// Gets or sets the direction of the ray to fire.
            /// </summary>
            public Vector3 Direction { get; set; } = Vector3.forward;

            /// <summary>
            /// Gets or sets the up vector of the ray to fire.  Use (0, 0, 0) to use the up vector of the rig frame.
            /// </summary>
            public Vector3 UpVector { get; set; } = Vector3.zero;

            /// <summary>
            /// Gets or sets the number of horizontal rays. For single point ray cast, set this to 1.
            /// </summary>
            public uint Width { get; set; } = 1;

            /// <summary>
            /// Gets or sets the number of vertical rays. For single point ray cast, set this to 1.
            /// </summary>
            public uint Height { get; set; } = 1;

            /// <summary>
            /// Gets or sets the horizontal field of view, in degrees.
            /// </summary>
            public float HorizontalFovDegrees { get; set; } = 50.0f;

            /// <summary>
            /// Gets or sets a value indicating whether a ray will terminate when encountering an unobserved area and return
            /// a surface or the ray will continue until it ends or hits a observed surface.
            /// </summary>
            public bool CollideWithUnobserved { get; set; } = false;
        }
        #endif
    }
}
