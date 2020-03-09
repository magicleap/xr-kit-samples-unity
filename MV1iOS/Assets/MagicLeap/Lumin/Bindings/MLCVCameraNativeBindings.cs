// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLCVCameraNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    /// See ml_cv_camera.h for additional comments.
    /// </summary>
    public partial class MLCVCameraNativeBindings : MagicLeapNativeBindings
    {
        /// <summary>
        /// <c>MLCVCameraIntrinsics_MaxDistortionCoefficients</c> from ml_cv_camera.h
        /// </summary>
        public const uint MaxDistortionCoefficients = 5;

        /// <summary>
        /// Prevents a default instance of the <see cref="MLCVCameraNativeBindings" /> class from being created.
        /// </summary>
        private MLCVCameraNativeBindings()
        {
        }

        /// <summary>
        /// MLCVCameraID enum from ml_cv_camera.h
        /// </summary>
        public enum CameraID : uint
        {
            /// <summary>
            /// RGB Camera.
            /// </summary>
            ColorCamera = 0,
        }

        /// <summary>
        /// Create Camera Tracker.
        /// </summary>
        /// <param name="cvCameraHandle">tracker Handle.</param>
        /// <returns>MLResult_Ok On success.
        /// MLResult_PrivilegeDenied Necessary privilege is missing.
        /// MLResult_UnspecifiedFailure Unable to create tracker.</returns>
        [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCVCameraTrackingCreate(ref ulong cvCameraHandle);

        /// <summary>
        /// Destroy Tracker after usage.
        /// </summary>
        /// <param name="cvCameraHandle">MLHandle previously created with MLCVCameraTrackingCreate.</param>
        /// <returns>
        /// MLResult_Ok On success.
        /// MLResult_PrivilegeDenied Necessary privilege is missing.
        /// MLResult_UnspecifiedFailure Unable to create tracker.
        /// </returns>
        [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCVCameraTrackingDestroy(ulong cvCameraHandle);

        /// <summary>
        /// Get the intrinsic calibration parameters.
        /// </summary>
        /// <param name="cvCameraHandle">MLHandle previously created with MLCVCameraTrackingCreate.</param>
        /// <param name="id">The id of the camera.</param>
        /// <param name="outIntrinsics">The intrinsic calibration parameters.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained result extras successfully.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to obtain result extras due to invalid input parameter.
        /// </returns>
        [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCVCameraGetIntrinsicCalibrationParameters(ulong cvCameraHandle, CameraID id, ref IntrinsicCalibrationParametersNative outIntrinsics);

        /// <summary>
        /// Get the camera pose in the world coordinate system.
        /// </summary>
        /// <param name="cvCameraHandle">MLHandle previously created with MLCVCameraTrackingCreate.</param>
        /// <param name="headHandle">MLHandle previously created with MLHeadCameraCreate.</param>
        /// <param name="id">The camera id.</param>
        /// <param name="vcamTimestampUs">The timestamp of the frame pose.</param>
        /// <param name="outTransform">The transform of the frame pose.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// </returns>
        /// <returns></returns>
        [DllImport(MLPerceptionClientDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLCVCameraGetFramePose(ulong cvCameraHandle, ulong headHandle, CameraID id, ulong vcamTimestampUs, ref MLTransform outTransform);

        /// <summary>
        /// Links to MLCVCameraIntrinsicCalibrationParameters in ml_cv_camera.h.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct IntrinsicCalibrationParametersNative
        {
            /// <summary>
            /// Structure version.
            /// </summary>
            public uint Version;

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
            public MLVec2f FocalLength;

            /// <summary>
            /// Camera principle point.
            /// </summary>
            public MLVec2f PrincipalPoint;

            /// <summary>
            /// Field of view.
            /// </summary>
            public float FOV;

            /// <summary>
            /// Distortion vector.
            /// The distortion coefficients are in the following order:
            /// [k1, k2, p1, p2, k3]
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MaxDistortionCoefficients)]
            public double[] Distortion;

            /// <summary>
            /// Create and return an initialized version of this struct.
            /// </summary>
            /// <returns>
            /// Returns a struct containing camera intrinsic parameters.
            /// </returns>
            public static IntrinsicCalibrationParametersNative Create()
            {
                return new IntrinsicCalibrationParametersNative()
                {
                    Version = 1u,
                    Width = 0u,
                    Height = 0u,
                    FocalLength = new MLVec2f() { X = 0.0f, Y = 0.0f },
                    PrincipalPoint = new MLVec2f() { X = 0.0f, Y = 0.0f },
                    FOV = 0.0f,
                    Distortion = Enumerable.Repeat<double>(0.0d, (int)MaxDistortionCoefficients).ToArray()
                };
            }
        }
    }
}

#endif
