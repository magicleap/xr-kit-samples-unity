﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MagicLeapTools;
using System.Linq;
#if PLATFORM_IOS
using MagicLeap.XR.XRKit;
#elif PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

public class RuntimeManager : MonoBehaviour
{
#if PLATFORM_LUMIN
    public ControlInput controlInput;
#endif

    public Text info;
    private string _initialInfo;
    List<GameObject> spawnedAndUnattachedGameObjects;
    List<GameObject> attachedGameObjects;

    void Awake()
    {
         _initialInfo = info.text;
        spawnedAndUnattachedGameObjects = new List<GameObject>();
        attachedGameObjects = new List<GameObject>();

        #if PLATFORM_LUMIN
            //trigger events:
            controlInput.OnTriggerDown.AddListener(HandleTriggerDown);
        #endif
    }

    // Update is called once per frame
    private void Update()
    {
        #if PLATFORM_IOS
            UpdateTouches();
        #endif

        string output = _initialInfo + System.Environment.NewLine;
        output += "Peers Available: " + Transmission.Peers.Length + System.Environment.NewLine;

        info.text = output;
    }

    private void SendRPC()
    {
        //RPCs use SendMessage under the hood and are sent to the Transmission GameObject and any GameObject in its RPC Targets
        RPCMessage rpcMessage = new RPCMessage("ChangeColor");
        Transmission.Send(rpcMessage);
    }

    //Public Methods(RPCs):
    public void ChangeColor()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        foreach(var go in attachedGameObjects)
        {
            go.GetComponent<Material>().color = randomColor;
        }
    }

#if PLATFORM_IOS

    void UpdateTouches()
    {
        if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began) {

            // Touch the screen and place 5 units in front of the touch position
            Vector3 fingerPos = Input.GetTouch (0).position;
            fingerPos.z = 5;
            Vector3 objPos = Camera.main.ScreenToWorldPoint (fingerPos);

            // Sort the list of PCFs by distance to where the object will be spawned and retreive the first pcf in the list (since its the closest)
            var pcfList = MagicversePcfManager.PCFListSortedByDistanceTo(objPos);
            if (pcfList.Count > 0) {
                MagicversePcfManager.PcfPoseData pcfToBindTo = pcfList[0].Value;
                SpawnAndAttachToPCF(objPos, pcfToBindTo.position, pcfToBindTo.rotation);
            } 
            else{
                // cant bind yet. Spawn locally but update when PCF lists are obtained from PCFsAdded callback.
                TransmissionObject characterTransmissionObject = Transmission.Spawn("Dummy", objPos, Quaternion.identity, Vector3.one);
                spawnedAndUnattachedGameObjects.Add(characterTransmissionObject.gameObject);
                Debug.LogWarning("Spawned but not attached to PCF");
            }
        }
    }

    public void PCFsAdded() {
        foreach (var to in spawnedAndUnattachedGameObjects){
            var objPos = to.transform.position;
            var pcfList = MagicversePcfManager.PCFListSortedByDistanceTo(objPos);
            if (pcfList.Count > 0) {
                MagicversePcfManager.PcfPoseData pcfToBindTo = pcfList[0].Value;
                AttachToPCF(to, pcfToBindTo.position, pcfToBindTo.rotation);
            }
        }
        // we've added them to the attached list, clear from here.
        spawnedAndUnattachedGameObjects.Clear();
    }

#elif PLATFORM_LUMIN
    private void HandleTriggerDown()
    {
        SendRPC();
    }
#endif

    void AttachToPCF(GameObject transmissionObject, Vector3 pcfPosition, Quaternion pcfRotation){

        var transformHelper = new GameObject("(TransformHelper)").transform;
        transformHelper.gameObject.hideFlags = HideFlags.HideInHierarchy;
        transformHelper.SetPositionAndRotation(pcfPosition, pcfRotation);
        
        Vector3 positionOffset = transformHelper.InverseTransformPoint(transmissionObject.transform.position);
        Quaternion rotationOffset = Quaternion.Inverse(transformHelper.rotation) * transmissionObject.transform.rotation;

        transmissionObject.transform.SetParent(transformHelper);
        transmissionObject.GetComponent<TransmissionObject>().targetPosition = positionOffset;
        transmissionObject.GetComponent<TransmissionObject>().targetRotation = rotationOffset;
        
        attachedGameObjects.Add(transmissionObject.gameObject);
    }

    void SpawnAndAttachToPCF(Vector3 objPosition, Vector3 pcfPosition, Quaternion pcfRotation)
    {
        // bind the object to the PCF
        var transformHelper = new GameObject("(TransformHelper)").transform;
        transformHelper.gameObject.hideFlags = HideFlags.HideInHierarchy;
        transformHelper.SetPositionAndRotation(pcfPosition, pcfRotation);
        
        Vector3 positionOffset = transformHelper.InverseTransformPoint(objPosition);
        Quaternion rotationOffset = Quaternion.Inverse(transformHelper.rotation) * Quaternion.LookRotation(Vector3.forward);

        // spawn everywhere and on the network using the local position and rotation (pcf offset) 
        TransmissionObject characterTransmissionObject = Transmission.Spawn("Dummy", Vector3.zero, Quaternion.identity, Vector3.one);
        characterTransmissionObject.transform.SetParent(transformHelper);
        characterTransmissionObject.targetPosition = positionOffset;
        characterTransmissionObject.targetRotation = rotationOffset;
        
        attachedGameObjects.Add(characterTransmissionObject.gameObject);
    }
}
