// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLTextureUtils.cs" company="Magic Leap, Inc">
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
    using System.Text;
    using UnityEngine;
    using UnityEngine.XR.MagicLeap;

    /// <summary>
    /// Class containing utility functions to convert Unity Texture to <c>Lumin</c> OS compatible data structures.
    /// </summary>
    public static class MLTextureUtils
    {
        /// <summary>
        /// Convert Unity Texture2D to a byte array.
        /// Texture2D should be in one of the following formats: DXT1, RGBA32, RGB24.
        /// For DXT1 and RGBA32, resulting byte array is in RGBA format.
        /// For RGB24, resulting byte array is in RGB format.
        /// The origin of the byte array will be at the top left corner.
        /// Returns null on unsupported formats.
        /// </summary>
        /// <param name="texture">Texture to extract byte array from</param>
        /// <param name="numChannels">Out parameter to determine how many bytes per pixel</param>
        /// <returns>Returns an array of bytes that holds the converted texture</returns>
        public static byte[] ConvertToByteArray(Texture2D texture, out int numChannels)
        {
            byte[] encodedImage = null;
            numChannels = 0;

            // [1] Convert the unity texture to RGBA pixel format (if needed).
            // For a pixel at (x,y) the channel c data of that pixels is at the position
            // given by [(width * y + x)*channels + c].
            if (TextureFormat.DXT1 == texture.format)
            {
                Color32[] colorArray = texture.GetPixels32();
                numChannels  = Marshal.SizeOf(typeof(Color32));
                int totalBytes = numChannels * colorArray.Length;
                encodedImage = new byte[totalBytes];
                GCHandle handle = default(GCHandle);
                try
                {
                    handle = GCHandle.Alloc(colorArray, GCHandleType.Pinned);
                    IntPtr ptr = handle.AddrOfPinnedObject();
                    Marshal.Copy(ptr, encodedImage, 0, totalBytes);
                }
                finally
                {
                    if (handle != default(GCHandle))
                    {
                        handle.Free();
                    }
                }
            }
            else if (TextureFormat.RGBA32 == texture.format)
            {
                numChannels = 4;
                encodedImage = texture.GetRawTextureData();
            }
            else if (TextureFormat.RGB24 == texture.format)
            {
                numChannels = 3;
                encodedImage = texture.GetRawTextureData();
            }
            else
            {
                MLPluginLog.Error("MLTextureUtils.ConvertToByteArray failed to convert to byte array. Reason: unsupported format. Use RGBA32, RGB24 or DXT1 format when importing textures.");
                return encodedImage;
            }

            // [2] Convert to coorindate frame used by Lumin OS
            // Unity texture origin in bottom left, but Lumin OS is expecting top left to be the origin.
            int rowLength = texture.width * numChannels;
            var rowTemp = new byte[rowLength];
            for (var i = 0; i < texture.height / 2; i++)
            {
                Buffer.BlockCopy(encodedImage, i * rowLength, rowTemp, 0, rowLength);
                Buffer.BlockCopy(encodedImage, (texture.height - i - 1) * rowLength, encodedImage, i * rowLength, rowLength);
                Buffer.BlockCopy(rowTemp, 0, encodedImage, (texture.height - i - 1) * rowLength, rowLength);
            }

            return encodedImage;
        }
    }
}

#endif
