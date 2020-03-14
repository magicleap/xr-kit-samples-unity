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
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Core.StarterKit
{
    /// <summary>
    /// Starter kit class made for practical use of MLPrivileges.
    /// </summary>
    public static class MLPrivilegesStarterKit
    {
        #pragma warning disable 649
        private static MLResult _result;
        #pragma warning restore 649

        /// <summary>
        /// Starts up MLPrivileges.
        /// </summary>
        public static MLResult Start()
        {
            #if PLATFORM_LUMIN
            _result = MLPrivileges.Start();

            if (!_result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLPrivilegesStarterKit failed starting MLPrivileges. Reason: {0}", _result);
            }
            #endif

            return _result;
        }

        /// <summary>
        /// Stops MLPrivileges.
        /// </summary>
        public static void Stop()
        {
            #if PLATFORM_LUMIN
            if (MLPrivileges.IsStarted)
            {
                MLPrivileges.Stop();
            }
            #endif
        }

        /// <summary>
        /// Request privileges.
        /// </summary>
        /// <param name="privileges">An array of privileges to request.</param>
        public static MLResult RequestPrivileges(params MLPrivileges.Id[] privileges)
        {
            #if PLATFORM_LUMIN
            if (MLPrivileges.IsStarted)
            {
                foreach (MLPrivileges.Id privilege in privileges)
                {
                    _result = CheckPrivilege(privilege);
                    if (_result.Result == MLResult.Code.PrivilegeGranted)
                    {
                        continue;
                    }

                    _result = MLPrivileges.RequestPrivilege(privilege);
                    if (_result.Result != MLResult.Code.PrivilegeGranted)
                    {
                        return _result;
                    }
                }
            }
            else
            {
                Debug.LogError("Error: MLPrivilegesStarterKit.RequestPrivileges failed because MLPrivileges was not started.");
                _result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPrivileges was not started");
            }
            #endif

            return _result;
        }

        /// <summary>
        /// Request privileges asynchronously.
        /// </summary>
        ////<param name="callback">The method to call back once a result is known. It will be called when all privileges are granted or if requesting all privileges fails.</param>
        /// <param name="privileges">An array of privileges to request.</param>
        public static MLResult RequestPrivilegesAsync(Action<MLResult> callback, params MLPrivileges.Id[] privileges)
        {
            #if PLATFORM_LUMIN
            if (MLPrivileges.IsStarted)
            {
                int numPrivilegesToRequest = privileges.Length;

                for(int i = 0; i < privileges.Length; i++)
                {
                    MLPrivileges.Id privilege = privileges[i];

                    _result = CheckPrivilege(privilege);
                    if (_result.Result == MLResult.Code.PrivilegeGranted)
                    {
                        numPrivilegesToRequest--;
                        if(numPrivilegesToRequest == 0)
                        {
                            callback?.Invoke(_result);
                        }
                        continue;
                    }

                    _result = MLPrivileges.RequestPrivilegeAsync(privilege, (MLResult result, MLPrivileges.Id priv) =>
                    {
                        numPrivilegesToRequest--;

                        if (result.Result == MLResult.Code.PrivilegeGranted)
                        {
                            if (numPrivilegesToRequest == 0)
                            {
                                callback?.Invoke(result);
                            }
                        }

                        // Privilege was not granted
                        else
                        {
                            numPrivilegesToRequest = 0;
                            if (numPrivilegesToRequest == 0)
                            {
                                callback?.Invoke(result);
                            }
                        }
                    });

                    if (!_result.IsOk)
                    {
                        return _result;
                    }
                }
            }
            else
            {
                Debug.LogError("Error: MLPrivilegesStarterKit.RequestPrivilegesAsync failed because MLPrivileges was not started.");
                _result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPrivileges was not started");
            }

            // Override result in case privilege was already granted.
            if(_result.Result == MLResult.Code.PrivilegeGranted)
            {
                _result = MLResult.Create(MLResult.Code.Ok);
            }
            #endif

            return _result;
        }

        /// <summary>
        /// Used to check if your privilege has already been granted.
        /// </summary>
        /// <param name="privilege">The privilege to check for.</param>
        public static MLResult CheckPrivilege(MLPrivileges.Id privilege)
        {
            #if PLATFORM_LUMIN
            if (MLPrivileges.IsStarted)
            {
                _result = MLPrivileges.CheckPrivilege(privilege);

                if (_result.Result != MLResult.Code.PrivilegeGranted && _result.Result != MLResult.Code.PrivilegeNotGranted)
                {
                    Debug.LogErrorFormat("Error: MLPrivilegesStarterKit.CheckPrivilege failed for the privilege {0}. Reason: {1}", privilege, _result);
                }
            }

            else
            {
                Debug.LogError("Error: MLPrivilegesStarterKit.CheckPrivilege failed because MLPrivileges was not started.");
                _result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPrivileges was not started");
            }
            #endif

            return _result;
        }
    }
}
