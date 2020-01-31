// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// MLMovementSettingsManager contains the movement session settings
    /// to use on a movement session. This class also allows to use custom
    /// settings or use the system level default ones.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/Movement/MLMovementSettingsManager")]
    public class MLMovementSettingsManager : MonoBehaviour
    {
        public bool UseDefaultSettings = true;

        /// <summary>
        /// Holds the movement session settings. Switches between
        /// global and local based on user's given value to UseGlobalConfig.
        /// </summary>
        public MLMovementSettings Settings = new MLMovementSettings()
        {
            SwayHistorySize = 30,
            MaxDeltaAngle = 0.392f,
            ControlDampeningFactor = 7.0f,
            MaxSwayAngle = 0.52f,
            MaximumHeadposeRotationSpeed = 5.23f,
            MaximumHeadposeMovementSpeed = 0.75f,
            MaximumDepthDeltaForSway = 0.1f,
            MinimumDistance = 0.5f,
            MaximumDistance = 15.0f,
            MaximumSwayTimeSeconds = 0.15f,
            EndResolveTimeoutSeconds = 10.0f
        };

        void Awake()
        {
            if (UseDefaultSettings)
            {
                MLResult result = MLMovement.GetDefaultSettings(out Settings);

                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("MLMovementSeetingsManager.Awake failed to initialize settings to default settings, disabling script. Reason: {0}", result);
                    enabled = false;
                    return;
                }
            }
        }
    }
}
