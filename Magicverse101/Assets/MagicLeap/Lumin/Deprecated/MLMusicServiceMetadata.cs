// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMusicServiceMetadata.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// MusicService Error type
    /// </summary>
    [Obsolete("Please use MLMusicService.ErrorType instead.", true)]
    public enum MLMusicServiceErrorType : uint
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
    /// MusicService status
    /// </summary>
    [Obsolete("Please use MLMusicService.Status instead.", true)]
    public enum MLMusicServiceStatus : uint
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
    /// MusicService playback state
    /// </summary>
    [Obsolete("Please use MLMusicService.PlaybackStateType instead.", true)]
    public enum MLMusicServicePlaybackState : uint
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
    [Obsolete("Please use MLMusicService.RepeatStateType instead.", true)]
    public enum MLMusicServiceRepeatState : uint
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
    [Obsolete("Please use MLMusicService.ShuffleStateType instead.", true)]
    public enum MLMusicServiceShuffleState : uint
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
    [Obsolete("Please use MLMusicService.TrackType instead.", true)]
    public enum MLMusicServiceTrackType : uint
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
    /// Contains the meta data for a track
    /// Currently only provides support for ANSI strings
    /// </summary>
    [Obsolete("Please use MLMusicService.Metadata instead.", true)]
    public struct MLMusicServiceMetadata
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
    [Obsolete("Please use MLMusicService.Error instead.", true)]
    public struct MLMusicServiceError
    {
        /// <summary>
        /// The error type.
        /// </summary>
        public MLMusicServiceErrorType ErrorType;

        /// <summary>
        /// For ErrorTypes other than ServiceSpecific, the value of ErrorCode will match the value of ErrorType.
        /// When ErrorType is ServiceSpecific, the ErrorCode will be specific to the music streaming service.
        /// </summary>
        public int ErrorCode;

        /// <summary>
        /// Create and return an initialized version of this struct.
        /// </summary>
        /// <returns>The new generated MLMusicServiceError.</returns>
        public static MLMusicServiceError Create()
        {
            return new MLMusicServiceError()
            {
                ErrorType = MLMusicServiceErrorType.None,
                ErrorCode = 0
            };
        }
    }
    #endif
}
