// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMediaPlayerEditor.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine.Video;

    /// <summary>
    /// Media player script that allows playback of a streaming video (either from file or web URL)
    /// This script will update the main texture parameter of the Renderer attached as a sibling
    /// with the video frame from playback. Audio is also handled through this class and will
    /// playback audio from the file.
    /// </summary>
    public sealed partial class MLMediaPlayer : MonoBehaviour
    {
        /// <summary>
        /// This class is the video player used in Editor. It implements the IMediaPlayer interface.
        /// This version of the media player does not support DRM videos.
        /// </summary>
        private class MLMediaPlayerEditor : MonoBehaviour, IMediaPlayer
        {
            /// <summary>
            /// The constant is used in second to millisecond conversion.
            /// </summary>
            private const int SecondsToMS = 1000;

            /// <summary>
            /// Stores a reference to the audio source.
            /// </summary>
            private AudioSource audioSource;

            /// <summary>
            /// Stores a reference to the Unity video player.
            /// </summary>
            private VideoPlayer videoPlayer;

            /// <summary>
            /// Action is invoked after video has started preparing.
            /// </summary>
            private MediaPlayerStartDelegate startPrepared;

            /// <summary>
            /// Action is invoked after seeking has been completed.
            /// </summary>
            private MediaPlayerEndDelegate seekCompleted = delegate { };

            /// <summary>
            /// Action is invoked after video has finished playback.
            /// </summary>
            private MediaPlayerEndDelegate videoEnded = delegate { };

            /// <summary>
            /// Stores the ID of the media player.
            /// </summary>
            private int mediaPlayerID;

            /// <summary>
            /// Texture used to render the video frame.
            /// </summary>
            private RenderTexture texture;

            /// <summary>
            /// Delegate used for various media player actions.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player</param>
            public delegate void MediaPlayerEndDelegate(object mediaPlayerID);

            /// <summary>
            /// Delegate used for various media player actions.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            public delegate MLResult MediaPlayerStartDelegate(object mediaPlayerID);

            /// <summary>
            /// Initialize the Media Player for the Editor
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <param name="videoEndedCallback">Callback when video ends</param>
            /// <param name="startPreparedCallback">Callback when media player is created</param>
            /// <param name="seekCompletedCallback">Callback when a seek completes</param>
            public void Initialize(int localMediaPlayerID, MediaPlayerEndDelegate videoEndedCallback, MediaPlayerStartDelegate startPreparedCallback, MediaPlayerEndDelegate seekCompletedCallback)
            {
                this.mediaPlayerID = localMediaPlayerID;
                this.startPrepared = startPreparedCallback;
                this.seekCompleted = seekCompletedCallback;
                this.videoEnded = videoEndedCallback;
            }

            /// <summary>
            /// Creates the streaming media player for the editor
            /// </summary>
            /// <param name="mediaPlayerGO">The media player game object</param>
            /// <param name="source">URL of the media</param>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c>
            /// </returns>
            public MLResult CreateStreamingMediaPlayer(GameObject mediaPlayerGO, string source, int localMediaPlayerID)
            {
                this.videoPlayer = mediaPlayerGO.AddComponent<VideoPlayer>();
                //// Add AudioSource
                this.audioSource = mediaPlayerGO.AddComponent<AudioSource>();

                // Create a Url with provided string and test if its a local file
                Uri uri;
                bool result = Uri.TryCreate(source, UriKind.Absolute, out uri);
                string path = result ? source : Path.Combine(Application.streamingAssetsPath, source);

                this.videoPlayer.url = path;

                this.videoPlayer.playOnAwake = true;
                this.videoPlayer.waitForFirstFrame = true;

                // Setup and attch audio source
                this.audioSource.playOnAwake = false;
                this.SetVolume(30, localMediaPlayerID);
                this.videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                this.videoPlayer.EnableAudioTrack(0, true);
                this.videoPlayer.SetTargetAudioSource(0, this.audioSource);

                this.videoPlayer.prepareCompleted += this.HandlePrepareCompleted;
                this.videoPlayer.seekCompleted += this.HandleSeekCompleted;
                this.videoPlayer.loopPointReached += this.HandleVideoEnded;

                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Creates the texture on the renderer to play the video on.
            /// </summary>
            /// <param name="renderer">Renderer of the object to play on.</param>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            /// <returns>True on success, false otherwise.</returns>
            public bool CreateTexture(Renderer renderer, int localMediaPlayerID)
            {
                if (this.texture == null)
                {
                    this.texture = new RenderTexture(1280, 720, 16, RenderTextureFormat.ARGB32);
                    this.videoPlayer.targetTexture = this.texture;
                    renderer.material.SetTexture("_MainTex", this.texture);
                }

                return true;
            }

            /// <summary>
            /// Initiate asynchronous reset of media player. Use <see cref="OnResetCompleted"/> event to know when reset completes,
            /// the player will be in a pre-prepared state. This method can be called anytime except while asynchronously preparing.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player.</param>
            /// <returns>
            /// MLResult.Result will be MLResult.Code.NotImplemented
            /// </returns>
            public MLResult ResetAsync(int localMediaPlayerID)
            {
                MLPluginLog.Error("MLMediaPlayer.ResetAsync is only available on device");
                return MLResult.Create(MLResult.Code.NotImplemented);
            }

            /// <summary>
            /// Plays the video in the editor.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c>
            /// </returns>
            public MLResult Play(int localMediaPlayerID)
            {
                this.videoPlayer.Play();
                this.audioSource.Play();
                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Pauses the video in the editor.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c>.
            /// </returns>
            public MLResult PauseVideo(int localMediaPlayerID)
            {
                this.videoPlayer.Pause();
                this.audioSource.Pause();
                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Seeks the specified time in the video in the editor
            /// </summary>
            /// <param name="positionMilliseconds">Absolute time to seek to</param>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c>
            /// </returns>
            public MLResult Seek(int positionMilliseconds, int localMediaPlayerID)
            {
                const float MSToSeconds = 0.001f;
                int seconds = (int)(positionMilliseconds * MSToSeconds);
                this.videoPlayer.time = seconds;
                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Sets the volume of the video in the editor
            /// </summary>
            /// <param name="vol">Volume to be set.</param>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c>
            /// </returns>
            public MLResult SetVolume(float vol, int localMediaPlayerID)
            {
                this.audioSource.volume = vol;
                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Stops the video in the editor
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c>
            /// </returns>
            public MLResult Stop(int localMediaPlayerID)
            {
                this.videoPlayer.Stop();
                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Resumes the video in the editor
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c>
            /// </returns>
            public MLResult Resume(int localMediaPlayerID)
            {
                this.videoPlayer.Play();
                this.audioSource.Play();
                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Sets the loop flag for the video in the editor
            /// </summary>
            /// <param name="loop">Flag to loop</param>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c>
            /// </returns>
            public MLResult SetLooping(bool loop, int localMediaPlayerID)
            {
                this.videoPlayer.isLooping = loop;
                this.audioSource.loop = loop;

                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Releases any resource used by this media player ID.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c>
            /// </returns>
            public MLResult Cleanup(int localMediaPlayerID)
            {
                UnityEngine.Object.Destroy(this.videoPlayer);
                UnityEngine.Object.Destroy(this.audioSource);
                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Selects the subtitle track, not available for editor.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <param name="trackID">(unused) track id to be selected</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.NotImplemented</c>
            /// </returns>
            public MLResult SelectSubtitleTrack(int localMediaPlayerID, uint trackID)
            {
                MLPluginLog.Warning("MLMediaPlayer.SelectSubtitleTrack is only available on device");
                return MLResult.Create(MLResult.Code.NotImplemented);
            }

            /// <summary>
            /// Unselects the subtitle track, not available for editor.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <param name="trackID">(unused) track id to be selected</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.NotImplemented</c>
            /// </returns>
            public MLResult UnselectSubtitleTrack(int localMediaPlayerID, uint trackID)
            {
                MLPluginLog.Warning("MLMediaPlayer.UnselectSubtitleTrack is only available on device");
                return MLResult.Create(MLResult.Code.NotImplemented);
            }

            /// <summary>
            /// Gets active audio channel count.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <param name="outAudioChannelCount">(unused) Return channel count.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.NotImplemented</c>
            /// </returns>
            public MLResult GetAudioChannelCount(int localMediaPlayerID, out int outAudioChannelCount)
            {
                outAudioChannelCount = 1;
                MLPluginLog.Warning("MLMediaPlayer.GetAudioChannelCount is only available on device");
                return MLResult.Create(MLResult.Code.NotImplemented);
            }

            /// <summary>
            /// Sets spatial audio state.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <param name="isEnabled">(unused) Desired state of spatial audio.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.NotImplemented</c>
            /// </returns>
            public MLResult SetSpatialAudio(int localMediaPlayerID, bool isEnabled)
            {
                MLPluginLog.Warning("MLMediaPlayer.SetSpatialAudioEnable is only available on device");
                return MLResult.Create(MLResult.Code.NotImplemented);
            }

            /// <summary>
            /// Gets spatial audio state.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <param name="outIsEnabled">(unused) Return state of spatial audio.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.NotImplemented</c>
            /// </returns>
            public MLResult GetSpatialAudio(int localMediaPlayerID, out bool outIsEnabled)
            {
                outIsEnabled = false;
                MLPluginLog.Warning("MLMediaPlayer.GetSpatialAudioEnable is only available on device");
                return MLResult.Create(MLResult.Code.NotImplemented);
            }

            /// <summary>
            /// Sets world position of requested audio channel.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <param name="channel">(unused) Selects the channel whose position is being set.</param>
            /// <param name="position">(unused) Set selected channel's world position</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.NotImplemented</c>
            /// </returns>
            public MLResult SetAudioChannelPosition(int localMediaPlayerID, MLMediaPlayer.AudioChannel channel, Vector3 position)
            {
                MLPluginLog.Warning("MLMediaPlayer.SetAudioChannelPosition is only available on device");
                return MLResult.Create(MLResult.Code.NotImplemented);
            }

            /// <summary>
            /// Gets world position of requested audio channel.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <param name="channel">(unused) Selects the channel whose position is being read.</param>
            /// <param name="position">(unused) Return selected channel's world position</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.NotImplemented</c>
            /// </returns>
            public MLResult GetAudioChannelPosition(int localMediaPlayerID, MLMediaPlayer.AudioChannel channel, out Vector3 position)
            {
                position = new Vector3(0f, 0f, 0f);
                MLPluginLog.Warning("MLMediaPlayer.GetAudioChannelPosition is only available on device");
                return MLResult.Create(MLResult.Code.NotImplemented);
            }

            /// <summary>
            /// Returns dictionary with information for all available tracks, not available for editor.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>
            /// Dictionary of track data for each track.
            /// </returns>
            public Dictionary<long, MLMediaPlayer.TrackData> GetAllTrackInfo(int localMediaPlayerID)
            {
            MLPluginLog.Warning("MLMediaPlayer.GetAllTrackInfo is only available on device");
                return new Dictionary<long, MLMediaPlayer.TrackData>();
            }

            /// <summary>
            /// Register a request to get the bytes used for a DRM key request.
            /// </summary>
            /// <param name="drmUUIDBytes">Bytes identifying the desired DRM type.</param>
            /// <param name="callback">Callback to be called when successfully retrieved request data.</param>
            /// <returns>
            /// True if request was successfully registered.
            /// </returns>
            public bool RequestActivationKeyRequest(byte[] drmUUIDBytes, Action<MLResult, byte[], string> callback)
            {
            MLPluginLog.Warning("MLMediaPlayerEditor.RequestActivationKeyRequest failed, editor version of MLMediaPlayer does not support DRM.");
                return false;
            }

            /// <summary>
            /// Get the resolution of the video
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>The resolution of the video</returns>
            public Rect GetResolution(int localMediaPlayerID)
            {
                return new Rect(0, 0, this.videoPlayer.targetTexture.width, this.videoPlayer.targetTexture.height);
            }

            /// <summary>
            /// Get the video track bitrate
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>The bitrate of the video track</returns>
            public int GetVideoBitrate(int localMediaPlayerID)
            {
            MLPluginLog.Warning("MLMediaPlayerEditor.GetVideoBitrate failed, editor version of MLMediaPlayer does not support bitrate.");
                return 0;
            }

            /// <summary>
            /// Gets the duration of the video in milliseconds.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>Duration of the video</returns>
            public int GetDurationMilliseconds(int localMediaPlayerID)
            {
                return (int)(this.videoPlayer.frameCount / this.videoPlayer.frameRate) * SecondsToMS;
            }

            /// <summary>
            /// Gets the current position of the video in milliseconds
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>Position of the playback of the video</returns>
            public int GetPositionMilliseconds(int localMediaPlayerID)
            {
                return (int)this.videoPlayer.time * SecondsToMS;
            }

            /// <summary>
            /// Get the width of the video in pixels
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>The width of the video</returns>
            public int GetWidth(int localMediaPlayerID)
            {
                return this.videoPlayer.targetTexture.width;
            }

            /// <summary>
            /// Get the height of the video in pixels
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <returns>The height of the video</returns>
            public int GetHeight(int localMediaPlayerID)
            {
                return this.videoPlayer.targetTexture.height;
            }

            /// <summary>
            /// Sets the license server for DRM videos (should not be called)
            /// </summary>
            /// <param name="licenseServer">(unused) URL of the License Server</param>
            public void SetLicenseServer(string licenseServer)
            {
                if (!string.IsNullOrEmpty(licenseServer))
                {
                MLPluginLog.Warning("MLMediaPlayerEditor.SetLicenseServer failed, editor version of MLMediaPlayer does not support DRM.");
                }
            }

            /// <summary>
            /// Set custom header key-value pairs to use in addition to default of <c>"User-Agent : Widevine CDM v1.0"</c>
            /// when performing key request to the DRM license server.
            /// </summary>
            /// <param name="headerData">(unused) Dictionary of custom header key-value pairs</param>
            public void SetCustomLicenseHeaderData(Dictionary<string, string> headerData)
            {
                if (headerData != null)
                {
                MLPluginLog.Warning("MLMediaPlayerEditor.SetCustomLicenseHeaderData failed, editor version of MLMediaPlayer does not support DRM.");
                }
            }

            /// <summary>
            /// Set custom key request key-value pair parameters used when generating default key request.
            /// </summary>
            /// <param name="messageData">(unused) Dictionary of optional key-value pair parameters</param>
            public void SetCustomLicenseMessageData(Dictionary<string, string> messageData)
            {
                if (messageData != null)
                {
                MLPluginLog.Warning("MLMediaPlayerEditor.SetCustomLicenseMessageData failed, editor version of MLMediaPlayer does not support DRM.");
                }
            }

            /// <summary>
            /// Sets the custom function to customize the license request.
            /// The default implementation, setting this to null, will simply use the default generated request.
            /// </summary>
            /// <param name="requestBuilder">
            /// (unused) Function to generate a license request body.
            /// First parameter is the DRM track being processed, either Video or Audio.
            /// Second parameter is the byte[] containing the default generated request.
            /// Return is the byte[] which will be used as the body to the license request.
            /// </param>
            public void SetCustomLicenseRequestBuilder(MLMediaPlayer.MediaPlayerCustomLicenseDelegate requestBuilder)
            {
                if (requestBuilder != null)
                {
                MLPluginLog.Warning("MLMediaPlayerEditor.SetCustomLicenseRequestBuilder failed, editor version of MLMediaPlayer does not support DRM.");
                }
            }

            /// <summary>
            /// Sets a custom function to custom parse the license response.
            /// The default implementation, setting this to null, will treat the entire response as the raw license data.
            /// </summary>
            /// <param name="responseParser">
            /// (unused) Function to parse license response.
            /// First parameter is the DRM track being processed, either Video or Audio.
            /// Second parameter is the byte[] containing the response we received from the license server.
            /// Return is the byte[] which will be the raw license data, base64 decoding if necessary.
            /// </param>
            public void SetCustomLicenseResponseParser(MLMediaPlayer.MediaPlayerCustomLicenseDelegate responseParser)
            {
                if (responseParser != null)
                {
                MLPluginLog.Warning("MLMediaPlayerEditor.SetCustomLicenseResponseParser failed, editor version of MLMediaPlayer does not support DRM.");
                }
            }

            /// <summary>
            /// Gets the frame drop threshold.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player.</param>
            /// <returns>The currently set millisecond threshold.</returns>
            public ulong GetFrameDropThresholdMs(int localMediaPlayerID)
            {
                MLPluginLog.Warning("MLMediaPlayerEditor.GetFrameDropThresholdMs is only available on device.");
                return long.MaxValue;
            }

            /// <summary>
            /// Sets a threshold to drop video frames if they are older than specified value.
            /// Setting this to 0 will not drop any frames, this is the default behavior.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player.</param>
            /// <param name="threshold">(unused) New threshold in milliseconds.</param>
            public void SetFrameDropThresholdMs(int localMediaPlayerID, ulong threshold)
            {
                MLPluginLog.Warning("MLMediaPlayerEditor.SetFrameDropThresholdMs is only available on device.");
            }

            /// <summary>
            /// Sets sharing information for the media player being shared and enables only functionality
            /// for synchronize the content playback. Follower setting can only be set before video has been prepared.
            /// </summary>
            /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
            /// <param name="sharedType">(unused) The shared type for the current media player from enum SharedType.</param>
            /// <param name="sessionID">(unused) Unique Identifier of the sharing session in which the media players are being shared.</param>
            /// <param name="isPrepared">(unused) Indicates if the media player has been prepared.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.NotImplemented</c>
            /// </returns>
            public MLResult SetSharingInfo(int localMediaPlayerID, MLMediaPlayer.SharedType sharedType, string sessionID, bool isPrepared)
            {
                MLPluginLog.Warning("MLMediaPlayerEditor.SetSharingInfo is only available on device.");
                return MLResult.Create(MLResult.Code.NotImplemented);
            }

            /// <summary>
            /// Set the unique id on this player.
            /// This function needs to be called before the media player is prepared.
            /// The id should be unique across all media player sessions that are being shared.
            /// Once the id is set, then it can not be changed.
            /// Prepare will give an error if the ID is set and not unique.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <param name="ID">Unique ID for this player.</param>
            public void SetID(int localMediaPlayerID, int ID)
            {
                MLPluginLog.Warning("MLMediaPlayerEditor.SetID is only available on device.");
            }

            /// <summary>
            /// Called after video has been prepared.
            /// </summary>
            /// <param name="vp">reference of the Video Player.</param>
            private void HandlePrepareCompleted(VideoPlayer vp)
            {
                if (this.startPrepared != null)
                {
                    this.startPrepared(this.mediaPlayerID);
                }
            }

            /// <summary>
            /// Called after video playback has ended.
            /// </summary>
            /// <param name="vp">reference of the Video Player</param>
            private void HandleVideoEnded(VideoPlayer vp)
            {
                if (this.videoEnded != null)
                {
                    this.videoEnded(this.mediaPlayerID);
                }
            }

            /// <summary>
            /// Called after video completed seeking to the desired position.
            /// </summary>
            /// <param name="vp">reference of the Video Player</param>
            private void HandleSeekCompleted(VideoPlayer vp)
            {
                if (this.seekCompleted != null)
                {
                    this.seekCompleted(this.mediaPlayerID);
                }
            }

            /// <summary>
            /// OnDestroy callback is used to unsubscribe to all the events.
            /// </summary>
            private void OnDestroy()
            {
                if (this.videoPlayer)
                {
                    this.videoPlayer.prepareCompleted -= this.HandlePrepareCompleted;
                    this.videoPlayer.seekCompleted -= this.HandleSeekCompleted;
                    this.videoPlayer.loopPointReached -= this.HandleVideoEnded;
                }
            }
        }
    }
}

#endif
