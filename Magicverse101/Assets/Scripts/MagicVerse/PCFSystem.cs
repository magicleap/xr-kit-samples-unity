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
using UnityEngine.XR.MagicLeap.Native;
#elif PLATFORM_IOS || PLATFORM_ANDROID
using MagicLeap.XR.XRKit;
#endif

// PCFSystem does all the work of subscribing to XRAnchors (iOS/Android) and PCFs (Lumin)

// NOTE: This code requires the modified version of Transmission in this project. 
// This can be easily modified by removing the delegate setter. Then it can be used with any other networking API.

public class PCFSystem : MonoBehaviour
{
#if PLATFORM_IOS || PLATFORM_ANDROID
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

        // NOTE: this uses a modified version of MLTK Transmission. 
        // PCFSystem sets itself as the delegate for Transmission to query for a PCFID given a string
        Transmission.Instance.SetPCFPoseDelegate(PoseForPCFID);
    }

    void Start()
    {
        // parent game object for all PCF visuals
        visualParent = new GameObject("Anchors");
        visualParent.transform.parent = transform;

#if PLATFORM_LUMIN
        StartPCFS();

        // setup handlers for pcf status updates
        MLPersistentCoordinateFrames.PCF.OnStatusChange += OnStatusChange;

#elif PLATFORM_IOS || PLATFORM_ANDROID
        if (MLXRSessionInstance == null)
        {
            Debug.LogError("Don't have a reference to an MLXRSessionInstance.");
        }
        // Register for the Anchor callbacks
        MLXRSessionInstance.anchorsChanged += HandleAnchorsChanged;
#endif
        
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

    // A unified method for anyone to get a Pose given a pcfid string. 
    Func<Vector3, Quaternion, Pose> returnPose = (p,r) => { return new Pose(p, r); };
    public void PoseForPCFID(string pcfId, Action<bool, Pose> poseHandler)
    {                            
        // Find PCF
        Debug.Log($"PoseForPCFID with PCFID: {pcfId}");

#if PLATFORM_LUMIN
        
        if (MLPersistentCoordinateFrames.FindPCFByCFUID(PCFUIDFromString(pcfId), out MLPersistentCoordinateFrames.PCF pcf).IsOk)
        {
            Pose pose  = returnPose(pcf.Position, pcf.Rotation);
            Debug.Log($"PoseForPCFID callback PCF: {pcf} Returning Pose: {pose}");
            poseHandler(true, pose);
        } else {
            Debug.Log("PoseForPCFID Error: No Matching Pcf found");
            poseHandler(false, returnPose(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0)));
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
        if (!displayDebugVisuals)
            return;

#if PLATFORM_IOS || PLATFORM_ANDROID
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

    #if PLATFORM_IOS || PLATFORM_ANDROID
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Localization Status: {0}\n", MLXRSessionInstance.GetLocalizationStatus());

        // Once we've localized one time, we want to start printing # of Anchors
        if (!LoggedLocalizedOnce)
        {
            if (MLXRSessionInstance.IsLocalized())
            {
                LoggedLocalizedOnce = true;
            }
        }
        if (LoggedLocalizedOnce)
        {
            sb.AppendFormat("Number of Anchors Found: {0}\n", numAnchors);
        }

        _pcfStatusText.text = sb.ToString();
    #endif

    }


#if PLATFORM_IOS || PLATFORM_ANDROID

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
            if (result.Result == MLResult.Code.PrivilegeDenied)
            {
                Instantiate(Resources.Load("PrivilegeDeniedError"));
            }

            Debug.LogErrorFormat("Error: PersistenceExample failed starting MLPersistentCoordinateFrames, disabling script. Reason: {0}", result);
            enabled = false;
            return;
        }

        pcfsStarted = true;
        if (MLPersistentCoordinateFrames.IsLocalized)
        {
            // Success of PCF systems. Handle startup
            PerformStartup();
        }
        else
        {
            // Wait for initialization callback to check for PCF status
            MLPersistentCoordinateFrames.OnLocalized += HandleInitialized;
        }
    }

    void HandleInitialized(bool localized)
    {
        MLPersistentCoordinateFrames.OnLocalized -= HandleInitialized;

        if (localized)
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
                _pcfStatusText.text = string.Format("<color=red> Not yet Localized </color>");
            }
            Debug.LogErrorFormat("Error: MLPersistentCoordinateFrames failed to localize, trying again.");
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

        //trigger updates
        if (displayDebugVisuals) {
            MLPersistentCoordinateFrames.FindAllPCFs(out List<MLPersistentCoordinateFrames.PCF> list, int.MaxValue, MLPersistentCoordinateFrames.PCF.Types.MultiUserMultiSession);
        }

        if (_pcfStatusText != null) {
            _pcfStatusText.text = "Done Restoring Content";
        }
    }

    private void OnStatusChange(MLPersistentCoordinateFrames.PCF.Status pcfStatus, MLPersistentCoordinateFrames.PCF pcf) {
        if (displayDebugVisuals) {
            if (pcfStatus == MLPersistentCoordinateFrames.PCF.Status.Created){
                PCFAnchorVisual newVisual = Instantiate(AnchorVisualPrefab, pcf.Position, pcf.Rotation);
                newVisual.transform.parent = visualParent.transform;
                newVisual.GetComponent<PCFAnchorVisual>().PCF = pcf;
            }

            if (pcfStatus == MLPersistentCoordinateFrames.PCF.Status.Lost){
                foreach (var visual in visualParent.GetComponentsInChildren<PCFAnchorVisual>()){
                    if (visual.PCF.Equals(pcf)){
                        DestroyImmediate(visual.gameObject);
                    }
                }
            }

            if (pcfStatus == MLPersistentCoordinateFrames.PCF.Status.Updated){
                foreach (var visual in visualParent.GetComponentsInChildren<PCFAnchorVisual>()){
                    if (visual.PCF.Equals(pcf)){
                        visual.gameObject.transform.position = pcf.Position;
                        visual.gameObject.transform.rotation = pcf.Rotation;
                        Debug.LogFormat("PCF Updated anchor: was {0}, is now {1}", visual.PCF.ToString(), pcf.ToString());
                        visual.PCF = pcf;
                    } 
                }
            }
        }
    }

    void ShowError(MLResult result)
    {
        Debug.LogErrorFormat("Error: {0}", result);
        if (_pcfStatusText != null) {
            _pcfStatusText.text = string.Format("<color=red>Error: {0}</color>", result);
        }
    }

    // -- Utility methods to convert between MLCoordinateFrameUID and a strings. Will be replaced in future SDK updates with API -- //

     /// <summary>
    /// Returns the GUID based on the values of this MLCoordinateFrameUID.
    /// </summary>
    /// <returns>The calculated GUID.</returns>
    public static Guid PCFUIDToGuid(MagicLeapNativeBindings.MLCoordinateFrameUID frameUID)
    {
        byte[] high = BitConverter.GetBytes(frameUID.First);
        byte[] low = BitConverter.GetBytes(frameUID.Second);
        FlipGuidComponents(high);
        ulong newFirst = BitConverter.ToUInt64(high, 0);
        return new Guid((int)(newFirst >> 32 & 0x00000000FFFFFFFF), (short)(newFirst >> 16 & 0x000000000000FFFF), (short)(newFirst & 0x000000000000FFFF), low);
    }
    /// <summary>
    /// Flips a component of the GUID based on <c>endianness</c>.
    /// </summary>
    /// <param name="bytes">The array of bytes to reverse.</param>
    private static void FlipGuidComponents(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
    }
    public static MagicLeapNativeBindings.MLCoordinateFrameUID PCFUIDFromString(string s)
    {
        Guid guid = Guid.Parse(s);
        string guidString = guid.ToString("N");
        ulong flippedFirst = ulong.Parse(guidString.Substring(0, 16), System.Globalization.NumberStyles.HexNumber);
        ulong flippedSecond = ulong.Parse(guidString.Substring(16, 16), System.Globalization.NumberStyles.HexNumber);
        byte[] bytes = BitConverter.GetBytes(flippedFirst);
        FlipGuidComponents(bytes);
        ulong first = BitConverter.ToUInt64(bytes, 0);
        bytes = BitConverter.GetBytes(flippedSecond);
        FlipGuidComponents(bytes);
        ulong second = BitConverter.ToUInt64(bytes, 0);
        MagicLeapNativeBindings.MLCoordinateFrameUID cfuid =  new MagicLeapNativeBindings.MLCoordinateFrameUID();
        cfuid.First = first;
        cfuid.Second = second;
        return cfuid;
    }

#endif

}