// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLImageNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;

    /// <summary>
    /// See ml_image.h for additional comments
    /// </summary>
    public class MLImageNativeBindings : MagicLeapNativeBindings
    {
        /// <summary>
        /// Information about the data contained inside the image.
        /// </summary>
        public enum MLImageType
        {
            /// <summary>
            /// Bit mask storing 8-bit unsigned integer per pixel.
            /// </summary>
            BitMask,

            /// <summary>
            /// RGB format (linear color space).
            /// </summary>
            RGB24,

            /// <summary>
            /// RGBA format (linear color space).
            /// </summary>
            RGBA32
        }

        /// <summary>
        /// Image and its meta-data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLImageNative
        {
            /// <summary>
            /// Image width in pixels.
            /// </summary>
            public uint Width;

            /// <summary>
            /// Image height in pixels.
            /// </summary>
            public uint Height;

            /// <summary>
            /// Type of image.
            /// </summary>
            public MLImageType ImageType;

            /// <summary>
            /// Row alignment.
            /// </summary>
            public byte Alignment;

            /// <summary>
            /// The image data stored without compression in row major order. Image origin is the upper left corner.
            /// </summary>
            public IntPtr Image;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <returns>A new instance of this struct.</returns>
            public static MLImageNative Create()
            {
                return new MLImageNative
                {
                    Width = 1u,
                    Height = 1u,
                    ImageType = MLImageType.BitMask,
                    Alignment = 0,
                    Image = IntPtr.Zero
                };
            }
        }
    }
}

#endif
