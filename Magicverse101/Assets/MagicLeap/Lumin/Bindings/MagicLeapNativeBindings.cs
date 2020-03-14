// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MagicLeapNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap.Native
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Defines C# API interface to C-API layer.
    /// </summary>
    public partial class MagicLeapNativeBindings
    {
        /// <summary>
        /// Perception library name string.
        /// </summary>
        public const string MLPerceptionClientDll = "ml_perception_client";

        /// <summary>
        /// Platform level library name string.
        /// </summary>
        public const string MLPlatformDll = "ml_platform";

        /// <summary>
        /// The 64 bit id for an invalid native handle.
        /// </summary>
        public const ulong InvalidHandle = 0xFFFFFFFFFFFFFFFF;

        /// <summary>
        /// Initializes a new instance of the <see cref="MagicLeapNativeBindings" /> class.
        /// </summary>
        protected MagicLeapNativeBindings()
        {
        }

        /// <summary>
        /// The current state of a given tracker.
        /// </summary>
        public enum MLSensoryState
        {
            /// <summary>
            /// The tracker is not ready, don't use the data.
            /// </summary>
            Initializing,

            /// <summary>
            /// The tracker's data can be used.
            /// </summary>
            Ready
        }

        /// <summary>
        /// Checks if 64 bit handle is valid.
        /// </summary>
        /// <returns><c>true</c>, if handle is valid, <c>false</c> if invalid.</returns>
        /// <param name="handle">The handle to check.</param>
        public static bool MLHandleIsValid(ulong handle)
        {
            return handle != InvalidHandle;
        }

        /// <summary>
        /// Returns an ASCII string for MLResultGlobal codes.
        /// </summary>
        /// <param name="result">The input MLResult enum from ML API methods.</param>
        /// <returns>An ASCII string containing readable version of result code.</returns>
        public static string MLGetResultString(MLResult.Code result)
        {
            switch (result)
            {
                case MLResult.Code.Ok:
                    {
                        return "MLResult_Ok";
                    }

                case MLResult.Code.Pending:
                    {
                        return "MLResult_Pending";
                    }

                case MLResult.Code.Timeout:
                    {
                        return "MLResult_Timeout";
                    }

                case MLResult.Code.Locked:
                    {
                        return "MLResult_Locked";
                    }

                case MLResult.Code.UnspecifiedFailure:
                    {
                        return "MLResult_UnspecifiedFailure";
                    }

                case MLResult.Code.InvalidParam:
                    {
                        return "MLResult_InvalidParam";
                    }

                case MLResult.Code.AllocFailed:
                    {
                        return "MLResult_AllocFailed";
                    }

                case MLResult.Code.PrivilegeDenied:
                    {
                        return "MLResult_PrivilegeDenied";
                    }

                case MLResult.Code.NotImplemented:
                    {
                        return "MLResult_NotImplemented";
                    }

                case MLResult.Code.SnapshotPoseNotFound:
                    {
                        return "MLResult_SnapshotPoseNotFound";
                    }

                default:
                    {
                        return "MLResult_Unknown";
                    }
            }
        }

        /// <summary>
        /// Pull in the latest state of all persistent transforms and all
        /// enabled trackers extrapolated to the next frame time.
        /// Returns an MLSnapshot with this latest state. This snap should be
        /// used for the duration of the frame being constructed and then
        /// released with a call to MLPerceptionReleaseSnapshot().
        /// </summary>
        /// <param name="snapshot">Pointer to a pointer containing an MLSnapshot on success.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if operation was successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLPerceptionGetSnapshot(ref IntPtr snapshot);

        /// <summary>
        /// Pull in the latest state of all persistent transforms and all
        /// enabled trackers extrapolated to the next frame time.
        /// Return an MLSnapshot with this latest state. This snap should be
        /// used for the duration of the frame being constructed and then
        /// released with a call to MLPerceptionReleaseSnapshot().
        /// </summary>
        /// <param name="snap">Pointer to a pointer containing an MLSnapshot on success.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if a Snapshot was created successfully successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if a Snapshot was not created successfully.
        /// </returns>
        [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLPerceptionReleaseSnapshot(IntPtr snap);

        /// <summary>
        /// Gets the transform between world origin and the coordinate frame `id'.
        /// </summary>
        /// <param name="snap">A snapshot of the tracker state. Can be obtained with MLPerceptionGetSnapshot().</param>
        /// <param name="id">Look up the transform between the current origin and this coordinate frame id.</param>
        /// <param name="outTransform">Valid pointer to an MLTransform. To be filled out with requested transform data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the transform was obtained successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid parameter.
        /// MLResult.Result will be <c>MLResult.Code.PoseNotFound</c> if the coordinate frame is valid, but not found in the current pose snapshot.
        /// </returns>
        [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLSnapshotGetTransform(IntPtr snap, ref MLCoordinateFrameUID id, ref MLTransform outTransform);

        /// <summary>
        /// Returns a pointer to an ASCII string representation for each result code.
        /// This call can return a pointer to the string for any of the MLSnapshot related MLResult codes.
        /// Developers should use MLResult.CodeToString(MLResult.Code).
        /// </summary>
        /// <param name="result">MLResult type to be converted to string.</param>
        /// <returns>Returns a pointer to an ASCII string containing readable version of the result code.</returns>
        [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MLSnapshotGetResultString(MLResult.Code result);

        /// <summary>
        /// Query the OS for which Platform API Level is supported.
        /// </summary>
        /// <param name="level">Pointer to an integer that will store the API level.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if operation was successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if level was not valid (null).
        /// </returns>
        [DllImport(MLPlatformDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLPlatformGetAPILevel(ref uint level);

        /// <summary>
        /// Tries to get the pose for the given coordinate frame id.
        /// </summary>
        /// <param name="id">The coordinate frame id to get the pose of.</param>
        /// <param name="pose">The object to initialize the found pose with.</param>
        /// <returns>True if a pose was successfully found.</returns>
        [DllImport("UnityMagicLeap", EntryPoint = "UnityMagicLeap_TryGetPose")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UnityMagicLeap_TryGetPose(MLCoordinateFrameUID id, out UnityEngine.Pose pose);

        /// <summary>
        /// 2D vector represented with X and Y floats.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLVec2f
        {
            /// <summary>
            /// X coordinate.
            /// </summary>
            public float X;

            /// <summary>
            /// Y coordinate.
            /// </summary>
            public float Y;
        }

        /// <summary>
        /// 3D vector in native format.
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct MLVec3f
        {
            /// <summary>
            /// X coordinate.
            /// </summary>
            public float X;

            /// <summary>
            /// Y coordinate.
            /// </summary>
            public float Y;

            /// <summary>
            /// Z coordinate.
            /// </summary>
            public float Z;
        }

        /// <summary>
        /// Quaternion in native format.
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct MLQuaternionf
        {
            /// <summary>
            /// X coordinate.
            /// </summary>
            public float X;

            /// <summary>
            /// Y coordinate.
            /// </summary>
            public float Y;

            /// <summary>
            /// Z coordinate.
            /// </summary>
            public float Z;

            /// <summary>
            /// W coordinate.
            /// </summary>
            public float W;

            /// <summary>
            /// Returns an initialized <c>MLQuaternionf</c> with default values.
            /// </summary>
            /// <returns>An initialized <c>MLQuaternionf</c>.</returns>
            public static MLQuaternionf Identity()
            {
                MLQuaternionf quat = new MLQuaternionf()
                {
                    X = 0,
                    Y = 0,
                    Z = 0,
                    W = 1
                };

                return quat;
            }
        }

        /// <summary>
        /// Information used to transform from one coordinate frame to another.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLTransform
        {
            /// <summary>
            /// The rotation of the coordinate frame to apply after the translation.
            /// </summary>
            public MLQuaternionf Rotation;

            /// <summary>
            /// The translation to apply to get the coordinate frame in the proper location.
            /// </summary>
            public MLVec3f Position;

            /// <summary>
            /// Returns an initialized MLTransform with default values.
            /// </summary>
            /// <returns>An initialized MLTransform.</returns>
            public static MLTransform Identity()
            {
                MLTransform t = new MLTransform();
                t.Rotation = MLQuaternionf.Identity();
                return t;
            }
        }

        /// <summary>
        /// 4x4 matrix in native format.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLMat4f
        {
            /// <summary>
            /// The 16 matrix values.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public float[] MatrixColmajor;
        }

        /// <summary>
        /// 2D rectangle in native format.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLRectf
        {
            /// <summary>
            /// The x coordinate.
            /// </summary>
            public float X;

            /// <summary>
            /// The y coordinate.
            /// </summary>
            public float Y;

            /// <summary>
            /// The width.
            /// </summary>
            public float W;

            /// <summary>
            /// The height.
            /// </summary>
            public float H;
        }

        /// <summary>
        /// 2D rectangle with integer values in native format.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLRecti
        {
            /// <summary
            /// >The x coordinate.
            /// </summary>
            public int X;

            /// <summary>
            /// The y coordinate.
            /// </summary>
            public int Y;

            /// <summary>
            /// The width.
            /// </summary>
            public int W;

            /// <summary>
            /// The height.
            /// </summary>
            public int H;
        }

        /// <summary>
        /// Universally unique identifier
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLUUID
        {
            /// <summary>
            /// The TimeLow field.
            /// </summary>
            public uint TimeLow;

            /// <summary>
            /// The TimeMid field.
            /// </summary>
            public ushort TimeMid;

            /// <summary>
            /// The TimeHiAndVersion field.
            /// </summary>
            public ushort TimeHiAndVersion;

            /// <summary>
            /// The <c>ClockSeqHiAndReserved</c> field.
            /// </summary>
            public byte ClockSeqHiAndReserved;

            /// <summary>
            /// The <c>ClockSeqLow</c> field.
            /// </summary>
            public byte ClockSeqLow;

            /// <summary>
            /// The Node0 field.
            /// </summary>
            public byte Node0;

            /// <summary>
            /// The Node1 field.
            /// </summary>
            public byte Node1;

            /// <summary>
            /// The Node2 field.
            /// </summary>
            public byte Node2;

            /// <summary>
            /// The Node3 field.
            /// </summary>
            public byte Node3;

            /// <summary>
            /// The Node4 field.
            /// </summary>
            public byte Node4;

            /// <summary>
            /// The Node5 field.
            /// </summary>
            public byte Node5;
        }

        /// <summary>
        /// Universally unique identifier, byte array.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLUUIDBytes
        {
            /// <summary>
            /// The 16 byte data array.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Data;
        }

        /// <summary>
        /// A unique identifier which represents a coordinate frame.
        /// The unique identifier is comprised of two values.
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct MLCoordinateFrameUID
        {
            /// <summary>
            /// The first data value.
            /// </summary>
            public ulong First;

            /// <summary>
            /// The second data value.
            /// </summary>
            public ulong Second;

            /// <summary>
            /// Gets an initialized MLCoordinateFrameUID.
            /// </summary>
            /// <returns>An initialized MLCoordinateFrameUID.</returns>
            public static MLCoordinateFrameUID EmptyFrame
            {
                get
                {
                    return new MLCoordinateFrameUID();
                }
            }

            /// <summary>
            /// The equality check to be used for comparing two MLCoordinateFrameUID structs.
            /// </summary>
            /// <param name="one">The first struct to compare with the second struct. </param>
            /// <param name="two">The second struct to compare with the first struct. </param>
            /// <returns>True if the two provided structs have the same two data values.</returns>
            public static bool operator ==(MLCoordinateFrameUID one, MLCoordinateFrameUID two)
            {
                return one.First == two.First && one.Second == two.Second;
            }

            /// <summary>
            /// The inequality check to be used for comparing two MLCoordinateFrameUID structs.
            /// </summary>
            /// <param name="one">The first struct to compare with the second struct. </param>
            /// <param name="two">The second struct to compare with the first struct. </param>
            /// <returns>True if the two provided structs do not have the same two data values.</returns>
            public static bool operator !=(MLCoordinateFrameUID one, MLCoordinateFrameUID two)
            {
                return !(one == two);
            }

            /// <summary>
            /// The equality check to be used for when being compared to an object.
            /// </summary>
            /// <param name="obj">The object to compare to this one with.</param>
            /// <returns>True if the the provided object is of the MLCoordinateFrameUID type and has the same two data values.</returns>
            public override bool Equals(object obj)
            {
                if (obj is MLCoordinateFrameUID)
                {
                    var rhs = (MLCoordinateFrameUID)obj;
                    return this == rhs;
                }

                return false;
            }

            /// <summary>
            /// Gets the hash code to use from the first data value.
            /// </summary>
            /// <returns>The hash code returned by the first data value of this object </returns>
            public override int GetHashCode()
            {
                return this.First.GetHashCode();
            }

            /// <summary>
            /// Returns the string value of the GUID of this MLCoordinateFrameUID.
            /// </summary>
            /// <returns>The string value of the GUID.</returns>
            public override string ToString()
            {
                return this.ToGuid().ToString();
            }

            /// <summary>
            /// Returns the GUID based on the values of this MLCoordinateFrameUID.
            /// </summary>
            /// <returns>The calculated GUID.</returns>
            public Guid ToGuid()
            {
                byte[] toConvert = BitConverter.GetBytes(this.First);
                byte[] newSecond = BitConverter.GetBytes(this.Second);
                FlipGuidComponents(toConvert);
                ulong newFirst = BitConverter.ToUInt64(toConvert, 0);

                return new Guid((int)(newFirst >> 32 & 0x00000000FFFFFFFF), (short)(newFirst >> 16 & 0x000000000000FFFF), (short)(newFirst & 0x000000000000FFFF), newSecond);
            }

            /// <summary>
            /// Flips a component of the GUID based on <c>endianness</c>.
            /// </summary>
            /// <param name="bytes">The array of bytes to reverse.</param>
            private static void FlipGuidComponents(byte[] bytes)
            {
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }
            }
        }
    }
}
#endif
