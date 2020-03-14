// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLPluginLog.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// Used to print logs within the plugin.
    /// </summary>
    public static class MLPluginLog
    {
        /// <summary>
        /// Different verbosity levels of which logs to print.
        /// Modify this to change the verbosity of the Magic Leap APIs.
        /// </summary>
        public enum VerbosityLevel : uint
        {
            /// <summary>
            /// Don't print any MLPlugin logs.
            /// </summary>
            Silent,

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
            Verbose,
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Gets or sets current level of logs to print.
        /// </summary>
        public static VerbosityLevel Level { get; set; }  = VerbosityLevel.ErrorsOnly;

        /// <summary>
        /// Prints the given log message.
        /// </summary>
        /// <param name="logMsg">The message to print.</param>
        public static void Debug(object logMsg)
        {
            if (MLPluginLog.Level == MLPluginLog.VerbosityLevel.Verbose)
            {
                UnityEngine.Debug.Log(logMsg);
            }
        }

        /// <summary>
        /// Prints the given log message with formatting.
        /// </summary>
        /// <param name="logMsg">The formatted message to print.</param>
        /// <param name="args">The arguments to pass the formatted log message</param>
        public static void DebugFormat(string logMsg, params object[] args)
        {
            if (MLPluginLog.Level == MLPluginLog.VerbosityLevel.Verbose)
            {
                UnityEngine.Debug.LogFormat(logMsg, args);
            }
        }

        /// <summary>
        /// Prints the given log message as a warning.
        /// </summary>
        /// <param name="logMsg">The warning to print.</param>
        public static void Warning(object logMsg)
        {
            if (MLPluginLog.Level == MLPluginLog.VerbosityLevel.Verbose || MLPluginLog.Level == MLPluginLog.VerbosityLevel.WarningsAndErrors)
            {
                UnityEngine.Debug.LogWarning("Warning: " + logMsg);
            }
        }

        /// <summary>
        /// Prints the given log message as a warning with formatting.
        /// </summary>
        /// <param name="logMsg">The formatted warning message to print.</param>
        /// <param name="args">The arguments to pass the formatted message</param>
        public static void WarningFormat(string logMsg, params object[] args)
        {
            if (MLPluginLog.Level == MLPluginLog.VerbosityLevel.Verbose || MLPluginLog.Level == MLPluginLog.VerbosityLevel.WarningsAndErrors)
            {
                UnityEngine.Debug.LogWarningFormat("Warning: " + logMsg, args);
            }
        }

        /// <summary>
        /// Prints the given log message as an error.
        /// </summary>
        /// <param name="logMsg">The error to print.</param>
        public static void Error(object logMsg)
        {
            if (MLPluginLog.Level != MLPluginLog.VerbosityLevel.Silent)
            {
                UnityEngine.Debug.LogError("Error: " + logMsg);
            }
        }

        /// <summary>
        /// Prints the given log message as an error with formatting.
        /// </summary>
        /// <param name="logMsg">The formatted error message to print.</param>
        /// <param name="args">The arguments to pass the formatted message</param>
        public static void ErrorFormat(string logMsg, params object[] args)
        {
            if (MLPluginLog.Level != MLPluginLog.VerbosityLevel.Silent)
            {
                UnityEngine.Debug.LogErrorFormat("Error: " + logMsg, args);
            }
        }
        #endif
    }
}
