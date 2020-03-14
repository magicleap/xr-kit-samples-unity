// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMediaPlayer.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Media player script that allows playback of a streaming video (either from file or web URL)
    /// This script will update the main texture parameter of the Renderer attached as a sibling
    /// with the video frame from playback. Audio is also handled through this class and will
    /// playback audio from the file.
    /// </summary>
    public sealed partial class MLMediaPlayer : MonoBehaviour
    {
        #if PLATFORM_LUMIN

        /// <summary>
        /// CEA708 Caption maximum windows.
        /// </summary>
        public static readonly int Cea708CaptionWindowsMax = 20;

        /// <summary>
        /// DRM : <c>Widevine</c> UUID Identifier: <c>"edef8ba9-79d6-4ace-a3c8-27dcd51d21ed"</c>
        /// </summary>
        public static readonly byte[] WidevineUUID = new byte[] { 0xed, 0xef, 0x8b, 0xa9, 0x79, 0xd6, 0x4a, 0xce, 0xa3, 0xc8, 0x27, 0xdc, 0xd5, 0x1d, 0x21, 0xed };

        /// <summary>
        /// Subtitle Text VTT mime type.
        /// </summary>
        public static readonly string MimeTypeTextVTT = "text/vtt";

        /// <summary>
        /// Subtitle Text CEA608 mime type.
        /// </summary>
        public static readonly string MimeTypeTextCEA608 = "text/cea-608";

        /// <summary>
        /// Subtitle Text CEA708 mime type.
        /// </summary>
        public static readonly string MimeTypeTextCEA708 = "text/cea-708";

        /// <summary>
        /// Array of recommended speaker offset locations for 5.1 audio. This array follows the <see cref="AudioChannel"/> enumeration order.
        /// <c>
        /// | Channel | Azimuth(degrees) | Elevation(degrees) | Distance(meters) |
        /// |    L    |      -30.0       |         0.0        |        1.0       |
        /// |    R    |       30.0       |         0.0        |        1.0       |
        /// |    C    |        0.0       |         0.0        |        1.0       |
        /// |  LFE    |        0.0       |         0.0        |        1.0       |
        /// |   Ls    |     -110.0       |         0.0        |        1.0       |
        /// |   Rs    |      110.0       |         0.0        |        1.0       |
        /// </c>
        /// </summary>
        public static readonly Vector3[] RecommendedSpeakerOffsets = new[]
        {
            Quaternion.AngleAxis(-30.0f,  Vector3.up) * Vector3.forward, // Front Left
            Quaternion.AngleAxis(30.0f,   Vector3.up) * Vector3.forward, // Front Right
                                                        Vector3.forward, // Front Center
                                                        Vector3.forward, // Low Frequency Effects
            Quaternion.AngleAxis(-110.0f, Vector3.up) * Vector3.forward, // Surround Left
            Quaternion.AngleAxis(110.0f,  Vector3.up) * Vector3.forward  // Surround Right
        };

        /// <summary>
        /// Value incremented when a new MLMediaPlayer is created, used as a unique ID among any active players.
        /// </summary>
        private static int activePlayerCount = 0;

        /// <summary>
        /// The Media Player's license server URL.
        /// </summary>
        private string licenseServer = string.Empty;

        /// <summary>
        /// Duration of the video in milliseconds.
        /// Zero duration means its a live video.
        /// </summary>
        private int durationMs = 0;

        /// <summary>
        /// Instance of <c>IMediaPlayer</c>
        /// </summary>
        private IMediaPlayer videoPlayerImpl = null;

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
        private MediaPlayerCustomLicenseDelegate customLicenseRequestBuilder = null;

        /// <summary>
        /// Function pointer for license response.
        /// </summary>
        private MediaPlayerCustomLicenseDelegate customLicenseResponseParser = null;

        /// <summary>
        /// Toggle to switch on and off the media player.
        /// </summary>
        private bool looping = false;

        /// <summary>
        /// Texture used to render the video frame.
        /// </summary>
        private bool textureGenerated = false;

        /// <summary>
        /// Indicates if video is ready for playback.
        /// </summary>
        private bool prepared = false;

        /// <summary>
        /// Indicates if media player is in the process of being reset.
        /// </summary>
        private bool resetting = false;

        /// <summary>
        /// Indicates if the video was playing when the application switched to pause state.
        /// </summary>
        private bool wasPlayingBeforeApplicationPaused = false;

        /// <summary>
        /// Media player's renderer's material.
        /// </summary>
        private MeshRenderer meshRenderer = null;

        /// <summary>
        /// Basic black texture to be displayed until a video starts up.
        /// </summary>
        private Texture2D defaultBackdropTexture = null;

        /// <summary>
        /// A reference of media player's renderer's material.
        /// </summary>
        private Material videoRenderMaterial = null;

        /// <summary>
        /// Used to indicate the stereo mode set for the media player.
        /// </summary>
        private VideoStereoMode stereoMode = VideoStereoMode.Mono;

        /// <summary>
        /// Used to cache last received subtitle track info.
        /// </summary>
        private Dictionary<long, TrackData> subtitleTracksCache = null;

        /// <summary>
        /// Invoked when media player finds new closed caption track information.
        /// First parameter is a dictionary of TrackData objects.
        /// </summary>
        private SubtitleFoundDelegate onSubtitleTracksFound = null;

        /// <summary>
        /// Delegates used for media player control events.
        /// </summary>
        public delegate void MediaControlDelegate();

        /// <summary>
        /// Defines a delegate used for media player control events.
        /// </summary>
        /// <param name="time">Time in milliseconds.</param>
        public delegate void MediaControlTimeDelegate(int time);

        /// <summary>
        /// Defines a delegate used for media player seek and buffer events.
        /// </summary>
        /// <param name="position">Position between 0 and 1.</param>
        public delegate void MediaControlPositionDelegate(float position);

        /// <summary>
        /// Defines a delegate used for video player size events.
        /// </summary>
        /// <param name="rectTransform">Height and width of the video player.</param>
        public delegate void MediaControlRectDelegate(Rect rectTransform);

        /// <summary>
        /// Defines a delegate used for CEA608 caption events.
        /// </summary>
        /// <param name="cea608">Defines the type of </param>
        public delegate void Subtitle608Delegate(Cea608CaptionSegment cea608);

        /// <summary>
        /// Defines a delegate used for CEA708 caption events.
        /// </summary>
        /// <param name="cea608">Defines the type of </param>
        public delegate void Subtitle708Delegate(Cea708CaptionEvent cea608);

        /// <summary>
        /// Defines a delegate used for media player error events.
        /// </summary>
        /// <param name="result">The type of error as MLMediaPlayerError</param>
        /// <param name="error">Extra information about the error as MLMediaError.</param>
        public delegate void MediaErrorDelegate(MLResult.Code result, string error);

        /// <summary>
        /// Defines a delegate used for informational events available.
        /// </summary>
        /// <param name="type">The type of information event as <c>MLMediaPlayerInfo</c></param>
        /// <param name="info">Any extra info the informational event may define.s</param>
        public delegate void MediaInfoDelegate(PlayerInfo type, int info);

        /// <summary>
        /// Defines a delegate used for closed caption track information.
        /// </summary>
        /// <param name="subtitleTrackInfo">Dictionary of TrackData objects.</param>
        public delegate void SubtitleFoundDelegate(Dictionary<long, TrackData> subtitleTrackInfo);

        /// <summary>
        /// Invoked when timed text data is received from media library, time is based on media time.
        /// </summary>
        /// <param name="data">The textual data for this update</param>
        /// <param name="startTime">The time in milliseconds this text should be displayed</param>
        /// <param name="duration">The time in milliseconds this text should stop being displayed</param>
        public delegate void TimedTextDelegate(string data, long startTime, long duration);

        /// <summary>
        /// Delegate used for license request or response.
        /// </summary>
        /// <param name="drmTrack">DRM track being processed, either video or audio.</param>
        /// <param name="body">body to the license request or response.</param>
        /// <returns>Returns the byte[] which will be used as the body to the license request.</returns>
        public delegate byte[] MediaPlayerCustomLicenseDelegate(DRMTrack drmTrack, byte[] body);

        /// <summary>
        /// Callback that is fired when the Media Player begins playback.
        /// Provides the length/duration of the video being played, in milliseconds.
        /// </summary>
        public event MediaControlTimeDelegate OnPlay = delegate { };

        /// <summary>
        /// Callback that is fired when the Media Player pauses playback
        /// </summary>
        public event MediaControlDelegate OnPause = delegate { };

        /// <summary>
        /// Callback that is fired when the Media Player stops playback
        /// </summary>
        public event MediaControlDelegate OnStop = delegate { };

        /// <summary>
        /// When beginning a seek, notifies with the target position in the media as a percentage.
        /// </summary>
        public event MediaControlPositionDelegate OnSeekStarted = delegate { };

        /// <summary>
        /// When a seek completes, notifies with the current position in the media as a percentage.
        /// </summary>
        public event MediaControlPositionDelegate OnSeekCompleted = delegate { };

        /// <summary>
        /// When a video's resolution changes, notifies with the current resolution as a <c>Rect</c>.
        /// </summary>
        public event MediaControlRectDelegate OnVideoSizeChanged = delegate { };

        /// <summary>
        /// Callback that notifies when the video has reached the end of the stream.
        /// </summary>
        public event MediaControlDelegate OnEnded = delegate { };

        /// <summary>
        /// When a video is buffering, notifies the percentage of completeness of the buffering.
        /// </summary>
        public event MediaControlPositionDelegate OnBufferingUpdate = delegate { };

        /// <summary>
        /// On starting, notifies the current aspect ratio of the video file
        /// </summary>
        public event MediaControlPositionDelegate OnFrameSizeSetup = delegate { };

        /// <summary>
        /// Notifies when a video has been prepared and is ready to begin playback
        /// </summary>
        public event MediaControlDelegate OnVideoPrepared = delegate { };

        /// <summary>
        /// Invoked when media player encounters an error.
        /// First parameter is the type of error as MLMediaPlayerError.
        /// Second parameter is extra information about the error as MLMediaError.
        /// </summary>
        public event MediaErrorDelegate OnMediaError = delegate { };

        /// <summary>
        /// Invoked when CEA608 subtitle data is received from the media library.
        /// First parameter is a <c>MLCea608CaptionSegment</c> object that contains all the data given.
        /// </summary>
        public event Subtitle608Delegate OnSubtitle608DataFound = delegate { };

        /// <summary>
        /// Invoked when a CEA708 subtitle event is received from the media library.
        /// First parameter is a <c>Cea708CaptionEvent</c> object that contains all the data given.
        /// You must cast the object based on the type.
        /// </summary>
        public event Subtitle708Delegate OnSubtitle708EventFound = delegate { };

        /// <summary>
        /// Invoked when timed text data is received from media library, time is based on media time.
        /// First parameter is the textual data for this update.
        /// Second parameter is time in milliseconds this text should be displayed.
        /// Third parameter is time in milliseconds this text should stop being displayed.
        /// </summary>
        public event TimedTextDelegate OnTimedTextDataFound = delegate { };

        /// <summary>
        /// Invoked when an asynchronous reset completes, player is ready to be prepared again.
        /// </summary>
        public event MediaControlDelegate OnResetCompleted = delegate { };

        /// <summary>
        /// Invoked when media player has informational events available
        /// First parameter is the type of information event as MLMediaPlayerInfo
        /// Second parameter is any extra info the informational event may define:
        /// When info is MLMediaPlayerInfo.NetworkBandwidth, this holds bandwidth in kbps.
        /// It is 0 for others.
        /// </summary>
        public event MediaInfoDelegate OnInfo = delegate { };
        #endif

        /// <summary>
        /// Enumeration of the available audio channel indices in 5.1 SMPTE order.
        /// </summary>
        public enum AudioChannel : uint
        {
            /// <summary>
            /// Front left channel index.
            /// </summary>
            FrontLeft           = 0,

            /// <summary>
            /// Front right channel index.
            /// </summary>
            FrontRight          = 1,

            /// <summary>
            /// Front center channel index.
            /// </summary>
            FrontCenter         = 2,

            /// <summary>
            /// Low frequency effects channel index.
            /// </summary>
            LowFrequencyEffects = 3,

            /// <summary>
            /// Surround left channel index.
            /// </summary>
            SurroundLeft        = 4,

            /// <summary>
            /// Surround right channel index.
            /// </summary>
            SurroundRight       = 5
        }

        /// <summary>
        /// Enumeration of the available stereo rendering modes for video frames.
        /// </summary>
        public enum VideoStereoMode : int
        {
            /// <summary>
            /// <c>Monoscopic</c> Video.
            /// </summary>
            Mono = 0,

            /// <summary>
            /// Stereoscopic Video.
            /// Left side for left eye.
            /// Right side for right eye.
            /// </summary>
            SideBySide = 1,

            /// <summary>
            /// Stereoscopic Video.
            /// Top part for left eye.
            /// Bottom part side for right eye
            /// </summary>
            OverUnder = 2
        }

        /// <summary>
        /// Media Player color Space.
        /// </summary>
        public enum ColorSpace
        {
            /// <summary>
            /// Linear color space.
            /// </summary>
            Linear = 0,

            /// <summary>
            /// Gamma color space.
            /// </summary>
            Gamma
        }

        /// <summary>
        /// Indicates various trigger various media player actions.
        /// <c>MLMediaPlayerInfo</c> from <c>ml_media_player.h</c>.
        /// </summary>
        public enum PlayerInfo
        {
            /// <summary>
            /// Unknown status
            /// </summary>
            Unknown = 1,

            /// <summary>
            /// The player was started because it was used as the next player.
            /// </summary>
            StartedAsNext = 2,

            /// <summary>
            /// The player just pushed the very first video frame for rendering.
            /// </summary>
            RenderingStart = 3,

            /// <summary>
            /// The player just reached EOS and started from beginning loop.
            /// </summary>
            Looping = 4,

            /// <summary>
            /// The player acknowledgement that it has started playing.
            /// </summary>
            Started = 5,

            /// <summary>
            /// The player acknowledgement that it has paused.
            /// </summary>
            Paused = 6,

            /// <summary>
            /// The player acknowledgement that it has stopped playing.
            /// </summary>
            Stopped = 7,

            /// <summary>
            /// The player acknowledgement that it has started playing as result of shared player's request.
            /// </summary>
            StartedBySharedPlayer = 8,

            /// <summary>
            /// The player acknowledgement that it has paused playing as result of shared player's request.
            /// </summary>
            PausedBySharedPlayer = 9,

            /// <summary>
            /// The player acknowledgement that it is seeking as result of shared player's request.
            /// </summary>
            SeekBySharedPlayer = 10,

            /// <summary>
            /// The player acknowledgement that it has stopped playing as result of shared player's request.
            /// </summary>
            StoppedBySharedPlayer = 11,

            /// <summary>
            /// The Media player has started sync'ing with other shared players.
            /// </summary>
            SyncStart = 12,

            /// <summary>
            /// The Media player has completed sync'ing with other shared players.
            /// </summary>
            SyncComplete = 13,

            /// <summary>
            /// The video is too complex for the decoder: it can't decode frames fast enough.
            /// </summary>
            VideoTrackLagging = 700,

            /// <summary>
            /// Media player is temporarily pausing playback.
            /// </summary>
            BufferingStart = 701,

            /// <summary>
            /// Media player is resuming playback after filling buffers.
            /// </summary>
            BufferingEnd = 702,

            /// <summary>
            /// Network bandwidth info.
            /// </summary>
            NetworkBandwidth = 703,

            /// <summary>
            /// The player is Behind Live Window.
            /// </summary>
            BehindLiveWindow = 704,

            /// <summary>
            /// Media player is paused because device is in sleep or standby state.
            /// </summary>
            PowerStatePause = 705,

            /// <summary>
            /// Media player has resumed playback because device has returned from sleep or standby state.
            /// </summary>
            PowerStateResume = 706,

            /// <summary>
            /// Duration in milliseconds of buffered content.
            /// </summary>
            BufferedDuration = 707,

            /// <summary>
            /// Bad interleaving means that a media has been improperly interleaved.
            /// </summary>
            BadInterleaving = 800,

            /// <summary>
            /// The media is not seekable e.g live stream.
            /// </summary>
            NotSeekable = 801,

            /// <summary>
            /// New media metadata is available.
            /// </summary>
            MetadataUpdate = 802,

            /// <summary>
            /// Media timed text error.
            /// </summary>
            TimedTextError = 900,
        }

        /// <summary>
        /// Type of DRM key.
        /// <c>MLMediaDRMKeyType</c> from <c>ml_media_drm.h</c>.
        /// </summary>
        public enum DRMKeyType : int
        {
            /// <summary>
            /// This key request type specifies that the keys will be for online use, they will.
            /// not be saved to the device for subsequent use when the device is not connected to a network.
            /// </summary>
            Streaming = 1,

            /// <summary>
            /// This key request type specifies that the keys will be for offline use, they
            /// will be saved to the device for use when the device is not connected to a network.
            /// </summary>
            Offline = 2,

            /// <summary>
            /// This key request type specifies that previously saved offline keys should be released.
            /// </summary>
            Release = 3,
        }

        /// <summary>
        /// Types of DRM track.
        /// <c>MediaPlayerDRMIndex</c> from <c>ml_mediaplayer_plugin_common.h</c>.
        /// </summary>
        public enum DRMTrack : uint
        {
            /// <summary>
            /// Indicates the DRM track is a video.
            /// </summary>
            Video = 0,

            /// <summary>
            /// Indicates the DRM track is audio.
            /// </summary>
            Audio
        }

        /// <summary>
        /// MediaTrack types returned by MLMediaPlayerGetTrackType(...).
        /// <c>MLMediaPlayerTrackType</c> from <c>ml_media_player.h</c>.
        /// </summary>
        public enum PlayerTrackType : uint
        {
            /// <summary>
            /// Unspecified type.
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Indicates the track is a video.
            /// </summary>
            Video = 1,

            /// <summary>
            /// Indicates the track is audio.
            /// </summary>
            Audio = 2,

            /// <summary>
            /// Indicates the track is timed text.
            /// </summary>
            TimedText = 3,

            /// <summary>
            /// Indicates the track is subtitle.
            /// </summary>
            Subtitle = 4,

            /// <summary>
            /// Indicates the track is metadata.
            /// </summary>
            Metadata = 5
        }

        /// <summary>
        /// CEA608 caption color code.
        /// <c>MLCea608CaptionColor</c> from <c>ml_media_cea608_caption.h</c>.
        /// </summary>
        public enum Cea608CaptionColor : uint
        {
            /// <summary>
            /// CEA608 caption color is white.
            /// </summary>
            White = 0,

            /// <summary>
            /// CEA608 caption color is green.
            /// </summary>
            Green = 1,

            /// <summary>
            /// CEA608 caption color is blue.
            /// </summary>
            Blue = 2,

            /// <summary>
            /// CEA608 caption color is cyan.
            /// </summary>>
            Cyan = 3,

            /// <summary>
            /// CEA608 caption color is red.
            /// </summary>
            Red = 4,

            /// <summary>
            /// CEA608 caption color is yellow.
            /// </summary>
            Yellow = 5,

            /// <summary>
            /// CEA608 caption color is magenta.
            /// </summary>
            Magenta = 6,

            /// <summary>
            /// CEA608 caption color is invalid.
            /// </summary>
            Invalid = 7
        }

        /// <summary>
        /// CEA608 caption style code.
        /// <c>MLCea608CaptionStyle</c> from <c>ml_media_cea608_caption.h</c>.
        /// </summary>
        public enum Cea608CaptionStyle : uint
        {
            /// <summary>
            /// CEA608 caption style code is normal.
            /// </summary>
            Normal = 0x00000000,

            /// <summary>
            /// CEA608 caption style code is italics.
            /// </summary>
            Italics = 0x00000001,

            /// <summary>
            /// CEA608 caption style code is underline.
            /// </summary>
            Underline = 0x00000002
        }

        /// <summary>
        /// CEA608 caption Dimension constants.
        /// <c>MLCea608CaptionDimension</c> from <c>ml_media_cea608_caption.h</c>.
        /// </summary>
        public enum Cea608CaptionDimension : int
        {
            /// <summary>
            /// Max number of rows.
            /// </summary>
            MaxRows = 15,

            /// <summary>
            /// Max number of columns.
            /// </summary>
            MaxCols = 32,

            /// <summary>
            /// Max number of plus 2.
            /// </summary>
            MaxRowsPlus2 = 17,

            /// <summary>
            /// Max number of columns plus 2.
            /// </summary>
            MaxColsPlus2 = 34
        }

        /// <summary>
        /// CEA708 Caption Pen Size constants.
        /// <c>MLCea708CaptionPenSize</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        public enum Cea708CaptionPenSize
        {
            /// <summary>
            /// Small pen size.
            /// </summary>
            Small     = 0,

            /// <summary>
            /// Standard pen size.
            /// </summary>
            Standard  = 1,

            /// <summary>
            /// Large pen size.
            /// </summary>
            Large     = 2,
        }

        /// <summary>
        /// CEA708 Caption Pen Offset constants.
        /// <c>MLCea708CaptionPenOffset</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        public enum Cea708CaptionPenOffset
        {
            /// <summary>
            /// Subscript offset.
            /// </summary>
            Subscript     = 0,

            /// <summary>
            /// Normal offset.
            /// </summary>
            Normal        = 1,

            /// <summary>
            /// Superscript offset.
            /// </summary>
            Superscript   = 2,
        }

        /// <summary>
        /// CEA708 Caption Emit Commands constants.
        /// <c>MLCea708CaptionEmitCommand</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        public enum Cea708CaptionEmitCommand : int
        {
            /// <summary>
            /// Buffer command.
            /// </summary>
            Buffer     = 1,

            /// <summary>
            /// Control command.
            /// </summary>
            Control    = 2,

            /// <summary>
            /// SetCurrentWindow tells the caption decoder which window the following commands describe:
            /// - SetWindowAttributes
            /// - SetPenAttributes
            /// - SetPenColor
            /// - SetPenLocation.
            /// If the window specified has not already been created with a DefineWindow command then,
            /// SetCurrentWindow and the window property commands can be safely ignored.
            /// </summary>
            CWX        = 3,

            /// <summary>
            /// ClearWindows clears all the windows specified in the 8 bit window bitmap.
            /// </summary>
            CLW        = 4,

            /// <summary>
            /// DisplayWindows displays all the windows specified in the 8 bit window bitmap.
            /// </summary>
            DSW        = 5,

            /// <summary>
            /// HideWindows hides all the windows specified in the 8 bit window bitmap.
            /// </summary>
            HDW        = 6,

            /// <summary>
            /// ToggleWindows hides all displayed windows, and displays all hidden windows specified in the 8 bit window bitmap.
            /// </summary>
            TGW        = 7,

            /// <summary>
            /// DeleteWindows deletes all the windows specified in the 8 bit window bitmap.
            /// If the current window, as specified by the last SetCurrentWindow command,
            /// is deleted then the current window becomes undefined and the window attribute commands
            /// should have no effect until after the next SetCurrentWindow or DefineWindow command.
            /// </summary>
            DLW        = 8,

            /// <summary>
            /// Delay suspends all processing of the current service, except for DelayCancel and Reset scanning.
            /// </summary>
            DLY        = 9,

            /// <summary>
            /// DelayCancel terminates any active delay and resumes normal command processing. DelayCancel should be scanned for during a Delay.
            /// </summary>
            DLC        = 10,

            /// <summary>
            /// Reset deletes all windows, cancels any active delay, and clears the buffer before the Reset command. Reset should be scanned for during a Delay.
            /// </summary>
            RST        = 11,

            /// <summary>
            /// The SetPenAttributes command specifies how certain attributes of subsequent characters are to be rendered in the current window, until the next SetPenAttributes command.
            /// </summary>
            SPA        = 12,

            /// <summary>
            /// SetPenColor sets the foreground, background, and edge color for the subsequent characters.
            /// </summary>
            SPC        = 13,

            /// <summary> SetPenLocation sets the location of for the next bit of appended text in the current window. It has two parameters, row and column.
            /// </summary>
            SPL        = 14,

            /// <summary>
            /// SetWindowAttributes Sets the window attributes of the current window.
            /// </summary>
            SWA        = 15,

            /// <summary>
            /// DefineWindow0-7 creates one of the eight windows used by a caption decoder.
            /// </summary>
            DFX        = 16
        }

        /// <summary>
        /// CEA708 Caption Emit Command Control constants.
        /// <c>MLCea708CaptionEmitCommandControl</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        public enum Cea708CaptionEmitCommandControl : int
        {
            /// <summary>
            /// End of text.
            /// </summary>
            ETX    = 0x03,

            /// <summary>
            /// Back space.
            /// </summary>
            BS     = 0x08,

            /// <summary>
            /// This code is equivalent to CEA708 CLW command
            /// </summary>
            FF     = 0x0c,

            /// <summary>
            /// Carriage return.
            /// </summary>
            HCR    = 0x0e
        }

        /// <summary>
        /// CEA708 Caption Color Opacity constants.
        /// <c>MLCea708CaptionColorOpacity</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        public enum Cea708CaptionColorOpacity : int
        {
            /// <summary>
            /// Solid opacity.
            /// </summary>
            Solid           = 0,

            /// <summary>
            /// Flashing opacity.
            /// </summary>
            Flash           = 1,

            /// <summary>
            /// Translucent opacity.
            /// </summary>
            Translucent     = 2,

            /// <summary>
            /// Transparent opacity.
            /// </summary>
            Transparent     = 3
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// MediaShared types to be set in MLMediaPlayerSetSharingInfo().
        /// </summary>
        public enum SharedType : int
        {
            /// <summary>
            /// Unknown sharing.
            /// </summary>
            Unknown   = -1,

            /// <summary>
            /// No sharing or stop sharing if shared.
            /// </summary>
            None      = 0,

            /// <summary>
            /// Share as initiator.
            /// </summary>
            Initiator = 1,

            /// <summary>
            /// Share as follower.
            /// </summary>
            Follower  = 2,
        }

        /// <summary>
        /// Gets or sets when media player finds new closed caption track information.
        /// First parameter is a dictionary of TrackData objects.
        /// </summary>
        public SubtitleFoundDelegate OnSubtitleTracksFound
        {
            get
            {
                return this.onSubtitleTracksFound;
            }

            set
            {
                this.onSubtitleTracksFound = value;
                if (this.subtitleTracksCache != null && this.onSubtitleTracksFound != null)
                {
                    this.onSubtitleTracksFound(this.subtitleTracksCache);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Media Player's video rendering material. This will default to
        /// "Unlit/Texture" if not set. If stereo rendering is to be supported
        /// then the "_VideoStereoMode" property must be correctly implemented.
        /// </summary>
        public Material VideoRenderMaterial
        {
            get
            {
                return this.videoRenderMaterial;
            }

            set
            {
                if (!this.prepared)
                {
                    this.videoRenderMaterial = value;
                    //// Apply the material and get the renderer.material instance of it
                    this.ApplyVideoRenderMaterial("VideoRenderMaterial");
                    this.TryApplyStereoMode("VideoRenderMaterial");
                }
                else
                {
                    MLPluginLog.Warning("MLMediaPlayer.VideoRenderMaterial cannot be set after successful PrepareVideo()");
                }
            }
        }

        /// <summary>
        /// Gets or sets the Media Player's video rendering mode. This can be used to specify if the video
        /// frame contains separate left and right eye video frames combined using the
        /// Over/Under or Side-by-Side methods.
        /// Mono is the default which specifies no stereo rendering.
        /// </summary>
        public VideoStereoMode StereoMode
        {
            get
            {
                return this.stereoMode;
            }

            set
            {
                this.stereoMode = value;
                this.ApplyVideoRenderMaterial("StereoMode");
                this.TryApplyStereoMode("StereoMode");
            }
        }

        /// <summary>
        /// Gets or sets the Media Player's source video URL. This can be a local streaming asset or web content
        /// After changing the Video Source, you must call PrepareVideo() before calling Play()
        /// </summary>
        public string VideoSource { get; set; }

        /// <summary>
        /// Gets or sets the Media Player's license server URL. If left blank, will attempt to use default
        /// server returned by the DRM library.
        /// </summary>
        public string LicenseServer
        {
            get
            {
                return this.licenseServer;
            }

            set
            {
                this.licenseServer = value;
                if (this.videoPlayerImpl != null)
                {
                    this.videoPlayerImpl.SetLicenseServer(this.licenseServer);
                }
            }
        }

        /// <summary>
        /// Gets or sets custom header key-value pairs to use in addition to default of <c>"User-Agent : Widevine CDM v1.0"</c>
        /// when performing key request to the DRM license server.
        /// </summary>
        public Dictionary<string, string> CustomLicenseHeaderData
        {
            get
            {
                return this.customLicenseHeaderData;
            }

            set
            {
                this.customLicenseHeaderData = value;
                if (this.videoPlayerImpl != null)
                {
                    this.videoPlayerImpl.SetCustomLicenseHeaderData(this.customLicenseHeaderData);
                }
            }
        }

        /// <summary>
        /// Gets or sets custom key request key-value pair parameters used when generating default key request.
        /// </summary>
        public Dictionary<string, string> CustomLicenseMessageData
        {
            get
            {
                return this.customLicenseMessageData;
            }

            set
            {
                this.customLicenseMessageData = value;
                if (this.videoPlayerImpl != null)
                {
                    this.videoPlayerImpl.SetCustomLicenseMessageData(this.customLicenseMessageData);
                }
            }
        }

        /// <summary>
        /// Gets or sets a function to generate a license request body.
        /// First parameter is the DRM track being processed, either Video or Audio.
        /// Second parameter is the byte[] containing the default generated request.
        /// Return is the byte[] which will be used as the body to the license request.
        /// </summary>
        public MediaPlayerCustomLicenseDelegate CustomLicenseRequestBuilder
        {
            get
            {
                return this.customLicenseRequestBuilder;
            }

            set
            {
                this.customLicenseRequestBuilder = value;
                if (this.videoPlayerImpl != null)
                {
                    this.videoPlayerImpl.SetCustomLicenseRequestBuilder(this.customLicenseRequestBuilder);
                }
            }
        }

        /// <summary>
        /// Gets or sets a function to parse license response.
        /// First parameter is the DRM track being processed, either Video or Audio.
        /// Second parameter is the byte[] containing the response we received from the license server.
        /// Return is the byte[] which will be the raw license data, base64 decoding if necessary.
        /// </summary>
        public MediaPlayerCustomLicenseDelegate CustomLicenseResponseParser
        {
            get
            {
                return this.customLicenseResponseParser;
            }

            set
            {
                this.customLicenseResponseParser = value;
                if (this.videoPlayerImpl != null)
                {
                    this.videoPlayerImpl.SetCustomLicenseResponseParser(this.customLicenseResponseParser);
                }
            }
        }

        /// <summary>
        /// Gets a unique identifier for this specific MLMediaPlayer object.
        /// </summary>
        public int MediaPlayerID { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this MLMediaPlayer has been prepared.
        /// </summary>
        public bool IsPrepared
        {
            get
            {
                return this.prepared;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the video is looping.
        /// </summary>
        public bool IsLooping
        {
            get
            {
                return this.looping;
            }

            set
            {
                this.looping = value;
                if (this.prepared)
                {
                    this.videoPlayerImpl.SetLooping(this.looping, this.MediaPlayerID);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the video is currently playing.
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Gets the current percentage position through the video, useful for a UI to update a timeline.
        /// </summary>
        public float AnimationPosition
        {
            get
            {
                // This is used by the ui to properly update the timeline when it is shown.
                if (this.prepared && this.durationMs != 0)
                {
                    return (float)this.videoPlayerImpl.GetPositionMilliseconds(this.MediaPlayerID) / (float)this.durationMs;
                }
                else
                {
                    return 0.0f;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current frame drop threshold in milliseconds, default value is 0.
        /// Video frames older than this value are dropped, setting this to 0 will never drop frames.
        /// </summary>
        public ulong FrameDropThresholdMs
        {
            get
            {
                return this.videoPlayerImpl.GetFrameDropThresholdMs(this.MediaPlayerID);
            }

            set
            {
                this.videoPlayerImpl.SetFrameDropThresholdMs(this.MediaPlayerID, value);
            }
        }

        /// <summary>
        /// Sets sharing information for the media player being shared and enables only functionality
        /// for synchronize the content playback. Follower setting can only be set before video has been prepared.
        /// </summary>
        /// <param name="sharedType">The shared type for the current media player from enum SharedType.</param>
        /// <param name="sessionID">Unique Identifier of the sharing session in which the media players are being shared.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> for invalid parameters.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> for invalid settings.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> shared information is successfully set.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> The operation failed with an unspecified error.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
        /// </returns>
        public MLResult SetSharingInfo(SharedType sharedType, string sessionID)
        {
            if (string.IsNullOrEmpty(sessionID))
            {
                MLPluginLog.ErrorFormat("MLMediaPlayer.SetSharingInfo failed because session id is null or empty.");
                return MLResult.Create(MLResult.Code.InvalidParam);
            }

            if (sharedType == MLMediaPlayer.SharedType.Unknown)
            {
                MLPluginLog.ErrorFormat("MLMediaPlayer.SetSharingInfo failed because share type is invalid.");
                return MLResult.Create(MLResult.Code.MediaGenericInvalidOperation);
            }

            if (sharedType == MLMediaPlayer.SharedType.Follower && this.IsPrepared)
            {
                MLPluginLog.ErrorFormat("MLMediaPlayer.SetSharingInfo failed because Follower needs be set before video is prepared.");
                return MLResult.Create(MLResult.Code.MediaGenericInvalidOperation);
            }

            return this.videoPlayerImpl.SetSharingInfo(this.MediaPlayerID, sharedType, sessionID, this.IsPrepared);
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
        public MLResult SetID(int ID)
        {
            if (this.IsPrepared)
            {
                MLPluginLog.ErrorFormat("MLMediaPlayer.SetID failed because the video has already been prepared.");
                return MLResult.Create(MLResult.Code.MediaGenericInvalidOperation);
            }
            else if(ID <= 0)
            {
                MLPluginLog.ErrorFormat("MLMediaPlayer.SetID failed because the ID needs to be greater than zero.");
                return MLResult.Create(MLResult.Code.InvalidParam);
            }

            this.videoPlayerImpl.SetID(this.MediaPlayerID, ID);
            return MLResult.Create(MLResult.Code.Ok);
        }

        /// <summary>
        /// After setting VideoSource, prepare the video and start playback.
        /// </summary>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to an internal error prevented MediaPlayer from starting.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if failed due to media player not properly initialized.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed due to calling from the wrong state.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.PrivilegeDenied</c> if attempting to access web content without appropriate network privileges.
        /// </returns>
        public MLResult PrepareVideo()
        {
            MLResult result;
            MLPluginLog.Debug(this.VideoSource);

            if (this.resetting)
            {
                string message = string.Format("MLMediaPlayer.PrepareVideo failed, cannot prepare while player is resetting");

                MLPluginLog.Warning(message);
                return MLResult.Create(MLResult.Code.MediaGenericNoInit, message);
            }

            if (this.meshRenderer == null)
            {
                string message = string.Format("MLMediaPlayer.PrepareVideo failed, no valid MeshRenderer found");

                MLPluginLog.Warning(message);
                return MLResult.Create(MLResult.Code.MediaGenericNoInit, message);
            }

            this.ApplyVideoRenderMaterial("PrepareVideo");
            this.CreateAndApplyBackdropTexture(1280, 720);
            this.TryApplyStereoMode("PrepareVideo");

            this.videoPlayerImpl.SetLicenseServer(this.LicenseServer);
            this.videoPlayerImpl.SetCustomLicenseHeaderData(this.CustomLicenseHeaderData);
            this.videoPlayerImpl.SetCustomLicenseMessageData(this.CustomLicenseMessageData);
            this.videoPlayerImpl.SetCustomLicenseRequestBuilder(this.CustomLicenseRequestBuilder);
            this.videoPlayerImpl.SetCustomLicenseResponseParser(this.CustomLicenseResponseParser);

            // Create the media player
            result = this.videoPlayerImpl.CreateStreamingMediaPlayer(transform.gameObject, this.VideoSource, this.MediaPlayerID);

            return result;
        }

        /// <summary>
        /// Initiate asynchronous reset of media player. Use <see cref="OnResetCompleted"/> event to know when reset completes,
        /// the player will be in a pre-prepared state. This method can be called anytime except while asynchronously preparing.
        /// </summary>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if successful.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to an internal error prevented MediaPlayer from resetting.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if failed due to media player not properly initialized.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed due to calling from the wrong state.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if internally passed media player handle was not found.
        /// </returns>
        public MLResult ResetAsync()
        {
            MLResult result = this.videoPlayerImpl.ResetAsync(this.MediaPlayerID);
            if (result.IsOk)
            {
                this.IsPlaying = false;
                this.wasPlayingBeforeApplicationPaused = false;
                this.prepared = false;
                this.resetting = true;
            }

            return result;
        }

        /// <summary>
        /// Gets active audio channel count.
        /// </summary>
        /// <param name="outAudioChannelCount">Return channel count.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if operation failed with an unspecified internal error.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> Method was called from the wrong state. Can only be called after one of the setDataSource methods.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> MediaPlayer was not properly built or initialized.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.NotImplemented</c> No audio track found.
        /// </returns>
        public MLResult GetAudioChannelCount(out int outAudioChannelCount)
        {
            return this.videoPlayerImpl.GetAudioChannelCount(this.MediaPlayerID, out outAudioChannelCount);
        }

        /// <summary>
        /// Sets spatial audio state.
        /// </summary>
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
        public MLResult SetSpatialAudio(bool isEnabled)
        {
            return this.videoPlayerImpl.SetSpatialAudio(this.MediaPlayerID, isEnabled);
        }

        /// <summary>
        /// Gets spatial audio state.
        /// </summary>
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
        public MLResult GetSpatialAudio(out bool outIsEnabled)
        {
            return this.videoPlayerImpl.GetSpatialAudio(this.MediaPlayerID, out outIsEnabled);
        }

        /// <summary>
        /// Sets world position of requested audio channel.
        /// </summary>
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
        public MLResult SetAudioChannelPosition(AudioChannel channel, Vector3 position)
        {
            return this.videoPlayerImpl.SetAudioChannelPosition(this.MediaPlayerID, channel, position);
        }

        /// <summary>
        /// Gets world position of requested audio channel.
        /// </summary>
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
        public MLResult GetAudioChannelPosition(AudioChannel channel, out Vector3 outPosition)
        {
            return this.videoPlayerImpl.GetAudioChannelPosition(this.MediaPlayerID, channel, out outPosition);
        }

        /// <summary>
        /// Seek to a relative time in milliseconds.
        /// </summary>
        /// <param name="timeMS">The time increment to jump to.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to an internal error prevented MediaPlayer from starting.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if failed due to media player not properly initialized.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaOutOfRange</c> if failed due to attempting seek on a live video.
        /// </returns>
        public MLResult Seek(int timeMS)
        {
            if (!this.prepared)
            {
                string message = string.Format("MLMediaPlayer.Seek failed attempting to seek to {0} milliseconds on a video that has not been prepared, URL is {1}", timeMS, this.VideoSource);

                MLPluginLog.Warning(message);
                return MLResult.Create(MLResult.Code.MediaGenericNoInit, message);
            }

            if (this.durationMs == 0)
            {
                string message = string.Format("MLMediaPlayer.Seek is not supported on live video, URL is {0}", this.VideoSource);

                MLPluginLog.Warning(message);
                return MLResult.Create(MLResult.Code.MediaOutOfRange, message);
            }

            MLPluginLog.DebugFormat("Onseek mediaPlayerID {0}, playing is {1}", this.MediaPlayerID, this.IsPlaying);

            // Jump backwards or forwards in the video by 'time'
            int position = this.videoPlayerImpl.GetPositionMilliseconds(this.MediaPlayerID);
            position = Mathf.Clamp(position + timeMS, 0, this.durationMs);

            MLResult result = this.videoPlayerImpl.Seek(position, this.MediaPlayerID);
            if (!result.IsOk)
            {
                MLPluginLog.Warning("MLMediaPlayer.Seek implementation failed to seek. Reason: " + result);
                return result;
            }

            float percent = (float)position / (float)this.durationMs;
            //// Update user interface.
            if (this.OnSeekStarted != null)
            {
                this.OnSeekStarted(percent);
            }

            return result;
        }

        /// <summary>
        /// Seek to a specified percentage through the video.
        /// </summary>
        /// <param name="percent">Percent time to seek to [0.0 -> 1.0].</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to an internal error prevented MediaPlayer from starting.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if failed due to media player not properly initialized.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaOutOfRange</c> if failed due to attempting seek on a live video.
        /// </returns>
        public MLResult Seek(float percent)
        {
            if (!this.prepared)
            {
                string message = string.Format("MLMediaPlayer.Seek failed attempting to seek to {0} percent on a video that has not been prepared, URL is {1}", percent, this.VideoSource);

                MLPluginLog.Warning(message);
                return MLResult.Create(MLResult.Code.MediaGenericNoInit, message);
            }

            if (this.durationMs == 0)
            {
                string message = string.Format("MLMediaPlayer.Seek is not supported on live video, URL is {0}", this.VideoSource);

                MLPluginLog.Warning(message);
                return MLResult.Create(MLResult.Code.MediaOutOfRange, message);
            }

            percent = Mathf.Clamp01(percent);
            int time = (int)((float)this.durationMs * percent);

            MLPluginLog.DebugFormat("MLMediaPlayer.Seek to {0}% [{1} ms] mediaPlayerID {2}, playing is {3}", percent, time, this.MediaPlayerID, this.IsPlaying);

            MLResult result = this.videoPlayerImpl.Seek(time, this.MediaPlayerID);
            if (!result.IsOk)
            {
                MLPluginLog.Warning("MLMediaPlayer.Seek implementation failed to seek. Reason: " + result);
                return result;
            }

            // Update ui
            if (this.OnSeekStarted != null)
            {
                this.OnSeekStarted(percent);
            }

            return result;
        }

        /// <summary>
        /// Set the volume of the media player.
        /// </summary>
        /// <param name="volume">Volume in range [0.0 -> 1.0]</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to an internal error prevented MediaPlayer from starting.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if failed due to media player not properly initialized.
        /// </returns>
        public MLResult SetVolume(float volume)
        {
            if (!this.prepared)
            {
                string message = string.Format("MLMediaPlayer.SetVolume failed attempting to set volume to {0} on a video that has not been prepared, URL is {1}", volume, this.VideoSource);

                MLPluginLog.Warning(message);
                return MLResult.Create(MLResult.Code.MediaGenericNoInit, message);
            }

            // Set the volume level of the player
            return this.videoPlayerImpl.SetVolume(volume, this.MediaPlayerID);
        }

        /// <summary>
        /// Get the elapsed time in milliseconds.
        /// </summary>
        /// <returns>
        /// Integer of milliseconds passed.
        /// </returns>
        public int GetElapsedTimeMs()
        {
            if (!this.prepared)
            {
                MLPluginLog.WarningFormat("MLMediaPlayer.GetElapsedTimeMs failed attempting to get elapsed time on a video that has not been prepared, URL is {0}", this.VideoSource);
                return 0;
            }

            return this.videoPlayerImpl.GetPositionMilliseconds(this.MediaPlayerID);
        }

        /// <summary>
        /// Get the duration of the video in milliseconds.
        /// </summary>
        /// <returns>
        /// Integer of milliseconds total duration.
        /// </returns>
        public int GetDurationMs()
        {
            if (!this.prepared)
            {
                MLPluginLog.WarningFormat("MLMediaPlayer.GetDurationMs failed attempting to get duration of a video that has not been prepared, URL is {0}", this.VideoSource);
                return 0;
            }

            return this.durationMs;
        }

        /// <summary>
        /// Play the video
        /// </summary>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if failed due to media player not properly initialized.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed due to calling from the wrong state.
        /// </returns>
        public MLResult Play()
        {
            MLResult result;

            if (!this.prepared)
            {
                result = MLResult.Create(MLResult.Code.MediaGenericInvalidOperation, "MLMediaPlayer.Play called on a video that has not been prepared.");
                MLPluginLog.WarningFormat("MLMediaPlayer.Play failed. Reason: {0}", result);
                return result;
            }

            if (!this.IsPlaying)
            {
                result = this.videoPlayerImpl.Play(this.MediaPlayerID);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayer.Play failed to play video. Reason: {0}", result);
                }

                this.IsPlaying = result.IsOk;
                if (this.OnPlay != null)
                {
                    this.OnPlay(this.durationMs);
                }
            }
            else
            {
                result = MLResult.Create(MLResult.Code.Ok, "Already playing");
            }

            return result;
        }

        /// <summary>
        /// Pause the video
        /// </summary>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if failed due to media player not properly initialized.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed due to calling from the wrong state.
        /// </returns>
        public MLResult Pause()
        {
            MLResult result;

            if (!this.prepared)
            {
                result = MLResult.Create(MLResult.Code.MediaGenericInvalidOperation, "MLMediaPlayer.Pause called on a video that has not been prepared.");
                MLPluginLog.WarningFormat("MLMediaPlayer.Pause failed. Reason: {0}", result);
                return result;
            }

            if (this.IsPlaying)
            {
                result = this.videoPlayerImpl.PauseVideo(this.MediaPlayerID);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayer.Pause failed to pause video. Reason: {0}", result);
                }

                this.IsPlaying = !result.IsOk;
                if (this.OnPause != null)
                {
                    this.OnPause();
                }
            }
            else
            {
                result = MLResult.Create(MLResult.Code.Ok, "Not playing");
            }

            return result;
        }

        /// <summary>
        /// Stop the video and return to beginning.
        /// </summary>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if failed due to media player not properly initialized.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed due to calling from the wrong state.
        /// </returns>
        public MLResult Stop()
        {
            MLResult result;

            if (this.prepared)
            {
                result = this.videoPlayerImpl.Stop(this.MediaPlayerID);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayer.Stop failed to stop video. Reason: {0}", result);
                }

                this.IsPlaying = !result.IsOk;
                if (this.OnStop != null)
                {
                    this.OnStop();
                }
            }
            else
            {
                result = MLResult.Create(MLResult.Code.MediaGenericInvalidOperation, "MLMediaPlayer.Stop called on a video that has not been prepared.");
                MLPluginLog.WarningFormat("MLMediaPlayer.Stop failed. Reason: {0}", result);
                return result;
            }

            return result;
        }

        /// <summary>
        /// Select specific track in the media
        /// </summary>
        /// <param name="trackID">The id of the track to be selected.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if media_player was invalid, track did not refer to a valid track number, was out of range or out_track_language was NULL.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if MediaPlayer was not properly built or initialized.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed due to calling from the wrong state, can only be called after one of the setDataSource methods.
        /// </returns>
        public MLResult SelectSubtitleTrack(uint trackID)
        {
            return this.videoPlayerImpl.SelectSubtitleTrack(this.MediaPlayerID, trackID);
        }

        /// <summary>
        /// Unselect specific track in the media
        /// </summary>
        /// <param name="trackID">The id of the track to be unselected.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.InvalidParam</c> if media_player was invalid, track did not refer to a valid track number, was out of range or out_track_language was NULL.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericNoInit</c> if MediaPlayer was not properly built or initialized.
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed due to calling from the wrong state, can only be called after one of the setDataSource methods.
        /// </returns>
        public MLResult UnselectSubtitleTrack(uint trackID)
        {
            return this.videoPlayerImpl.UnselectSubtitleTrack(this.MediaPlayerID, trackID);
        }

        /// <summary>
        /// Query a snapshot of all known track info for a given media player.
        /// This data can change from frame to frame.
        /// </summary>
        /// <returns>
        /// A Dictionary&lt;long, TrackData&gt; of all known tracks, empty if no track are known.
        /// </returns>
        public Dictionary<long, TrackData> GetAllTrackInfo()
        {
            return this.videoPlayerImpl.GetAllTrackInfo(this.MediaPlayerID);
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
            return this.videoPlayerImpl.RequestActivationKeyRequest(drmUUIDBytes, callback);
        }

        /// <summary>
        /// Returns the last known resolution for the media player.
        /// This data can change from frame to frame.
        /// </summary>
        /// <returns>
        /// A <c>Rect</c> of the resolution.
        /// </returns>
        public Rect GetResolution()
        {
            return this.videoPlayerImpl.GetResolution(this.MediaPlayerID);
        }

        /// <summary>
        /// The bitrate of the video track in kbps
        /// This data can change from frame to frame.
        /// It is recommended that you use the OnInfo event for best results
        /// </summary>
        /// <returns>
        /// The bitrate of the video, -1 on failure.
        /// </returns>
        public int GetVideoBitrate()
        {
            return this.videoPlayerImpl.GetVideoBitrate(this.MediaPlayerID);
        }

        /// <summary>
        /// Clean up on application shutdown
        /// </summary>
        public void OnDestroy()
        {
            UnityEngine.Object.Destroy(this.defaultBackdropTexture);

            if (this.videoRenderMaterial)
            {
                UnityEngine.Object.Destroy(this.videoRenderMaterial);
            }

            // Cleanup resources in the native plugin
            this.videoPlayerImpl.Cleanup(this.MediaPlayerID);
        }

        /// <summary>
        /// Gets the result string for a MLResult.Code.
        /// </summary>
        /// <param name="result">The MLResult.Code to be requested.</param>
        /// <returns>A pointer to the result string.</returns>
        internal static IntPtr GetResultString(MLResult.Code result)
        {
            try
            {
                return NativeBindings.MLMediaResultGetString(result);
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLMediaPlayer.GetResultString failed. Reason: API symbols not found");
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Utility method used to check and apply stereo mode property on the currently set VideoRenderMaterial material.
        /// </summary>
        /// <param name="callerName">The method or class calling the function.</param>
        private void TryApplyStereoMode(string callerName)
        {
            if (this.videoRenderMaterial)
            {
                if (this.videoRenderMaterial.HasProperty("_VideoStereoMode"))
                {
                    this.videoRenderMaterial.SetInt("_VideoStereoMode", (int)this.stereoMode);
                }
                else if (this.stereoMode != VideoStereoMode.Mono)
                {
                    //// Only print a warning if trying to do something other than Mono.
                    //// The Mono stereo render mode is the default which requires no addition logic
                    //// to implement while SideBySide and OverUnder require processing that is
                    //// implemented by supporting the _VideoStereoMode property this relies on.
                    //// The example implementation is distributed in the "StereoVideoRender.shader" shader.
                    MLPluginLog.WarningFormat("MLMediaPlayer.{0} failed to apply {1} StereoMode, material is missing \"_VideoStereoMode\" property", callerName, this.stereoMode);
                }
            }
        }

        /// <summary>
        /// Utility method used to create if necessary and apply the currently set VideoRenderMaterial material.
        /// </summary>
        /// <param name="callerName">The method or class calling the function.</param>
        private void ApplyVideoRenderMaterial(string callerName)
        {
            // If material has not been initialized, use default Unity shader "Unlit/Texture".
            if (this.videoRenderMaterial == null)
            {
                Shader videoShader = Shader.Find("Unlit/Texture");
                if (videoShader == null)
                {
                    MLPluginLog.ErrorFormat("MLMediaPlayer.{0} failed, unable to find shader \"Unlit/Texture\".", callerName);
                    return;
                }
                else
                {
                    this.videoRenderMaterial = new Material(videoShader);
                }
            }

            if (this.meshRenderer != null)
            {
                this.meshRenderer.material = this.videoRenderMaterial;

                // Accessing the renderer's material automatically instantiates it and makes it unique to this renderer, so keep a reference.
                this.videoRenderMaterial = this.meshRenderer.material;
            }
        }

        /// <summary>
        /// <c>MonoBehaviour</c> callback.
        /// </summary>
        private void Awake()
        {
            this.meshRenderer = this.GetComponent<MeshRenderer>();
            if (this.meshRenderer == null)
            {
                MLPluginLog.ErrorFormat("MLMediaPlayer.Awake failed to get required MeshRenderer, disabling MLMediaPlayer");
                this.enabled = false;
            }

            // Set this media player's ID using static counter
            this.MediaPlayerID = activePlayerCount++;
            if (Application.isEditor)
            {
                this.videoPlayerImpl = gameObject.AddComponent<MLMediaPlayerEditor>();
                MLMediaPlayerEditor mlmp = this.videoPlayerImpl as MLMediaPlayerEditor;
                mlmp.Initialize(this.MediaPlayerID, this.VideoEnded, this.StartPreparedVideo, this.HandleSeekCompleted);
            }
            else
            {
                this.videoPlayerImpl = gameObject.AddComponent<MLMediaPlayerLumin>();
                MLMediaPlayerLumin mlmp = this.videoPlayerImpl as MLMediaPlayerLumin;
                mlmp.Initialize(
                    this.MediaPlayerID,
                    this.VideoEnded,
                    this.StartPreparedVideo,
                    this.UpdateBufferingUI,
                    this.HandleVideoError,
                    this.HandleVideoInfo,
                    this.HandleSeekCompleted,
                    this.HandleVideoSizeChanged,
                    this.HandleResetCompleted,
                    this.HandleSubtitleTracksFound,
                    this.HandleSubtitleDataReceived,
                    this.HandleSubtitle708EventReceived,
                    this.HandleTimedTextDataReceived);
            }

            Application.quitting += this.Quit;
        }

        /// <summary>
        /// This function will stop the video when the application is about to quit
        /// this is to work around the couple frames of audio heard when trying to
        /// close a paused app from the Universe.
        /// </summary>
        private void Quit()
        {
            // Only attempt to stop if the video has been prepared.
            if (this.prepared)
            {
                this.Stop();
            }
        }

        /// <summary>
        /// Create and set texture with given dimension.
        /// Default black texture to be displayed until a video starts.
        /// </summary>
        /// <param name="width">Width of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        private void CreateAndApplyBackdropTexture(int width, int height)
        {
            // Apply a basic black texture to be displayed until a video starts up.
            if (this.defaultBackdropTexture != null)
            {
                UnityEngine.Object.Destroy(this.defaultBackdropTexture);
            }

            // Create texture with given dimensions.
            this.defaultBackdropTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            int size = width * height;
            Color[] colors = new Color[size];
            Color black = Color.black;
            for (int i = 0; i < size; ++i)
            {
                colors[i] = black;
            }

            this.defaultBackdropTexture.SetPixels(colors);
            this.defaultBackdropTexture.filterMode = FilterMode.Point;
            this.defaultBackdropTexture.Apply();

            this.videoRenderMaterial.SetTexture("_MainTex", this.defaultBackdropTexture);
        }

        /// <summary>
        /// Pause the media player when the app loses focus
        /// </summary>
        /// <param name="isPaused">True when leaving the app, False when re-entering</param>
        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                // application is losing focus
                this.wasPlayingBeforeApplicationPaused = this.IsPlaying;
                if (this.IsPlaying)
                {
                    //// media player is playing
                    this.Pause();
                }
            }
            else if (!isPaused && this.wasPlayingBeforeApplicationPaused)
            {
                // application regained focus and was playing before leaving
                this.Play();
            }
        }

        /// <summary>
        /// Cache the last know subtitle track data since we can't be sure the user set up.
        /// The callback before we send the event.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        /// <param name="subtitleTracks">The subtitle track information to cache.</param>
        private void CacheSubtitleTracks(object mediaPlayerID, object subtitleTracks)
        {
            this.subtitleTracksCache = (Dictionary<long, TrackData>)subtitleTracks;
        }

        /// <summary>
        /// Invoked after video has completed playback.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        private void VideoEnded(object mediaPlayerID)
        {
            // this will get called by event system for all screens so filter for the one we want
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return;
            }

            // Video ended, if not looping then indicate we are no longer playing
            if (!this.IsLooping)
            {
                this.IsPlaying = false;
            }

            if (this.OnEnded != null)
            {
                this.OnEnded();
            }
        }

        /// <summary>
        /// Invoked when media player encounters an error.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        /// <param name="error">The type of error as MLMediaPlayerError.</param>
        /// <param name="errorString">Information about the error as MLMediaError</param>
        private void HandleVideoError(object mediaPlayerID, object error, object errorString)
        {
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return;
            }

            MLPluginLog.DebugFormat("MLMediaPlayer MediaPlayerID {0} Player had an error of {1} with error string: {2}", mediaPlayerID, error, errorString);

            if (this.OnMediaError != null)
            {
                this.OnMediaError((MLResult.Code)error, (string)errorString);
            }
        }

        /// <summary>
        /// Invoked when media player has informational events available.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        /// <param name="info">the type of information event as <c>MLMediaPlayerInfo</c>.</param>
        /// <param name="extra">any extra info the informational event may define:
        /// When info is MLMediaPlayerInfo.NetworkBandwidth, this holds bandwidth in kbps.
        /// It is 0 for others.</param>
        private void HandleVideoInfo(object mediaPlayerID, object info, object extra)
        {
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return;
            }

            MLPluginLog.DebugFormat("MLMediaPlayer MediaPlayerID {0} Player has reported informational event of {1} with extra data {2}", mediaPlayerID, (PlayerInfo)info, extra);

            if (this.OnInfo != null)
            {
                this.OnInfo((PlayerInfo)info, (int)extra);
            }
        }

        /// <summary>
        ///  When a seek completes, notifies with the current position in the media as a percentage.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        private void HandleSeekCompleted(object mediaPlayerID)
        {
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return;
            }

            MLPluginLog.DebugFormat("MLMediaPlayer MediaPlayerID {0} Player has completed a seek event.", mediaPlayerID);

            if (this.OnSeekCompleted != null)
            {
                int position = this.videoPlayerImpl.GetPositionMilliseconds(this.MediaPlayerID);
                float percent = (float)position / (float)this.durationMs;
                this.OnSeekCompleted(percent);
            }
        }

        /// <summary>
        /// When a video's resolution changes, notifies with the current resolution as a <c>Rect</c>.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        /// <param name="resolution">The current resolution as a <c>Rect</c></param>
        private void HandleVideoSizeChanged(object mediaPlayerID, object resolution)
        {
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return;
            }

            MLPluginLog.DebugFormat("MLMediaPlayer MediaPlayerID {0} Video resolution has changed.", mediaPlayerID);

            if (this.OnVideoSizeChanged != null)
            {
                this.OnVideoSizeChanged((Rect)resolution);
            }
        }

        /// <summary>
        /// Invoked when media player finds new closed caption track information.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        /// <param name="subtitleTracks">A dictionary of TrackData objects.</param>
        private void HandleSubtitleTracksFound(object mediaPlayerID, object subtitleTracks)
        {
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return;
            }

            this.CacheSubtitleTracks(mediaPlayerID, subtitleTracks);

            if (this.OnSubtitleTracksFound != null)
            {
                this.OnSubtitleTracksFound(this.subtitleTracksCache);
            }
        }

        /// <summary>
        ///  Invoked when CEA608 subtitle data is received from the media library.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        /// <param name="subtitleData"><c>MLCea608CaptionSegment</c> object that contains all the data given.</param>
        private void HandleSubtitleDataReceived(object mediaPlayerID, object subtitleData)
        {
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return;
            }

            if (this.OnSubtitle608DataFound != null)
            {
                this.OnSubtitle608DataFound((Cea608CaptionSegment)subtitleData);
            }
        }

        /// <summary>
        /// Invoked when a CEA708 subtitle event is received from the media library.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        /// <param name="subtitle708Event"><c>Cea708CaptionEvent</c> object that contains CEA708 event data given.</param>
        private void HandleSubtitle708EventReceived(object mediaPlayerID, object subtitle708Event)
        {
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return;
            }

            if (this.OnSubtitle708EventFound != null)
            {
                this.OnSubtitle708EventFound((Cea708CaptionEvent)subtitle708Event);
            }
        }

        /// <summary>
        /// Invoked when timed text data is received from media library, time is based on media time.
        /// First parameter is t.
        /// Second parameter is.
        /// Third parameter is time in milliseconds this text should stop being displayed.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        /// <param name="text">The textual data for this update.</param>
        /// <param name="start">Time in milliseconds this text should be displayed.</param>
        /// <param name="end">Time in milliseconds this text should stop being displayed.</param>
        private void HandleTimedTextDataReceived(object mediaPlayerID, object text, object start, object end)
        {
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return;
            }

            if (this.OnTimedTextDataFound != null)
            {
                this.OnTimedTextDataFound((string)text, (long)start, (long)end);
            }
        }

        /// <summary>
        /// Invoked when an asynchronous reset has completed. Video player will need to be prepared after this occurs.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        private void HandleResetCompleted(object mediaPlayerID)
        {
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return;
            }

            MLPluginLog.DebugFormat("MLMediaPlayer MediaPlayerID {0} Player has completed a reset.", mediaPlayerID);

            this.resetting = false;

            if (this.OnResetCompleted != null)
            {
                this.OnResetCompleted();
            }
        }

        /// <summary>
        /// When a video is buffering, notifies the percentage of completeness of the buffering.
        /// </summary>
        /// <param name="mediaPlayerID">ID for the media player.</param>
        /// <param name="percent">Amount of buggering completed.</param>
        private void UpdateBufferingUI(object mediaPlayerID, object percent)
        {
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return;
            }

            MLPluginLog.DebugFormat("MLMediaPlayer MediaPlayerID {0} Buffering {1}%", mediaPlayerID, percent);

            //// update buffering UI here or just move this handler and listeners to other scripts more responsible for UI.

            if (this.OnBufferingUpdate != null)
            {
                this.OnBufferingUpdate((int)percent * 0.01f);
            }
        }

        /// <summary>
        /// Starts Video playback for a prepared video.
        /// </summary>
        /// <param name="mediaPlayerID">ID of the media player.</param>
        /// <returns>
        /// <c>MLResult.Result</c> will be <c>MLResult.Code.Ok</c> if operation succeeded.
        /// </returns>
        private MLResult StartPreparedVideo(object mediaPlayerID)
        {
            // this will get called by event system for all screens so filter for the one we want
            if (this.MediaPlayerID != (int)mediaPlayerID)
            {
                return MLResult.Create(MLResult.Code.Ok, "Nothing to do");
            }

            MLPluginLog.DebugFormat("MLMediaPlayer.StartPreparedVideo, starting playback of video {0}", this.MediaPlayerID);

            // Create the texture and attach it to the screen
            if (!this.textureGenerated)
            {
                // Create and set material
                this.ApplyVideoRenderMaterial("StartPreparedVideo");
                this.TryApplyStereoMode("StartPreparedVideo");

                // The following call also sets "_MainTex" on currently set material.
                this.videoPlayerImpl.CreateTexture(this.meshRenderer, this.MediaPlayerID);
                this.textureGenerated = true;
            }

            int realVidWidth = this.videoPlayerImpl.GetWidth(this.MediaPlayerID);
            int realVidHeight = this.videoPlayerImpl.GetHeight(this.MediaPlayerID);

            float aspectRatio = (float)realVidWidth / (float)realVidHeight;

            if (this.OnFrameSizeSetup != null)
            {
                this.OnFrameSizeSetup(aspectRatio);
            }

            // Zero duration means the stream is a live video feed.
            this.durationMs = this.videoPlayerImpl.GetDurationMilliseconds(this.MediaPlayerID);

            this.prepared = true;

            // Apply cached looping state now that we are prepared.
            MLResult result = this.videoPlayerImpl.SetLooping(this.looping, this.MediaPlayerID);
            if (!result.IsOk)
            {
                MLPluginLog.ErrorFormat("MLMediaPlayer.StartPreparedVideo failed to set looping state. Reason: {0}", result);
            }

            // Play the video and update the ui.
            result = this.videoPlayerImpl.Play(this.MediaPlayerID);
            if (!result.IsOk)
            {
                MLPluginLog.ErrorFormat("MLMediaPlayer.StartPreparedVideo failed to play video. Reason: {0}", result);
            }

            this.IsPlaying = result.IsOk;
            if (this.IsPlaying == true)
            {
                if (this.OnPlay != null)
                {
                    this.OnPlay(this.durationMs);
                }
            }
            else
            {
                if (this.OnPause != null)
                {
                    this.OnPause();
                }
            }

            if (this.OnVideoPrepared != null)
            {
                this.OnVideoPrepared();
            }

            return result;
        }

        /// <summary>
        /// Data for a specific track.
        /// <c>MediaPlayerTrackData</c> from <c>ml_mediaplayer_plugin_common.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct TrackData
        {
            /// <summary>
            /// Track ID.
            /// </summary>
            public uint ID;

            /// <summary>
            /// the language of the media track.
            /// </summary>
            public string Language;

            /// <summary>
            /// Type of media player track.
            /// </summary>
            public PlayerTrackType Type;

            /// <summary>
            /// Mime format type of the track.
            /// </summary>
            public string MimeType;
        }

        /// <summary>
        /// Structure which contains an array of track data.
        /// <c>MediaPlayerTracks</c> from <c>ml_mediaplayer_plugin_common.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MediaPlayerTracks
        {
            /// <summary>
            /// Pointer to array of MediaPlayerTrackData.
            /// </summary>
            public IntPtr Tracks;

            /// <summary>
            /// Length of the array.
            /// </summary>
            public uint Length;
        }

        /// <summary>
        /// CEA608 caption line structure is an internal one used to marshal data.
        /// <c>MLCea608CaptionLineEx</c> from <c>ml_media_cea608_caption.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Cea608CaptionLineExInternal
        {
            /// <summary>
            /// Characters to be displayed.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Cea608CaptionDimension.MaxColsPlus2 * 2)]
            public ushort[] DisplayChars;

            /// <summary>
            /// CEA608 caption style and color.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Cea608CaptionDimension.MaxColsPlus2)]
            public IntPtr[] MidRowStyles;

            /// <summary>
            /// CEA608 caption preamble address code.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Cea608CaptionDimension.MaxColsPlus2)]
            public IntPtr[] PacStyles;
        }

        /// <summary>
        /// CEA608 caption segment structure.
        /// <c>MLCea608CaptionSegment</c> from <c>ml_media_cea608_caption.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Cea608CaptionSegmentInternal
        {
            /// <summary>
            /// Specific version.
            /// </summary>
            public uint Version;

            /// <summary>
            /// CEA608 caption line structure.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Cea608CaptionDimension.MaxRowsPlus2)]
            public IntPtr[] CCLines;
        }

        /// <summary>
        /// CEA608 caption segment structure.
        /// </summary>
        public struct Cea608CaptionSegment
        {
            /// <summary>
            /// CEA608 caption line structure.
            /// </summary>
            public Cea608CaptionLine[] CCLines;
        }

        /// <summary>
        /// CEA708 Caption Color.
        /// <c>MLCea708CaptionColor</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Cea708CaptionColor
        {
            /// <summary>
            /// Opacity setting.
            /// </summary>
            public Cea708CaptionColorOpacity Opacity;

            /// <summary>
            /// Red component.
            /// </summary>
            public int Red;

            /// <summary>
            /// Green component.
            /// </summary>
            public int Green;

            /// <summary>
            /// Blue component.
            /// </summary>
            public int Blue;
        }

        /// <summary>
        /// CEA708 Caption Pen Attributes.
        /// <c>MLCea708CaptionPenAttr</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Cea708CaptionPenAttr
        {
            /// <summary>
            /// Pen size.
            /// </summary>
            public Cea708CaptionPenSize PenSize;

            /// <summary>
            /// Pen offset.
            /// </summary>
            public Cea708CaptionPenOffset PenOffset;

            /// <summary>
            /// Text tag.
            /// </summary>
            public int TextTag;

            /// <summary>
            /// Font tag.
            /// </summary>
            public int FontTag;

            /// <summary>
            /// Edge type.
            /// </summary>
            public int EdgeType;

            /// <summary>
            /// Underline setting.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool Underline;

            /// <summary>
            /// Italic setting.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool Italic;
        }

        /// <summary>
        /// CEA708 Caption Pen Color.
        /// <c>MLCea708CaptionPenColor</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Cea708CaptionPenColor
        {
            /// <summary>
            /// Foreground color.
            /// </summary>
            public Cea708CaptionColor ForegroundColor;

            /// <summary>
            /// Background color.
            /// </summary>
            public Cea708CaptionColor BackgroundColor;

            /// <summary>
            /// Edge color.
            /// </summary>
            public Cea708CaptionColor EdgeColor;
        }

        /// <summary>
        /// CEA708 Caption Pen Location.
        /// <c>MLCea708CaptionPenLocation</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Cea708CaptionPenLocation
        {
            /// <summary>
            /// Pen row.
            /// </summary>
            public int Row;

            /// <summary>
            /// Pen column.
            /// </summary>
            public int Column;
        }

        /// <summary>
        /// CEA708 Caption Window Attributes.
        /// <c>MLCea708CaptionWindowAttr</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Cea708CaptionWindowAttr
        {
            /// <summary>
            /// Window fill color.
            /// </summary>
            public Cea708CaptionColor FillColor;

            /// <summary>
            /// Window border color.
            /// </summary>
            public Cea708CaptionColor BorderColor;

            /// <summary>
            /// Window border type.
            /// </summary>
            public int BorderType;

            /// <summary>
            /// Window word wrap setting.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool WordWrap;

            /// <summary>
            /// Window print direction.
            /// </summary>
            public int PrintDirection;

            /// <summary>
            /// Window scroll direction.
            /// </summary>
            public int ScrollDirection;

            /// <summary>
            /// Window justification setting.
            /// </summary>
            public int Justify;

            /// <summary>
            /// Window effect direction.
            /// </summary>
            public int EffectDirection;

            /// <summary>
            /// Window effect speed.
            /// </summary>
            public int EffectSpeed;

            /// <summary>
            /// Window display effect.
            /// </summary>
            public int DisplayEffect;
        }

        /// <summary>
        /// CEA708 Caption Window.
        /// <c>MLCea708CaptionWindow</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Cea708CaptionWindow
        {
            /// <summary>
            /// Window ID.
            /// </summary>
            public int ID;

            /// <summary>
            /// Window visible setting.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool Visible;

            /// <summary>
            /// Window row lock setting.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool RowLock;

            /// <summary>
            /// Window column lock setting.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool ColumnLock;

            /// <summary>
            /// Window Priority.
            /// </summary>
            public int Priority;

            /// <summary>
            /// Window relative positioning setting.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool RelativePositioning;

            /// <summary>
            /// Window anchor vertical.
            /// </summary>
            public int AnchorVertical;

            /// <summary>
            /// Window anchor horizontal.
            /// </summary>
            public int AnchorHorizontal;

            /// <summary>
            /// Window anchor ID.
            /// </summary>
            public int AnchorID;

            /// <summary>
            /// Window row count.
            /// </summary>
            public int RowCount;

            /// <summary>
            /// Window column count.
            /// </summary>
            public int ColumnCount;

            /// <summary>
            /// Window pen style.
            /// </summary>
            public int PenStyle;

            /// <summary>
            /// Window style.
            /// </summary>
            public int WindowStyle;
        }

        /// <summary>
        /// Internal CEA708 Caption Event.
        /// <c>MLCea708CaptionEvent</c> from <c>ml_media_cea708_caption.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Cea708CaptionEventInternal
        {
            /// <summary>
            /// Emitted CEA708 caption event type.
            /// </summary>
            public Cea708CaptionEmitCommand Type;

            /// <summary>
            /// Emitted CEA708 caption event specific data, based on the event type.
            /// If the type is <c>Cea708CaptionEmitCommand.Buffer</c>, Object will point to a null terminated string of maximum size <c>MLCea708CaptionEmitCommandBufferMaxSize</c> bytes.
            /// If the type is <c>Cea708CaptionEmitCommand.Control</c>, Object will point to one byte character.
            /// If the type is <c>Cea708CaptionEmitCommand.CWX</c>, Object will point to an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.CLW</c>, Object will point to an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.DSW</c>, Object will point to an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.HDW</c>, Object will point to an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.TGW</c>, Object will point to an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.DLW</c>, Object will point to an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.DLY</c>, Object will point to an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.DLC</c>, Object will be NULL.
            /// If the type is <c>Cea708CaptionEmitCommand.RST</c>, Object will be NULL.
            /// If the type is <c>Cea708CaptionEmitCommand.SPA</c>, Object will point to <c>Cea708CaptionPenAttr</c>.
            /// If the type is <c>Cea708CaptionEmitCommand.SPC</c>, Object will point to <c>Cea708CaptionPenColor</c>.
            /// If the type is <c>Cea708CaptionEmitCommand.SPL</c>, Object will point to <c>Cea708CaptionPenLocation</c>.
            /// If the type is <c>Cea708CaptionEmitCommand.SWA</c>, Object will point to <c>Cea708CaptionWindowAttr</c>.
            /// If the type is <c>Cea708CaptionEmitCommand.DFX</c>, Object will point to <c>Cea708CaptionWindow</c>.
            /// </summary>
            public IntPtr Object;
        }

        /// <summary>
        /// CEA708 Caption Event.
        /// </summary>
        public struct Cea708CaptionEvent
        {
            /// <summary>
            /// Emitted CEA708 caption event type.
            /// </summary>
            public Cea708CaptionEmitCommand Type;

            /// <summary>
            /// Emitted CEA708 caption event specific data, based on the event type.
            /// If the type is <c>Cea708CaptionEmitCommand.Buffer</c>, Object will be a string.
            /// If the type is <c>Cea708CaptionEmitCommand.Control</c>, Object will be a byte.
            /// If the type is <c>Cea708CaptionEmitCommand.CWX</c>, Object will be an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.CLW</c>, Object will be an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.DSW</c>, Object will be an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.HDW</c>, Object will be an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.TGW</c>, Object will be an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.DLW</c>, Object will be an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.DLY</c>, Object will be an integer.
            /// If the type is <c>Cea708CaptionEmitCommand.DLC</c>, Object will be null.
            /// If the type is <c>Cea708CaptionEmitCommand.RST</c>, Object will be null.
            /// If the type is <c>Cea708CaptionEmitCommand.SPA</c>, Object will be a <c>Cea708CaptionPenAttr</c>.
            /// If the type is <c>Cea708CaptionEmitCommand.SPC</c>, Object will be a <c>Cea708CaptionPenColor</c>.
            /// If the type is <c>Cea708CaptionEmitCommand.SPL</c>, Object will be a <c>Cea708CaptionPenLocation</c>.
            /// If the type is <c>Cea708CaptionEmitCommand.SWA</c>, Object will be a <c>Cea708CaptionWindowAttr</c>.
            /// If the type is <c>Cea708CaptionEmitCommand.DFX</c>, Object will be a <c>Cea708CaptionWindow</c>.
            /// </summary>
            public object Object;
        }

        /// <summary>
        /// CEA608 caption style and color.
        /// <c>MLCea608CaptionStyleColor</c> from <c>ml_media_cea608_caption.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class Cea608CaptionStyleColor
        {
            /// <summary>
            /// Gets or sets Specific style of the closed caption is to be displayed in.
            /// </summary>
            public uint Style { get; set; }

            /// <summary>
            /// Gets or sets Specific color of the closed caption is to be displayed in.
            /// </summary>
            public uint Color { get; set; }
        }

        /// <summary>
        /// CEA608 caption preamble address code.
        /// <c>MLCea608CaptionPAC</c> from <c>ml_media_cea608_caption.h</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class Cea608CaptionPAC
        {
            /// <summary>
            /// Gets or sets Specific color of the closed caption is to be displayed in.
            /// </summary>
            public Cea608CaptionStyleColor StyleColor { get; set; }

            /// <summary>
            /// Gets or sets The number of rows for closed caption text.
            /// </summary>
            public uint Row { get; set; }

            /// <summary>
            /// Gets or sets The number of columns for closed caption text.
            /// </summary>
            public uint Col { get; set; }
        }

        /// <summary>
        /// CEA608 caption line structure that contains managed data.
        /// </summary>
        public class Cea608CaptionLine
        {
            /// <summary>
            /// Gets or sets the String to be displayed
            /// </summary>
            public string DisplayString { get; set; }

            /// <summary>
            /// Gets or sets CEA608 caption style and color.
            /// </summary>
            public Cea608CaptionStyleColor[] MidRowStyles { get; set; }

            /// <summary>
            /// Gets or sets CEA608 caption preamble address code.
            /// </summary>
            public Cea608CaptionPAC[] PacStyles { get; set; }

            /// <summary>
            /// Gets byte array of UTF8 encoded characters to be displayed.
            /// </summary>
            [Obsolete("Please use Cea608CaptionLine.DisplayString instead.")]
            public byte[] DisplayChars
            {
                get
                {
                    return Encoding.UTF8.GetBytes(this.DisplayString);
                }
            }
        }

        #endif
    }
}
