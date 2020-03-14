// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLSecureStorage.cs" company="Magic Leap, Inc">
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
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// User interface to ML's secure storage API.
    /// </summary>
    public sealed partial class MLSecureStorage
    {
        /// <summary>
        /// MLSecureStorage library name.
        /// </summary>
        private const string DllNotFoundError = "MLSecureStorage API is currently available only on device.";

        /// <summary>
        /// Stores the specified data under the specified key. An existing key would be overwritten.
        /// </summary>
        /// <param name="dataKey">The key string associated with the data.</param>
        /// <param name="data">The data byte array to store.</param>
        /// <returns>
        /// MLResult.Result will be<c> MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.SecureStorageIOFailure</c> if an I/O failure occurred.
        /// </returns>
        public static MLResult StoreData(string dataKey, byte[] data)
        {
            try
            {
                // Early exit if array is invalid.
                if (data == null)
                {
                    return MLResult.Create(MLResult.Code.InvalidParam, "Data parameter was null");
                }

                // Early exit if string key is invalid.
                MLResult result = CheckKey(dataKey);
                if (!result.IsOk)
                {
                    return result;
                }

                MLResult.Code resultCode = MLSecureStorageNativeBindings.MLSecureStoragePutBlob(dataKey, data, (uint)data.Length);
                result = MLResult.Create(resultCode);

                return result;
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(DllNotFoundError);
                throw;
            }
        }

        /// <summary>
        /// Generic store function for all value types.
        /// Using BinaryFormatter.Serialize() to serialize data to bytes.
        /// </summary>
        /// <typeparam name="T">The type of data that is being retrieved.</typeparam>
        /// <param name="dataKey">The key string associated with the data.</param>
        /// <param name="value">The generic type value to store.</param>
        /// <returns>
        /// MLResult.Result will be<c> MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.SecureStorageIOFailure</c> if an I/O failure occurred.
        /// </returns>
        public static MLResult StoreData<T>(string dataKey, T value) where T : struct
        {
            // TODO: use non-template version of StoreData after converting value to byte array
            try
            {
                MLResult result = CheckKey(dataKey);
                if (!result.IsOk)
                {
                    return result;
                }

                byte[] valueByteArray = SerializeData(value);
                if (valueByteArray == null)
                {
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "Data serialization failed");
                }

                MLResult.Code resultCode = MLSecureStorageNativeBindings.MLSecureStoragePutBlob(dataKey, valueByteArray, (uint)valueByteArray.Length);
                result = MLResult.Create(resultCode);

                return result;
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(DllNotFoundError);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the data associated with the specified key.
        /// </summary>
        /// <param name="dataKey">The key for the data that is being requested.</param>
        /// <param name="data">A valid array of bytes to store retrieved data.</param>
        /// <returns>
        /// MLResult.Result will be<c> MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.SecureStorageBlobNotFound</c> if the dataKey was not found.
        /// MLResult.Result will be <c>MLResult.Code.SecureStorageIOFailure</c> if an I/O failure occurred.
        /// </returns>
        public static MLResult GetData(string dataKey, ref byte[] data)
        {
            try
            {
                MLResult result = CheckKey(dataKey);
                if (!result.IsOk)
                {
                    return result;
                }

                IntPtr outputBytes = IntPtr.Zero;
                ulong dataLength = 0;

                MLResult.Code resultCode = MLSecureStorageNativeBindings.MLSecureStorageGetBlob(dataKey, ref outputBytes, ref dataLength);
                result = MLResult.Create(resultCode);

                // Are there situations where the result is Ok but no data is available?
                if (result.IsOk && dataLength > 0 && outputBytes != IntPtr.Zero)
                {
                    data = new byte[dataLength];
                    Marshal.Copy(outputBytes, data, 0, (int)dataLength);

                    MLSecureStorageNativeBindings.MLSecureStorageFreeBlobBuffer(outputBytes);
                }

                return result;
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(DllNotFoundError);
                throw;
            }
        }

        /// <summary>
        /// Generic get function for all value types.
        /// Using BinaryFormatter.Deserialize() to deserialize bytes to specified value type.
        /// </summary>
        /// <typeparam name="T">The type of data that is being retrieved.</typeparam>
        /// <param name="dataKey">The key for the data that is being requested.</param>
        /// <param name="value">The value of the data that is being requested.</param>
        /// <returns>
        /// MLResult.Result will be<c> MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.SecureStorageBlobNotFound</c> if the dataKey was not found.
        /// MLResult.Result will be <c>MLResult.Code.SecureStorageIOFailure</c> if an I/O failure occurred.
        /// </returns>
        public static MLResult GetData<T>(string dataKey, ref T value) where T : struct
        {
            byte[] valueByteArray = null;
            MLResult result = GetData(dataKey, ref valueByteArray);
            if (!result.IsOk)
            {
                return result;
            }

            // Make sure we have read enough data to contain the type T.
            // Because T is restricted to ValueTypes (where T: struct), Marshal.SizeOf is sufficient.
            if (valueByteArray.Length < Marshal.SizeOf(value))
            {
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "Failed to deserialize value because of insufficient data");
            }

            object deserializedData = DeserializeData(valueByteArray);
            if (deserializedData == null)
            {
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "Failed to deserialize value");
            }

            value = (T)deserializedData;
            return result;
        }

        /// <summary>
        /// Deletes the item associated with the specified key.
        /// </summary>
        /// <param name="dataKey">The key string of the item to delete.</param>
        /// <returns>
        /// MLResult.Result will be<c> MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.SecureStorageIOFailure</c> if an I/O failure occurred.
        /// </returns>
        public static MLResult DeleteData(string dataKey)
        {
            try
            {
                MLResult result = CheckKey(dataKey);
                if (!result.IsOk)
                {
                    return result;
                }

                return MLResult.Create(MLSecureStorageNativeBindings.MLSecureStorageDeleteBlob(dataKey));
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(DllNotFoundError);
                throw;
            }
        }

        /// <summary>
        /// Gets a readable version of the result code as an ASCII string.
        /// </summary>
        /// <param name="result">The MLResult that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        public static string GetResultString(MLResult.Code result)
        {
            return Marshal.PtrToStringAnsi(MLSecureStorageNativeBindings.MLSecureStorageGetResultString(result));
        }

        /// <summary>
        /// Utility function that searches for unicode characters in a string.
        /// </summary>
        /// <param name="dataKey">The string to test.</param>
        /// <returns>
        /// MLResult.Result will be<c> MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// </returns>
        private static MLResult CheckKey(string dataKey)
        {
            if (string.IsNullOrEmpty(dataKey))
            {
                return MLResult.Create(MLResult.Code.InvalidParam, "Data key parameter was null or empty string");
            }

            const int MaxAnsiCode = 255;
            if (dataKey.Any(i => i > MaxAnsiCode))
            {
                return MLResult.Create(MLResult.Code.InvalidParam, "Data key parameter contains non Ansi characters");
            }

            return MLResult.Create(MLResult.Code.Ok);
        }

        /// <summary>
        /// Method to serialize arbitrary objects into a byte array.
        /// </summary>
        /// <param name="obj">Base System.Object of data to serialize.</param>
        /// <returns>A byte array of serialized data.</returns>
        private static byte[] SerializeData(object obj)
        {
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (var memoryStream = new MemoryStream())
                {
                    binaryFormatter.Serialize(memoryStream, obj);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception e)
            {
                MLPluginLog.ErrorFormat("MLSecureStorage.SerializeData failed. Reason: {0}", e.Message);
                return null;
            }
        }

        /// <summary>
        /// Method to deserialize arbitrary objects from a byte array.
        /// </summary>
        /// <param name="data">A byte array of serialized data.</param>
        /// <returns>Base System.Object of deserialized data.</returns>
        private static System.Object DeserializeData(byte[] data)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    memoryStream.Write(data, 0, data.Length);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return binaryFormatter.Deserialize(memoryStream);
                }
            }
            catch (Exception e)
            {
                MLPluginLog.ErrorFormat("MLSecureStorage.DeserializeData failed. Reason: {0}", e.Message);
                return null;
            }
        }
    }
}

#endif
