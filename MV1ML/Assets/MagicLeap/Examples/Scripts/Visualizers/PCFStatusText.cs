// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// Utility class to help visual the status of the PCF
    /// </summary>
    public class PCFStatusText : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("Text to display status")]
        private TextMesh _statusText = null;

        [SerializeField, Tooltip("Text to display name")]
        private TextMesh _nameText = null;

        private MLPCF _pcf = null;
        #endregion

        #region Public Properties
        /// <summary>
        /// Sets up PCF event handlers
        /// </summary>
        public MLPCF PCF
        {
            set
            {
                UnregisterPCFEventHandlers(_pcf);
                _pcf = value;
                RegisterPCFEventHandlers(_pcf);
                _statusText.text = "Good";
            }
        }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Start Up
        /// </summary>
        private void Start()
        {
            if (_statusText == null)
            {
                Debug.LogError("Error: PCFStatusText._statusText is not set, disabling script");
                enabled = false;
                return;
            }

            if (_nameText == null)
            {
                Debug.LogError("Error: PCFStatusText._nameText is not set, disabling script");
                enabled = false;
                return;
            }

            _nameText.text = gameObject.name;
        }

        /// <summary>
        /// Clean Up
        /// </summary>
        void OnDestroy()
        {
            UnregisterPCFEventHandlers(_pcf);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handler when PCF is lost
        /// </summary>
        void HandleLost()
        {
            CancelInvoke("QueuePCFForUpdates");
            Invoke("QueuePCFForUpdates", 1);
            _statusText.text = "<color=red>Lost</color>";
        }

        /// <summary>
        /// Handler when PCF regains
        /// </summary>
        void HandleRegain()
        {
            _statusText.text = "<color=cyan>Regained</color>\n" + GetPCFStateString();
            transform.position = _pcf.Position;
            transform.rotation = _pcf.Orientation;
        }

        /// <summary>
        /// Handler when PCF updates
        /// </summary>
        void HandleUpdate()
        {
            _statusText.text = "<color=yellow>Updated</color>\n" + GetPCFStateString();
            transform.position = _pcf.Position;
            transform.rotation = _pcf.Orientation;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Queues pcf for updates from MLPersistentCoordinateFrames
        /// </summary>
        void QueuePCFForUpdates()
        {
            MLPersistentCoordinateFrames.QueueForUpdates(_pcf);
        }

        /// <summary>
        /// Unregister Event Handlers for given PCF
        /// </summary>
        /// <param name="pcf">PCF</param>
        void UnregisterPCFEventHandlers(MLPCF pcf)
        {
            if (pcf != null)
            {
                pcf.OnLost -= HandleLost;
                pcf.OnRegain -= HandleRegain;
                pcf.OnUpdate -= HandleUpdate;
            }
        }

        /// <summary>
        /// Register Event Handlers for given PCF
        /// </summary>
        /// <param name="pcf">PCF</param>
        void RegisterPCFEventHandlers(MLPCF pcf)
        {
            if (pcf != null)
            {
                pcf.OnLost += HandleLost;
                pcf.OnRegain += HandleRegain;
                pcf.OnUpdate += HandleUpdate;
            }
        }

        string GetPCFStateString()
        {
            return string.Format("Confidence {0}\nValidRadiusM {1}\nRotationErrDeg {2}\nTranslationErrM {3}",
                _pcf.Confidence,
                _pcf.ValidRadiusM,
                _pcf.RotationErrDeg,
                _pcf.TranslationErrM);
        }
        #endregion
    }
}
