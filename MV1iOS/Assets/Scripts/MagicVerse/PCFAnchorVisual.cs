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
            Debug.LogFormat("Added MLXRAnchor: {0}", anchorText);
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

#endif
}
