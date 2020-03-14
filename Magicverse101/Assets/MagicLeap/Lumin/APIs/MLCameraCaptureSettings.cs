// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLCameraCaptureSettings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap
{
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// MLCamera class exposes static functions to query camera related
    /// functions. Most functions are currently a direct pass through functions to the
    /// native C-API functions and incur no overhead.
    /// </summary>
    public sealed partial class MLCamera : MLAPISingleton<MLCamera>
    {
        /// <summary>
        /// Settings that are specific to a submitted capture
        /// </summary>
        public sealed partial class CaptureSettings
        {
            /// <summary>
            /// The handle to the prepared camera.
            /// </summary>
            private ulong prepareHandle = 0;

            /// <summary>
            /// Color correction mode.
            /// </summary>
            private MLCamera.MetadataColorCorrectionMode colorCorrectionMode;

            /// <summary>
            /// Color correction transform.
            /// </summary>
            private MLCamera.ColorCorrectionTransform colorCorrectionTransform;

            /// <summary>
            /// Color correction gains.
            /// </summary>
            private MLCamera.ColorCorrectionGains colorCorrectionGains;

            /// <summary>
            /// AE target FPS range.
            /// </summary>
            private MLCamera.ControlAETargetFPSRange controlAETargetFPSRange;

            /// <summary>
            /// Scaler crop region
            /// </summary>
            private MLCamera.ScalerCropRegion scalerCropRegion;

            /// <summary>
            /// Color correction aberration mode.
            /// </summary>
            private MLCamera.MetadataColorCorrectionAberrationMode colorCorrectionAberrationMode;

            /// <summary>
            /// AE anti-banding mode
            /// </summary>
            private MLCamera.MetadataControlAEAntibandingMode controlAEAntibandingMode;

            /// <summary>
            /// AE exposure compensation.
            /// </summary>
            private int controlAECompensation;

            /// <summary>
            /// AE lock.
            /// </summary>
            private MLCamera.MetadataControlAELock controlAELock;

            /// <summary>
            /// AE mode.
            /// </summary>
            private MLCamera.MetadataControlAEMode controlAEMode;

            /// <summary>
            /// AWB Lock
            /// </summary>
            private MLCamera.MetadataControlAWBLock controlAWBLock;

            /// <summary>
            /// AWB Mode.
            /// </summary>
            private MLCamera.MetadataControlAWBMode controlAWBMode;

            /// <summary>
            /// Sensor exposure time.
            /// </summary>
            private long sensorExposureTime;

            /// <summary>
            /// Sensor sensitivity.
            /// </summary>
            private int sensorSensitivity;

            /// <summary>
            /// Gets the camera capture type.
            /// </summary>
            public MLCamera.CaptureType CaptureType { get; private set; }

            /// <summary>
            /// Gets or sets the color correction mode.
            /// </summary>
            public MLCamera.MetadataColorCorrectionMode ColorCorrectionMode
            {
                get
                {
                    return this.colorCorrectionMode;
                }

                set
                {
                    this.IsDirty = true;
                    this.colorCorrectionMode = value;
                }
            }

            /// <summary>
            /// Gets or sets the color correction transform.
            /// </summary>
            public MLCamera.ColorCorrectionTransform ColorCorrectionTransform
            {
                get
                {
                    return this.colorCorrectionTransform;
                }

                set
                {
                    this.IsDirty = true;
                    this.colorCorrectionTransform = value;
                }
            }

            /// <summary>
            /// Gets or sets the color correction gains.
            /// </summary>
            public MLCamera.ColorCorrectionGains ColorCorrectionGains
            {
                get
                {
                    return this.colorCorrectionGains;
                }

                set
                {
                    this.IsDirty = true;
                    this.colorCorrectionGains = value;
                }
            }

            /// <summary>
            /// Gets or sets the AE target FPS range.
            /// </summary>
            public MLCamera.ControlAETargetFPSRange ControlAETargetFPSRange
            {
                get
                {
                    return this.controlAETargetFPSRange;
                }

                set
                {
                    this.IsDirty = true;
                    this.controlAETargetFPSRange = value;
                }
            }

            /// <summary>
            /// Gets or sets the scaler crop region.
            /// </summary>
            public MLCamera.ScalerCropRegion ScalerCropRegion
            {
                get
                {
                    return this.scalerCropRegion;
                }

                set
                {
                    this.IsDirty = true;
                    this.scalerCropRegion = value;
                }
            }

            /// <summary>
            /// Gets or sets the color correction aberration mode.
            /// </summary>
            public MLCamera.MetadataColorCorrectionAberrationMode ColorCorrectionAberrationMode
            {
                get
                {
                    return this.colorCorrectionAberrationMode;
                }

                set
                {
                    this.IsDirty = true;
                    this.colorCorrectionAberrationMode = value;
                }
            }

            /// <summary>
            /// Gets or sets the anti-banding mode.
            /// </summary>
            public MLCamera.MetadataControlAEAntibandingMode ControlAEAntibandingMode
            {
                get
                {
                    return this.controlAEAntibandingMode;
                }

                set
                {
                    this.IsDirty = true;
                    this.controlAEAntibandingMode = value;
                }
            }

            /// <summary>
            /// Gets or sets the AE exposure compensation.
            /// </summary>
            public int AEExposureCompensation
            {
                get
                {
                    return this.controlAECompensation;
                }

                set
                {
                    this.IsDirty = true;
                    this.controlAECompensation = value;
                }
            }

            /// <summary>
            /// Gets or sets the AE lock.
            /// </summary>
            public MLCamera.MetadataControlAELock ControlAELock
            {
                get
                {
                    return this.controlAELock;
                }

                set
                {
                    this.IsDirty = true;
                    this.controlAELock = value;
                }
            }

            /// <summary>
            /// Gets or sets the AE mode.
            /// </summary>
            public MLCamera.MetadataControlAEMode ControlAEMode
            {
                get
                {
                    return this.controlAEMode;
                }

                set
                {
                    this.IsDirty = true;
                    this.controlAEMode = value;
                }
            }

            /// <summary>
            /// Gets or sets the AWB lock.
            /// </summary>
            public MLCamera.MetadataControlAWBLock ControlAWBLock
            {
                get
                {
                    return this.controlAWBLock;
                }

                set
                {
                    this.IsDirty = true;
                    this.controlAWBLock = value;
                }
            }

            /// <summary>
            /// Gets or sets AWB Mode.
            /// </summary>
            public MLCamera.MetadataControlAWBMode ControlAWBMode
            {
                get
                {
                    return this.controlAWBMode;
                }

                set
                {
                    this.IsDirty = true;
                    this.controlAWBMode = value;
                }
            }

            /// <summary>
            /// Gets or sets the sensor exposure time.
            /// </summary>
            public long SensorExposureTime
            {
                get
                {
                    return this.sensorExposureTime;
                }

                set
                {
                    this.IsDirty = true;
                    this.sensorExposureTime = value;
                }
            }

            /// <summary>
            /// Gets or sets the sensor sensitivity.
            /// </summary>
            public int SensorSensitivity
            {
                get
                {
                    return this.sensorSensitivity;
                }

                set
                {
                    this.IsDirty = true;
                    this.sensorSensitivity = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether settings have changed and need to be updated.
            /// </summary>
            internal bool IsDirty { get; set; }

            /// <summary>
            /// Populate the settings.
            /// </summary>
            /// <param name="prepareHandle">A handle to the prepare capture camera.</param>
            /// <param name="captureType">The capture type</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the request completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
            /// </returns>
            internal MLResult.Code PopulateSettings(ulong prepareHandle, MLCamera.CaptureType captureType)
            {
                MLResult.Code result = MLResult.Code.Ok;
                this.prepareHandle = prepareHandle;
                this.CaptureType = captureType;

                result = MLCameraNativeBindings.MLCameraMetadataGetColorCorrectionModeRequestMetadata(this.prepareHandle, ref this.colorCorrectionMode);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get color correction mode. Reason: {0}", result);
                    return result;
                }

                MLCameraNativeBindings.MLCameraMetadataRationalNative[] colorCorrectionTransform = new MLCameraNativeBindings.MLCameraMetadataRationalNative[9];
                result = MLCameraNativeBindings.MLCameraMetadataGetColorCorrectionTransformRequestMetadata(this.prepareHandle, colorCorrectionTransform);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get color correction transform matrix. Reason: {0}", result);
                    return result;
                }

                this.ColorCorrectionTransform = new MLCamera.ColorCorrectionTransform(
                    colorCorrectionTransform[0].ToFloat(),
                    colorCorrectionTransform[1].ToFloat(),
                    colorCorrectionTransform[2].ToFloat(),
                    colorCorrectionTransform[3].ToFloat(),
                    colorCorrectionTransform[4].ToFloat(),
                    colorCorrectionTransform[5].ToFloat(),
                    colorCorrectionTransform[6].ToFloat(),
                    colorCorrectionTransform[7].ToFloat(),
                    colorCorrectionTransform[8].ToFloat());

                float[] colorCorrectionGains = new float[4];
                result = MLCameraNativeBindings.MLCameraMetadataGetColorCorrectionGainsRequestMetadata(this.prepareHandle, colorCorrectionGains);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get color correction gains vector. Reason: {0}", result);
                    return result;
                }

                // The order of this, red, greenEven, greenOdd, blue, was taken from the android developer site for the color corrections gain vector
                // https://developer.android.com/reference/android/hardware/camera2/params/RggbChannelVector
                this.ColorCorrectionGains = new MLCamera.ColorCorrectionGains(colorCorrectionGains[0], colorCorrectionGains[1], colorCorrectionGains[2], colorCorrectionGains[3]);
                result = MLCameraNativeBindings.MLCameraMetadataGetColorCorrectionAberrationModeRequestMetadata(this.prepareHandle, ref this.colorCorrectionAberrationMode);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get color correction aberration mode. Reason: {0}", result);
                    return result;
                }

                result = MLCameraNativeBindings.MLCameraMetadataGetControlAEAntibandingModeRequestMetadata(this.prepareHandle, ref this.controlAEAntibandingMode);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get control AE antibanding mode. Reason: {0}", result);
                    return result;
                }

                result = MLCameraNativeBindings.MLCameraMetadataGetControlAEExposureCompensationRequestMetadata(this.prepareHandle, ref this.controlAECompensation);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get AE exposure compensation. Reason: {0}", result);
                    return result;
                }

                result = MLCameraNativeBindings.MLCameraMetadataGetControlAELockRequestMetadata(this.prepareHandle, ref this.controlAELock);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get control AE lock. Reason: {0}", result);
                    return result;
                }

                result = MLCameraNativeBindings.MLCameraMetadataGetControlAEModeRequestMetadata(this.prepareHandle, ref this.controlAEMode);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get control AE mode. Reason: {0}", result);
                    return result;
                }

                int[] controlAETargetFPSRangeArray = new int[2];
                result = MLCameraNativeBindings.MLCameraMetadataGetControlAETargetFPSRangeRequestMetadata(this.prepareHandle, controlAETargetFPSRangeArray);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get control AE Target FPS Range. Reason: {0}", result);
                    return result;
                }

                this.ControlAETargetFPSRange = new MLCamera.ControlAETargetFPSRange(controlAETargetFPSRangeArray[0], controlAETargetFPSRangeArray[1]);

                result = MLCameraNativeBindings.MLCameraMetadataGetControlAWBLockRequestMetadata(this.prepareHandle, ref this.controlAWBLock);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get control AWB Lock. Reason: {0}", result);
                    return result;
                }

                result = MLCameraNativeBindings.MLCameraMetadataGetControlAWBModeRequestMetadata(this.prepareHandle, ref this.controlAWBMode);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get control AWB mode. Reason: {0}", result);
                    return result;
                }

                result = MLCameraNativeBindings.MLCameraMetadataGetSensorExposureTimeRequestMetadata(this.prepareHandle, ref this.sensorExposureTime);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get sensor exposure time. Reason: {0}", result);
                    return result;
                }

                result = MLCameraNativeBindings.MLCameraMetadataGetSensorSensitivityRequestMetadata(this.prepareHandle, ref this.sensorSensitivity);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get sensor sensitivity. Reason: {0}", result);
                    return result;
                }

                int[] scalerCropRegionArray = new int[4];
                result = MLCameraNativeBindings.MLCameraMetadataGetScalerCropRegionRequestMetadata(this.prepareHandle, scalerCropRegionArray);
                if (!MLResult.IsOK(result))
                {
                    MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.PopulateSettings failed to get scaler crop region. Reason: {0}", result);
                    return result;
                }

                this.ScalerCropRegion = new MLCamera.ScalerCropRegion(scalerCropRegionArray[0], scalerCropRegionArray[1], scalerCropRegionArray[2], scalerCropRegionArray[3]);

                return result;
            }

            /// <summary>
            /// Apply the settings.
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the request completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
            /// </returns>
            internal MLResult.Code ApplySettings()
            {
                MLResult.Code result = MLResult.Code.Ok;

                if (this.IsDirty)
                {
                    result = MLCameraNativeBindings.MLCameraMetadataSetColorCorrectionMode(this.prepareHandle, ref this.colorCorrectionMode);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set color correction mode. Reason: {0}", result);
                        return result;
                    }

                    result = MLCameraNativeBindings.MLCameraMetadataSetColorCorrectionAberrationMode(this.prepareHandle, ref this.colorCorrectionAberrationMode);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set color correction aberration mode. Reason: {0}", result);
                        return result;
                    }

                    result = MLCameraNativeBindings.MLCameraMetadataSetControlAEAntibandingMode(this.prepareHandle, ref this.controlAEAntibandingMode);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set control AE antibanding mode. Reason: {0}", result);
                        return result;
                    }

                    result = MLCameraNativeBindings.MLCameraMetadataSetControlAEExposureCompensation(this.prepareHandle, ref this.controlAECompensation);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set AE exposure compensation. Reason: {0}", result);
                        return result;
                    }

                    result = MLCameraNativeBindings.MLCameraMetadataSetControlAELock(this.prepareHandle, ref this.controlAELock);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set control AE lock. Reason: {0}", result);
                        return result;
                    }

                    result = MLCameraNativeBindings.MLCameraMetadataSetControlAEMode(this.prepareHandle, ref this.controlAEMode);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set control AE mode. Reason: {0}", result);
                        return result;
                    }

                    result = MLCameraNativeBindings.MLCameraMetadataSetControlAWBLock(this.prepareHandle, ref this.controlAWBLock);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set control AWB lock. Reason: {0}", result);
                        return result;
                    }

                    result = MLCameraNativeBindings.MLCameraMetadataSetControlAWBMode(this.prepareHandle, ref this.controlAWBMode);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set control AWB mode. Reason: {0}", result);
                        return result;
                    }

                    result = MLCameraNativeBindings.MLCameraMetadataSetSensorExposureTime(this.prepareHandle, ref this.sensorExposureTime);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set sensor exposure time. Reason: {0}", result);
                        return result;
                    }

                    result = MLCameraNativeBindings.MLCameraMetadataSetSensorSensitivity(this.prepareHandle, ref this.sensorSensitivity);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set sensor sensitivity. Reason: {0}", result);
                        return result;
                    }

                    this.IsDirty = false;
                }

                if (this.ColorCorrectionTransform.IsDirty)
                {
                    MLCameraNativeBindings.MLCameraMetadataRationalNative[] colorcorrectiontransform = new MLCameraNativeBindings.MLCameraMetadataRationalNative[9];
                    colorcorrectiontransform[0].FromFloat(this.ColorCorrectionTransform.X0, 1000);
                    colorcorrectiontransform[1].FromFloat(this.ColorCorrectionTransform.X1, 1000);
                    colorcorrectiontransform[2].FromFloat(this.ColorCorrectionTransform.X2, 1000);
                    colorcorrectiontransform[3].FromFloat(this.ColorCorrectionTransform.Y0, 1000);
                    colorcorrectiontransform[4].FromFloat(this.ColorCorrectionTransform.Y1, 1000);
                    colorcorrectiontransform[5].FromFloat(this.ColorCorrectionTransform.Y2, 1000);
                    colorcorrectiontransform[6].FromFloat(this.ColorCorrectionTransform.Z0, 1000);
                    colorcorrectiontransform[7].FromFloat(this.ColorCorrectionTransform.Z1, 1000);
                    colorcorrectiontransform[8].FromFloat(this.ColorCorrectionTransform.Z2, 1000);
                    result = MLCameraNativeBindings.MLCameraMetadataSetColorCorrectionTransform(this.prepareHandle, colorcorrectiontransform);

                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set color correction transform. Reason: {0}", result);
                        return result;
                    }

                    this.ColorCorrectionTransform.IsDirty = false;
                }

                if (this.ColorCorrectionGains.IsDirty)
                {
                    // The order of this, red, greenEven, greenOdd, blue, was taken from the android developer site for the color corrections gain vector
                    // https://developer.android.com/reference/android/hardware/camera2/params/RggbChannelVector
                    float[] gains = new float[4];
                    gains[0] = this.ColorCorrectionGains.Red;
                    gains[1] = this.ColorCorrectionGains.GreenEven;
                    gains[2] = this.ColorCorrectionGains.GreenOdd;
                    gains[3] = this.ColorCorrectionGains.Blue;
                    result = MLCameraNativeBindings.MLCameraMetadataSetColorCorrectionGains(this.prepareHandle, gains);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set color correction gains. Reason: {0}", result);
                        return result;
                    }

                    this.ColorCorrectionGains.IsDirty = false;
                }

                if (this.ControlAETargetFPSRange.IsDirty)
                {
                    int[] fpsrange = new int[2];
                    fpsrange[0] = this.ControlAETargetFPSRange.Minimum;
                    fpsrange[1] = this.ControlAETargetFPSRange.Maximum;
                    result = MLCameraNativeBindings.MLCameraMetadataSetControlAETargetFPSRange(this.prepareHandle, fpsrange);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set Control AE Target FPS range. Reason: {0}", result);
                        return result;
                    }

                    this.ControlAETargetFPSRange.IsDirty = false;
                }

                if (this.ScalerCropRegion.IsDirty)
                {
                    int[] cropRegion = new int[4];
                    cropRegion[0] = this.ScalerCropRegion.Left;
                    cropRegion[1] = this.ScalerCropRegion.Top;
                    cropRegion[2] = this.ScalerCropRegion.Right;
                    cropRegion[3] = this.ScalerCropRegion.Bottom;
                    result = MLCameraNativeBindings.MLCameraMetadataSetScalerCropRegion(this.prepareHandle, cropRegion);
                    if (!MLResult.IsOK(result))
                    {
                        MLPluginLog.ErrorFormat("MLCamera.CaptureSettings.ApplySettings failed to set scaler crop region. Reason: {0}", result);
                        return result;
                    }

                    this.ScalerCropRegion.IsDirty = false;
                }

                return result;
            }
        }
    }
}

#endif
