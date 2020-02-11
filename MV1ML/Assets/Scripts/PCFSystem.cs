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
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
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
    private static List<MLPCF> _localPCFs = new List<MLPCF>();
    private List<MLPCF> _localPCFData = new List<MLPCF>();

    void Awake()
    {
        if (_pcfStatusText != null)
        {
            _pcfStatusText.text = "Status: Requesting Privileges";
        }
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
