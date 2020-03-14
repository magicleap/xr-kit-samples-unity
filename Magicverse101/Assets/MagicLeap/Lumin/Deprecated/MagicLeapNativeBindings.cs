// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MagicLeapNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%


namespace UnityEngine.XR.MagicLeap.Native
{
    using System;

    #if PLATFORM_LUMIN

    /// <summary>
    /// Defines C# API interface to C-API layer.
    /// </summary>
    public partial class MagicLeapNativeBindings
    {
        /// <summary>
        /// Returns an ASCII string for MLResultGlobal codes.
        /// </summary>
        /// <param name="result">The input MLResult enum from ML API methods.</param>
        /// <returns>ASCII string containing readable version of result code.</returns>
        [Obsolete("Please use MLResult.CodeToString(MLResult.Code) instead.", false)]
        public static string MLGetResultString(MLResultCode result)
        {
            switch (result)
            {
                // TODO: Rename to include Code in string?
                case MLResultCode.Ok:
                    { return "MLResult_Ok"; }
                case MLResultCode.Pending:
                    { return "MLResult_Pending"; }
                case MLResultCode.Timeout:
                    { return "MLResult_Timeout"; }
                case MLResultCode.Locked:
                    { return "MLResult_Locked"; }
                case MLResultCode.UnspecifiedFailure:
                    { return "MLResult_UnspecifiedFailure"; }
                case MLResultCode.InvalidParam:
                    { return "MLResult_InvalidParam"; }
                case MLResultCode.AllocFailed:
                    { return "MLResult_AllocFailed"; }
                case MLResultCode.PrivilegeDenied:
                    { return "MLResult_PrivilegeDenied"; }
                case MLResultCode.NotImplemented:
                    { return "MLResult_NotImplemented"; }
                case MLResultCode.NotCompatible:
                    { return "MLResult_NotCompatible"; }
                case MLResultCode.SnapshotPoseNotFound:
                    { return "MLResult_SnapshotPoseNotFound"; }
                default:
                    { return "MLResult_Invalid"; }
            }
        }
    }
#endif

}

