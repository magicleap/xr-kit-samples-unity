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

    /// <summary>
    /// Camera characteristics and settings
    /// </summary>
    [Obsolete("Please use MLCamera.GeneralSettings instead.", true)]
    public sealed class MLCameraSettings
    {
    }

    public sealed partial class MLCamera : MLAPISingleton<MLCamera>
    {
            /// <summary>
            /// A structure containing the sensor info sensitivity
            /// </summary>
            [Obsolete("Please use MLCamera.SensorInfoSensitivityRangeValues instead.", true)]
            public struct SensorInfoSensitivtyRangeValues
            {
                /// <summary>
                /// The minimum value.
                /// </summary>
                public readonly int Minimum;

                /// <summary>
                /// The maximum value.
                /// </summary>
                public readonly int Maximum;

                internal SensorInfoSensitivtyRangeValues(int minimum, int maximum) { Minimum = minimum; Maximum = maximum; }
            }
    }
}

#endif
