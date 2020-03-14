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
            [Obsolete("Please use MLHandMeshing.NativeBindings.MLHandMeshingGetResult(ulong, ulong, NativeBindings.MeshNative) instead.", false)]
            public static extern MLResult.Code MLHandMeshingGetResult(ulong handle, ulong requestHandle, ref MLHandMeshNative mesh);

            /// <summary>
            /// The native structure for a hand mesh.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            [Obsolete("Please use MLHandMeshing.NativeBindings.MeshNative instead.", false)]
            public struct MLHandMeshNative
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
                        int sizeOfHandMeshBlock = Marshal.SizeOf(typeof(NativeBindings.MLHandMeshBlockNative));

                        IntPtr blockIterator = this.MeshData;
                        MLHandMeshing.Mesh handMesh = new MLHandMeshing.Mesh
                        {
                            MeshBlock = new MLHandMeshing.Mesh.Block[this.DataCount]
                        };

                        for (uint i = 0; i < this.DataCount; ++i)
                        {
                            NativeBindings.MLHandMeshBlockNative meshBlockNative = Marshal.PtrToStructure<NativeBindings.MLHandMeshBlockNative>(blockIterator);

                            handMesh.MeshBlock[i].Vertex = new Vector3[meshBlockNative.VertexCount];
                            IntPtr vertexIterator = meshBlockNative.Vertex;
                            for (uint j = 0; j < meshBlockNative.VertexCount; ++j)
                            {
                                NativeBindings.MLVec3f vec3Native = Marshal.PtrToStructure<NativeBindings.MLVec3f>(vertexIterator);
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
                public static MLHandMeshNative Create()
                {
                    return new MLHandMeshNative
                    {
                        Version = 1u,
                        DataCount = 0u,
                        MeshData = IntPtr.Zero
                    };
                }
            }

            /// <summary>
            /// The native structure for a hand mesh block.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            [Obsolete("Please use MLHandMeshing.NativeBindings.MeshNative.BlockNative instead.", false)]
            public struct MLHandMeshBlockNative
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
                public static MLHandMeshBlockNative Create()
                {
                    return new MLHandMeshBlockNative
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

#endif
