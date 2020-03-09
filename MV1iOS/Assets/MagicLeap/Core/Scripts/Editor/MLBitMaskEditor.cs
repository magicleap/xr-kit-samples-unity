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
using UnityEditor;

namespace UnityEngine.XR.MagicLeap
{
    #if PLATFORM_LUMIN
    [CustomPropertyDrawer(typeof(MLBitMask))]
    public class MLBitMaskEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type propertyType = (attribute as MLBitMask).PropertyType;

            string[] enumNames = Enum.GetNames(propertyType);
            int[] enumValues = (int[]) Enum.GetValues(propertyType);

            int curIntValue = property.intValue;
            int curMaskValue = 0;

            for (int index = 0; index < enumValues.Length; ++index)
            {
                if ((curIntValue & enumValues[index]) == enumValues[index])
                {
                    curMaskValue |= 1 << index;
                }
            }

            // Draw the field using the built in MaskField functionality
            // However, since MaskField has no reference to the System.Type
            // of our enum, the value that is returned will not be shifted
            int newMaskValue = EditorGUI.MaskField(position, label, curMaskValue, enumNames);

            // Reset the current value
            curIntValue = 0;

            // Go through each value in the new mask and set the correct bit
            for (int index = 0; index < enumValues.Length; ++index)
            {
                if ((newMaskValue & (1 << index)) == (1 << index))
                {
                    curIntValue |= enumValues[index];
                }
            }

            // Make sure to set the value of the property in the end
            property.intValue = curIntValue;
        }
    }
    #endif
}
