// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLSecureStorageNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

// Disable warnings about missing documentation for native interop.
#pragma warning disable 1591

namespace UnityEngine.XR.MagicLeap.Native
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// See ml_secure_storage.h for additional comments
    /// </summary>
    public class MLSecureStorageNativeBindings : MagicLeapNativeBindings
    {
        /// <summary>
        /// MLSecureStorage library name.
        /// </summary>
        private const string MLSecureStorageDll = "ml_secure_storage";

        /// <summary>
        /// Prevents a default instance of the <see cref="MLSecureStorageNativeBindings"/> class from being created.
        /// </summary>
        private MLSecureStorageNativeBindings()
        {
        }

        /// <summary>
        /// Store the specified blob under the specified alias.
        /// An existing alias would be overwritten.
        /// </summary>
        /// <param name="alias">The NULL-terminated alias associated with the blob. Must not be NULL.</param>
        /// <param name="blob">The blob to store.</param>
        /// <param name="blobLength">length of the blob to store.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the blob was stored successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the alias is empty and/or the blob pointer is invalid.
        /// MLResult.Result will be <c>MLResult.Code.SecureStorageIOFailure</c> if an I/O failure occurred.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        [DllImport(MLSecureStorageDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLSecureStoragePutBlob([MarshalAs(UnmanagedType.LPStr)] string alias, byte[] blob, ulong blobLength);

        /// <summary>
        /// Retrieve the blob associated with the specified alias.
        /// Note that the caller must invoke MLSecureStorageFreeBlobBuffer() on the blob pointer.
        /// </summary>
        /// <param name="alias">The NULL-terminated alias associated with the blob. Must not be NULL.</param>
        /// <param name="blob">The blob to store.</param>
        /// <param name="blobLength">length of the blob to store.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the blob was stored successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the alias is empty and/or the blob pointer is invalid.
        /// MLResult.Result will be <c>MLResult.Code.SecureStorageBlobNotFound</c> if the alias was not found.
        /// MLResult.Result will be <c>MLResult.Code.SecureStorageIOFailure</c> if an I/O failure occurred.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        [DllImport(MLSecureStorageDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLSecureStorageGetBlob([MarshalAs(UnmanagedType.LPStr)] string alias, ref IntPtr blob, ref ulong blobLength);

        /// <summary>
        /// Delete the item associated with the specified alias.
        /// </summary>
        /// <param name="alias">The NULL-terminated alias associated with the blob. Must not be NULL.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the blob was stored successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the alias is empty and/or the blob pointer is invalid.
        /// MLResult.Result will be <c>MLResult.Code.SecureStorageIOFailure</c> if an I/O failure occurred.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        [DllImport(MLSecureStorageDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLSecureStorageDeleteBlob([MarshalAs(UnmanagedType.LPStr)] string alias);

        /// <summary>
        /// Free the buffer allocated internally when MLSecureStorageGetBlob() is called.
        /// </summary>
        /// <param name="blob">Pointer to the internally allocated buffer.</param>
        [DllImport(MLSecureStorageDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MLSecureStorageFreeBlobBuffer(IntPtr blob);

        /// <summary>
        /// Gets the result string for a MLResult.Code.
        /// </summary>
        /// <param name="result">The MLResult.Code to be requested.</param>
        /// <returns>A pointer to the result string.</returns>
        [DllImport(MLSecureStorageDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MLSecureStorageGetResultString(MLResult.Code result);
    }
}

#endif
