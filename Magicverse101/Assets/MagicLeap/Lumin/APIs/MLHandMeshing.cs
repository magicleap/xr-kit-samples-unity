// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHandMeshing.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// The MLHandMeshing API is used to request for the mesh information of the hands.
    /// </summary>
    public sealed partial class MLHandMeshing : MLAPISingleton<MLHandMeshing>
    {
        /// <summary>
        /// Handle to the native Hand Meshing tracker.
        /// </summary>
        private ulong nativeTracker = MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// Holds all the queries that are not yet completed.
        /// </summary>
        private List<Query> pendingQueries = new List<Query>();

        /// <summary>
        /// Holds all the queries that have been completed.
        /// </summary>
        private List<Query> completedQueries = new List<Query>();

        /// <summary>
        /// The native structure used to get hand meshing query results.
        /// </summary>
        private NativeBindings.MeshNative handMeshNativeStruct = NativeBindings.MeshNative.Create();

        /// <summary>
        /// Request Hand Mesh Callback delegate.
        /// </summary>
        /// <param name="result">Result of request.</param>
        /// <param name="meshData">Mesh Data of the request if result is ok. Otherwise, meshData.MeshBlock is null.</param>
        public delegate void RequestHandMeshCallback(MLResult result, MLHandMeshing.Mesh meshData);

        /// <summary>
        /// Starts the MLHandMeshing API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLHandMeshing.BaseStart(true);
        }

        /// <summary>
        /// Requests for the hand mesh and executes the callback when the request is done.
        /// </summary>
        /// <param name="callback">Callback to execute when the request is done.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// </returns>
        public static MLResult RequestHandMesh(RequestHandMeshCallback callback)
        {
            if (callback == null)
            {
                MLPluginLog.ErrorFormat("MLHandMeshing.RequestHandMesh failed. Reason: Passed input callback is null");
                return MLResult.Create(MLResult.Code.InvalidParam);
            }

            ulong requestHandle = MagicLeapNativeBindings.InvalidHandle;
            MLResult.Code resultCode = NativeBindings.MLHandMeshingRequestMesh(Instance.nativeTracker, ref requestHandle);
            if (resultCode != MLResult.Code.Ok)
            {
                MLPluginLog.ErrorFormat("MLHandMeshing.RequestHandMesh failed to request hand mesh. Reason: {0}", resultCode);
                return MLResult.Create(resultCode);
            }

            Instance.pendingQueries.Add(new Query(requestHandle, callback));

            return MLResult.Create(resultCode);
        }

        #if !DOXYGENSHOULDSKIPTHIS
        /// <summary>
        /// Initializes the HandMeshing API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        protected override MLResult StartAPI()
        {
            MLResult.Code resultCode = this.InitNativeTracker();
            MLResult result = MLResult.Create(resultCode);
            if (!result.IsOk)
            {
                MLPluginLog.ErrorFormat("MLHandMeshing.StartAPI failed to initialize native hand tracker. Reason: {0}", result);
            }

            return result;
        }
        #endif // DOXYGENSHOULDSKIPTHIS

        /// <summary>
        /// Cleans up unmanaged memory. Frees up resources of pending queries.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Allow complete cleanup of the API.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            if (isSafeToAccessManagedObjects)
            {
                foreach (Query query in this.pendingQueries)
                {
                    ulong requestHandle = query.RequestHandle;
                    NativeBindings.MLHandMeshingFreeResource(this.nativeTracker, ref requestHandle);
                }

                this.pendingQueries.Clear();
            }

            this.DestroyNativeTracker();
        }

        /// <summary>
        /// Processes pending and completed queries.
        /// </summary>
        protected override void Update()
        {
            this.ProcessPendingQueries();
            this.ProcessCompletedQueries();
            this.completedQueries.Clear();
        }

        /// <summary>
        /// Creates static instance of MLHandMeshing class.
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLHandMeshing.IsValidInstance())
            {
                MLHandMeshing._instance = new MLHandMeshing();
            }
        }

        /// <summary>
        /// Destroys the native Hand Meshing client.
        /// </summary>
        private void DestroyNativeTracker()
        {
            if (!MagicLeapNativeBindings.MLHandleIsValid(this.nativeTracker))
            {
                return;
            }

            MLResult.Code result = NativeBindings.MLHandMeshingDestroyClient(ref this.nativeTracker);
            if (!MLResult.IsOK(result))
            {
                MLPluginLog.ErrorFormat("MLHandMeshing.DestroyNativeTracker failed to destroy native hand tracker. Reason: {0}", result);
            }

            this.nativeTracker = MagicLeapNativeBindings.InvalidHandle;
        }

        /// <summary>
        /// Initializes the native Hand Meshing client.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid handle.
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the Hand Meshing client was created successfully.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> > if there was a lack of privileges.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
        /// </returns>
        private MLResult.Code InitNativeTracker()
        {
            this.nativeTracker = MagicLeapNativeBindings.InvalidHandle;
            return NativeBindings.MLHandMeshingCreateClient(ref this.nativeTracker);
        }

        /// <summary>
        /// Processes the current pending queries and adds them to the completed queries list when finished.
        /// Queries that are still pending are skipped from being added to the completed queries list until next Update loop.
        /// </summary>
        private void ProcessPendingQueries()
        {
            foreach (Query query in this.pendingQueries)
            {
                MLResult.Code resultCode = NativeBindings.MLHandMeshingGetResult(this.nativeTracker, query.RequestHandle, ref this.handMeshNativeStruct);
                if (resultCode == MLResult.Code.Pending)
                {
                    continue;
                }

                query.Result = MLResult.Create(resultCode);
                if (resultCode == MLResult.Code.Ok)
                {
                    query.HandMesh = this.handMeshNativeStruct.Data;
                }

                this.completedQueries.Add(query);

                ulong requestHandle = query.RequestHandle;
                resultCode = NativeBindings.MLHandMeshingFreeResource(this.nativeTracker, ref requestHandle);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLHandMeshing.ProcessPendingQueries failed to free resource. Reason: {0}", resultCode);
                }
            }
        }

        /// <summary>
        /// Calls the RequestCallback for all current completed queries.
        /// </summary>
        private void ProcessCompletedQueries()
        {
            foreach (Query query in this.completedQueries)
            {
                query.RequestCallback(query.Result, query.HandMesh);

                this.pendingQueries.Remove(query);
            }
        }

        /// <summary>
        /// Structure to represent a hand mesh.
        /// </summary>
        public struct Mesh
        {
            /// <summary>
            /// Gets the array of mesh blocks.
            /// </summary>
            public Block[] MeshBlock { get; internal set; }

            /// <summary>
            /// Structure to represent a mesh block
            /// </summary>
            public struct Block
            {
                /// <summary>
                /// Gets the array of vertex positions relative to the origin.
                /// This can be directly plugged into Mesh.vertices.
                /// </summary>
                public Vector3[] Vertex { get; internal set; }

                /// <summary>
                /// Gets the array of Indices. Guaranteed to be a multiple of 3. Every 3 indices creates a triangles.
                /// This can be directly plugged into Mesh.triangles.
                /// </summary>
                public int[] Index { get; internal set; }
            }
        }

        /// <summary>
        /// Object used to hold the results of a query, including it's request handle and callback.
        /// </summary>
        private sealed class Query
        {
            /// <summary>
            /// Handle to the query request.
            /// </summary>
            public readonly ulong RequestHandle;

            /// <summary>
            /// Callback used for when the query has a result.
            /// </summary>
            public readonly RequestHandMeshCallback RequestCallback;

            /// <summary>
            /// Initializes a new instance of the <see cref="Query" /> class.
            /// </summary>
            /// <param name="reqHandle">The handle to the query request.</param>
            /// <param name="reqCallback">The callback to use when the query has a result.</param>
            public Query(ulong reqHandle, RequestHandMeshCallback reqCallback)
            {
                this.RequestHandle = reqHandle;
                this.RequestCallback = reqCallback;
            }

            /// <summary>
            /// Prevents a default instance of the <see cref="Query" /> class from being created.
            /// </summary>
            private Query()
            {
            }

            /// <summary>
            /// Gets or sets the result of the query.
            /// </summary>
            public MLResult Result { get; set; }

            /// <summary>
            /// Gets or sets the hand meshing data of the query result.
            /// </summary>
            public MLHandMeshing.Mesh HandMesh { get; set; }
        }
    }
}

#endif
