// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLCameraResultSettings.cs" company="Magic Leap, Inc">
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
        /// Settings that are specific to a result of a capture
        /// </summary>
        public sealed partial class ResultSettings
        {
            /// <summary>
            /// Gets the color correction mode.
            /// </summary>
            public MLCamera.MetadataColorCorrectionMode ColorCorrectionMode { get; private set; }

            /// <summary>
            /// Gets the color correction transform.
            /// </summary>
            public MLCamera.ColorCorrectionTransform ColorCorrectionTransform { get; private set; }

            /// <summary>
            /// Gets the color correction aberration mode.
            /// </summary>
            public MLCamera.MetadataColorCorrectionAberrationMode ColorCorrectionAberrationMode { get; private set; }

            /// <summary>
            /// Gets the color correction gains.
            /// </summary>
            public MLCamera.ColorCorrectionGains ColorCorrectionGains { get; private set; }

            /// <summary>
            /// Gets the AE anti-banding mode.
            /// </summary>
            public MLCamera.MetadataControlAEAntibandingMode ControlAEAntibandingMode { get; private set; }

            /// <summary>
            /// Gets the AE exposure compensation.
            /// </summary>
            public int AEExposureCompensation { get; private set; }

            /// <summary>
            /// Gets the AE lock.
            /// </summary>
            public MLCamera.MetadataControlAELock ControlAELock { get; private set; }

            /// <summary>
            /// Gets the AE mode.
            /// </summary>
            public MLCamera.MetadataControlAEMode ControlAEMode { get; private set; }

            /// <summary>
            /// Gets the AE target FPS range.
            /// </summary>
            public MLCamera.ControlAETargetFPSRange ControlAETargetFPSRange { get; private set; }

            /// <summary>
            /// Gets the AE state.
            /// </summary>
            public MLCamera.MetadataControlAEState ControlAEState { get; private set; }

            /// <summary>
            /// Gets the AWB lock.
            /// </summary>
            public MLCamera.MetadataControlAWBLock ControlAWBLock { get; private set; }

            /// <summary>
            /// Gets the AWB state.
            /// </summary>
            public MLCamera.MetadataControlAWBState ControlAWBState { get; private set; }

            /// <summary>
            /// Gets the sensor exposure time.
            /// </summary>
            public long SensorExposureTime { get; private set; }

            /// <summary>
            /// Gets the sensor sensitivity.
            /// </summary>
            public int SensorSensitivity { get; private set; }

            /// <summary>
            /// Gets the sensor timestamp.
            /// </summary>
            public long SensorTimestamp { get; private set; }

            /// <summary>
            /// Gets the scaler crop region.
            /// </summary>
            public MLCamera.ScalerCropRegion ScalerCropRegion { get; private set; }

            /// <summary>
            /// Gets the sensor frame duration.
            /// </summary>
            public long SensorFrameDuration { get; private set; }

            /// <summary>
            /// Populate the camera result settings.
            /// </summary>
            /// <param name="prepareHandle">A pointer to the camera prepare handle.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if an invalid parameter was provided.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
            /// </returns>
            internal MLResult.Code PopulateSettings(ulong prepareHandle)
            {
                MLResult.Code resultCode = MLResult.Code.Ok;

                MLCamera.MetadataColorCorrectionMode colorCorrectionMode = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetColorCorrectionModeResultMetadata(prepareHandle, ref colorCorrectionMode);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get color correction mode for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.ColorCorrectionMode = colorCorrectionMode;

                MLCameraNativeBindings.MLCameraMetadataRationalNative[] colorCorrectionTransform = new MLCameraNativeBindings.MLCameraMetadataRationalNative[9];
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetColorCorrectionTransformResultMetadata(prepareHandle, colorCorrectionTransform);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get color correction transform matrix for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
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

                MLCamera.MetadataColorCorrectionAberrationMode colorCorrectionAberrationMode = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetColorCorrectionAberrationModeResultMetadata(prepareHandle, ref colorCorrectionAberrationMode);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get color correction aberration mode for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.ColorCorrectionAberrationMode = colorCorrectionAberrationMode;

                float[] colorCorrectionGains = new float[4];
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetColorCorrectionGainsResultMetadata(prepareHandle, colorCorrectionGains);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get color correction gains for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                // The order of this, red, greenEven, greenOdd, blue, was taken from the android developer site for the color corrections gain vector
                // https://developer.android.com/reference/android/hardware/camera2/params/RggbChannelVector
                this.ColorCorrectionGains = new MLCamera.ColorCorrectionGains(colorCorrectionGains[0], colorCorrectionGains[1], colorCorrectionGains[2], colorCorrectionGains[3]);

                MLCamera.MetadataControlAEAntibandingMode controlAEAntiBandingMode = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAEAntibandingModeResultMetadata(prepareHandle, ref controlAEAntiBandingMode);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get control AE antibanding mode for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.ControlAEAntibandingMode = controlAEAntiBandingMode;

                int controlAEExposureCompensation = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAEExposureCompensationResultMetadata(prepareHandle, ref controlAEExposureCompensation);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get control AE exposure compensation for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.AEExposureCompensation = controlAEExposureCompensation;

                MLCamera.MetadataControlAELock controlAELock = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAELockResultMetadata(prepareHandle, ref controlAELock);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get control AE lock for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.ControlAELock = controlAELock;

                MLCamera.MetadataControlAEMode controlAEMode = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAEModeResultMetadata(prepareHandle, ref controlAEMode);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get control ae mode for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.ControlAEMode = controlAEMode;

                int[] controlAETargetFPSRangeArray = new int[2];
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAETargetFPSRangeResultMetadata(prepareHandle, controlAETargetFPSRangeArray);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get control AE target fps for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.ControlAETargetFPSRange = new MLCamera.ControlAETargetFPSRange(controlAETargetFPSRangeArray[0], controlAETargetFPSRangeArray[1]);

                MLCamera.MetadataControlAEState controlAEState = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAEStateResultMetadata(prepareHandle, ref controlAEState);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get control AE state for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.ControlAEState = controlAEState;

                MLCamera.MetadataControlAWBLock controlAWBLock = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAWBLockResultMetadata(prepareHandle, ref controlAWBLock);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get control AWB lock for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.ControlAWBLock = controlAWBLock;

                MLCamera.MetadataControlAWBState controlAWBState = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAWBStateResultMetadata(prepareHandle, ref controlAWBState);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get control AWB state for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.ControlAWBState = controlAWBState;

                long sensorExposureTime = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetSensorExposureTimeResultMetadata(prepareHandle, ref sensorExposureTime);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get sensor exposure time for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.SensorExposureTime = sensorExposureTime;

                int sensorySensitivity = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetSensorSensitivityResultMetadata(prepareHandle, ref sensorySensitivity);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get sensor sensitivity for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.SensorSensitivity = sensorySensitivity;

                long sensorTimestamp = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetSensorTimestampResultMetadata(prepareHandle, ref sensorTimestamp);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get sensor time stamp for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.SensorTimestamp = sensorTimestamp;

                int[] scalerCropRegionArray = new int[4];
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetScalerCropRegionResultMetadata(prepareHandle, scalerCropRegionArray);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get scaler crop region for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.ScalerCropRegion = new MLCamera.ScalerCropRegion(scalerCropRegionArray[0], scalerCropRegionArray[1], scalerCropRegionArray[2], scalerCropRegionArray[3]);

                long sensorFrameDuration = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetSensorFrameDurationResultMetadata(prepareHandle, ref sensorFrameDuration);
                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLCamera.ResultSettings.PopulateSettings failed to get sensor frame duration for prepare handle {0}. Reason: {1}", prepareHandle, resultCode);
                    return resultCode;
                }

                this.SensorFrameDuration = sensorFrameDuration;

                return resultCode;
            }
        }
    }
}

#endif
