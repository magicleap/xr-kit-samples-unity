using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MagicLeapTools;
using System.Linq;
#if PLATFORM_IOS
using MagicLeap.XR.XRKit;
#endif

public class RuntimeManager : MonoBehaviour
{
    public Text info;
    private string _initialInfo;

    List<GameObject> spawnedAndUnattachedGameObjects = new List<GameObject>();
    List<GameObject> attachedGameObjects = new List<GameObject>();
    
    void Awake()
    {
         _initialInfo = info.text;
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
        
        // remove from the unattached and add to attached now that it has a pcf
        spawnedAndUnattachedGameObjects.Remove(transmissionObject);
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
        TransmissionObject characterTransmissionObject = Transmission.Spawn("MV__Placement_PictureFrame", Vector3.zero, Quaternion.identity, Vector3.one);
        characterTransmissionObject.transform.SetParent(transformHelper);
        characterTransmissionObject.targetPosition = positionOffset;
        characterTransmissionObject.targetRotation = rotationOffset;
        
        attachedGameObjects.Add(characterTransmissionObject.gameObject);
    }
}
