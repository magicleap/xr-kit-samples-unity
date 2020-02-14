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
