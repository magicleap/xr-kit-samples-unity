// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLInputController.cs" company="Magic Leap, Inc">
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
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary/>
    [Obsolete("Please use MLInput.Controller instead.", true)]
    public partial class MLInputController
    {
    }

    /// <summary>
    /// Manages the input state for controllers, MCA and tablet devices.
    /// </summary>
    public partial class MLInput : MLAPISingleton<MLInput>
    {
        /// <summary>
        /// Contains state information for a controller.
        /// </summary>
        public partial class Controller
        {
            /// <summary>
            /// Start the controller's LED in the specified pattern and set the duration (in seconds)
            /// </summary>
            /// <param name="pattern">The pattern of the LED effect.</param>
            /// <param name="color">The color of the LED effect.</param>
            /// <param name="duration">The duration of the LED effect.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// </returns>
            [Obsolete("Please use MLInput.Controller.StartFeedbackPatternLED(MLInput.Controller.FeedbackPatternLED, MLInput.Controller.FeedbackColorLED, float) instead.", false)]
            public MLResult StartFeedbackPatternLED(MLInputControllerFeedbackPatternLED pattern, MLInputControllerFeedbackColorLED color, float duration)
            {
                duration = Mathf.Round(duration * 1000.0f);
                if (MLControllerNativeBindings.MLInputStartControllerFeedbackPatternLED(controllerId, pattern, color, (uint)duration))
                {
                    return MLResult.Create(MLResult.Code.Ok);
                }

                MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "UnityMagicLeap - MLInputStartControllerFeedbackPatternLED() returned an error.");
                MLPluginLog.ErrorFormat("MLInputController.StartFeedbackPatternLED failed. Reason: {0}", result);

                return result;
            }

            /// <summary>
            /// Start the controller's LED performing the specified effect at the specified speed with the specified pattern for the specified duration (in seconds)
            /// </summary>
            /// <param name="effect">The LED effect.</param>
            /// <param name="speed">The speed of the LED effect.</param>
            /// <param name="pattern">The pattern of the LED effect.</param>
            /// <param name="color">The color of the LED effect.</param>
            /// <param name="duration">The duration of the LED effect.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// </returns>
            [Obsolete("Please use MLInput.Controller.StartFeedbackPatternEffectLED(MLInput.Controller.FeedbackEffectLED, MLInput.Controller.FeedbackEffectSpeedLED, MLInput.Controller.FeedbackPatternLED, float) instead.", false)]
            public MLResult StartFeedbackPatternEffectLED(MLInputControllerFeedbackEffectLED effect, MLInputControllerFeedbackEffectSpeedLED speed, MLInputControllerFeedbackPatternLED pattern, MLInputControllerFeedbackColorLED color, float duration)
            {
                duration = Mathf.Round(duration * 1000.0f);
                if (MLControllerNativeBindings.MLInputStartControllerFeedbackPatternEffectLED(controllerId, effect, speed, pattern, color, (uint)duration))
                {
                    return MLResult.Create(MLResult.Code.Ok);
                }

                MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "UnityMagicLeap - MLInputStartControllerFeedbackPatternEffectLED() returned an error.");
                MLPluginLog.ErrorFormat("MLInputController.StartFeedbackPatternEffectLED failed. Reason: {0}", result);

                return result;
            }

            /// <summary>
            /// Vibrate the controller in the specified pattern
            /// </summary>
            /// <param name="pattern">The feedback vibration pattern.</param>
            /// <param name="intensity">The intensity of the vibration.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// </returns>
            [Obsolete("Please use MLInput.Controller.StartFeedbackPatternVibe(MLInput.Controller.FeedbackPatternVibe, MLInput.Controller.FeedbackIntensity) instead.", false)]
            public MLResult StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe pattern, MLInputControllerFeedbackIntensity intensity)
            {
                if (MLControllerNativeBindings.MLInputStartControllerFeedbackPatternVibe(controllerId, pattern, intensity))
                {
                    return MLResult.Create(MLResult.Code.Ok);
                }

                MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "UnityMagicLeap - MLInputStartControllerFeedbackPatternVibe() returned an error.");
                MLPluginLog.ErrorFormat("MLInputController.StartFeedbackPatternVibe failed. Reason: {0}", result);

                return result;
            }
        }
    }
}

#endif
