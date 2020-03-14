// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMusicServiceNativeBindings.cs" company="Magic Leap, Inc">
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
    /// See ml_music_service.h for additional comments.
    /// </summary>
    public partial class MLMusicServiceNativeBindings : MagicLeapNativeBindings
    {
        /// <summary>
        /// music service library name.
        /// </summary>
        private const string MLMusicServiceDLL = "ml_musicservice";

        /// <summary>
        /// This delegate is meant to handle playback state changes.
        /// </summary>
        /// <param name="state">The new state.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        public delegate void OnPlaybackStateChangeCallback(MLMusicService.PlaybackStateType state, IntPtr data);

        /// <summary>
        /// This delegate is meant to handle repeat state changes.
        /// </summary>
        /// <param name="state">The new state.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        public delegate void OnRepeatStateChangeCallback(MLMusicService.RepeatStateType state, IntPtr data);

        /// <summary>
        /// This delegate is meant to handle shuffle state changes.
        /// </summary>
        /// <param name="state">The new state.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        public delegate void OnShuffleStateChangeCallback(MLMusicService.ShuffleStateType state, IntPtr data);

        /// <summary>
        /// This delegate is meant to handle metadata changes.
        /// </summary>
        /// <param name="metadata">The new metadata.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        public delegate void OnMetadataChangeCallback(ref MetadataNative metadata, IntPtr data);

        /// <summary>
        /// This delegate is meant to handle position changes.
        /// </summary>
        /// <param name="position">The new position.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        public delegate void OnPositionChangeCallback(int position, IntPtr data);

        /// <summary>
        /// This delegate is meant to handle when the music service encounters an error.
        /// </summary>
        /// <param name="errorType">Error type.</param>
        /// <param name="errorCode">Error code.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        public delegate void OnErrorCallback(MLMusicService.ErrorType errorType, int errorCode, IntPtr data);

        /// <summary>
        /// This delegate is meant to handle when the music service status changes.
        /// </summary>
        /// <param name="status">The new status.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        public delegate void OnStatusChangeCallback(MLMusicService.Status status, IntPtr data);

        /// <summary>
        /// This delegate is meant to handle when the music service volume changes.
        /// </summary>
        /// <param name="volume">The new volume.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        public delegate void OnVolumeChangeCallback(float volume, IntPtr data);

        /// <summary>
        /// Raised when playback state has changed.
        /// </summary>
        public static event MLMusicService.OnPlaybackStateChangeDelegate OnPlaybackStateChange = delegate { };

        /// <summary>
        /// Raised when repeat state has changed.
        /// </summary>
        public static event MLMusicService.OnRepeatStateChangeDelegate OnRepeatStateChange = delegate { };

        /// <summary>
        /// Raised when shuffle state has changed.
        /// </summary>
        public static event MLMusicService.OnShuffleStateChangeDelegate OnShuffleStateChange = delegate { };

        /// <summary>
        /// Raised when metadata has has changed.
        /// </summary>
        public static event MLMusicService.OnMetadataChangeDelegate OnMetadataChange = delegate { };

        /// <summary>
        /// Raised when playback head position has changed.
        /// </summary>
        public static event MLMusicService.OnPositionChangeDelegate OnPositionChange = delegate { };

        /// <summary>
        /// Raised when an error has occurred.
        /// </summary>
        public static event MLMusicService.OnErrorDelegate OnError = delegate { };

        /// <summary>
        /// Raised when the Music Service status has changed.
        /// </summary>
        public static event MLMusicService.OnStatusChangeDelegate OnStatusChange = delegate { };

        /// <summary>
        /// Raised when the Music Service volume has changed.
        /// </summary>
        public static event MLMusicService.OnVolumeChangeDelegate OnVolumeChange = delegate { };

        /// <summary>
        /// Gets the static pointer to the CallbacksNative instance storing callbacks the API is set to.
        /// </summary>
        public static IntPtr SystemCallbacks { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// Connects to MusicService.
        /// </summary>
        /// <param name="name">Name of the music service provider (defined as the visible_name in the manifest).</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if connected to MusicService successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if one of the parameters is invalid.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if connection failed with resource allocation failure
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericAlreadyExists</c> if connection exists already.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceConnect(string name);

        /// <summary>
        /// Disconnects from MusicService.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceDisconnect();

        /// <summary>
        /// Sets the client-implemented callbacks to receive information from MusicService.
        /// Client needs to implement the callbacks defined by MLMusicServiceCallbacks.
        /// The library passes the MusicService info to the client via those callbacks.
        /// </summary>
        /// <param name="callbacks">Client-implemented callbacks.</param>
        /// <param name="data">User metadata.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceSetCallbacks(IntPtr callbacks, IntPtr data);

        /// <summary>
        /// Sets the authentication string on the MusicService.
        /// </summary>
        /// <param name="authPtr">The authentication string.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceSetAuthString(IntPtr authPtr);

        /// <summary>
        /// Sets a specified URL.
        /// </summary>
        /// <param name="url">The URL to play.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceSetURL(IntPtr url);

        /// <summary>
        /// Sets the the playlist on the MusicService.
        /// </summary>
        /// <param name="playlist">List of URLs to play.</param>
        /// <param name="size">Number of URLs in the list.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceSetPlayList(IntPtr[] playlist, ulong size);

        /// <summary>
        /// Starts playing the current track.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceStart();

        /// <summary>
        /// Stop playing the current track.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceStop();

        /// <summary>
        /// Pause the current track.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServicePause();

        /// <summary>
        /// Resume playback of the current track.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceResume();

        /// <summary>
        /// Seek to a specified position within the current track.
        /// </summary>
        /// <param name="position">The position in milliseconds to seek.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceSeek(uint position);

        /// <summary>
        /// Advances to the next track in the playlist.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceNext();

        /// <summary>
        /// Moves to the previous track in the playlist.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServicePrevious();

        /// <summary>
        /// Sets the shuffle mode.
        /// </summary>
        /// <param name="mode">The state to set shuffle to.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceSetShuffle(MLMusicService.ShuffleStateType mode);

        /// <summary>
        /// Sets the repeat mode.
        /// </summary>
        /// <param name="mode">State to set repeat mode to.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceSetRepeat(MLMusicService.RepeatStateType mode);

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="volume">Value to set the volume to.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceSetVolume(float volume);

        /// <summary>
        /// Gets the current track's length in seconds.
        /// </summary>
        /// <param name="outLength">The current track's length in seconds.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceGetTrackLength(out uint outLength);

        /// <summary>
        /// Obtains the position of the current track.
        /// </summary>
        /// <param name="outPosition">Position of the current track.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceGetCurrentPosition(out uint outPosition);

        /// <summary>
        /// Get the music service status.
        /// </summary>
        /// <param name="outStatus">Status from the music service.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceGetStatus(out MLMusicService.Status outStatus);

        /// <summary>
        /// Obtains the error from MusicService.
        /// </summary>
        /// <param name="outErrorType">Error type.</param>
        /// <param name="outErrorCode">Error code.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceGetError(out MLMusicService.ErrorType outErrorType, out int outErrorCode);

        /// <summary>
        /// Polls the state of playback from MusicService.
        /// </summary>
        /// <param name="outState">The state of playback.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceGetPlaybackState(out MLMusicService.PlaybackStateType outState);

        /// <summary>
        /// Polls the state of repeat setting from music service.
        /// </summary>
        /// <param name="outState">The repeat setting state.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceGetRepeatState(out MLMusicService.RepeatStateType outState);

        /// <summary>
        /// Polls the state of shuffle setting from music service.
        /// </summary>
        /// <param name="outState">The current shuffle state.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceGetShuffleState(out MLMusicService.ShuffleStateType outState);

        /// <summary>
        /// Polls the music volume from music service provider.
        /// </summary>
        /// <param name="outVolume">The current volume.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceGetVolume(out float outVolume);

        /// <summary>
        /// Get Metadata for the track at relative position to the current track.
        /// The library maintains the lifetime of the data.Call MLMusicServiceReleaseMetadata()
        /// to release them.
        /// </summary>
        /// <param name="track_index">Relative index of the track for which to retrieve metadata.</param>
        /// <param name="out_metadata">Metadata to be returned.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceGetMetadataForIndex(int track_index, ref MetadataNative out_metadata);

        /// <summary>
        /// Releases the metadata.
        /// </summary>
        /// <param name="metadata">Metadata obtained by MLMusicServiceGetMetadataForIndex().</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        [DllImport(MLMusicServiceDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLMusicServiceReleaseMetadata(ref MetadataNative metadata);

        /// <summary>
        /// Converts authentication string to UTF8 and sets it for the MusicService.
        /// </summary>
        /// <param name="authString">Service provider specific authentication string.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not music service is not connected.
        /// </returns>
        public static MLResult.Code SetAuthStringHelper(string authString)
        {
            try
            {
                IntPtr authPtr = MLConvert.EncodeToUnmanagedUTF8(authString);
                MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceSetAuthString(authPtr);
                Marshal.FreeHGlobal(authPtr);
                return resultCode;
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLMusicServiceNativeBindings.SetAuthStringHelper failed. Reason: API symbols not found");
                return MLResult.Code.UnspecifiedFailure;
            }
        }

        /// <summary>
        /// Converts URL string to UTF8 and sets it for the MusicService.
        /// </summary>
        /// <param name="url">The URL to play</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        public static MLResult.Code SetURLHelper(string url)
        {
            try
            {
                IntPtr urlPtr = MLConvert.EncodeToUnmanagedUTF8(url);
                MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceSetURL(urlPtr);
                Marshal.FreeHGlobal(urlPtr);
                return resultCode;
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLMusicServiceNativeBindings.SetURLHelper failed. Reason: API symbols not found");
                return MLResult.Code.UnspecifiedFailure;
            }
        }

        /// <summary>
        /// Converts playlist URLs to UTF8 and sets it to use for the playlist in the MusicService.
        /// </summary>
        /// <param name="playlist">The array of URLs</param>
        /// <param name="size">The size of the playList array</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        public static MLResult.Code SetPlayListHelper(string[] playlist, ulong size)
        {
            try
            {
                IntPtr[] intPtrList = new IntPtr[playlist.Length];
                for (int i = 0; i < playlist.Length; ++i)
                {
                    intPtrList[i] = MLConvert.EncodeToUnmanagedUTF8(playlist[i]);
                }

                MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceSetPlayList(intPtrList, size);

                for (int i = 0; i < intPtrList.Length; ++i)
                {
                    Marshal.FreeHGlobal(intPtrList[i]);
                }

                return resultCode;
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLMusicServiceNativeBindings.SetPlayListHelper failed. Reason: API symbols not found");
                return MLResult.Code.UnspecifiedFailure;
            }
        }

        /// <summary>
        /// Handles playback state changes.
        /// </summary>
        /// <param name="state">The new state.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        [AOT.MonoPInvokeCallback(typeof(OnPlaybackStateChangeCallback))]
        private static void HandlePlaybackStateChange(MLMusicService.PlaybackStateType state, IntPtr data)
        {
            MLThreadDispatch.Call(state, OnPlaybackStateChange);
        }

        /// <summary>
        /// Handles repeat state changes.
        /// </summary>
        /// <param name="state">The new state.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        [AOT.MonoPInvokeCallback(typeof(OnRepeatStateChangeCallback))]
        private static void HandleRepeatStateChange(MLMusicService.RepeatStateType state, IntPtr data)
        {
            MLThreadDispatch.Call(state, OnRepeatStateChange);
        }

        /// <summary>
        /// Handles shuffle state changes.
        /// </summary>
        /// <param name="state">The new state.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        [AOT.MonoPInvokeCallback(typeof(OnShuffleStateChangeCallback))]
        private static void HandleShuffleStateChange(MLMusicService.ShuffleStateType state, IntPtr data)
        {
            MLThreadDispatch.Call(state, OnShuffleStateChange);
        }

        /// <summary>
        /// Handles metadata changes.
        /// </summary>
        /// <param name="metadata">The new metadata.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        [AOT.MonoPInvokeCallback(typeof(OnMetadataChangeCallback))]
        private static void HandleMetadataChange(ref MLMusicServiceNativeBindings.MetadataNative metadata, IntPtr data)
        {
            MLMusicService.Metadata lambdaExtra = metadata.Data;
            MLThreadDispatch.Call(lambdaExtra, OnMetadataChange);
        }

        /// <summary>
        /// Handles position changes.
        /// </summary>
        /// <param name="position">The new position.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        [AOT.MonoPInvokeCallback(typeof(OnPositionChangeCallback))]
        private static void HandlePositionChange(int position, IntPtr data)
        {
            MLThreadDispatch.Call(position, OnPositionChange);
        }

        /// <summary>
        /// Handles new errors.
        /// </summary>
        /// <param name="type">Error type.</param>
        /// <param name="code">Error code.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        [AOT.MonoPInvokeCallback(typeof(OnErrorCallback))]
        private static void HandleError(MLMusicService.ErrorType type, int code, IntPtr data)
        {
            MLMusicService.Error error = new MLMusicService.Error { Type = type, Code = code };
            MLThreadDispatch.Call(error, OnError);
        }

        /// <summary>
        /// Handles when the music service status changes.
        /// </summary>
        /// <param name="status">The new status.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        [AOT.MonoPInvokeCallback(typeof(OnStatusChangeCallback))]
        private static void HandleStatusChange(MLMusicService.Status status, IntPtr data)
        {
            MLThreadDispatch.Call(status, OnStatusChange);
        }

        /// <summary>
        /// Handles when the music service volume changes.
        /// </summary>
        /// <param name="volume">The new volume.</param>
        /// <param name="data">User pointer that was set through MLMusicServiceSetCallbacks().</param>
        [AOT.MonoPInvokeCallback(typeof(OnVolumeChangeCallback))]
        private static void HandleVolumeChange(float volume, IntPtr data)
        {
            MLThreadDispatch.Call(volume, OnVolumeChange);
        }

        /// <summary>
        /// Callbacks to be implemented by client to receive information from
        /// MusicService if callback mechanism is used. Note that the data passed
        /// into callbacks is destroyed upon return.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CallbacksNative
        {
            /// <summary>
            /// Raised when playback state has changed.
            /// </summary>
            public OnPlaybackStateChangeCallback OnPlaybackStateChange;

            /// <summary>
            /// Raised when repeat state has changed.
            /// </summary>
            public OnRepeatStateChangeCallback OnRepeatStateChange;

            /// <summary>
            /// Raised when shuffle state has changed.
            /// </summary>
            public OnShuffleStateChangeCallback OnShuffleStateChange;

            /// <summary>
            /// Raised when metadata has has changed.
            /// </summary>
            public OnMetadataChangeCallback OnMetadataChange;

            /// <summary>
            /// Raised when playback head position has changed.
            /// </summary>
            public OnPositionChangeCallback OnPositionChange;

            /// <summary>
            /// Raised when an error has occurred.
            /// </summary>
            public OnErrorCallback OnError;

            /// <summary>
            /// Raised when the Music Service status has changed.
            /// </summary>
            public OnStatusChangeCallback OnStatusChange;

            /// <summary>
            /// Raised when the Music Service volume has changed.
            /// </summary>
            public OnVolumeChangeCallback OnVolumeChange;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <returns>The new generated CallbacksNative struct.</returns>
            public static CallbacksNative Create()
            {
                return new CallbacksNative()
                {
                    OnPlaybackStateChange = null,
                    OnRepeatStateChange = null,
                    OnShuffleStateChange = null,
                    OnMetadataChange = null,
                    OnPositionChange = null,
                    OnError = null,
                    OnStatusChange = null,
                    OnVolumeChange = null
                };
            }

            /// <summary>
            /// Allocates the static pointer to the CallbacksNative passed in. This is used to pass it into the CAPI.
            /// </summary>
            /// <returns>The pointer to the native callbacks.</returns>
            public static IntPtr AllocateSystemCallbacks()
            {
                if (SystemCallbacks != IntPtr.Zero)
                {
                    MLPluginLog.Error("MLMusicServiceNativeBindings.CallbacksNative.AllocateCallbacksPointer failed to allocate new callbacks pointer. Reason: Static callback pointer was already allocated, please call MLMusicServiceNativeBindings.CallbacksNative.DeallocateCallbacksPointer and try again");
                    return IntPtr.Zero;
                }

                SystemCallbacks = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CallbacksNative)));
                CallbacksNative callbacks = CallbacksNative.Create();
                callbacks.OnPlaybackStateChange = HandlePlaybackStateChange;
                callbacks.OnRepeatStateChange = HandleRepeatStateChange;
                callbacks.OnShuffleStateChange = HandleShuffleStateChange;
                callbacks.OnMetadataChange = HandleMetadataChange;
                callbacks.OnPositionChange = HandlePositionChange;
                callbacks.OnError = HandleError;
                callbacks.OnStatusChange = HandleStatusChange;
                callbacks.OnVolumeChange = HandleVolumeChange;
                Marshal.StructureToPtr(callbacks, SystemCallbacks, false);
                return SystemCallbacks;
            }

            /// <summary>
            /// De-allocates previously allocated pointer to a CallbacksNative struct.
            /// </summary>
            public static void DeallocateSystemCallbacks()
            {
                if (SystemCallbacks == IntPtr.Zero)
                {
                    return;
                }

                Marshal.FreeHGlobal(SystemCallbacks);
                SystemCallbacks = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Contains the meta data for a track.
        /// Currently only provides support for ANSI strings.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public partial struct MetadataNative
        {
            /// <summary>
            /// Track name/title.
            /// </summary>
            public IntPtr TrackTitle;

            /// <summary>
            /// Album name.
            /// </summary>
            public IntPtr AlbumInfoName;

            /// <summary>
            /// Album URL.
            /// </summary>
            public IntPtr AlbumInfoUrl;

            /// <summary>
            /// Album cover URL.
            /// </summary>
            public IntPtr AlbumInfoCoverUrl;

            /// <summary>
            /// Artist name.
            /// </summary>
            public IntPtr ArtistInfoName;

            /// <summary>
            /// Artist URL.
            /// </summary>
            public IntPtr ArtistInfoUrl;

            /// <summary>
            /// Length/Duration of the track in seconds.
            /// </summary>
            public uint Length;

            /// <summary>
            /// Gets the user facing struct from the native one.
            /// </summary>
            public MLMusicService.Metadata Data
            {
                get
                {
                    return new MLMusicService.Metadata()
                    {
                        TrackTitle = MLConvert.DecodeUTF8(this.TrackTitle),
                        AlbumInfoName = MLConvert.DecodeUTF8(this.AlbumInfoName),
                        AlbumInfoUrl = MLConvert.DecodeUTF8(this.AlbumInfoUrl),
                        AlbumInfoCoverUrl = MLConvert.DecodeUTF8(this.AlbumInfoCoverUrl),
                        ArtistInfoName = MLConvert.DecodeUTF8(this.ArtistInfoName),
                        ArtistInfoUrl = MLConvert.DecodeUTF8(this.ArtistInfoUrl),
                        Length = this.Length,
                    };
                }
            }

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <returns>The new generated MetadataNative struct.</returns>
            public static MetadataNative Create()
            {
                return new MetadataNative()
                {
                    TrackTitle = IntPtr.Zero,
                    AlbumInfoName = IntPtr.Zero,
                    AlbumInfoUrl = IntPtr.Zero,
                    AlbumInfoCoverUrl = IntPtr.Zero,
                    ArtistInfoName = IntPtr.Zero,
                    ArtistInfoUrl = IntPtr.Zero,
                    Length = 0u,
                };
            }
        }
    }
}

#endif
