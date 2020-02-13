using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MagicLeapTools;
using System.Linq;
#if PLATFORM_IOS // TODO: ANDROID
using MagicLeap.XR.XRKit;
#endif

// TODO: if PLATFORM_LUMIN Add basic cube placement using MLControl trigger

public class RuntimeManager : MonoBehaviour
{
    public Text info;
    private string _initialInfo;
    List<GameObject> attachedGameObjects;

    [Tooltip("A prefab located in the resource folder with TransmissionObject on it, which is needed for spawning across the network")]
    public GameObject resourceToSpawn; 

    void Awake()
    {
         _initialInfo = info.text;
        attachedGameObjects = new List<GameObject>();
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
                SpawnAndAttachToPCF(resourceToSpawn.name, objPos, pcfToBindTo.pcfId, pcfToBindTo.position, pcfToBindTo.rotation);
            } 
            else{
               
                Debug.LogWarning("No PCFs to attach to yet, are y9ou logged in to the mlcloud via Oath?");
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
#endif

    void AttachToPCF(GameObject transmissionObject, Vector3 pcfPosition, Quaternion pcfRotation){

        var transformHelper = new GameObject("(TransformHelper)").transform;
        transformHelper.gameObject.hideFlags = HideFlags.HideInHierarchy;
        transformHelper.SetPositionAndRotation(pcfPosition, pcfRotation);
        
        Vector3 positionOffset = transformHelper.InverseTransformPoint(transmissionObject.transform.position);
        Quaternion rotationOffset = Quaternion.Inverse(transformHelper.rotation) * transmissionObject.transform.rotation;

        //transmissionObject.transform.SetParent(transformHelper);
        transmissionObject.GetComponent<TransmissionObject>().targetPosition = positionOffset;
        transmissionObject.GetComponent<TransmissionObject>().targetRotation = rotationOffset;
        
        attachedGameObjects.Add(transmissionObject.gameObject);
    }
    void SpawnAndAttachToPCF(string resourceName, Vector3 objPosition, string pcfid, Vector3 pcfPosition, Quaternion pcfRotation)
    {
        // bind the object to the PCF
        var transformHelper = new GameObject("(TransformHelper)").transform;
        transformHelper.gameObject.hideFlags = HideFlags.HideInHierarchy;
        transformHelper.SetPositionAndRotation(pcfPosition, pcfRotation);
        
        Vector3 positionOffset = transformHelper.InverseTransformPoint(objPosition);
        Quaternion rotationOffset = Quaternion.Inverse(transformHelper.rotation) * Quaternion.LookRotation(Vector3.forward);

        // TODO: HACK: pass in the pcfid and let Transmission handle it
        var resourceObjectGuidHack = resourceName + ":" + pcfid;

        // spawn everywhere and on the network using the local position and rotation (pcf offset) 
        TransmissionObject characterTransmissionObject = Transmission.Spawn(resourceObjectGuidHack, positionOffset, rotationOffset, Vector3.one);
        
        attachedGameObjects.Add(characterTransmissionObject.gameObject);
    }
}
