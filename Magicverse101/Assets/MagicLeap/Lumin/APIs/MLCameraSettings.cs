// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLCameraSettings.cs" company="Magic Leap, Inc">
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
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// MLCamera class exposes static functions to query camera related
    /// functions. Most functions are currently a direct pass through functions to the
    /// native C-API functions and incur no overhead.
    /// </summary>
    public sealed partial class MLCamera : MLAPISingleton<MLCamera>
    {
        /// <summary>
        /// Camera characteristics and settings
        /// </summary>
        public sealed class GeneralSettings
        {
            /// <summary>
            /// Gets a list containing the AE mode.
            /// </summary>
            public List<MLCamera.MetadataControlAEMode> ControlAEModesAvailable { get; private set; }

            /// <summary>
            /// Gets a list containing the color correction aberration mode.
            /// </summary>
            public List<MLCamera.MetadataColorCorrectionAberrationMode> ColorCorrectionAberrationModesAvailable { get; private set; }

            /// <summary>
            /// Gets a list containing the AWB modes.
            /// </summary>
            public List<MLCamera.MetadataControlAWBMode> ControlAWBModesAvailable { get; private set; }

            /// <summary>
            /// Gets a list containing the scaler processed sizes.
            /// </summary>
            public List<ScalerProcessedSize> ScalerProcessedSizes { get; private set; }

            /// <summary>
            /// Gets a list containing the scaler available stream configurations
            /// </summary>
            public List<StreamConfiguration> ScalerAvailableStreamConfigurations { get; private set; }

            /// <summary>
            /// Gets the AE compensation range.
            /// </summary>
            public AECompensationRangeValues AECompensationRange { get; private set; }

            /// <summary>
            /// Gets the sensor info active array size.
            /// </summary>
            public SensorInfoActiveArraySizeValues SensorInfoActiveArraySize { get; private set; }

            /// <summary>
            /// Gets the sensor info sensitivity range.
            /// </summary>
            public SensorInfoSensitivityRangeValues SensorInfoSensitivityRange { get; private set; }

            /// <summary>
            /// Gets the sensor info exposure time range.
            /// </summary>
            public SensorInfoExposureTimeRangeValues SensorInfoExposureTimeRange { get; private set; }

           /// <summary>
           /// Gets the AE compensation step numerator.
           /// </summary>
            public int AECompensationStepNumerator { get; private set; }

            /// <summary>
            /// Gets the AE compensation step denominator.
            /// </summary>
            public int AECompensationStepDenominator { get; private set; }

            /// <summary>
            /// Gets the AE compensation step.
            /// </summary>
            public float AECompensationStep { get; private set; }

            /// <summary>
            /// Gets the max digital zoom.
            /// </summary>
            public float AvailableMaxDigitalZoom { get; private set; }

            /// <summary>
            /// Gets the AE lock.
            /// </summary>
            public MLCamera.MetadataControlAELock ControlAELockAvailable { get; private set; }

            /// <summary>
            /// Gets the AWB lock.
            /// </summary>
            public MLCamera.MetadataControlAWBLock ControlAWBLockAvailable { get; private set; }

            /// <summary>
            /// Gets the sensor orientation.
            /// </summary>
            public int SensorOrientation { get; private set; }

            /// <summary>
            /// Apply any changes made to the MLCamera.GeneralSettings properties.
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to connect to camera characteristic due to null pointer.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
            /// </returns>
            public MLResult Apply()
            {
                MLResult.Code resultCode;

                if (this.SensorInfoExposureTimeRange.IsDirty)
                {
                    ulong cameraCharacteristicsHandle = MagicLeapNativeBindings.InvalidHandle;
                    resultCode = MLCameraNativeBindings.MLCameraGetCameraCharacteristics(ref cameraCharacteristicsHandle);

                    if (!MLResult.IsOK(resultCode))
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.Apply failed to get camera characteristics for MLCamera while applying. Reason: {0}", result);
                        return result;
                    }

                    long[] sensorExposureTimeRange = new long[2];
                    sensorExposureTimeRange[0] = this.SensorInfoExposureTimeRange.Minimum;
                    sensorExposureTimeRange[1] = this.SensorInfoExposureTimeRange.Maximum;

                    resultCode = MLCameraNativeBindings.MLCameraMetadataSetSensorInfoExposureTimeRange(cameraCharacteristicsHandle, sensorExposureTimeRange);

                    if (!MLResult.IsOK(resultCode))
                    {
                        MLResult result = MLResult.Create(resultCode);
                        MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.Apply failed to set sensor exposure time range. Reason: {0}", result);
                        return result;
                    }
                }

                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Update the MLCamera characteristics.
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if obtained camera characteristic handle successfully.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to obtain camera characteristic handle due to invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.MediaGenericUnexpectedNull</c> if failed to capture raw image due to null pointer.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if a required permission is missing.
            /// </returns>
            internal MLResult PopulateCharacteristics()
            {
                MLResult.Code resultCode;

                ulong cameraCharacteristicsHandle = MagicLeapNativeBindings.InvalidHandle;
                resultCode = MLCameraNativeBindings.MLCameraGetCameraCharacteristics(ref cameraCharacteristicsHandle);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get camera characteristics for MLCamera. Reason: {0}", result);
                    return result;
                }

                ulong controlAEModeCount = 0;
                IntPtr controlAEAvailableModesData = IntPtr.Zero;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAEAvailableModes(cameraCharacteristicsHandle, ref controlAEAvailableModesData, ref controlAEModeCount);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get camera control AE available modes for MLCamera. Reason: {0}", result);
                    return result;
                }

                this.ControlAEModesAvailable = new List<MLCamera.MetadataControlAEMode>();
                int[] controlAEModeArray = new int[controlAEModeCount];
                Marshal.Copy(controlAEAvailableModesData, controlAEModeArray, 0, (int)controlAEModeCount);
                for (int i = 0; i < controlAEModeArray.Length; ++i)
                {
                    this.ControlAEModesAvailable.Add((MLCamera.MetadataControlAEMode)controlAEModeArray[i]);
                }

                ulong colorCorrectionAberrationModeCount = 0;
                IntPtr colorCorrectionAberrationModesData = IntPtr.Zero;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetColorCorrectionAvailableAberrationModes(cameraCharacteristicsHandle, ref colorCorrectionAberrationModesData, ref colorCorrectionAberrationModeCount);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get color correction aberration modes available for MLCamera. Reason: {0}", result);
                    return result;
                }

                this.ColorCorrectionAberrationModesAvailable = new List<MLCamera.MetadataColorCorrectionAberrationMode>((int)colorCorrectionAberrationModeCount);
                int[] aberrationModeArray = new int[colorCorrectionAberrationModeCount];
                Marshal.Copy(colorCorrectionAberrationModesData, aberrationModeArray, 0, (int)colorCorrectionAberrationModeCount);
                for (int i = 0; i < aberrationModeArray.Length; ++i)
                {
                    this.ColorCorrectionAberrationModesAvailable.Add((MLCamera.MetadataColorCorrectionAberrationMode)aberrationModeArray[i]);
                }

                int[] compensationRange = new int[2];
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAECompensationRange(cameraCharacteristicsHandle, compensationRange);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get control AE compensation range. Reason: {0}", result);
                    return result;
                }

                this.AECompensationRange = new AECompensationRangeValues(compensationRange[0], compensationRange[1]);

                MLCameraNativeBindings.MLCameraMetadataRationalNative rational = new MLCameraNativeBindings.MLCameraMetadataRationalNative();
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAECompensationStep(cameraCharacteristicsHandle, ref rational);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get control AE compensation step. Reason: {0}", result);
                    return result;
                }

                this.AECompensationStepNumerator = rational.Numerator;
                this.AECompensationStepDenominator = rational.Denominator;

                this.AECompensationStep = (float)this.AECompensationStepNumerator / (float)this.AECompensationStepDenominator;

                float availableMaxDigitalZoom = 0.0f;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetScalerAvailableMaxDigitalZoom(cameraCharacteristicsHandle, ref availableMaxDigitalZoom);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get max available digital zoom. Reason: {0}", result);
                    return result;
                }

                this.AvailableMaxDigitalZoom = availableMaxDigitalZoom;

                int sensorOrientation = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetSensorOrientation(cameraCharacteristicsHandle, ref sensorOrientation);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get sensor orientation. Reason: {0}", result);
                    return result;
                }

                this.SensorOrientation = sensorOrientation;

                MLCamera.MetadataControlAELock controlAELockAvailable = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAELockAvailable(cameraCharacteristicsHandle, ref controlAELockAvailable);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get control AE lock available. Reason: {0}", result);
                    return result;
                }

                this.ControlAELockAvailable = controlAELockAvailable;

                ulong controlAWBModeCount = 0;
                IntPtr controlAWBAvailableModesData = IntPtr.Zero;

                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAWBAvailableModes(cameraCharacteristicsHandle, ref controlAWBAvailableModesData, ref controlAWBModeCount);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get camera control ABW available modes for MLCamera. Reason: {0}", result);
                    return result;
                }

                this.ControlAWBModesAvailable = new List<MLCamera.MetadataControlAWBMode>();
                int[] awbModeArray = new int[controlAWBModeCount];
                Marshal.Copy(controlAWBAvailableModesData, awbModeArray, 0, (int)controlAWBModeCount);
                for (int i = 0; i < awbModeArray.Length; ++i)
                {
                    this.ControlAWBModesAvailable.Add((MLCamera.MetadataControlAWBMode)awbModeArray[i]);
                }

                MLCamera.MetadataControlAWBLock controlAWBLockAvailable = 0;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetControlAWBLockAvailable(cameraCharacteristicsHandle, ref controlAWBLockAvailable);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get control AWB lock available. Reason: {0}", result);
                    return result;
                }

                this.ControlAWBLockAvailable = controlAWBLockAvailable;

                int[] sensorInfoActiveArraySize = new int[4];

                resultCode = MLCameraNativeBindings.MLCameraMetadataGetSensorInfoActiveArraySize(cameraCharacteristicsHandle, sensorInfoActiveArraySize);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get sensor info active array size. Reason: {0}", result);
                    return result;
                }

                this.SensorInfoActiveArraySize = new SensorInfoActiveArraySizeValues(
                    sensorInfoActiveArraySize[0],
                    sensorInfoActiveArraySize[1],
                    sensorInfoActiveArraySize[2],
                    sensorInfoActiveArraySize[3]);

                ulong scalerProcessedSizesCount = 0;
                IntPtr scalerProcessedSizesData = IntPtr.Zero;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetScalerProcessedSizes(cameraCharacteristicsHandle, ref scalerProcessedSizesData, ref scalerProcessedSizesCount);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get scaler processed sizes. Reason: {0}", result);
                    return result;
                }

                int[] scalerProcessedSizesDataArray = new int[scalerProcessedSizesCount];
                Marshal.Copy(scalerProcessedSizesData, scalerProcessedSizesDataArray, 0, (int)scalerProcessedSizesCount);
                this.ScalerProcessedSizes = new List<ScalerProcessedSize>((int)scalerProcessedSizesCount);
                for (int i = 0; i < (int)scalerProcessedSizesCount; i += 2)
                {
                    int width = scalerProcessedSizesDataArray[i];
                    int height = scalerProcessedSizesDataArray[i + 1];
                    ScalerProcessedSize newSize = new ScalerProcessedSize(width, height);
                    this.ScalerProcessedSizes.Add(newSize);
                }

                ulong streamConfigurationsCount = 0;
                IntPtr streamConfigurationsData = IntPtr.Zero;
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetScalerAvailableStreamConfigurations(cameraCharacteristicsHandle, ref streamConfigurationsData, ref streamConfigurationsCount);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get scaler available stream configurations. Reason: {0}", result);
                    return result;
                }

                int[] streamConfigurationsDataArray = new int[streamConfigurationsCount];
                Marshal.Copy(streamConfigurationsData, streamConfigurationsDataArray, 0, (int)streamConfigurationsCount);

                this.ScalerAvailableStreamConfigurations = new List<StreamConfiguration>();
                for (int i = 0; i < (int)streamConfigurationsCount; i += 4)
                {
                    StreamConfiguration config = new StreamConfiguration(
                        (MLCamera.MetadataScalerAvailableFormats)streamConfigurationsDataArray[i],
                        streamConfigurationsDataArray[i + 1],
                        streamConfigurationsDataArray[i + 2],
                        (MLCamera.MetadataScalerAvailableStreamConfigurations)streamConfigurationsDataArray[i + 3]);

                    this.ScalerAvailableStreamConfigurations.Add(config);
                }

                int[] sensorInfoSensitivityRange = new int[2];
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetSensorInfoSensitivityRange(cameraCharacteristicsHandle, sensorInfoSensitivityRange);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get sensor info sensitivity range. Reason: {0}", result);
                    return result;
                }

                this.SensorInfoSensitivityRange = new SensorInfoSensitivityRangeValues(sensorInfoSensitivityRange[0], sensorInfoSensitivityRange[1]);

                long[] sensorInfoExposureTimeRange = new long[2];
                resultCode = MLCameraNativeBindings.MLCameraMetadataGetSensorInfoExposureTimeRange(cameraCharacteristicsHandle, sensorInfoExposureTimeRange);

                if (!MLResult.IsOK(resultCode))
                {
                    MLResult result = MLResult.Create(resultCode);
                    MLPluginLog.ErrorFormat("MLCamera.GeneralSettings.PopulateCharacteristics failed to get sensor info exposure time range. Reason: {0}", result);
                    return result;
                }

                this.SensorInfoExposureTimeRange = new SensorInfoExposureTimeRangeValues(sensorInfoExposureTimeRange[0], sensorInfoExposureTimeRange[1]);

                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// AE compensation range values.
            /// </summary>
            public struct AECompensationRangeValues
            {
                /// <summary>
                /// Minimum compensation range.
                /// </summary>
                public readonly int Minimum;

                /// <summary>
                /// Maximum compensation range.
                /// </summary>
                public readonly int Maximum;

                /// <summary>
                /// Initializes a new instance of the <see cref="AECompensationRangeValues"/> struct.
                /// </summary>
                /// <param name="minimum">The minimum value.</param>
                /// <param name="maximum">The maximum value.</param>
                internal AECompensationRangeValues(int minimum, int maximum)
                {
                    this.Minimum = minimum;
                    this.Maximum = maximum;
                }
            }

            /// <summary>
            /// A structure containing the sensor info active array size/
            /// </summary>
            public struct SensorInfoActiveArraySizeValues
            {
                /// <summary>
                /// The left position of the area.
                /// </summary>
                public readonly int Left;

                /// <summary>
                /// The top position of the area.
                /// </summary>
                public readonly int Top;

                /// <summary>
                /// The right position of the area.
                /// </summary>
                public readonly int Right;

                /// <summary>
                /// The bottom position of the area.
                /// </summary>
                public readonly int Bottom;

                /// <summary>
                /// Initializes a new instance of the <see cref="SensorInfoActiveArraySizeValues"/> struct.
                /// </summary>
                /// <param name="left">The left position of the area.</param>
                /// <param name="top">The top position of the area.</param>
                /// <param name="right">The right position of the area.</param>
                /// <param name="bottom">The bottom position of the area.</param>
                internal SensorInfoActiveArraySizeValues(int left, int top, int right, int bottom)
                {
                    this.Left = left;
                    this.Top = top;
                    this.Right = right;
                    this.Bottom = bottom;
                }
            }

            /// <summary>
            /// A structure containing the sensor info sensitivity range.
            /// </summary>
            public struct SensorInfoSensitivityRangeValues
            {
                /// <summary>
                /// The minimum range value.
                /// </summary>
                public readonly int Minimum;

                /// <summary>
                /// The maximum range value.
                /// </summary>
                public readonly int Maximum;

                /// <summary>
                /// Initializes a new instance of the <see cref="SensorInfoSensitivityRangeValues"/> struct
                /// </summary>
                /// <param name="minimum">The minimum value for the range.</param>
                /// <param name="maximum">The maximum value for the range.</param>
                internal SensorInfoSensitivityRangeValues(int minimum, int maximum)
                {
                    this.Minimum = minimum;
                    this.Maximum = maximum;
                }
            }

            /// <summary>
            /// A structure containing the scaler processed size.
            /// </summary>
            public struct ScalerProcessedSize
            {
                /// <summary>
                /// The width of the scaler.
                /// </summary>
                public readonly int Width;

                /// <summary>
                /// The height of the scaler.
                /// </summary>
                public readonly int Height;

                /// <summary>
                /// Initializes a new instance of the <see cref="ScalerProcessedSize"/> struct.
                /// </summary>
                /// <param name="width">The width of the scaler.</param>
                /// <param name="height">The height of the scaler.</param>
                internal ScalerProcessedSize(int width, int height)
                {
                    this.Width = width;
                    this.Height = height;
                }
            }

            /// <summary>
            /// A structure containing the stream configuration.
            /// </summary>
            public struct StreamConfiguration
            {
                /// <summary>
                /// Available scaler formats.
                /// </summary>
                public readonly MLCamera.MetadataScalerAvailableFormats AvailableFormats;

                /// <summary>
                /// Available stream configurations.
                /// </summary>
                public readonly MLCamera.MetadataScalerAvailableStreamConfigurations AvailableStreamConfigurations;

                /// <summary>
                /// The width of the capture.
                /// </summary>
                public readonly int Width;

                /// <summary>
                /// The height of the capture.
                /// </summary>
                public readonly int Height;

                /// <summary>
                /// Initializes a new instance of the <see cref="StreamConfiguration"/> struct.
                /// </summary>
                /// <param name="availableFormats">Available scaler formats.</param>
                /// <param name="width">The width of the capture.</param>
                /// <param name="height">The height of the capture.</param>
                /// <param name="availableStreamConfigurations">Available stream configurations.</param>
                internal StreamConfiguration(MLCamera.MetadataScalerAvailableFormats availableFormats, int width, int height, MLCamera.MetadataScalerAvailableStreamConfigurations availableStreamConfigurations)
                {
                    this.AvailableFormats = availableFormats;
                    this.Width = width;
                    this.Height = height;
                    this.AvailableStreamConfigurations = availableStreamConfigurations;
                }
            }

            /// <summary>
            /// A class containing information about camera exposure time.
            /// </summary>
            public class SensorInfoExposureTimeRangeValues
            {
                /// <summary>
                /// The minimum exposure time.
                /// </summary>
                public readonly long Minimum;

                /// <summary>
                /// The maximum exposure time.
                /// </summary>
                private long maximum;

                /// <summary>
                /// Initializes a new instance of the <see cref="SensorInfoExposureTimeRangeValues"/> class.
                /// </summary>
                /// <param name="minimum">The minimum camera exposure time.</param>
                /// <param name="maximum">The maximum camera exposure time.</param>
                internal SensorInfoExposureTimeRangeValues(long minimum, long maximum)
                {
                    this.Minimum = minimum;
                    this.Maximum = maximum;
                    this.IsDirty = false;
                }

                /// <summary>
                /// Gets or sets the Maximum exposure time range can be modified.
                /// </summary>
                public long Maximum
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

                /// <summary>
                /// Gets a value indicating whether settings have changed and need to be updated.
                /// </summary>
                public bool IsDirty { get; private set; }
            }
        }
    }
}

#endif
