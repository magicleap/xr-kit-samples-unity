// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMediaplayerLumin.cs" company="Magic Leap, Inc">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using AOT;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// Media player script that allows playback of a streaming video (either from file or web URL)
    /// This script will update the main texture parameter of the Renderer attached as a sibling
    /// with the video frame from playback. Audio is also handled through this class and will
    /// playback audio from the file.
    /// </summary>
    public sealed partial class MLMediaPlayer
    {
        /// <summary>
        /// This class is the video player for the device. It implements the IMediaPlayer interface
        /// </summary>
        private class MLMediaPlayerLumin : MonoBehaviour, IMediaPlayer
        {
            // These match MLMediaPlayerEventCallbacks in ml_media_player.h
            // They are to be used with our EventManager system to trigger events originating from the MLMediaPlayer callback system in our native plugin
            // Usage: In any script that is interested in responding to these events, Create a new string with this prefix

            /// <summary>
            /// Prefix used to respond to OnVideoEnded event.
            /// </summary>
            public const string OnVideoEnded = "VideoEnded";

            /// <summary>
            /// Prefix used to respond to OnVideoPrepared event.
            /// </summary>
            public const string OnVideoPrepared = "VideoPrepared";

            /// <summary>
            /// Prefix used to respond to OnVideoBuffering event.
            /// </summary>
            public const string OnVideoBuffering = "VideoBuffering";

            /// <summary>
            /// Prefix used to respond to OnVideoError event.
            /// </summary>
            public const string OnVideoError = "VideoError";

            /// <summary>
            /// Prefix used to respond to OnVideoInfo event.
            /// </summary>
            public const string OnVideoInfo = "VideoInfo";

            /// <summary>
            /// Prefix used to respond to OnVideoSeekComplete event.
            /// </summary>
            public const string OnVideoSeekComplete = "VideoSeekComplete";

            /// <summary>
            /// Prefix used to respond to OnVideoSizeChanged event.
            /// </summary>
            public const string OnVideoSizeChanged = "VideoSizeChanged";

            /// <summary>
            /// Prefix used to respond to OnResetAsyncCompleted event.
            /// </summary>
            public const string OnResetAsyncCompleted = "ResetAsyncCompleted";

            /// <summary>
            /// Prefix used to respond to OnTrackDRMInfo event.
            /// </summary>
            public const string OnTrackDRMInfo = "TrackDRMInfo";

            /// <summary>
            /// Prefix used to respond to OnSubtitleTracksFound event.
            /// </summary>
            public const string OnSubtitleTracksFound = "VideoSubtitleTracksFound";

            /// <summary>
            /// Prefix used to respond to OnSubtitleInfoReceived event.
            /// </summary>
            public const string OnSubtitleInfoReceived = "VideoSubtitleInfoReceived";

            /// <summary>
            /// Prefix used to respond to OnSubtitle708EventReceived event.
            /// </summary>
            public const string OnSubtitle708EventReceived = "CEA708EventReceived";

            /// <summary>
            /// Prefix used to respond to OnTimedTextInfoReceived event.
            /// </summary>
            public const string OnTimedTextInfoReceived = "VideoTimedTextInfoReceived";

            /// <summary>
            /// Prefix used to respond to OnTrackInfoReceived event.
            /// </summary>
            public const string OnTrackInfoReceived = "TrackInfoReceived";

            /// <summary>
            /// The timeout duration for the http request.
            /// </summary>
            private const int HttpRequestTimeoutSeconds = 60;

            /// <summary>
            /// Maximum size for CEA708 Caption Emit command buffers.
            /// </summary>
            private const int MLCea708CaptionEmitCommandBufferMaxSize = 20;

            /// <summary>
            /// Static readonly instead of constant because I want to use reflection to know how many are in the enum.
            /// </summary>
            private static readonly int MLMediaDRMTrackCount = Enum.GetNames(typeof(MLMediaPlayer.DRMTrack)).Length;

            /// <summary>
            /// Mutual-exclusion lock for activation key request.
            /// </summary>
            private static readonly object RequestActivationKeyRequestLock = new object();

            /// <summary>
            /// Stores Information of all the Queued Callbacks.
            /// </summary>
            private static Dictionary<int, List<QueuedCallback>> queuedCallbacks = new Dictionary<int, List<QueuedCallback>>();

            /// <summary>
            /// Stores the ID of the media player.
            /// </summary>
            private int mediaPlayerID = -1;

            /// <summary>
            /// URL of the License Server.
            /// </summary>
            private string licenseServer = string.Empty;

            /// <summary>
            /// Session name for Media Player's sharing information.
            /// </summary>
            private string sessionID = string.Empty;

            /// <summary>
            /// Session name for Media Player's sharing information.
            /// </summary>
            private int uniqueID = -1;

            /// <summary>
            /// Session type for Media Player's sharing information.
            /// </summary>
            private MLMediaPlayer.SharedType sharedType = MLMediaPlayer.SharedType.None;

            /// <summary>
            /// Custom header key-value pairs to use in addition to default of <c>"User-Agent : Widevine CDM v1.0"</c>
            /// when performing key request to the DRM license server.
            /// </summary>
            private Dictionary<string, string> customLicenseHeaderData = null;

            /// <summary>
            /// Custom key request key-value pair parameters used when generating default key request.
            /// </summary>
            private Dictionary<string, string> customLicenseMessageData = null;

            /// <summary>
            /// Function pointer for license request.
            /// </summary>
            private MLMediaPlayer.MediaPlayerCustomLicenseDelegate customLicenseRequestBuilder = null;

            /// <summary>
            /// Function pointer for license response.
            /// </summary>
            private MLMediaPlayer.MediaPlayerCustomLicenseDelegate customLicenseResponseParser = null;

            /// <summary>
            /// Indicates the <c>DRM</c> key was MLResult.OK if the request was successful.
            /// </summary>
            private MLResult.Code[] getKeyResult = new MLResult.Code[MLMediaDRMTrackCount];

            /// <summary>
            /// Indicates the MLResult for GetProvisionRequest().
            /// </summary>
            private MLResult.Code provisionResult = MLResult.Code.Ok;

            /// <summary>
            /// Indicates if we need to re-start the render <c>coroutines</c>.
            /// </summary>
            private bool restoreRenderCoroutines = false;

            /// <summary>
            /// <c>Coroutine</c> for video playback.
            /// </summary>
            private IEnumerator playbackCoroutine = null;

            /// <summary>
            /// <c>Coroutine</c> used to wait until end of frame.
            /// </summary>
            private IEnumerator callPluginAtEndOfFramesCoroutine = null;

            /// <summary>
            /// <c>Coroutine</c> used for activating DRM key.
            /// </summary>
            private IEnumerator requestActivationKeyRequestCoroutine = null;

            /// <summary>
            /// A reference of the media player texture.
            /// </summary>
            private Texture2D textureReference = null;

            /// <summary>
            /// Last known size of the video.
            /// </summary>
            private Rect lastKnownResolution = new Rect();

            /// <summary>
            /// stores the TrackData for each media track.
            /// </summary>
            private Dictionary<long, MLMediaPlayer.TrackData> mediaTracks = new Dictionary<long, MLMediaPlayer.TrackData>();

            /// <summary>
            /// Notifies when a video has been prepared and is ready to begin playback
            /// </summary>
            private PreparedDelegate startPrepared;

            /// <summary>
            /// Delegate used by events having one payload and returns MLResult.
            /// </summary>
            /// <param name="obj">First payload</param>
            /// <returns>Should returns MLResult.OK if operation is successful.</returns>
            public delegate MLResult PreparedDelegate(object obj);

            /// <summary>
            /// Delegate used by events having one payload
            /// </summary>
            /// <param name="obj">ID of the media player</param>
            public delegate void VideoEndedDelegate(object obj);

            /// <summary>
            /// Delegate used by events events requiring 2 payloads.
            /// </summary>
            /// <param name="obj">First payload</param>
            /// <param name="pos">Second payload.</param>
            public delegate void VideoUpdateDelegate(object obj, object pos);

            /// <summary>
            /// Delegate used by events events requiring 3 payloads.
            /// </summary>
            /// <param name="obj">First payload.</param>
            /// <param name="result">Second payload.</param>
            /// <param name="info">Third payload.</param>
            public delegate void VideoInfoDelegate(object obj, object result, object info);

            /// <summary>
            /// Delegate used by events events requiring 3 payloads.
            /// </summary>
            /// <param name="obj">First payload.</param>
            /// <param name="data">Second payload.</param>
            /// <param name="startTime">Third payload.</param>
            /// <param name="duration">Fourth payload.</param>
            public delegate void TimedTextDelegate(object obj, object data, object startTime, object duration);

            /// <summary>
            /// Callback that notifies when the video has reached the end of the stream.
            /// </summary>
            private event VideoEndedDelegate VideoEnded;

            /// <summary>
            /// Invoked when a video is buffering, notifies the percentage of completeness of the buffering.
            /// </summary>
            private event VideoUpdateDelegate UpdateBufferingUI;

            /// <summary>
            /// Invoked when media player encounters an error.
            /// First parameter is the type of error as MLMediaPlayerError.
            /// Second parameter is extra information about the error as MLMediaError.
            /// </summary>
            private event VideoInfoDelegate OnError;

            /// <summary>
            /// Invoked when media player has informational events available
            /// First parameter is the type of information event as MLMediaPlayerInfo
            /// Second parameter is any extra info the informational event may define:
            /// When info is MLMediaPlayerInfo.NetworkBandwidth, this holds bandwidth in kbps.
            /// It is 0 for others.
            /// </summary>
            private event VideoInfoDelegate OnInfo;

            /// <summary>
            /// Invoked when a seek completes, notifies with the current position in the media as a percentage.
            /// </summary>
            private event VideoEndedDelegate OnSeekCompleted;

            /// <summary>
            /// Invoked when a video's resolution changes, notifies with the current resolution as a <c>Rect</c>.
            /// </summary>
            private event VideoUpdateDelegate VideoSizeChanged;

            /// <summary>
            /// Invoked when an asynchronous reset completes, player is ready to be prepared again.
            /// </summary>
            private event VideoEndedDelegate OnResetCompleted;

            /// <summary>
            /// Invoked when media player finds new closed caption track information.
            /// First parameter is a dictionary of TrackData objects.
            /// </summary>y>
            private event VideoUpdateDelegate GetSubtitleTracksCallback;

            /// <summary>
            /// Invoked when subtitle data is received from the media library.
            /// First parameter is a <c>MLCea608CaptionSegment</c> object that contains all the data given.
            /// </summary>
            private event VideoUpdateDelegate GetSubtitleDataCallback;

            /// <summary>
            /// Invoked when CEA708 subtitle event data is received from the media library.
            /// First parameter is a <c>MLCea608CaptionSegment</c> object that contains all the data given.
            /// </summary>
            private event VideoUpdateDelegate GetSubtitle708EventCallback;

            /// <summary>
            /// Invoked when timed text data is received from media library, time is based on media time.
            /// First parameter is the textual data for this update.
            /// Second parameter is time in milliseconds this text should be displayed.
            /// Third parameter is time in milliseconds this text should stop being displayed.
            /// </summary>
            private event TimedTextDelegate GetTimedTextDataCallback;

            // These functions below are just wrapping the C functions
            // These are public but they're supposed to be used only by MLMediaPlayer

            /// <summary>
            /// Initiate asynchronous reset of media player. Use <see cref="OnResetCompleted"/> event to know when reset completes,
            /// the player will be in a pre-prepared state. This method can be called anytime except while asynchronously preparing.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if successful.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to an internal error prevented MediaPlayer from resetting.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if failed due to media player not properly initialized.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed due to calling from the wrong state.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if internally passed media player handle was not found.
            /// </returns>
            public MLResult ResetAsync(int localMediaPlayerID)
            {
                MLResult result;
                if (Application.isEditor)
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMediaPlayer can not be used in Editor or ML Remote");
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.ResetAsync failed to initiate asynchronous player reset for media player {0}. Reason: {1}", localMediaPlayerID, result);
                    return result;
                }

                MLResult.Code resultCode = NativeBindings.MediaPlayerResetAsync(localMediaPlayerID);

                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.ResetAsync failed to initiate asynchronous player reset for media player {0}. Reason: {1}", localMediaPlayerID, result);
                    return result;
                }

                return result;
            }

            /// <summary>
            /// Plays the video
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if media player was not properly built or initialized.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if method was called from the wrong state.
            /// </returns>
            public MLResult Play(int localMediaPlayerID)
            {
                MLResult result;
                if (Application.isEditor)
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMediaPlayer can not be used in Editor or ML Remote");
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.Play failed to begin playing media player {0}. Reason: {1}", localMediaPlayerID, result);
                    return result;
                }

                MLResult.Code resultCode = NativeBindings.Play(localMediaPlayerID);
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.Play failed to begin playing media player {0}. Reason: {1}", localMediaPlayerID, result);
                }

                return result;
            }

            /// <summary>
            /// Pauses the video
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if media player was not properly built or initialized.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if method was called from the wrong state.
            /// </returns>
            public MLResult PauseVideo(int localMediaPlayerID)
            {
                MLResult result;
                if (Application.isEditor)
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMediaPlayer can not be used in Editor or ML Remote");
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.PauseVideo failed to pause media player {0}. Reason: {1}", localMediaPlayerID, result);
                    return result;
                }

                MLResult.Code resultCode = NativeBindings.PauseVideo(localMediaPlayerID);
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.PauseVideo failed to pause media player {0}. Reason: {1}", localMediaPlayerID, result);
                }

                return result;
            }

            /// <summary>
            /// Seeks the specified time in the video
            /// </summary>
            /// <param name="positionMilliseconds">Absolute time to seek to</param>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if media player was not properly built or initialized.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if method was called from the wrong state.
            /// </returns>
            public MLResult Seek(int positionMilliseconds, int localMediaPlayerID)
            {
                MLResult result;
                if (Application.isEditor)
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMediaPlayer can not be used in Editor or ML Remote");
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.Seek failed to seek media player {0}. Reason: {1}", localMediaPlayerID, result);
                    return result;
                }

                MLResult.Code resultCode = NativeBindings.Seek(localMediaPlayerID, positionMilliseconds);
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.Seek failed to seek media player {0}. Reason: {1}", localMediaPlayerID, result);
                }

                return result;
            }

            /// <summary>
            /// Sets the volume of the video.
            /// </summary>
            /// <param name="vol">Volume to be set.</param>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if media player was not properly built or initialized.
            /// </returns>
            public MLResult SetVolume(float vol, int localMediaPlayerID)
            {
                MLResult result;
                if (Application.isEditor)
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMediaPlayer can not be used in Editor or ML Remote");
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.SetVolume failed to begin set volume for media player {0}. Reason: {0}", result);
                    return result;
                }

                MLResult.Code resultCode = NativeBindings.SetVolume(localMediaPlayerID, vol);
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.SetVolume failed to begin set volume for media player {0}. Reason: {1}", localMediaPlayerID, result);
                }

                return result;
            }

            /// <summary>
            /// Stops the video in the editor
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if media player was not properly built or initialized.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if method was called from the wrong state.
            /// </returns>
            public MLResult Stop(int localMediaPlayerID)
            {
                MLResult result;
                if (Application.isEditor)
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMediaPlayer can not be used in Editor or ML Remote");
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.Stop failed to stop playing media player {0}. Reason: {1}", localMediaPlayerID, result);
                    return result;
                }

                MLResult.Code resultCode = NativeBindings.Stop(localMediaPlayerID);
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.Stop failed to stop playing media player {0}. Reason: {1}", localMediaPlayerID, result);
                }

                return result;
            }

            /// <summary>
            /// Resumes the video
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if media player was not properly built or initialized.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if method was called from the wrong state.
            /// </returns>
            public MLResult Resume(int localMediaPlayerID)
            {
                MLResult result;
                if (Application.isEditor)
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMediaPlayer can not be used in Editor or ML Remote");
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.Resume failed to resume playing media player {0}. Reason: {1}", localMediaPlayerID, result);
                    return result;
                }

                MLResult.Code resultCode = NativeBindings.Resume(localMediaPlayerID);
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.Resume failed to resume playing media player {0}. Reason: {1}", localMediaPlayerID, result);
                }

                return result;
            }

            /// <summary>
            /// Sets the loop flag for the video
            /// </summary>
            /// <param name="loop">Flag to loop</param>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if media player was not properly built or initialized.
            /// </returns>
            public MLResult SetLooping(bool loop, int localMediaPlayerID)
            {
                MLResult result;
                if (Application.isEditor)
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMediaPlayer can not be used in Editor or ML Remote");
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.SetLooping failed to set looping for media player {0}. Reason: {1}", localMediaPlayerID, result);
                    return result;
                }

                MLResult.Code resultCode = NativeBindings.SetLooping(localMediaPlayerID, loop);
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.SetLooping failed to set looping for media player {0}. Reason: {1}", localMediaPlayerID, result);
                }

                return result;
            }

            /// <summary>
            /// Releases any resource used by this media player ID.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if media player was not properly built or initialized.
            /// </returns>
            public MLResult Cleanup(int localMediaPlayerID)
            {
                if (this.callPluginAtEndOfFramesCoroutine != null)
                {
                    this.StopCoroutine(this.callPluginAtEndOfFramesCoroutine);
                    this.callPluginAtEndOfFramesCoroutine = null;
                }

                if (this.playbackCoroutine != null)
                {
                    this.StopCoroutine(this.playbackCoroutine);
                    this.playbackCoroutine = null;
                }

                MLResult result;
                if (Application.isEditor)
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMediaPlayer can not be used in Editor or ML Remote");
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.Cleanup failed to clean up media player {0}. Reason: {1}", localMediaPlayerID, result);
                    return result;
                }

                MLResult.Code resultCode = NativeBindings.Cleanup(localMediaPlayerID);
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.Cleanup failed to clean up media player {0}. Reason: {1}", localMediaPlayerID, result);
                }

                if (this.textureReference != null)
                {
                    GetComponent<MeshRenderer>().material.SetTexture("_MainTex", null);
                    UnityEngine.Object.Destroy(this.textureReference);
                    this.textureReference = null;
                }

                queuedCallbacks.Remove(localMediaPlayerID);

                // This special sauce is needed to release GL objects on render thread.
                if (!Application.isEditor)
                {
                    GL.IssuePluginEvent(NativeBindings.GetRenderCleanupCallback(), localMediaPlayerID);
                }

                return result;
            }

            /// <summary>
            /// Register a request to get the bytes used for a DRM key request for <c>Lumin</c>.
            /// </summary>
            /// <param name="drmUUIDBytes">Bytes identifying the desired DRM type.</param>
            /// <param name="callback">Callback to be called when successfully retrieved request data.</param>
            /// <returns>
            /// True if request was successfully registered.
            /// </returns>
            public bool RequestActivationKeyRequest(byte[] drmUUIDBytes, Action<MLResult, byte[], string> callback)
            {
                lock (RequestActivationKeyRequestLock)
                {
                    if (callback == null || this.requestActivationKeyRequestCoroutine != null)
                    {
                        return false;
                    }

                    this.requestActivationKeyRequestCoroutine = this.RequestActivationKeyRequestCoroutine(drmUUIDBytes, callback);
                    this.StartCoroutine(this.requestActivationKeyRequestCoroutine);

                    return true;
                }
            }

            /// <summary>
            /// Gets the duration of the video in milliseconds
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            /// <returns>
            /// Duration of the video, -1 on failure.
            /// </returns>
            public int GetDurationMilliseconds(int localMediaPlayerID)
            {
                if (Application.isEditor)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetDurationMilliseconds failed to get duration of media player {0}. Reason: MLMediaPlayer can not be used in Editor or ML Remote.", localMediaPlayerID);
                    return -1;
                }

                int duration = 0;
                MLResult.Code resultCode = NativeBindings.GetDurationMilliseconds(localMediaPlayerID, out duration);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetDurationMilliseconds failed to get duration of media player {0}. Result Code: {1}", localMediaPlayerID, resultCode);
                    return -1;
                }

                return duration;
            }

            /// <summary>
            /// Gets the current position of the video in milliseconds.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// Position of the playback of the video, -1 on failure.
            /// </returns>
            public int GetPositionMilliseconds(int localMediaPlayerID)
            {
                if (Application.isEditor)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetPositionMilliseconds failed to begin get position of media player {0}. Reason: MLMediaPlayer can not be used in Editor or ML Remote", localMediaPlayerID);
                    return -1;
                }

                int position = 0;
                MLResult.Code resultCode = NativeBindings.GetPositionMilliseconds(localMediaPlayerID, out position);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetPositionMilliseconds failed to begin get position of media player {0}. Result Code: {1}", localMediaPlayerID, resultCode);
                    return -1;
                }

                return position;
            }

            /// <summary>
            /// Get the width of the video in pixels
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// The width of the video, -1 on failure.
            /// </returns>
            public int GetWidth(int localMediaPlayerID)
            {
                if (Application.isEditor)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetWidth failed to get width of media player {0}. Reason: MLMediaPlayer can not be used in Editor or ML Remote", localMediaPlayerID);
                    return -1;
                }

                int width = 0;
                MLResult.Code resultCode = NativeBindings.GetWidth(localMediaPlayerID, out width);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetWidth failed to get width of media player {0}. Result Code: {1}", localMediaPlayerID, resultCode);
                    return -1;
                }

                return width;
            }

            /// <summary>
            /// Get the height of the video in pixels
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// The height of the video, -1 on failure.
            /// </returns>
            public int GetHeight(int localMediaPlayerID)
            {
                if (Application.isEditor)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetHeight failed to get height media player {0}. Reason: MLMediaPlayer can not be used in Editor or ML Remote", localMediaPlayerID);
                    return -1;
                }

                int height = 0;
                MLResult.Code resultCode = NativeBindings.GetHeight(localMediaPlayerID, out height);
                MLResult result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetHeight failed to get height media player {0}. Reason: {1}", localMediaPlayerID, result);
                    return -1;
                }

                return height;
            }

            /// <summary>
            /// Get the last known resolution of the video in pixels
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// The <c>Rect</c> representing the video resolution.
            /// </returns>
            public Rect GetResolution(int localMediaPlayerID)
            {
                return this.lastKnownResolution;
            }

            /// <summary>
            /// Get the bitrate of the video kbps
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// The bitrate of the video, -1 on failure.
            /// </returns>
            public int GetVideoBitrate(int localMediaPlayerID)
            {
                int bitrate = 0;
                MLResult.Code resultCode = NativeBindings.GetVideoBitrate(localMediaPlayerID, out bitrate);
                MLResult result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetVideoBitrate failed to get video track bitrate media player {0}. Reason: {1}", localMediaPlayerID, result);
                    return -1;
                }

                return bitrate;
            }

            /// <summary>
            /// Sets the license server for DRM videos
            /// </summary>
            /// <param name="licenseServer">URL of the License Server</param>
            public void SetLicenseServer(string licenseServer)
            {
                this.licenseServer = licenseServer;
            }

            /// <summary>
            /// Set custom header key-value pairs to use in addition to default of <c>"User-Agent : Widevine CDM v1.0"</c>
            /// when performing key request to the DRM license server.
            /// </summary>
            /// <param name="headerData">Dictionary of custom header key-value pairs</param>
            public void SetCustomLicenseHeaderData(Dictionary<string, string> headerData)
            {
                this.customLicenseHeaderData = headerData;
            }

            /// <summary>
            /// Set custom key request key-value pair parameters used when generating default key request.
            /// </summary>
            /// <param name="messageData">Dictionary of optional key-value pair parameters</param>
            public void SetCustomLicenseMessageData(Dictionary<string, string> messageData)
            {
                this.customLicenseMessageData = messageData;
            }

            /// <summary>
            /// Sets the custom function to customize the license request.
            /// The default implementation, setting this to null, will simply use the default generated request.
            /// </summary>
            /// <param name="requestBuilder">
            /// Function to generate a license request body.
            /// First parameter is the DRM track being processed, either Video or Audio.
            /// Second parameter is the byte[] containing the default generated request.
            /// Return is the byte[] which will be used as the body to the license request.
            /// </param>
            public void SetCustomLicenseRequestBuilder(MLMediaPlayer.MediaPlayerCustomLicenseDelegate requestBuilder)
            {
                this.customLicenseRequestBuilder = requestBuilder;
            }

            /// <summary>
            /// Sets a custom function to custom parse the license response.
            /// The default implementation, setting this to null, will treat the entire response as the raw license data.
            /// </summary>
            /// <param name="responseParser">
            /// Function to parse license response.
            /// First parameter is the DRM track being processed, either Video or Audio.
            /// Second parameter is the byte[] containing the response we received from the license server.
            /// Return is the byte[] which will be the raw license data, base64 decoding if necessary.
            /// </param>
            public void SetCustomLicenseResponseParser(MLMediaPlayer.MediaPlayerCustomLicenseDelegate responseParser)
            {
                this.customLicenseResponseParser = responseParser;
            }

            /// <summary>
            /// Gets the frame drop threshold.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>The currently set millisecond threshold.</returns>
            public ulong GetFrameDropThresholdMs(int localMediaPlayerID)
            {
                return NativeBindings.GetFrameDropThresholdMs(localMediaPlayerID);
            }

            /// <summary>
            /// Sets a threshold to drop video frames if they are older than specified value.
            /// Setting this to 0 will not drop any frames, this is the default behavior.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <param name="threshold">New threshold in milliseconds</param>
            public void SetFrameDropThresholdMs(int localMediaPlayerID, ulong threshold)
            {
                NativeBindings.SetFrameDropThresholdMs(localMediaPlayerID, threshold);
            }

            /// <summary>
            /// Select specific subtitle or timed text track in the media for <c>Lumin</c>..
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            /// <param name="id">The id of the track to be selected.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            public MLResult SelectSubtitleTrack(int localMediaPlayerID, uint id)
            {
                MLResult.Code resultCode = NativeBindings.SelectSubtitleTrack(localMediaPlayerID, id);
                return MLResult.Create(resultCode);
            }

            /// <summary>
            /// Unselect specific subtitle or timed text track in the media for <c>Lumin</c>.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            /// <param name="id">The id of the track to be unselected.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            public MLResult UnselectSubtitleTrack(int localMediaPlayerID, uint id)
            {
                MLResult.Code resultCode = NativeBindings.UnselectSubtitleTrack(localMediaPlayerID, id);
                return MLResult.Create(resultCode);
            }

            /// <summary>
            /// Gets active audio channel count.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <param name="outAudioChannelCount">Return channel count.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> Method was called from the wrong state. Can only be called after one of the setDataSource methods.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.NotImplemented</c> No audio track found.
            /// </returns>
            public MLResult GetAudioChannelCount(int localMediaPlayerID, out int outAudioChannelCount)
            {
                outAudioChannelCount = 1;
                MLResult.Code resultCode = NativeBindings.GetAudioChannelCount(localMediaPlayerID, out outAudioChannelCount);
                return MLResult.Create(resultCode);
            }

            /// <summary>
            /// Sets spatial audio state.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <param name="isEnabled">Desired state of spatial audio.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioHandleNotFound</c> Audio Handle not found.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioInternalConfigError</c> Internal config error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioNotImplemented</c> Internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AllocFailed</c> No valid media player handle.
            /// </returns>
            public MLResult SetSpatialAudio(int localMediaPlayerID, bool isEnabled)
            {
                MLResult.Code resultCode = NativeBindings.SetSpatialAudioEnable(localMediaPlayerID, isEnabled);
                return MLResult.Create(resultCode);
            }

            /// <summary>
            /// Gets spatial audio state.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <param name="outIsEnabled">Return state of spatial audio.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioHandleNotFound</c> Audio Handle not found.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioInternalConfigError</c> Internal config error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioNotImplemented</c> Internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AllocFailed</c> No valid media player handle.
            /// </returns>
            public MLResult GetSpatialAudio(int localMediaPlayerID, out bool outIsEnabled)
            {
                outIsEnabled = false;
                MLResult.Code resultCode = NativeBindings.GetSpatialAudioEnable(localMediaPlayerID, out outIsEnabled);
                return MLResult.Create(resultCode);
            }

            /// <summary>
            /// Sets world position of requested audio channel.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <param name="channel">Selects the channel whose position is being set.</param>
            /// <param name="position">Set selected channel's world position</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> If input parameter is invalid.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> Internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioHandleNotFound</c> Handle not found.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioInternalConfigError</c> Internal config error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioNotImplemented</c> Internal error.
            /// </returns>
            public MLResult SetAudioChannelPosition(int localMediaPlayerID, MLMediaPlayer.AudioChannel channel, Vector3 position)
            {
                Native.MagicLeapNativeBindings.MLVec3f nativePosition = Native.MLConvert.FromUnity(position);
                MLResult.Code resultCode = NativeBindings.SetAudioChannelPosition(localMediaPlayerID, (uint)channel, nativePosition);
                return MLResult.Create(resultCode);
            }

            /// <summary>
            /// Gets world position of requested audio channel.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <param name="channel">Selects the channel whose position is being read.</param>
            /// <param name="outPosition">Return selected channel's world position</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> If input parameter is invalid.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> Internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioHandleNotFound</c> Handle not found.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioInternalConfigError</c> Internal config error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.AudioNotImplemented</c> Internal error.
            /// </returns>
            public MLResult GetAudioChannelPosition(int localMediaPlayerID, MLMediaPlayer.AudioChannel channel, out Vector3 outPosition)
            {
                Native.MagicLeapNativeBindings.MLVec3f nativePosition = new Native.MagicLeapNativeBindings.MLVec3f();
                MLResult.Code resultCode = NativeBindings.GetAudioChannelPosition(localMediaPlayerID, (uint)channel, out nativePosition);
                outPosition = Native.MLConvert.ToUnity(nativePosition);
                return MLResult.Create(resultCode);
            }

            /// <summary>
            /// Function is used to get information of tracks available on the media.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            /// <returns>
            /// Dictionary of track data for each track.
            /// </returns>
            public Dictionary<long, MLMediaPlayer.TrackData> GetAllTrackInfo(int localMediaPlayerID)
            {
                return this.mediaTracks;
            }

            /// <summary>
            /// Gets the Render callback on the graphics thread, which is used to render the video.
            /// </summary>
            /// <returns>
            /// Pointer to the render callback.
            /// </returns>
            public IntPtr GetRenderCallback()
            {
                if (Application.isEditor)
                {
                    MLPluginLog.Error("MLMediaPlayerLumin.GetRenderCallback failed to get render callback. Reason: MLMediaPlayer can not be used in Editor or ML Remote");
                    return IntPtr.Zero;
                }

                return NativeBindings.GetRenderCallback();
            }

            /// <summary>
            /// Creates the streaming media player
            /// </summary>
            /// <param name="mediaPlayerGO">The media player game object</param>
            /// <param name="source">URL of the media</param>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if media player was not properly built or initialized.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.PrivilegeDenied</c> if attempting to access web content without appropriate network privileges
            /// </returns>
            public MLResult CreateStreamingMediaPlayer(GameObject mediaPlayerGO, string source, int localMediaPlayerID)
            {
                MLResult result;
                if (Application.isEditor)
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLMediaPlayer can not be used in Editor or ML Remote");
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.CreateStreamingMediaPlayer, MLMediaPlayer can not be used in Editor or ML Remote. Reason: {0}", result);
                    return result;
                }

                MLPluginLog.DebugFormat("[{0}], CreateStreamingMediaPlayer - Begin InstantiateMediaPlayerContext", Time.realtimeSinceStartup);

                // Create a Url with provided string and test if its a local file
                Uri uri;
                bool resultURICreate = Uri.TryCreate(source, UriKind.Absolute, out uri);
                string mediaSource = resultURICreate ? source : (Application.dataPath + "/StreamingAssets/" + source).Replace("\\", "/");
                MLPluginLog.DebugFormat("CreateStreamingMediaPlayer Asset: {0}", mediaSource);

                MLMediaPlayer.ColorSpace colorSpace = MLMediaPlayer.ColorSpace.Linear;
                if (QualitySettings.activeColorSpace == UnityEngine.ColorSpace.Linear)
                {
                    colorSpace = MLMediaPlayer.ColorSpace.Linear;
                }
                else if (QualitySettings.activeColorSpace == UnityEngine.ColorSpace.Gamma)
                {
                    colorSpace = MLMediaPlayer.ColorSpace.Gamma;
                }

                NativeBindings.MediaPlayerManagedCallbacks managedCallbacks = new NativeBindings.MediaPlayerManagedCallbacks
                {
                    OnPrepared = new NativeBindings.PlayerPreparedCallback(PlayerPreparedHandler),
                    OnCompletion = new NativeBindings.PlayerCompletionCallback(PlayerCompletionHandler),
                    OnBuffering = new NativeBindings.PlayerBufferingCallback(PlayerBufferingHandler),
                    OnError = new NativeBindings.PlayerErrorCallback(PlayerErrorHandler),
                    OnInfo = new NativeBindings.PlayerInfoCallback(PlayerInfoHandler),
                    OnSeekCompleted = new NativeBindings.PlayerSeekCompletedCallback(PlayerSeekCompletedHandler),
                    OnVideoSizeChanged = new NativeBindings.PlayerVideoSizeChangedCallback(PlayerVideoSizeChangedHandler),
                    OnResetCompleted = new NativeBindings.PlayerResetAsyncCompletedCallback(PlayerResetAsyncCompletedHandler),
                    OnTrackDRMInfo = new NativeBindings.PlayerTrackDRMInfoCallback(PlayerTrackDRMInfoHandler),
                    OnAllTracks = new NativeBindings.PlayerAllTracksCallback(PlayerAllTracksHandler),
                    OnSubtitleTracks = new NativeBindings.PlayerSubtitleTracksCallback(PlayerSubtitleTracksHandler),
                    OnSubtitleInfo = new NativeBindings.PlayerSubtitle608InfoCallback(PlayerSubtitle608InfoHandler),
                    OnSubtitle708Event = new NativeBindings.PlayerSubtitle708EventCallback(PlayerSubtitle708EventHandler),
                    OnTimedTextInfo = new NativeBindings.PlayerTimedTextInfoCallback(PlayerTimedTextInfoHandler)
                };

                MLResult.Code resultCode = NativeBindings.InstantiateMediaPlayerContext(localMediaPlayerID, mediaSource, colorSpace, managedCallbacks, this.sharedType, this.sessionID, this.uniqueID);

                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.CreateStreamingMediaPlayer failed to prepare streaming media player with asset at {0}. Result: {1}", mediaSource, result);
                    return result;
                }

                MLPluginLog.DebugFormat("[{0}], CreateStreamingMediaPlayer - End InstantiateMediaPlayerContext", Time.realtimeSinceStartup);
                return result;
            }

            /// <summary>
            /// Creates the texture on the renderer to play the video on.
            /// </summary>
            /// <param name="renderer">Renderer of the object to play on.</param>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            /// <returns>
            /// True on success, false otherwise.
            /// </returns>
            public bool CreateTexture(Renderer renderer, int localMediaPlayerID)
            {
                if (Application.isEditor)
                {
                    MLPluginLog.Error("MLMediaPlayerLumin.CreateTexture, MLMediaPlayer can not be used in Editor or ML Remote");
                    return false;
                }

                int width = this.GetWidth(localMediaPlayerID);
                int height = this.GetHeight(localMediaPlayerID);

                return this.CreateTexture(renderer, localMediaPlayerID, width, height);
            }

            /// <summary>
            /// Initialize the Media Player for the Editor
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <param name="videoEndedCallback">Callback when video ends</param>
            /// <param name="startPreparedCallback">Callback when video is prepared</param>
            /// <param name="updateBufferingUICallback">Callback when video buffering is updated</param>
            /// <param name="onErrorCallback">Callback when an error occurs</param>
            /// <param name="onInfoCallback">Callback when an info event occurs</param>
            /// <param name="onSeekCompleted">Callback when a seek completes</param>
            /// <param name="onVideoSizeChanged">Callback when the video resolution changes</param>
            /// <param name="onResetCompleted">Callback when reset completes</param>
            /// <param name="getSubtitleTracksCallback">Callback when the subtitle tracks are found</param>
            /// <param name="getSubtitleDataCallback">Callback when the subtitle data is received</param>
            /// <param name="getSubtitle708EventCallback">Callback when the subtitle CEA708 event is received</param>
            /// <param name="getTimedTextDataCallback">Callback when the timed text data is received</param>
            public void Initialize(
                int localMediaPlayerID,
                VideoEndedDelegate videoEndedCallback,
                PreparedDelegate startPreparedCallback,
                VideoUpdateDelegate updateBufferingUICallback,
                VideoInfoDelegate onErrorCallback,
                VideoInfoDelegate onInfoCallback,
                VideoEndedDelegate onSeekCompleted,
                VideoUpdateDelegate onVideoSizeChanged,
                VideoEndedDelegate onResetCompleted,
                VideoUpdateDelegate getSubtitleTracksCallback,
                VideoUpdateDelegate getSubtitleDataCallback,
                VideoUpdateDelegate getSubtitle708EventCallback,
                TimedTextDelegate getTimedTextDataCallback)
            {
                this.mediaPlayerID = localMediaPlayerID;
                this.VideoEnded = videoEndedCallback;
                this.startPrepared = startPreparedCallback;
                this.UpdateBufferingUI = updateBufferingUICallback;
                this.OnError = onErrorCallback;
                this.OnInfo = onInfoCallback;
                this.OnSeekCompleted = onSeekCompleted;
                this.VideoSizeChanged = onVideoSizeChanged;
                this.OnResetCompleted = onResetCompleted;
                this.GetSubtitleTracksCallback = getSubtitleTracksCallback;
                this.GetSubtitleDataCallback = getSubtitleDataCallback;
                this.GetSubtitle708EventCallback = getSubtitle708EventCallback;
                this.GetTimedTextDataCallback = getTimedTextDataCallback;
            }

            /// <summary>
            /// Sets sharing information for the media player being shared and enables only functionality
            /// for synchronize the content playback. Follower setting can only be set before video has been prepared.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <param name="sharedType">The shared type for the current media player from enum SharedType.</param>
            /// <param name="sessionID">Unique Identifier of the sharing session in which the media players are being shared.</param>
            /// <param name="isPrepared">Indicates if the media player has been prepared.</param>
            /// <returns>
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> for invalid parameters.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> shared information is successfully set.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> The operation failed with an unspecified error.
            /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
            /// </returns>
            public MLResult SetSharingInfo(int localMediaPlayerID, MLMediaPlayer.SharedType sharedType, string sessionID, bool isPrepared)
            {
                MLResult result = MLResult.Create(MLResult.Code.Ok);
                this.sharedType = sharedType;
                this.sessionID = sessionID;
                if ((this.sharedType == MLMediaPlayer.SharedType.Initiator || this.sharedType == MLMediaPlayer.SharedType.None) && isPrepared)
                {
                    result = MLResult.Create(NativeBindings.SetSharingInfo(localMediaPlayerID, this.sharedType, this.sessionID));
                }

                return result;
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
                this.uniqueID = ID;
            }


            /// <summary>
            /// This callback function is invoked when the player has finished preparing
            /// media and is ready to playback.
            /// </summary>
            /// <param name="localMediaPlayerID">Id of the media player</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerPreparedCallback))]
            private static void PlayerPreparedHandler(int localMediaPlayerID)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerPreparedHandler player prepared callback fired for media player {0}", localMediaPlayerID);
                QueueCallback(OnVideoPrepared, localMediaPlayerID);
            }

            /// <summary>
            /// This callback function is invoked when media player played back until end of
            /// media and has now come to a stop.
            /// Note that this callback does not fire when 'looping = true',
            /// because MediaPlayer does not "stop" in that case, but rather
            /// loops to beginning of media.
            /// </summary>
            /// <param name="localMediaPlayerID">Id of the Media Player</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerCompletionCallback))]
            private static void PlayerCompletionHandler(int localMediaPlayerID)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerCompletionHandler player complete callback fired for media player {0}", localMediaPlayerID);
                QueueCallback(OnVideoEnded, localMediaPlayerID);
            }

            /// <summary>
            /// This callback function is invoked when media player is buffering.
            /// </summary>
            /// <param name="percent">Completed percentage</param>
            /// <param name="localMediaPlayerID">Id of the Media Player</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerBufferingCallback))]
            private static void PlayerBufferingHandler(int percent, int localMediaPlayerID)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerBufferingHandler player buffering update callback fired for media player {0} with buffering percent: {1}", localMediaPlayerID, percent);
                QueueCallback(OnVideoBuffering, localMediaPlayerID, new List<object> { percent });
            }

            /// <summary>
            /// This callback function is invoked when media player encounters an error.
            /// </summary>
            /// <param name="result"> result error/result code indicating failure reason.</param>
            /// <param name="error">Error data</param>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerErrorCallback))]
            private static void PlayerErrorHandler(int result, IntPtr error, int localMediaPlayerID)
            {
                string errorString = Marshal.PtrToStringAuto(error);
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerErrorHandler for media player {0} with MLResult.Code {1} and Error String: {2}", localMediaPlayerID, (MLResult.Code)result, errorString);
                QueueCallback(OnVideoError, localMediaPlayerID, new List<object> { result, errorString });
            }

            /// <summary>
            /// This callback function is invoked when \ref MediaPlayer generates informational events.
            /// </summary>
            /// <param name="info">Info type of informational event</param>
            /// <param name="extra">extra MLMediaPlayerInfo type specific extra information.
            /// When info is MLMediaPlayerInfo_NetworkBandwidth, this holds bandwidth
            /// in kbps.It is 0 for others.</param>
            /// <param name="localMediaPlayerID">Id of the media player.</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerInfoCallback))]
            private static void PlayerInfoHandler(int info, int extra, int localMediaPlayerID)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerInfoHandler for media player {0} with info enum: {1}, extra: {2}", localMediaPlayerID, (MLMediaPlayer.PlayerInfo)info, extra);
                QueueCallback(OnVideoInfo, localMediaPlayerID, new List<object> { info, extra });
            }

            /// <summary>
            /// This callback function is invoked when the internal surface has changed size.
            /// </summary>
            /// <param name="width">New width of the video.</param>
            /// <param name="height">New height of the video.</param>
            /// <param name="localMediaPlayerID">Id of the media player.</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerVideoSizeChangedCallback))]
            private static void PlayerVideoSizeChangedHandler(int width, int height, int localMediaPlayerID)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerVideoSizeChangedHandler for media player {0} with width: {1}, height: {2}", localMediaPlayerID, width, height);
                QueueCallback(OnVideoSizeChanged, localMediaPlayerID, new List<object> { width, height });
            }

            /// <summary>
            /// Callback signature called when asynchronous reset completes.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerResetAsyncCompletedCallback))]
            private static void PlayerResetAsyncCompletedHandler(int localMediaPlayerID)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerResetAsyncCompletedHandler player asynchronous reset completed callback fired for media player {0}", localMediaPlayerID);
                QueueCallback(OnResetAsyncCompleted, localMediaPlayerID);
            }

            /// <summary>
            /// This callback function is invoked when source has DRM protected media track(s).
            /// </summary>
            /// <param name="drmTrackIndex">Track id for the DRM</param>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerTrackDRMInfoCallback))]
            private static void PlayerTrackDRMInfoHandler(uint drmTrackIndex, int localMediaPlayerID)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerTrackDRMInfoHandler for media player {0} with track: {1}", localMediaPlayerID, drmTrackIndex);
                QueueCallback(OnTrackDRMInfo, localMediaPlayerID, new List<object> { (MLMediaPlayer.DRMTrack)drmTrackIndex });
            }

            /// <summary>
            /// This callback function is invoked when a seek operation has completed.
            /// </summary>
            /// <param name="localMediaPlayerID">The Id of the media player.</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerSeekCompletedCallback))]
            private static void PlayerSeekCompletedHandler(int localMediaPlayerID)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerSeekCompletedHandler for media player {0}", localMediaPlayerID);
                QueueCallback(OnVideoSeekComplete, localMediaPlayerID);
            }

            /// <summary>
            /// This callback function is invoked when a new subtitle track is found
            /// </summary>
            /// <param name="localMediaPlayerID">Id of theMedia Player.</param>
            /// <param name="tracks">The list of subtitle tracks.</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerSubtitleTracksCallback))]
            private static void PlayerSubtitleTracksHandler(int localMediaPlayerID, MLMediaPlayer.MediaPlayerTracks tracks)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerSubtitleTracksHandler for media player {0}", localMediaPlayerID);
                Dictionary<long, MLMediaPlayer.TrackData> subtitleTracks = new Dictionary<long, MLMediaPlayer.TrackData>();

                long tracksArrayAddress = tracks.Tracks.ToInt64();
                for (var i = 0; i < tracks.Length; ++i)
                {
                    long entryAddress = tracksArrayAddress +
                                        (i * Marshal.SizeOf(typeof(MLMediaPlayer.TrackData)));
                    var entryPtr = new IntPtr(entryAddress);
                    MLMediaPlayer.TrackData nativeEntry = (MLMediaPlayer.TrackData)Marshal.PtrToStructure(entryPtr, typeof(MLMediaPlayer.TrackData));
                    subtitleTracks.Add(nativeEntry.ID, nativeEntry);
                }

                QueueCallback(OnSubtitleTracksFound, localMediaPlayerID, new List<object> { subtitleTracks });
            }

            /// <summary>
            /// This callback function is invoked when a new track is found.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            /// <param name="tracks">return all the tracks currently present.</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerAllTracksCallback))]
            private static void PlayerAllTracksHandler(int localMediaPlayerID, MLMediaPlayer.MediaPlayerTracks tracks)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerAllTracksHandler for media player {0}", localMediaPlayerID);
                Dictionary<long, MLMediaPlayer.TrackData> allTracks = new Dictionary<long, MLMediaPlayer.TrackData>();

                long tracksArrayAddress = tracks.Tracks.ToInt64();
                for (var i = 0; i < tracks.Length; ++i)
                {
                    long entryAddress = tracksArrayAddress +
                                        (i * Marshal.SizeOf(typeof(MLMediaPlayer.TrackData)));
                    var entryPtr = new IntPtr(entryAddress);
                MLMediaPlayer.TrackData nativeEntry = (MLMediaPlayer.TrackData)Marshal.PtrToStructure(entryPtr, typeof(MLMediaPlayer.TrackData));
                    allTracks.Add(nativeEntry.ID, nativeEntry);
                }

                QueueCallback(OnTrackInfoReceived, localMediaPlayerID, new List<object> { allTracks });
            }

            /// <summary>
            /// This callback function is invoked when new 608 data is ready.
            /// </summary>
            /// <param name="localMediaPlayerID">Id of the media player</param>
            /// <param name="closedCaptionSegInt">Reference to CEA608 closed captioned segment structure.</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerSubtitle608InfoCallback))]
            private static void PlayerSubtitle608InfoHandler(int localMediaPlayerID, ref MLMediaPlayer.Cea608CaptionSegmentInternal closedCaptionSegInt)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerSubtitle608InfoHandler for media player {0} with version {1}", localMediaPlayerID, closedCaptionSegInt.Version);

                MLMediaPlayer.Cea608CaptionSegment closedCaptionSeg = new MLMediaPlayer.Cea608CaptionSegment();
                closedCaptionSeg.CCLines = new MLMediaPlayer.Cea608CaptionLine[(int)MLMediaPlayer.Cea608CaptionDimension.MaxRowsPlus2];
                for (var i = 0; i < (int)MLMediaPlayer.Cea608CaptionDimension.MaxRowsPlus2; ++i)
                {
                    if (closedCaptionSegInt.CCLines[i] != IntPtr.Zero)
                    {
                        MLMediaPlayer.Cea608CaptionLine newCCLine = new MLMediaPlayer.Cea608CaptionLine();
                        MLMediaPlayer.Cea608CaptionLineExInternal internalCCLine = (MLMediaPlayer.Cea608CaptionLineExInternal)Marshal.PtrToStructure(closedCaptionSegInt.CCLines[i], typeof(MLMediaPlayer.Cea608CaptionLineExInternal));
                        newCCLine.DisplayString = new string(Array.ConvertAll(internalCCLine.DisplayChars, Convert.ToChar));
                        newCCLine.MidRowStyles = new MLMediaPlayer.Cea608CaptionStyleColor[(int)MLMediaPlayer.Cea608CaptionDimension.MaxColsPlus2];
                        newCCLine.PacStyles = new MLMediaPlayer.Cea608CaptionPAC[(int)MLMediaPlayer.Cea608CaptionDimension.MaxColsPlus2];

                        for (var j = 0; j < (int)MLMediaPlayer.Cea608CaptionDimension.MaxColsPlus2; ++j)
                        {
                            if (internalCCLine.MidRowStyles[j] != IntPtr.Zero)
                            {
                                MLMediaPlayer.Cea608CaptionStyleColor internalCCstyle = (MLMediaPlayer.Cea608CaptionStyleColor)Marshal.PtrToStructure(internalCCLine.MidRowStyles[j], typeof(MLMediaPlayer.Cea608CaptionStyleColor));
                                newCCLine.MidRowStyles[j] = internalCCstyle;
                            }
                            else
                            {
                                newCCLine.MidRowStyles[j] = null;
                            }

                            if (internalCCLine.PacStyles[j] != IntPtr.Zero)
                            {
                                MLMediaPlayer.Cea608CaptionPAC internalCCPAC = (MLMediaPlayer.Cea608CaptionPAC)Marshal.PtrToStructure(internalCCLine.PacStyles[j], typeof(MLMediaPlayer.Cea608CaptionPAC));
                                newCCLine.PacStyles[j] = internalCCPAC;
                            }
                            else
                            {
                                newCCLine.PacStyles[j] = null;
                            }
                        }

                        closedCaptionSeg.CCLines[i] = newCCLine;
                    }
                    else
                    {
                        closedCaptionSeg.CCLines[i] = null;
                    }
                }

                QueueCallback(OnSubtitleInfoReceived, localMediaPlayerID, new List<object> { closedCaptionSeg });
            }

            /// <summary>
            /// This callback function is invoked when a new 708 event is ready.
            /// </summary>
            /// <param name="mediaPlayerID">Id of the media player</param>
            /// <param name="cea708EventInternal">Reference to CEA708 closed captioned event structure.</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerSubtitle708EventCallback))]
            private static void PlayerSubtitle708EventHandler(int mediaPlayerID, ref MLMediaPlayer.Cea708CaptionEventInternal cea708EventInternal)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerSubtitle708EventHandler for mediaPlayerID: {0} with version {1}", mediaPlayerID, cea708EventInternal.Type);
                MLMediaPlayer.Cea708CaptionEvent cea708Event;
                cea708Event.Type = cea708EventInternal.Type;
                cea708Event.Object = null;
                switch (cea708EventInternal.Type)
                {
                    case MLMediaPlayer.Cea708CaptionEmitCommand.Buffer:
                        cea708Event.Object = Native.MLConvert.DecodeUTF8(cea708EventInternal.Object, MLCea708CaptionEmitCommandBufferMaxSize);
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.Control:
                        cea708Event.Object = Marshal.ReadByte(cea708EventInternal.Object);
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.CWX:
                        cea708Event.Object = Marshal.ReadInt32(cea708EventInternal.Object);
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.CLW:
                        cea708Event.Object = Marshal.ReadInt32(cea708EventInternal.Object);
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.DSW:
                        cea708Event.Object = Marshal.ReadInt32(cea708EventInternal.Object);
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.HDW:
                        cea708Event.Object = Marshal.ReadInt32(cea708EventInternal.Object);
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.TGW:
                        cea708Event.Object = Marshal.ReadInt32(cea708EventInternal.Object);
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.DLW:
                        cea708Event.Object = Marshal.ReadInt32(cea708EventInternal.Object);
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.DLY:
                        cea708Event.Object = Marshal.ReadInt32(cea708EventInternal.Object);
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.DLC:
                        cea708Event.Object = null;
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.RST:
                        cea708Event.Object = null;
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.SPA:
                        cea708Event.Object = Marshal.PtrToStructure(cea708EventInternal.Object, typeof(MLMediaPlayer.Cea708CaptionPenAttr));
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.SPC:
                        cea708Event.Object = Marshal.PtrToStructure(cea708EventInternal.Object, typeof(MLMediaPlayer.Cea708CaptionPenColor));
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.SPL:
                        cea708Event.Object = Marshal.PtrToStructure(cea708EventInternal.Object, typeof(MLMediaPlayer.Cea708CaptionPenLocation));
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.SWA:
                        cea708Event.Object = Marshal.PtrToStructure(cea708EventInternal.Object, typeof(MLMediaPlayer.Cea708CaptionWindowAttr));
                        break;
                    case MLMediaPlayer.Cea708CaptionEmitCommand.DFX:
                        cea708Event.Object = Marshal.PtrToStructure(cea708EventInternal.Object, typeof(MLMediaPlayer.Cea708CaptionWindow));
                        break;
                }

                QueueCallback(OnSubtitle708EventReceived, mediaPlayerID, new List<object> { cea708Event });
            }

            /// <summary>
            /// Callback signature called when Timed Text update is available.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player</param>
            /// <param name="utf8ByteArray">An array containing the text as UTF8.</param>
            /// <param name="startTimeMs">Start time in milliseconds.</param>
            /// <param name="endTimeMs">End time in milliseconds</param>
            [MonoPInvokeCallback(typeof(NativeBindings.PlayerTimedTextInfoCallback))]
            private static void PlayerTimedTextInfoHandler(int localMediaPlayerID, System.IntPtr utf8ByteArray, long startTimeMs, long endTimeMs)
            {
                MLPluginLog.DebugFormat("MLMediaPlayerLumin.PlayerTimedTextInfoHandler for media player {0}", localMediaPlayerID);

                List<byte> byteList = new List<byte>();
                int byteListLength = 0;
                while (true)
                {
                    byte byteValue = Marshal.ReadByte(utf8ByteArray, byteListLength);
                    if (byteValue == 0)
                    {
                        break;
                    }

                    byteListLength++;
                    byteList.Add(byteValue);
                }

                string text = System.Text.Encoding.UTF8.GetString(byteList.ToArray(), 0, byteListLength);
                QueueCallback(OnTimedTextInfoReceived, localMediaPlayerID, new List<object> { text, startTimeMs, endTimeMs });
            }

            /// <summary>
            /// Queues the string  for all the callbacks.
            /// </summary>
            /// <param name="callbackPrefix">Name of the Callback.</param>
            /// <param name="localMediaPlayerID">Id of the media player.</param>
            /// <param name="parameters">List of all payloads.</param>
            private static void QueueCallback(string callbackPrefix, int localMediaPlayerID, List<object> parameters = null)
            {
                if (!queuedCallbacks.ContainsKey(localMediaPlayerID))
                {
                    queuedCallbacks.Add(localMediaPlayerID, new List<QueuedCallback>());
                }

                QueuedCallback qc = new QueuedCallback(callbackPrefix, parameters);
                lock (queuedCallbacks[localMediaPlayerID])
                {
                    queuedCallbacks[localMediaPlayerID].Add(qc);
                    MLPluginLog.DebugFormat("MLMediaPlayerLumin.QueueCallback, queued new callback of prefix {0} to media player {1}, total callbacks now {2}", callbackPrefix, localMediaPlayerID, queuedCallbacks.Count);
                }
            }

            /// <summary>
            ///  Create a activation key instance from a UUID.
            /// </summary>
            /// <param name="drmUUIDBytes">The universal unique ID of the crypto scheme. UUID must be 16 bytes.</param>
            /// <param name="callback">Callback to be called when successfully retrieved request data.</param>
            /// <returns>
            /// Waits for ProvisionDeviceForDRM().
            /// </returns>
            private IEnumerator RequestActivationKeyRequestCoroutine(byte[] drmUUIDBytes, Action<MLResult, byte[], string> callback)
            {
                if (drmUUIDBytes.Length != 16)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.RequestActivationKeyRequestCoroutine invalid DRM UUID, must be 16 bytes.");
                    callback(MLResult.Create(MLResult.Code.InvalidParam), null, null);
                    yield break;
                }

                IntPtr requestMessagePtr = IntPtr.Zero;
                ulong requestMessageSize = 0;

                Native.MagicLeapNativeBindings.MLUUIDBytes drm_uuid;
                drm_uuid.Data = drmUUIDBytes;

                MLResult callbackResult = MLResult.Create(MLResult.Code.UnspecifiedFailure);
                byte[] callbackRrequestData = null;
                string callbackDefaultLicenseAddress = string.Empty;

                const int MaxTries = 3;
                for (int tries = 0; tries < MaxTries; tries++)
                {
                    MLResult.Code resultCode = NativeBindings.GetActivationKeyRequest(
                        ref drm_uuid,
                        MLMediaPlayer.DRMKeyType.Streaming,
                        ref requestMessagePtr,
                        ref requestMessageSize,
                        ref callbackDefaultLicenseAddress);
                    if (MLResult.Code.MediaDRMNotProvisioned == resultCode)
                    {
                        yield return this.StartCoroutine(this.ProvisionDeviceForDRM(-1));
                        if (this.provisionResult != MLResult.Code.Ok)
                        {
                            MLPluginLog.ErrorFormat("MLMediaPlayerLumin.RequestActivationKeyRequestCoroutine failed to provision device. Reason: {0}", this.provisionResult);

                            callbackResult = MLResult.Create(this.provisionResult);
                            break;
                        }
                    }
                    else if (MLResult.Code.MediaDRMResourceBusy == resultCode)
                    {
                        MLPluginLog.WarningFormat("MLMediaPlayerLumin.RequestActivationKeyRequestCoroutine trying again, resource busy");
                        yield return new WaitForSeconds(3);
                    }
                    else if (MLResult.Code.Ok == resultCode)
                    {
                        MLPluginLog.ErrorFormat("MLMediaPlayerLumin.RequestActivationKeyRequestCoroutine Result OK, copying data {0}[{1}]", requestMessagePtr, requestMessageSize);
                        callbackRrequestData = new byte[requestMessageSize];
                        Marshal.Copy(requestMessagePtr, callbackRrequestData, 0, callbackRrequestData.Length);

                        callbackResult = MLResult.Create(MLResult.Code.Ok);
                        break;
                    }
                    else
                    {
                        MLPluginLog.ErrorFormat("MLMediaPlayerLumin.RequestActivationKeyRequestCoroutine failed to get activation request. Reason: {0}", resultCode);

                        callbackResult = MLResult.Create(resultCode);
                        break;
                    }
                }

                callback(callbackResult, callbackRrequestData, callbackDefaultLicenseAddress);
                lock (RequestActivationKeyRequestLock)
                {
                    this.requestActivationKeyRequestCoroutine = null;
                }
            }

            /// <summary>
            /// <c>Coroutine</c> to ben media playback.
            /// </summary>
            /// <returns>
            /// On end of Frame.
            /// </returns>
            private IEnumerator BeginVideo()
            {
                // Use Unity Native Plugin rendering
                this.callPluginAtEndOfFramesCoroutine = this.CallPluginAtEndOfFrames();
                yield return this.StartCoroutine(this.callPluginAtEndOfFramesCoroutine);
            }

            /// <summary>
            /// The <c>Coroutine</c> is used to get DRM provision request for media player, it wait until the web request has posted a response.
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the Media player</param>
            /// <returns>
            /// End of Web request.
            /// </returns>
            private IEnumerator ProvisionDeviceForDRM(int localMediaPlayerID)
            {
                this.provisionResult = MLResult.Code.Ok;

                IntPtr requestMessagePtr = IntPtr.Zero;
                ulong requestMessageSize = 0;
                string defaultLicenseAddress = string.Empty;
                if (localMediaPlayerID == -1)
                {
                    this.provisionResult = NativeBindings.GetActivationProvisionRequest(
                        "none",
                    ref requestMessagePtr,
                    ref requestMessageSize,
                    ref defaultLicenseAddress);
                }
                else
                {
                    this.provisionResult = NativeBindings.GetProvisionRequest(
                        localMediaPlayerID,
                        "none",
                        ref requestMessagePtr,
                        ref requestMessageSize,
                        ref defaultLicenseAddress);
                }

                if (this.provisionResult != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.ProvisionDeviceForDRM failed to get DRM provision request for media player {0}. Reason: {1}", localMediaPlayerID, this.provisionResult);
                    yield break;
                }

                byte[] requestMessageData = new byte[requestMessageSize];
                Marshal.Copy(requestMessagePtr, requestMessageData, 0, requestMessageData.Length);

                if (string.IsNullOrEmpty(defaultLicenseAddress))
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.ProvisionDeviceForDRM failed, no license server specified!");
                    this.provisionResult = MLResult.Code.InvalidParam;
                    yield break;
                }

                string provisionPostUrl = defaultLicenseAddress + "&signedRequest=" + System.Text.Encoding.UTF8.GetString(requestMessageData, 0, requestMessageData.Length);
                UnityWebRequest provisionRequest = new UnityWebRequest(provisionPostUrl);

                provisionRequest.method = UnityWebRequest.kHttpVerbPOST;
                provisionRequest.downloadHandler = new DownloadHandlerBuffer();
                provisionRequest.useHttpContinue = false;
                //// Defaults to no timeout, best to specify one.
                provisionRequest.timeout = HttpRequestTimeoutSeconds;

                provisionRequest.SetRequestHeader("Accept", "*/*");
                provisionRequest.SetRequestHeader("Content-Type", "application/json");
                provisionRequest.SetRequestHeader("User-Agent", "Widevine CDM v1.0");

                // Perform request and get response.
                yield return provisionRequest.SendWebRequest();

                if (provisionRequest.isHttpError || provisionRequest.isNetworkError)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.ProvisionDeviceForDRM failed to send web request. Reason: {0}", provisionRequest.error);
                    this.provisionResult = MLResult.Code.UnspecifiedFailure;
                    yield break;
                }
                else
                {
                    if (!provisionRequest.downloadHandler.isDone)
                    {
                        MLPluginLog.ErrorFormat("MLMediaPlayerLumin.ProvisionDeviceForDRM failed, download handler not done!");
                    }
                    else
                    {
                        MLPluginLog.DebugFormat("MLMediaPlayerLumin.ProvisionDeviceForDRM download handler read {0} bytes!", provisionRequest.downloadedBytes);
                    }

                    byte[] response = provisionRequest.downloadHandler.data;

                    if (localMediaPlayerID == -1)
                    {
                        this.provisionResult = NativeBindings.ProvideActivationProvisionResponse(response, (ulong)response.Length);
                    }
                    else
                    {
                        this.provisionResult = NativeBindings.ProvideProvisionResponse(localMediaPlayerID, response, (ulong)response.Length);
                    }

                    if (this.provisionResult != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLMediaPlayerLumin.ProvisionDeviceForDRM failed to set provision response for media player {0}. Reason: {1}", localMediaPlayerID, this.provisionResult);
                        yield break;
                    }
                }
            }

            /// <summary>
            /// Prepares the License header for the DRM request.
            /// </summary>
            /// <param name="localMediaPlayerID">Id of the media player.</param>
            /// <param name="drmTrackEnum">Indicates the for the type of media (audio or video).</param>
            /// <returns>
            /// Waits until Web request is complete.
            /// </returns>
            private IEnumerator GetKeysAndPrepareDRM(int localMediaPlayerID, MLMediaPlayer.DRMTrack drmTrackEnum)
            {
                int drmTrack = (int)drmTrackEnum;
                this.getKeyResult[drmTrack] = MLResult.Code.Ok;

                IntPtr requestMessagePtr = IntPtr.Zero;
                ulong requestMessageSize = 0;
                string defaultLicenseAddress = string.Empty;

                // If we have any, pass custom license message data before GetKeyRequest.
                if (this.customLicenseMessageData != null && this.customLicenseMessageData.Count > 0)
                {
                    // Convert Dictionary to something C friendly.
                    string[] keys = new string[this.customLicenseMessageData.Count];
                    string[] values = new string[this.customLicenseMessageData.Count];
                    ulong counter = 0;
                    foreach (KeyValuePair<string, string> pair in this.customLicenseMessageData)
                    {
                        keys[counter] = pair.Key;
                        values[counter] = pair.Value;
                        counter++;
                    }

                    // This function will copy the strings, so no need to keep them around.
                    NativeBindings.SetKeyRequestCustomData(localMediaPlayerID, drmTrackEnum, keys, values, counter);
                }

                this.getKeyResult[drmTrack] = NativeBindings.GetKeyRequest(
                                            localMediaPlayerID,
                                            drmTrackEnum,
                                            MLMediaPlayer.DRMKeyType.Streaming,
                                            ref requestMessagePtr,
                                            ref requestMessageSize,
                                            ref defaultLicenseAddress);
                if (this.getKeyResult[drmTrack] != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat(
                        "MLMediaPlayerLumin.GetKeysAndPrepareDRM failed to get {0} DRM Provision request for media player {1}. Reason: {2}",
                        drmTrackEnum.ToString(),
                        localMediaPlayerID,
                        this.getKeyResult[drmTrack]);
                    yield break;
                }

                byte[] requestMessageData = new byte[requestMessageSize];
                Marshal.Copy(requestMessagePtr, requestMessageData, 0, requestMessageData.Length);

                // If we have a builder callback, use it and make use of what is returned as the new request data.
                if (this.customLicenseRequestBuilder != null)
                {
                    requestMessageData = this.customLicenseRequestBuilder(drmTrackEnum, requestMessageData);
                }

                string licenseServer = string.IsNullOrEmpty(this.licenseServer) ? defaultLicenseAddress : this.licenseServer;
                if (string.IsNullOrEmpty(licenseServer))
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetKeysAndPrepareDRM failed, no license server specified!");
                    this.getKeyResult[drmTrack] = MLResult.Code.InvalidParam;
                    yield break;
                }

                UploadHandlerRaw keyRequestData = new UploadHandlerRaw(requestMessageData);
                UnityWebRequest keyRequest = new UnityWebRequest(licenseServer);

                keyRequest.method = UnityWebRequest.kHttpVerbPOST;
                keyRequest.uploadHandler = keyRequestData;
                keyRequest.downloadHandler = new DownloadHandlerBuffer();
                keyRequest.useHttpContinue = false;
                //// Defaults to no timeout, best to specify one.
                keyRequest.timeout = HttpRequestTimeoutSeconds;

                keyRequest.SetRequestHeader("User-Agent", "Widevine CDM v1.0");
                //// As Unity documentation indicates:
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                // "Connection" cannot be set.
                ////keyRequest.SetRequestHeader("Connection", "close");

                this.AddCustomLicenseHeaders(keyRequest);

                // Perform request and get response.
                yield return keyRequest.SendWebRequest();

                if (keyRequest.isHttpError || keyRequest.isNetworkError)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetKeysAndPrepareDRM failed to send {0} DRM web request. Reason: {1}", drmTrackEnum.ToString(), keyRequest.error);
                    this.getKeyResult[drmTrack] = MLResult.Code.UnspecifiedFailure;
                    yield break;
                }
                else
                {
                    if (!keyRequest.downloadHandler.isDone)
                    {
                        MLPluginLog.ErrorFormat("MLMediaPlayerLumin.GetKeysAndPrepareDRM failed, download handler not done.");
                    }
                    else
                    {
                        MLPluginLog.DebugFormat("MLMediaPlayerLumin.GetKeysAndPrepareDRM download handler read {0} bytes!", keyRequest.downloadedBytes);
                    }

                    byte[] response = this.ProcessLicenseResponse(drmTrackEnum, keyRequest.downloadHandler.data);

                    this.getKeyResult[drmTrack] = NativeBindings.ProvideKeyResponse(localMediaPlayerID, drmTrackEnum, response, (ulong)response.Length);
                    if (this.getKeyResult[drmTrack] != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat(
                            "MLMediaPlayerLumin.GetKeysAndPrepareDRM failed to set {0} key response for media player {1}. Reason: {2}",
                            drmTrackEnum.ToString(),
                            localMediaPlayerID,
                            this.getKeyResult[drmTrack]);
                        yield break;
                    }

                    this.getKeyResult[drmTrack] = NativeBindings.PrepareDRM(localMediaPlayerID, drmTrackEnum);
                    if (this.getKeyResult[drmTrack] != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat(
                            "MLMediaPlayerLumin.GetKeysAndPrepareDRM failed to prepare {0} DRM for media player {1}. Reason: {2}",
                            drmTrackEnum.ToString(),
                            localMediaPlayerID,
                            this.getKeyResult[drmTrack]);
                        yield break;
                    }

                    MLPluginLog.DebugFormat(
                        "MLMediaPlayerLumin.GetKeysAndPrepareDRM {0} DRM handshake complete, DRM Prepared callback fired for media player {1}",
                        drmTrackEnum.ToString(),
                        localMediaPlayerID);
                }
            }

            /// <summary>
            /// <c>Coroutine</c> used for DRM handshake, waits until DRM preparation to be completed
            /// </summary>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            /// <param name="drmTrackEnum">Indicates the DRM is for Audio or video.</param>
            /// <returns>
            /// Waits for GetKeysAndPrepareDRM to be completed.
            /// </returns>
            private IEnumerator PerformDRMHandshakeForDRMTrack(int localMediaPlayerID, MLMediaPlayer.DRMTrack drmTrackEnum)
            {
                int drmTrack = (int)drmTrackEnum;
                const int MaxTries = 3;
                for (int tries = 0; tries < MaxTries; tries++)
                {
                    MLResult.Code resultCode = NativeBindings.OpenDRMSession(localMediaPlayerID, drmTrackEnum);
                    if (MLResult.Code.MediaDRMNotProvisioned == resultCode)
                    {
                        yield return this.StartCoroutine(this.ProvisionDeviceForDRM(localMediaPlayerID));
                        if (this.provisionResult != MLResult.Code.Ok)
                        {
                            MLPluginLog.ErrorFormat(
                                "MLMediaPlayerLumin.PerformDRMHandshakeForDRMTrack failed to privision device for {0} DRM with media player {1}. Reason: {2}",
                                drmTrackEnum.ToString(),
                                localMediaPlayerID,
                                this.provisionResult);
                            break;
                        }
                    }
                    else if (MLResult.Code.MediaDRMResourceBusy == resultCode)
                    {
                        MLPluginLog.WarningFormat(
                            "MLMediaPlayerLumin.PerformDRMHandshakeForDRMTrack trying again, {0} DRM resource busy for media player {1}",
                            drmTrackEnum.ToString(),
                            localMediaPlayerID);
                        yield return new WaitForSeconds(3);
                    }
                    else if (resultCode == MLResult.Code.Ok)
                    {
                        yield return this.StartCoroutine(this.GetKeysAndPrepareDRM(localMediaPlayerID, drmTrackEnum));
                        if (this.getKeyResult[drmTrack] != MLResult.Code.Ok)
                        {
                            MLPluginLog.ErrorFormat(
                                "MLMediaPlayerLumin.PerformDRMHandshakeForDRMTrack failed to acquire {0} DRM keys for media player {1}. Reason: {2}",
                                drmTrackEnum.ToString(),
                                localMediaPlayerID,
                                this.getKeyResult[drmTrack]);

                            resultCode = NativeBindings.CloseDRMSession(localMediaPlayerID, drmTrackEnum);
                            if (resultCode != MLResult.Code.Ok)
                            {
                                MLPluginLog.ErrorFormat(
                                    "MLMediaPlayerLumin.PerformDRMHandshakeForDRMTrack failed to close {0} DRM session for media player {1}. Reason: {2}",
                                    drmTrackEnum.ToString(),
                                    localMediaPlayerID,
                                    resultCode);
                            }

                            break;
                        }

                        MLPluginLog.DebugFormat("MLMediaPlayerLumin.PerformDRMHandshakeForDRMTrack successful {0} DRM handshake", drmTrackEnum.ToString());
                        yield break;
                    }
                    else
                    {
                        MLPluginLog.ErrorFormat(
                            "MLMediaPlayerLumin.PerformDRMHandshakeForDRMTrack failed {0} OpenDRMSession for media player {1}. Reason: {2}",
                            drmTrackEnum.ToString(),
                            localMediaPlayerID,
                            resultCode);
                        break;
                    }
                }

                MLPluginLog.ErrorFormat(
                    "MLMediaPlayerLumin.PerformDRMHandshakeForDRMTrack failed, unable to complete {0} DRM handshake for media player {1}.",
                    drmTrackEnum.ToString(),
                    localMediaPlayerID);
            }

            /// <summary>
            /// Function enables to add custom DRM request headers.
            /// </summary>
            /// <param name="keyRequest">The specific key for the requested header.</param>
            private void AddCustomLicenseHeaders(UnityWebRequest keyRequest)
            {
                if (this.customLicenseHeaderData != null)
                {
                    foreach (KeyValuePair<string, string> pair in this.customLicenseHeaderData)
                    {
                        if (!string.IsNullOrEmpty(pair.Key) && !string.IsNullOrEmpty(pair.Value))
                        {
                            keyRequest.SetRequestHeader(pair.Key, pair.Value);
                        }
                    }
                }
            }

            /// <summary>
            /// Function is used to parse the license response.
            /// </summary>
            /// <param name="drmTrackEnum">Indicates if its a video or audio.</param>
            /// <param name="data">response data from the license request.</param>
            /// <returns>
            /// Response from the license request as a byte array.
            /// </returns>
            private byte[] ProcessLicenseResponse(MLMediaPlayer.DRMTrack drmTrackEnum, byte[] data)
            {
                return (this.customLicenseResponseParser != null) ? this.customLicenseResponseParser(drmTrackEnum, data) : data;
            }

            /// <summary>
            /// Creates the texture on the renderer to play the video on <c>Lumin</c>.
            /// </summary>
            /// <param name="renderer">Renderer of the object to play on.</param>
            /// <param name="localMediaPlayerID">ID of the media player.</param>
            /// <param name="width">The width of the video player.</param>
            /// <param name="height">The height of the video player.</param>
            /// <returns>
            /// True on success, false otherwise.
            /// </returns>
            private bool CreateTexture(Renderer renderer, int localMediaPlayerID, int width, int height)
            {
                width = Mathf.Max(width, 1);
                height = Mathf.Max(height, 1);

                if (this.textureReference != null)
                {
                    UnityEngine.Object.Destroy(this.textureReference);
                }

                // Create texture with given dimensions
                this.textureReference = new Texture2D(width, height, TextureFormat.RGBA32, false);
                int size = width * height;
                Color[] colors = new Color[size];
                for (int i = 0; i < size; ++i)
                {
                    colors[i] = Color.black;
                }

                this.textureReference.SetPixels(colors);
                this.textureReference.filterMode = FilterMode.Point;
                this.textureReference.Apply();

                // Set texture on quad
                renderer.material.SetTexture("_MainTex", this.textureReference);
                NativeBindings.SetTextureFromUnity(localMediaPlayerID, this.textureReference.GetNativeTexturePtr(), width, height);

                return true;
            }

            /// <summary>
            /// The <c>coroutine</c> waits for end of frame before calling render on device.
            /// </summary>
            /// <returns>
            /// Waits till end of frame to return.
            /// </returns>
            private IEnumerator CallPluginAtEndOfFrames()
            {
                while (true)
                {
                    yield return new WaitForEndOfFrame();
                    //// This special sauce is only needed to call render on the device
                    if (!Application.isEditor)
                    {
                        GL.IssuePluginEvent(this.GetRenderCallback(), this.mediaPlayerID);
                        NativeBindings.GetTrackData(this.mediaPlayerID);
                    }
                }
            }

            /// <summary>
            /// <c>MonoBehaviour</c> callback, restores render <c>coroutines</c> if necessary.
            /// </summary>
            private void OnEnable()
            {
                if (this.restoreRenderCoroutines)
                {
                    this.restoreRenderCoroutines = false;
                    this.playbackCoroutine = this.BeginVideo();
                    this.StartCoroutine(this.playbackCoroutine);
                }
            }

            /// <summary>
            /// <c>MonoBehaviour</c> callback, stops render <c>coroutines</c> and marks them for restoration.
            /// </summary>
            private void OnDisable()
            {
                if (this.playbackCoroutine != null)
                {
                    if (this.callPluginAtEndOfFramesCoroutine != null)
                    {
                        this.StopCoroutine(this.callPluginAtEndOfFramesCoroutine);
                        this.callPluginAtEndOfFramesCoroutine = null;
                    }

                    this.StopCoroutine(this.playbackCoroutine);
                    this.playbackCoroutine = null;

                    this.restoreRenderCoroutines = true;
                }
            }

            /// <summary>
            /// <c>MonoBehaviour</c> callback triggered every frame.
            /// </summary>
            private void Update()
            {
                if (queuedCallbacks != null)
                {
                    List<QueuedCallback> queueCallbackList;
                    List<QueuedCallback> dispatchQueue = null;

                    if (queuedCallbacks.TryGetValue(this.mediaPlayerID, out queueCallbackList))
                    {
                        lock (queueCallbackList)
                        {
                            dispatchQueue = new List<QueuedCallback>(queueCallbackList);
                            queueCallbackList.Clear();
                        }

                        if (dispatchQueue != null)
                        {
                            foreach (QueuedCallback callback in dispatchQueue)
                            {
                                switch (callback.CallbackPrefix)
                                {
                                    case OnVideoEnded:
                                        if (this.VideoEnded != null)
                                        {
                                            this.VideoEnded(this.mediaPlayerID);
                                        }

                                        break;
                                    case OnVideoPrepared:
                                        if (this.startPrepared != null)
                                        {
                                            this.startPrepared(this.mediaPlayerID);

                                            if (this.playbackCoroutine != null)
                                            {
                                                this.StopCoroutine(this.playbackCoroutine);
                                            }

                                            this.playbackCoroutine = this.BeginVideo();
                                            this.StartCoroutine(this.playbackCoroutine);
                                        }

                                        break;
                                    case OnVideoBuffering:
                                        if (this.UpdateBufferingUI != null)
                                        {
                                            this.UpdateBufferingUI(this.mediaPlayerID, callback.Parameters[0]);
                                        }

                                        break;
                                    case OnVideoError:
                                        if (this.OnError != null)
                                        {
                                            this.OnError(this.mediaPlayerID, callback.Parameters[0], callback.Parameters[1]);
                                        }

                                        break;
                                    case OnVideoInfo:
                                        if (this.OnInfo != null)
                                        {
                                            this.OnInfo(this.mediaPlayerID, callback.Parameters[0], callback.Parameters[1]);
                                        }

                                        break;
                                    case OnVideoSizeChanged:
                                        this.lastKnownResolution.width = (int)callback.Parameters[0];
                                        this.lastKnownResolution.height = (int)callback.Parameters[1];
                                        this.CreateTexture(this.GetComponent<MeshRenderer>(), this.mediaPlayerID, (int)callback.Parameters[0], (int)callback.Parameters[1]);
                                        if (this.VideoSizeChanged != null)
                                        {
                                            this.VideoSizeChanged(this.mediaPlayerID, this.lastKnownResolution);
                                        }

                                        break;
                                    case OnResetAsyncCompleted:
                                        if (this.OnResetCompleted != null)
                                        {
                                            this.OnResetCompleted(this.mediaPlayerID);
                                        }

                                        break;
                                    case OnTrackDRMInfo:
                                        this.StartCoroutine(this.PerformDRMHandshakeForDRMTrack(this.mediaPlayerID, (MLMediaPlayer.DRMTrack)callback.Parameters[0]));
                                        break;
                                    case OnVideoSeekComplete:
                                        if (this.OnSeekCompleted != null)
                                        {
                                            this.OnSeekCompleted(this.mediaPlayerID);
                                        }

                                        break;
                                    case OnSubtitleTracksFound:
                                        if (this.GetSubtitleTracksCallback != null)
                                        {
                                            this.GetSubtitleTracksCallback(this.mediaPlayerID, callback.Parameters[0]);
                                        }

                                        break;
                                    case OnSubtitleInfoReceived:
                                        if (this.GetSubtitleDataCallback != null)
                                        {
                                            this.GetSubtitleDataCallback(this.mediaPlayerID, callback.Parameters[0]);
                                        }

                                        break;
                                    case OnSubtitle708EventReceived:
                                        if (this.GetSubtitle708EventCallback != null)
                                        {
                                            this.GetSubtitle708EventCallback(this.mediaPlayerID, callback.Parameters[0]);
                                        }

                                        break;
                                    case OnTimedTextInfoReceived:
                                        if (this.GetTimedTextDataCallback != null)
                                        {
                                            this.GetTimedTextDataCallback(this.mediaPlayerID, callback.Parameters[0], callback.Parameters[1], callback.Parameters[2]);
                                        }

                                        break;
                                    case OnTrackInfoReceived:
                                        this.mediaTracks = (Dictionary<long, MLMediaPlayer.TrackData>)callback.Parameters[0];
                                        break;
                                }
                            }
                        }
                        else
                        {
                            MLPluginLog.Debug("MLMediaPlayerLumin.Update DispatchQueue was null");
                        }
                    }
                    else
                    {
                        MLPluginLog.DebugFormat("MLMediaPlayerLumin.Update failed to get queued callbacks for media player {0}", this.mediaPlayerID);
                    }
                }
                else
                {
                    MLPluginLog.Debug("MLMediaPlayerLumin.Update, MLMediaPlayerLumin.QueuedCallbacks was NULL");
                }
            }

            /// <summary>
            /// Structure for the items to be queued.
            /// </summary>
            public struct QueuedCallback
            {
                /// <summary>
                /// The string representing the callback to be queued.
                /// </summary>
                public string CallbackPrefix;

                /// <summary>
                /// The payloads for the callback.
                /// </summary>
                public List<object> Parameters;

                /// <summary>
                /// Initializes a new instance of the <see cref="QueuedCallback"/> struct.
                /// Constructor used to initialize.
                /// </summary>
                /// <param name="prefix">The string representing the callback to be queued.</param>
                /// <param name="parameters"> The payloads for the callback.</param>
                public QueuedCallback(string prefix, List<object> parameters = null)
                {
                    this.CallbackPrefix = prefix;
                    this.Parameters = parameters;
                }
            }
        }
    }
}

#endif
