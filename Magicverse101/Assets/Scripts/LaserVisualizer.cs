//
// </copyright>
//
// %COPYRIGHTEND%
// ---------------------------------------------------------------------
// %BANNEREND%
// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeapTools;

public class LaserVisualizer : MonoBehaviour
{
    public LineRenderer laser;

    public ControlInput controlInput; 

    public float distance = 5.0f;
    
    // Update is called once per frame
    void Update()
    {
        laser.SetPosition(0, controlInput.transform.position + controlInput.transform.forward);
        laser.SetPosition(1, controlInput.transform.position + controlInput.transform.forward * distance);
    }
}
