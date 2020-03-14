// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLCameraNativeBindings.cs" company="Magic Leap, Inc">
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
    /// Camera errors
    /// </summary>
    [Obsolete("Please use MLCamera.ErrorType instead.", true)]
    public enum MLCameraError
    {
        /// <summary>
        /// No error
        /// </summary>
        None = 0,

        /// <summary>
        /// Invalid state
        /// </summary>
        Invalid,

        /// <summary>
        /// Camera disabled
        /// </summary>
        Disabled,

        /// <summary>
        /// Camera device failed
        /// </summary>
        DeviceFailed,

        /// <summary>
        /// Camera service failed
        /// </summary>
        ServiceFailed,

        /// <summary>
        /// Capture failed
        /// </summary>
        CaptureFailed,

        /// <summary>
        /// Unknown capture error
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Capture operation type
    /// </summary>
    [Obsolete("Please use MLCamera.CaptureType instead.", true)]
    public enum MLCameraCaptureType
    {
        /// <summary>
        /// To capture an image and save the JPEG-compressed data to a file.
        /// </summary>
        Image = 0,

        /// <summary>
        /// To capture an image and obtain the JPEG-compressed stream.
        /// </summary>
        ImageRaw,

        /// <summary>
        /// To capture a video and save it to a file.
        /// </summary>
        Video,

        /// <summary>
        /// To capture a video and access the raw buffer of the frames.
        /// </summary>
        VideoRaw
    }

    /// <summary>
    /// Captured output format
    /// </summary>
    [Obsolete("Please use MLCamera.OutputFormat instead.", true)]
    public enum MLCameraOutputFormat
    {
        /// <summary>
        /// Unknown format
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// YUV planar format
        /// </summary>
        YUV_420_888,

        /// <summary>
        /// Compressed output stream
        /// </summary>
        JPEG
    }

    /// <summary>
    /// Client can implement polling mechanism to retrieve device status
    /// and use these masks to view device status.
    /// </summary>
    [Obsolete("Please use MLCamera.DeviceStatusFlag instead.", true)]
    public enum MLCameraDeviceStatusFlag
    {
        /// <summary>
        /// The device status when the camera is available.
        /// </summary>
        Available = 1 << 0,

        /// <summary>
        /// The device status when the camera is opened.
        /// </summary>
        Opened = 1 << 1,

        /// <summary>
        /// The device status when the camera is disconnected.
        /// </summary>
        Disconnected = 1 << 2,

        /// <summary>
        /// The device status when the camera has an error.
        /// </summary>
        Error = 1 << 3
    }

    /// <summary>
    /// Client can implement polling mechanism to retrieve capture status
    /// and use these masks to view capture status.
    /// </summary>
    [Obsolete("Please use MLCamera.CaptureStatusFlag instead.", true)]
    public enum MLCameraCaptureStatusFlag
    {
        /// <summary>
        /// The capture has started.
        /// </summary>
        Started = 1 << 0,

        /// <summary>
        /// The capture failed.
        /// </summary>
        Failed = 1 << 1,

        /// <summary>
        /// The buffer was lost.
        /// </summary>
        BufferLost = 1 << 2,

        /// <summary>
        /// The capture is in progress.
        /// </summary>
        InProgress = 1 << 3,

        /// <summary>
        /// The capture completed.
        /// </summary>
        Completed = 1 << 4
    }

    /// <summary>
    /// The metadata for the control AE mode.
    /// </summary>
    [Obsolete("Please use MLCamera.MetadataControlAEMode instead.", true)]
    public enum MLCameraMetadataControlAEMode
    {
        /// <summary>
        /// The control AE mode: Off.
        /// </summary>
        Off = 0,

        /// <summary>
        /// The control AE mode: On.
        /// </summary>
        On
    }

    /// <summary>
    /// The metadata for the color correction aberration mode.
    /// </summary>
    [Obsolete("Please use MLCamera.MetadataColorCorrectionAberrationMode instead.", true)]
    public enum MLCameraMetadataColorCorrectionAberrationMode
    {
        /// <summary>
        /// The color correction aberration mode: Off.
        /// </summary>
        Off = 0,

        /// <summary>
        /// The color correction aberration mode: Fast.
        /// </summary>
        Fast,

        /// <summary>
        /// The color correction aberration mode: High Quality.
        /// </summary>
        HighQuality,
    }

    /// <summary>
    /// The metadata for the control AE lock.
    /// </summary>
    [Obsolete("Please use MLCamera.MetadataControlAELock instead.", true)]
    public enum MLCameraMetadataControlAELock
    {
        /// <summary>
        /// The control AE lock: Off
        /// </summary>
        Off = 0,

        /// <summary>
        /// The control AE lock: On
        /// </summary>
        On,
    }

    /// <summary>
    /// The metadata for the control AWB mode.
    /// </summary>
    [Obsolete("Please use MLCamera.MetadataControlAWBMode instead.")]
    public enum MLCameraMetadataControlAWBMode
    {
        /// <summary>
        /// The control AWB mode: Off
        /// </summary>
        Off = 0,

        /// <summary>
        /// The control AWB mode: Auto
        /// </summary>
        Auto,

        /// <summary>
        /// The control AWB mode: Incandescent
        /// </summary>
        Incandescent,

        /// <summary>
        /// The control AWB mode: Fluorescent
        /// </summary>
        Fluorescent,

        /// <summary>
        /// The control AWB mode: Warm Fluorescent
        /// </summary>
        WarmFluorescent,

        /// <summary>
        /// The control AWB mode: Daylight
        /// </summary>
        Daylight,

        /// <summary>
        /// The control AWB mode: Cloudy Day Light
        /// </summary>
        CloudyDaylight,

        /// <summary>
        /// The control AWB mode: Twilight
        /// </summary>
        Twilight,

        /// <summary>
        /// The control AWB mode: Shade
        /// </summary>
        Shade,
    }

    /// <summary>
    /// The metadata for the control AWB lock.
    /// </summary>
    [Obsolete("Please use MLCamera.MetadataControlAWBLock instead.")]
    public enum MLCameraMetadataControlAWBLock
    {
        /// <summary>
        /// The control AWB lock: Off
        /// </summary>
        Off = 0,

        /// <summary>
        /// The control AWB lock: On
        /// </summary>
        On,
    }

    /// <summary>
    /// The metadata for the color correction mode.
    /// </summary>
    [Obsolete("Please use MLCamera.MetadataColorCorrectionMode instead.")]
    public enum MLCameraMetadataColorCorrectionMode
    {
        /// <summary>
        /// The color correction mode: Transform Matrix
        /// </summary>
        TransformMatrix = 0,

        /// <summary>
        /// The color correction mode: Fast
        /// </summary>
        Fast,

        /// <summary>
        /// The color correction mode: High Quality
        /// </summary>
        HighQuality,
    }

    /// <summary>
    /// The metadata for the control AE anti banding mode.
    /// </summary>
    [Obsolete("Please use MLCamera.MetadataControlAEAntibandingMode instead.")]
    public enum MLCameraMetadataControlAEAntibandingMode
    {
        /// <summary>
        /// The control AE anti banding mode: Off
        /// </summary>
        Off = 0,

        /// <summary>
        /// The control AE anti banding mode: 50hz
        /// </summary>
        FiftyHz,

        /// <summary>
        /// The control AE anti banding mode: 60hz
        /// </summary>
        SixtyHz,

        /// <summary>
        /// The control AE anti banding mode: Auto
        /// </summary>
        Auto,
    }

    /// <summary>
    /// The metadata for scaler available formats.
    /// </summary>
    [Obsolete("Please use MLCamera.MetadataScalerAvailableFormats instead.")]
    public enum MLCameraMetadataScalerAvailableFormats
    {
        /// <summary>
        /// RAW16 Format
        /// </summary>
        RAW16 = 0x20,

        /// <summary>
        /// RAW OPAQUE Format
        /// </summary>
        RAW_OPAQUE = 0x24,

        /// <summary>
        /// YV12 Format
        /// </summary>
        YV12 = 0x32315659,

        /// <summary>
        /// <c>YCrCb 420 SP Format</c>
        /// </summary>
        YCrCb_420_SP = 0x11,

        /// <summary>
        /// Implementation Defined
        /// </summary>
        IMPLEMENTATION_DEFINED = 0x22,

        /// <summary>
        /// <c>YCbCr 420 888 Format</c>
        /// </summary>
        YCbCr_420_888 = 0x23,

        /// <summary>
        /// BLOB Format
        /// </summary>
        BLOB = 0x21,
    }

    /// <summary>
    /// The metadata for scaler available stream configurations.
    /// </summary>
    [Obsolete("Please use MLCamera.MetadataScalerAvailableStreamConfigurations instead.")]
    public enum MLCameraMetadataScalerAvailableStreamConfigurations
    {
        /// <summary>
        /// The scaler available stream configuration: Output
        /// </summary>
        OUTPUT = 0,

        /// <summary>
        /// The scaler available stream configuration: Input
        /// </summary>
        INPUT,
    }

   /// <summary>
   /// The metadata for the control AE state.
   /// </summary>
    [Obsolete("Please use MLCamera.MetadataControlAEState instead.")]
    public enum MLCameraMetadataControlAEState
    {
        /// <summary>
        /// The control AE state: Inactive
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// The control AE state: Searching
        /// </summary>
        Searching,

        /// <summary>
        /// The control AE state: Converged
        /// </summary>
        Converged,

        /// <summary>
        /// The control AE state: Locked
        /// </summary>
        Locked,

        /// <summary>
        /// The control AE state: Flash Required
        /// </summary>
        FlashRequired,

        /// <summary>
        /// The control AE state: Pre-capture
        /// </summary>
        PreCapture,
    }

    /// <summary>
    /// The metadata for the control AWB state.
    /// </summary>
    [Obsolete("Please use MLCamera.MetadataControlAWBState instead.")]
    public enum MLCameraMetadataControlAWBState
    {
        /// <summary>
        /// The control AWB state: Inactive
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// The control AWB state: Searching
        /// </summary>
        Searching,

        /// <summary>
        /// The control AWB state: Converged
        /// </summary>
        Converged,

        /// <summary>
        /// The control AWB state: Locked
        /// </summary>
        Locked,
    }

    /// <summary>
    /// Per plane info for captured output
    /// </summary>
    [Obsolete("Please use MLCamera.PlaneInfo instead.")]
    public struct MLCameraPlaneInfo
    {
        /// <summary>
        /// Width of the output image in pixels.
        /// </summary>
        public uint Width;

        /// <summary>
        /// Height of the output image in pixels.
        /// </summary>
        public uint Height;

        /// <summary>
        /// Stride of the output image in pixels.
        /// </summary>
        public uint Stride;

        /// <summary>
        /// Number of bytes used to represent a pixel.
        /// </summary>
        public uint BytesPerPixel;

        /// <summary>
        /// Image data
        /// </summary>
        public IntPtr Data;

        /// <summary>
        /// Number of bytes in the image output data.
        /// </summary>
        public uint Size;

        /// <summary>
        /// Create and return an initialized version of this struct.
        /// </summary>
        /// <returns>Returns a new MLCameraPlaneInfo structure.</returns>
        public static MLCameraPlaneInfo Create()
        {
            return new MLCameraPlaneInfo()
            {
            };
        }
    }

    /// <summary>
    /// Captured output
    /// </summary>
    [Obsolete("Please use MLCamera.Output instead.")]
    public struct MLCameraOutput
    {
        /// <summary>
        /// Number of output image planes:
        /// - 1 for compressed output such as JPEG stream,
        /// - 3 for separate color component output such as <c>YUV/YCbCr/RGB.</c>
        /// </summary>
        public byte PlaneCount;

        /// <summary>
        /// Output image plane info.
        /// The number of output planes is specified by PlaneCount.
        /// </summary>
        public MLCameraPlaneInfo[] Planes;

        /// <summary>
        /// Supported output format specified by MLCameraOutputFormat
        /// </summary>
        public MLCameraOutputFormat Format;

        /// <summary>
        /// Create and return an initialized version of this struct.
        /// </summary>
        /// <returns>Returns a new MLCameraOutput structure.</returns>
        public static MLCameraOutput Create()
        {
            return new MLCameraOutput()
            {
            };
        }
    }

    /// <summary>
    /// Contains both the data and information necessary to read the data for a specific buffer in a YUV capture
    /// </summary>
    [Obsolete("Please use MLCamera.YUVBuffer instead.")]
    public struct YUVBuffer
    {
        /// <summary>
        /// Indicate if this structure contains valid data
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// Width of the output image in pixels
        /// </summary>
        public uint Width;

        /// <summary>
        /// Height of the output image in pixels
        /// </summary>
        public uint Height;

        /// <summary>
        /// Stride of the output image in pixels
        /// </summary>
        public uint Stride;

        /// <summary>
        /// Number of bytes used to represent a pixel
        /// </summary>
        public uint BytesPerPixel;

        /// <summary>
        /// Image Data
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Number of bytes in the image output data
        /// </summary>
        public uint Size;

        /// <summary>
        /// Create and return an initialized version of this struct.
        /// </summary>
        /// <returns>Returns a new YUVBuffer structure.</returns>
        public static YUVBuffer Create()
        {
            return new YUVBuffer()
            {
            };
        }

        /// <summary>
        /// Copy the properties from a MLCameraPlaneInfo structure.
        /// </summary>
        /// <param name="plane">The MLCameraPlaneInfo to copy.</param>
        internal void CopyFromPlane(MLCamera.PlaneInfo plane)
        {
        }
    }

    /// <summary>
    /// Contains the information and data of each of the available buffers/planes in a YUV capture
    /// </summary>
    [Obsolete("Please use MLCamera.YUVFrameInfo instead.")]
    public struct YUVFrameInfo
    {
        /// <summary>
        /// Y Buffer information and data
        /// </summary>
        public YUVBuffer Y;

        /// <summary>
        /// U Buffer information and data
        /// </summary>
        public YUVBuffer U;

        /// <summary>
        /// V Buffer information and data
        /// </summary>
        public YUVBuffer V;

        /// <summary>
        /// Create and return an initialized version of this struct.
        /// </summary>
        /// <returns>Returns a new YUVFrameInfo structure.</returns>
        public static YUVFrameInfo Create()
        {
            return new YUVFrameInfo()
            {
                Y = YUVBuffer.Create(),
                U = YUVBuffer.Create(),
                V = YUVBuffer.Create()
            };
        }
    }

    /// <summary>
    /// ResultExtras is a structure to encapsulate various indices for a capture result.
    /// </summary>
    [Obsolete("Please use MLCamera.MLCameraResultExtras instead.")]
    public struct MLCameraResultExtras
    {
        /// <summary>
        /// An integer to index the request sequence that this result belongs to
        /// </summary>
        public int RequestId;

        /// <summary>
        /// An integer to index this result inside a request sequence, starting from 0
        /// </summary>
        public int BurstId;

        /// <summary>
        /// A 64bit integer to index the frame number associated with this result
        /// </summary>
        public long FrameNumber;

        /// <summary>
        /// The partial result count (index) for this capture result
        /// </summary>
        public int PartialResultCount;

        /// <summary>
        /// VCam exposure timestamp in microseconds (us)
        /// </summary>
        public ulong VcamTimestampUs;

        /// <summary>
        /// Create and return an initialized version of this struct.
        /// </summary>
        /// <returns>Returns a new MLCameraResultExtras structure.</returns>
        public static MLCameraResultExtras Create()
        {
            return new MLCameraResultExtras()
            {
                RequestId = 0,
                BurstId = 0,
                FrameNumber = 0,
                PartialResultCount = 0,
                VcamTimestampUs = 0
            };
        }
    }

    /// <summary>
    /// Structure to encapsulate camera frame specific metadata.
    /// </summary>
    [Obsolete("Please use MLCamera.FrameMetadata instead.")]
    public struct MLCameraFrameMetadata
    {
        /// <summary>
        /// Frame exposure time for the given frame in nanoseconds.
        /// </summary>
        public long ExposureTimeNs;

        /// <summary>
        /// Create and return an initialized version of this struct.
        /// </summary>
        /// <returns>Returns a new MLCameraFrameMetadata structure.</returns>
        public static MLCameraFrameMetadata Create()
        {
            return new MLCameraFrameMetadata()
            {
            };
        }
    }

    /// <summary>
    /// Information about the camera color correction transform.
    /// </summary>
    [Obsolete("Please use MLCamera.ColorCorrectionTransform instead.")]
    public class MLCameraColorCorrectionTransform
    {
        #pragma warning disable 414
        /// <summary>
        /// A dirty flag to indicate a change has been made.
        /// </summary>
        private bool isDirty;
        #pragma warning restore 414

        /// <summary>
        /// The x0 value.
        /// </summary>
        private float x0;

        /// <summary>
        /// The x1 value.
        /// </summary>
        private float x1;

        /// <summary>
        /// The x2 value.
        /// </summary>
        private float x2;

        /// <summary>
        /// The y0 value.
        /// </summary>
        private float y0;

        /// <summary>
        /// The y1 value.
        /// </summary>
        private float y1;

        /// <summary>
        /// The y2 value.
        /// </summary>
        private float y2;

        /// <summary>
        /// The z0 value.
        /// </summary>
        private float z0;

        /// <summary>
        /// The z1 value.
        /// </summary>
        private float z1;

        /// <summary>
        /// The z2 value.
        /// </summary>
        private float z2;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLCameraColorCorrectionTransform"/> class.
        /// </summary>
        /// <param name="x0">The x0 value.</param>
        /// <param name="x1">The x1 value.</param>
        /// <param name="x2">The x2 value.</param>
        /// <param name="y0">The y0 value.</param>
        /// <param name="y1">The y1 value.</param>
        /// <param name="y2">The y2 value.</param>
        /// <param name="z0">The z0 value.</param>
        /// <param name="z1">The z1 value.</param>
        /// <param name="z2">The z2 value.</param>
        public MLCameraColorCorrectionTransform(float x0, float x1, float x2, float y0, float y1, float y2, float z0, float z1, float z2)
        {
            this.X0 = x0;
            this.X1 = x1;
            this.X2 = x2;
            this.Y0 = y0;
            this.Y1 = y1;
            this.Y2 = y2;
            this.Z0 = z0;
            this.Z1 = z1;
            this.Z2 = z2;

            this.isDirty = true;
        }

        /// <summary>
        /// Gets or sets the X0 value.
        /// </summary>
        public float X0
        {
            get
            {
                return this.x0;
            }

            set
            {
                this.isDirty = true;
                this.x0 = value;
            }
        }

        /// <summary>
        /// Gets or sets the X1 value.
        /// </summary>
        public float X1
        {
            get
            {
                return this.x1;
            }

            set
            {
                this.isDirty = true;
                this.x1 = value;
            }
        }

        /// <summary>
        /// Gets or sets the X2 value.
        /// </summary>
        public float X2
        {
            get
            {
                return this.x2;
            }

            set
            {
                this.isDirty = true;
                this.x2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y0 value.
        /// </summary>
        public float Y0
        {
            get
            {
                return this.y0;
            }

            set
            {
                this.isDirty = true;
                this.y0 = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y1 value.
        /// </summary>
        public float Y1
        {
            get
            {
                return this.y1;
            }

            set
            {
                this.isDirty = true;
                this.y1 = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y2 value.
        /// </summary>
        public float Y2
        {
            get
            {
                return this.y2;
            }

            set
            {
                this.isDirty = true;
                this.y2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the Z0 value.
        /// </summary>
        public float Z0
        {
            get
            {
                return this.z0;
            }

            set
            {
                this.isDirty = true;
                this.z0 = value;
            }
        }

        /// <summary>
        /// Gets or sets the Z1 value.
        /// </summary>
        public float Z1
        {
            get
            {
                return this.z1;
            }

            set
            {
                this.isDirty = true;
                this.z1 = value;
            }
        }

        /// <summary>
        /// Gets or sets the Z2 value.
        /// </summary>
        public float Z2
        {
            get
            {
                return this.z2;
            }

            set
            {
                this.isDirty = true;
                this.z2 = value;
            }
        }
    }

    /// <summary>
    /// Camera color correction gains.
    /// </summary>
    [Obsolete("Please use MLCamera.ColorCorrectionGains instead.")]
    public class MLCameraColorCorrectionGains
    {
        #pragma warning disable 414
        /// <summary>
        /// A dirty flag to indicate a change has been made.
        /// </summary>
        private bool isDirty;
        #pragma warning restore 414

        /// <summary>
        /// The blue color value.
        /// </summary>
        private float blue;

        /// <summary>
        /// The green even color value.
        /// </summary>
        private float greenEven;

        /// <summary>
        /// The green odd color value.
        /// </summary>
        private float greenOdd;

        /// <summary>
        /// The red color value.
        /// </summary>
        private float red;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLCameraColorCorrectionGains"/> class.
        /// </summary>
        /// <param name="red">The red color value.</param>
        /// <param name="greenEven">The green even color value.</param>
        /// <param name="greenOdd">The green odd color value.</param>
        /// <param name="blue">The blue color value.</param>
        public MLCameraColorCorrectionGains(float red, float greenEven, float greenOdd, float blue)
        {
            this.Red = red;
            this.GreenEven = greenEven;
            this.GreenOdd = greenOdd;
            this.Blue = blue;

            this.isDirty = true;
        }

        /// <summary>
        /// Gets or sets the blue color value.
        /// </summary>
        public float Blue
        {
            get
            {
                return this.blue;
            }

            set
            {
                this.isDirty = true;
                this.blue = value;
            }
        }

        /// <summary>
        /// Gets or sets the green even color value.
        /// </summary>
        public float GreenEven
        {
            get
            {
                return this.greenEven;
            }

            set
            {
                this.isDirty = true;
                this.greenEven = value;
            }
        }

        /// <summary>
        /// Gets or sets the green odd color value.
        /// </summary>
        public float GreenOdd
        {
            get
            {
                return this.greenOdd;
            }

            set
            {
                this.isDirty = true;
                this.greenOdd = value;
            }
        }

        /// <summary>
        /// Gets or sets the red color value.
        /// </summary>
        public float Red
        {
            get
            {
                return this.red;
            }

            set
            {
                this.isDirty = true;
                this.red = value;
            }
        }
    }

    /// <summary>
    /// The camera control AE target FPS range.
    /// </summary>
    [Obsolete("Please use MLCamera.ControlAETargetFPSRange instead.")]
    public class MLCameraControlAETargetFPSRange
    {
        #pragma warning disable 414
        /// <summary>
        /// A dirty flag to indicate a change has been made.
        /// </summary>
        private bool isDirty;
        #pragma warning restore 414

        /// <summary>
        /// The minimum distance.
        /// </summary>
        private int minimum;

        /// <summary>
        /// The maximum distance.
        /// </summary>
        private int maximum;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLCameraControlAETargetFPSRange"/> class.
        /// </summary>
        /// <param name="minimum">The minimum distance.</param>
        /// <param name="maximum">The maximum distance.</param>
        public MLCameraControlAETargetFPSRange(int minimum, int maximum)
        {
            this.Minimum = minimum;
            this.Maximum = maximum;

            this.isDirty = true;
        }

        /// <summary>
        /// Gets or sets the minimum distance.
        /// </summary>
        public int Minimum
        {
            get
            {
                return this.minimum;
            }

            set
            {
                this.isDirty = true;
                this.minimum = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum distance.
        /// </summary>
        public int Maximum
        {
            get
            {
                return this.maximum;
            }

            set
            {
                this.isDirty = true;
                this.maximum = value;
            }
        }
    }

    /// <summary>
    /// The camera scaler crop region.
    /// </summary>
    [Obsolete("Please use MLCamera.ScalerCropRegion instead.")]
    public class MLCameraScalerCropRegion
    {
        #pragma warning disable 414
        /// <summary>
        /// A dirty flag to indicate a change has been made.
        /// </summary>
        private bool isDirty;
        #pragma warning restore 414

        /// <summary>
        /// The left crop region value.
        /// </summary>
        private int left;

        /// <summary>
        /// The top crop region value.
        /// </summary>
        private int top;

        /// <summary>
        /// The right crop region value.
        /// </summary>
        private int right;

        /// <summary>
        /// The bottom crop region value.
        /// </summary>
        private int bottom;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLCameraScalerCropRegion"/> class.
        /// </summary>
        /// <param name="left">The left crop region value.</param>
        /// <param name="top">The top crop region value.</param>
        /// <param name="right">The right crop region value.</param>
        /// <param name="bottom">The bottom crop region value.</param>
        public MLCameraScalerCropRegion(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;

            this.isDirty = true;
        }

        /// <summary>
        /// Gets or sets the left crop region value.
        /// </summary>
        public int Left
        {
            get
            {
                return this.left;
            }

            set
            {
                this.isDirty = true;
                this.left = value;
            }
        }

        /// <summary>
        /// Gets or sets the top crop region value.
        /// </summary>
        public int Top
        {
            get
            {
                return this.top;
            }

            set
            {
                this.isDirty = true;
                this.top = value;
            }
        }

        /// <summary>
        /// Gets or sets the right crop region value.
        /// </summary>
        public int Right
        {
            get
            {
                return this.right;
            }

            set
            {
                this.isDirty = true;
                this.right = value;
            }
        }

        /// <summary>
        /// Gets or sets the bottom crop region value.
        /// </summary>
        public int Bottom
        {
            get
            {
                return this.bottom;
            }

            set
            {
                this.isDirty = true;
                this.bottom = value;
            }
        }
    }
}

#endif
