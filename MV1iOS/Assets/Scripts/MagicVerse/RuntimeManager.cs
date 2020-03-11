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
using UnityEngine.UI;
using MagicLeapTools;
using System.Linq;
#if PLATFORM_IOS || PLATFORM_ANDROID
using MagicLeap.XR.XRKit;
#elif PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

public class RuntimeManager : MonoBehaviour
{
    public Text info;
    private string _initialInfo;
    List<GameObject> attachedGameObjects;

    [Tooltip("Disable this if you dont want to use the test placement methods in this class")]
    public bool testPlacement = true; 

    #if PLATFORM_LUMIN
    [Tooltip("Needed only if Test Placement is checked to use control for testing object placement")]

    public ControlInput controlInput;
    #endif

    [Tooltip("A prefab located in the resource folder with TransmissionObject on it, which is needed for spawning across the network, will only be used if Test Placement is checked")]
    public GameObject resourceToSpawn; 

    void Awake()
    {
         _initialInfo = info.text;
        attachedGameObjects = new List<GameObject>();

        #if PLATFORM_LUMIN
            if (testPlacement)
                controlInput.OnTriggerDown.AddListener(HandleTriggerDown);
        #endif

        #if PLATFORM_IOS || PLATFORM_ANDROID
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        #endif

    }

    // Update is called once per frame
    private void Update()
    {
        #if PLATFORM_IOS || PLATFORM_ANDROID
            if (testPlacement)
                UpdateTouches();
        #endif

        string output = _initialInfo + System.Environment.NewLine;
        output += "Peers Available: " + Transmission.Peers.Length + System.Environment.NewLine;

        info.text = output;
    }

#if PLATFORM_IOS || PLATFORM_ANDROID

    void UpdateTouches()
    {
        if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began) {

            // Touch the screen and place 5 units in front of the touch position
            Vector3 fingerPos = Input.GetTouch (0).position;
            fingerPos.z = 5;
            Vector3 objPos = Camera.main.ScreenToWorldPoint (fingerPos);

            // Sort the list of PCFs by distance to where the object will be spawned and retreive the first pcf in the list (since its the closest)
            var pcfList = PCFSystem.PCFListSortedByDistanceTo(objPos);
            if (pcfList.Count > 0) {
                PCFSystem.PcfPoseData pcfToBindTo = pcfList[0].Value;
                SpawnAndAttachToPCF(resourceToSpawn.name, objPos, pcfToBindTo.pcfId, pcfToBindTo.position, pcfToBindTo.rotation);
            } 
            else{
               
                Debug.LogWarning("No PCFs to attach to yet, are you logged in to the mlcloud via Oath?");
            }
        }
    }

#elif PLATFORM_LUMIN
    private void HandleTriggerDown()
    {
            Vector3 objPos = controlInput.transform.position + controlInput.transform.forward * 5;

            MLPersistentCoordinateFrames.PCF pcfToBindTo;
            var returnResult = MLPersistentCoordinateFrames.FindClosestPCF(objPos, out pcfToBindTo, MLPersistentCoordinateFrames.PCF.Types.MultiUserMultiSession, true);
            SpawnAndAttachToPCF(resourceToSpawn.name, objPos, pcfToBindTo.CFUID.ToString(), pcfToBindTo.Position, pcfToBindTo.Rotation);
    }

#endif

    void SpawnAndAttachToPCF(string resourceName, Vector3 objPosition, string pcfid, Vector3 pcfPosition, Quaternion pcfRotation)
    {
        // bind the object to the PCF
        var transformHelper = new GameObject("(TransformHelper)").transform;
        transformHelper.gameObject.hideFlags = HideFlags.HideInHierarchy;
        transformHelper.SetPositionAndRotation(pcfPosition, pcfRotation);
        
        Vector3 positionOffset = transformHelper.InverseTransformPoint(objPosition);
        Quaternion rotationOffset = Quaternion.Inverse(transformHelper.rotation) * Quaternion.LookRotation(Vector3.forward);

        // spawn everywhere and on the network using the local position and rotation (pcf offset) 
        TransmissionObject characterTransmissionObject = Transmission.Spawn(resourceName, positionOffset, rotationOffset, Vector3.one, pcfid);
        
        attachedGameObjects.Add(characterTransmissionObject.gameObject);
    }
}
