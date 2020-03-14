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

using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.Events;
using System;
using System.Collections;

namespace MagicLeap.Core
{
    /// <summary>
    /// This class applies a uniform scale to the ContentParent object referenced.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/MLWorldScaleBehavior")]
    public class MLWorldScaleBehavior : MonoBehaviour
    {
        /// <summary>
        /// Different types of measurements available for world scale.
        /// </summary>
        public enum ScaleMeasurement
        {
            Meters = 1,
            Decimeters = 10,
            Centimeters = 100,
            CustomUnits
        }

        /// <summary>
        /// Transform to apply scale on.
        /// </summary>
        public Transform ContentParent;

        /// <summary>
        /// Measurement type to apply world scale.
        /// </summary>
        public ScaleMeasurement Measurement = ScaleMeasurement.Meters;

        /// <summary>
        /// Value to apply as world scale when measurement type is CustomUnits.
        /// </summary>
        public float CustomValue = (float)ScaleMeasurement.Meters;

        /// <summary>
        /// Event that gets triggered whenever UpdateWorldScale is called.
        /// </summary>
        public event Action<float, string> OnUpdateEvent = delegate { };

        private float _scale = float.MinValue;

        /// <summary>
        /// Returns the scale that is being applied.
        /// </summary>
        public float Scale
        {
            get { return _scale; }
        }

        /// <summary>
        /// Returns the friendly name of the units being used based on the scale.
        /// </summary>
        public string Units
        {
            get
            {
                switch (Measurement)
                {
                    case ScaleMeasurement.Meters:
                        return "Meters";
                    case ScaleMeasurement.Decimeters:
                        return "Decimeters";
                    case ScaleMeasurement.Centimeters:
                        return "Centimeters";
                    case ScaleMeasurement.CustomUnits:
                        return "CustomUnits";
                    default:
                        Debug.LogError("WorldScale.Measurement set to an invalid value, disabling script.");
                        enabled = false;
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Ensure all required variables are set.
        /// </summary>
        void Awake()
        {
            if (ContentParent == null)
            {
                Debug.LogError("Error: MLWorldScaleBehavior.ContentParent is not set, disabling script.");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Invoke world scale value change to initial given value.
        /// This invoke forces the call to happen after all behavior Start have finished.
        /// </summary>
        void Start()
        {
            Invoke("UpdateWorldScale", 0);
        }

        /// <summary>
        /// Updates the world scale using the latest units.
        /// </summary>
        public void UpdateWorldScale()
        {
            float newScale = (Measurement == ScaleMeasurement.CustomUnits) ? CustomValue : (float)Measurement;

            if (newScale != _scale)
            {
                _scale = (Measurement == ScaleMeasurement.CustomUnits) ? CustomValue : (float)Measurement;

                float previousWorldScale = ContentParent.lossyScale.x;

                // Update the world scale on the main camera's parent.
                ContentParent.localScale = new Vector3(_scale, _scale, _scale);

                float newWorldScale = ContentParent.lossyScale.x;

                // Calculate the updated clip distances based on the world scale.
                // Assumes the original clip distances are in meters.
                Camera mainCamera = Camera.main;
                mainCamera.nearClipPlane = mainCamera.nearClipPlane / previousWorldScale * newWorldScale;
                mainCamera.farClipPlane = mainCamera.farClipPlane / previousWorldScale * newWorldScale;

                #if PLATFORM_LUMIN
                // Notify the MLDevice the scale has changed.
                MLDevice.UpdateWorldScale();
                #endif
            }

            OnUpdateEvent?.Invoke(Scale, Units);
        }
    }
}
