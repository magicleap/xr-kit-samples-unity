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

#if PLATFORM_LUMIN
namespace MagicLeap.Core.StarterKit
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR.MagicLeap;

    /// <summary>
    /// Starterkit class for practical use of MLFoundObjects.
    /// </summary>
    public static class MLFoundObjectsStarterKit
    {
        /// <summary>
        /// Use this to determine when you are allowed to Query for found objects again.
        /// </summary>
        public static bool IsQuerying
        {
            get;
            private set;
        }

        /// <summary>
        /// Used to set IsQuerying to false.
        /// </summary>
        private static MLFoundObjects.QueryResultsDelegate queryCallback = (MLFoundObjects.FoundObject foundObject, List<KeyValuePair<string, string>> properties) => { IsQuerying = false; };

        private static MLResult result;

        /// <summary>
        /// Starts up MLFoundObjects.
        /// </summary>
        public static MLResult Start()
        {
            #if PLATFORM_LUMIN
            result = MLPrivilegesStarterKit.Start();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLFoundObjectsStarterKit failed when calling MLPrivilegesStarterKit.Start. Reason: {0}", result);
                return result;
            }

            result = MLPrivilegesStarterKit.RequestPrivileges(MLPrivileges.Id.ObjectData);
            if (result.Result != MLResult.Code.PrivilegeGranted)
            {
                Debug.LogErrorFormat("Error: MLFoundObjectsStarterKit failed requesting privileges. Reason: {0}", result);
                return result;
            }
            MLPrivilegesStarterKit.Stop();

            result = MLFoundObjects.Start();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLFoundObjectsStarterKit failed starting MLFoundObjects. Reason: {0}", result);
            }
            return result;
            #endif
        }

        /// <summary>
        /// Stops MLFoundObjects if it has been started.
        /// </summary>
        public static void Stop()
        {
            if (MLFoundObjects.IsStarted)
            {
                MLFoundObjects.Stop();
            }
        }

        /// <summary>
        /// Function used to query for found objects present in the real world.
        /// </summary>
        /// <param name="parameters">The parameters to use for this query.</param>
        /// <param name="callback">The function to call when the query is done.</param>
        public static MLResult QueryFoundObjects(MLFoundObjects.QueryResultsDelegate callback)
        {
            if (MLFoundObjects.IsStarted)
            {
                if (IsQuerying)
                {
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "A previous query is still in progress.");
                }

                callback += queryCallback;
                result = MLFoundObjects.GetObjects(callback);
                IsQuerying = result.IsOk;

                if (!result.IsOk)
                {
                    callback = null;
                    Debug.LogErrorFormat("Error: MLFoundObjectsStarterKit.QueryFoundObjects failed. Reason: {0}", result);
                }
            }

            else
            {
                Debug.LogError("Error: MLFoundObjectsStarterKit.QueryFoundObjects failed because MLFoundObjects was not started.");
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLFoundObjects was not started");
            }

            return result;
        }
    }
}
#endif
