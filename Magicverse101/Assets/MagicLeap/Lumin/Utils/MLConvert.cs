// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLConvert.cs" company="Magic Leap, Inc">
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
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using UnityEngine;
    using UnityEngine.XR.MagicLeap;

    /// <summary>
    /// Utility class used for converting vectors and matrices between native and unity format.
    /// </summary>
    public static class MLConvert
    {
        /// <summary>
        /// Gets a float value converted from meters to Unity units.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>value converted to Unity units</returns>
        public static float ToUnity(float value)
        {
            return value * MLDevice.WorldScale;
        }

        /// <summary>
        /// Creates a Unity 3D vector from a native vector.
        /// </summary>
        /// <param name="vec">A native vector.</param>
        /// <param name="transformToRUF">(Optional) If false, prevents conversion to Unity's coordinate system.</param>
        /// <param name="applyScale">(Optional) If false, prevents scaling to Unity's unit per meter scale.</param>
        /// <returns>A Unity vector.</returns>
        public static Vector3 ToUnity(MagicLeapNativeBindings.MLVec3f vec, bool transformToRUF = true, bool applyScale = true)
        {
            return ToUnity(vec.X, vec.Y, vec.Z, transformToRUF, applyScale);
        }

        /// <summary>
        /// Creates a Unity 3D vector from a x, y and z parameters.
        /// </summary>
        /// <param name="x">X component</param>
        /// <param name="y">Y component</param>
        /// <param name="z">Z component</param>
        /// <param name="transformToRUF">(Optional) If false, prevents conversion to Unity's coordinate system.</param>
        /// <param name="applyScale">(Optional) If false, prevents scaling to Unity's unit per meter scale.</param>
        /// <returns>A Unity vector.</returns>
        public static Vector3 ToUnity(float x, float y, float z, bool transformToRUF = true, bool applyScale = true)
        {
            Vector3 unityVec = new Vector3();
            unityVec.x = x;
            unityVec.y = y;
            unityVec.z = z;

            if (transformToRUF)
            {
                unityVec.z = -unityVec.z;
            }

            if (applyScale)
            {
                unityVec = unityVec * MLDevice.WorldScale;
            }

            return unityVec;
        }

        /// <summary>
        /// Creates a Unity quaternion from a native vector.
        /// </summary>
        /// <param name="quat">A native quaternion.</param>
        /// <param name="transformToRUF">(Optional) If false, prevents conversion to Unity's coordinate system.</param>
        /// <returns>A Unity quaternion.</returns>
        public static Quaternion ToUnity(MagicLeapNativeBindings.MLQuaternionf quat, bool transformToRUF = true)
        {
            Quaternion unityQuat = new Quaternion(quat.X, quat.Y, quat.Z, quat.W);

            if (transformToRUF)
            {
                unityQuat.z = -unityQuat.z;
                unityQuat.w = -unityQuat.w;
            }

            return unityQuat;
        }

        /// <summary>
        /// Creates Unity 4x4 matrix from native matrix.
        /// </summary>
        /// <param name="mat">A native matrix.</param>
        /// <returns>A Unity matrix.</returns>
        public static Matrix4x4 ToUnity(MagicLeapNativeBindings.MLMat4f mat)
        {
            return FloatsToMat(mat.MatrixColmajor);
        }

        /// <summary>
        /// Creates Unity 4x4 matrix from native transform.
        /// </summary>
        /// <param name="transform">A native transform.</param>
        /// <param name="transformToRUF">(Optional) If false, prevents conversion to Unity's coordinate system.</param>
        /// <param name="applyScale">(Optional) If false, prevents scaling to Unity's unit per meter scale.</param>
        /// <returns>A Unity matrix.</returns>
        public static Matrix4x4 ToUnity(MagicLeapNativeBindings.MLTransform transform, bool transformToRUF = true, bool applyScale = true)
        {
            Vector3 position = ToUnity(transform.Position, transformToRUF, applyScale);
            Quaternion rotation = ToUnity(transform.Rotation, transformToRUF);

            return Matrix4x4.TRS(position, rotation, Vector3.one);
        }

        /// <summary>
        /// Creates a System.Guid from an MLUUID
        /// </summary>
        /// <param name="uuid">A native UUID</param>
        /// <returns>A System.Guid</returns>
        public static Guid ToUnity(MagicLeapNativeBindings.MLUUID uuid)
        {
            return new Guid(uuid.TimeLow, uuid.TimeMid, uuid.TimeHiAndVersion, uuid.ClockSeqHiAndReserved, uuid.ClockSeqLow, uuid.Node0, uuid.Node1, uuid.Node2, uuid.Node3, uuid.Node4, uuid.Node5);
        }

        /// <summary>
        /// Creates native transform from a Unity matrix.
        /// </summary>
        /// <param name="mat">A Unity matrix.</param>
        /// <param name="transformFromRUF">(Optional) If false, prevents conversion to the native SDK coordinate system.</param>
        /// <param name="applyScale">(Optional) If false, prevents scaling to the native SDK's unit per meter scale.</param>
        /// <returns>A native transform.</returns>
        public static MagicLeapNativeBindings.MLTransform FromUnity(Matrix4x4 mat, bool transformFromRUF = true, bool applyScale = true)
        {
            MagicLeapNativeBindings.MLTransform transform = new MagicLeapNativeBindings.MLTransform();

            transform.Position = FromUnity(GetPositionFromTransformMatrix(mat), transformFromRUF, applyScale);
            transform.Rotation = FromUnity(GetRotationFromTransformMatrix(mat), transformFromRUF);

            return transform;
        }

        /// <summary>
        /// Fills out array with values from 4x4 Unity matrix.
        /// </summary>
        /// <param name="mat">An input native matrix.</param>
        /// <param name="matrixColMajor">An array to populate in Unity format.</param>
        public static void FromUnity(Matrix4x4 mat, ref float[] matrixColMajor)
        {
            for (int i = 0; i < 16; ++i)
            {
                matrixColMajor[i] = mat[i];
            }
        }

        /// <summary>
        /// Creates native 3d vector from a Unity vector.
        /// </summary>
        /// <param name="vec">A Unity vector.</param>
        /// <param name="transformFromRUF">(Optional) If false, prevents conversion to the native SDK coordinate system.</param>
        /// <param name="applyScale">(Optional) If false, prevents scaling to the native SDK's unit per meter scale.</param>
        /// <returns>A native vector.</returns>
        public static MagicLeapNativeBindings.MLVec3f FromUnity(Vector3 vec, bool transformFromRUF = true, bool applyScale = true)
        {
            if (transformFromRUF)
            {
                vec.z = -vec.z;
            }

            if (applyScale)
            {
                if (MLDevice.WorldScale == 0.0f)
                {
                    MLPluginLog.Error("Divide by zero, unit scale vector contains 0");
                }
                else
                {
                    vec = vec / MLDevice.WorldScale;
                }
            }

            MagicLeapNativeBindings.MLVec3f outVec = new MagicLeapNativeBindings.MLVec3f();
            outVec.X = vec.x;
            outVec.Y = vec.y;
            outVec.Z = vec.z;

            return outVec;
        }

        /// <summary>
        /// Creates native quaternion from a Unity quaternion.
        /// </summary>
        /// <param name="quat">A Unity quaternion.</param>
        /// <param name="transformFromRUF">(Optional) If false, prevents conversion to the native SDK coordinate system.</param>
        /// <returns>A native quaternion.</returns>
        public static MagicLeapNativeBindings.MLQuaternionf FromUnity(Quaternion quat, bool transformFromRUF = true)
        {
            if (transformFromRUF)
            {
                quat.z = -quat.z;
                quat.w = -quat.w;
            }

            MagicLeapNativeBindings.MLQuaternionf outQuat = new MagicLeapNativeBindings.MLQuaternionf();

            outQuat.X = quat.x;
            outQuat.Y = quat.y;
            outQuat.Z = quat.z;
            outQuat.W = quat.w;

            return outQuat;
        }

        /// <summary>
        /// Gets a float value converted from Unity units to meters.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Returns the Unity value to meters</returns>
        public static float FromUnity(float value)
        {
            float scale = MLDevice.WorldScale;

            if (scale == 0.0f)
            {
                scale = 1.0f;
            }

            return value / scale;
        }

        /// <summary>
        /// Creates an MLUUID from a System.Guid
        /// </summary>
        /// <param name="guid">A System.Guid</param>
        /// <returns>A native MLUUID</returns>
        public static MagicLeapNativeBindings.MLUUID FromUnity(Guid guid)
        {
            MagicLeapNativeBindings.MLUUID result = new MagicLeapNativeBindings.MLUUID();
            string guidString = guid.ToString("N");

            result.TimeLow = uint.Parse(guidString.Substring(0, 8), NumberStyles.HexNumber);
            result.TimeMid = ushort.Parse(guidString.Substring(8, 4), NumberStyles.HexNumber);
            result.TimeHiAndVersion = ushort.Parse(guidString.Substring(12, 4), NumberStyles.HexNumber);
            result.ClockSeqHiAndReserved = byte.Parse(guidString.Substring(16, 2), NumberStyles.HexNumber);
            result.ClockSeqLow = byte.Parse(guidString.Substring(18, 2), NumberStyles.HexNumber);
            result.Node0 = byte.Parse(guidString.Substring(20, 2), NumberStyles.HexNumber);
            result.Node1 = byte.Parse(guidString.Substring(22, 2), NumberStyles.HexNumber);
            result.Node2 = byte.Parse(guidString.Substring(24, 2), NumberStyles.HexNumber);
            result.Node3 = byte.Parse(guidString.Substring(26, 2), NumberStyles.HexNumber);
            result.Node4 = byte.Parse(guidString.Substring(28, 2), NumberStyles.HexNumber);
            result.Node5 = byte.Parse(guidString.Substring(30, 2), NumberStyles.HexNumber);

            return result;
        }

        /// <summary>
        /// Gets the position vector stored in a transform matrix.
        /// </summary>
        /// <param name="transformMatrix">A Unity matrix treated as a transform matrix.</param>
        /// <returns>A Unity vector representing a position.</returns>
        public static Vector3 GetPositionFromTransformMatrix(Matrix4x4 transformMatrix)
        {
            return transformMatrix.GetColumn(3);
        }

        /// <summary>
        /// Gets the rotation quaternion stored in a transform matrix.
        /// </summary>
        /// <param name="transformMatrix">A Unity matrix treated as a transform matrix.</param>
        /// <returns>A Unity quaternion.</returns>
        public static Quaternion GetRotationFromTransformMatrix(Matrix4x4 transformMatrix)
        {
            return Quaternion.LookRotation(transformMatrix.GetColumn(2), transformMatrix.GetColumn(1));
        }

        /// <summary>
        /// Take a string, snips it to a desired length and converts it to UTF8.
        /// </summary>
        /// <param name="inString">String to snip and convert</param>
        /// <param name="snipLength">length to snip to</param>
        /// <returns>UTF8 string byte array</returns>
        public static byte[] ToUTF8Snipped(string inString, int snipLength)
        {
            int snipSize = Math.Min(inString.Length, snipLength);
            int size = Encoding.UTF8.GetByteCount(inString.Substring(0, snipSize));

            while (snipSize >= 0 && size > snipLength)
            {
                size -= Encoding.UTF8.GetByteCount(inString.Substring(snipSize - 1, 1));
                --snipSize;
            }

            return Encoding.UTF8.GetBytes(inString.Substring(0, snipSize));
        }

        /// <summary>
        /// Decodes a buffer of bytes into an ASCII string.
        /// </summary>
        /// <param name="buffer">bytes to convert to a string</param>
        /// <returns>A managed string</returns>
        public static string DecodeAscii(byte[] buffer)
        {
            int count = Array.IndexOf<byte>(buffer, 0, 0);

            if (count < 0)
            {
                count = buffer.Length;
            }

            return Encoding.ASCII.GetString(buffer, 0, count);
        }

        /// <summary>
        /// Decodes a buffer of bytes into a UTF8 string.
        /// </summary>
        /// <param name="buffer">bytes to convert to a UTF8 string</param>
        /// <returns>A managed string</returns>
        public static string DecodeUTF8(byte[] buffer)
        {
            int count = Array.IndexOf<byte>(buffer, 0, 0);

            if (count < 0)
            {
                count = buffer.Length;
            }

            return Encoding.UTF8.GetString(buffer, 0, count);
        }

        /// <summary>
        /// Converts a managed string into an unmanaged null terminated UTF-8 string.
        /// </summary>
        /// <param name="s">The managed string to convert</param>
        /// <returns>A pointer to the unmanaged string</returns>
        public static IntPtr EncodeToUnmanagedUTF8(string s)
        {
            int length = Encoding.UTF8.GetByteCount(s);
            byte[] buffer = new byte[length + 1];

            Encoding.UTF8.GetBytes(s, 0, s.Length, buffer, 0);

            IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);

            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);

            return nativeUtf8;
        }

        /// <summary>
        /// This encodes the string into a UTF-8 byte array.
        /// </summary>
        /// <param name="decodedString">string to encode</param>
        /// <returns>UTF8 string byte array</returns>
        public static byte[] EncodeUTF8(string decodedString)
        {
            return Encoding.UTF8.GetBytes(decodedString);
        }

        /// <summary>
        /// Converts an unmanaged null terminated UTF-8 string into a managed string.
        /// </summary>
        /// <param name="nativeString">The unmanaged string to convert</param>
        /// <param name="maximumSize">maximum number of characters to convert</param>
        /// <returns>A managed string</returns>
        public static string DecodeUTF8(IntPtr nativeString, int maximumSize = -1)
        {
            if (nativeString == IntPtr.Zero)
            {
                return string.Empty;
            }

            int byteLength = 0;

            if (maximumSize > 0)
            {
                while (Marshal.ReadByte(nativeString, byteLength) != 0)
                {
                    ++byteLength;
                    if (byteLength == maximumSize)
                    {
                        break;
                    }
                }
            }
            else
            {
                while (Marshal.ReadByte(nativeString, byteLength) != 0)
                {
                    ++byteLength;
                }
            }

            if (byteLength == 0)
            {
                return string.Empty;
            }

            byte[] buffer = new byte[byteLength];
            Marshal.Copy(nativeString, buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Converts an unmanaged UTF-16 string into a managed string.
        /// </summary>
        /// <param name="nativeArray">Native byte array to convert</param>
        /// <returns>A managed string</returns>
        public static string DecodeUTF16BE(byte[] nativeArray)
        {
            return Encoding.BigEndianUnicode.GetString(nativeArray);
        }

        /// <summary>
        /// Converts an unmanaged UTF-16 string into a managed string.
        /// </summary>
        /// <param name="nativeArray">Native byte array to convert</param>
        /// <returns>A managed string</returns>
        public static string DecodeUTF16LE(byte[] nativeArray)
        {
            return Encoding.Unicode.GetString(nativeArray);
        }

        /// <summary>
        /// Convert an object to a byte array. Uses C# Binary formatter to serialize
        /// </summary>
        /// <typeparam name="T">Data type of object</typeparam>
        /// <param name="obj">Object to convert</param>
        /// <returns>Returns a binary array representation of the object</returns>
        public static byte[] ObjectToByteArray<T>(T obj)
        {
            BinaryFormatter bf = new BinaryFormatter();

            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Convert a byte array to an Object
        /// </summary>
        /// <param name="byteArray">Byte array to convert</param>
        /// <returns>Returns the newly converted object</returns>
        public static object ByteArrayToObject(byte[] byteArray)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(byteArray, 0, byteArray.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        /// <summary>
        /// Creates Unity 4x4 matrix from an array of 16 floats.
        /// </summary>
        /// <param name="vals">An array of 16 floats.</param>
        /// <returns>A Unity matrix.</returns>
        private static Matrix4x4 FloatsToMat(float[] vals)
        {
            Matrix4x4 mat = new Matrix4x4();

            for (int i = 0; i < 16; ++i)
            {
                mat[i] = vals[i];
            }

            return mat;
        }
    }
}

#endif
