// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHandMeshingNativeBindings.cs" company="Magic Leap, Inc">
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
    /// The MLHandMeshing API is used to request for the mesh information of the hands.
    /// </summary>
    public sealed partial class MLHandMeshing : MLAPISingleton<MLHandMeshing>
    {
        /// <summary>
        /// The native bindings to the Hand Meshing API.
        /// See ml_hand_meshing.h for additional comments
        /// </summary>
        private partial class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// Creates the Hand Meshing client.
            /// Note that this will be the only function in the HandMeshing API that will return <c>MLResult.Code.PrivilegeDenied</c>.
            /// Trying to call the other functions with an invalid handle will result in <c>MLResult.Code.InvalidParam</c>.
            /// </summary>
            /// <param name="handle">Handle to the created Hand Meshing client.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid handle.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the Hand Meshing client was created successfully.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> > if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLHandMeshingCreateClient(ref ulong handle);

            /// <summary>
            /// Destroys the Hand Meshing client.
            /// </summary>
            /// <param name="handle">Handle to the Hand Meshing client to destroy.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid handle.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the Hand Meshing client was destroyed successfully.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLHandMeshingDestroyClient(ref ulong handle);

            /// <summary>
            /// Requests the hand mesh.
            /// </summary>
            /// <param name="handle">Handle to the created Hand Meshing client.</param>
            /// <param name="requestHandle">Handle to the current query request.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid handle.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the hand mesh was requested successfully.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLHandMeshingRequestMesh(ulong handle, ref ulong requestHandle);

            /// <summary>
            /// Gets the Result of a previous hand mesh request
            /// </summary>
            /// <param name="handle">Handle to the created Hand Meshing client.</param>
            /// <param name="requestHandle">Handle received from a previous MLHandMeshingRequestMesh call.</param>
            /// <param name="mesh">The mesh object which will be populated only if the result is successful.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the mesh object was populated successfully.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the mesh result is pending a update.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLHandMeshingGetResult(ulong handle, ulong requestHandle, ref MeshNative mesh);

            /// <summary>
            /// Free the resources created by the hand meshing API. Needs to be called whenever MLHandMeshingGetResult returns a success.
            /// </summary>
            /// <param name="handle">Handle to the created Hand Meshing client.</param>
            /// <param name="requestHandle">Handle received from a previous MLHandMeshingRequestMesh call.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid handle.
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the resources were freed successfully.
            /// </returns>
            [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLHandMeshingFreeResource(ulong handle, ref ulong requestHandle);

            /// <summary>
            /// The native structure for a hand mesh.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MeshNative
            {
                /// <summary>
                /// The current structure version.
                /// </summary>
                public uint Version;

                /// <summary>
                /// The number of data blocks this mesh has.
                /// </summary>
                public uint DataCount;

                /// <summary>
                /// Pointer used to traverse through the different blocks this mesh has.
                /// </summary>
                public IntPtr MeshData; // MLHandMeshBlockNative[]

                /// <summary>
                /// Gets a conversion from this native structure to an external one.
                /// </summary>
                /// <returns> An external structure with a copy of the data from this native structure.</returns>
                public MLHandMeshing.Mesh Data
                {
                    get
                    {
                        int sizeOfMLVec3f = Marshal.SizeOf(typeof(Native.MagicLeapNativeBindings.MLVec3f));
                        int sizeOfHandMeshBlock = Marshal.SizeOf(typeof(NativeBindings.MeshNative.BlockNative));

                        IntPtr blockIterator = this.MeshData;
                        MLHandMeshing.Mesh handMesh = new MLHandMeshing.Mesh
                        {
                            MeshBlock = new MLHandMeshing.Mesh.Block[this.DataCount]
                        };

                        for (uint i = 0; i < this.DataCount; ++i)
                        {
                            NativeBindings.MeshNative.BlockNative meshBlockNative = Marshal.PtrToStructure<NativeBindings.MeshNative.BlockNative>(blockIterator);

                            handMesh.MeshBlock[i].Vertex = new Vector3[meshBlockNative.VertexCount];
                            IntPtr vertexIterator = meshBlockNative.Vertex;
                            for (uint j = 0; j < meshBlockNative.VertexCount; ++j)
                            {
                                Native.MagicLeapNativeBindings.MLVec3f vec3Native = Marshal.PtrToStructure<Native.MagicLeapNativeBindings.MLVec3f>(vertexIterator);
                                handMesh.MeshBlock[i].Vertex[j] = Native.MLConvert.ToUnity(vec3Native);
                                vertexIterator = new IntPtr(vertexIterator.ToInt64() + sizeOfMLVec3f);
                            }

                            short[] indexBuffer = new short[(int)meshBlockNative.IndexCount];
                            Marshal.Copy(meshBlockNative.Index, indexBuffer, 0, (int)meshBlockNative.IndexCount);
                            handMesh.MeshBlock[i].Index = new int[meshBlockNative.IndexCount];
                            for (ushort k = 0; k < meshBlockNative.IndexCount; ++k)
                            {
                                handMesh.MeshBlock[i].Index[k] = (int)indexBuffer[k];
                            }

                            blockIterator = new IntPtr(blockIterator.ToInt64() + sizeOfHandMeshBlock);
                        }

                        return handMesh;
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>An initialized version of this struct.</returns>
                public static MeshNative Create()
                {
                    return new MeshNative
                    {
                        Version = 1u,
                        DataCount = 0u,
                        MeshData = IntPtr.Zero
                    };
                }

                /// <summary>
                /// The native structure for a hand mesh block.
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public struct BlockNative
                {
                    /// <summary>
                    /// The number of indices in this block.
                    /// </summary>
                    public ushort IndexCount;

                    /// <summary>
                    /// The number of vertices in this block.
                    /// </summary>
                    public uint VertexCount;

                    /// <summary>
                    /// Pointer to the array containing all the vertices for this block.
                    /// </summary>
                    public IntPtr Vertex; // MLVec3f[]

                    /// <summary>
                    /// Pointer to the array containing all the indices for this block.
                    /// </summary>
                    public IntPtr Index; // ushort[]

                    /// <summary>
                    /// Create and return an initialized version of this struct.
                    /// </summary>
                    /// <returns>An initialized version of this struct.</returns>
                    public static BlockNative Create()
                    {
                        return new BlockNative
                        {
                            IndexCount = 0,
                            VertexCount = 0u,
                            Vertex = IntPtr.Zero,
                            Index = IntPtr.Zero
                        };
                    }
                }
            }
        }
    }
}

#endif
