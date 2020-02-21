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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Text;
using System;
using System.Linq;


namespace MagicLeap.XR.XRKit
{
    public class MagicversePcfManager : MonoBehaviour
    {
#if UNITY_IOS || UNITY_ANDROID
        public MLXRSession MLXRSessionInstance;

        public class PcfPoseData
        {
            public string pcfId;
            public Vector3 position;
            public Quaternion rotation;
        }

        public static Dictionary<string, PcfPoseData> PcfPoseLookup = new Dictionary<string, PcfPoseData>();
        public static List<KeyValuePair<string, MagicversePcfManager.PcfPoseData>> PCFListSortedByDistanceTo(Vector3 objPos){
            var pcfList = PcfPoseLookup.ToList();
            pcfList = pcfList.OrderBy(p => Vector3.Distance(objPos, p.Value.position)).ToList();
            return pcfList;
        }

        public static MagicversePcfManager Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (MLXRSessionInstance == null)
            {
                Debug.LogError("Don't have a reference to an MLXRSessionInstance.");
            }
            // Register for the Anchor callbacks
            MLXRSessionInstance.anchorsChanged += HandleAnchorsChanged;
        }

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
            }
         
            foreach (MLXRAnchor anchor in e.removed)
            {
                Debug.Log("PCF: REMOVE " + anchor.id);
                string anchorString = anchor.id.ToString();
                if (PcfPoseLookup.ContainsKey(anchorString))
                {
                    PcfPoseLookup.Remove(anchorString);
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
            }
        }
#endif
    }
}