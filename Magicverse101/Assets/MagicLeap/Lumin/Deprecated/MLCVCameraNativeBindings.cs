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

using System;
using System.Linq;
using System.Runtime.InteropServices;
using static UnityEngine.XR.MagicLeap.Native.MagicLeapNativeBindings;

namespace UnityEngine.XR.MagicLeap
{


    /// <summary>
    /// MLCVCameraID enum from ml_cv_camera.h
    /// </summary>
    [Obsolete("Please use MLCVCameraNativeBindings.CameraID instead.", false)]
    public enum MLCVCameraID : uint
    {
        /// <summary>
        /// RGB Camera.
        /// </summary>
        ColorCamera = 0,
    }

    /// <summary>
    /// Contains camera intrinsic parameters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [Obsolete("Please use MLCVCameraNativeBindings.IntrinsicCalibrationParameters instead.", false)]
    public struct MLCVCameraIntrinsicCalibrationParameters
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

    namespace Native
    {
        /// <summary>
        /// See ml_cv_camera.h for additional comments.
        /// </summary>
        public partial class MLCVCameraNativeBindings : MagicLeapNativeBindings
        {

        }
    }
}
#endif
