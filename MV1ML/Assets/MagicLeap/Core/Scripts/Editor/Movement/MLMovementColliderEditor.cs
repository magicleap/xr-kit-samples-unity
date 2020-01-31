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
    /// This class extends the inspector for the MLMovementCollider component, providing visual runtime information.
    /// </summary>
    [CustomEditor(typeof(MLMovementCollider))]
    public class MLMovementColliderEditor : Editor
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
            MLMovementCollider myTarget = (MLMovementCollider)target;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            myTarget.ColliderType = (MLMovementCollider.MovementColliderType)EditorGUILayout.EnumPopup(Tooltips.ColliderType, myTarget.ColliderType);

            if (myTarget.ColliderType == MLMovementCollider.MovementColliderType.Soft)
            {
                myTarget.MaxDepth = EditorGUILayout.IntSlider(Tooltips.MaxDepth, myTarget.MaxDepth, 0, 100);
            }
        }
    }
}
