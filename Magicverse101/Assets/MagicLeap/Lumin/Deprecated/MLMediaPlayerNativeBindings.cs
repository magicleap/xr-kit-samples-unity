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

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Indicates various  trigger various media player actions.
    /// </summary>
    [Obsolete("Please use MLMediaPlayer.PlayerInfo instead")]
    public enum MLMediaPlayerInfo
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
    };

    /// <summary>
    /// Type of DRM key.
    /// </summary>
    [Obsolete("Please use MLMediaPlayer.DRMKeyType instead")]
    public enum MLMediaDRMKeyType : int
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
    };

    /// <summary>
    /// Types of DRM track.
    /// </summary>
    [Obsolete("Please use MLMediaPlayer.DRMTrack instead")]
    public enum MLMediaDRMTrack : uint
    {
        /// <summary>
        /// Indicates the DRM track is a video.
        /// </summary>
        Video = 0,

        /// <summary>
        /// Indicates the DRM track is audio.
        /// </summary>
        Audio = 1
    }

    /// <summary>
    /// MediaTrack types returned by MLMediaPlayerGetTrackType(...).
    /// </summary>
    [Obsolete("Please use MLMediaPlayer.PlayerTrackType instead")]
    public enum MLMediaPlayerTrackType : uint
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
    /// <c>Cea608</c> caption color code.
    /// </summary>
    [Obsolete("Please use MLMediaPlayer.Cea608CaptionColor instead")]
    public enum MLCea608CaptionColor : uint
    {
        /// <summary>
        ///  <c>Cea608</c> caption color is white.
        /// </summary>
        White = 0,

        /// <summary>
        ///  <c>Cea608</c> caption color is green.
        /// </summary>
        Green = 1,

        /// <summary>
        ///  <c>Cea608</c> caption color is blue.
        /// </summary>
        Blue = 2,

        /// <summary>
        ///  <c>Cea608</c> caption color is cyan.
        /// </summary>>
        Cyan = 3,

        /// <summary>
        ///  <c>Cea608</c> caption color is red.
        /// </summary>
        Red = 4,

        /// <summary>
        ///  <c>Cea608</c> caption color is yellow.
        /// </summary>
        Yellow = 5,

        /// <summary>
        ///  <c>Cea608</c> caption color is magenta.
        /// </summary>
        Magenta = 6,

        /// <summary>
        ///  <c>Cea608</c> caption color is invalid.
        /// </summary>
        Invalid = 7
    }

    /// <summary>
    /// <c>Cea608</c> caption style code.
    /// </summary>
    [Obsolete("Please use MLMediaPlayer.Cea608CaptionStyle instead")]
    public enum MLCea608CaptionStyle : uint
    {
        /// <summary>
        /// <c>Cea608</c> caption style code is normal.
        /// </summary>
        Normal = 0x00000000,

        /// <summary>
        /// <c>Cea608</c> caption style code is italics.
        /// </summary>
        Italics = 0x00000001,

        /// <summary>
        /// <c>Cea608</c> caption style code is underline.
        /// </summary>
        Underline = 0x00000002
    }

    /// <summary>
    /// <c>Cea608</c> caption Dimension constants.
    /// </summary>
    [Obsolete("Please use MLMediaPlayer.Cea608CaptionDimension instead")]
    public enum MLCea608CaptionDimension : int
    {
        /// <summary>
        /// Max number of rows.
        /// </summary>
        MLCea608_CCMaxRows = 15,

        /// <summary>
        /// Max number of columns.
        /// </summary>
        MLCea608_CCMaxCols = 32,

        /// <summary>
        /// Max number of plus 2.
        /// </summary>
        MLCea608_CCMaxRowsPlus2 = 17,

        /// <summary>
        /// Max number of columns plus 2.
        /// </summary>
        MLCea608_CCMaxColsPlus2 = 34
    }

    #if PLATFORM_LUMIN
    /// <summary>
    /// Data for a specific track.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    [Obsolete("Please use MLMediaPlayer.TrackData instead")]
    public struct TrackData
    {
        /// <summary>
        /// Track ID.
        /// </summary>
        public uint id;

        /// <summary>
        /// the language of the media track.
        /// </summary>
        public string language;

        /// <summary>
        /// Type of media player track.
        /// </summary>
        public MLMediaPlayerTrackType type;

        /// <summary>
        /// Mime format type of the track.
        /// </summary>
        public string mimeType;
    }

    /// <summary>
    /// Structure which contains an array of track data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [Obsolete("Please use MLMediaPlayer.MediaPlayerTracks instead")]
    public struct MediaPlayerTracks
    {
        /// <summary>
        /// Pointer to array of MediaPlayerTrackData.
        /// </summary>
        public IntPtr tracks;

        /// <summary>
        /// Length of the array.
        /// </summary>
        public uint length;
    }

    /// <summary>
    /// <c>Cea608</c> caption style and color.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [Obsolete("Please use MLMediaPlayer.Cea608CaptionStyleColor instead")]
    public class MLCea608CaptionStyleColor
    {
        /// <summary>
        /// Specific style of the closed caption is to be displayed in.
        /// </summary>
        public uint style;

        /// <summary>
        /// Specific color of the closed caption is to be displayed in.
        /// </summary>
        public uint color;
    }

    /// <summary>
    /// <c>Cea608</c> caption preamble address code.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [Obsolete("Please use MLMediaPlayer.Cea608CaptionPAC instead")]
    public class MLCea608CaptionPAC
    {
        /// <summary>
        /// Specific color of the closed caption is to be displayed in.
        /// </summary>
        public MLCea608CaptionStyleColor styleColor;

        /// <summary>
        /// The number of rows for closed caption text.
        /// </summary>
        public uint row;

        /// <summary>
        /// The number of columns for closed caption text.
        /// </summary>
        public uint col;
    }

    /// <summary>
    /// <c>Cea608</c> caption line structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    [Obsolete("Please use MLMediaPlayer.Cea608CaptionLineInternal instead")]
    public struct MLCea608CaptionLineInternal
    {
        /// <summary>
        /// Characters to be displayed.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MLCea608CaptionDimension.MLCea608_CCMaxColsPlus2)]
        public byte[] displayChars;

        /// <summary>
        ///  <c>Cea608</c> caption style and color.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MLCea608CaptionDimension.MLCea608_CCMaxColsPlus2)]
        public IntPtr[] midRowStyles;

        /// <summary>
        /// <c>Cea608</c> caption preamble address code.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MLCea608CaptionDimension.MLCea608_CCMaxColsPlus2)]
        public IntPtr[] pacStyles;
    }

    /// <summary>
    /// <c>Cea608</c> caption segment structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [Obsolete("Please use MLMediaPlayer.Cea608CaptionSegmentInternal instead")]
    public struct MLCea608CaptionSegmentInternal
    {
        /// <summary>
        /// Specific version.
        /// </summary>
        public uint version;

        /// <summary>
        /// <c>Cea608</c> caption line structure.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MLCea608CaptionDimension.MLCea608_CCMaxRowsPlus2)]
        public IntPtr[] ccLines;
    }

    /// <summary>
    /// <c>Cea608</c> caption line structure.
    /// </summary>
    [Obsolete("Please use MLMediaPlayer.Cea608CaptionLine instead")]
    public class MLCea608CaptionLine
    {
        /// <summary>
        /// Characters to be displayed.
        /// </summary>
        public byte[] displayChars;

        /// <summary>
        ///  <c>Cea608</c> caption style and color.
        /// </summary>
        public MLCea608CaptionStyleColor[] midRowStyles;

        /// <summary>
        /// <c>Cea608</c> caption preamble address code.
        /// </summary>
        public MLCea608CaptionPAC[] pacStyles;
    }

    /// <summary>
    /// <c>Cea608</c> caption segment structure.
    /// </summary>
    [Obsolete("Please use MLMediaPlayer.Cea608CaptionSegment instead")]
    public struct MLCea608CaptionSegment
    {
        /// <summary>
        /// Specific version.
        /// </summary>
        public uint version;

        /// <summary>
        /// <c>Cea608</c> caption line structure.
        /// </summary>
        public MLCea608CaptionLine[] ccLines;
    }
    #endif
}
