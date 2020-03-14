// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLCamera.cs" company="Magic Leap, Inc">
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
    using System.Linq;
    using System.Runtime.InteropServices;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// MLCamera class exposes static functions to query camera related
    /// functions. Most functions are currently a direct pass through functions to the
    /// native C-API functions and incur no overhead.
    /// </summary>
    public sealed partial class MLCamera : MLAPISingleton<MLCamera>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// Width of the internal camera preview texture
        /// </summary>
        public const int PreviewTextureWidth = 1920;

        /// <summary>
        /// Height of the internal camera preview texture
        /// </summary>
        public const int PreviewTextureHeight = 1080;

        /// <summary>
        /// The capture path for the camera.
        /// </summary>
        private static string capturePath = string.Empty;

        /// <summary>
        /// A raw video queue lock object.
        /// </summary>
        private static object rawVideoFrameQueueLockObject = new object();

        /// <summary>
        /// A queue for the raw video frames.
        /// </summary>
        private static Queue<Action> queuedRawVideoFrames = new Queue<Action>();

        /// <summary>
        /// A queue for the dispatch raw video frames.
        /// </summary>
        private static Queue<Action> dispatchRawVideoFramesQueue = new Queue<Action>();

        /// <summary>
        /// The maximum number of raw video frames in the queue.
        /// </summary>
        private static uint maximumRawVideoFrameQueue = 1;

        /// <summary>
        /// The dropped raw video frames.
        /// </summary>
        private static uint droppedRawVideoFrames = 0;

        /// <summary>
        /// Connection status of the camera.
        /// </summary>
        private bool cameraConnectionEstablished = false;

        /// <summary>
        /// Capture status of the camera.
        /// </summary>
        private bool captureVideoStarted = false;

        /// <summary>
        /// A camera status callback.
        /// </summary>
        private MLCameraNativeBindings.MLCameraDeviceStatusCallbacksNative statusCallbacks;

        /// <summary>
        /// The unmanaged pointer for the status callback.
        /// </summary>
        private IntPtr statusCallbacksUnmanaged = IntPtr.Zero;

        /// <summary>
        /// The camera capture callback.
        /// </summary>
        private MLCameraNativeBindings.MLCameraCaptureCallbacksExNative captureCallbacks;

        /// <summary>
        /// The unmanaged pointer for the capture callback.
        /// </summary>
        private IntPtr captureCallbacksUnmanaged = IntPtr.Zero;

        /// <summary>
        /// A handle to the head tracker.
        /// </summary>
        private ulong headTrackerHandle = MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// A handle to the CV camera.
        /// </summary>
        private ulong cameraCVTrackerHandle = MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// The number of dropped raw video frames since the last update.
        /// </summary>
        private uint droppedRawVideoFramesLastUpdate = 0;

        /// <summary>
        /// General settings for the camera.
        /// </summary>
        private MLCamera.GeneralSettings settings = null;

        /// <summary>
        /// The preview texture for the camera captures.
        /// </summary>
        private Texture2D previewTexture2D = null;

        /// <summary>
        /// The resume connect state of the camera.
        /// </summary>
        private bool resumeConnect = false;

        /// <summary>
        /// The resume preview state of the camera.
        /// </summary>
        private bool resumePreview = false;

        /// <summary>
        /// Prevents a default instance of the <see cref="MLCamera"/> class from being created.
        /// </summary>
        private MLCamera()
        {
            this.DllNotFoundError = "{0} is only available on device.";
        }

        /// <summary>
        /// A delegate for device available events.
        /// </summary>
        public delegate void OnDeviceAvailableDelegate();

        /// <summary>
        /// A delegate for device unavailable events.
        /// </summary>
        public delegate void OnDeviceUnavailableDelegate();

        /// <summary>
        /// A delegate for device opened events.
        /// </summary>
        public delegate void OnDeviceOpenedDelegate();

        /// <summary>
        /// A delegate for device closed events.
        /// </summary>
        public delegate void OnDeviceClosedDelegate();

        /// <summary>
        /// A delegate for device disconnected events.
        /// </summary>
        public delegate void OnDeviceDisconnectedDelegate();

        /// <summary>
        /// A delegate for device error events.
        /// </summary>
        /// <param name="error">The error being reported.</param>
        public delegate void OnDeviceErrorDelegate(MLCamera.ErrorType error);

        /// <summary>
        /// A delegate for camera capture started events.
        /// </summary>
        /// <param name="results">A structure containing extra result information.</param>
        public delegate void OnCaptureStartedDelegate(MLCamera.ResultExtras results);

        /// <summary>
        /// A delegate for camera capture failed events.
        /// </summary>
        /// <param name="results">A structure containing extra result information.</param>
        public delegate void OnCaptureFailedDelegate(MLCamera.ResultExtras results);

        /// <summary>
        /// A delegate for camera capture buffer lost events.
        /// </summary>
        /// <param name="results">A structure containing extra result information.</param>
        public delegate void OnCaptureBufferLostDelegate(MLCamera.ResultExtras results);

        /// <summary>
        /// A delegate for camera progressed events.
        /// </summary>
        /// <param name="results">A structure containing extra result information.</param>
        /// <param name="capturePath">The path where the capture is being stored.</param>
        public delegate void OnCaptureProgressedDelegate(MLCamera.ResultExtras results, string capturePath);

        /// <summary>
        /// A delegate for camera completed events.
        /// </summary>
        /// <param name="results">A structure containing extra result information.</param>
        /// <param name="capturePath">The path where the capture is being stored.</param>
        public delegate void OnCaptureCompletedDelegate(MLCamera.ResultExtras results, string capturePath);

        /// <summary>
        /// A delegate for camera capture settings updates events.
        /// </summary>
        /// <param name="result">A structure containing extra result information.</param>
        /// <param name="path">The path of the file.</param>
        /// <param name="settings">The camera result settings.</param>
        public delegate void OnCaptureCompletedSettingsDelegate(MLCamera.ResultExtras result, string path, MLCamera.ResultSettings settings);

        /// <summary>
        /// A delegate for camera raw image available events.
        /// </summary>
        /// <param name="image">The image in a raw byte array.</param>
        public delegate void OnRawImageAvailableDelegate(byte[] image);

        /// <summary>
        /// A delegate for camera raw image available YUV events.00
        /// </summary>
        /// <param name="frameInfo">YUV frame information for the image.</param>
        public delegate void OnRawImageAvailableYUVDelegate(YUVFrameInfo frameInfo);

        /// <summary>
        /// A delegate for camera raw video frame available events.
        /// </summary>
        /// <param name="results">A structure containing extra result information.</param>
        /// <param name="frameInfo">A structure containing the YUV frame information.</param>
        /// <param name="metadata">A structure containing the frame metadata.</param>
        public delegate void OnRawVideoFrameAvailableYUVDelegate(MLCamera.ResultExtras results, YUVFrameInfo frameInfo, MLCamera.FrameMetadata metadata);

        /// <summary>
        /// Camera status callback, device available.
        /// </summary>
        public static event OnDeviceAvailableDelegate OnDeviceAvailable = delegate { };

        /// <summary>
        /// Camera status callback, device unavailable.
        /// </summary>
        public static event OnDeviceUnavailableDelegate OnDeviceUnavailable = delegate { };

        /// <summary>
        /// Camera status callback, device opened.
        /// </summary>
        public static event OnDeviceOpenedDelegate OnDeviceOpened = delegate { };

        /// <summary>
        /// Camera status callback, device closed.
        /// </summary>
        public static event OnDeviceClosedDelegate OnDeviceClosed = delegate { };

        /// <summary>
        /// Camera status callback, device disconnected.
        /// </summary>
        public static event OnDeviceDisconnectedDelegate OnDeviceDisconnected = delegate { };

        /// <summary>Camera status callback, device error.
        /// </summary>
        public static event OnDeviceErrorDelegate OnDeviceError = delegate { };

        /// <summary>Camera capture callback, capture started.
        /// </summary>
        public static event OnCaptureStartedDelegate OnCaptureStarted = delegate { };

        /// <summary>
        /// Camera capture callback, capture failed.
        /// </summary>
        public static event OnCaptureFailedDelegate OnCaptureFailed = delegate { };

        /// <summary>
        /// Camera capture callback, capture buffer lost.
        /// </summary>
        public static event OnCaptureBufferLostDelegate OnCaptureBufferLost = delegate { };

        /// <summary>
        /// Camera capture callback, capture progressed.
        /// </summary>
        public static event OnCaptureProgressedDelegate OnCaptureProgressed = delegate { };

        /// <summary>
        /// Camera capture callback, capture completed.
        /// Video capture will execute a capture completed callback every frame.
        /// </summary>
        public static event OnCaptureCompletedDelegate OnCaptureCompleted = delegate { };

        /// <summary>
        /// Camera capture callback, capture completed with settings results from the capture.
        /// A Video capture will cause this callback to execute on every video frame capture.
        /// </summary>
        public static event OnCaptureCompletedSettingsDelegate OnCaptureCompletedSettings = delegate { };

        /// <summary>
        /// Camera capture callback, capture raw image available.
        /// </summary>
        public static event OnRawImageAvailableDelegate OnRawImageAvailable = delegate { };

        /// <summary>
        /// Camera capture callback, capture raw image available, parameters are YUVFrameInfo.
        /// </summary>
        public static event OnRawImageAvailableYUVDelegate OnRawImageAvailableYUV = delegate { };

        /// <summary>
        /// Camera capture callback, capture raw video frame, parameters are YUVFrameInfo and FrameMetadata.
        /// </summary>
        public static event OnRawVideoFrameAvailableYUVDelegate OnRawVideoFrameAvailableYUV = delegate { };
        #endif

        /// <summary>
        /// The type of raw video frame data.
        /// </summary>
        public enum RawVideoFrameData
        {
            /// <summary>
            /// Intensity only frame data.
            /// </summary>
            Intensity = 0,

            /// <summary>
            /// Intensity and color frame data.
            /// </summary>
            IntensityAndColor
        }

        /// <summary>
        /// Camera errors
        /// </summary>
        public enum ErrorType
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
        public enum CaptureType
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
        public enum OutputFormat
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
        public enum DeviceStatusFlag
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
        public enum CaptureStatusFlag
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
        public enum MetadataControlAEMode
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
        public enum MetadataColorCorrectionAberrationMode
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
        public enum MetadataControlAELock
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
        public enum MetadataControlAWBMode
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
        public enum MetadataControlAWBLock
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
        public enum MetadataColorCorrectionMode
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
        public enum MetadataControlAEAntibandingMode
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
        /// The metadata for <c>scaler</c> available formats.
        /// </summary>
        public enum MetadataScalerAvailableFormats
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
        /// The metadata for <c>scaler</c> available stream configurations.
        /// </summary>
        public enum MetadataScalerAvailableStreamConfigurations
        {
            /// <summary>
            /// The <c>scaler</c> available stream configuration: Output
            /// </summary>
            OUTPUT = 0,

            /// <summary>
            /// The <c>scaler</c> available stream configuration: Input
            /// </summary>
            INPUT,
        }

        /// <summary>
        /// The metadata for the control AE state.
        /// </summary>
        public enum MetadataControlAEState
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
        public enum MetadataControlAWBState
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

        #if PLATFORM_LUMIN
        /// <summary>
        /// Gets or sets the raw video frame capture filter.
        /// This can be used to specify if the OnRawVideoFrameAvailableYUV callback
        /// will be passed all 3 YUV planes (RawVideoFrameData.IntensityAndColor) or just the Y (RawVideoFrameData.Intensity) plane.
        /// </summary>
        public static RawVideoFrameData RawVideoFrameDataFilter { get; set; } = RawVideoFrameData.IntensityAndColor;

        /// <summary>
        /// Gets or sets the maximum number of raw video frames that are queued per a Unity frame.
        /// If more frames are received than this number, the oldest frames are dropped.
        /// 1 is the default; this means only the most recent raw video frame will be sent
        /// to the user per game frame.
        /// You can set this value to 0 to have no limit to the size of the queue,
        /// however, if it is not drained quick enough, you may run out of memory.
        /// </summary>
        public static uint MaximumRawVideoFrameQueue
        {
            get
            {
                lock (rawVideoFrameQueueLockObject)
                {
                    return maximumRawVideoFrameQueue;
                }
            }

            set
            {
                lock (rawVideoFrameQueueLockObject)
                {
                    maximumRawVideoFrameQueue = value;
                }
            }
        }

        /// <summary>
        /// Gets the amount of raw video frames dropped last update
        /// beyond the currently set MaximumRawVideoFrameQueue.
        /// </summary>
        public static uint DroppedRawVideoFramesLastUpdate
        {
            get
            {
                return Instance.droppedRawVideoFramesLastUpdate;
            }
        }

        /// <summary>
        /// Gets the camera settings object.
        /// </summary>
        public static MLCamera.GeneralSettings Settings
        {
            get
            {
                return Instance.settings;
            }

            private set
            {
                Instance.settings = value;
            }
        }

        /// <summary>
        /// Gets the texture for the camera preview render.
        /// (null) when preview is not available yet.
        /// </summary>
        public static Texture2D PreviewTexture2D
        {
            get
            {
                if (Instance.Previewing)
                {
                    return Instance.previewTexture2D;
                }
                else
                {
                    return null;
                }
            }

            private set
            {
                Instance.previewTexture2D = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the camera preview is enabled and textures are being rendered.
        /// </summary>
        public bool Previewing { get; private set; }

        /// <summary>
        /// Starts the Camera API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();

            // Not passing true since only CV Camera functions are unavailable if XR is not loaded.
            return MLCamera.BaseStart();
        }

        /// <summary>
        /// Connect to camera resource and register callbacks.
        /// Only one connection to the camera resource is supported at a time.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to connect to camera device due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed to connect to camera device due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        public static MLResult Connect()
        {
            return Instance.InternalConnect();
        }

        /// <summary>
        /// Disconnect from camera resource and unregister callbacks.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        public static MLResult Disconnect()
        {
            return Instance.InternalDisconnect();
        }

        /// <summary>
        /// Captures an image from the camera and writes a jpeg to the specified path.
        /// </summary>
        /// <param name="filePath">Path to write the captured image.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericBadType</c> if failed due to invalid capture type.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed due to on-going video recording.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        public static MLResult CaptureImage(string filePath)
        {
            return Instance.InternalCaptureImage(filePath);
        }

        /// <summary>
        /// Initiates a raw image capture request.  Register to OnRawImageAvailable event to get result if format is JPEG.
        /// OnRawImageAvailableYUV to get result if format is YUV_420_888
        /// If this function is invoked before the camera sensor has locked AE and AWB,
        /// it will be blocked until AE (Auto Exposure), AWB (Auto White balance) is locked and then starts to capture. The capture itself is asynchronous
        /// </summary>
        /// <param name="outputFormat">The output format for the output type.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericBadType</c> if failed due to invalid capture type.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed due to on-going video recording.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// </returns>
        public static MLResult CaptureRawImageAsync(OutputFormat outputFormat = OutputFormat.JPEG)
        {
            return Instance.InternalCaptureRawImageAsync(outputFormat);
        }

        /// <summary>
        /// Starts capturing a video to write as an MP4 to the specified path.
        /// </summary>
        /// <param name="filePath">Path to write the video.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericBadType</c> if failed to prepare for capture due to invalid capture type.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// </returns>
        public static MLResult StartVideoCapture(string filePath)
        {
            return Instance.InternalStartVideoCapture(filePath);
        }

        /// <summary>
        /// Starts capturing video from the color camera. Frames are returned with OnRawVideoFrameAvailableYUV callback.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericBadType</c> if failed to prepare for capture due to invalid capture type.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// </returns>
        public static MLResult StartRawVideoCapture()
        {
            return Instance.InternalStartRawVideoCapture();
        }

        /// <summary>
        /// Stops capturing video. If initiated with StartVideoCapture(string) also
        /// write as an MP4 to the path specified with StartVideoCapture(string).
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed to start video recording due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        public static MLResult StopVideoCapture()
        {
            return Instance.InternalStopVideoCapture();
        }

        /// <summary>
        /// Prepare a capture for manual submission, allowing modifying of the capture settings.
        /// </summary>
        /// <param name="captureType">The type of capture you will be submitting. This must match the capture type of SubmitCapture()</param>
        /// <param name="captureSettings">Structure containing all of the existing capture settings that can be viewed or modified. Must be passed into SubmitCapture</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to prepare for capture due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to prepare for capture due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericBadType</c> if failed to prepare for capture due to invalid capture type.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed to prepare for capture due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        public static MLResult PrepareCapture(MLCamera.CaptureType captureType, ref MLCamera.CaptureSettings captureSettings)
        {
            return Instance.InternalPrepareCapture(captureType, ref captureSettings);
        }

        /// <summary>
        /// Submit a previously prepared capture and execute the capture itself.
        /// If MLCamera.CaptureType.Video is submitted, users must call StopVideoCapture() to stop capture at a later time.
        /// </summary>
        /// <param name="captureSettings">Structure containing the settings used for this capture</param>
        /// <param name="filePath">When captureType is of type Video or Image (but not ImageRaw), filePath must be a valid target path for the output capture</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed to capture image due to on-going video recording.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to start capture or recording due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to start capture or recording due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed to start capture or recording due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed to start video recording due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        public static MLResult SubmitCapture(ref MLCamera.CaptureSettings captureSettings, string filePath = "")
        {
            return Instance.InternalSubmitCapture(captureSettings.CaptureType, ref captureSettings, filePath);
        }

        /// <summary>
        /// Get camera intrinsic parameter.
        /// Requires ComputerVision privilege.
        /// </summary>
        /// <param name="outParameters">Output structure containing intrinsic parameters on success.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if unable to retrieve intrinsic parameter.
        /// </returns>
        public static MLResult GetIntrinsicCalibrationParameters(out MLCamera.IntrinsicCalibrationParameters outParameters)
        {
            return Instance.InternalGetIntrinsicCalibrationParameters(MLCVCameraNativeBindings.CameraID.ColorCamera, out outParameters);
        }

        /// <summary>
        /// Get transform between world origin and the camera. This method relies on a camera timestamp
        /// that is normally acquired from the MLCameraResultExtras structure, therefore this method is
        /// best used within a capture callback to maintain as much accuracy as possible.
        /// Requires ComputerVision privilege.
        /// </summary>tran
        /// <param name="vcamTimestamp">Time in nanoseconds to request the transform.</param>
        /// <param name="outTransform">Output transformation matrix on success.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if outTransform parameter was not valid (null).
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed to obtain transform due to internal error.
        /// </returns>
        public static MLResult GetFramePose(ulong vcamTimestamp, out Matrix4x4 outTransform)
        {
            return Instance.InternalGetFramePose(MLCVCameraNativeBindings.CameraID.ColorCamera, vcamTimestamp, out outTransform);
        }

        /// <summary>
        /// Returns the most recent error code.
        /// </summary>
        /// <returns>Most recent MLCameraError.</returns>
        public static MLCamera.ErrorType GetErrorCode()
        {
            return Instance.InternalGetErrorCode();
        }

        /// <summary>
        /// Begin previewing the camera image for a live feed. StopPreview() must be called when finished
        /// Camera must be connected first with Connect()
        /// </summary>
        /// <param name="texture">Texture reference that the live footage should be drawn on</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the texture passed in is invalid (null)
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if some other failure occurred.
        /// </returns>
        public static MLResult StartPreview(Texture2D texture)
        {
            if (texture == null)
            {
                MLResult result = MLResult.Create(MLResult.Code.InvalidParam, "Camera Preview attempted to start with a null texture object");
                MLPluginLog.ErrorFormat("MLCamera.StartPreview failed to start preview. Reason: {0}", result);
                return result;
            }

            return Instance.InternalStartPreview(texture);
        }

        /// <summary>
        /// Begin previewing the camera image for a live feed. StopPreview() must be called when finished
        /// Camera must be connected first with Connect()
        /// </summary>
        /// <returns>The texture that the live footage will be drawn on. null is returned if any errors occurred.</returns>
        public static Texture2D StartPreview()
        {
            if (Instance.InternalStartPreview(null).IsOk)
            {
                return PreviewTexture2D;
            }

            MLPluginLog.Error("MLCamera.StartPreview failed to start preview, null texture will be returned");
            return null;
        }

        /// <summary>
        /// Handles correctly pausing and resuming the MLCamera API. This is includes, stop/resume capture,
        /// stop/resume preview, camera
        /// </summary>
        /// <param name="pause">The pause state of the application.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        public static MLResult ApplicationPause(bool pause)
        {
            MLResult result = pause ? Instance.Pause() : Instance.Resume();
            if (!result.IsOk)
            {
                MLPluginLog.ErrorFormat("MLCamera.ApplicationPause failed to {0} the camera.", pause ? "pause" : "resume");
            }

            return result;
        }

        /// <summary>
        /// Stop previewing the live camera feed.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if you are calling stop without having already started previewing.
        /// </returns>
        public static MLResult StopPreview()
        {
            if (!Instance.Previewing)
            {
                MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Camera preview has not been started");
                MLPluginLog.WarningFormat("MLCamera.StopPreview failed to stop camera preview. Reason: {0}", result);
                return result;
            }

            return Instance.InternalStopPreview();
        }

#if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Start Camera API and set up callbacks.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        protected override MLResult StartAPI()
        {
            // Used by CV functions.
            if (!MLDevice.IsReady())
            {
                MLPluginLog.WarningFormat("MLCamera API is attempting to start before the MagicLeap XR Loader has been initialiazed, this could cause issues with MLCamera CV features. If your application needs these features please wait to start API until Monobehavior.Start and if issue persists make sure ProjectSettings/XR/Initialize On Startup is enabled.");
            }

            MLResult.Code resultCode = MLResult.Code.UnspecifiedFailure;

            // Pin down the callbacks structures since the native library keeps a reference to them.
            this.statusCallbacksUnmanaged = Marshal.AllocHGlobal(Marshal.SizeOf(this.statusCallbacks));
            this.captureCallbacksUnmanaged = Marshal.AllocHGlobal(Marshal.SizeOf(this.captureCallbacks));

            MLResult result = MLHeadTracking.Start();
            if (result.IsOk)
            {
                result = MLHeadTracking.GetState(out MLHeadTracking.State headTrackingState);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLCamera.StartAPI failed to get head pose state. Reason: {0}", result);
                }

                this.headTrackerHandle = headTrackingState.Handle;
                MLHeadTracking.Stop();
            }
            else
            {
                MLPluginLog.ErrorFormat("MLCamera.StartAPI failed to get head pose state. MLHeadTracking could not be successfully started.");
            }

            result = MLResult.Create(resultCode);

            // Create CV camera tracker.
            resultCode = MLCVCameraNativeBindings.MLCVCameraTrackingCreate(ref this.cameraCVTrackerHandle);
            result = MLResult.Create(resultCode);
            if (resultCode == MLResult.Code.PrivilegeDenied)
            {
                MLPluginLog.WarningFormat("MLCamera.StartAPI missing ComputerVision privilege, CV specific features disabled.");
                result = MLResult.Create(MLResult.Code.Ok);
            }
            else if (!result.IsOk)
            {
                MLPluginLog.ErrorFormat("MLCamera.StartAPI failed to create native cv camera tracker. Reason: {0}", result);
                return result;
            }

            return result;
        }
#endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Cleans up unmanaged memory.
        /// </summary>
        /// <param name="isSafeToAccessManagedObject">Informs the cleanup process it's safe to clear the initialized MLCamera callbacks.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObject)
        {
            if (isSafeToAccessManagedObject)
            {
                MLDevice.Unregister(this.Update);

                this.InternalDisconnect();

                capturePath = string.Empty;
            }
            else
            {
                this.DisconnectNative();
            }

            // Release callback structure memory.
            Marshal.FreeHGlobal(this.statusCallbacksUnmanaged);
            Marshal.FreeHGlobal(this.captureCallbacksUnmanaged);

            // Used by CV functions.

            // Head Tracker is destroyed in the XR package, it cannot be destroyed in the plugin.
            // Destroy cv camera tracker.
            MLResult.Code resultCode = MLCVCameraNativeBindings.MLCVCameraTrackingDestroy(this.cameraCVTrackerHandle);
            if (resultCode != MLResult.Code.Ok)
            {
                MLPluginLog.ErrorFormat("MLCamera.CleanupAPI failed to destroy native cv camera tracker. Reason: {0}", resultCode);
            }
        }

        /// <summary>
        /// Process any pending video queues.
        /// </summary>
        protected override void Update()
        {
            // Copy queued events
            {
                lock (rawVideoFrameQueueLockObject)
                {
                    Queue<Action> dispatchRawVideoFramesQueue = queuedRawVideoFrames;
                    queuedRawVideoFrames = MLCamera.dispatchRawVideoFramesQueue;
                    MLCamera.dispatchRawVideoFramesQueue = dispatchRawVideoFramesQueue;

                    this.droppedRawVideoFramesLastUpdate = droppedRawVideoFrames;
                    droppedRawVideoFrames = 0;
                }
            }

            // Dispatch raw frame queued events
            foreach (Action action in dispatchRawVideoFramesQueue)
            {
                action();
            }

            dispatchRawVideoFramesQueue.Clear();
        }

        /// <summary>
        /// Add a new frame to the raw video queue.
        /// </summary>
        /// <param name="callback">The callback when the frame completes.</param>
        private static void QueueRawVideoFrameCallback(Action callback)
        {
            lock (rawVideoFrameQueueLockObject)
            {
                // Drop oldest frame if we have reached the limit.
                if (maximumRawVideoFrameQueue > 0 &&
                    queuedRawVideoFrames.Count == maximumRawVideoFrameQueue)
                {
                    queuedRawVideoFrames.Dequeue();
                    droppedRawVideoFrames++;
                }

                queuedRawVideoFrames.Enqueue(callback);
            }
        }

        /// <summary>
        /// static instance of the MLCamera class
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLCamera.IsValidInstance())
            {
                MLCamera._instance = new MLCamera();
            }
        }

        /// <summary>
        /// Create a preview texture.
        /// </summary>
        /// <param name="width">The width of the preview texture.</param>
        /// <param name="height">The height of the preview texture.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the request completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private static Texture2D CreatePreviewTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            int size = width * height;
            Color[] colors = new Color[size];

            for (int i = 0; i < size; ++i)
            {
                colors[i] = Color.black;
            }

            tex.SetPixels(colors);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Issues an event when the camera becomes available.
        /// </summary>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnDataCallback))]
        private static void OnDeviceAvailableCallback(IntPtr data)
        {
            MLThreadDispatch.Call(OnDeviceAvailable);
        }

        /// <summary>
        /// Issues an event when the camera becomes unavailable.
        /// </summary>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnDataCallback))]
        private static void OnDeviceUnavailableCallback(IntPtr data)
        {
            MLThreadDispatch.Call(OnDeviceUnavailable);
        }

        /// <summary>
        /// Issues an event when the camera is opened.
        /// </summary>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnDataCallback))]
        private static void OnDeviceOpenedCallback(IntPtr data)
        {
            MLThreadDispatch.Call(OnDeviceOpened);
        }

        /// <summary>
        /// Issues an event when the camera is closed.
        /// </summary>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnDataCallback))]
        private static void OnDeviceClosedCallback(IntPtr data)
        {
            MLThreadDispatch.Call(OnDeviceClosed);
        }

        /// <summary>
        /// Issues an event when the camera is disconnected.
        /// </summary>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnDataCallback))]
        private static void OnDeviceDisconnectedCallback(IntPtr data)
        {
            MLThreadDispatch.Call(OnDeviceClosed);
        }

        /// <summary>
        /// Issues an event when there is an error with the camera.
        /// </summary>
        /// <param name="error">The error type that is being reported.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnErrorDataCallback))]
        private static void OnDeviceErrorCallback(MLCamera.ErrorType error, IntPtr data)
        {
            MLThreadDispatch.Call(error, OnDeviceError);
        }

        /// <summary>
        /// Issues an event when camera capture has started.
        /// </summary>
        /// <param name="extra">A structure containing extra result information.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnResultExtrasRefDataCallback))]
        private static void OnCaptureStartedCallback(ref MLCamera.ResultExtras extra, IntPtr data)
        {
            MLCamera.ResultExtras lambdaExtra = extra;
            MLThreadDispatch.Call(lambdaExtra, OnCaptureStarted);
        }

        /// <summary>
        /// Issues an event when camera capture has failed.
        /// </summary>
        /// <param name="extra">A structure containing extra result information.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnResultExtrasRefDataCallback))]
        private static void OnCaptureFailedCallback(ref MLCamera.ResultExtras extra, IntPtr data)
        {
            MLCamera.ResultExtras lambdaExtra = extra;
            MLThreadDispatch.Call(lambdaExtra, OnCaptureFailed);
        }

        /// <summary>
        /// Issues an event when the camera capture buffer is lost.
        /// </summary>
        /// <param name="extra">A structure containing extra result information.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnResultExtrasRefDataCallback))]
        private static void OnCaptureBufferLostCallback(ref MLCamera.ResultExtras extra, IntPtr data)
        {
            MLCamera.ResultExtras lambdaExtra = extra;
            MLThreadDispatch.Call(lambdaExtra, OnCaptureBufferLost);
        }

        /// <summary>
        /// Issues an event when there has been camera capture progress.
        /// </summary>
        /// <param name="metadataHandle">A pointer to the metadata handle.</param>
        /// <param name="extra">A structure containing extra result information.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnHandleAndResultExtrasRefDataCallback))]
        private static void OnCaptureProgressedCallback(ulong metadataHandle, ref MLCamera.ResultExtras extra, IntPtr data)
        {
            MLCamera.ResultExtras lambdaExtra = extra;
            MLThreadDispatch.Call(lambdaExtra, capturePath, OnCaptureProgressed);
        }

        /// <summary>
        /// Issues an event when camera capture has completed.
        /// </summary>
        /// <param name="metadataHandle">A pointer to the metadata handle.</param>
        /// <param name="extra">A structure containing extra result information.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnHandleAndResultExtrasRefDataCallback))]
        private static void OnCaptureCompletedCallback(ulong metadataHandle, ref MLCamera.ResultExtras extra, IntPtr data)
        {
            string capturePath = MLCamera.capturePath;
            MLCamera.capturePath = string.Empty;

            MLCamera.ResultExtras lambdaExtra = extra;
            MLThreadDispatch.Call(lambdaExtra, capturePath, OnCaptureCompleted);

            MLCamera.ResultSettings lambdaSettings = new MLCamera.ResultSettings();
            lambdaSettings.PopulateSettings(metadataHandle);

            MLThreadDispatch.Call(lambdaExtra, capturePath, lambdaSettings, OnCaptureCompletedSettings);
        }

        /// <summary>
        /// Issues an event when the camera image buffer is available.
        /// </summary>
        /// <param name="output">A structure containing camera output information.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnOutputRefDataCallback))]
        private static void OnImageBufferAvailableCallback(ref MLCamera.Output output, IntPtr data)
        {
            if ((output.Format == OutputFormat.JPEG) &&
                (output.PlaneCount == 1))
            {
                byte[] jpegBytes = new byte[output.Planes[0].Size];
                Marshal.Copy(output.Planes[0].Data, jpegBytes, 0, jpegBytes.Length);

                MLThreadDispatch.Call(jpegBytes, OnRawImageAvailable);
            }
            else if ((output.Format == OutputFormat.YUV_420_888) &&
                (output.PlaneCount == 3))
            {
                YUVFrameInfo frameInfo = YUVFrameInfo.Create();
                frameInfo.Y.CopyFromPlane(output.Planes[(int)MLCameraNativeBindings.YUVPlaneIndex.YPlane]);
                frameInfo.U.CopyFromPlane(output.Planes[(int)MLCameraNativeBindings.YUVPlaneIndex.UPlane]);
                frameInfo.V.CopyFromPlane(output.Planes[(int)MLCameraNativeBindings.YUVPlaneIndex.VPlane]);

                MLThreadDispatch.Call(frameInfo, OnRawImageAvailableYUV);
            }
        }

        /// <summary>
        /// Issues an event when the camera video buffer is available.
        /// </summary>
        /// <param name="output">A structure containing camera output information.</param>
        /// <param name="extra">A structure containing extra result information.</param>
        /// <param name="frameMetadata">A structure containing frame meta data.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        [AOT.MonoPInvokeCallback(typeof(MLCameraNativeBindings.OnOutputRefResultExtrasRefFrameMetadataRefDataCallback))]
        private static void OnVideoBufferAvailableCallback(ref MLCamera.Output output, ref MLCamera.ResultExtras extra, ref MLCamera.FrameMetadata frameMetadata, IntPtr data)
        {
            if ((output.Format == OutputFormat.YUV_420_888) &&
                (output.PlaneCount == 3))
            {
                if (OnRawVideoFrameAvailableYUV != null)
                {
                    MLCamera.ResultExtras lambdaExtra = extra;
                    MLCamera.FrameMetadata lambdaFrameMetadata = frameMetadata;
                    YUVFrameInfo frameInfo = YUVFrameInfo.Create();
                    frameInfo.Y.CopyFromPlane(output.Planes[(int)MLCameraNativeBindings.YUVPlaneIndex.YPlane]);

                    if (RawVideoFrameDataFilter == RawVideoFrameData.IntensityAndColor)
                    {
                        frameInfo.U.CopyFromPlane(output.Planes[(int)MLCameraNativeBindings.YUVPlaneIndex.UPlane]);
                        frameInfo.V.CopyFromPlane(output.Planes[(int)MLCameraNativeBindings.YUVPlaneIndex.VPlane]);
                    }

                    QueueRawVideoFrameCallback(() =>
                    {
                        OnRawVideoFrameAvailableYUV(lambdaExtra, frameInfo, lambdaFrameMetadata);
                    });
                }
            }
        }

        /// <summary>
        /// Issues an event when a render event has occurred.
        /// </summary>
        private void GLPluginEvent()
        {
            GL.IssuePluginEvent(MLCameraNativeBindings.GetPluginRenderCallback(), 0);
        }

        /// <summary>
        /// Get the intrinsic calibration parameters.
        /// </summary>
        /// <param name="cameraId">The id of the camera.</param>
        /// <param name="outParameters">The intrinsic calibration parameters.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained result extras successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to obtain result extras due to invalid input parameter.
        /// </returns>
        private MLResult InternalGetIntrinsicCalibrationParameters(MLCVCameraNativeBindings.CameraID cameraId, out MLCamera.IntrinsicCalibrationParameters outParameters)
        {
            outParameters = new MLCamera.IntrinsicCalibrationParameters();

            MLCVCameraNativeBindings.IntrinsicCalibrationParametersNative internalParameters =
                MLCVCameraNativeBindings.IntrinsicCalibrationParametersNative.Create();

            MLResult.Code resultCode = MLCVCameraNativeBindings.MLCVCameraGetIntrinsicCalibrationParameters(this.cameraCVTrackerHandle, cameraId, ref internalParameters);

            MLResult parametersResult = MLResult.Create(resultCode);
            if (!parametersResult.IsOk)
            {
                MLPluginLog.ErrorFormat("MLCamera.InternalGetIntrinsicCalibrationParameters failed to get camera parameters. Reason: {0}", parametersResult);
            }
            else
            {
                outParameters.Width = internalParameters.Width;
                outParameters.Height = internalParameters.Height;
                outParameters.FocalLength = new Vector2(internalParameters.FocalLength.X, internalParameters.FocalLength.Y);
                outParameters.PrincipalPoint = new Vector2(internalParameters.PrincipalPoint.X, internalParameters.PrincipalPoint.Y);
                outParameters.FOV = internalParameters.FOV;
                outParameters.Distortion = new double[internalParameters.Distortion.Length];
                internalParameters.Distortion.CopyTo(outParameters.Distortion, 0);
            }

            return parametersResult;
        }

        /// <summary>
        /// Pause the camera capture.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        private MLResult Pause()
        {
            this.resumeConnect = this.resumePreview = false;

            MLResult result;

            if (this.cameraConnectionEstablished)
            {
                this.resumeConnect = true;
                if (this.Previewing)
                {
                    this.resumePreview = true;

                    result = StopPreview();
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLCamera.Pause failed to stop camera preview. Reason: {0}", result);
                        return result;
                    }
                }

                result = Disconnect();
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLCamera.Pause failed to disconnect camera. Reason: {0}", result);
                    return result;
                }
            }

            return MLResult.Create(MLResult.Code.Ok);
        }

        /// <summary>
        /// Resume the camera capture.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to connect to camera device due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a parameter is invalid (null).
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed to connect to camera device due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        private MLResult Resume()
        {
            MLResult result;

            if (this.resumeConnect)
            {
                result = Connect();
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLCamera.Resume failed to connect camera. Reason: {0}", result);
                    return result;
                }

                if (this.resumePreview)
                {
                    result = StartPreview(this.previewTexture2D);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLCamera.Resume failed to start camera preview. Reason: {0}", result);
                        return result;
                    }
                }
            }

            return MLResult.Create(MLResult.Code.Ok);
        }

        /// <summary>
        /// Disconnect and clear the capture callbacks for the camera.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private MLResult DisconnectNative()
        {
            // Clear out the callbacks. It is worth noting that the OnDeviceDisconnected() callback is unrelated to MLCameraDisconnect().
            MLResult.Code resultCode = MLCameraNativeBindings.MLCameraSetDeviceStatusCallbacks(IntPtr.Zero, IntPtr.Zero);
            if (resultCode != MLResult.Code.Ok)
            {
                MLPluginLog.ErrorFormat("MLCamera.DisconnectNative failed to clear device status callbacks for MLCamera. Reason: {0}", MLResult.CodeToString(resultCode));
            }

            resultCode = MLCameraNativeBindings.MLCameraSetCaptureCallbacksEx(IntPtr.Zero, IntPtr.Zero);

            if (resultCode != MLResult.Code.Ok)
            {
                MLPluginLog.ErrorFormat("MLCamera.DisconnectNative failed to clear capture callbacks for MLCamera. Reason: {0}", MLResult.CodeToString(resultCode));
            }

            resultCode = MLCameraNativeBindings.MLCameraDisconnect();
            var result = MLResult.Create(resultCode);

            if (!result.IsOk)
            {
                MLPluginLog.ErrorFormat("MLCamera.DisconnectNative failed disconnecting from camera. Reason: {0}", result);
            }

            // Return result fo MLCameraDisconnect()
            return result;
        }

        /// <summary>
        /// Establish a connection to the camera.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if connected to camera device successfully.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to connect to camera device due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed  connecting the camera due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private MLResult InternalConnect()
        {
            var result = MLResult.Create(MLResult.Code.Ok, "Camera connection already established");

            if (!this.cameraConnectionEstablished)
            {
                this.statusCallbacks.OnDeviceAvailable = OnDeviceAvailableCallback;
                this.statusCallbacks.OnDeviceUnavailable = OnDeviceUnavailableCallback;
                this.statusCallbacks.OnDeviceOpened = OnDeviceOpenedCallback;
                this.statusCallbacks.OnDeviceClosed = OnDeviceClosedCallback;
                this.statusCallbacks.OnDeviceDisconnected = OnDeviceDisconnectedCallback;
                this.statusCallbacks.OnDeviceError = OnDeviceErrorCallback;
                this.statusCallbacks.OnPreviewBufferAvailable = null;

                Marshal.StructureToPtr(this.statusCallbacks, this.statusCallbacksUnmanaged, false);
                MLResult.Code resultCode = MLCameraNativeBindings.MLCameraSetDeviceStatusCallbacks(this.statusCallbacksUnmanaged, IntPtr.Zero);
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLCamera.InternalConnect failed to set device status callbacks for MLCamera. Reason: {0}", result);
                    return result;
                }

                this.captureCallbacks.Version = 1u;
                this.captureCallbacks.OnCaptureStarted = OnCaptureStartedCallback;
                this.captureCallbacks.OnCaptureFailed = OnCaptureFailedCallback;
                this.captureCallbacks.OnCaptureBufferLost = OnCaptureBufferLostCallback;
                this.captureCallbacks.OnCaptureProgressed = OnCaptureProgressedCallback;
                this.captureCallbacks.OnCaptureCompleted = OnCaptureCompletedCallback;
                this.captureCallbacks.OnImageBufferAvailable = OnImageBufferAvailableCallback;
                this.captureCallbacks.OnVideoBufferAvailable = OnVideoBufferAvailableCallback;

                Marshal.StructureToPtr(this.captureCallbacks, this.captureCallbacksUnmanaged, false);
                resultCode = MLCameraNativeBindings.MLCameraSetCaptureCallbacksEx(this.captureCallbacksUnmanaged, IntPtr.Zero);
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    // Clean up device status callbacks if capture callbacks fails
                    // TODO: Check for result.
                    MLCameraNativeBindings.MLCameraSetDeviceStatusCallbacks(IntPtr.Zero, IntPtr.Zero);
                    MLPluginLog.ErrorFormat("MLCamera.InternalConnect failed to set capture callbacks for MLCamera. Reason: {0}", result);
                    return result;
                }

                // This call on an average can take anywhere between 1500ms to 2500ms.
                resultCode = MLCameraNativeBindings.MLCameraConnect();
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLCamera.InternalConnect failed connecting to camera. Reason: {0}", result);
                    return result;
                }

                this.cameraConnectionEstablished = result.IsOk;

                Settings = new MLCamera.GeneralSettings();
                result = Settings.PopulateCharacteristics();

                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLCamera.InternalConnect failed to populate characteristics for MLCamera. Reason: {0}", result);
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Disconnect the camera.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private MLResult InternalDisconnect()
        {
            var result = MLResult.Create(MLResult.Code.Ok, "No camera connection established");

            if (this.cameraConnectionEstablished)
            {
                if (this.Previewing)
                {
                    this.InternalStopPreview();
                }

                if (this.captureVideoStarted)
                {
                    // TODO: check for result?
                    this.InternalStopVideoCapture();
                }

                result = this.DisconnectNative();

                // Assume that connection is no longer established
                // even if there is some failure disconnecting.
                this.cameraConnectionEstablished = false;
            }

            return result;
        }

        /// <summary>
        /// Start the camera preview.
        /// </summary>
        /// <param name="texture">The texture that will contain the preview.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        private MLResult InternalStartPreview(Texture2D texture)
        {
            MLResult result;

            if (!Instance.cameraConnectionEstablished)
            {
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Camera Preview is unavailable until the camera has been connected. Call Connect() first");
                MLPluginLog.ErrorFormat("MLCamera.InternalStartPreview failed. Reason: {0}", result);
                return result;
            }

            if (Instance.Previewing)
            {
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Camera Preview has already been started. Use PreviewTexture2D to access the preview");
                MLPluginLog.ErrorFormat("MLCamera.InternalStartPreview failed. Reason: {0}", result);
                return result;
            }

            if (texture == null)
            {
                texture = CreatePreviewTexture(PreviewTextureWidth, PreviewTextureHeight);
            }

            PreviewTexture2D = texture;

            MLCameraNativeBindings.SetTextureFromUnity(texture.GetNativeTexturePtr(), PreviewTextureWidth, PreviewTextureHeight);
            this.Previewing = true;

            MLDevice.RegisterEndOfFrameUpdate(this.GLPluginEvent);

            return MLResult.Create(MLResult.Code.Ok);
        }

        /// <summary>
        /// Stop the camera preview.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// </returns>
        private MLResult InternalStopPreview()
        {
            MLDevice.UnregisterEndOfFrameUpdate(this.GLPluginEvent);

            this.Previewing = false;

            GL.IssuePluginEvent(MLCameraNativeBindings.GetPluginRenderCleanupCallback(), 0);

            return MLResult.Create(MLResult.Code.Ok);
        }

        /// <summary>
        /// Submit the camera capture.
        /// </summary>
        /// <param name="captureType">The type of capture.</param>
        /// <param name="captureSettings">The settings for the capture.</param>
        /// <param name="filePath">The path where the capture should be saved.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to start due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to start due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed to start due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private MLResult InternalSubmitCapture(MLCamera.CaptureType captureType, ref MLCamera.CaptureSettings captureSettings, string filePath = "")
        {
            MLResult result;
            MLResult.Code resultCode;

            if (!this.captureVideoStarted)
            {
                if (this.cameraConnectionEstablished)
                {
                    resultCode = captureSettings.ApplySettings();
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLCamera.InternalSubmitCapture failed attempting to apply capture settings to the prepared capture. Reason: {0}", result);
                        return result;
                    }

                    switch (captureType)
                    {
                        case MLCamera.CaptureType.Image:
                        {
                            resultCode = MLCameraNativeBindings.MLCameraCaptureImage(filePath);
                            result = MLResult.Create(resultCode);
                            if (!result.IsOk)
                            {
                                MLPluginLog.ErrorFormat("MLCamera.InternalSubmitCapture failed capturing image. Reason: {0}", result);
                                return result;
                            }

                            capturePath = filePath;

                            break;
                        }

                        case MLCamera.CaptureType.ImageRaw:
                        {
                            resultCode = MLCameraNativeBindings.MLCameraCaptureImageRaw();
                            result = MLResult.Create(resultCode);
                            if (!result.IsOk)
                            {
                                MLPluginLog.ErrorFormat("MLCamera.InternalSubmitCapture failed capturing raw image. Reason: {0}", result);
                                return result;
                            }

                            break;
                        }

                        case MLCamera.CaptureType.Video:
                        {
                            resultCode = MLCameraNativeBindings.MLCameraCaptureVideoStart(filePath);
                            result = MLResult.Create(resultCode);
                            if (!result.IsOk)
                            {
                                MLPluginLog.ErrorFormat("MLCamera.InternalSubmitCapture failed capturing video. Reason: {0}", result);
                                return result;
                            }

                            this.captureVideoStarted = true;
                            capturePath = filePath;

                            break;
                        }
                    }
                }
                else
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Camera connection not established, call Connect()");
                    MLPluginLog.ErrorFormat("MLCamera.InternalSubmitCapture failed to submit capture. Reason: {0}", result);
                }
            }
            else
            {
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Already capturing video.");
                MLPluginLog.ErrorFormat("MLCamera.InternalSubmitCapture failed trying to submit capture. Reason: {0}", result);
            }

            return MLResult.Create(MLResult.Code.Ok);
        }

        /// <summary>
        /// Get the frame pose.
        /// </summary>
        /// <param name="cameraId">The camera id.</param>
        /// <param name="vcamTimestamp">The timestamp of the frame pose.</param>
        /// <param name="outTransform">The transform of the frame pose.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        private MLResult InternalGetFramePose(MLCVCameraNativeBindings.CameraID cameraId, ulong vcamTimestamp, out Matrix4x4 outTransform)
        {
            MagicLeapNativeBindings.MLTransform outInternalTransform = new MagicLeapNativeBindings.MLTransform();

            MLResult.Code resultCode = MLCVCameraNativeBindings.MLCVCameraGetFramePose(this.cameraCVTrackerHandle, this.headTrackerHandle, cameraId, vcamTimestamp, ref outInternalTransform);
            MLResult poseResult = MLResult.Create(resultCode);

            if (!poseResult.IsOk)
            {
                MLPluginLog.ErrorFormat("MLCamera.InternalGetFramePose failed to get camera frame pose. Reason: {0}", poseResult);
                outTransform = new Matrix4x4();
            }
            else
            {
                outTransform = MLConvert.ToUnity(outInternalTransform);
            }

            return poseResult;
        }

        /// <summary>
        /// Prepare the camera capture.
        /// </summary>
        /// <param name="captureType">The type of capture.</param>
        /// <param name="captureSettings">The capture settings.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the request completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private MLResult InternalPrepareCapture(MLCamera.CaptureType captureType, ref MLCamera.CaptureSettings captureSettings)
        {
            MLResult result;

            if (!this.captureVideoStarted)
            {
                if (this.cameraConnectionEstablished)
                {
                    ulong prepareHandle = MagicLeapNativeBindings.InvalidHandle;
                    MLResult.Code resultCode = MLCameraNativeBindings.MLCameraPrepareCapture(captureType, ref prepareHandle);
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLCamera.InternalPrepareCapture failed to prepare capture for capture type {0}. Reason: {1}", captureType.ToString(), result);
                        return result;
                    }

                    resultCode = captureSettings.PopulateSettings(prepareHandle, captureType);
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLCamera.InternalPrepareCapture failed to populate existing capture settings. Reason: {0}", result);
                        return result;
                    }
                }
                else
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Camera connection not established, call Connect()");
                    MLPluginLog.ErrorFormat("MLCamera.InternalPrepareCapture failed to prepare capture. Reason: {0}", result);
                }
            }
            else
            {
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Already capturing video.");
                MLPluginLog.ErrorFormat("MLCamera.InternalPrepareCapture failed trying to prepare capture. Reason: {0}", result);
            }

            return MLResult.Create(MLResult.Code.Ok);
        }

        /// <summary>
        /// Start video capture.
        /// </summary>
        /// <param name="filePath">The path to store the video capture.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the request completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private MLResult InternalStartVideoCapture(string filePath)
        {
            ulong prepareHandle = MagicLeapNativeBindings.InvalidHandle;
            MLResult result;

            if (!this.captureVideoStarted)
            {
                if (this.cameraConnectionEstablished)
                {
                    MLResult.Code resultCode = MLCameraNativeBindings.MLCameraPrepareCapture(MLCamera.CaptureType.Video, ref prepareHandle);
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLCamera.InternalStartVideoCapture failed preparing camera for video capture. Reason: {0}", result);
                    }

                    if (MagicLeapNativeBindings.MLHandleIsValid(prepareHandle))
                    {
                        resultCode = MLCameraNativeBindings.MLCameraCaptureVideoStart(filePath);
                        result = MLResult.Create(resultCode);
                        if (!result.IsOk)
                        {
                            MLPluginLog.ErrorFormat("MLCamera.InternalStartVideoCapture failed starting to capture video. Reason: {0}", result);
                        }
                        else
                        {
                            this.captureVideoStarted = true;
                            capturePath = filePath;
                        }
                    }
                    else
                    {
                        MLPluginLog.Error("MLCamera.InternalStartVideoCapture failed preparing camera to capture video. Reason: Invalid capture prepare handle.");
                    }
                }
                else
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Camera connection not established, call Connect()");
                    MLPluginLog.ErrorFormat("MLCamera.InternalStartVideoCapture failed to capture video. Reason: {0}", result);
                }
            }
            else
            {
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Already capturing video.");
                MLPluginLog.ErrorFormat("MLCamera.InternalStartVideoCapture failed trying to capture video. Reason: {0}", result);
            }

            return result;
        }

        /// <summary>
        /// Start raw video capture.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the request completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private MLResult InternalStartRawVideoCapture()
        {
            ulong prepareHandle = MagicLeapNativeBindings.InvalidHandle;
            MLResult result;

            if (!this.captureVideoStarted)
            {
                if (this.cameraConnectionEstablished)
                {
                    MLResult.Code resultCode = MLCameraNativeBindings.MLCameraPrepareCapture(MLCamera.CaptureType.VideoRaw, ref prepareHandle);
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLCamera.InternalStartRawVideoCapture failed preparing camera for raw video capture. Reason: {0}", result);
                    }

                    if (MagicLeapNativeBindings.MLHandleIsValid(prepareHandle))
                    {
                        resultCode = MLCameraNativeBindings.MLCameraCaptureRawVideoStart();
                        result = MLResult.Create(resultCode);
                        if (!result.IsOk)
                        {
                            MLPluginLog.ErrorFormat("MLCamera.InternalStartRawVideoCapture failed starting to capture raw video. Reason: {0}", result);
                        }
                        else
                        {
                            this.captureVideoStarted = true;
                        }
                    }
                    else
                    {
                        MLPluginLog.Error("MLCamera.InternalStartRawVideoCapture failed preparing camera to capture raw video. Reason: Invalid capture prepare handle.");
                    }
                }
                else
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Camera connection not established, call Connect()");
                    MLPluginLog.ErrorFormat("MLCamera.InternalStartRawVideoCapture failed to capture raw video. Reason: {0}", result);
                }
            }
            else
            {
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Already capturing video.");
                MLPluginLog.ErrorFormat("MLCamera.InternalStartRawVideoCapture failed trying to capture raw video. Reason: {0}", result);
            }

            return result;
        }

        /// <summary>
        /// Stop video capture.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the request completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private MLResult InternalStopVideoCapture()
        {
            MLResult result;

            if (this.captureVideoStarted)
            {
                MLResult.Code resultCode = MLCameraNativeBindings.MLCameraCaptureVideoStop();
                result = MLResult.Create(resultCode);
                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLCamera.InternalStopVideoCapture failed stopping to capture video. Reason: {0}", result);
                }

                this.captureVideoStarted = false;
            }
            else
            {
                // TODO: Change to warning?
                result = MLResult.Create(MLResult.Code.Ok, "Video capture was never started");
                MLPluginLog.ErrorFormat("MLCamera.InternalStopVideoCapture failed stopping to capture video. Reason: {0}", result);
            }

            return result;
        }

        /// <summary>
        /// Capture an image to a file.
        /// </summary>
        /// <param name="filePath">The path where the image will be stored.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the request completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private MLResult InternalCaptureImage(string filePath)
        {
            ulong prepareHandleResult = MagicLeapNativeBindings.InvalidHandle;
            MLResult result;

            if (!this.captureVideoStarted)
            {
                if (this.cameraConnectionEstablished)
                {
                    MLResult.Code resultCode = MLCameraNativeBindings.MLCameraSetOutputFormat(MLCamera.OutputFormat.JPEG);
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLCamera.InternalCaptureImage failed setting camera output format to JPEG. Reason: {0}", result);
                    }

                    resultCode = MLCameraNativeBindings.MLCameraPrepareCapture(MLCamera.CaptureType.Image, ref prepareHandleResult);
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLCamera.InternalCaptureImage failed preparing camera for image capture. Reason: {0}", result);
                    }

                    // TODO: Check result instead?
                    if (MagicLeapNativeBindings.MLHandleIsValid(prepareHandleResult))
                    {
                        capturePath = filePath;
                        resultCode = MLCameraNativeBindings.MLCameraCaptureImage(filePath);
                        result = MLResult.Create(resultCode);
                        if (!result.IsOk)
                        {
                            MLPluginLog.ErrorFormat("MLCamera.InternalCaptureImage failed capturing image. Reason: {0}", result);
                        }
                    }
                    else
                    {
                        MLPluginLog.Error("MLCamera.InternalCaptureImage failed preparing camera to capture image. Reason: Invalid capture prepare handle.");
                    }
                }
                else
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Camera connection not established, call Connect()");
                    MLPluginLog.ErrorFormat("MLCamera.InternalCaptureImage failed to capture image. Reason: {0}", result);
                }
            }
            else
            {
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Already capturing video.");
                MLPluginLog.ErrorFormat("MLCamera.InternalCaptureImage failed trying to capture image. Reason: {0}", result);
            }

            return result;
        }

        /// <summary>
        /// Capture a raw image to a specified format.
        /// </summary>
        /// <param name="outputFormat">The file format.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the request completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private MLResult InternalCaptureRawImageAsync(OutputFormat outputFormat = OutputFormat.JPEG)
        {
            ulong prepareHandle = MagicLeapNativeBindings.InvalidHandle;
            MLResult result;

            if (!this.captureVideoStarted)
            {
                if (this.cameraConnectionEstablished)
                {
                    MLResult.Code resultCode = MLCameraNativeBindings.MLCameraSetOutputFormat(outputFormat);
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLCamera.InternalCaptureRawImageAsync failed setting camera output format to {0}. Reason: {1}", outputFormat, result);
                    }

                    resultCode = MLCameraNativeBindings.MLCameraPrepareCapture(MLCamera.CaptureType.ImageRaw, ref prepareHandle);
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLCamera.InternalCaptureRawImageAsync failed preparing camera for raw image capture. Reason: {0}", result);
                    }

                    // TODO: Check result instead?
                    if (MagicLeapNativeBindings.MLHandleIsValid(prepareHandle))
                    {
                        resultCode = MLCameraNativeBindings.MLCameraCaptureImageRaw();
                        result = MLResult.Create(resultCode);
                        if (!result.IsOk)
                        {
                            MLPluginLog.ErrorFormat("MLCamera.InternalCaptureRawImageAsync failed capturing raw image. Reason: {0}", result);
                        }
                    }
                    else
                    {
                        MLPluginLog.Error("MLCamera.InternalCaptureRawImageAsync failed preparing camera to capture raw image. Reason: Invalid capture prepare handle.");
                    }
                }
                else
                {
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Camera connection not established, call Connect()");
                    MLPluginLog.ErrorFormat("MLCamera.InternalCaptureRawImageAsync failed to capture raw image. Reason: {0}", result);
                }
            }
            else
            {
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Already capturing video.");
                MLPluginLog.ErrorFormat("MLCamera.InternalCaptureRawImageAsync failed trying to capture raw image. Reason: {0}", result);
            }

            return result;
        }

        /// <summary>
        /// Get the last camera error.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the request completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        private MLCamera.ErrorType InternalGetErrorCode()
        {
            MLCamera.ErrorType errorCode = MLCamera.ErrorType.None;

            MLResult.Code result = MLCameraNativeBindings.MLCameraGetErrorCode(ref errorCode);
            if (result != MLResult.Code.Ok)
            {
                MLPluginLog.ErrorFormat("MLCamera.InternalGetErrorCode failed getting camera error code. Reason: {0}", result);
                return MLCamera.ErrorType.Unknown;
            }

            return errorCode;
        }

        /// <summary>
        /// Per plane info for captured output
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PlaneInfo
        {
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
            /// Image data
            /// </summary>
            public IntPtr Data;

            /// <summary>
            /// Number of bytes in the image output data
            /// </summary>
            public uint Size;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <returns>Returns a new MLCamera.PlaneInfo structure.</returns>
            public static MLCamera.PlaneInfo Create()
            {
                return new MLCamera.PlaneInfo()
                {
                    Width = 0u,
                    Height = 0u,
                    Stride = 0u,
                    BytesPerPixel = 0u,
                    Data = IntPtr.Zero,
                    Size = 0u
                };
            }
        }

        /// <summary>
        /// Captured output
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Output
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MLCameraNativeBindings.MLCameraImagePlanes)]
            public MLCamera.PlaneInfo[] Planes;

            /// <summary>
            /// Supported output format specified by MLCamera.OutputFormat
            /// </summary>
            public MLCamera.OutputFormat Format;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <returns>Returns a new MLCamera.Output structure.</returns>
            public static MLCamera.Output Create()
            {
                return new MLCamera.Output()
                {
                    PlaneCount = 0,
                    Planes = Enumerable.Repeat<MLCamera.PlaneInfo>(MLCamera.PlaneInfo.Create(), MLCameraNativeBindings.MLCameraImagePlanes).ToArray(),
                    Format = MLCamera.OutputFormat.Unknown
                };
            }
        }

        /// <summary>
        /// Contains both the data and information necessary to read the data for a specific buffer in a YUV capture
        /// </summary>
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
                    IsValid = false,
                    Width = 0u,
                    Height = 0u,
                    Stride = 0u,
                    BytesPerPixel = 0u,
                    Data = new byte[0],
                    Size = 0u
                };
            }

            /// <summary>
            /// Copy the properties from a PlaneInfo structure.
            /// </summary>
            /// <param name="plane">The PlaneInfo to copy.</param>
            internal void CopyFromPlane(MLCamera.PlaneInfo plane)
            {
                this.Width = plane.Width;
                this.Height = plane.Height;
                this.Stride = plane.Stride;
                this.BytesPerPixel = plane.BytesPerPixel;
                this.Size = plane.Size;
                this.Data = new byte[this.Stride * this.Height];
                Marshal.Copy(plane.Data, this.Data, 0, this.Data.Length);
                this.IsValid = true;
            }
        }

        /// <summary>
        /// Contains the information and data of each of the available buffers/planes in a YUV capture
        /// </summary>
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
        [StructLayout(LayoutKind.Sequential)]
        public struct ResultExtras
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
            /// <returns>Returns a new ResultExtras structure.</returns>
            public static ResultExtras Create()
            {
                return new ResultExtras()
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
        [StructLayout(LayoutKind.Sequential)]
        public struct FrameMetadata
        {
            /// <summary>
            /// Frame exposure time for the given frame in nanoseconds.
            /// </summary>
            public long ExposureTimeNs;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <returns>Returns a new FrameMetadata structure.</returns>
            public static FrameMetadata Create()
            {
                return new FrameMetadata()
                {
                    ExposureTimeNs = 0
                };
            }
        }

        /// <summary>
        /// Contains camera intrinsic parameters.
        /// </summary>
        public struct IntrinsicCalibrationParameters
        {
            /// <summary>
            /// Camera width.
            /// </summary>
            public uint Width;

            /// <summary>
            /// Camera height.
            /// </summary>
            public uint Height;

            /// <summary>
            /// Camera focal length.
            /// </summary>
            public Vector2 FocalLength;

            /// <summary>
            /// Camera principle point.
            /// </summary>
            public Vector2 PrincipalPoint;

            /// <summary>
            /// Field of view.
            /// </summary>
            public float FOV;

            /// <summary>
            /// Distortion Coefficients.
            /// The distortion coefficients are in the following order:
            /// [k1, k2, p1, p2, k3]
            /// </summary>
            public double[] Distortion;
        }

        /// <summary>
        /// Information about the camera color correction transform.
        /// </summary>
        public class ColorCorrectionTransform
        {
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
            /// Initializes a new instance of the <see cref="ColorCorrectionTransform"/> class.
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
            public ColorCorrectionTransform(float x0, float x1, float x2, float y0, float y1, float y2, float z0, float z1, float z2)
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

                this.IsDirty = true;
            }

            /// <summary>
            /// Gets or sets a value indicating whether a change has been made, which requires an update.
            /// </summary>
            public bool IsDirty { get; set; }

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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
                    this.z2 = value;
                }
            }
        }

        /// <summary>
        /// Camera color correction gains.
        /// </summary>
        public class ColorCorrectionGains
        {
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
            /// Initializes a new instance of the <see cref="ColorCorrectionGains"/> class.
            /// </summary>
            /// <param name="red">The red color value.</param>
            /// <param name="greenEven">The green even color value.</param>
            /// <param name="greenOdd">The green odd color value.</param>
            /// <param name="blue">The blue color value.</param>
            public ColorCorrectionGains(float red, float greenEven, float greenOdd, float blue)
            {
                this.Red = red;
                this.GreenEven = greenEven;
                this.GreenOdd = greenOdd;
                this.Blue = blue;

                this.IsDirty = true;
            }

            /// <summary>
            /// Gets or sets a value indicating whether a change has been made, which requires an update.
            /// </summary>
            public bool IsDirty { get; set; }

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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
                    this.red = value;
                }
            }
        }

        /// <summary>
        /// The camera control AE target FPS range.
        /// </summary>
        public class ControlAETargetFPSRange
        {
            /// <summary>
            /// The minimum distance.
            /// </summary>
            private int minimum;

            /// <summary>
            /// The maximum distance.
            /// </summary>
            private int maximum;

            /// <summary>
            /// Initializes a new instance of the <see cref="ControlAETargetFPSRange"/> class.
            /// </summary>
            /// <param name="minimum">The minimum distance.</param>
            /// <param name="maximum">The maximum distance.</param>
            public ControlAETargetFPSRange(int minimum, int maximum)
            {
                this.Minimum = minimum;
                this.Maximum = maximum;

                this.IsDirty = true;
            }

            /// <summary>
            /// Gets or sets a value indicating whether a change has been made, which requires an update.
            /// </summary>
            public bool IsDirty { get; set; }

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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
                    this.maximum = value;
                }
            }
        }

        /// <summary>
        /// The camera <c>scaler</c> crop region.
        /// </summary>
        public class ScalerCropRegion
        {
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
            /// Initializes a new instance of the <see cref="ScalerCropRegion"/> class.
            /// </summary>
            /// <param name="left">The left crop region value.</param>
            /// <param name="top">The top crop region value.</param>
            /// <param name="right">The right crop region value.</param>
            /// <param name="bottom">The bottom crop region value.</param>
            public ScalerCropRegion(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;

                this.IsDirty = true;
            }

            /// <summary>
            /// Gets or sets a value indicating whether a change has been made, which requires an update.
            /// </summary>
            public bool IsDirty { get; set; }

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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
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
                    this.IsDirty = true;
                    this.bottom = value;
                }
            }
        }
        #endif
    }
}
