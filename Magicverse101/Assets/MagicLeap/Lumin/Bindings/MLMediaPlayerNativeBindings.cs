// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMediaPlayerNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;

    /// <summary>
    /// Media player script that allows playback of a streaming video (either from file or web URL)
    /// This script will update the main texture parameter of the Renderer attached as a sibling
    /// with the video frame from playback. Audio is also handled through this class and will
    /// playback audio from the file.
    /// </summary>
    public sealed partial class MLMediaPlayer
    {
        /// <summary>
        /// See the MLMediaPlayer native plugin <c>"ml_mediaplayer_plugin.cpp"</c> for additional comments.
        /// </summary>
        private partial class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// Indicates the name of the plugin file.
            /// </summary>
            private const string MediaPlayerPluginDLL = "ml_mediaplayer_plugin";

            // The callback delegates used internally to handle events from MLMediaPlayer in our native plugin

            /// <summary>
            /// This callback function is invoked when the player has finished preparing
            /// media and is ready to playback.
            /// </summary>
            /// <param name="mediaPlayerID">Id of the media player</param>
            public delegate void PlayerPreparedCallback(int mediaPlayerID);

            /// <summary>
            /// This callback function is invoked when media player played back until end of
            /// media and has now come to a stop.
            /// Note that this callback does not fire when 'looping = true',
            /// because MediaPlayer does not "stop" in that case, but rather
            /// loops to beginning of media.
            /// </summary>
            /// <param name="mediaPlayerID">Id of the Media Player</param>
            public delegate void PlayerCompletionCallback(int mediaPlayerID);

            /// <summary>
            /// This callback function is invoked when media player is buffering.
            /// </summary>
            /// <param name="percent">Completed percentage</param>
            /// <param name="mediaPlayerID">Id of the Media Player</param>
            public delegate void PlayerBufferingCallback(int percent, int mediaPlayerID);

            /// <summary>
            /// This callback function is invoked when media player encounters an error.
            /// </summary>
            /// <param name="result"> result error/result code indicating failure reason.</param>
            /// <param name="errorString">Error data</param>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            public delegate void PlayerErrorCallback(int result, System.IntPtr errorString, int mediaPlayerID);

            /// <summary>
            /// This callback function is invoked when \ref MediaPlayer generates informational events.
            /// </summary>
            /// <param name="info">Info type of informational event</param>
            /// <param name="extra">extra MLMediaPlayerInfo type specific extra information.
            /// When info is MLMediaPlayerInfo_NetworkBandwidth, this holds bandwidth
            /// in kbps.It is 0 for others.</param>
            /// <param name="mediaPlayerID">Id of the media player.</param>
            public delegate void PlayerInfoCallback(int info, int extra, int mediaPlayerID);

            /// <summary>
            /// This callback function is invoked when a seek operation has completed.
            /// </summary>
            /// <param name="mediaPlayerID">The Id of the media player.</param>
            public delegate void PlayerSeekCompletedCallback(int mediaPlayerID);

            /// <summary>
            /// This callback function is invoked when the internal surface has changed size.
            /// </summary>
            /// <param name="width">New width of the video.</param>
            /// <param name="height">New height of the video.</param>
            /// <param name="mediaPlayerID">Id of the media player.</param>
            public delegate void PlayerVideoSizeChangedCallback(int width, int height, int mediaPlayerID);

            /// <summary>
            /// Callback signature called when an asynchronous reset has completed.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player</param>
            public delegate void PlayerResetAsyncCompletedCallback(int mediaPlayerID);

            /// <summary>
            /// This callback function is invoked when source has DRM protected media track(s).
            /// </summary>
            /// <param name="drmTrackIndex">Track id for the DRM</param>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            public delegate void PlayerTrackDRMInfoCallback(uint drmTrackIndex, int mediaPlayerID);

            /// <summary>
            /// This callback function is invoked when a new track is found.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="outInfo">return all the tracks currently present.</param>
            public delegate void PlayerAllTracksCallback(int mediaPlayerID, MLMediaPlayer.MediaPlayerTracks outInfo);

            /// <summary>
            /// This callback function is invoked when a new subtitle track is found
            /// </summary>
            /// <param name="mediaPlayerID">Id of theMedia Player.</param>
            /// <param name="outInfo">The list of subtitle tracks.</param>
            public delegate void PlayerSubtitleTracksCallback(int mediaPlayerID, MLMediaPlayer.MediaPlayerTracks outInfo);

            /// <summary>
            /// This callback function is invoked when new 608 data is ready.
            /// </summary>
            /// <param name="mediaPlayerID">Id of the media player</param>
            /// <param name="closedCaptionSegInt">Reference to CEA608 closed captioned segment structure.</param>
            public delegate void PlayerSubtitle608InfoCallback(int mediaPlayerID, ref MLMediaPlayer.Cea608CaptionSegmentInternal closedCaptionSegInt);

            /// <summary>
            /// This callback function is invoked when a new 708 event is ready.
            /// </summary>
            /// <param name="mediaPlayerID">Id of the media player</param>
            /// <param name="cea708Event">Reference to CEA708 closed captioned event structure.</param>
            public delegate void PlayerSubtitle708EventCallback(int mediaPlayerID, ref MLMediaPlayer.Cea708CaptionEventInternal cea708Event);

            /// <summary>
            /// Callback signature called when Timed Text update is available.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player</param>
            /// <param name="utf8ByteArray">An array containing the text as UTF8.</param>
            /// <param name="startTimeMs">Start time in milliseconds.</param>
            /// <param name="endTimeMs">End time in milliseconds</param>
            public delegate void PlayerTimedTextInfoCallback(int mediaPlayerID, System.IntPtr utf8ByteArray, long startTimeMs, long endTimeMs);

            /// <summary>
            /// Initializes the streaming media player.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="mediaSource">URL of the media.</param>
            /// <param name="colorSpace">cIndicates the color space Linear or Gamma.</param>
            /// <param name="managedCallbacks">The media player callbacks.</param>
            /// <param name="sharedType">The shared type for the current media player from enum SharedType.</param>
            /// <param name="sessionID">Unique Identifier of the sharing session in which the media players are being shared.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// <c>MLResult.Code.MediaGenericNoInit</c> if media player was not properly built or initialized.
            /// <c>MLResult.Code.PrivilegeDenied</c> if attempting to access web content without appropriate network privileges
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code InstantiateMediaPlayerContext(int mediaPlayerID, [MarshalAs(UnmanagedType.LPStr)] string mediaSource, MLMediaPlayer.ColorSpace colorSpace, MediaPlayerManagedCallbacks managedCallbacks, MLMediaPlayer.SharedType sharedType, [MarshalAs(UnmanagedType.LPStr)] string sessionID, int uniqueID);

            /// <summary>
            /// Initiate asynchronous reset of media player. Use <see cref="OnResetCompleted"/> event to know when reset completes,
            /// the player will be in a pre-prepared state. This method can be called anytime except while asynchronously preparing.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if successful.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if failed due to an internal error prevented MediaPlayer from resetting.
            /// <c>MLResult.Code.MediaGenericNoInit</c> if failed due to media player not properly initialized.
            /// <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed due to calling from the wrong state.
            /// <c>MLResult.Code.InvalidParam</c> if media player handle was not found.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code MediaPlayerResetAsync(int mediaPlayerID);

            /// <summary>
            /// Sets the Unity texture on the renderer to play the video on.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="texture">Texture of the video player.</param>
            /// <param name="w">New width of the video.</param>
            /// <param name="h">New height of the video.</param>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern void SetTextureFromUnity(int mediaPlayerID, System.IntPtr texture, int w, int h);

            /// <summary>
            /// Gets the Render callback on the graphics thread, which is used to render the video.
            /// </summary>
            /// <returns>
            /// Pointer to the render callback.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern System.IntPtr GetRenderCallback();

            /// <summary>
            /// Clean up the callback on the graphics thread, which is used to render the video.
            /// </summary>
            /// <returns>
            /// Pointer to the render callback.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern System.IntPtr GetRenderCleanupCallback();

            // DRM

            /// <summary>
            /// Open DM session.
            /// </summary>
            /// <param name="mediaPlayerID">Id of media player</param>
            /// <param name="drmTrack">DRM rack type</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code OpenDRMSession(int mediaPlayerID, MLMediaPlayer.DRMTrack drmTrack);

            /// <summary>
            /// Close DM session.
            /// </summary>
            /// <param name="mediaPlayerID">Id of media player</param>
            /// <param name="drmTrack">DRM rack type</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code CloseDRMSession(int mediaPlayerID, MLMediaPlayer.DRMTrack drmTrack);

            /// <summary>
            /// A provision request/response exchange occurs between the app and a provisioning
            /// server to retrieve a device certificate.
            /// If provisioning is required, the #EVENT_PROVISION_REQUIRED event will be sent to the event handler.
            /// MLMediaDRMGetProvisionRequest() is used to obtain the opaque provision request byte array that
            /// should be delivered to the provisioning server.
            /// </summary>
            /// <param name="mediaPlayerID">ID of media player.</param>
            /// <param name="certType">The device certificate type, which can be "none" or "X.509".</param>
            /// <param name="outRequestMessage">out_provision_request Upon successful return, contains provision request message.</param>
            /// <param name="outRequestMessageLength">Message length.</param>
            /// <param name="outDefaultURL">default DRM request url.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetProvisionRequest(int mediaPlayerID, [MarshalAs(UnmanagedType.LPStr)] string certType, ref IntPtr outRequestMessage, ref ulong outRequestMessageLength, [MarshalAs(UnmanagedType.LPStr)] ref string outDefaultURL);

            /// <summary>
            /// After a provision response is received by the app, it is provided to the DRM
            /// </summary>
            /// <param name="mediaPlayerID">ID of media player</param>
            /// <param name="responseMessage">The opaque provisioning response byte array to provide to the DRM engine plugin.</param>
            /// <param name="responseMessageLength">Response length.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code ProvideProvisionResponse(int mediaPlayerID, byte[] responseMessage, ulong responseMessageLength);

            /// <summary>
            /// A provision request/response exchange occurs between the app and a provisioning
            /// server to retrieve a device certificate.
            /// </summary>
            /// <param name="certType">The device certificate type, which can be "none" or "X.509".</param>
            /// <param name="outRequestMessage">
            /// Upon successful return, contains provision request message.
            /// To free/release this, call MLMediaDRMRequestMessageRelease().
            /// </param>
            /// <param name="outRequestMessageLength"> The message length.</param>
            /// <param name="outDefaultURL">The URL of the media</param>
            /// <returns>
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.Ok Device</c> Provision Request message is constructed successfully.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetActivationProvisionRequest([MarshalAs(UnmanagedType.LPStr)] string certType, ref IntPtr outRequestMessage, ref ulong outRequestMessageLength, [MarshalAs(UnmanagedType.LPStr)] ref string outDefaultURL);

            /// <summary>
            /// A provision request/response exchange occurs between the app and a provisioning
            /// server to retrieve a device certificate.
            /// </summary>
            /// <param name="responseMessage">The response message.</param>
            /// <param name="responseMessageLength">the message length.</param>
            /// <returns>
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.Ok Device</c> Provision Request message is constructed successfully.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code ProvideActivationProvisionResponse(byte[] responseMessage, ulong responseMessageLength);

            /// <summary>
            /// This function will copy the strings, so no need to keep them around.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="drmTrack">The DRM track type</param>
            /// <param name="keys">The DRM keys associated withe the track.</param>
            /// <param name="values">the key value</param>
            /// <param name="pairCount">Number of key value pairs</param>
            /// <returns>
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.Ok Device</c> Key Request message is constructed successfully.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code SetKeyRequestCustomData(int mediaPlayerID, MLMediaPlayer.DRMTrack drmTrack, [MarshalAs(UnmanagedType.LPStr)] string[] keys, [MarshalAs(UnmanagedType.LPStr)] string[] values, ulong pairCount);

            /// <summary>
            /// A key request/response exchange occurs between the app and a license server
            /// to obtain or release keys used to decrypt encrypted content.
            /// GetKeyRequest() is used to obtain an opaque key request byte array
            /// that is delivered to the license server.
            /// The opaque key request byte array is returned in out_key_request.request
            /// The recommended URL to deliver the key request to is returned in out_key_request.default_URL.
            /// </summary>
            /// <param name="drmUUID">Bytes identifying the desired DRM type.</param>
            /// <param name="keyType">DRM key type</param>
            /// <param name="outRequestMessage">contains key request message.</param>
            /// <param name="outRequestMessageLength">request message length</param>
            /// <param name="outDefaultURL">The recommended URL to deliver the request to.</param>
            /// <returns>
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.Ok Device</c> Key Request message is constructed successfully.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetActivationKeyRequest(ref MLUUIDBytes drmUUID, MLMediaPlayer.DRMKeyType keyType, ref IntPtr outRequestMessage, ref ulong outRequestMessageLength, [MarshalAs(UnmanagedType.LPStr)] ref string outDefaultURL);

            /// <summary>
            /// A key request/response exchange occurs between the app and a license server
            /// to obtain or release keys used to decrypt encrypted content.
            /// GetKeyRequest() is used to obtain an opaque key request byte array
            /// that is delivered to the license server.
            /// The opaque key request byte array is returned in out_key_request.request
            /// The recommended URL to deliver the key request to is returned in out_key_request.default_URL.
            /// When the response is for a streaming or release request, a null key_set_id is returned.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="drmTrack">The DRM track type</param>
            /// <param name="keyType">DRM key type.</param>
            /// <param name="outRequestMessage">The opaque response from the server</param>
            /// <param name="outRequestMessageLength">>Response message length.</param>
            /// <param name="outDefaultURL">The recommended URL to deliver the request to. </param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetKeyRequest(int mediaPlayerID, MLMediaPlayer.DRMTrack drmTrack, MLMediaPlayer.DRMKeyType keyType, ref IntPtr outRequestMessage, ref ulong outRequestMessageLength, [MarshalAs(UnmanagedType.LPStr)] ref string outDefaultURL);

            /// <summary>
            /// A key response is received from the license server by the app, then it is
            /// provided to the DRM engine plugin using ProvideKeyResponse().
            /// When the response is for an offline key request, a key_set_id is returned that can be
            /// used to later restore the keys to a new session with restoreKeys().
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="drmTrack">the DRM track type.</param>
            /// <param name="responseMessage">The opaque response from the server.</param>
            /// <param name="responseMessageLength">Response message length.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code ProvideKeyResponse(int mediaPlayerID, MLMediaPlayer.DRMTrack drmTrack, byte[] responseMessage, ulong responseMessageLength);

            /// <summary>
            /// Prepare DRM.
            /// </summary>
            /// <param name="mediaPlayerID">Id of media player.</param>
            /// <param name="drmTrack">DRM Track type</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code PrepareDRM(int mediaPlayerID, MLMediaPlayer.DRMTrack drmTrack);

            // End DRM

            /// <summary>
            /// Starts playing the media.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code Play(int mediaPlayerID);

            /// <summary>
            /// Pauses the video.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code PauseVideo(int mediaPlayerID);

            /// <summary>
            /// Seeks the specified time in the video.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="positionMilliseconds">Absolute time to seek to.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code Seek(int mediaPlayerID, int positionMilliseconds);

            /// <summary>
            /// Resume the video.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code Resume(int mediaPlayerID);

            /// <summary>
            /// Stops the video in the editor.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code Stop(int mediaPlayerID);

            /// <summary>
            /// Sets the loop flag for the video.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="loop">Flag to loop</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code SetLooping(int mediaPlayerID, [MarshalAs(UnmanagedType.I1)] bool loop);

            /// <summary>
            /// Sets the volume of the video.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="vol">Volume between 0 and 1.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code SetVolume(int mediaPlayerID, float vol);

            /// <summary>
            /// Gets the duration of the video in milliseconds
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="outMS">Duration of the video, -1 on failure</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetDurationMilliseconds(int mediaPlayerID, out int outMS);

            /// <summary>
            /// Gets the current position of the video in milliseconds
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player</param>
            /// <param name="outMS">Position of the playback of the video, -1 on failure</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetPositionMilliseconds(int mediaPlayerID, out int outMS);

            /// <summary>
            /// Get the width of the video in pixels
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="outWidth">The width of the video, -1 on failure.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetWidth(int mediaPlayerID, out int outWidth);

            /// <summary>
            /// Get the height of the video in pixels
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player</param>
            /// <param name="outHeight">The height of the video, -1 on failure.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetHeight(int mediaPlayerID, out int outHeight);

            /// <summary>
            /// The bitrate of the video track in kbps
            /// This data can change from frame to frame.
            /// It is recommended that you use the OnInfo event for best results
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="outBitrate">The bitrate of the video, -1 on failure</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetVideoBitrate(int mediaPlayerID, out int outBitrate);

            /// <summary>
            /// Gets the frame drop threshold.
            /// </summary>
            /// <param name="mediaPlayerID">(unused) ID of the media player</param>
            /// <returns>
            /// The currently set millisecond threshold.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern ulong GetFrameDropThresholdMs(int mediaPlayerID);

            /// <summary>
            /// Sets a threshold to drop video frames if they are older than specified value.
            /// Setting this to 0 will not drop any frames, this is the default behavior.
            /// </summary>
            /// <param name="mediaPlayerID">(unused) ID of the media player</param>
            /// <param name="threshold">(unused) New threshold in milliseconds</param>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern void SetFrameDropThresholdMs(int mediaPlayerID, ulong threshold);

            //// Closed Captions

            /// <summary>
            /// Query a snapshot of all known track info for a given media player.
            /// This data can change from frame to frame.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <returns>
            /// A Dictionary&lt;long, TrackData&gt; of all known tracks, empty if no track are known.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetTrackData(int mediaPlayerID);

            /// <summary>
            /// Select specific subtitle or timed text track in the media
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="id">The id of the track to be selected.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code SelectSubtitleTrack(int mediaPlayerID, uint id);

            /// <summary>
            /// Unselect specific subtitle or timed text track in the media
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="id">The id of the track to be unselected.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code UnselectSubtitleTrack(int mediaPlayerID, uint id);

            /// <summary>
            /// Gets active audio channel count.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="outAudioChannelCount">Return channel count.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// <c>MLResult.Code.MediaGenericInvalidOperation</c> Method was called from the wrong state. Can only be called after one of the setDataSource methods.
            /// <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
            /// <c>MLResult.Code.NotImplemented</c> No audio track found.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetAudioChannelCount(int mediaPlayerID, out int outAudioChannelCount);

            /// <summary>
            /// Sets spatial audio state.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="isEnabled">Desired state of spatial audio.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
            /// <c>MLResult.Code.AudioHandleNotFound</c> Audio Handle not found.
            /// <c>MLResult.Code.AudioInternalConfigError</c> Internal config error.
            /// <c>MLResult.Code.AudioNotImplemented</c> Internal error.
            /// <c>MLResult.Code.AllocFailed</c> No valid media player handle.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code SetSpatialAudioEnable(int mediaPlayerID, [MarshalAs(UnmanagedType.I1)] bool isEnabled);

            /// <summary>
            /// Gets spatial audio state.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="outIsEnabled">Return state of spatial audio.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
            /// <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
            /// <c>MLResult.Code.AudioHandleNotFound</c> Audio Handle not found.
            /// <c>MLResult.Code.AudioInternalConfigError</c> Internal config error.
            /// <c>MLResult.Code.AudioNotImplemented</c> Internal error.
            /// <c>MLResult.Code.AllocFailed</c> No valid media player handle.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetSpatialAudioEnable(int mediaPlayerID, [MarshalAs(UnmanagedType.I1)] out bool outIsEnabled);

            /// <summary>
            /// Sets world position of requested audio channel.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="channel">Selects the channel whose position is being set.</param>
            /// <param name="position">Set selected channel's world position</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Code.InvalidParam</c> If input parameter is invalid.
            /// <c>MLResult.Code.UnspecifiedFailure</c> Internal error.
            /// <c>MLResult.Code.AudioHandleNotFound</c> Handle not found.
            /// <c>MLResult.Code.AudioInternalConfigError</c> Internal config error.
            /// <c>MLResult.Code.AudioNotImplemented</c> Internal error.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code SetAudioChannelPosition(int mediaPlayerID, uint channel, MLVec3f position);

            /// <summary>
            /// Gets world position of requested audio channel.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="channel">Selects the channel whose position is being read.</param>
            /// <param name="outPosition">Return selected channel's world position</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// <c>MLResult.Code.InvalidParam</c> If input parameter is invalid.
            /// <c>MLResult.Code.UnspecifiedFailure</c> Internal error.
            /// <c>MLResult.Code.AudioHandleNotFound</c> Handle not found.
            /// <c>MLResult.Code.AudioInternalConfigError</c> Internal config error.
            /// <c>MLResult.Code.AudioNotImplemented</c> Internal error.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code GetAudioChannelPosition(int mediaPlayerID, uint channel, out MLVec3f outPosition);

            /// <summary>
            /// Releases any resource used by this media player ID.
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if operation succeeded.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code Cleanup(int mediaPlayerID);

            /// <summary>
            /// Sets sharing information for the media player being shared and enables only functionality
            /// for synchronize the content playback.
            /// This function needs to be called only after Id is set on media player using MLMediaPlayerSetID().
            /// </summary>
            /// <param name="mediaPlayerID">ID of the media player.</param>
            /// <param name="sharedType">The shared type for the current media player from enum SharedType.</param>
            /// <param name="sessionID">Unique Identifier of the sharing session in which the media players are being shared.</param>
            /// <returns>
            /// <c>MLResult.Code.InvalidParam</c> for invalid parameters.
            /// <c>MLResult.Code.OK</c> shared information is successfully set.
            /// <c>MLResult.Code.UnspecifiedFailure</c> The operation failed with an unspecified error.
            /// <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
            /// </returns>
            [DllImport(MediaPlayerPluginDLL)]
            public static extern MLResult.Code SetSharingInfo(int mediaPlayerID, MLMediaPlayer.SharedType sharedType, [MarshalAs(UnmanagedType.LPStr)] string sessionID);

            /// <summary>
            /// Structure Containing al the callbacks used during Initialization.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MediaPlayerManagedCallbacks
            {
                /// <summary>
                /// This callback function is invoked when the player has finished preparing
                /// media and is ready to playback.
                /// </summary>
                public PlayerPreparedCallback OnPrepared;

                /// <summary>
                /// This callback function is invoked when media player played back until end of
                /// media and has now come to a stop.
                /// Note that this callback does not fire when 'looping = true',
                /// because MediaPlayer does not "stop" in that case, but rather
                /// loops to beginning of media.
                /// </summary>
                public PlayerCompletionCallback OnCompletion;

                /// <summary>
                /// This callback function is invoked when media player is buffering.
                /// </summary>
                public PlayerBufferingCallback OnBuffering;

                /// <summary>
                /// This callback function is invoked when media player encounters an error.
                /// </summary>
                public PlayerErrorCallback OnError;

                /// <summary>
                /// This callback function is invoked when \ref MediaPlayer generates informational events.
                /// </summary>
                public PlayerInfoCallback OnInfo;

                /// <summary>
                /// This callback function is invoked when a seek operation has completed.
                /// </summary>
                public PlayerSeekCompletedCallback OnSeekCompleted;

                /// <summary>
                /// This callback function is invoked when the internal surface has changed size.
                /// </summary>
                public PlayerVideoSizeChangedCallback OnVideoSizeChanged;

                /// <summary>
                /// Callback signature called when an asynchronous reset has completed.
                /// </summary>
                public PlayerResetAsyncCompletedCallback OnResetCompleted;

                /// <summary>
                /// This callback function is invoked when source has DRM protected media track(s).
                /// </summary>
                public PlayerTrackDRMInfoCallback OnTrackDRMInfo;

                /// <summary>
                /// This callback function is invoked when a new track is found.
                /// </summary>
                public PlayerAllTracksCallback OnAllTracks;

                /// <summary>
                /// This callback function is invoked when a new subtitle track is found
                /// </summary>
                public PlayerSubtitleTracksCallback OnSubtitleTracks;

                /// <summary>
                /// This callback function is invoked when new 608 data is ready.
                /// </summary>
                public PlayerSubtitle608InfoCallback OnSubtitleInfo;

                /// <summary>
                /// This callback function is invoked when a new 708 event is ready.
                /// </summary>
                public PlayerSubtitle708EventCallback OnSubtitle708Event;

                /// <summary>
                /// Callback signature called when Timed Text update is available.
                /// </summary>
                public PlayerTimedTextInfoCallback OnTimedTextInfo;
            }
        }
    }
}

#endif
