// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLScreensNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.XR.MagicLeap;

// Disable warnings about missing documentation for native interop.
#pragma warning disable 1591

namespace UnityEngine.XR.MagicLeap.Native
{
    /// <summary>
    /// See ml_screens.h for additional comments
    /// </summary>
    [System.Obsolete("Deprecated and scheduled for removal.", true)]
    public class MLScreensNativeBindings : MagicLeapNativeBindings
    {
        public const string MLScreensDll = "ml_screens";

        private MLScreensNativeBindings() { }

        [StructLayout(LayoutKind.Sequential)]
        public struct MLScreensWatchHistoryListNative
        {
            public uint Count;

            // Pointer to array of MLScreensWatchHistoryEntryNative
            public IntPtr Entries;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            public static MLScreensWatchHistoryListNative Create()
            {
                return new MLScreensWatchHistoryListNative()
                {
                    Count = 0u,
                    Entries = IntPtr.Zero
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MLScreensScreenInfoNative
        {
            public MLTransform Transform;
            public MLVec3f Dimensions;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            public static MLScreensScreenInfoNative Create()
            {
                return new MLScreensScreenInfoNative()
                {
                    Transform = MLTransform.Identity(),
                    Dimensions = new MLVec3f() {X = 0.0f, Y = 0.0f, Z = 0.0f},
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MLScreensScreenInfoExNative
        {
            public uint Version;
            public ulong ScreenId;
            public MLVec3f Dimensions;
            public MLMat4f Transform;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            public static MLScreensScreenInfoExNative Create()
            {
                return new MLScreensScreenInfoExNative()
                {
                    Version = 1u,
                    ScreenId = 0,
                    Dimensions = new MLVec3f() {X = 1.0f, Y = 0.5625f, Z = 0.1f},
                    Transform = new MLMat4f() {MatrixColmajor = new float[16]}
                };
            }

            /// <summary>
            /// Provides easy conversion from the native structure to the external one
            /// and viceversa.
            /// </summary>
            public MLScreensScreenInfo Data
            {
                set
                {
                    ScreenId = value.Id;

                    // transformFromRUF = false, to prevent inverting dimension
                    Dimensions = MLConvert.FromUnity(value.Dimensions, false);

                    MagicLeapNativeBindings.MLQuaternionf nativeRotation = MLConvert.FromUnity(value.Rotation);
                    MagicLeapNativeBindings.MLVec3f nativeTranslation = MLConvert.FromUnity(value.Position);

                    // transformFromRUF = false, to prevent inverting dimension
                    MagicLeapNativeBindings.MLVec3f nativeScale = MLConvert.FromUnity(value.Scale, false);

                    // This logic will be simplified once MLScreensScreenInfoExNative has
                    // a MLTransform (Position, Rotation) and MLVec3f (Scale) instead of a matrix.
                    Quaternion rotationConverted = new Quaternion()
                    {
                        x = nativeRotation.X,
                        y = nativeRotation.Y,
                        z = nativeRotation.Z,
                        w = nativeRotation.W
                    };
                    Vector3 translationConverted = new Vector3()
                    {
                        x = nativeTranslation.X,
                        y = nativeTranslation.Y,
                        z = nativeTranslation.Z
                    };
                    Vector3 scaleConverted = new Vector3()
                    {
                        x = nativeScale.X,
                        y = nativeScale.Y,
                        z = nativeScale.Z
                    };
                    Matrix4x4 matrix = Matrix4x4.TRS(translationConverted, rotationConverted, scaleConverted);
                    float[] matrixColMajor = new float[16];
                    MLConvert.FromUnity(matrix, ref matrixColMajor);
                    Transform.MatrixColmajor = matrixColMajor;
                }
                get
                {
                    // This logic will be simplified once MLScreensScreenInfoExNative has
                    // a MLTransform (Position, Rotation) and MLVec3f (Scale) instead of a matrix.
                    Matrix4x4 matrixUnconverted = MLConvert.ToUnity(Transform);
                    MagicLeapNativeBindings.MLVec3f scaleUnconverted = new MagicLeapNativeBindings.MLVec3f()
                    {
                        X = matrixUnconverted.GetColumn(0).magnitude,
                        Y = matrixUnconverted.GetColumn(1).magnitude,
                        Z = matrixUnconverted.GetColumn(2).magnitude
                    };

                    // Convert To Unity: RUF
                    MagicLeapNativeBindings.MLTransform transform = MLConvert.FromUnity(matrixUnconverted, true, false);
                    MLScreensScreenInfo screenInfo = new MLScreensScreenInfo()
                    {
                        Id = ScreenId,
                        Position = MLConvert.ToUnity(transform.Position, false),
                        Rotation = MLConvert.ToUnity(transform.Rotation, false),
                        Dimensions = MLConvert.ToUnity(Dimensions, false),
                        Scale = MLConvert.ToUnity(scaleUnconverted, false)
                    };

                    return screenInfo;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct MLScreensWatchHistoryEntryNative
        {
            public long Id;
            public IntPtr Title;
            public IntPtr Subtitle;
            public uint PlaybackPositionMs;
            public uint PlaybackDurationMs;
            public IntPtr CustomData;

            /// <summary>
            /// Entry is valid if:
            /// * PlaybackPositionMs is smaller or equal to PlaybackDurationMs.
            /// </summary>
            public bool IsValid
            {
                get
                {
                    return Title != IntPtr.Zero
                        && Subtitle != IntPtr.Zero
                        && PlaybackPositionMs <= PlaybackDurationMs;
                }
            }

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            public static MLScreensWatchHistoryEntryNative Create()
            {
                return new MLScreensWatchHistoryEntryNative()
                {
                    Id = 0,
                    Title = IntPtr.Zero,
                    Subtitle = IntPtr.Zero,
                    PlaybackPositionMs = 0u,
                    PlaybackDurationMs = 0u,
                    CustomData = IntPtr.Zero
                };
            }

            /// <summary>
            /// Provides easy conversion from the native structure to the external one
            /// and viceversa.
            /// </summary>
            public MLScreensWatchHistoryEntry Data
            {
                set
                {
                    Id = value.Id;
                    PlaybackPositionMs = value.PlaybackPositionMs;
                    PlaybackDurationMs = value.PlaybackDurationMs;
                    Title = MLConvert.EncodeToUnmanagedUTF8(value.Title);
                    Subtitle = MLConvert.EncodeToUnmanagedUTF8(value.Subtitle);
                    CustomData = Marshal.AllocHGlobal(1);

                    byte[] buffer = { (byte)'\0'};
                    Marshal.Copy(buffer, 0, CustomData, buffer.Length);
                }
                get
                {
                    MLScreensWatchHistoryEntry entry = new MLScreensWatchHistoryEntry();
                    entry.Id = Id;
                    entry.PlaybackPositionMs = PlaybackPositionMs;
                    entry.PlaybackDurationMs = PlaybackDurationMs;
                    entry.Title = MLConvert.DecodeUTF8(Title);
                    entry.Subtitle = MLConvert.DecodeUTF8(Subtitle);

                    return entry;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MLScreensScreenInfoListNative
        {
            public uint Count;

            // Pointer to array of MLScreensScreenInfoNative
            public IntPtr Entries;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            public static MLScreensScreenInfoListNative Create()
            {
                return new MLScreensScreenInfoListNative()
                {
                    Count = 0u,
                    Entries = IntPtr.Zero
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MLScreensScreenInfoListExNative {
            public uint Version;
            public uint Count;

            // Pointer to array of MLScreensScreenInfoExNative
            public IntPtr Entries;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            public static MLScreensScreenInfoListExNative Create()
            {
                return new MLScreensScreenInfoListExNative()
                {
                    Version = 1u,
                    Count = 0u,
                    Entries = IntPtr.Zero
                };
            }

            public void Initialize()
            {
                Version = 1u;
                Count = 0u;
                Entries = IntPtr.Zero;
            }
        }

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensInsertWatchHistoryEntry(ref MLScreensWatchHistoryEntryNative entry, [In] ref MLImageNativeBindings.MLImageNative thumbnail);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensRemoveWatchHistoryEntry(long entryId);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensUpdateWatchHistoryEntry([In] ref MLScreensWatchHistoryEntryNative entry, [In] ref MLImageNativeBindings.MLImageNative thumbnail);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensGetWatchHistoryList(ref MLScreensWatchHistoryListNative list);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensReleaseWatchHistoryList(ref MLScreensWatchHistoryListNative list);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensGetWatchHistoryThumbnail(long id, ref MLImageNativeBindings.MLImageNative outThumbnail);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensReleaseWatchHistoryThumbnail([In] ref MLImageNativeBindings.MLImageNative thumbnail);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensGetScreenInfoList(ref MLScreensScreenInfoListNative out_list);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensGetScreenInfoListEx(ref MLScreensScreenInfoListExNative out_list);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensReleaseScreenInfoList([In] ref MLScreensScreenInfoListNative list);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensReleaseScreenInfoListEx([In] ref MLScreensScreenInfoListExNative list);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MLScreensGetResultString(MLResult.Code result);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensUpdateScreenInfo([In] ref MLScreensScreenInfoExNative screenInfo);

        [DllImport(MLScreensDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLScreensGetScreenInfo(ulong screen_id, ref MLScreensScreenInfoExNative out_screen_info);
    }
}

#endif
