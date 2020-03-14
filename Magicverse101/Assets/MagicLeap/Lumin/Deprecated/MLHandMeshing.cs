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
    using System;

    /// <summary>
    /// The MLHandMeshing API is used to request for the mesh information of the hands.
    /// </summary>
    public sealed partial class MLHandMeshing : MLAPISingleton<MLHandMeshing>
    {
        /// <summary>
        /// Hand Mesh Request Callback delegate.
        /// </summary>
        /// <param name="result">Result of request.</param>
        /// <param name="meshData">Mesh Data of the request if result is ok. Otherwise, meshData.MeshBlock is null.</param>
        [Obsolete("Please use MLHandMeshing.RequestHandMeshCallback instead.", true)]
        public delegate void HandMeshRequestCallback(MLResult result, MLHandMesh meshData);

        /// <summary>
        /// Requests for the hand mesh and executes the callback when the request is done.
        /// </summary>
        /// <param name="callback">Callback to execute when the request is done.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// </returns>
        [Obsolete("Please use MLHandMeshing.RequestHandMesh with the MLHandMeshing.RequestHandMeshCallback delegate instead.", true)]
        public static MLResult RequestHandMesh(HandMeshRequestCallback callback)
        {
            return MLResult.Create(MLResult.Code.UnspecifiedFailure);
        }
    }
}

#endif
