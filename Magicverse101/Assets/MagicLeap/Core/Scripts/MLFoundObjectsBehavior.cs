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

using System.Collections.Generic;
using UnityEngine;

#if PLATFORM_LUMIN
using MagicLeap.Core.StarterKit;
using UnityEngine.XR.MagicLeap;
#endif

namespace MagicLeap.Core
{
    /// <summary>
    /// Starts the MLFoundObjectsToolkit and queries for found objects.
    /// </summary>
    public class MLFoundObjectsBehavior : MonoBehaviour
    {
        #pragma warning disable 414
        /// <summary>
        /// When enabled will continuously query for new objects.
        /// </summary>
        [SerializeField, Tooltip("When enabled this behaviour will continuously query for new objects.")]
        private bool _autoQuery = true;
        #pragma warning restore 414

        public delegate void QueryFoundObjectsResult(System.Guid id, Vector3 position, Quaternion rotation, Vector3 extents, List<KeyValuePair<string, string>> properties);

        /// <summary>
        /// Event for when a query has completed.
        /// </summary>
        public event QueryFoundObjectsResult OnQueryFoundObjectsResult = delegate { };

        /// <summary>
        /// Starts up MLFoundObjectsToolkit.
        /// </summary>
        void Start()
        {
            #if PLATFORM_LUMIN
            MLFoundObjectsStarterKit.Start();
            #endif
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        void OnDestroy()
        {
            #if PLATFORM_LUMIN
            MLFoundObjectsStarterKit.Stop();
            #endif
        }

        /// <summary>
        /// Obtains the latest found object query.
        /// </summary>
        void Update()
        {
            #if PLATFORM_LUMIN
            if (_autoQuery && !MLFoundObjectsStarterKit.IsQuerying)
            {
                QueryFoundObjects();
            }
            #endif
        }

        /// <summary>
        /// Requests a new query if one is not currently active.
        /// </summary>
        /// <returns>Will return true if the request was successful.</returns>
        public bool QueryFoundObjects()
        {
            #if PLATFORM_LUMIN
            if (MLFoundObjects.IsStarted)
            {
                MLFoundObjects.GetObjects(HandleOnFoundObject);
                return true;
            }
            #endif

            return false;
        }

        #if PLATFORM_LUMIN
        private void HandleOnFoundObject(MLFoundObjects.FoundObject foundObject, List<KeyValuePair<string, string>> properties)
        {
            OnQueryFoundObjectsResult?.Invoke(foundObject.Id, foundObject.Position, foundObject.Rotation, foundObject.Size, properties);
        }
        #endif
    }
}
