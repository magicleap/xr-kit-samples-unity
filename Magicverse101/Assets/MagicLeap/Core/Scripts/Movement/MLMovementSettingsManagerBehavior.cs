// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://auth.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Core
{
    /// <summary>
    /// MLMovementSettingsManagerBehavior contains the movement session settings
    /// to use on a movement session. This class also allows to use custom
    /// settings or use the system level default ones.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/Movement/MLMovementSettingsManagerBehavior")]
    public class MLMovementSettingsManagerBehavior : MonoBehaviour
    {
        public bool UseDefaultSettings = true;

        /// <summary>
        /// Holds the movement session settings. Switches between
        /// global and local based on user's given value to UseGlobalConfig.
        /// </summary>
        public MLMovement.Settings Settings = new MLMovement.Settings()
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
                Settings = MLMovement.Settings.Create();
            }
        }
    }
}
