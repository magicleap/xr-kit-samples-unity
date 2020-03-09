// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLLocale.cs" company="Magic Leap, Inc">
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
    /// MLLocale provides the Language and Country set at the system level.
    /// </summary>
    public partial class MLLocale
    {
        /// <summary>
        /// Reads the language code of the system locale.
        /// </summary>
        /// <param name="language">Language code defined in ISO 639. Valid only if <c>MLResult.Code.Ok</c> is returned, empty string otherwise.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the language code was retrieved successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an unspecified error.
        /// </returns>
        public static MLResult GetSystemLanguage(out string language)
        {
            IntPtr outLanguage = IntPtr.Zero;
            MLResult result;

            try
            {
                MLResult.Code resultCode = NativeBindings.MLLocaleGetSystemLanguage(ref outLanguage);
                result = MLResult.Create(resultCode);

                if (result.IsOk)
                {
                    language = MLConvert.DecodeUTF8(outLanguage);
                }
                else
                {
                    language = string.Empty;
                    MLPluginLog.ErrorFormat("MLLocale.GetSystemLanguage failed. Reason: {0}", result);
                }
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error("MLLocale.GetSystemLanguage failed. Reason: MLLocale API is currently available only on device.");
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLLocale.GetSystemLanguage failed. Reason: Dll not found.");
                language = string.Empty;
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLLocale.GetSystemLanguage failed. Reason: API symbols not found.");
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLLocale.GetSystemLanguage failed. Reason: API symbols not found.");
                language = string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Reads the country code of the system locale.
        /// </summary>
        /// <param name="country">Country code defined in ISO 3166, or an empty string. Valid only if <c>MLResult.Code.Ok</c> is returned, empty string otherwise.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the country code was retrieved successfully.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an unspecified error.
        /// </returns>
        public static MLResult GetSystemCountry(out string country)
        {
            IntPtr outCountry = IntPtr.Zero;
            MLResult result;

            try
            {
                MLResult.Code resultCode = NativeBindings.MLLocaleGetSystemCountry(ref outCountry);
                result = MLResult.Create(resultCode);
                if (result.IsOk)
                {
                    country = MLConvert.DecodeUTF8(outCountry);
                }
                else
                {
                    country = string.Empty;
                    MLPluginLog.ErrorFormat("MLLocale.GetSystemCountry failed. Reason: {0}", result);
                }
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error("MLLocale.GetSystemCountry failed. Reason: MLLocale API is currently available only on device.");
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLLocale.GetSystemCountry failed. Reason: Dll not found.");
                country = string.Empty;
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLLocale.GetSystemCountry failed. Reason: API symbols not found.");
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLLocale.GetSystemCountry failed. Reason: API symbols not found.");
                country = string.Empty;
            }

            return result;
        }
    }
}

#endif
