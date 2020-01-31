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
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MagicLeap.XR.XRKit.Sample
{
    public class MLXRSessionDebugger : MonoBehaviour
    {
        public MLXRSession MLXRSessionInstance;
        public GameObject AnchorVisual;
        public float AnchorVisualUpdateRate = 0.1f;

        public Text debugText;
        public Camera cam;

        private bool LoggedLocalizedOnce = false;

        private GameObject childContainer;
        private int numAnchors = 0;

        private struct AnchorObject
        {
            public MLXRAnchor anchor;
            public GameObject gameObject;
        }

        private Dictionary<MagicLeap.XR.XRKit.MLXRApi.MLXrPCFId, AnchorObject> anchorGameObjects
            = new Dictionary<MagicLeap.XR.XRKit.MLXRApi.MLXrPCFId, AnchorObject>();

        // Start is called before the first frame update
        void Start()
        {
            if (MLXRSessionInstance == null)
            {
                Debug.LogError("Don't have a reference to an MLXRSessionInstance.");
            }
            // Register for the Anchor callbacks
            MLXRSessionInstance.anchorsChanged += HandleAnchorsChanged;

            childContainer = new GameObject("Anchors");
            childContainer.transform.parent = transform;
        }

        // Update is called once per frame
        void Update()
        {
            // Don't attempt to do anything, unless the MLXRSession has started
            if (!MLXRSessionInstance.gameObject.activeSelf)
            {
                return;
            }

            // For each existing Anchor Visual, billboard the ID to the current Headpose
            foreach (Transform child in childContainer.transform)
            {
                TextMesh tm = child.gameObject.GetComponentInChildren<TextMesh>();
                if (!tm)
                {
                    continue;
                }

                tm.transform.LookAt(tm.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
            }

            // All direct children of the child container will represent a single Anchor
            numAnchors = childContainer.transform.childCount;


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

            debugText.text = sb.ToString();
        }

        private static string MakeAnchorString(MLXRAnchor anchor)
        {
            return $"ID: {anchor.id}\n"
                + $"pose: {anchor.pose}\n"
                + $"confidence: {anchor.confidence.confidence}\n"
                + $"rotation error: {anchor.confidence.rotation_err_deg} degrees\n"
                + $"translation error: {anchor.confidence.translation_err_m} meters\n"
                + $"valid radius: {anchor.confidence.valid_radius_m} meters";
        }

        public void HandleAnchorsChanged(MLXRSession.AnchorsUpdatedEventArgs e)
        {
            foreach (MLXRAnchor anchor in e.added)
            {
                // Create a new GameObject, and insert it to the PCFId -> GameObject map
                Pose pose = anchor.pose;
                GameObject newVisual = Instantiate(AnchorVisual, pose.position, pose.rotation);
                newVisual.transform.parent = childContainer.transform;
                TextMesh tm = newVisual.GetComponentInChildren<TextMesh>();
                string anchorText = MakeAnchorString(anchor);
                tm.text = anchorText;

                anchorGameObjects.Add(anchor.id, new AnchorObject { anchor = anchor, gameObject = newVisual });

                Debug.LogFormat("Added anchor: {0}", anchorText);
            }

            foreach (MLXRAnchor anchor in e.removed)
            {
                // Look up the GameObject in the map, and remove it
                if (anchorGameObjects.TryGetValue(anchor.id, out AnchorObject anchorObject))
                {
                    DestroyImmediate(anchorObject.gameObject);
                    anchorGameObjects.Remove(anchor.id);
                }
                else
                {
                    Debug.LogWarningFormat("Anchor ID {0} removed, but no visual was found.", anchor.id);
                    continue;
                }
            }

            foreach (MLXRAnchor anchor in e.updated)
            {
                // Look up the GameObject in the map, and reset the pose!
                if (anchorGameObjects.TryGetValue(anchor.id, out AnchorObject anchorObject))
                {
                    Pose pose = anchor.pose;
                    anchorObject.gameObject.transform.position = pose.position;
                    anchorObject.gameObject.transform.rotation = pose.rotation;
                    TextMesh tm = anchorObject.gameObject.GetComponentInChildren<TextMesh>();
                    string anchorText = MakeAnchorString(anchor);
                    tm.text = anchorText;

                    Debug.LogFormat("Updated anchor: was {0}, is now {1}", MakeAnchorString(anchorObject.anchor), anchorText);

                    anchorObject.anchor = anchor;
                    anchorGameObjects[anchor.id] = anchorObject;
                }
                else
                {
                    Debug.LogWarningFormat("Anchor ID {0} updated, but no visual was found.", anchor.id);
                }
            }
        }
    }
}