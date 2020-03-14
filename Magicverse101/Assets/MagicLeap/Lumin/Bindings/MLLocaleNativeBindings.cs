// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLLocaleNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;

    /// <summary>
    /// MLLocale provides the Language and Country set at the system level.
    /// </summary>
    public partial class MLLocale
    {
        /// <summary>
        /// See ml_locale.h for additional comments.
        /// </summary>
        private class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// ML Locale DLL name.
            /// </summary>
            private const string MLLocaleDll = "ml_locale";

            /// <summary>
            /// Reads the language code of the system locale.
            /// </summary>
            /// <param name="out_language">Language code defined in ISO 639. Valid only if <c>MLResult.Code.Ok</c> is returned, empty string otherwise.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the language code was retrieved successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an unspecified error.
            /// </returns>
            [DllImport(MLLocaleDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLLocaleGetSystemLanguage(ref IntPtr out_language);

            /// <summary>
            /// Reads the country code of the system locale.
            /// </summary>
            /// <param name="out_country">Country code defined in ISO 3166, or an empty string. Valid only if <c>MLResult.Code.Ok</c> is returned, empty string otherwise.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the country code was retrieved successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an unspecified error.
            /// </returns>
            [DllImport(MLLocaleDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLLocaleGetSystemCountry(ref IntPtr out_country);
        }
    }
}

#endif
