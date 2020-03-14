// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLFoundObjects.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// Manages calls to the native MLFoundObjects bindings.
    /// </summary>
    public sealed partial class MLFoundObjects : MLAPISingleton<MLFoundObjects>
    {
        /// <summary>
        /// Stores all pending found object queries.
        /// </summary>
        private readonly Dictionary<ulong, NativeBindings.Query> pendingQueries = new Dictionary<ulong, NativeBindings.Query>();

        /// <summary>
        /// Keeps the queries that were completed on a specific frame.
        /// </summary>
        private Dictionary<ulong, List<Tuple<NativeBindings.Query, FoundObject, List<KeyValuePair<string, string>>>>> completedQueries = new Dictionary<ulong, List<Tuple<NativeBindings.Query, FoundObject, List<KeyValuePair<string, string>>>>>();

        /// <summary>
        /// Keeps the queries that failed on a specific frame.
        /// </summary>
        private List<ulong> errorQueries = new List<ulong>();

        /// <summary>
        /// Stores the found object system tracker.
        /// </summary>
        private ulong tracker = MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// Query result delegate for found objects.
        /// </summary>
        /// <param name="foundObject">The found object that was detected.</param>
        /// <param name="properties">The properties for the found object.</param>
        public delegate void QueryResultsDelegate(FoundObject foundObject, List<KeyValuePair<string, string>> properties);

        /// <summary>
        /// Starts the MLFoundObjects API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLFoundObjects.BaseStart(true);
        }

        /// <summary>
        /// Request the list of detected found objects.
        /// Callback will never be called while request is still pending.
        /// </summary>
        /// <param name="callback">
        /// Callback used to report query results.
        /// Callback MLResult code will never be <c>MLResult.Code.Pending</c>.
        /// </param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        public static MLResult GetObjects(QueryResultsDelegate callback)
        {
            if (MLFoundObjects.IsValidInstance())
            {
                // Don't allow null callbacks to be registered.
                if (callback == null)
                {
                    MLPluginLog.Error("MLFoundObject.GetObjects failed. Reason: Passed input callback is null.");
                    return MLResult.Create(MLResult.Code.InvalidParam);
                }

                return _instance.BeginObjectQuery(callback);
            }
            else
            {
                MLPluginLog.ErrorFormat("MLFoundObject.GetObjects failed. Reason: No Instance for MLFoundObject");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLFoundObject.GetFoundObjects failed. Reason: No Instance for MLFoundObject");
            }
        }

        #if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Starts the found object requests, Must be called to start receiving found object results from the underlying system.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        protected override MLResult StartAPI()
        {
            _instance.tracker = MagicLeapNativeBindings.InvalidHandle;

            return _instance.CreateTracker();
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
                _instance.pendingQueries.Clear();
            }

            _instance.DestroyNativeTracker();
        }

        /// <summary>
        /// Polls for the result of pending found object requests.
        /// </summary>
        protected override void Update()
        {
            _instance.ProcessPendingQueries();
        }

        /// <summary>
        /// static instance of the MLFoundObjects class
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLFoundObjects.IsValidInstance())
            {
                MLFoundObjects._instance = new MLFoundObjects();
            }
        }

        /// <summary>
        /// Create a new found object native tracker.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        private MLResult CreateTracker()
        {
            try
            {
                MLResult.Code resultCode = NativeBindings.MLFoundObjectTrackerCreate(out _instance.tracker);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLFoundObject.CreateTracker failed to initialize native tracker. Reason: {0}", resultCode);
                }

                return MLResult.Create(resultCode);
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLFoundObject.CreateTracker failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLFoundObject.CreateTracker failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Destroy the found object native tracker.
        /// </summary>
        private void DestroyNativeTracker()
        {
            try
            {
                if (!MagicLeapNativeBindings.MLHandleIsValid(_instance.tracker))
                {
                    return;
                }

                MLResult.Code resultCode = NativeBindings.MLFoundObjectTrackerDestroy(_instance.tracker);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLFoundObject.DestroyNativeTracker failed to destroy found object tracker. Reason: {0}", MLResult.CodeToString(resultCode));
                }

                _instance.tracker = MagicLeapNativeBindings.InvalidHandle;
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLFoundObject.DestroyNativeTracker failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Begin querying for found objects.
        /// </summary>
        /// <param name="callback">Callback used to report query results.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        private MLResult BeginObjectQuery(QueryResultsDelegate callback)
        {
            try
            {
                if (!MagicLeapNativeBindings.MLHandleIsValid(_instance.tracker))
                {
                    MLPluginLog.Error("MLFoundObject.BeginObjectQuery failed to request found objects. Reason: Tracker handle is invalid");
                    return MLResult.Create(MLResult.Code.InvalidParam);
                }

                NativeBindings.QueryFilterNative queryFilter = new NativeBindings.QueryFilterNative();

                MLResult.Code resultCode = NativeBindings.MLFoundObjectQuery(_instance.tracker, ref queryFilter, out ulong queryHandle);
                MLResult result = MLResult.Create(resultCode);

                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLFoundObject.BeginObjectQuery failed to request objects. Reason: {0}", resultCode);
                    return result;
                }

                // Create query object to prepresent this newly registered found object query.
                NativeBindings.Query query = new NativeBindings.Query(callback, queryFilter, result);
                MLFoundObjects._instance.pendingQueries.Add(queryHandle, query);

                return result;
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLFoundObjects.BeginObjectQuery failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLFoundObjects.BeginObjectQuery failed. Reason: API symbols not found");
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
                        MLResult.Code resultCode = NativeBindings.MLFoundObjectGetResultCount(_instance.tracker, handle, out uint resultCount);

                        if (MLResult.IsOK(resultCode))
                        {
                            for (uint objectIndex = 0; objectIndex < resultCount; objectIndex++)
                            {
                                NativeBindings.FoundObjectNative foundObject = new NativeBindings.FoundObjectNative();

                                resultCode = NativeBindings.MLFoundObjectGetResult(_instance.tracker, handle, objectIndex, ref foundObject);
                                if (MLResult.IsOK(resultCode))
                                {
                                    List<KeyValuePair<string, string>> properties = null;
                                    if (foundObject.PropertyCount > 0)
                                    {
                                        NativeBindings.PropertyNative objectProperty = new NativeBindings.PropertyNative();
                                        properties = new List<KeyValuePair<string, string>>();

                                        for (uint propertyIndex = 0; propertyIndex < foundObject.PropertyCount; propertyIndex++)
                                        {
                                            resultCode = NativeBindings.MLFoundObjectGetProperty(_instance.tracker, foundObject.Id, propertyIndex, ref objectProperty);
                                            if (MLResult.IsOK(resultCode))
                                            {
                                                properties.Add(new KeyValuePair<string, string>(new string(objectProperty.Key), new string(objectProperty.Value)));
                                            }
                                            else
                                            {
                                                MLPluginLog.ErrorFormat("MLFoundObject.ProcessPendingQueries failed to get found object property. Reason: {0}", MLResult.CodeToString(resultCode));
                                            }
                                        }
                                    }

                                    if (!_instance.completedQueries.ContainsKey(handle))
                                    {
                                        _instance.completedQueries.Add(handle, new List<Tuple<NativeBindings.Query, FoundObject, List<KeyValuePair<string, string>>>>());
                                    }

                                    _instance.completedQueries[handle].Add(new Tuple<NativeBindings.Query, FoundObject, List<KeyValuePair<string, string>>>(
                                                                                query,
                                                                                new FoundObject
                                                                                {
                                                                                    Id = MLConvert.ToUnity(foundObject.Id),
                                                                                    PropertyCount = foundObject.PropertyCount,
                                                                                    Position = MLConvert.ToUnity(foundObject.Position),
                                                                                    Rotation = MLConvert.ToUnity(foundObject.Rotation),
                                                                                    Size = MLConvert.ToUnity(foundObject.Size)
                                                                                },
                                                                                properties));
                                }
                                else
                                {
                                    MLPluginLog.ErrorFormat("MLFoundObject.ProcessPendingQueries failed to get found object. Reason: {0}", MLResult.CodeToString(resultCode));
                                    _instance.errorQueries.Add(handle);
                                }
                            }
                        }
                        else
                        {
                            MLPluginLog.ErrorFormat("MLFoundObjects.ProcessPendingQueries failed to query found objects. Reason: {0}", MLResult.CodeToString(resultCode));
                            _instance.errorQueries.Add(handle);
                        }
                    }

                    foreach (ulong handle in _instance.errorQueries)
                    {
                        _instance.pendingQueries.Remove(handle);
                    }

                    _instance.errorQueries.Clear();

                    foreach (KeyValuePair<ulong, List<Tuple<NativeBindings.Query, FoundObject, List<KeyValuePair<string, string>>>>> handle in _instance.completedQueries)
                    {
                        for (int i = 0; i < handle.Value.Count; ++i)
                        {
                            handle.Value[i].Item1.Callback(handle.Value[i].Item2, handle.Value[i].Item3);
                        }

                        _instance.pendingQueries.Remove(handle.Key);
                    }

                    _instance.completedQueries.Clear();
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLFoundObjects.ProcessPendingQueries failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Contains information about the found object.
        /// </summary>
        public struct FoundObject
        {
            /// <summary>
            /// The identifier of the Found Object.
            /// </summary>
            public System.Guid Id;

            /// <summary>
            /// The number of properties.
            /// </summary>
            public uint PropertyCount;

            /// <summary>
            /// The center position of found object.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The rotation of found object.
            /// </summary>
            public Quaternion Rotation;

            /// <summary>
            /// The Vector3 extents of the object where each dimension is defined as max-min.
            /// </summary>
            public Vector3 Size;
        }
    }
}

#endif
