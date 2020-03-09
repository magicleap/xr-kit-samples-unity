// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMusicService.cs" company="Magic Leap, Inc">
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
    /// MLMusicService class is the entry point for the MusicService API.
    /// </summary>
    public sealed partial class MLMusicService
    {
        /// <summary>
        /// Starts the Music Service API.
        /// </summary>
        /// <param name="musicServiceProvider">The name of the music service provider</param>
        /// <param name="data">Extra user data passed as a void pointer through the callbacks</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if connected to MusicService successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if one of the parameters is invalid.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if connection failed with resource allocation failure
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericAlreadyExists</c> if connection exists already.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        [Obsolete("Please use MLMusicService.Start(string) instead.", true)]
        public static MLResult Start(string musicServiceProvider, IntPtr data)
        {
            return MLResult.Create(MLResult.Code.UnspecifiedFailure);
        }

        /// <summary>
        /// Get the music service status.
        /// </summary>
        /// <param name="status">Status from the music service</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [Obsolete("Please use MLMusicService.GetStatus(MLMusicService.Status) instead.", true)]
        public static MLResult GetStatus(ref MLMusicServiceStatus status)
        {
            return MLResult.Create(MLResult.Code.UnspecifiedFailure);
        }

        /// <summary>
        /// Get the last music service error
        /// </summary>
        /// <param name="error">Structure to contain the error parameters</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [Obsolete("Please use MLMusicService.GetError(MLMusicService.Error) instead.", true)]
        public static MLResult GetError(ref MLMusicServiceError error)
        {
            return MLResult.Create(MLResult.Code.UnspecifiedFailure);
        }

        /// <summary>
        ///  Get the metadata for the current track.
        /// </summary>
        /// <param name="metadata">Structure to contain the meta data</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if one of the parameters is invalid.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        [Obsolete("Please use MLMusicService.GetMetadata(MLMusicService.Metadata, int) instead.", true)]
        public static MLResult GetMetadata(ref MLMusicServiceMetadata metadata)
        {
            return MLResult.Create(MLResult.Code.UnspecifiedFailure);
        }

        /// <summary>
        ///  Get the metadata for a track.
        /// </summary>
        /// <param name="relativeOffest">The relative offset of the current track that you would like metadata for.</param>
        /// <param name="metadata">Structure to contain the metadata.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if one of the parameters is invalid.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        [Obsolete("Please use MLMusicService.GetMetadata(MLMusicService.Metadata, int) instead.", true)]
        public static MLResult GetMetadata(int relativeOffest, ref MLMusicServiceMetadata metadata)
        {
            return MLResult.Create(MLResult.Code.UnspecifiedFailure);
        }
    }
}

#endif
