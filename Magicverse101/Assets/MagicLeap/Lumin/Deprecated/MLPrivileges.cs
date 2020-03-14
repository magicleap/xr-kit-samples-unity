// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLPrivileges.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap
{
    using System;

    /// <summary>
    /// Functionality to validate or query privileges from the system.
    /// </summary>
    public sealed partial class MLPrivileges : MLAPISingleton<MLPrivileges>
    {
        [System.Obsolete("Please use MLPrivileges.RequestPrivilege with MLPrivileges.Id instead.", true)]
        public static MLResult RequestPrivilege(MLPrivilegeId privilegeId)
        {
            return MLResult.Create(MLResult.Code.UnspecifiedFailure);
        }

        [System.Obsolete("Please use MLPrivileges.CheckPrivilege with MLPrivileges.Id instead.", true)]
        public static MLResult CheckPrivilege(MLPrivilegeId privilegeId)
        {
            return MLResult.Create(MLResult.Code.UnspecifiedFailure);
        }

        [System.Obsolete("Please use MLPrivileges.RequestPrivilegeAsync with MLPrivileges.Id instead.", true)]
        public static MLResult RequestPrivilegeAsync(MLPrivilegeId privilegeId, CallbackDelegate callback)
        {
            return MLResult.Create(MLResult.Code.UnspecifiedFailure);
        }

        /// <summary>
        /// Gets a readable version of the result code as an ASCII string.
        /// </summary>
        /// <param name="result">The MLResult that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        [Obsolete("Please use MLResult.CodeToString(MLResult.Code) instead.", true)]
        public static string GetResultString(MLResultCode result)
        {
            return "This function is deprecated. Use MLResult.CodeToString(MLResult.Code) instead.";
        }
    }
}

#endif
