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

using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeap.Core
{
    /// <summary>
    /// This class extends the inspector for the MLWorldScaleBehavior component, providing visual runtime information.
    /// </summary>
    [CustomEditor(typeof(MLWorldScaleBehavior))]
    public class MLWorldScaleBehaviorEditor : Editor
    {
        class Tooltips
        {
            public static readonly GUIContent ContentParent = new GUIContent(
                "Content Parent",
                "Reference to the transform to which the scale will be applied to and propagated to it's children.");

            public static readonly GUIContent Measurement = new GUIContent(
                "Measurement",
                "Measurement type to apply as worldscale.");

            public static readonly GUIContent CustomValue = new GUIContent(
                "Custom Value",
                "Custom value to apply as worlscale.");

            public static readonly GUIContent OnUpdateEvent = new GUIContent(
                "On Update Event",
                "Event that gets triggered when world scale is updated.");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            LayoutGUI();

            this.serializedObject.ApplyModifiedProperties();
        }

        void LayoutGUI()
        {
            MLWorldScaleBehavior myTarget = (MLWorldScaleBehavior)target;

            EditorGUILayout.Space();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(myTarget), typeof(MLWorldScaleBehavior), false);
            GUI.enabled = true;

            EditorGUILayout.Space();

            myTarget.ContentParent = (Transform) EditorGUILayout.ObjectField(Tooltips.ContentParent, (Object)myTarget.ContentParent, typeof(Transform), true);

            EditorGUILayout.Space();

            myTarget.Measurement = (MLWorldScaleBehavior.ScaleMeasurement)EditorGUILayout.EnumPopup(Tooltips.Measurement, myTarget.Measurement);

            if (myTarget.Measurement == MLWorldScaleBehavior.ScaleMeasurement.CustomUnits)
            {
                myTarget.CustomValue = EditorGUILayout.FloatField(Tooltips.CustomValue, myTarget.CustomValue);
            }
        }
    }
}
