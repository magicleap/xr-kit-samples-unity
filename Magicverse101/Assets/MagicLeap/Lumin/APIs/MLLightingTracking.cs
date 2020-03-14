// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLLightingTracking.cs" company="Magic Leap, Inc">
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
    using UnityEngine;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Provides environment lighting information.
    /// Capturing images or video will stop the lighting information update.
    /// </summary>
    public class MLLightingTracking : MLAPISingleton<MLLightingTracking>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// Time when captured in nanoseconds since the Epoch.
        /// </summary>
        private static readonly DateTime EPOCHSTART = new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

        /// <summary>
        /// Copy of camera's luminance in nits
        /// </summary>
        private static ushort[] tempCameraLuminanceCopy = new ushort[MLLightingTrackingNativeBindings.CameraCount];

        /// <summary>
        /// Handle to the native lighting tracking.
        /// </summary>
        private ulong nativeTracker = MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// Ambient light's intensity state.
        /// </summary>
        private MLLightingTrackingNativeBindings.AmbientGlobalStateNative intensityState;

        /// <summary>
        /// Color temperature state.
        /// </summary>
        private MLLightingTrackingNativeBindings.ColorTemperatureStateNative temperatureState;

        /// <summary>
        /// Stores the color value.
        /// </summary>
        private Color globalTemperatureColor;

        /// <summary>
        /// Average luminance in nits of all sensors
        /// </summary>
        private ushort averageLuminance;

        /// <summary>
        /// Indicates if MLLightTracker() fails to get ambient global state.
        /// </summary>
        private bool getAmbientGlobalStateFailed = false;

        /// <summary>
        /// Indicates if MLLightTracker() fails to get color temperature state.
        /// </summary>
        private bool getColorTemperatureStateFailed = false;

        /// <summary>
        /// Accumulated R, G and B values over a 250 milliseconds time period by the Ambient Light Sensor.
        /// </summary>
        private Vector3 rawPixelAccumulation;

        /// <summary>
        /// The CIE Tri-Stimulus values in Q16 format
        /// </summary>
        private Vector3 tristimulusValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLLightingTracking" /> class from being created.
        /// Constructor used to initialize MLLightingTracking data members.
        /// </summary>
        public MLLightingTracking()
        {
            this.nativeTracker = MagicLeapNativeBindings.InvalidHandle;
            this.intensityState = MLLightingTrackingNativeBindings.AmbientGlobalStateNative.Create();
            this.temperatureState = MLLightingTrackingNativeBindings.ColorTemperatureStateNative.Create();
            this.globalTemperatureColor = new Color();
            this.averageLuminance = 0;
        }
        #endif

        /// <summary>
        /// Camera sensor placements on device.
        /// </summary>
        public enum Camera : int
        {
            /// <summary>
            /// Camera sensor located on left.
            /// </summary>
            Left,

            /// <summary>
            /// Camera sensor located on right.
            /// </summary>
            Right,

            /// <summary>
            /// Camera sensor located on far left.
            /// </summary>
            FarLeft,

            /// <summary>
            /// Camera sensor located on far right.
            /// </summary>
            FarRight
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Gets kelvin color temperature
        /// </summary>
        public static ushort GlobalTemperature
        {
            get
            {
                if (!MLLightingTracking.IsValidInstance())
                {
                    MLPluginLog.ErrorFormat("MlLightingTracking.GlobalTemperature failed. Reason: No Instance for MlLightingTracking");
                    return 0;
                }

                return Instance.temperatureState.ColorTemperature;
            }
        }

        /// <summary>
        /// Gets time at which global temperature data was captured (represented as elapsed seconds
        /// since DateTime.MinValue).
        /// </summary>
        public static double GlobalTemperatureAgeSeconds
        {
            get
            {
                if (!MLLightingTracking.IsValidInstance())
                {
                    MLPluginLog.ErrorFormat("MlLightingTracking.GlobalTemperatureAgeSeconds failed. Reason: No Instance for MlLightingTracking");
                    return 0d;
                }

                return ElapsedSeconds(Instance.temperatureState.TimestampInNanoSeconds);
            }
        }

        /// <summary>
        /// Gets RGB representation of the kelvin color temperature.
        /// </summary>
        public static Color GlobalTemperatureColor
        {
            get
            {
                if (!MLLightingTracking.IsValidInstance())
                {
                    MLPluginLog.ErrorFormat("MlLightingTracking.GlobalTemperatureColor failed. Reason: No Instance for MlLightingTracking");
                    return Color.black;
                }

                return Instance.globalTemperatureColor;
            }
        }

        /// <summary>
        /// Gets array of each camera's luminance in nits.
        /// </summary>
        public static ushort[] AmbientCameraLuminance
        {
            get
            {
                if (!MLLightingTracking.IsValidInstance())
                {
                    MLPluginLog.ErrorFormat("MlLightingTracking.AmbientCameraLuminance failed. Reason: No Instance for MlLightingTracking");
                    return null;
                }

                Array.Copy(Instance.intensityState.CameraLuminance, tempCameraLuminanceCopy, MLLightingTrackingNativeBindings.CameraCount);
                return tempCameraLuminanceCopy;
            }
        }

        /// <summary>
        /// Gets time at which ambient camera luminance data was captured (represented as elapsed seconds
        /// since DateTime.MinValue).
        /// </summary>
        public static double AmbientCameraLuminanceAgeSeconds
        {
            get
            {
                if (!MLLightingTracking.IsValidInstance())
                {
                    MLPluginLog.ErrorFormat("MlLightingTracking.AmbientCameraLuminanceAgeSeconds failed. Reason: No Instance for MlLightingTracking");
                    return 0d;
                }

                return ElapsedSeconds(Instance.intensityState.TimestampInNanoSeconds);
            }
        }

        /// <summary>
        /// Gets average luminance in nits of all sensors.
        /// </summary>
        public static ushort AverageLuminance
        {
            get
            {
                if (!MLLightingTracking.IsValidInstance())
                {
                    MLPluginLog.ErrorFormat("MlLightingTracking.AverageLuminance failed. Reason: No Instance for MlLightingTracking");
                    return 0;
                }

                return Instance.averageLuminance;
            }
        }

        /// <summary>
        /// Gets accumulated R, G and B values over a 250 milliseconds time period by the Ambient Light Sensor.
        /// </summary>
        public static Vector3 RawPixelColorAverage
        {
            get
            {
                if (!MLLightingTracking.IsValidInstance())
                {
                    MLPluginLog.ErrorFormat("MlLightingTracking.RawPixelColorAverage failed. Reason: No Instance for MlLightingTracking");
                    return Vector3.zero;
                }

                return Instance.rawPixelAccumulation;
            }
        }

        /// <summary>Gets the CIE Tri-Stimulus values in Q16 format (scaled 2^16.)</summary>
        public static Vector3 TristimulusValues
        {
            get
            {
                if (!MLLightingTracking.IsValidInstance())
                {
                    MLPluginLog.ErrorFormat("MlLightingTracking.TristimulusValues failed. Reason: No Instance for MlLightingTracking");
                    return Vector3.zero;
                }

                return Instance.tristimulusValues;
            }
        }

        /// <summary>
        /// Starts MLLightingTracking.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to internal invalid input parameter.
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLLightingTracking.BaseStart(true);
        }

        #if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Turn on Lighting Tracking API.
        /// </summary>
        /// <returns>MLResult.Ok if successful.</returns>
        protected override MLResult StartAPI()
        {
            this.nativeTracker = MagicLeapNativeBindings.InvalidHandle;
            MLResult.Code resultCode = MLLightingTrackingNativeBindings.MLLightingTrackingCreate(ref this.nativeTracker);
            var result = MLResult.Create(resultCode);
            if (!result.IsOk)
            {
                MLPluginLog.ErrorFormat("MLLightingTracking.StartAPI failed to create lighting tracker. Reason: {0}", result);
            }

            return result;
        }
        #endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Destroys the NLLightingTracking instance.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Indicates if it is safe to destroy the tracker.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            if (MagicLeapNativeBindings.MLHandleIsValid(this.nativeTracker))
            {
                MLResult.Code result = MLLightingTrackingNativeBindings.MLLightingTrackingDestroy(this.nativeTracker);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLLightingTracking.CleanupAPI failed to destroy the lighting tracker. Reason: {0}", MagicLeapNativeBindings.MLGetResultString(result));
                }

                this.nativeTracker = MagicLeapNativeBindings.InvalidHandle;
            }
        }

        /// <summary>
        /// Update is called every frame. <c>>Moonobehaviour</c>. callback.
        /// </summary>
        protected override void Update()
        {
            MLResult.Code result = MLLightingTrackingNativeBindings.MLLightingTrackingGetAmbientGlobalState(this.nativeTracker, ref this.intensityState);
            if (result != MLResult.Code.Ok)
            {
                if (!this.getAmbientGlobalStateFailed)
                {
                    MLPluginLog.WarningFormat("MLLightingTracking.Update failed getting ambient global state. Reason: {0}", MagicLeapNativeBindings.MLGetResultString(result));
                    this.getAmbientGlobalStateFailed = true;
                }
            }
            else
            {
                this.getAmbientGlobalStateFailed = false;
            }

            this.CalculateGlobalAmbientScalar();
            result = MLLightingTrackingNativeBindings.MLLightingTrackingGetColorTemperatureState(this.nativeTracker, ref this.temperatureState);
            if (result != MLResult.Code.Ok)
            {
                if (!this.getColorTemperatureStateFailed)
                {
                    MLPluginLog.WarningFormat("MLLightingTracking.Update failed getting color temperature state. Reason: {0}", MagicLeapNativeBindings.MLGetResultString(result));
                    this.getColorTemperatureStateFailed = true;
                }
            }
            else
            {
                this.getColorTemperatureStateFailed = false;

                // populate tristimulus values
                this.tristimulusValues.x = this.temperatureState.XCIE;
                this.tristimulusValues.y = this.temperatureState.YCIE;
                this.tristimulusValues.z = this.temperatureState.ZCIE;

                // populate raw pixel accumulation vectors
                this.rawPixelAccumulation.x = this.temperatureState.RawPixelAverageR;
                this.rawPixelAccumulation.y = this.temperatureState.RawPixelAverageG;
                this.rawPixelAccumulation.z = this.temperatureState.RawPixelAverageB;
            }

            this.CalculateGlobalTemperatureColor();
        }

        /// <summary>
        /// Static instance of the MLLightingTracking class
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLLightingTracking.IsValidInstance())
            {
                MLLightingTracking._instance = new MLLightingTracking();
            }
        }

        /// <summary>
        /// Calculates the elapsed seconds since start of the API.
        /// </summary>
        /// <param name="timestampInNanoSeconds">Time when captured in nanoseconds since the Epoch.</param>
        /// <returns>Elapsed time in seconds</returns>
        private static double ElapsedSeconds(long timestampInNanoSeconds)
        {
            double timestampInSeconds = (double)timestampInNanoSeconds / Mathf.Pow(10, 9);
            DateTime timestampDateTime = EPOCHSTART.AddSeconds(timestampInSeconds);
            return timestampDateTime.Subtract(DateTime.MinValue).TotalSeconds;
        }

        /// <summary>
        /// Calculates the global color value using the lighting tracker data.
        /// </summary>
        private void CalculateGlobalTemperatureColor()
        {
            // Algorithm from: http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/
            double scaledTemp = Instance.temperatureState.ColorTemperature / 100.0;
            double red = 0.0;
            double green = 0.0;
            double blue = 0.0;

            if (scaledTemp <= 66)
            {
                red = 255;

                green = scaledTemp;
                green = (99.4708025861 * Math.Log(green)) - 161.1195681661;

                if (scaledTemp <= 19)
                {
                    blue = 0;
                }
                else
                {
                    blue = scaledTemp - 10;
                    blue = (138.5177312231 * Math.Log(blue)) - 305.0447927307;
                }
            }
            else
            {
                red = scaledTemp - 60;
                red = 329.698727446 * Math.Pow(red, -0.1332047592);

                green = scaledTemp - 60;
                green = 288.1221695283 * Math.Pow(green, -0.0755148492);

                blue = 255;
            }

            this.globalTemperatureColor.r = (float)(Math.Min(Math.Max(red, 0.0), 255.0) / 255.0);
            this.globalTemperatureColor.g = (float)(Math.Min(Math.Max(green, 0.0), 255.0) / 255.0);
            this.globalTemperatureColor.b = (float)(Math.Min(Math.Max(blue, 0.0), 255.0) / 255.0);
            this.globalTemperatureColor.a = 1.0f;
        }

        /// <summary>
        /// Calculates the average luminance from all camera sensors.
        /// </summary>
        private void CalculateGlobalAmbientScalar()
        {
            ulong luminanceSum = 0;
            for (int i = 0; i < MLLightingTrackingNativeBindings.CameraCount; ++i)
            {
                luminanceSum += this.intensityState.CameraLuminance[i];
            }

            this.averageLuminance = (ushort)(luminanceSum / MLLightingTrackingNativeBindings.CameraCount);
        }
        #endif
    }
}
