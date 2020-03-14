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

using UnityEngine;
using UnityEngine.UI;

namespace MagicLeap
{
    [RequireComponent(typeof(PlaceFromCamera))]
    public class UserInterface : MonoBehaviour
    {
        private const float SIDE_MENU_DEFAULT_WIDTH = 175;
        private const float SIDE_MENU_MAX_WIDTH = 475;

        [Header("Settings")]
        [SerializeField, Tooltip("The default and closest distance for the canvas.")]
        private float _minDistance = 1.0f;

        [SerializeField, Tooltip("The furthest distance for the canvas.")]
        private float _maxDistance = 1.5f;

        [SerializeField, Tooltip("The primary workspace, this area will be collapsed in the minimized view.")]
        private GameObject _workspace = null;

        [SerializeField, Tooltip("The button that maintains the canvas lock for the interface.")]
        private UIButton _lockButton = null;

        [SerializeField, Tooltip("When enabled, the interface text will be replaced with a localized version.")]
        private bool _useLocalization = true;

        [Header("Interface")]
        [SerializeField, Tooltip("The transform of the side menu.")]
        private RectTransform _sideMenu = null;

        [Header("Button & Text Fields")]

        [SerializeField, Tooltip("The title text element.")]
        private Text _title = null;

        [SerializeField, Tooltip("The UIButton for the overview tab.")]
        private UIButton _overviewTab = null;

        [SerializeField, Tooltip("The overview text element.")]
        private Text _overviewText = null;

        [SerializeField, Tooltip("The UIButton for the controls tab.")]
        private UIButton _controlsTab = null;

        [SerializeField, Tooltip("The controls text element.")]
        private Text _controlsText = null;

        [SerializeField, Tooltip("The UIButton for the status tab.")]
        private UIButton _statusTab = null;

        [SerializeField, Tooltip("The status text element.")]
        private Text _statusText = null;

        [SerializeField, Tooltip("The UIButton for the issues tab.")]
        private UIButton _issuesTab = null;

        [SerializeField, Tooltip("The container for issue related text elements.")]
        private GameObject _issuesContent = null;

        [SerializeField, Tooltip("The text entry prefab used for multiple line entries.")]
        private GameObject _textEntryPrefab = null;

        private PlaceFromCamera _placeFromCamera = null;
        private float _canvasDistance = 0f;

        /// <summary>
        /// Sets the overview text directly, does not use localization.
        /// </summary>
        public string OverviewText
        {
            set
            {
                _overviewText.text = value;
            }
        }

        /// <summary>
        /// Sets the controls text directly, does not use localization.
        /// </summary>
        public string ControlsText
        {
            set
            {
                _controlsText.text = value;
            }
        }

        /// <summary>
        /// Sets the status text directly, does not use localization.
        /// </summary>
        public string StatusText
        {
            set
            {
                _statusText.text = value;
            }
        }

        /// <summary>
        /// Sets the issues text directly, does not use localization.
        /// </summary>
        [System.Obsolete("This property has been replaced with the method AddIssue().", true)]
        public string IssuesText
        {
            get; set;
        }

        private void Awake()
        {
            // Canvas Initialization
            _placeFromCamera = GetComponent<PlaceFromCamera>();

            _canvasDistance = _minDistance;
            _placeFromCamera.Distance = _canvasDistance;

            if (_useLocalization)
            {
                // Title
                _title.text = LocalizeManager.GetString(_title.text);

                // Tabs
                Text overviewTabText = _overviewTab.GetComponentInChildren<Text>(true);
                if (overviewTabText != null)
                {
                    overviewTabText.text = LocalizeManager.GetString(overviewTabText.text);
                }

                Text controlsTabText = _controlsTab.GetComponentInChildren<Text>(true);
                if (controlsTabText != null)
                {
                    controlsTabText.text = LocalizeManager.GetString(controlsTabText.text);
                }

                Text _issueTabText = _issuesTab.GetComponentInChildren<Text>(true);
                if (_issueTabText != null)
                {
                    _issueTabText.text = LocalizeManager.GetString(_issueTabText.text);
                }

                Text _statusTabText = _statusTab.GetComponentInChildren<Text>(true);
                if (_statusTabText != null)
                {
                    _statusTabText.text = LocalizeManager.GetString(_statusTabText.text);
                }

                // Descriptions - Load From File
                _overviewText.text = LocalizeManager.GetString(_overviewText.text);
                _controlsText.text = LocalizeManager.GetString(_controlsText.text);
                _statusText.text = LocalizeManager.GetString(_statusText.text);
            }

            Application.logMessageReceived += HandleOnLogMessageReceived;

            // Open the these two tabs by default.
            _overviewTab.Pressed();
            _statusTab.Pressed();
        }

        /// <summary>
        /// Adds a new entry into the UI issues section.
        /// </summary>
        /// <param name="text">The text of the issue to add.</param>
        public void AddIssue(string text)
        {
            if(_issuesContent != null && _textEntryPrefab != null)
            {
                GameObject textEntry = Instantiate(_textEntryPrefab, _issuesContent.transform, false);
                textEntry.GetComponent<Text>().text = text;
            }
        }

        /// <summary>
        /// Clears any existing issue entries.
        /// </summary>
        public void ClearIssues()
        {
            if(_issuesContent != null)
            {
                Text[] entries = _issuesContent.GetComponentsInChildren<Text>();
                for(int i = 0; i < entries.Length; i++)
                {
                    Destroy(entries[i].gameObject);
                }
            }
        }

        /// <summary>
        /// Toggle the lock state of the canvas.
        /// </summary>
        public void ToggleCanvasLock()
        {
            _placeFromCamera.PlaceOnUpdate = !_placeFromCamera.PlaceOnUpdate;
        }

        /// <summary>
        /// Toggle the canvas distance between the min and max distance.
        /// </summary>
        public void ToggleCanvasDistance()
        {
            if(_lockButton.IsActive)
            {
                return;
            }

            _canvasDistance = (_canvasDistance == _minDistance) ? _maxDistance : _minDistance;
            _placeFromCamera.Distance = _canvasDistance;

            _placeFromCamera.ForceUpdate();
        }

        /// <summary>
        /// Toggle the visibility of the workspace.
        /// </summary>
        public void ToggleCanvas()
        {
            ShowCanvas(!_workspace.activeInHierarchy);
        }

        /// <summary>
        /// Set the visibility of the workspace.
        /// </summary>
        /// <param name="visible">The desired visible state of the workspace.</param>
        public void ShowCanvas(bool visible)
        {
            _workspace.SetActive(visible);

            // Adjust the width of the side menu, this allows it to shift left/right.
            _sideMenu.sizeDelta = new Vector2((_workspace.activeInHierarchy) ? SIDE_MENU_DEFAULT_WIDTH : SIDE_MENU_MAX_WIDTH, _sideMenu.sizeDelta.y);
        }

        public void HandleOnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error)
            {
                AddIssue(FormatText(condition));

                // Only show the issues button, if an error is reported.
                StartCoroutine(SendErrorNotifications());
            }
        }

        private string FormatText(string text)
        {
            if (text.Contains("Error:"))
            {
                text = text.Replace("Error:", string.Format("<color=#{0}><b>Error:</b> </color><i>", ColorUtility.ToHtmlStringRGB(Color.red))) + "</i>";
            }
            else
            {
                text = string.Format("<color=#{0}><b>Error:</b> </color><i>", ColorUtility.ToHtmlStringRGB(Color.red)) + text + "</i>";
            }

            return text;
        }

        private IEnumerator SendErrorNotifications()
        {
            yield return new WaitForEndOfFrame();

            _issuesTab.ForceActive();
        }
    }
}
