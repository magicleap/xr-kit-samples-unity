// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#elif PLATFORM_IOS || PLATFORM_ANDROID
using MagicLeap.XR.XRKit;
#endif

public class PCFAnchorVisual : MonoBehaviour
{
#if PLATFORM_IOS || PLATFORM_ANDROID

    private MLXRAnchor _anchor; 
    public MLXRAnchor Anchor {
        get { return this._anchor; }
        set {
            TextMesh tm = GetComponentInChildren<TextMesh>();
            string anchorText = MakeAnchorString(value);
            tm.text = anchorText;
            this._anchor = value;
            Debug.LogFormat("Added MLXRAnchor Visual: {0}", anchorText);
        }
    }
        
    private static string MakeAnchorString(MLXRAnchor anchor)
    {
        return $"ID: {anchor.id}\n"
            + $"pose: {anchor.pose}\n"
            + $"confidence: {anchor.confidence.confidence}\n"
            + $"rotation error: {anchor.confidence.rotation_err_deg} degrees\n"
            + $"translation error: {anchor.confidence.translation_err_m} meters\n"
            + $"valid radius: {anchor.confidence.valid_radius_m} meters";
    }

#elif PLATFORM_LUMIN
    private MLPersistentCoordinateFrames.PCF _pcf;
    public MLPersistentCoordinateFrames.PCF PCF {
        get { return this._pcf; }
        set {
            TextMesh tm = GetComponentInChildren<TextMesh>();
            string pcfText = MakePCFString(value);
            tm.text = pcfText;
            this._pcf= value;
            Debug.LogFormat("Added MLPersistentCoordinateFrames.PCF Visual: {0}", pcfText);
        }
    }

    private static string MakePCFString(MLPersistentCoordinateFrames.PCF pcf)
    {
        return $"ID: {pcf.CFUID.ToString()}\n"
            + $"position: {pcf.Position}\n"
            + $"rotation: {pcf.Rotation}\n"
            + $"confidence: {pcf.Confidence}\n"
            + $"rotation error: {pcf.RotationErrDeg} degrees\n"
            + $"translation error: {pcf.TranslationErrM} meters\n"
            + $"valid radius: {pcf.ValidRadiusM} meters\n"
            + $"type: {pcf.Type}";
    }

#endif

}
