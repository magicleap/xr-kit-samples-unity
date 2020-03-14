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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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

        public Text AnchorModeStatusText;

        public Dropdown AnchorIdTestDropdown;
        public Text AnchorIdTestResult;

        private bool LoggedLocalizedOnce = false;

        private GameObject childContainer;
        private int numAnchors = 0;

        private Coroutine pollingCoroutine = null;
        private float pollingIntervalSeconds = 1.0f;

        enum AnchorMode
        {
            Callback,
            PollingCached,
            PollingNative,
        }

        private struct AnchorObject
        {
            public MLXRAnchor anchor;
            public GameObject gameObject;
        }

        private Dictionary<MLXRApi.MLXrPCFId, AnchorObject> anchorGameObjects
            = new Dictionary<MLXRApi.MLXrPCFId, AnchorObject>();

        private AnchorMode mode = AnchorMode.Callback;

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

            if (AnchorIdTestDropdown)
            {
                AnchorIdTestDropdown.ClearOptions();
                AnchorIdTestDropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(); });
            }
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

        public void ToggleAnchorMode()
        {
            switch (mode)
            {
                case AnchorMode.Callback:
                    {
                        mode = AnchorMode.PollingCached;

                        StartPollingAnchors();

                        if (AnchorModeStatusText)
                        {
                            AnchorModeStatusText.text = "Polling Cached";
                        }
                        break;
                    }
                case AnchorMode.PollingCached:
                    {
                        mode = AnchorMode.PollingNative;

                        if (AnchorModeStatusText)
                        {
                            AnchorModeStatusText.text = "Polling Native";
                        }
                        break;
                    }
                case AnchorMode.PollingNative:
                    {
                        mode = AnchorMode.Callback;

                        StopPollingAnchors();

                        if (AnchorModeStatusText)
                        {
                            AnchorModeStatusText.text = "Callback";
                        }
                        break;
                    }
            }
        }

        private void StartPollingAnchors()
        {
            if (pollingCoroutine == null)
            {
                MLXRSessionInstance.anchorsChanged -= HandleAnchorsChanged;
                pollingCoroutine = StartCoroutine(PollAnchorsPeriodic());
            }
        }

        private void StopPollingAnchors()
        {
            if (pollingCoroutine != null)
            {
                PollAnchors();
                MLXRSessionInstance.anchorsChanged += HandleAnchorsChanged;
                StopCoroutine(pollingCoroutine);
                pollingCoroutine = null;
            }
        }

        private IEnumerator PollAnchorsPeriodic()
        {
            while (true)
            {
                PollAnchors();
                yield return new WaitForSeconds(pollingIntervalSeconds);
            }
        }

        // Poll for anchors and determine what needs to be added, updated, or removed.
        public void PollAnchors()
        {
            List<MLXRAnchor> added = new List<MLXRAnchor>();
            List<MLXRAnchor> updated = new List<MLXRAnchor>();
            List<MLXRAnchor> removed = new List<MLXRAnchor>();

            IReadOnlyDictionary<MLXRApi.MLXrPCFId, MLXRAnchor> anchors
                = mode == AnchorMode.PollingCached
                ? MLXRSessionInstance.GetAllAnchors()
                : MLXRSessionInstance.GetAllAnchorsNative();

            foreach (var anchorPair in anchors)
            {
                if (anchorGameObjects.TryGetValue(anchorPair.Key, out AnchorObject anchorObject))
                {
                    if (!anchorObject.anchor.Equals(anchorPair.Value))
                    {
                        updated.Add(anchorPair.Value);
                    }
                }
                else
                {
                    added.Add(anchorPair.Value);
                }
            }

            foreach (var anchorPair in anchorGameObjects)
            {
                if (!anchors.ContainsKey(anchorPair.Key))
                {
                    removed.Add(anchorPair.Value.anchor);
                }
            }

            MLXRSession.AnchorsUpdatedEventArgs e;
            e.added = added;
            e.updated = updated;
            e.removed = removed;
            HandleAnchorsChanged(e);
        }

        public void OnDropdownValueChanged()
        {
            if (AnchorIdTestDropdown && AnchorIdTestResult)
            {
                int index = AnchorIdTestDropdown.value;
                if (index >= 0 && index < AnchorIdTestDropdown.options.Count())
                {
                    Dropdown.OptionData optionData = AnchorIdTestDropdown.options[index];
                    MLXRApi.MLXrPCFId? id = MLXRApi.MLXrPCFId.FromString(optionData.text);

                    if (id.HasValue)
                    {
                        MLXRAnchor? anchor = MLXRSessionInstance.GetAnchorByIdNative(id.Value);

                        if (anchor.HasValue)
                        {
                            AnchorIdTestResult.text = MakeAnchorString(anchor.Value);
                        }
                    }
                }
            }
        }

        public void HandleAnchorsChanged(MLXRSession.AnchorsUpdatedEventArgs e)
        {
            List<MLXRAnchor> updated = new List<MLXRAnchor>(e.updated);

            foreach (MLXRAnchor anchor in e.added)
            {
                // The session may have restarted and we may have gotten an anchor we knew about already, so
                // add it to the updated list instead.
                if (anchorGameObjects.TryGetValue(anchor.id, out AnchorObject anchorObject))
                {
                    Debug.LogFormat("Anchor ID {0} was added, but it already had a visual; Updating instead.", anchor.id);
                    updated.Add(anchor);
                    continue;
                }

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

            foreach (MLXRAnchor anchor in updated)
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

                    Debug.LogFormat("Updated anchor:\n{0}\nNow:\n{1}", MakeAnchorString(anchorObject.anchor), anchorText);

                    anchorObject.anchor = anchor;
                    anchorGameObjects[anchor.id] = anchorObject;
                }
                else
                {
                    Debug.LogWarningFormat("Anchor ID {0} updated, but no visual was found.", anchor.id);
                }
            }

            if (AnchorIdTestDropdown)
            {
                AnchorIdTestDropdown.ClearOptions();
                AnchorIdTestDropdown.AddOptions(anchorGameObjects.Keys.Select(x => x.ToString()).ToList());
            }
        }
    }
}