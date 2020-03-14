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

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Runtime.InteropServices;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// MLMusicService class is the entry point for the MusicService API.
    /// </summary>
    public sealed partial class MLMusicService : MLAPISingleton<MLMusicService>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// Name of the music provider.
        /// </summary>
        private string musicServiceProvider;

        /// <summary>
        /// Stores if the music service is connected.
        /// </summary>
        private bool connected = false;

        /// <summary>
        /// Prevents a default instance of the <see cref="MLMusicService"/> class from being created.
        /// </summary>
        private MLMusicService()
        {
            this.DllNotFoundError = "MLMusicService API is currently available only on device.";
        }

        /// <summary>
        /// This delegate is meant to handle playback state changes.
        /// </summary>
        /// <param name="state">The new state.</param>
        public delegate void OnPlaybackStateChangeDelegate(PlaybackStateType state);

        /// <summary>
        /// This delegate is meant to handle repeat state changes.
        /// </summary>
        /// <param name="state">The new state.</param>
        public delegate void OnRepeatStateChangeDelegate(RepeatStateType state);

        /// <summary>
        /// This delegate is meant to handle shuffle state changes.
        /// </summary>
        /// <param name="state">The new state.</param>
        public delegate void OnShuffleStateChangeDelegate(ShuffleStateType state);

        /// <summary>
        /// This delegate is meant to handle metadata changes.
        /// </summary>
        /// <param name="metadata">The new metadata.</param>
        public delegate void OnMetadataChangeDelegate(Metadata metadata);

        /// <summary>
        /// This delegate is meant to handle position changes.
        /// </summary>
        /// <param name="position">The new position.</param>
        public delegate void OnPositionChangeDelegate(int position);

        /// <summary>
        /// This delegate is meant to handle when the music service encounters an error.
        /// </summary>
        /// <param name="error">The error.</param>
        public delegate void OnErrorDelegate(Error error);

        /// <summary>
        /// This delegate is meant to handle when the music service status changes.
        /// </summary>
        /// <param name="status">The new status.</param>
        public delegate void OnStatusChangeDelegate(Status status);

        /// <summary>
        /// This delegate is meant to handle when the music service volume changes.
        /// </summary>
        /// <param name="volume">The new volume.</param>
        public delegate void OnVolumeChangeDelegate(float volume);

        /// <summary>
        /// Raised when playback state has changed.
        /// </summary>
        public static event OnPlaybackStateChangeDelegate OnPlaybackStateChange
        {
            add
            {
                MLMusicServiceNativeBindings.OnPlaybackStateChange += value;
            }

            remove
            {
                MLMusicServiceNativeBindings.OnPlaybackStateChange -= value;
            }
        }

        /// <summary>
        /// Raised when repeat state has changed.
        /// </summary>
        public static event OnRepeatStateChangeDelegate OnRepeatStateChange
        {
            add
            {
                MLMusicServiceNativeBindings.OnRepeatStateChange += value;
            }

            remove
            {
                MLMusicServiceNativeBindings.OnRepeatStateChange -= value;
            }
        }

        /// <summary>
        /// Raised when shuffle state has changed.
        /// </summary>
        public static event OnShuffleStateChangeDelegate OnShuffleStateChange
        {
            add
            {
                MLMusicServiceNativeBindings.OnShuffleStateChange += value;
            }

            remove
            {
                MLMusicServiceNativeBindings.OnShuffleStateChange -= value;
            }
        }

        /// <summary>
        /// Raised when metadata has has changed.
        /// </summary>
        public static event OnMetadataChangeDelegate OnMetadataChange
        {
            add
            {
                MLMusicServiceNativeBindings.OnMetadataChange += value;
            }

            remove
            {
                MLMusicServiceNativeBindings.OnMetadataChange -= value;
            }
        }

        /// <summary>
        /// Raised when playback head position has changed.
        /// </summary>
        public static event OnPositionChangeDelegate OnPositionChange
        {
            add
            {
                MLMusicServiceNativeBindings.OnPositionChange += value;
            }

            remove
            {
                MLMusicServiceNativeBindings.OnPositionChange -= value;
            }
        }

        /// <summary>
        /// Raised when an error has occurred.
        /// </summary>
        public static event OnErrorDelegate OnError
        {
            add
            {
                MLMusicServiceNativeBindings.OnError += value;
            }

            remove
            {
                MLMusicServiceNativeBindings.OnError -= value;
            }
        }

        /// <summary>
        /// Raised when the Music Service status has changed.
        /// </summary>
        public static event OnStatusChangeDelegate OnStatusChange
        {
            add
            {
                MLMusicServiceNativeBindings.OnStatusChange += value;
            }

            remove
            {
                MLMusicServiceNativeBindings.OnStatusChange -= value;
            }
        }

        /// <summary>
        /// Raised when the Music Service volume has changed.
        /// </summary>
        public static event OnVolumeChangeDelegate OnVolumeChange
        {
            add
            {
                MLMusicServiceNativeBindings.OnVolumeChange += value;
            }

            remove
            {
                MLMusicServiceNativeBindings.OnVolumeChange -= value;
            }
        }
        #endif

        /// <summary>
        /// MusicService Error type.
        /// </summary>
        public enum ErrorType : uint
        {
            /// <summary>
            /// No error.
            /// </summary>
            None = 0,

            /// <summary>
            /// Connectivity error.
            /// </summary>
            Connectivity,

            /// <summary>
            /// Timeout error.
            /// </summary>
            Timeout,

            /// <summary>
            /// General playback error.
            /// </summary>
            GeneralPlayback,

            /// <summary>
            /// Privilege error.
            /// </summary>
            Privilege,

            /// <summary>
            /// Service specific error.
            /// </summary>
            ServiceSpecific,

            /// <summary>
            /// No memory error.
            /// </summary>
            NoMemory,

            /// <summary>
            /// Unspecified error.
            /// </summary>
            Unspecified,
        }

        /// <summary>
        /// MusicService status.
        /// </summary>
        public enum Status : uint
        {
            /// <summary>
            /// Context changed.
            /// </summary>
            ContextChanged = 0,

            /// <summary>
            /// Context was created.
            /// </summary>
            Created,

            /// <summary>
            /// Logged in.
            /// </summary>
            LoggedIn,

            /// <summary>
            /// Logged out.
            /// </summary>
            LoggedOut,

            /// <summary>
            /// Next track.
            /// </summary>
            NextTrack,

            /// <summary>
            /// Previous track.
            /// </summary>
            PrevTrack,

            /// <summary>
            /// Track changed.
            /// </summary>
            TrackChanged,

            /// <summary>
            /// Unknown status.
            /// </summary>
            Unknown,
        }

        /// <summary>
        /// MusicService playback state.
        /// </summary>
        public enum PlaybackStateType : uint
        {
            /// <summary>
            /// Playback playing.
            /// </summary>
            Playing = 0,

            /// <summary>
            /// Playback paused.
            /// </summary>
            Paused,

            /// <summary>
            /// Playback stopped.
            /// </summary>
            Stopped,

            /// <summary>
            /// Playback had an error.
            /// </summary>
            Error,

            /// <summary>
            /// Unknown playback state.
            /// </summary>
            Unknown,
        }

        /// <summary>
        /// MusicService repeat setting
        /// </summary>
        public enum RepeatStateType : uint
        {
            /// <summary>
            /// Repeat state Off.
            /// </summary>
            Off = 0,

            /// <summary>
            /// Repeat state Song.
            /// </summary>
            Song,

            /// <summary>
            /// Repeat state Album.
            /// </summary>
            Album,

            /// <summary>
            /// Repeat state Unknown.
            /// </summary>
            Unknown,
        }

        /// <summary>
        /// MusicService shuffle setting
        /// </summary>
        public enum ShuffleStateType : uint
        {
            /// <summary>
            /// Shuffle state On.
            /// </summary>
            On = 0,

            /// <summary>
            /// Shuffle state Off.
            /// </summary>
            Off,

            /// <summary>
            /// Shuffle state Unknown.
            /// </summary>
            Unknown,
        }

        /// <summary>
        /// MusicService enumerations used to get the Metadata information of a track.
        /// </summary>
        public enum TrackType : uint
        {
            /// <summary>
            /// Previous track.
            /// </summary>
            Previous = 0,

            /// <summary>
            /// Current track.
            /// </summary>
            Current,

            /// <summary>
            /// Next track.
            /// </summary>
            Next,
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Gets or sets the music volume in range [0.0f ... 1.0f]. This can only be set when a media has been loaded.
        /// </summary>
        public static float Volume
        {
            get
            {
                return MLMusicService.GetVolume();
            }

            set
            {
                MLMusicService.SetVolume(value);
            }
        }

        /// <summary>
        /// Gets the current track length in milliseconds.
        /// </summary>
        public static uint TrackLength
        {
            get
            {
                return MLMusicService.GetTrackLength();
            }
        }

        /// <summary>
        /// Gets the current position of play head in milliseconds.
        /// </summary>
        public static uint CurrentPosition
        {
            get
            {
                return MLMusicService.GetCurrentPosition();
            }
        }

        /// <summary>
        /// Gets the current playback state of the music service.
        /// </summary>
        public static PlaybackStateType PlaybackState
        {
            get
            {
                return MLMusicService.GetPlaybackState();
            }
        }

        /// <summary>
        /// Gets or sets the current repeat state of the music service.
        /// </summary>
        public static RepeatStateType RepeatState
        {
            get
            {
                return MLMusicService.GetRepeatState();
            }

            set
            {
                MLMusicService.SetRepeatState(value);
            }
        }

        /// <summary>
        /// Gets or sets the current shuffle state of the music service.
        /// </summary>
        public static ShuffleStateType ShuffleState
        {
            get
            {
                return MLMusicService.GetShuffleState();
            }

            set
            {
                MLMusicService.SetShuffleState(value);
            }
        }

        /// <summary>
        /// Starts the Music Service API.
        /// </summary>
        /// <param name="musicServiceProvider">The name of the music service provider</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if connected to MusicService successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if one of the parameters is invalid.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if connection failed with resource allocation failure
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericAlreadyExists</c> if connection exists already.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        public static MLResult Start(string musicServiceProvider)
        {
            CreateInstance(musicServiceProvider);
            return MLMusicService.BaseStart();
        }

        /// <summary>
        /// Sets the authentication string for the MusicService.
        /// </summary>
        /// <param name="authString">Service provider specific authentication string.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not music service is not connected.
        /// </returns>
        public static MLResult SetAuthString(string authString)
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.SetAuthStringHelper(authString);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.SetAuthString failed to set music service authentication string. Reason: {0}", result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.SetAuthString failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.SetAuthString failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.SetAuthString failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.SetAuthString failed. Reason: No Instance for MLMusicService");
            }
        }

        /// <summary>
        /// Set a single, specific URL to play.
        /// </summary>
        /// <param name="url">The URL to play.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        public static MLResult SetURL(string url)
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    if (string.IsNullOrEmpty(url))
                    {
                        MLResult result = MLResult.Create(MLResult.Code.InvalidParam, "Empty URL parameter");
                        MLPluginLog.ErrorFormat("MLMusicService.SetURL failed to set URL. Reason: {0}", result);
                        return result;
                    }

                    MLResult.Code resultCode = MLMusicServiceNativeBindings.SetURLHelper(url);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.SetURL failed to set URL. Reason: {0}", result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.SetURL failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.SetURL failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.SetURL failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.SetURL failed. Reason: No Instance for MLMusicService");
            }
        }

        /// <summary>
        /// Set an array of URLs to use for the playlist.
        /// </summary>
        /// <param name="playList">The array of URLs</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        public static MLResult SetPlayList(string[] playList)
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    if (playList.Length == 0)
                    {
                        MLResult result = MLResult.Create(MLResult.Code.InvalidParam, "Empty playlist parameter");
                        MLPluginLog.ErrorFormat("MLMusicService.SetPlayList failed to set playlist. Reason: {0}", result);
                        return result;
                    }

                    MLResult.Code resultCode = MLMusicServiceNativeBindings.SetPlayListHelper(playList, (ulong)playList.Length);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.SetPlayList failed to set playlist. Reason: {0}", result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.SetPlayList failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.SetPlayList failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.SetPlayList failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.SetPlayList failed. Reason: No Instance for MLMusicService");
            }
        }

        /// <summary>
        /// Start playing the current track.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        public static MLResult StartPlayback()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceStart();
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.StartPlayback failed to start playback. Reason: {0}", result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.StartPlayback failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.StartPlayback failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.StartPlayback failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.StartPlayback failed. Reason: No Instance for MLMusicService");
            }
        }

        /// <summary>
        /// Stop playing the current track.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        public static MLResult StopPlayback()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceStop();
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.StopPlayback failed to stop playback. Reason: {0}", result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.StopPlayback failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.StopPlayback failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.StopPlayback failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.StopPlayback failed. Reason: No Instance for MLMusicService");
            }
        }

        /// <summary>
        /// Pause the current track.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        public static MLResult PausePlayback()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServicePause();
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.PausePlayback failed to pause playback. Reason: {0}", result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.PausePlayback failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.PausePlayback failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.PausePlayback failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.PausePlayback failed. Reason: No Instance for MLMusicService");
            }
        }

        /// <summary>
        /// Resume playback of the current track.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        public static MLResult ResumePlayback()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceResume();
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.ResumePlayback failed to resume playback. Reason: {0}", result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.ResumePlayback failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.ResumePlayback failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.ResumePlayback failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.ResumePlayback failed. Reason: No Instance for MLMusicService");
            }
        }

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
        public static MLResult Seek(uint position)
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceSeek(position);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.Seek failed to seek to position {0}. Reason: {1}", position, result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.Seek failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.Seek failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.Seek failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.Seek failed. Reason: No Instance for MLMusicService");
            }
        }

        /// <summary>
        /// Advances to the next track in the playlist
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        public static MLResult Next()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceNext();
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.Next failed move to the next track. Reason: {0}", result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.Next failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.Next failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.Next failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.Next failed. Reason: No Instance for MLMusicService");
            }
        }

        /// <summary>
        /// Moves to the previous track in the playlist
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        public static MLResult Previous()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServicePrevious();
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.Previous failed move to the previous track. Reason: {0}", result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.Previous failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.Previous failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.Previous failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.Previous failed. Reason: No Instance for MLMusicService");
            }
        }

        /// <summary>
        /// Get the music service status
        /// </summary>
        /// <param name="status">Status from the music service</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// </returns>
        public static MLResult GetStatus(ref Status status)
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceGetStatus(out status);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.GetStatus failed get music service status. Reason: {0}", result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.GetStatus failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.GetStatus failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.GetStatus failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.GetStatus failed. Reason: No Instance for MLMusicService");
            }
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
        public static MLResult GetError(ref Error error)
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceGetError(out error.Type, out error.Code);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.GetError failed to get music service error. Reason: {0}", result);
                        return result;
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.GetError failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.GetError failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.GetError failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.GetError failed. Reason: No Instance for MLMusicService");
            }
        }

        /// <summary>
        ///  Get the metadata for a track.
        /// </summary>
        /// <param name="metadata">Structure to contain the metadata.</param>
        /// <param name="relativeOffest">The relative offset from the current track that you would like metadata for. Default (0) will return the metadata of the current track.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if one of the parameters is invalid.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericNoInit</c> if not connected.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        public static MLResult GetMetadata(ref Metadata metadata, int relativeOffest = 0)
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    return MLResult.Create(MLResult.Code.MediaGenericNoInit, "Music Service is not connected.");
                }

                try
                {
                    // Internally, the music service malloc's memory for the strings used in this and then
                    // expects them to be freed. If we just pass in the above metadata struct, we can no longer
                    // just return it because the internal memory won't be freed
                    // We can just make a temporary one and free it, and let the 'metadata' structure that we return
                    // be handled entirely in managed space
                    MLMusicServiceNativeBindings.MetadataNative metadataInternal = MLMusicServiceNativeBindings.MetadataNative.Create();

                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceGetMetadataForIndex(relativeOffest, ref metadataInternal);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLMusicService.GetMetadata failed to get metadata. Reason: {0}", result);
                        return result;
                    }
                    else
                    {
                        metadata = metadataInternal.Data;
                        resultCode = MLMusicServiceNativeBindings.MLMusicServiceReleaseMetadata(ref metadataInternal);
                        if (resultCode != MLResult.Code.Ok)
                        {
                            MLResult result = MLResult.Create(resultCode);
                            MLPluginLog.ErrorFormat("MLMusicService.GetMetadata failed to release metadata. Reason: {0}", result);
                            return result;
                        }
                    }

                    return MLResult.Create(MLResult.Code.Ok);
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.GetMetadata failed. Reason: API symbols not found");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.GetMetadata failed. Reason: API symbols not found");
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.GetMetadata failed. Reason: No Instance for MLMusicService");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMusicService.GetMetadata failed. Reason: No Instance for MLMusicService");
            }
        }

        #if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Starts the Music Service API and sets up callbacks.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if connected to MusicService successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if one of the parameters is invalid.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if connection failed with resource allocation failure
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericAlreadyExists</c> if connection exists already.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        protected override MLResult StartAPI()
        {
            MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceConnect(_instance.musicServiceProvider);
            if (resultCode != MLResult.Code.Ok)
            {
                _instance.connected = false;

                MLResult result = MLResult.Create(resultCode);
                MLPluginLog.ErrorFormat("MLMusicService.StartAPI failed to connect to the music service provider \"{0}\". Reason: {1}", _instance.musicServiceProvider, result);
                return result;
            }

            _instance.connected = true;

            MLMusicServiceNativeBindings.CallbacksNative.AllocateSystemCallbacks();
            resultCode = MLMusicServiceNativeBindings.MLMusicServiceSetCallbacks(MLMusicServiceNativeBindings.SystemCallbacks, IntPtr.Zero);
            if (resultCode != MLResult.Code.Ok)
            {
                MLResult result = MLResult.Create(resultCode);
                MLPluginLog.ErrorFormat("MLMusicService.StartAPI failed to set callbacks for the music service provider \"{0}\". Reason: {1}", _instance.musicServiceProvider, result);
                return result;
            }

            return MLResult.Create(MLResult.Code.Ok);
        }
        #endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Cleans up unmanaged memory.
        /// </summary>
        /// <param name="isSafeToAccessManagedObject">Allow complete cleanup of the API.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObject)
        {
            if (isSafeToAccessManagedObject)
            {
                _instance.musicServiceProvider = string.Empty;
            }

            try
            {
                MLResult.Code resultCode;

                if (PlaybackState == PlaybackStateType.Playing || PlaybackState == PlaybackStateType.Paused)
                {
                    resultCode = MLMusicServiceNativeBindings.MLMusicServiceStop();
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLMusicService.CleanupAPI failed trying to stop the music service \"{0}\". Reason: {1}", _instance.musicServiceProvider, MLResult.CodeToString(resultCode));
                    }
                }

                resultCode = MLMusicServiceNativeBindings.MLMusicServiceDisconnect();
                if (resultCode != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMusicService.CleanupAPI failed trying to disconnect from the music service \"{0}\". Reason: {1}", _instance.musicServiceProvider, MLResult.CodeToString(resultCode));
                }

                _instance.connected = false;

                MLMusicServiceNativeBindings.CallbacksNative.DeallocateSystemCallbacks();
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLMusicService.CleanupAPI failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Required Update function override.
        /// </summary>
        protected override void Update()
        {
        }

        /// <summary>
        /// Instantiates the MLMusicService class if one doesn't already exist.
        /// </summary>
        /// <param name="musicServiceProvider">Name of the music service provider.</param>
        private static void CreateInstance(string musicServiceProvider)
        {
            if (!MLMusicService.IsValidInstance())
            {
                MLMusicService._instance = new MLMusicService
                {
                    musicServiceProvider = musicServiceProvider,
                };
            }
        }

        /// <summary>
        /// Polls the volume level of MusicService Provider.
        /// </summary>
        /// <returns>The current volume.</returns>
        private static float GetVolume()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    MLPluginLog.Error("MLMusicService.GetVolume failed get music volume. Reason: Music Service is not connected.");
                    return 0.0f;
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceGetVolume(out float volume);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLMusicService.GetVolume failed to get music volume. Reason: {0}", MLResult.CodeToString(resultCode));
                    }

                    return volume;
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.GetVolume failed. Reason: API symbols not found");
                    return 0.0f;
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.GetVolume failed. Reason: No Instance for MLMusicService");
                return 0.0f;
            }
        }

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="volume">Value to set the volume to.</param>
        private static void SetVolume(float volume)
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    MLPluginLog.Error("MLMusicService.SetVolume failed set music volume. Reason: Music Service is not connected.");
                    return;
                }

                try
                {
                    if (volume < 0.0f || volume > 1.0f)
                    {
                        MLPluginLog.ErrorFormat("MLMusicService.SetVolume failed to set music volume. Reason: Volume parameter was outside of range [0.. 1]");
                        return;
                    }

                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceSetVolume(volume);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLMusicService.SetVolume failed to set music volume. Reason: {0}", MLResult.CodeToString(resultCode));
                    }
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.SetVolume failed. Reason: API symbols not found");
                    return;
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.SetVolume failed. Reason: No Instance for MLMusicService");
            }
        }

        /// <summary>
        /// Gets the current track's length in seconds.
        /// </summary>
        /// <returns>The current track's length.</returns>
        private static uint GetTrackLength()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    MLPluginLog.Error("MLMusicService.GetTrackLength failed get track length. Reason: Music Service is not connected.");
                    return 0;
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceGetTrackLength(out uint trackLength);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLMusicService.GetTrackLength failed get track length. Reason: {0}", MLResult.CodeToString(resultCode));
                    }

                    return trackLength;
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.GetTrackLength failed. Reason: API symbols not found");
                    return 0;
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.GetTrackLength failed. Reason: No Instance for MLMusicService");
                return 0;
            }
        }

        /// <summary>
        /// Obtains the position of the current track.
        /// </summary>
        /// <returns>Position of the current track.</returns>
        private static uint GetCurrentPosition()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    MLPluginLog.Error("MLMusicService.GetCurrentPosition failed get play head current position. Reason: Music Service is not connected.");
                    return 0;
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceGetCurrentPosition(out uint position);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLMusicService.GetCurrentPosition failed get play head current position. Reason: {0}", MLResult.CodeToString(resultCode));
                    }

                    return position;
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.GetCurrentPosition failed. Reason: API symbols not found");
                    return 0;
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.GetCurrentPosition failed. Reason: No Instance for MLMusicService");
                return 0;
            }
        }

        /// <summary>
        /// Polls the state of playback from MusicService.
        /// </summary>
        /// <returns>The state of playback.</returns>
        private static PlaybackStateType GetPlaybackState()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    MLPluginLog.Error("MLMusicService.GetPlaybackState failed to get playback state. Reason: Music Service is not connected.");
                    return PlaybackStateType.Unknown;
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceGetPlaybackState(out PlaybackStateType state);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLMusicService.GetPlaybackState failed to get playback state. Reason: {0}", MLResult.CodeToString(resultCode));
                    }

                    return state;
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.GetPlaybackState failed. Reason: API symbols not found");
                    return PlaybackStateType.Unknown;
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.GetPlaybackState failed. Reason: No Instance for MLMusicService");
                return PlaybackStateType.Unknown;
            }
        }

        /// <summary>
        /// Polls the state of repeat setting from MusicService.
        /// </summary>
        /// <returns>The repeat setting state.</returns>
        private static RepeatStateType GetRepeatState()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    MLPluginLog.Error("MLMusicService.GetRepeatState failed to get repeat mode. Reason: Music Service is not connected.");
                    return RepeatStateType.Unknown;
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceGetRepeatState(out RepeatStateType state);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLMusicService.GetRepeatState failed to get repeat mode. Reason: {0}", MLResult.CodeToString(resultCode));
                    }

                    return state;
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.GetRepeatState failed. Reason: API symbols not found");
                    return RepeatStateType.Unknown;
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.GetRepeatState failed. Reason: No Instance for MLMusicService");
                return RepeatStateType.Unknown;
            }
        }

        /// <summary>
        /// Sets the repeat mode.
        /// </summary>
        /// <param name="state">State to set repeat mode to.</param>
        private static void SetRepeatState(RepeatStateType state)
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    MLPluginLog.Error("MLMusicService.SetRepeatState failed to set repeat mode. Reason: Music Service is not connected.");
                    return;
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceSetRepeat(state);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLMusicService.SetRepeatState failed to set repeat mode. Reason: {0}", MLResult.CodeToString(resultCode));
                    }
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.SetRepeatState failed. Reason: API symbols not found");
                    return;
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.SetRepeatState failed. Reason: No Instance for MLMusicService");
                return;
            }
        }

        /// <summary>
        /// Polls the state of shuffle setting from MusicService.
        /// </summary>
        /// <returns>The current shuffle state.</returns>
        private static ShuffleStateType GetShuffleState()
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    MLPluginLog.Error("MLMusicService.GetShuffleState failed to get shuffle mode. Reason: Music Service is not connected.");
                    return ShuffleStateType.Unknown;
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceGetShuffleState(out ShuffleStateType state);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLMusicService.GetShuffleState failed to get shuffle mode. Reason: {0}", MLResult.CodeToString(resultCode));
                    }

                    return state;
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.GetShuffleState failed. Reason: API symbols not found");
                    return ShuffleStateType.Unknown;
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.GetShuffleState failed. Reason: No Instance for MLMusicService");
                return ShuffleStateType.Unknown;
            }
        }

        /// <summary>
        /// Sets the shuffle mode.
        /// </summary>
        /// <param name="state">The state to set shuffle to.</param>
        private static void SetShuffleState(ShuffleStateType state)
        {
            if (MLMusicService.IsValidInstance())
            {
                if (!_instance.connected)
                {
                    MLPluginLog.Error("MLMusicService.SetShuffleState failed to set shuffle mode. Reason: Music Service is not connected.");
                    return;
                }

                try
                {
                    MLResult.Code resultCode = MLMusicServiceNativeBindings.MLMusicServiceSetShuffle(state);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLMusicService.SetShuffleState failed to set shuffle mode. Reason: {0}", MLResult.CodeToString(resultCode));
                    }
                }
                catch (System.EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLMusicService.SetShuffleState failed. Reason: API symbols not found");
                    return;
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLMusicService.SetShuffleState failed. Reason: No Instance for MLMusicService");
                return;
            }
        }

        /// <summary>
        /// Contains the meta data for a track
        /// Currently only provides support for ANSI strings
        /// </summary>
        public struct Metadata
        {
            /// <summary>
            /// Track name/title.
            /// </summary>
            public string TrackTitle;

            /// <summary>
            /// Album name.
            /// </summary>
            public string AlbumInfoName;

            /// <summary>
            /// Album URL.
            /// </summary>
            public string AlbumInfoUrl;

            /// <summary>
            /// Album cover URL.
            /// </summary>
            public string AlbumInfoCoverUrl;

            /// <summary>
            /// Artist name.
            /// </summary>
            public string ArtistInfoName;

            /// <summary>
            /// Artist URL.
            /// </summary>
            public string ArtistInfoUrl;

            /// <summary>
            /// Length/Duration of the track in seconds.
            /// </summary>
            public uint Length;
        }

        /// <summary>
        /// Contains the error type and error code received from the music service
        /// </summary>
        public struct Error
        {
            /// <summary>
            /// The error type.
            /// </summary>
            public ErrorType Type;

            /// <summary>
            /// For ErrorTypes other than ServiceSpecific, the value of ErrorCode will match the value of ErrorType.
            /// When ErrorType is ServiceSpecific, the ErrorCode will be specific to the music streaming service.
            /// </summary>
            public int Code;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <returns>The new generated Error.</returns>
            public static Error Create()
            {
                return new Error()
                {
                    Type = ErrorType.None,
                    Code = 0
                };
            }
        }
        #endif
    }
}
