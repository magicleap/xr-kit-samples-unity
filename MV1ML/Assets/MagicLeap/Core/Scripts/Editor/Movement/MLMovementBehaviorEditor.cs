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
    /// This class extends the inspector for the MLMovementBehavior component, providing visual runtime information.
    /// </summary>
    [CustomEditor(typeof(MLMovementBehavior))]
    public class MLMovementBehaviorEditor : Editor
    {
        class Tooltips
        {
            public static readonly GUIContent ControllerHandler = new GUIContent(
                "Controller Connection Handler",
                "Reference to the ControllerConnectionHandler script to drive the movement.");

            public static readonly GUIContent SettingsManager = new GUIContent(
                "Movement Session Settings Manager",
                "Reference to the MLMovementSettingsManager script containing the session settings.");

            public static readonly GUIContent RunOnStart = new GUIContent(
                "Run On Start",
                "Indicates if movement session will start on Start or manually started later on.");

            public static readonly GUIContent AllowCollision = new GUIContent(
                "Allow Collision",
                "When enabled, object will collide against objects with the MLMovementCollider component");

            public static readonly GUIContent UseTouchForDepth = new GUIContent(
                "Use Touch For Depth",
                "When enabled, touchpad will be available to change the position of the object. Press top of the touchpad to move object away and bottom to move object closer to the user.");

            public static readonly GUIContent MaxDepthDelta = new GUIContent(
                "Max Depth Delta",
                "Maximum depth change per movement update allowed.");

            public static readonly GUIContent UseTouchForRotation = new GUIContent(
                "Use Touch For Rotation",
                "When enabled, touchpad will be available to change the rotation of the object. Radial scroll on touchpad will rotate object towards appropiate direction.");

            public static readonly GUIContent MaxRotationDelta = new GUIContent(
                "Max Rotation Delta",
                "Maximum rotation change per movement update allowed in degrees.");

            public static readonly GUIContent InteractionMode = new GUIContent(
                "Interaction Mode",
                "Type of interaction for this movement session.");

            public static readonly GUIContent AutoCenter = new GUIContent(
                "Auto Center",
                "If the object should automatically center on the control direction when beginning movement.");

            public static readonly GUIContent InputDriverType = new GUIContent(
                "Input Driver Type",
                "Type of input to drive this movement session.");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            LayoutGUI();

            this.serializedObject.ApplyModifiedProperties();
        }

        void LayoutGUI()
        {
            MLMovementBehavior myTarget = (MLMovementBehavior)target;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            myTarget.ControllerHandler = (ControllerConnectionHandler)EditorGUILayout.ObjectField(Tooltips.ControllerHandler, (Object)myTarget.ControllerHandler, typeof(ControllerConnectionHandler), true);

            EditorGUILayout.Space();

            myTarget.SettingsManager = (MLMovementSettingsManager)EditorGUILayout.ObjectField(Tooltips.SettingsManager, (Object)myTarget.SettingsManager, typeof(MLMovementSettingsManager), true);

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.Space();

                myTarget.RunOnStart = EditorGUILayout.Toggle(Tooltips.RunOnStart, myTarget.RunOnStart);

                EditorGUILayout.Space();

                myTarget.AllowCollision = EditorGUILayout.Toggle(Tooltips.AllowCollision, myTarget.AllowCollision);
                if (myTarget.AllowCollision)
                {
                    if (myTarget.gameObject.GetComponent<Collider>() == null)
                    {
                        Debug.LogError("Error: MLMovementBehavior.AllowCollision cannot be enabled if object doesn't contain a Collider component.");
                        myTarget.AllowCollision = false;
                    }

                    if (myTarget.gameObject.GetComponent<Rigidbody>() == null)
                    {
                        Debug.LogError("Error: MLMovementBehavior.AllowCollision cannot be enabled if object doesn't contain a Rigidbody component.");
                        myTarget.AllowCollision = false;
                    }
                }

                EditorGUILayout.Space();

                myTarget.UseTouchForDepth = EditorGUILayout.Toggle(Tooltips.UseTouchForDepth, myTarget.UseTouchForDepth);
                if (myTarget.UseTouchForDepth)
                {
                    myTarget.MaxDepthDelta = EditorGUILayout.FloatField(Tooltips.MaxDepthDelta, myTarget.MaxDepthDelta);
                }

                myTarget.UseTouchForRotation = EditorGUILayout.Toggle(Tooltips.UseTouchForRotation, myTarget.UseTouchForRotation);
                if (myTarget.UseTouchForRotation)
                {
                    myTarget.MaxRotationDelta = EditorGUILayout.FloatField(Tooltips.MaxRotationDelta, myTarget.MaxRotationDelta);
                }

                EditorGUILayout.Space();

                myTarget.InteractionMode = (MLMovementBehavior.MovementInteractionMode)EditorGUILayout.EnumPopup(Tooltips.InteractionMode, myTarget.InteractionMode);

                myTarget.AutoCenter = EditorGUILayout.Toggle(Tooltips.AutoCenter, myTarget.AutoCenter);

                EditorGUILayout.Space();

                myTarget.InputDriverType = (MLMovementBehavior.MovementInputDriverType)EditorGUILayout.EnumPopup(Tooltips.InputDriverType, myTarget.InputDriverType);

                EditorGUILayout.Space();

            }
            EditorGUILayout.EndVertical();
        }
    }
}
