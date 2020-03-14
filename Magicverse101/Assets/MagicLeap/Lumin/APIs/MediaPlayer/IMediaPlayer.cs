// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="IMediaPlayer.cs" company="Magic Leap, Inc">
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
    using UnityEngine;

    /// <summary>
    /// Interface for target classes used by MLMediaPlayer
    /// Each function must be implemented by the child player type
    /// </summary>
    internal interface IMediaPlayer
    {
        /// <summary>
        /// Creates the streaming media player.
        /// </summary>
        /// <param name="mediaPlayerGO">The media player game object.</param>
        /// <param name="source">URL of the media.</param>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if media player was not properly built or initialized.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.PrivilegeDenied</c> if attempting to access web content without appropriate network privileges
        /// </returns>
        MLResult CreateStreamingMediaPlayer(GameObject mediaPlayerGO, string source, int localMediaPlayerID);

        /// <summary>
        /// Creates the texture on the renderer to play the video on.
        /// </summary>
        /// <param name="render">Renderer of the object to play on.</param>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// True on success, false otherwise.
        /// </returns>
        bool CreateTexture(Renderer render, int localMediaPlayerID);

        /// <summary>
        /// Initiate asynchronous reset of media player.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if successful.
        /// </returns>
        MLResult ResetAsync(int localMediaPlayerID);

        /// <summary>
        /// Plays the video.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult Play(int localMediaPlayerID);

        /// <summary>
        /// Pauses the video.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult PauseVideo(int localMediaPlayerID);

        /// <summary>
        /// Seeks the specified time in the video.
        /// </summary>
        /// <param name="position_seconds">Absolute time to seek to.</param>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult Seek(int position_seconds, int localMediaPlayerID);

        /// <summary>
        /// Sets the volume of the video.
        /// </summary>
        /// <param name="vol">Volume between 0 and 1.</param>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult SetVolume(float vol, int localMediaPlayerID);

        /// <summary>
        /// Stops the video in the editor.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult Stop(int localMediaPlayerID);

        /// <summary>
        /// Resume the video.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult Resume(int localMediaPlayerID);

        /// <summary>
        /// Sets the loop flag for the video.
        /// </summary>
        /// <param name="loop">Flag to loop</param>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult SetLooping(bool loop, int localMediaPlayerID);

        /// <summary>
        /// Releases any resource used by this media player ID.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult Cleanup(int localMediaPlayerID);

        /// <summary>
        /// Select specific subtitle or timed text track in the media
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <param name="trackID">The id of the track to be selected.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult SelectSubtitleTrack(int localMediaPlayerID, uint trackID);

        /// <summary>
        /// Unselect specific subtitle or timed text track in the media
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <param name="trackID">The id of the track to be unselected.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult UnselectSubtitleTrack(int localMediaPlayerID, uint trackID);

        /// <summary>
        /// Gets active audio channel count.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player</param>
        /// <param name="outAudioChannelCount">Return channel count.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult GetAudioChannelCount(int localMediaPlayerID, out int outAudioChannelCount);

        /// <summary>
        /// Sets spatial audio state.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player</param>
        /// <param name="isEnabled">Desired state of spatial audio.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult SetSpatialAudio(int localMediaPlayerID, bool isEnabled);

        /// <summary>
        /// Gets spatial audio state.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player</param>
        /// <param name="outIsEnabled">Return state of spatial audio.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult GetSpatialAudio(int localMediaPlayerID, out bool outIsEnabled);

        /// <summary>
        /// Sets world position of requested audio channel.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player</param>
        /// <param name="channel">Selects the channel whose position is being set.</param>
        /// <param name="position">Set selected channel's world position</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult SetAudioChannelPosition(int localMediaPlayerID, MLMediaPlayer.AudioChannel channel, Vector3 position);

        /// <summary>
        /// Gets world position of requested audio channel.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player</param>
        /// <param name="channel">Selects the channel whose position is being read.</param>
        /// <param name="outPosition">Return selected channel's world position</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        MLResult GetAudioChannelPosition(int localMediaPlayerID, MLMediaPlayer.AudioChannel channel, out Vector3 outPosition);

        /// <summary>
        /// Query a snapshot of all known track info for a given media player.
        /// This data can change from frame to frame.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// A Dictionary&lt;long, TrackData&gt; of all known tracks, empty if no track are known.
        /// </returns>
        Dictionary<long, MLMediaPlayer.TrackData> GetAllTrackInfo(int localMediaPlayerID);

        /// <summary>
        /// Register a request to get the bytes used for a DRM key request.
        /// </summary>
        /// <param name="drmUUIDBytes">Bytes identifying the desired DRM type.</param>
        /// <param name="callback">Callback to be called when successfully retrieved request data.</param>
        /// <returns>
        /// True if request was successfully registered.
        /// </returns>
        bool RequestActivationKeyRequest(byte[] drmUUIDBytes, Action<MLResult, byte[], string> callback);

        /// <summary>
        /// Gets the duration of the video in milliseconds
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player</param>
        /// <returns>
        /// Duration of the video, -1 on failure.
        /// </returns>
        int GetDurationMilliseconds(int localMediaPlayerID);

        /// <summary>
        /// Gets the current position of the video in milliseconds
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player</param>
        /// <returns>
        /// Position of the playback of the video, -1 on failure.
        /// </returns>
        int GetPositionMilliseconds(int localMediaPlayerID);

        /// <summary>
        /// Get the width of the video in pixels
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player</param>
        /// <returns>
        /// The width of the video, -1 on failure.
        /// </returns>
        int GetWidth(int localMediaPlayerID);

        /// <summary>
        /// Get the height of the video in pixels
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player</param>
        /// <returns>
        /// The height of the video, -1 on failure.
        /// </returns>
        int GetHeight(int localMediaPlayerID);

        /// <summary>
        /// Returns the last known resolution of the video in pixels.
        /// This data can change from frame to frame.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player</param>
        /// <returns>
        /// A <c>Rect</c> of the resolution.
        /// </returns>
        Rect GetResolution(int localMediaPlayerID);

        /// <summary>
        /// The bitrate of the video track in kbps
        /// This data can change from frame to frame.
        /// It is recommended that you use the OnInfo event for best results
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// The bitrate of the video, -1 on failure.
        /// </returns>
        int GetVideoBitrate(int localMediaPlayerID);

        /// <summary>
        /// Sets the license server for DRM videos.
        /// </summary>
        /// <param name="licenseServer">(unused) URL of the License Server</param>
        void SetLicenseServer(string licenseServer);

        /// <summary>
        /// Set custom header key-value pairs to use in addition to default of <c>"User-Agent : Widevine CDM v1.0"</c>
        /// when performing key request to the DRM license server.
        /// </summary>
        /// <param name="headerData">(unused) Dictionary of custom header key-value pairs</param>
        void SetCustomLicenseHeaderData(Dictionary<string, string> headerData);

        /// <summary>
        /// Set custom key request key-value pair parameters used when generating default key request.
        /// </summary>
        /// <param name="messageData">(unused) Dictionary of optional key-value pair parameters</param>
        void SetCustomLicenseMessageData(Dictionary<string, string> messageData);

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
        void SetCustomLicenseRequestBuilder(MLMediaPlayer.MediaPlayerCustomLicenseDelegate requestBuilder);

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
        void SetCustomLicenseResponseParser(MLMediaPlayer.MediaPlayerCustomLicenseDelegate responseParser);

        /// <summary>
        /// Gets the frame drop threshold.
        /// </summary>
        /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
        /// <returns>
        /// The currently set millisecond threshold.
        /// </returns>
        ulong GetFrameDropThresholdMs(int localMediaPlayerID);

        /// <summary>
        /// Sets a threshold to drop video frames if they are older than specified value.
        /// Setting this to 0 will not drop any frames, this is the default behavior.
        /// </summary>
        /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
        /// <param name="threshold">(unused) New threshold in milliseconds</param>
        void SetFrameDropThresholdMs(int localMediaPlayerID, ulong threshold);

        /// <summary>
        /// Sets sharing information for the media player being shared and enables only functionality
        /// for synchronize the content playback. Follower setting can only be set before video has been prepared.
        /// </summary>
        /// <param name="localMediaPlayerID">(unused) ID of the media player</param>
        /// <param name="sharedType">(unused) The shared type for the current media player from enum SharedType.</param>
        /// <param name="sessionID">(unused) Unique Identifier of the sharing session in which the media players are being shared.</param>
        /// <param name="isPrepared">Indicates if the media player has been prepared.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> for invalid parameters.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> shared information is successfully set.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> The operation failed with an unspecified error.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
        /// </returns>
        MLResult SetSharingInfo(int localMediaPlayerID, MLMediaPlayer.SharedType sharedType, string sessionID, bool isPrepared);

        /// <summary>
        /// Set the unique id on this player.
        /// This function needs to be called before the media player is prepared.
        /// The id should be unique across all media player sessions that are being shared.
        /// Once the id is set, then it can not be changed.
        /// Prepare will give an error if the ID is set and not unique.
        /// </summary>
        /// <param name="localMediaPlayerID">ID of the media player</param>
        /// <param name="ID">Unique ID for this player.</param>
        void SetID(int localMediaPlayerID, int ID);
    }
}
#endif
