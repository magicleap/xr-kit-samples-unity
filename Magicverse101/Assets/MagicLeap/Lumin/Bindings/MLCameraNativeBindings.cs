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

// Disable warnings about missing documentation for native interop.
#pragma warning disable 1591

namespace UnityEngine.XR.MagicLeap.Native
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// This class defines the C# interface to the C functions/structures in "ml_camera.h".
    /// </summary>
    public class MLCameraNativeBindings : MagicLeapNativeBindings
    {
        /// <summary>
        /// Number of MLCamera image planes.
        /// </summary>
        public const int MLCameraImagePlanes = 3;

        /// <summary>
        /// The MLCamera library name.
        /// </summary>
        private const string MLCameraDll = "ml_camera";

        /// <summary>
        /// The MLCameraMetaData library name.
        /// </summary>
        private const string MLCameraMetaDll = "ml_camera_metadata";

        /// <summary>
        /// The MLCameraPlugin library name.
        /// </summary>
        private const string MLCameraPluginDll = "ml_camera_plugin";

        /// <summary>
        /// Initializes a new instance of the <see cref="MLCameraNativeBindings"/> class.
        /// </summary>
        protected MLCameraNativeBindings()
        {
        }

        /// <summary>
        /// A generic delegate for camera events.
        /// </summary>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        public delegate void OnDataCallback(IntPtr data);

        /// <summary>
        /// A delegate for camera error events.
        /// </summary>
        /// <param name="error">The type of error that was reported.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        public delegate void OnErrorDataCallback(MLCamera.ErrorType error, IntPtr data);

        /// <summary>
        /// A delegate for image buffer events.
        /// </summary>
        /// <param name="output">The camera output type.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        public delegate void OnOutputRefDataCallback(ref MLCamera.Output output, IntPtr data);

        /// <summary>
        /// A delegate for camera preview events.
        /// </summary>
        /// <param name="metadataHandle">A handle to the metadata.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        public delegate void OnHandleDataCallback(ulong metadataHandle, IntPtr data);

        /// <summary>
        /// A delegate for camera capture events.
        /// </summary>
        /// <param name="extra">A structure containing extra result information.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        public delegate void OnResultExtrasRefDataCallback(ref MLCamera.ResultExtras extra, IntPtr data);

        /// <summary>
        /// A delegate for camera capture events with additional information.
        /// </summary>
        /// <param name="metadataHandle">A handle to the metadata.</param>
        /// <param name="extra">A structure containing extra result information.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        public delegate void OnHandleAndResultExtrasRefDataCallback(ulong metadataHandle, ref MLCamera.ResultExtras extra, IntPtr data);

        /// <summary>
        /// A delegate for the video buffer events.
        /// </summary>
        /// <param name="output">A structure containing camera output information.</param>
        /// <param name="extra">A structure containing extra result information.</param>
        /// <param name="frameMetadata">A structure containing frame meta data.</param>
        /// <param name="data">Custom data returned when the callback is triggered, user metadata.</param>
        internal delegate void OnOutputRefResultExtrasRefFrameMetadataRefDataCallback(ref MLCamera.Output output, ref MLCamera.ResultExtras extra, ref MLCamera.FrameMetadata frameMetadata, IntPtr data);

        /// <summary>
        /// An enumeration for the plane index.
        /// </summary>
        internal enum YUVPlaneIndex : int
        {
            /// <summary>
            /// The YPlane index.
            /// </summary>
            YPlane = 0,

            /// <summary>
            /// The UPlane index.
            /// </summary>
            UPlane,

            /// <summary>
            /// The VPlane index.
            /// </summary>
            VPlane
        }

        /// <summary>
        /// Connect to camera device.
        /// <c>After this function returns, a preview stream will be created. The preview
        /// might not have good quailty image at beginning due to the camera sensor
        /// requires to adjust and lock the exposure(AE) and white balance(AWB). This
        /// process takes several frames and it might take up to half second in low light
        /// condition environment.</c>
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if connected to camera device successfully.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to connect to camera device due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed  connecting the camera due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraConnect();

        /// <summary>
        /// Disconnect from camera device.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if disconnected from camera device successfully.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraDisconnect();

        /// <summary>
        /// Prepare for capture.
        /// This API prepares capture per specified MLCamera.CaptureType by creating
        /// a capture request, and a handle to which is returned to the user, who can choose
        /// to manipulate the request data(metadata) via APIs defined in ml_camera_metadata.h
        /// before performing the capture.
        /// Shall be called after MLCameraConnect().
        /// </summary>
        /// <param name="type">Capture operation type.</param>
        /// <param name="outHandle">Handle to the capture metadata.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if prepared for capture successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to prepare for capture due to an invalid parameter.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to prepare for capture due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.BadType</c> if failed to prepare for capture due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraPrepareCapture(MLCamera.CaptureType type, ref ulong outHandle);

        /// <summary>
        /// Set the client-implemented callbacks to convey camera device status.
        /// Client needs to implement the callbacks defined by MLCameraDeviceStatusCallbacks.
        /// The library passes the camera device status to the client via those callbacks.
        /// Shall be called before MLCameraConnect().
        /// </summary>
        /// <param name="deviceStatusCallbacks">Capture status callbacks.</param>
        /// <param name="data">User metadata.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if callbacks were set successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraSetDeviceStatusCallbacks(IntPtr deviceStatusCallbacks, IntPtr data);

        /// <summary>
        /// Capture still image and save output to a file
        /// The output image will be stored in the format set by MLCameraSetOutputFormat and
        /// saved into the file specified by the file path.The library is responsible for
        /// opening and closing the file.
        /// If this function is invoked before the camera sensor has locked AE and AWB,
        /// it will be blocked till AE, AWB is locked and then starts to capture.
        /// </summary>
        /// <param name="path">File path to store the output image.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if image was captured successfully.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed to capture image due to on-going video recording.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to capture image due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to capture image due to an invalid parameter.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed to capture image due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern MLResult.Code MLCameraCaptureImage([MarshalAs(UnmanagedType.LPStr)]string path);

        /// <summary>
        /// Capture still image and get output data in buffer.
        /// The output is the raw image data with format set by MLCameraSetOutputFormat
        /// and passed to client via on_image_buffer_available.Client can also choose to
        /// implement polling mechanism and obtain the stream by MLCameraPollImageStream.
        /// If this function is invoked before the camera sensor has locked AE and AWB,
        /// it will be blocked till AE, AWB is locked and then starts to capture.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if captured raw image successfully.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericInvalidOperation</c> if failed to capture raw image due to on-going video recording.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to capture raw image due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed to capture raw image due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraCaptureImageRaw();

        /// <summary>
        /// Start video capture and save output to a file.
        /// <c>The captured video and audio streams will be encoded with AVC and AAC codecs
        /// and packed in mp4 container format and stored into the file specified by the
        /// file path.The library is responsible for opening and closing the file.The
        /// current supported video resolution is 1080p.
        /// If this function is invoked before the camera sensor has locked AE and AWB,
        /// it will be blocked till AE, AWB is locked and then starts to capture.
        /// MLCameraCaptureVideoStop() needs to be called to stop the capture.</c>
        /// </summary>
        /// <param name="path">File path to store the output video.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if started video recording successfully.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to start video recording due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to start video recording due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed to start video recording image due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern MLResult.Code MLCameraCaptureVideoStart([MarshalAs(UnmanagedType.LPStr)]string path);

        /// <summary>
        /// Start video capture and provide raw frames through callback
        /// The captured video YUV frames will be returned to the application via on_video_buffer_available.
        /// The current supported video resolution is 1080p.
        /// If this function is invoked before the camera sensor has locked AE and AWB,
        /// it will be blocked till AE, AWB is locked and then starts to capture.
        /// MLCameraCaptureVideoStop() needs to be called to stop the capture.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if started video recording successfully.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to start video recording due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to start video recording due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.Timeout</c> if failed to start video recording image due to timeout.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraCaptureRawVideoStart();

        /// <summary>
        /// Stop video capture.
        /// <c>User should allow some time, i.e., >500ms, after MLCameraCaptureVideoStart and before
        /// calling this API, as captured frames are being encoded.Otherwise, MLResult_UnspecifiedFailure
        /// will be returned.</c>
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if stopped video recording successfully.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to stop video recording due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraCaptureVideoStop();

        /// <summary>
        /// Poll camera device status.
        /// Use #MLCamera.DeviceStatusFlag to view specific status bit.
        /// Call MLCameraGetErrorCode() to obtain the error code if
        /// MLCamera.DeviceStatusFlag.Error bit is set.
        /// Note: This API can still be used even if MLCameraSetDeviceStatusCallbacks() has been called.
        /// </summary>
        /// <param name="outDeviceStatus">Device status.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained device status successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to obtain device status due to invalid input parameter.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraGetDeviceStatus(ref uint outDeviceStatus);

        /// <summary>
        /// Poll capture status.
        /// Use MLCamera.CaptureStatusFlag to view specific status bit.
        /// Call MLCameraGetErrorCode() to obtain the error code if
        /// MLCamera.CaptureStatusFlag.Error bit is set.
        /// Note: This API can still be used even if MLCameraSetCaptureStatusCallbacks() has been called.
        /// </summary>
        /// <param name="outCaptureStatus">Capture status.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained capture status successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to obtain capture status due to invalid input parameter.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraGetCaptureStatus(ref uint outCaptureStatus);

        /// <summary>
        /// Obtain device error code.
        /// </summary>
        /// <param name="outErrorCode">Device error code.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained device error code successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to obtain device error code due to invalid input parameter.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraGetErrorCode(ref MLCamera.ErrorType outErrorCode);

        /// <summary>
        /// Poll preview stream
        /// The preview stream and image stream are separate streams.The preview data is
        /// available upon MLCameraConnect(), and will always be available; whereas the image stream
        /// is produced when the user captures images.
        /// The library allocates the buffer and destroys it when disconnecting from the camera.
        /// Note: This API can still be used even if MLCameraSetDeviceStatusCallbacks() has been called.
        /// </summary>
        /// <param name="outPreview">Preview stream.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained preview stream successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to obtain preview stream due to invalid input parameter.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraGetPreviewStream(IntPtr outPreview);

        /// <summary>
        /// Poll raw image stream.
        /// The library allocates the buffer and destroys it when disconnecting from the camera.
        /// Note: This API can still be used even if MLCameraSetCaptureStatusCallbacks() has been called.
        /// </summary>
        /// <param name="outImage">Raw image stream.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained raw stream successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to obtain raw image stream due to invalid input parameter.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraGetImageStream(IntPtr outImage);

        /// <summary>
        /// Poll capture result extras data.
        /// The library allocates the buffer and destroys it when disconnecting from the camera.
        /// Note: This API can still be used even if MLCameraSetCaptureStatusCallbacks() has been called.
        /// </summary>
        /// <param name="outResultExtras">Capture result extras data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained result extras successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to obtain result extras due to invalid input parameter.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraGetCaptureResultExtras(IntPtr outResultExtras);

        /// <summary>
        /// Obtain handle for retrieving camera characteristics.
        /// This API provides the handle for retrieving camera characteristics via APIs
        /// defined in ml_camera_metadata.h.
        /// </summary>
        /// <param name="outHandle">Handle to access camera characteristic metadata.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained camera characteristic handle successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to obtain camera characteristic handle due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to capture raw image due to null pointer.
        /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraGetCameraCharacteristics(ref ulong outHandle);

        /// <summary>
        /// Obtains handle for retrieving capture result metadata
        /// This API provides the handle for retrieving capture result metadata via APIs
        /// defined in ml_camera_metadata.h.
        /// Note: that this handle is also available via callbacks if capture callbacks has been set.
        /// </summary>
        /// <param name="outHandle">Handle to access capture result metadata.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained capture result successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to obtain capture result handle due to invalid input parameter.
        /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to obtain capture result due to null pointer.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraGetResultMetadata(ref ulong outHandle);

        /// <summary>
        /// Set the output format of captured image file or image raw buffer.
        /// After changing output format, MLCameraPrepareCapture must be invoked to take
        /// effect or capture will fail.
        /// Without calling this function, default output format will be JPEG.
        /// </summary>
        /// <param name="format">Output format</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if command completed successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if command failed to complete successfully.
        /// </returns>
        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraSetOutputFormat(MLCamera.OutputFormat format);

        /// <summary>
        /// Returns the pointer to the plugin render callback.
        /// </summary>
        /// <returns>A pointer to the plugin render callback.</returns>
        [DllImport(MLCameraPluginDll)]
        public static extern System.IntPtr GetPluginRenderCallback();

        /// <summary>
        /// Returns the pointer to the plugin render cleanup callback.
        /// </summary>
        /// <returns>A pointer to the plugin render cleanup callback.</returns>
        [DllImport(MLCameraPluginDll)]
        public static extern System.IntPtr GetPluginRenderCleanupCallback();

        /// <summary>
        /// Sets the width and height of the native camera render texture.
        /// </summary>
        /// <param name="texture">A pointer to the native render texture.</param>
        /// <param name="w">The width of the texture.</param>
        /// <param name="h">The height of the texture.</param>
        [DllImport(MLCameraPluginDll)]
        public static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h);

        /// <summary>
        /// Get color correction aberration modes.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">Array of aberration modes.</param>
        /// <param name="outCount">Number of output data elements.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained color correction aberration modes successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetColorCorrectionAvailableAberrationModes(ulong handle, ref IntPtr outData, ref ulong outCount);

        /// <summary>
        /// Get AE modes.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">Array of AE modes.</param>
        /// <param name="outCount">Number of output data elements.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE modes successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAEAvailableModes(ulong handle, ref IntPtr outData, ref ulong outCount);

        /// <summary>
        /// Get AE compensation range.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">Array of min (1st) and max (2nd) of AE compensation.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE compensation range successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAECompensationRange(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] int[] outData);

        /// <summary>
        /// Get AE compensation step.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">AE compensation step.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE compensation step successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAECompensationStep(ulong handle, ref MLCameraMetadataRationalNative outData);

        /// <summary>
        /// Get AE lock.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">AE lock</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE lock successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAELockAvailable(ulong handle, ref MLCamera.MetadataControlAELock outData);

        /// <summary>
        /// Get AWB modes.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">Array of AWB modes.</param>
        /// <param name="outCount">Number of output data elements.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AWB modes successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAWBAvailableModes(ulong handle, ref IntPtr outData, ref ulong outCount);

        /// <summary>
        /// Get AWB lock.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">AWB lock.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AWB lock successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAWBLockAvailable(ulong handle, ref MLCamera.MetadataControlAWBLock outData);

        /// <summary>
        /// Get scaler processed sizes list
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">List of [width, height] pairs.</param>
        /// <param name="outCount">Number of output data elements(total of 2 x pairs).</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained scaler processed sizes list successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetScalerProcessedSizes(ulong handle, ref IntPtr outData, ref ulong outCount);

        /// <summary>
        /// Get scaler available max digital zoom.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">Max digital zoom.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained scaler available max digital zoom successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetScalerAvailableMaxDigitalZoom(ulong handle, ref float outData);

        /// <summary>
        /// Get scaler available stream configurations.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">List of stream configuration settings.</param>
        /// <param name="outCount"><c>Number of output data elements(total of 4 x settings).</c></param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained scaler available stream configuration successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetScalerAvailableStreamConfigurations(ulong handle, ref IntPtr outData, ref ulong outCount);

        /// <summary>
        /// Get sensor info active array sizes.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">Active array size [left, top, right, bottom].</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained sensor info active array sizes successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetSensorInfoActiveArraySize(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] int[] outData);

        /// <summary>
        /// Get sensor info sensitivity range.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">Sensor info sensitivity range[min, max].</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained sensor info sensitivity range successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetSensorInfoSensitivityRange(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] int[] outData);

        /// <summary>
        /// Get sensor info exposure time range.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">Sensor info exposure time range[min, max] in microseconds.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained sensor info exposure time range successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetSensorInfoExposureTimeRange(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] long[] outData);

        /// <summary>
        /// Get sensor orientation degree.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="outData">Sensor orientation degree.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained sensor orientation degree successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetSensorOrientation(ulong handle, ref int outData);

        /// <summary>
        /// Set sensor info exposure time range. Only max time will be set currently.
        /// </summary>
        /// <param name="handle">Camera characteristic metadata handle acquired from MLCameraGetCameraCharacteristics().</param>
        /// <param name="data">Sensor info exposure time range[min, max] in microseconds.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set sensor info exposure time range successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetSensorInfoExposureTimeRange(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] long[] data);

        /// <summary>
        /// Get color correction mode.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">Color correction mode.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained correction mode successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetColorCorrectionModeRequestMetadata(ulong handle, ref MLCamera.MetadataColorCorrectionMode outData);

        /// <summary>
        /// Get color correction transform.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">3x3 color correction transform.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained color correction transform successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetColorCorrectionTransformRequestMetadata(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 9)] MLCameraMetadataRationalNative[] outData);

        /// <summary>
        /// Get color correction gains.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">Color correction aberration.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained color correction aberration successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetColorCorrectionGainsRequestMetadata(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] float[] outData);

        /// <summary>
        /// Get color correction aberration.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">Color correction aberration.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained color correction aberration successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetColorCorrectionAberrationModeRequestMetadata(ulong handle, ref MLCamera.MetadataColorCorrectionAberrationMode outData);

        /// <summary>
        /// Get AE anti-banding mode.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">AE anti-banding mode.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE anti-banding mode successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAEAntibandingModeRequestMetadata(ulong handle, ref MLCamera.MetadataControlAEAntibandingMode outData);

        /// <summary>
        /// Get AE exposure compensation.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">Exposure compensation value.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE exposure compensation successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAEExposureCompensationRequestMetadata(ulong handle, ref int outData);

        /// <summary>
        /// Get AE lock.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">AE Lock.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE lock successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAELockRequestMetadata(ulong handle, ref MLCamera.MetadataControlAELock outData);

        /// <summary>
        /// Get AE mode.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">AE mode</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE mode successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAEModeRequestMetadata(ulong handle, ref MLCamera.MetadataControlAEMode outData);

        /// <summary>
        /// Get AE target FPS range.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">AE target FPS range.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE target FPS range successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAETargetFPSRangeRequestMetadata(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] int[] outData);

        /// <summary>
        /// Get AWB lock.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">AWB Lock.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AWB lock successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAWBLockRequestMetadata(ulong handle, ref MLCamera.MetadataControlAWBLock outData);

        /// <summary>
        /// Get AWB mode.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">AWB mode.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AWB mode successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAWBModeRequestMetadata(ulong handle, ref MLCamera.MetadataControlAWBMode outData);

        /// <summary>
        /// Get sensor exposure time.
        /// </summary>
        /// <param name="handle">handle Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">Sensor exposure time.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained sensor exposure time successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetSensorExposureTimeRequestMetadata(ulong handle, ref long outData);

        /// <summary>
        /// Get sensor sensitivity.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture.</param>
        /// <param name="outData">Sensor sensitivity.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained sensor info active array sizes successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetSensorSensitivityRequestMetadata(ulong handle, ref int outData);

        /// <summary>
        /// Get scaler crop region.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="outData">Cropped region [left, top, right, bottom].</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained sensor exposure time successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetScalerCropRegionRequestMetadata(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] int[] outData);

        /// <summary>
        /// Set color correction mode.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">Color correction mode, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set color correction mode successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetColorCorrectionMode(ulong handle, ref MLCamera.MetadataColorCorrectionMode data);

        /// <summary>
        /// Set color correction transform.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">data 3x3 color correction transform, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set color correction transform successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetColorCorrectionTransform(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 9)]MLCameraMetadataRationalNative[] data);

        /// <summary>
        /// Set color correction gains.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">Color correction gains, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set color correction gains successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetColorCorrectionGains(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] float[] data);

        /// <summary>
        /// Set color correction aberration.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">Color correction aberration, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set color correction aberration successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetColorCorrectionAberrationMode(ulong handle, ref MLCamera.MetadataColorCorrectionAberrationMode data);

        /// <summary>
        /// Set AE anti-banding mode.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">AE anti-banding mode, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained sensor info active array sizes successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetControlAEAntibandingMode(ulong handle, ref MLCamera.MetadataControlAEAntibandingMode data);

        /// <summary>
        /// Set AE exposure compensation.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">Exposure compensation value, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set AE exposure compensation successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetControlAEExposureCompensation(ulong handle, ref int data);

        /// <summary>
        /// Set AE lock.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">AE Lock, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set AE lock successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetControlAELock(ulong handle, ref MLCamera.MetadataControlAELock data);

        /// <summary>
        /// Set AE mode.
        /// </summary>
        /// <param name="handle">handle Camera request metadata handle acquired from MLCameraPrepareCapture()</param>
        /// <param name="data">AE Mode, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set AE mode successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetControlAEMode(ulong handle, ref MLCamera.MetadataControlAEMode data);

        /// <summary>
        /// Set AE target FPS range.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">AE target FPS range.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set AE target FPS range successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetControlAETargetFPSRange(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] int[] data);

        /// <summary>
        /// Set AWB lock.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">AWB Lock, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set AE target FPS range successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetControlAWBLock(ulong handle, ref MLCamera.MetadataControlAWBLock data);

        /// <summary>
        /// Set AWB mode.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">AWB mode, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set AWB mode successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetControlAWBMode(ulong handle, ref MLCamera.MetadataControlAWBMode data);

        /// <summary>
        /// Set sensor exposure time.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">Sensor exposure time, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set sensor exposure time successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetSensorExposureTime(ulong handle, ref long data);

        /// <summary>
        /// Set sensor sensitivity.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">Sensor sensitivity, a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set sensor sensitivity successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetSensorSensitivity(ulong handle, ref int data);

        /// <summary>
        /// Set scaler crop region.
        /// </summary>
        /// <param name="handle">Camera request metadata handle acquired from MLCameraPrepareCapture().</param>
        /// <param name="data">Cropped region [left, top, right, bottom], a null pointer will clear the data.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if set scaler crop region successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataSetScalerCropRegion(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] int[] data);

        /// <summary>
        /// Get color correction.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">Color correction mode.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained color correction successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetColorCorrectionModeResultMetadata(ulong handle, ref MLCamera.MetadataColorCorrectionMode outData);

        /// <summary>
        /// Get color correction transform.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">3x3 color correction transform matrix.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained color correction transform successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetColorCorrectionTransformResultMetadata(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 9)] MLCameraMetadataRationalNative[] outData);

        /// <summary>
        /// Get color correction aberration.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">Color correction aberration mode.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained color correction aberration successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetColorCorrectionAberrationModeResultMetadata(ulong handle, ref MLCamera.MetadataColorCorrectionAberrationMode outData);

        /// <summary>
        /// Get color correction gains.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">Color correction gains.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained color correction gains successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetColorCorrectionGainsResultMetadata(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)]float[] outData);

        /// <summary>
        ///  Get AE anti-banding mode.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">AE anti-banding mode.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE anti-banding mode successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAEAntibandingModeResultMetadata(ulong handle, ref MLCamera.MetadataControlAEAntibandingMode outData);

        /// <summary>
        /// Get AE exposure compensation.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">AE exposure compensation.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE exposure compensation successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAEExposureCompensationResultMetadata(ulong handle, ref int outData);

        /// <summary>
        /// Get AE lock.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">AE lock.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE lock successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAELockResultMetadata(ulong handle, ref MLCamera.MetadataControlAELock outData);

        /// <summary>
        /// Get AE mode.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">AE control mode.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE mode successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAEModeResultMetadata(ulong handle, ref MLCamera.MetadataControlAEMode outData);

        /// <summary>
        /// Get AE target FPS range.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">AE target FPS range.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE target FPS range successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAETargetFPSRangeResultMetadata(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)]int[] outData);

        /// <summary>
        /// Get AE state.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">AE state.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AE state successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAEStateResultMetadata(ulong handle, ref MLCamera.MetadataControlAEState outData);

        /// <summary>
        /// Get AWB lock.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">AWB Lock.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AWB lock successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAWBLockResultMetadata(ulong handle, ref MLCamera.MetadataControlAWBLock outData);

        /// <summary>
        /// Get AWB state.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">AWB state.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained AWB state successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetControlAWBStateResultMetadata(ulong handle, ref MLCamera.MetadataControlAWBState outData);

        /// <summary>
        /// Get sensor exposure time.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">Sensor exposure time.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained sensor exposure time successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetSensorExposureTimeResultMetadata(ulong handle, ref long outData);

        /// <summary>
        /// Get sensor sensitivity.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">Sensor sensitivity.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained sensor sensitivity successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetSensorSensitivityResultMetadata(ulong handle, ref int outData);

        /// <summary>
        /// Get frame captured timestamp.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">Timestamp in nanoseconds when captured.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained frame captured timestamp successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetSensorTimestampResultMetadata(ulong handle, ref long outData);

        /// <summary>
        /// Get scaler crop region.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">Cropped region [left, top, right, bottom].</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained scaler crop region successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetScalerCropRegionResultMetadata(ulong handle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)]int[] outData);

        /// <summary>
        /// Get sensor frame duration.
        /// </summary>
        /// <param name="handle">Camera result metadata handle acquired from MLCameraGetResultMetadata().</param>
        /// <param name="outData">Sensor frame duration.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained sensor frame duration successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
        /// </returns>
        [DllImport(MLCameraMetaDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCameraMetadataGetSensorFrameDurationResultMetadata(ulong handle, ref long outData);

        [DllImport(MLCameraDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MLResult.Code MLCameraSetCaptureCallbacksEx(IntPtr captureCallbacks, IntPtr data);

        /// <summary>
        /// Camera callbacks
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLCameraDeviceStatusCallbacksNative
        {
            /// <summary>
            /// Callback for OnDeviceAvailable.
            /// </summary>
            public OnDataCallback OnDeviceAvailable;

            /// <summary>
            /// Callback for OnDeviceUnavailable.
            /// </summary>
            public OnDataCallback OnDeviceUnavailable;

            /// <summary>
            /// Callback for OnDeviceOpened.
            /// </summary>
            public OnDataCallback OnDeviceOpened;

            /// <summary>
            /// Callback for OnDeviceClosed.
            /// </summary>
            public OnDataCallback OnDeviceClosed;

            /// <summary>
            /// Callback for OnDeviceDisconnected.
            /// </summary>
            public OnDataCallback OnDeviceDisconnected;

            /// <summary>
            /// Callback for OnDeviceError.
            /// </summary>
            public OnErrorDataCallback OnDeviceError;

            /// <summary>
            /// Callback for OnPreviewBufferAvailable.
            /// </summary>
            public OnHandleDataCallback OnPreviewBufferAvailable;
        }

        /// <summary>
        /// Camera metadata rational value.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLCameraMetadataRationalNative
        {
            /// <summary>
            /// The numerator value.
            /// </summary>
            public int Numerator;

            /// <summary>
            /// The denominator value.
            /// </summary>
            public int Denominator;

            /// <summary>
            /// Provides a string representation of the numerator and denominator.
            /// </summary>
            /// <returns>A string representation of the numerator and denominator.</returns>
            public override string ToString()
            {
                return this.Numerator + "/" + this.Denominator;
            }

            /// <summary>
            /// Returns the rational value as a decimal.
            /// </summary>
            /// <returns>The rational value.</returns>
            public float ToFloat()
            {
                if (this.Denominator == 0)
                {
                    MLPluginLog.Error("MLCameraMetadataRationalNative has a Denominator of 0. Cannot divide by zero!");
                    return 0.0f;
                }

                return (float)this.Numerator / (float)this.Denominator;
            }

            /// <summary>
            /// Sets the numerator and denominator based on the decimal rational and with the provided denominator.
            /// </summary>
            /// <param name="value">The decimal rational value.</param>
            /// <param name="denominator">The denominator used in the rational.</param>
            public void FromFloat(float value, int denominator)
            {
                this.Numerator = (int)(value * denominator);
                this.Denominator = denominator;
            }
        }

        /// <summary>
        /// Camera capture callback events.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MLCameraCaptureCallbacksExNative
        {
            /// <summary>
            /// The current version number of the structure.
            /// </summary>
            public uint Version;

            /// <summary>
            /// This event occurs when capture has started.
            /// </summary>
            public OnResultExtrasRefDataCallback OnCaptureStarted;

            /// <summary>
            /// This event occurs when capture has failed.
            /// </summary>
            public OnResultExtrasRefDataCallback OnCaptureFailed;

            /// <summary>
            /// This event occurs when the capture buffer is lost.
            /// </summary>
            public OnResultExtrasRefDataCallback OnCaptureBufferLost;

            /// <summary>
            /// This event occurs when there is capture progress.
            /// </summary>
            public OnHandleAndResultExtrasRefDataCallback OnCaptureProgressed;

            /// <summary>
            /// This event occurs when the capture has completed.
            /// </summary>
            public OnHandleAndResultExtrasRefDataCallback OnCaptureCompleted;

            /// <summary>
            /// This event occurs when the image buffer becomes available.
            /// </summary>
            public OnOutputRefDataCallback OnImageBufferAvailable;

            /// <summary>
            /// This event occurs when the video buffer becomes available.
            /// </summary>
            public OnOutputRefResultExtrasRefFrameMetadataRefDataCallback OnVideoBufferAvailable;

            /// <summary>
            /// Returns a new instance of the MLCameraCaptureCallbacksExNative structure.
            /// </summary>
            /// <returns>A new instance of the MLCameraCaptureCallbacksExNative structure.</returns>
            public static MLCameraCaptureCallbacksExNative Create()
            {
                return new MLCameraCaptureCallbacksExNative()
                {
                    Version = 1u,
                    OnCaptureStarted = null,
                    OnCaptureFailed = null,
                    OnCaptureBufferLost = null,
                    OnCaptureProgressed = null,
                    OnCaptureCompleted = null,
                    OnImageBufferAvailable = null,
                    OnVideoBufferAvailable = null
                };
            }
        }
    }
}
#endif
