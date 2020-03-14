// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLVerbosity.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using System;

    /// <summary>
    /// Class to handle verbosity of MLPlugin logs.
    /// </summary>
    [Obsolete("Please use MLPluginLog instead.", true)]
    public static class MLVerbosity
    {
        /// <summary>
        /// Different levels of which logs to print.
        /// Modify this to change the verbosity of the Magic Leap APIs.
        /// </summary>
        public enum Levels : uint
        {
            /// <summary>
            /// Don't print any MLPlugin logs.
            /// </summary>
            Silent = 0,

            /// <summary>
            /// Print only MLPlugin error logs.
            /// </summary>
            ErrorsOnly,

            /// <summary>
            /// Print MLPlugin error and warning logs.
            /// </summary>
            WarningsAndErrors,

            /// <summary>
            /// Print all MLPlugin logs.
            /// </summary>
            Verbose

        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// The current level of logs to print.
        /// </summary>
        [Obsolete("Please use MLPluginLog.Level instead.", true)]
        public static Levels Level = Levels.WarningsAndErrors;
        #endif
    }
}
