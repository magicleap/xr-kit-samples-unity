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
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System.Collections.Generic;

namespace MagicLeap.Core
{
    /// <summary>
    /// This class extends the inspector for the MLMovementColliderBehavior component, providing visual runtime information.
    /// </summary>
    [CustomEditor(typeof(MLMovementColliderBehavior))]
    public class MLMovementColliderBehaviorEditor : Editor
    {
        class Tooltips
        {
            public static readonly GUIContent ColliderType = new GUIContent(
                "Collider Type",
                "Type of movement collider.");

            public static readonly GUIContent MaxDepth = new GUIContent(
                "Max Depth",
                "Maximum depth percentage collider object will penetrate the object.");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            LayoutGUI();

            this.serializedObject.ApplyModifiedProperties();
        }

        void LayoutGUI()
        {
            MLMovementColliderBehavior myTarget = (MLMovementColliderBehavior)target;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            myTarget.ColliderType = (MLMovementColliderBehavior.MovementColliderType)EditorGUILayout.EnumPopup(Tooltips.ColliderType, myTarget.ColliderType);

            if (myTarget.ColliderType == MLMovementColliderBehavior.MovementColliderType.Soft)
            {
                myTarget.MaxDepth = EditorGUILayout.IntSlider(Tooltips.MaxDepth, myTarget.MaxDepth, 0, 100);
            }
        }
    }
}
