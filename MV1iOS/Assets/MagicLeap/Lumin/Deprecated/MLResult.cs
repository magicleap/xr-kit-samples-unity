// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLResult.cs" company="Magic Leap, Inc">
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
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// Magic Leap API return value.
    /// </summary>
    public partial struct MLResult
    {
        /// <summary>
        /// Gets a new MLResult with an Ok result.
        /// </summary>
        [Obsolete("Please use MLResult.Create instead.", false)]
        public static readonly MLResult ResultOk = MLResult.Create(MLResult.Code.Ok);

        /// <summary>
        /// Initializes a new instance of the <see cref="MLResult" /> struct.
        /// </summary>
        /// <param name="resultCode">The result code to give this MLResult.</param>
        /// <param name="resultStringer">Function used to generate result message.</param>
        [Obsolete("Please use MLResult.Create instead. Note the previous MLResult.Code has been replaced with MLResult.Result.", true)]
        public MLResult(MLResultCode resultCode, Func<MLResultCode, string> resultStringer)
        {
            if (resultStringer == null)
            {
                throw new ArgumentException("Result stringer parameter has invalid null value");
            }

            this.message = null;
            this.Result = Code.NotImplemented;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MLResult" /> struct.
        /// </summary> >
        /// <param name="resultCode">The result code to give this MLResult.</param>
        /// <param name="msg">The message to give this MLResult.</param>
        [Obsolete("Please use MLResult.Create instead. Note the previous MLResult.Code has been replaced with MLResult.Result.", true)]
        public MLResult(MLResultCode resultCode, string msg)
        {
            this.message = msg;
            this.Result = Code.NotImplemented;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MLResult" /> struct.
        /// </summary>
        /// <param name="resultCode">The result code to give this MLResult.</param>
        [Obsolete("Please use MLResult.Create instead. Note the previous MLResult.Code has been replaced with MLResult.Result.", true)]
        public MLResult(MLResultCode resultCode)
        {
            this.message = MagicLeapNativeBindings.MLGetResultString(resultCode);
            this.Result = Code.NotImplemented;
        }
    }
}

#endif
