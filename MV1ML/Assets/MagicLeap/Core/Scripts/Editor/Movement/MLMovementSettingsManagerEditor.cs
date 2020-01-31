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

using UnityEditor;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System.Collections.Generic;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// This class extends the inspector for the MLMovementSettingsManager component, providing visual runtime information.
    /// </summary>
    [CustomEditor(typeof(MLMovementSettingsManager))]
    public class MLMovementSettingsManagerEditor : Editor
    {
        class Tooltips
        {
            public static readonly GUIContent UseDefaultSettings = new GUIContent(
                "Use Default Settings",
                "When enabled, movement session will use the default system settings instead of the custom ones.");

            public static readonly GUIContent HistorySize = new GUIContent(
                "History Size",
                "Number of frames of sway history to track.");

            public static readonly GUIContent MaxDeltaAngle = new GUIContent(
                "Max Delta Angle",
                "Maximum angle, in radians, between the oldest and newest headpose to object vector.");

            public static readonly GUIContent DampeningFactor = new GUIContent(
                "Dampening Factor",
                "A unitless number that governs the smoothing of Control input.");

            public static readonly GUIContent MaxSwayAngle = new GUIContent(
                "Max Sway Angle",
                "The maximum angle, in radians, that the object will be tilted left/right and front/back.");

            public static readonly GUIContent MaxSwayTime = new GUIContent(
                "Max Sway Time",
                "Maximum length of time, in seconds, lateral sway should take to decay.");

            public static readonly GUIContent MaxHeadposeRotationSpeed = new GUIContent(
                "Max Headpose Rotation Speed",
                "The speed of rotation that will stop implicit depth translation from happening.");

            public static readonly GUIContent MaxHeadposeMovementSpeed = new GUIContent(
                "Max Headpose Movement Speed",
                "The maximum speed that headpose can move, in meters per second, that will stop implicit depth translation.");

            public static readonly GUIContent MinDistance = new GUIContent(
                "Min Distance",
                "The minimum distance in meters the object can be moved in depth relative to the headpose.");

            public static readonly GUIContent MaxDistance = new GUIContent(
                "Max Distance",
                "The maximum distance in meters the object can be moved in depth relative to the headpose.");

            public static readonly GUIContent MaxDepthDeltaForSway = new GUIContent(
                "Max Depth Delta For Sway",
                "Distance object must move in depth since the last frame to cause maximum push/pull sway.");

            public static readonly GUIContent EndResolveTimeout = new GUIContent(
                "End Resolve Timeout",
                "Maximum length of time, in seconds, to allow MLMovementEnd to resolve before forcefully aborting.");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            LayoutGUI();

            this.serializedObject.ApplyModifiedProperties();
        }

        void LayoutGUI()
        {
            MLMovementSettingsManager myTarget = (MLMovementSettingsManager)target;

            EditorGUILayout.Space();

            myTarget.UseDefaultSettings = EditorGUILayout.Toggle(Tooltips.UseDefaultSettings, myTarget.UseDefaultSettings);

            if (!myTarget.UseDefaultSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {

                    EditorGUILayout.LabelField("Smoothness and Latency", EditorStyles.boldLabel);

                    myTarget.Settings.SwayHistorySize = (uint)EditorGUILayout.IntSlider(Tooltips.HistorySize, (int)myTarget.Settings.SwayHistorySize, 3, 50);
                    myTarget.Settings.MaxDeltaAngle = EditorGUILayout.Slider(Tooltips.MaxDeltaAngle, myTarget.Settings.MaxDeltaAngle, 1.0f * Mathf.Deg2Rad, 120.0f * Mathf.Deg2Rad);
                    myTarget.Settings.ControlDampeningFactor = EditorGUILayout.Slider(Tooltips.DampeningFactor, myTarget.Settings.ControlDampeningFactor, 0.5f, 10.0f);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Sway", EditorStyles.boldLabel);

                    myTarget.Settings.MaxSwayAngle = EditorGUILayout.Slider(Tooltips.MaxSwayAngle, myTarget.Settings.MaxSwayAngle, 0.0f, 60.0f * Mathf.Deg2Rad);
                    myTarget.Settings.MaximumSwayTimeSeconds = EditorGUILayout.Slider(Tooltips.MaxSwayTime, myTarget.Settings.MaximumSwayTimeSeconds, 0.01f, 0.8f);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("6-DOF Implicit Depth Change Cutoff Speeds", EditorStyles.boldLabel);

                    myTarget.Settings.MaximumHeadposeRotationSpeed = EditorGUILayout.Slider(Tooltips.MaxHeadposeRotationSpeed, myTarget.Settings.MaximumHeadposeRotationSpeed, 1.0f * Mathf.Deg2Rad, 360.0f * Mathf.Deg2Rad);
                    myTarget.Settings.MaximumHeadposeMovementSpeed = EditorGUILayout.Slider(Tooltips.MaxHeadposeMovementSpeed, myTarget.Settings.MaximumHeadposeMovementSpeed, 0.01f, 2.0f);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Depth", EditorStyles.boldLabel);

                    myTarget.Settings.MaximumDepthDeltaForSway = EditorGUILayout.Slider(Tooltips.MaxDepthDeltaForSway, myTarget.Settings.MaximumDepthDeltaForSway, 0.01f, 1.0f);
                    myTarget.Settings.MinimumDistance = EditorGUILayout.Slider(Tooltips.MinDistance, myTarget.Settings.MinimumDistance, 0.1f, 3.0f);
                    myTarget.Settings.MaximumDistance = EditorGUILayout.Slider(Tooltips.MaxDistance, myTarget.Settings.MaximumDistance, 4.0f, 30.0f);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Movement End", EditorStyles.boldLabel);

                    myTarget.Settings.EndResolveTimeoutSeconds = EditorGUILayout.FloatField(Tooltips.EndResolveTimeout, myTarget.Settings.EndResolveTimeoutSeconds);

                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
