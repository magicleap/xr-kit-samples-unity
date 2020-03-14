// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://auth.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using MagicLeap;
using System.IO;
using System;

namespace MagicLeap
{
    /// <summary>
    /// This class demonstrates using the MLMusicService API
    /// </summary>
    public class MLMusicServiceBehavior : MonoBehaviour
    {
        #pragma warning disable 414
        [SerializeField, Tooltip("The visible name of the music provider to connect to.")]
        private string MusicServiceProvider = "ExampleMusicProvider";
        #pragma warning restore 414

        [SerializeField, Tooltip("The path of the tracks provided below starting from the StreammingAssets folder.")]
        public string StreamingAssetsLocationFolder = "BackgroundMusicExample";

        [SerializeField, Tooltip("Playlist of local files in StreamingAssets.")]
        public string[] StreamingPlaylist;

        public MLMusicService.PlaybackStateType PlaybackState
        {
            get
            {
                #if PLATFORM_LUMIN
                return MLMusicService.PlaybackState;
                #else
                return MLMusicService.PlaybackStateType.Unknown;
                #endif
            }
        }

        public MLMusicService.ShuffleStateType ShuffleState
        {
            get
            {
                #if PLATFORM_LUMIN
                return MLMusicService.ShuffleState;
                #else
                return MLMusicService.ShuffleStateType.Unknown;
                #endif
            }
        }

        public MLMusicService.RepeatStateType RepeatState
        {
            get
            {
                #if PLATFORM_LUMIN
                return MLMusicService.RepeatState;
                #else
                return MLMusicService.RepeatStateType.Unknown;
                #endif
            }
        }

        public uint CurrentPosition
        {
            get
            {
                #if PLATFORM_LUMIN
                return MLMusicService.CurrentPosition;
                #else
                return 0;
                #endif
            }
        }

        #if PLATFORM_LUMIN
        public MLMusicService.Metadata CurrentTrackMetadata { get; private set; } = default;

        private MLMusicService.Metadata previousTrackMetadata = default;
        public MLMusicService.Metadata PreviousTrackMetadata
        {
            get
            {
                return previousTrackMetadata;
            }
        }

        private MLMusicService.Metadata nextTrackMetadata = default;
        public MLMusicService.Metadata NextTrackMetadata
        {
            get
            {
                return nextTrackMetadata;
            }
        }
        #endif

        public delegate void PlaybackStateDelegate(MLMusicService.PlaybackStateType state);
        public event PlaybackStateDelegate OnPlaybackStateChanged = delegate { };

        public delegate void RepeatStateDelegate(MLMusicService.RepeatStateType state);
        public event RepeatStateDelegate OnRepeatStateChanged = delegate { };

        public delegate void ShuffleStateDelegate(MLMusicService.ShuffleStateType state);
        public event ShuffleStateDelegate OnShuffleStateChanged = delegate { };

        public delegate void PositionDelegate(int newPosition);
        public event PositionDelegate OnPositionChanged = delegate { };

        #if PLATFORM_LUMIN
        public delegate void MetadataDelegate(MLMusicService.Metadata metaData);
        public event MetadataDelegate OnMetadataChanged = delegate { };

        public delegate void ErrorDelegate(MLMusicService.Error error);
        public event ErrorDelegate OnError = delegate { };
        #endif

        public delegate void ServiceStatusDelegate(MLMusicService.Status state);
        public event ServiceStatusDelegate OnServiceStatusChanged = delegate { };

        #pragma warning disable 414
        private MLMusicService.PlaybackStateType playbackStateOnPause = MLMusicService.PlaybackStateType.Unknown;
        #pragma warning restore 414

        /// <summary>
        /// Locate tracks, connect provided music service provider, initialize MLMusicService API and setup callbacks.
        /// </summary>
        void Start()
        {
            string[] playlist = new string[StreamingPlaylist.Length];
            for (int i = 0; i < StreamingPlaylist.Length; ++i)
            {
                string streamingPath = Path.Combine(Path.Combine(Application.streamingAssetsPath, StreamingAssetsLocationFolder), StreamingPlaylist[i]);
                string persistentPath = Path.Combine(Application.persistentDataPath, StreamingPlaylist[i]);
                // The Music Provider will only search for songs correctly in /documents/C1 or /documents/C2 by their filename
                // This allows us to package the songs with the Unity application and deploy them from Streaming Assets.
                if (!File.Exists(persistentPath))
                {
                    File.Copy(streamingPath, persistentPath);
                }
                playlist[i] = "file://" + StreamingPlaylist[i];
            }

            #if PLATFORM_LUMIN
            MLResult result = MLMusicService.Start(MusicServiceProvider);
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLMusicServiceBehavior failed starting MLMusicService, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }

            MLMusicService.OnPlaybackStateChange += HandlePlaybackStateChanged;
            MLMusicService.OnShuffleStateChange += HandleShuffleStateChanged;
            MLMusicService.OnRepeatStateChange += HandleRepeatStateChanged;
            MLMusicService.OnMetadataChange += HandleMetadataChanged;
            MLMusicService.OnPositionChange += HandlePositionChanged;
            MLMusicService.OnError += HandleError;
            MLMusicService.OnStatusChange += HandleServiceStatusChanged;

            MLMusicService.RepeatState = MLMusicService.RepeatStateType.Off;
            MLMusicService.ShuffleState = MLMusicService.ShuffleStateType.Off;

            MLMusicService.SetPlayList(playlist);
            MLMusicService.StartPlayback();
            #endif
        }

        /// <summary>
        /// Cleanup the MLMusicService and unregister callbacks.
        /// </summary>
        void OnDestroy()
        {
            #if PLATFORM_LUMIN
            if (MLMusicService.IsStarted)
            {
                MLMusicService.StopPlayback();

                MLMusicService.OnPlaybackStateChange -= HandlePlaybackStateChanged;
                MLMusicService.OnShuffleStateChange -= HandleShuffleStateChanged;
                MLMusicService.OnRepeatStateChange -= HandleRepeatStateChanged;
                MLMusicService.OnMetadataChange -= HandleMetadataChanged;
                MLMusicService.OnPositionChange -= HandlePositionChanged;
                MLMusicService.OnError -= HandleError;
                MLMusicService.OnStatusChange -= HandleServiceStatusChanged;

                MLMusicService.Stop();
            }
            #endif
        }

        /// <summary>
        /// Stops the MLMusicService API.
        /// </summary>
        private void OnApplicationQuit()
        {
            #if PLATFORM_LUMIN
            if (MLMusicService.IsStarted)
            {
                MLMusicService.Stop();
            }
            #endif
        }

        /// <summary>
        /// Pauses or resumes playback based on if app is or was playing when paused.
        /// </summary>
        /// <param name="pause">Determines if it's a pause or a resume.</param>
        private void OnApplicationPause(bool pause)
        {
            #if PLATFORM_LUMIN
            if (MLMusicService.IsStarted)
            {
                if(pause)
                {
                    playbackStateOnPause = PlaybackState;
                }

                if (playbackStateOnPause == MLMusicService.PlaybackStateType.Playing)
                {
                    MLResult result = pause ? MLMusicService.PausePlayback() : MLMusicService.ResumePlayback();
                    if (!result.IsOk)
                    {
                        Debug.LogErrorFormat("MLMusicServiceBehavior failed to {0} the current track, disabling script. Reason: {1}.", pause ? "pause" : "resume", result);
                        enabled = false;
                        return;
                    }
                }
            }
            #endif
        }

        /// <summary>
        /// Seek to the specified position in the current track.
        /// </summary>
        /// <param name="targetMS">The target value to seek to.</param>
        public void Seek(uint targetMS)
        {
            #if PLATFORM_LUMIN
            MLResult result = MLMusicService.Seek(targetMS);
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLMusicServiceBehavior failed to seek in the current track, disabling script. Reason: {0}.", result);
                enabled = false;
                return;
            }
            #endif
        }

        /// <summary>
        /// Changes the track volume to the specified value
        /// </summary>
        /// <param name="volume">The target value to set the volume to.</param>
        public void SetVolume(float volume)
        {
            #if PLATFORM_LUMIN
            MLMusicService.Volume = volume;
            #endif
        }

        /// <summary>
        /// Pauses, resumes or starts playback based on playback state.
        /// </summary>
        public void PlayPause()
        {
            #if PLATFORM_LUMIN
            MLResult result;

            switch (MLMusicService.PlaybackState)
            {
                case MLMusicService.PlaybackStateType.Playing:
                    result = MLMusicService.PausePlayback();
                    if (!result.IsOk)
                    {
                        Debug.LogErrorFormat("MLMusicServiceBehavior failed to pause the current track, disabling script. Reason: {0}.", result);
                        enabled = false;
                        return;
                    }
                    break;
                case MLMusicService.PlaybackStateType.Paused:
                    result = MLMusicService.ResumePlayback();
                    if (!result.IsOk)
                    {
                        Debug.LogErrorFormat("MLMusicServiceBehavior failed to resume the current track, disabling script. Reason: {0}.", result);
                        enabled = false;
                        return;
                    }
                    break;
                case MLMusicService.PlaybackStateType.Stopped:
                    result = MLMusicService.StartPlayback();
                    if (!result.IsOk)
                    {
                        Debug.LogErrorFormat("MLMusicServiceBehavior failed to start the current track, disabling script. Reason: {0}.", result);
                        enabled = false;
                        return;
                    }
                    break;
                default:
                    break;
            }
            #endif
        }


        /// <summary>
        /// Changes the RepeatMode to the specified mode.
        /// </summary>
        public void ChangeRepeatState(MLMusicService.RepeatStateType state)
        {
            #if PLATFORM_LUMIN
            MLMusicService.RepeatState = state;
            #endif
        }

        /// <summary>
        /// Handle the next button being triggered
        /// </summary>
        public void Next()
        {
            #if PLATFORM_LUMIN
            MLResult result = MLMusicService.Next();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLMusicServiceBehavior failed to advance to the next track, disabling script. Reason: {0}.", result);
                enabled = false;
                return;
            }
            #endif
        }

        /// <summary>
        /// Handle the previous button being triggered
        /// </summary>
        public void Previous()
        {
            #if PLATFORM_LUMIN
            MLResult result = MLMusicService.Previous();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLMusicServiceBehavior failed to move to the previous track, disabling script. Reason: {0}.", result);
                enabled = false;
                return;
            }
            #endif
        }

        /// <summary>
        /// Handle the shuffon state button being triggered
        /// </summary>
        public void ToggleShuffle()
        {
            #if PLATFORM_LUMIN
            MLMusicService.ShuffleState = MLMusicService.ShuffleState == MLMusicService.ShuffleStateType.On ? MLMusicService.ShuffleStateType.Off : MLMusicService.ShuffleStateType.On;
            #endif
        }

        /// <summary>
        /// Popagate new playback state.
        /// </summary>
        /// <param name="state">The new state</param>
        void HandlePlaybackStateChanged(MLMusicService.PlaybackStateType state)
        {
            OnPlaybackStateChanged?.Invoke(state);
        }

        /// <summary>
        /// Popagate new repeat state.
        /// </summary>
        /// <param name="state">The new state</param>
        void HandleRepeatStateChanged(MLMusicService.RepeatStateType state)
        {
            OnRepeatStateChanged?.Invoke(state);
        }

        /// <summary>
        /// Popagate new suffle state.
        /// </summary>
        /// <param name="state">The new shuffle state</param>
        void HandleShuffleStateChanged(MLMusicService.ShuffleStateType state)
        {
            OnShuffleStateChanged?.Invoke(state);
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Event handler for the metadata of the track being changed
        /// </summary>
        /// <param name="metaData">Metadata of the track</param>
        void HandleMetadataChanged(MLMusicService.Metadata metaData)
        {
            CurrentTrackMetadata = metaData;
            MLResult result = MLMusicService.GetMetadata(ref previousTrackMetadata, -1);
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLMusicServiceBehavior failed to get the metadata of the previous track, disabling script. Reason: {0}.", result);
                enabled = false;
                return;
            }

            result = MLMusicService.GetMetadata(ref nextTrackMetadata, 1);
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLMusicServiceBehavior failed to get the metadata of the next track, disabling script. Reason: {0}.", result);
                enabled = false;
                return;
            }

            OnMetadataChanged?.Invoke(metaData);
        }
        #endif

        /// <summary>
        /// Event handler for the playback head position changing
        /// </summary>
        /// <param name="newPosition">The new position of the playback head</param>
        void HandlePositionChanged(int newPosition)
        {
            OnPositionChanged?.Invoke(newPosition);
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Event handler for when an error occurs in the music service
        /// </summary>
        /// <param name="error">Structure defining the error type and any extra error code</param>
        void HandleError(MLMusicService.Error error)
        {
            Debug.LogErrorFormat("MLMusicServiceBehavior failed with an error, type {0}, code {1}", error.Type, error.Code);
            OnError?.Invoke(error);
        }
        #endif

        /// <summary>
        /// Event handler for when the service status changes
        /// </summary>
        /// <param name="state">New status of the service</param>
        void HandleServiceStatusChanged(MLMusicService.Status state)
        {
            OnServiceStatusChanged?.Invoke(state);
        }
    }
}
