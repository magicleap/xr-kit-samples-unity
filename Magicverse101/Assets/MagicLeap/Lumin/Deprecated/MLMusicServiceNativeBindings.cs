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

    /// <summary>
    /// See ml_music_service.h for additional comments.
    /// </summary>
    public partial class MLMusicServiceNativeBindings : MagicLeapNativeBindings
    {
        /// <summary>
        /// Contains the meta data for a track.
        /// Currently only provides support for ANSI strings.
        /// </summary>
        public partial struct MetadataNative
        {
            /// <summary>
            /// Gets the user facing struct from the native one.
            /// </summary>
            [Obsolete("Please use MLMusicServiceNativeBindings.MetadataNative.Data instead.", true)]
            public MLMusicServiceMetadata DataEx
            {
                get
                {
                    return new MLMusicServiceMetadata()
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
        }
    }
}

#endif
