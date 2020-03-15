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

namespace MagicLeapTools
{
    public static class CurveUtilities
    {
        //Public Methods:
        //Quadratic Bezier:
        public static Vector3 GetPoint(Vector3 startPosition, Vector3 controlPoint, Vector3 endPosition, float percentage)
        {
            percentage = Mathf.Clamp01(percentage);
            float oneMinusT = 1f - percentage;
            return oneMinusT * oneMinusT * startPosition + 2f * oneMinusT * percentage * controlPoint + percentage * percentage * endPosition;
        }

        public static Vector3 GetFirstDerivative(Vector3 startPoint, Vector3 controlPoint, Vector3 endPosition, float percentage)
        {
            percentage = Mathf.Clamp01(percentage);
            return 2f * (1f - percentage) * (controlPoint - startPoint) + 2f * percentage * (endPosition - controlPoint);
        }
    }
}