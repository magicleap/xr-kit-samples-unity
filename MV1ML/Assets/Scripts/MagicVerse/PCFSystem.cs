// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MagicLeapTools;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#elif PLATFORM_IOS
using MagicLeap.XR.XRKit;
#endif

public class PCFSystem : MonoBehaviour
{
#if PLATFORM_IOS
    public MLXRSession MLXRSessionInstance;
#endif
    public class PcfPoseData
    {
        public string pcfId;
        public Vector3 position;
        public Quaternion rotation;
    }

    public static Dictionary<string, PcfPoseData> PcfPoseLookup = new Dictionary<string, PcfPoseData>();
    public static List<KeyValuePair<string, PcfPoseData>> PCFListSortedByDistanceTo(Vector3 objPos){
        var pcfList = PcfPoseLookup.ToList();
        pcfList = pcfList.OrderBy(p => Vector3.Distance(objPos, p.Value.position)).ToList();
        return pcfList;
    }

    public enum PCFStatus
    {
        Unavailable,
        PrivilegesDeclined,
        Active
    }
    PCFStatus pcfStatus = PCFStatus.Unavailable;
    public Text _pcfStatusText;
    bool pcfsStarted = false;

    public bool displayDebugVisuals;
    public PCFAnchorVisual AnchorVisualPrefab;
    public float AnchorVisualUpdateRate = 0.1f;
    public Camera cam;
    private bool LoggedLocalizedOnce = false;
    private GameObject visualParent;
    private int numAnchors = 0;

    void Awake()
    {
        if (_pcfStatusText != null)
        {
            _pcfStatusText.text = "Status: Requesting Privileges";
        }

        Transmission.Instance.SetPCFPoseDelegate(PoseForPCFID);
    }

    void Start()
    {
#if PLATFORM_LUMIN
        StartPCFS();
#elif PLATFORM_IOS
        if (MLXRSessionInstance == null)
        {
            Debug.LogError("Don't have a reference to an MLXRSessionInstance.");
        }
        // Register for the Anchor callbacks
        MLXRSessionInstance.anchorsChanged += HandleAnchorsChanged;
#endif

        visualParent = new GameObject("Anchors");
        visualParent.transform.parent = transform;

    }

    void OnDestroy()
    {
#if PLATFORM_LUMIN
        if (pcfsStarted)
        {
            MLPersistentCoordinateFrames.Stop();
        }
#endif
    }

    Func<Vector3, Quaternion, Pose> returnPose = (p,r) => { return new Pose(p, r); };
    public void PoseForPCFID(string pcfId, Action<bool, Pose> poseHandler)
    {                            
        // Find PCF
        Debug.Log($"PoseForPCFID with PCFID: {pcfId}");

#if PLATFORM_LUMIN
        List<MLPCF> allPcfs = new List<MLPCF>();
        if (MLPersistentCoordinateFrames.GetAllPCFs(out allPcfs).IsOk)
        {
            foreach (MLPCF mlPCF in allPcfs)
            {
                if (mlPCF.CFUID.ToString().Equals(pcfId))
                {
                    MLPersistentCoordinateFrames.GetPCFPose(mlPCF, (MLResult result, MLPCF posedPCF) =>
                    {
                        if (result.IsOk)
                        {
                            Pose pose  = returnPose(posedPCF.Position, posedPCF.Orientation);
                            Debug.Log($"PoseForPCFID callback PCF: {mlPCF} Returning Pose: {pose}");
                            poseHandler(true, pose);
                        } else {
                            Debug.Log("PoseForPCFID Error: No Matching Pcf found");
                            poseHandler(false, returnPose(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0)));
                        }
                    });
                }
            }
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (PcfPoseLookup.ContainsKey(pcfId))
        {
            PcfPoseData poseData = PcfPoseLookup[pcfId];
            Pose pose = returnPose(poseData.position, poseData.rotation);
            Debug.Log($"PoseForPCFID callback PCF: {pcfId} Returning Pose: {pose}");
            poseHandler(true, pose);

        } else
        {
            Debug.Log("PoseForPCFID Error: No Matching Pcf found");
            poseHandler(false, returnPose(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0)));
        }
#endif
    }

    void Update()
    {
        // Currently Update is just used to display debug visuals. 
        if (displayDebugVisuals)
            return;

#if PLATFORM_IOS
        // Don't attempt to do anything, unless the MLXRSession has started
        if (!MLXRSessionInstance.gameObject.activeSelf)
        {
            return;
        }
#endif
        // For each existing Anchor Visual, billboard the ID to the current Headpose
        foreach (Transform child in visualParent.transform)
        {
            TextMesh tm = child.gameObject.GetComponentInChildren<TextMesh>();
            if (!tm)
            {
                continue;
            }

            tm.transform.LookAt(tm.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
        }

        // All direct children of the child container will represent a single Anchor
        numAnchors = visualParent.transform.childCount;


        StringBuilder sb = new StringBuilder();
#if PLATFORM_IOS
        sb.AppendFormat("Localization Status: {0}\n", MLXRSessionInstance.GetLocalizationStatus());

        // Once we've localized one time, we want to start printing # of Anchors
        if (!LoggedLocalizedOnce)
        {
            if (MLXRSessionInstance.IsLocalized())
            {
                LoggedLocalizedOnce = true;
            }
        }
#endif
        if (LoggedLocalizedOnce)
        {
            sb.AppendFormat("Number of Anchors Found: {0}\n", numAnchors);
        }

        _pcfStatusText.text = sb.ToString();
    }


#if PLATFORM_IOS

    public void HandleAnchorsChanged(MLXRSession.AnchorsUpdatedEventArgs e)
    {
        foreach (MLXRAnchor anchor in e.added)
        {
            Debug.Log("PCF: ADD " + anchor.id);
            string anchorString = anchor.id.ToString();
            if (!PcfPoseLookup.ContainsKey(anchorString))
            {
                PcfPoseLookup[anchorString] = (new PcfPoseData()
                {
                    pcfId = anchorString,
                    position = anchor.pose.position,
                    rotation = anchor.pose.rotation
                });
            }

            if (displayDebugVisuals) 
            {
                Pose pose = anchor.pose;
                PCFAnchorVisual newVisual = Instantiate(AnchorVisualPrefab, pose.position, pose.rotation);
                newVisual.transform.parent = visualParent.transform;
                newVisual.GetComponent<PCFAnchorVisual>().Anchor = anchor;
            }
        }
    
        foreach (MLXRAnchor anchor in e.removed)
        {
            Debug.Log("PCF: REMOVE " + anchor.id);
            string anchorString = anchor.id.ToString();
            if (PcfPoseLookup.ContainsKey(anchorString))
            {
                PcfPoseLookup.Remove(anchorString);
            }

            if (displayDebugVisuals) {
                foreach (var visual in visualParent.GetComponentsInChildren<PCFAnchorVisual>()){
                    if (visual.Anchor.id.Equals(anchor.id)){
                        DestroyImmediate(visual.gameObject);
                    }
                }
            }
        }

        foreach (MLXRAnchor anchor in e.updated)
        {
            string anchorString = anchor.id.ToString();
            if (PcfPoseLookup.ContainsKey(anchorString))
            {
                PcfPoseLookup[anchorString].position = anchor.pose.position;
                PcfPoseLookup[anchorString].rotation = anchor.pose.rotation;
            }

            if (displayDebugVisuals) {
                foreach (var visual in visualParent.GetComponentsInChildren<PCFAnchorVisual>()){
                    if (visual.Anchor.id.Equals(anchor.id)){
                        Pose pose = anchor.pose;
                        visual.gameObject.transform.position = pose.position;
                        visual.gameObject.transform.rotation = pose.rotation;
                        Debug.LogFormat("PCF Updated anchor: was {0}, is now {1}", visual.Anchor.ToString(), anchor.ToString());
                        visual.Anchor = anchor;
                    }
                }
            }
        }
    }
#endif

#if PLATFORM_LUMIN

    void StartPCFS()
    {
        MLResult result = MLPersistentCoordinateFrames.Start();
        if (!result.IsOk)
        {
            if (result.Code == MLResultCode.PrivilegeDenied)
            {
                Instantiate(Resources.Load("PrivilegeDeniedError"));
            }

            Debug.LogErrorFormat("Error: PersistenceExample failed starting MLPersistentCoordinateFrames, disabling script. Reason: {0}", result);
            enabled = false;
            return;
        }

        pcfsStarted = true;
        if (MLPersistentCoordinateFrames.IsReady)
        {
            // Success of PCF systems. Handle startup
            PerformStartup();
        }
        else
        {
            // Wait for initialization callback to check for PCF status
            MLPersistentCoordinateFrames.OnInitialized += HandleInitialized;
        }
    }

    void HandleInitialized(MLResult status)
    {
        MLPersistentCoordinateFrames.OnInitialized -= HandleInitialized;

        if (status.IsOk)
        {
            if (_pcfStatusText != null) {
                _pcfStatusText.text = "<color=green>World Map Loaded</color>";
            }
            PerformStartup();
        }
        else
        {
            /// If the map initialization fails, it continues to scan in the background
            /// until it finds a landscape it can use. From here, re-attempt to startup the
            /// PersistenceSystems.
            /// 
            if (_pcfStatusText != null) {
                _pcfStatusText.text = string.Format("<color=red>{0}</color>", status);
            }
            Debug.LogErrorFormat("Error: MLPersistentCoordinateFrames failed to initialize, trying again. Reason: {0}", status);
            MLPersistentCoordinateFrames.Stop();
            pcfsStarted = false;

            Invoke("StartPCFs", 3);
        }
    }

    public delegate void PCFsReadyCallback(PCFSystem system);
    public static PCFsReadyCallback OnPcfsReady;

    void PerformStartup()
    {
        pcfStatus = PCFStatus.Active;
        if (_pcfStatusText != null) {
            _pcfStatusText.text = "Status: Restoring Content";
        }

        if (OnPcfsReady != null)
        {
            OnPcfsReady(this);
        }

        if (_pcfStatusText != null) {
            _pcfStatusText.text = "Done Restoring Content";
        }
    }

    void ShowError(MLResult result)
    {
        Debug.LogErrorFormat("Error: {0}", result);
        if (_pcfStatusText != null) {
            _pcfStatusText.text = string.Format("<color=red>Error: {0}</color>", result);
        }
    }
#endif
}