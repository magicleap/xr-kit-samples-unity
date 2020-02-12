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
    public enum PCFStatus
    {
        Unavailable,
        PrivilegesDeclined,
        Active
    }
    PCFStatus pcfStatus = PCFStatus.Unavailable;
    public Text _pcfStatusText;
    bool pcfsStarted = false;

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

    Func<Vector3, Quaternion, Pose> returnPose = (p,r) => { return new Pose(p, r); };
    public Pose PoseForPCFID(string pcfId)
    {
        Pose pose = new Pose(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));;
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
                            pose = returnPose(posedPCF.Position, posedPCF.Orientation);
                        }
                    });
                }
            }
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (MagicversePcfManager.PcfPoseLookup.ContainsKey(pcfId))
        {
            MagicversePcfManager.PcfPoseData poseData = MagicversePcfManager.PcfPoseLookup[pcfId];
            pose = returnPose(poseData.position, poseData.rotation);

        } else
        {
            Debug.Log("PoseForPCFID Error: No Matching Pcf found");
        }
#endif
        Debug.Log($"PoseForPCFID Returning Pose: {pose}");

        return pose;
    }


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