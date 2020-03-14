// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLIdentityNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Functionality to query for user profiles.
    /// </summary>
    public partial class MLIdentity
    {
        /// <summary>
        /// The native bindings for the MLIdentity API.
        /// See ml_identity.h for additional comments
        /// </summary>
        private class NativeBindings : MagicLeapNativeBindings
        {
            /// <summary>
            /// Name of the DLL to access the MLIdentity APIs from.
            /// </summary>
            private const string MLIdentityDll = "ml_identity";

            /// <summary>
            /// Map of MLIdentity.Profile class references to their unmanaged memory pointers.
            /// </summary>
            private static Dictionary<MLIdentity.Profile, IntPtr> profilePtrMap = new Dictionary<MLIdentity.Profile, IntPtr>();

            /// <summary>
            /// Map of MLIdentity.Profile class references to their associated MLInvokeFuture pointers used for polling.
            /// </summary>
            private static Dictionary<MLIdentity.Profile, IntPtr> profileFuturePtrMap = new Dictionary<MLIdentity.Profile, IntPtr>();

            /// <summary>
            /// Initializes a new instance of the <see cref="NativeBindings" /> class.
            /// </summary>
            protected NativeBindings()
            {
            }

            /// <summary>
            /// Cleans up unmanaged profile memory.
            /// </summary>
            /// <param name="profile">The profile reference to clean.</param>
            public static void CleanupProfileMemory(MLIdentity.Profile profile)
            {
                if (profile.CurrentRequest != null)
                {
                    MLPluginLog.Warning("This profile has an ongoing request and cannot have it's memory released.");
                    return;
                }

                IntPtr profilePtr = profilePtrMap.ContainsKey(profile) ? profilePtrMap[profile] : IntPtr.Zero;

                if (profilePtr != IntPtr.Zero)
                {
                    try
                    {
                        MLResult.Code resultCode = MLIdentity.NativeBindings.MLIdentityReleaseUserProfile(profilePtr);

                        if (resultCode != MLResult.Code.Ok)
                        {
                            MLPluginLog.ErrorFormat("MLIdentity.NativeBindings.CleanupProfileMemory failed to release user profile. Reason: {0}", resultCode);
                        }
                    }
                    catch (EntryPointNotFoundException)
                    {
                        MLPluginLog.Error("MLIdentity.NativeBindings.CleanupProfileMemory failed. Reason: API symbols not found.");
                    }
                }

                profilePtrMap.Remove(profile);
            }

            /// <summary>
            /// Queries a profile for a list of attribute names asynchronously.
            /// </summary>
            /// <param name="profile">Reference to the associated profile.</param>
            /// <param name="requestAttributes">Reference to any known attribute names that have been requested.</param>
            /// <param name="attributes">Reference to the array of attributes of a profile.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToLocalService</c> if the local service is not running, or it cannot be accessed
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToCloudService</c> if there is no IP connection or the cloud service is not available.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCloudAuthentication</c> if the user does not have the required privileges to use the requesting service.or the refresh token used by the service is invalid.
            /// MLResult.Result will be <c>MLResult.Code.IdentityInvalidInformationFromCloud</c> if the signature verification failed on the information returned by the cloud or a parsing error occurred.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNotLoggedIn</c> if the user is not logged in to the cloud.
            /// MLResult.Result will be <c>MLResult.Code.IdentityExpiredCredentials</c> if the user's credentials have expired.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToGetUserProfile</c> if failed to retrieve attributes of the user's profile.
            /// MLResult.Result will be <c>MLResult.Code.IdentityUnauthorized</c> if the user is not authorized to execute the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCertificateError</c> if the device failed to authenticate the server.
            /// MLResult.Result will be <c>MLResult.Code.IdentityRejectedByCloud</c> if the cloud rejected the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityAlreadyLoggedIn</c> if the user is already logged in.
            /// MLResult.Result will be <c>MLResult.Code.IdentityModifyIsNotSupported</c> if the cloud does not support modification of an attribute value.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNetworkError</c> if the device is not connected to a network.
            /// </returns>
            public static MLResult.Code RequestAttributeNamesAsync(MLIdentity.Profile profile, MLIdentity.Profile.Attribute.Type[] requestAttributes, ref MLIdentity.Profile.Attribute[] attributes)
            {
                try
                {
                    if (requestAttributes != null && requestAttributes.Length > 0)
                    {
                        IntPtr profilePtr = profilePtrMap.ContainsKey(profile) ? profilePtrMap[profile] : IntPtr.Zero;

                        MLResult.Code resultCode = MLIdentity.NativeBindings.MLIdentityGetKnownAttributeNames(requestAttributes, (uint)requestAttributes.Length, ref profilePtr);

                        if (MLResult.IsOK(resultCode))
                        {
                            profilePtrMap.Remove(profile);
                            profilePtrMap.Add(profile, profilePtr);

                            MLIdentity.NativeBindings.ProfileNative profileNative = (MLIdentity.NativeBindings.ProfileNative)Marshal.PtrToStructure(profilePtr, typeof(MLIdentity.NativeBindings.ProfileNative));

                            attributes = new MLIdentity.Profile.Attribute[profileNative.AttributeCount];
                            for (int i = 0; i < profileNative.AttributeCount; i++)
                            {
                                IntPtr offsetPtr = Marshal.ReadIntPtr(new IntPtr(profileNative.AttributePtrs.ToInt64() + (Marshal.SizeOf(typeof(IntPtr)) * i)));
                                attributes[i] = (MLIdentity.Profile.Attribute)Marshal.PtrToStructure(offsetPtr, typeof(MLIdentity.Profile.Attribute));

                                // Set this to true so that we can request the value.
                                attributes[i].IsRequested = true;
                            }
                        }
                        else
                        {
                            attributes = new MLIdentity.Profile.Attribute[0];
                            MLPluginLog.ErrorFormat("MLIdentity.NativeBindings.ProfileRequestAttributeNamesAsync failed to get known attributes names. Reason: {0}", resultCode);
                        }

                        return resultCode;
                    }
                    else
                    {
                        IntPtr profileFuturePtr = IntPtr.Zero;
                        MLResult.Code resultCode = MLIdentity.NativeBindings.MLIdentityGetAttributeNamesAsync(ref profileFuturePtr);

                        if (MLResult.IsOK(resultCode))
                        {
                            profileFuturePtrMap.Remove(profile);
                            profileFuturePtrMap.Add(profile, profileFuturePtr);
                        }
                        else if (resultCode == MLResult.Code.PrivilegeDenied)
                        {
                            MLPluginLog.Warning("MLIdentity.NativeBindings.ProfileRequestAttributeNamesAsync failed request for attribute names. Reason: Caller does not have IdentityRead Privilege.");
                        }
                        else
                        {
                            MLPluginLog.ErrorFormat("MLIdentity.NativeBindings.ProfileRequestAttributeNamesAsync failed request for attribute names. Reason: {0}", resultCode);
                        }

                        attributes = new MLIdentity.Profile.Attribute[0];

                        return resultCode;
                    }
                }
                catch (EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLIdentity.NativeBindings.ProfileRequestAttributeNamesAsync failed. Reason: API symbols not found.");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }

            /// <summary>
            /// Queries a profile for a list of attribute values asynchronously.
            /// </summary>
            /// <param name="profile">Reference to the associated profile.</param>
            /// <param name="attributes">Reference to the array of attributes of a profile.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToLocalService</c> if the local service is not running, or it cannot be accessed
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToCloudService</c> if there is no IP connection or the cloud service is not available.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCloudAuthentication</c> if the user does not have the required privileges to use the requesting service.or the refresh token used by the service is invalid.
            /// MLResult.Result will be <c>MLResult.Code.IdentityInvalidInformationFromCloud</c> if the signature verification failed on the information returned by the cloud or a parsing error occurred.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNotLoggedIn</c> if the user is not logged in to the cloud.
            /// MLResult.Result will be <c>MLResult.Code.IdentityExpiredCredentials</c> if the user's credentials have expired.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToGetUserProfile</c> if failed to retrieve attributes of the user's profile.
            /// MLResult.Result will be <c>MLResult.Code.IdentityUnauthorized</c> if the user is not authorized to execute the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCertificateError</c> if the device failed to authenticate the server.
            /// MLResult.Result will be <c>MLResult.Code.IdentityRejectedByCloud</c> if the cloud rejected the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityAlreadyLoggedIn</c> if the user is already logged in.
            /// MLResult.Result will be <c>MLResult.Code.IdentityModifyIsNotSupported</c> if the cloud does not support modification of an attribute value.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNetworkError</c> if the device is not connected to a network.
            /// </returns>
            public static MLResult.Code RequestAttributeValuesAsync(MLIdentity.Profile profile, ref MLIdentity.Profile.Attribute[] attributes)
            {
                try
                {
                    IntPtr profilePtr = profilePtrMap.ContainsKey(profile) ? profilePtrMap[profile] : IntPtr.Zero;
                    if (profilePtr == IntPtr.Zero)
                    {
                        MLPluginLog.Warning("MLIdentity.NativeBindings.RequestAttributeValuesAsync failed because a valid profile pointer could not be found with the passed profile.");
                        return MLResult.Code.UnspecifiedFailure;
                    }

                    MLIdentity.NativeBindings.ProfileNative profileNative = (MLIdentity.NativeBindings.ProfileNative)Marshal.PtrToStructure(profilePtr, typeof(MLIdentity.NativeBindings.ProfileNative));

                    // Copy any modifications made to the profile in managed memory to the unmanaged memory used by the Identity API.
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        IntPtr offsetPtr = Marshal.ReadIntPtr(new IntPtr(profileNative.AttributePtrs.ToInt64() + (Marshal.SizeOf(typeof(IntPtr)) * i)));

                        // Write the managed copy back onto the unmanaged memory we were originally given for it.
                        Marshal.StructureToPtr(attributes[i], offsetPtr, false);
                    }

                    IntPtr profileFuturePtr = IntPtr.Zero;
                    MLResult.Code resultCode = MLIdentity.NativeBindings.MLIdentityRequestAttributeValuesAsync(profilePtr, ref profileFuturePtr);

                    if (MLResult.IsOK(resultCode))
                    {
                        profileFuturePtrMap.Remove(profile);
                        profileFuturePtrMap.Add(profile, profileFuturePtr);
                    }
                    else if (resultCode == MLResult.Code.PrivilegeDenied)
                    {
                        MLPluginLog.Warning("MLIdentity.NativeBindings.RequestAttributeValuesAsync failed. Reason: Caller does not have IdentityRead Privilege.");
                    }
                    else
                    {
                        MLPluginLog.ErrorFormat("MLIdentity.NativeBindings.RequestAttributeValuesAsync failed. Reason: {0}", resultCode);
                    }

                    return resultCode;
                }
                catch (EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLIdentity.NativeBindings.RequestAttributeValuesAsync failed. Reason: API symbols not found.");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }

            /// <summary>
            /// Listens for a list of attribute names previously requested by RequestAttributeValuesAsync.
            /// </summary>
            /// <param name="profile">Reference to the associated profile.</param>
            /// <param name="attributes">Reference to the array of attributes of a profile.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the timeout elapsed before the asynchronous call completed.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToLocalService</c> if the local service is not running, or it cannot be accessed
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToCloudService</c> if there is no IP connection or the cloud service is not available.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCloudAuthentication</c> if the user does not have the required privileges to use the requesting service.or the refresh token used by the service is invalid.
            /// MLResult.Result will be <c>MLResult.Code.IdentityInvalidInformationFromCloud</c> if the signature verification failed on the information returned by the cloud or a parsing error occurred.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNotLoggedIn</c> if the user is not logged in to the cloud.
            /// MLResult.Result will be <c>MLResult.Code.IdentityExpiredCredentials</c> if the user's credentials have expired.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToGetUserProfile</c> if failed to retrieve attributes of the user's profile.
            /// MLResult.Result will be <c>MLResult.Code.IdentityUnauthorized</c> if the user is not authorized to execute the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCertificateError</c> if the device failed to authenticate the server.
            /// MLResult.Result will be <c>MLResult.Code.IdentityRejectedByCloud</c> if the cloud rejected the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityAlreadyLoggedIn</c> if the user is already logged in.
            /// MLResult.Result will be <c>MLResult.Code.IdentityModifyIsNotSupported</c> if the cloud does not support modification of an attribute value.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNetworkError</c> if the device is not connected to a network.
            /// </returns>
            public static MLResult.Code ListenAttributeNamesResponse(MLIdentity.Profile profile, ref MLIdentity.Profile.Attribute[] attributes)
            {
                try
                {
                    IntPtr profilePtr = profilePtrMap.ContainsKey(profile) ? profilePtrMap[profile] : IntPtr.Zero;
                    IntPtr profileFuturePtr = profileFuturePtrMap.ContainsKey(profile) ? profileFuturePtrMap[profile] : IntPtr.Zero;

                    if (profileFuturePtr == IntPtr.Zero)
                    {
                        MLPluginLog.Warning("MLIdentity.NativeBindings.ListenAttributeNamesResponse failed because a valid future pointer could not be found with the passed request.");
                        return MLResult.Code.UnspecifiedFailure;
                    }

                    //// Attempt to get data if available, 0 is passed as a timeout to immediately return and never wait for results.
                    MLResult.Code resultCode = MLIdentity.NativeBindings.MLIdentityGetAttributeNamesWait(profileFuturePtr, 0, ref profilePtr);

                    // If it succeeded, copy any modifications made to the profile in unmanaged memory by the Identity API to managed memory.
                    if (MLResult.IsOK(resultCode))
                    {
                        profileFuturePtrMap.Remove(profile);

                        profilePtrMap.Remove(profile);
                        profilePtrMap.Add(profile, profilePtr);

                        MLIdentity.NativeBindings.ProfileNative profileNative = (MLIdentity.NativeBindings.ProfileNative)Marshal.PtrToStructure(profilePtr, typeof(MLIdentity.NativeBindings.ProfileNative));

                        attributes = new MLIdentity.Profile.Attribute[profileNative.AttributeCount];
                        for (int i = 0; i < profileNative.AttributeCount; i++)
                        {
                            IntPtr offsetPtr = Marshal.ReadIntPtr(new IntPtr(profileNative.AttributePtrs.ToInt64() + (Marshal.SizeOf(typeof(IntPtr)) * i)));

                            // Write the unmanaged copy back onto the managed memory we were passed in.
                            attributes[i] = (MLIdentity.Profile.Attribute)Marshal.PtrToStructure(offsetPtr, typeof(MLIdentity.Profile.Attribute));

                            // Set this to true so that we can request the value.
                            attributes[i].IsRequested = true;
                        }
                    }
                    else
                    {
                        attributes = new MLIdentity.Profile.Attribute[0];
                    }

                    return resultCode;
                }
                catch (EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLIdentity.NativeBindings.ListenAttributeNamesResponse failed. Reason: API symbols not found.");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }

            /// <summary>
            /// Listens for a list of attribute values previously requested by RequestAttributeValuesAsync.
            /// </summary>
            /// <param name="profile">Reference to the associated profile.</param>
            /// <param name="attributes">Reference to the array of attributes of a profile.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully before the timeout elapsed.
            /// The profile provided in the MLIdentityRequestAttributeValuesAsync call was updated as follows:
            /// Attributes for which the IsRequested field is true, that are still available in the cloud and
            /// which the user has approved to make available for the calling service will have their values filled.
            /// The IsGranted field will be set by the library to true in those attributes that were filled and false in the others.
            /// If any other value is returned, the profile provided in the
            /// MLIdentityRequestAttributeValuesAsync() call was not updated.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the timeout elapsed before the asynchronous call completed.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToLocalService</c> if the local service is not running, or it cannot be accessed
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToCloudService</c> if there is no IP connection or the cloud service is not available.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCloudAuthentication</c> if the user does not have the required privileges to use the requesting service.or the refresh token used by the service is invalid.
            /// MLResult.Result will be <c>MLResult.Code.IdentityInvalidInformationFromCloud</c> if the signature verification failed on the information returned by the cloud or a parsing error occurred.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNotLoggedIn</c> if the user is not logged in to the cloud.
            /// MLResult.Result will be <c>MLResult.Code.IdentityExpiredCredentials</c> if the user's credentials have expired.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToGetUserProfile</c> if failed to retrieve attributes of the user's profile.
            /// MLResult.Result will be <c>MLResult.Code.IdentityUnauthorized</c> if the user is not authorized to execute the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCertificateError</c> if the device failed to authenticate the server.
            /// MLResult.Result will be <c>MLResult.Code.IdentityRejectedByCloud</c> if the cloud rejected the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityAlreadyLoggedIn</c> if the user is already logged in.
            /// MLResult.Result will be <c>MLResult.Code.IdentityModifyIsNotSupported</c> if the cloud does not support modification of an attribute value.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNetworkError</c> if the device is not connected to a network.
            /// </returns>
            public static MLResult.Code ListenAttributeValuesResponse(MLIdentity.Profile profile, ref MLIdentity.Profile.Attribute[] attributes)
            {
                try
                {
                    IntPtr profileFuturePtr = profileFuturePtrMap.ContainsKey(profile) ? profileFuturePtrMap[profile] : IntPtr.Zero;

                    if (profileFuturePtr == IntPtr.Zero)
                    {
                        MLPluginLog.Warning("MLIdentity.NativeBindings.ListenAttributeValuesResponse failed because a valid future pointer could not be found with the passed request.");
                        return MLResult.Code.UnspecifiedFailure;
                    }

                    // Attempt to get data if available, 0 is passed as a timeout to immediately return and never wait for results.
                    MLResult.Code resultCode = MLIdentity.NativeBindings.MLIdentityRequestAttributeValuesWait(profileFuturePtr, 0);

                    // If it succeeded, copy any modifications made to the profile in unmanaged memory by the Identity API to managed memory.
                    if (MLResult.IsOK(resultCode))
                    {
                        profileFuturePtrMap.Remove(profile);

                        IntPtr profilePtr = profilePtrMap.ContainsKey(profile) ? profilePtrMap[profile] : IntPtr.Zero;

                        MLIdentity.NativeBindings.ProfileNative profileNative = (MLIdentity.NativeBindings.ProfileNative)Marshal.PtrToStructure(profilePtr, typeof(MLIdentity.NativeBindings.ProfileNative));

                        for (int i = 0; i < attributes.Length; i++)
                        {
                            IntPtr offsetPtr = Marshal.ReadIntPtr(new IntPtr(profileNative.AttributePtrs.ToInt64() + (Marshal.SizeOf(typeof(IntPtr)) * i)));

                            // Write the unmanaged copy back onto the managed memory we were passed in.
                            attributes[i] = (MLIdentity.Profile.Attribute)Marshal.PtrToStructure(offsetPtr, typeof(MLIdentity.Profile.Attribute));
                        }
                    }

                    return resultCode;
                }
                catch (EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLIdentity.NativeBindings.ListenAttributeNamesResponse failed. Reason: API symbols not found.");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }

            /// <summary>
            /// MLIdentityGetAttributeNames() is a blocking function that accesses the cloud and
            /// returns an MLIdentity.Profile structure containing all of the attributes that are available
            /// for the Audience of the calling service.
            /// The library deduces the Audience being requested from the name of the calling service.
            /// This method does not request access to the values of the attributes from the user and does
            /// not return the values of these attributes. Only the names are provided.
            /// In order to request access for the attributes values and to receive them, set the
            /// IsRequested field of each required attribute in the profile that is returned and call
            /// one of the methods: MLIdentityRequestAttributeValues() or MLIdentityRequestAttributeValuesAsync().
            /// </summary>
            /// <param name="outProfile">
            /// A pointer to an MLIdentity.Profile pointer that is allocated by the library.
            /// In each attribute of the returned profile the name field will point to the name of the
            /// attribute, the value field will point to an empty string and the IsRequested and IsGranted
            /// flags will both be false.
            /// </param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToLocalService</c> if the local service is not running, or it cannot be accessed
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToCloudService</c> if there is no IP connection or the cloud service is not available.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCloudAuthentication</c> if the user does not have the required privileges to use the requesting service.or the refresh token used by the service is invalid.
            /// MLResult.Result will be <c>MLResult.Code.IdentityInvalidInformationFromCloud</c> if the signature verification failed on the information returned by the cloud or a parsing error occurred.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNotLoggedIn</c> if the user is not logged in to the cloud.
            /// MLResult.Result will be <c>MLResult.Code.IdentityExpiredCredentials</c> if the user's credentials have expired.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToGetUserProfile</c> if failed to retrieve attributes of the user's profile.
            /// MLResult.Result will be <c>MLResult.Code.IdentityUnauthorized</c> if the user is not authorized to execute the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCertificateError</c> if the device failed to authenticate the server.
            /// MLResult.Result will be <c>MLResult.Code.IdentityRejectedByCloud</c> if the cloud rejected the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityAlreadyLoggedIn</c> if the user is already logged in.
            /// MLResult.Result will be <c>MLResult.Code.IdentityModifyIsNotSupported</c> if the cloud does not support modification of an attribute value.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNetworkError</c> if the device is not connected to a network.
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLIdentityGetAttributeNames(ref IntPtr outProfile);

            /// <summary>
            /// MLIdentityGetKnownAttributeNames() returns an MLIdentity.Profile structure containing
            /// attributes of a user's profile whose names are specified in the attributeNames array.
            /// Each element of the attributeNames array may only be one of the values specified above.
            /// This method does not access the cloud to discover which attribute names are actually
            /// available for the calling service, does not request access to the values of these
            /// attributes for the service by the user, and does not return the values of these attributes.
            /// In order to request access for the attributes and to receive their values, pass the profile
            /// that is returned by MLIdentityGetKnownAttributeNames() to one of the methods:
            /// MLIdentityRequestAttributeValues() or MLIdentityRequestAttributeValuesAsync().
            /// </summary>
            /// <param name="keys">An array of distinct MLIdentity.Attribute.Type values that are not equal to MLIdentity.Attribute.Type.Unknown.</param>
            /// <param name="keyCount">The number of elements in the MLIdentity.Attribute.Type array.</param>
            /// <param name="outProfile">
            /// A pointer to an MLIdentity.Profile pointer that is allocated by the library.
            /// The returned profile will contain an array of MLIdentity.Attribute structures such that the
            /// enumValue of each attribute is contained in the keys array flags will both be false.
            /// If the attribute specified by any of the keys is not available, a corresponding
            /// MLIdentity.Attribute will not appear in the MLIdentity.Profile.
            /// Not available in this context means that the user profile does not contain the attribute,
            /// not that the caller does not have permission to access its value.
            /// All attributes in the returned profile will have their IsRequested field set to true and
            /// their IsGranted field to false. The values of the attributes will point to empty strings.
            /// The returned profile must be released using #MLIdentityReleaseUserProfile when no longer needed.
            /// </param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToLocalService</c> if the local service is not running, or it cannot be accessed
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToCloudService</c> if there is no IP connection or the cloud service is not available.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCloudAuthentication</c> if the user does not have the required privileges to use the requesting service.or the refresh token used by the service is invalid.
            /// MLResult.Result will be <c>MLResult.Code.IdentityInvalidInformationFromCloud</c> if the signature verification failed on the information returned by the cloud or a parsing error occurred.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNotLoggedIn</c> if the user is not logged in to the cloud.
            /// MLResult.Result will be <c>MLResult.Code.IdentityExpiredCredentials</c> if the user's credentials have expired.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToGetUserProfile</c> if failed to retrieve attributes of the user's profile.
            /// MLResult.Result will be <c>MLResult.Code.IdentityUnauthorized</c> if the user is not authorized to execute the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCertificateError</c> if the device failed to authenticate the server.
            /// MLResult.Result will be <c>MLResult.Code.IdentityRejectedByCloud</c> if the cloud rejected the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityAlreadyLoggedIn</c> if the user is already logged in.
            /// MLResult.Result will be <c>MLResult.Code.IdentityModifyIsNotSupported</c> if the cloud does not support modification of an attribute value.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNetworkError</c> if the device is not connected to a network.
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLIdentityGetKnownAttributeNames([MarshalAs(UnmanagedType.LPArray)] MLIdentity.Profile.Attribute.Type[] keys, uint keyCount, ref IntPtr outProfile);

            /// <summary>
            /// MLIdentityGetAttributeNamesAsync() invokes the MLIdentityGetAttributeNames()
            /// function asynchronously (in a different thread).
            /// </summary>
            /// <param name="outFuture">
            /// A pointer to an MLInvokeFuture which provides the means to poll for completion and
            /// to retrieve the profile returned by MLIdentityGetAttributeNames().
            /// This pointer will be freed by the library before returning from the first (and last)
            /// call to MLIdentityGetAttributeNamesWait(), after the asynchronous call completed, that is
            /// after MLIdentityGetAttributeNamesWait() returns any value that is not MLResult.Code.Pending.
            /// </param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToLocalService</c> if the local service is not running, or it cannot be accessed
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToCloudService</c> if there is no IP connection or the cloud service is not available.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCloudAuthentication</c> if the user does not have the required privileges to use the requesting service.or the refresh token used by the service is invalid.
            /// MLResult.Result will be <c>MLResult.Code.IdentityInvalidInformationFromCloud</c> if the signature verification failed on the information returned by the cloud or a parsing error occurred.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNotLoggedIn</c> if the user is not logged in to the cloud.
            /// MLResult.Result will be <c>MLResult.Code.IdentityExpiredCredentials</c> if the user's credentials have expired.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToGetUserProfile</c> if failed to retrieve attributes of the user's profile.
            /// MLResult.Result will be <c>MLResult.Code.IdentityUnauthorized</c> if the user is not authorized to execute the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCertificateError</c> if the device failed to authenticate the server.
            /// MLResult.Result will be <c>MLResult.Code.IdentityRejectedByCloud</c> if the cloud rejected the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityAlreadyLoggedIn</c> if the user is already logged in.
            /// MLResult.Result will be <c>MLResult.Code.IdentityModifyIsNotSupported</c> if the cloud does not support modification of an attribute value.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNetworkError</c> if the device is not connected to a network.
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLIdentityGetAttributeNamesAsync(ref IntPtr outFuture);

            /// <summary>
            /// Having made a call to MLIdentityGetAttributeNamesAsync(), the user can call
            /// MLIdentityGetAttributeNamesWait() to detect whether the asynchronous call completed and if
            /// successful, to retrieve the profile in outProfile.
            /// The call to MLIdentityGetAttributeNamesWait() blocks until either <c>msecTimeout</c> elapses or the asynchronous function completes.
            /// To poll without blocking use <c>msecTimeout</c> = 0.
            /// After the function returns a value other than MLResult.Code.Pending, future is freed by the
            /// library and must not be used again in a call to MLIdentityGetAttributeNamesWait().
            /// </summary>
            /// <param name="future">The pointer returned by the MLIdentityGetAttributeNamesAsync() function.</param>
            /// <param name="msecTimeout"> The timeout in milliseconds.</param>
            /// <param name="outProfile">
            /// The location in memory where the pointer to the profile structure allocated by
            /// the library will be copied, if the asynchronous call completed, or 0 (null) if not.
            /// </param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully before the timeout elapsed.
            /// The location pointed to by outProfile was set to the address of the ProfileNative
            /// structure allocated by the library.The returned profile must be released when no longer
            /// needed by calling MLIdentityReleaseUserProfile().
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the timeout elapsed before the asynchronous call completed.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToLocalService</c> if the local service is not running, or it cannot be accessed
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToCloudService</c> if there is no IP connection or the cloud service is not available.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCloudAuthentication</c> if the user does not have the required privileges to use the requesting service.or the refresh token used by the service is invalid.
            /// MLResult.Result will be <c>MLResult.Code.IdentityInvalidInformationFromCloud</c> if the signature verification failed on the information returned by the cloud or a parsing error occurred.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNotLoggedIn</c> if the user is not logged in to the cloud.
            /// MLResult.Result will be <c>MLResult.Code.IdentityExpiredCredentials</c> if the user's credentials have expired.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToGetUserProfile</c> if failed to retrieve attributes of the user's profile.
            /// MLResult.Result will be <c>MLResult.Code.IdentityUnauthorized</c> if the user is not authorized to execute the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCertificateError</c> if the device failed to authenticate the server.
            /// MLResult.Result will be <c>MLResult.Code.IdentityRejectedByCloud</c> if the cloud rejected the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityAlreadyLoggedIn</c> if the user is already logged in.
            /// MLResult.Result will be <c>MLResult.Code.IdentityModifyIsNotSupported</c> if the cloud does not support modification of an attribute value.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNetworkError</c> if the device is not connected to a network.
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLIdentityGetAttributeNamesWait(IntPtr future, uint msecTimeout, ref IntPtr outProfile);

            /// <summary>
            /// MLIdentityRequestAttributeValues() is a blocking function that accesses the cloud
            /// and requests permission from the user in order to fill the attribute values that are marked as
            /// requested in the MLIdentity.Profile pointed to by the profile argument.
            /// If any (even all) of the attributes passed in have IsRequested set to false this will not
            /// cause the function to return an error.
            /// If any (even all) of the attributes passed in are no longer available for the user, this
            /// will not cause the function to return an error.
            /// </summary>
            /// <param name="profile">
            /// A pointer to the MLIdentity.Profile that was allocated by the library in which
            /// none one or more of the attributes have had their IsRequested field set to true.
            /// Attributes for which the is_requested field is true, that are still available in the cloud and
            /// which the user has approved to make available for the calling service will have their values filled.
            /// The IsGranted field will be set by the library to true in those attributes that were filled and false in the others.
            /// </param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the timeout elapsed before the asynchronous call completed.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToLocalService</c> if the local service is not running, or it cannot be accessed
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToCloudService</c> if there is no IP connection or the cloud service is not available.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCloudAuthentication</c> if the user does not have the required privileges to use the requesting service.or the refresh token used by the service is invalid.
            /// MLResult.Result will be <c>MLResult.Code.IdentityInvalidInformationFromCloud</c> if the signature verification failed on the information returned by the cloud or a parsing error occurred.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNotLoggedIn</c> if the user is not logged in to the cloud.
            /// MLResult.Result will be <c>MLResult.Code.IdentityExpiredCredentials</c> if the user's credentials have expired.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToGetUserProfile</c> if failed to retrieve attributes of the user's profile.
            /// MLResult.Result will be <c>MLResult.Code.IdentityUnauthorized</c> if the user is not authorized to execute the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCertificateError</c> if the device failed to authenticate the server.
            /// MLResult.Result will be <c>MLResult.Code.IdentityRejectedByCloud</c> if the cloud rejected the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityAlreadyLoggedIn</c> if the user is already logged in.
            /// MLResult.Result will be <c>MLResult.Code.IdentityModifyIsNotSupported</c> if the cloud does not support modification of an attribute value.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNetworkError</c> if the device is not connected to a network.
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLIdentityRequestAttributeValues(IntPtr profile);

            /// <summary>
            /// MLIdentityRequestAttributeValuesAsync() invokes the MLIdentityRequestAttributeValues() function in a different thread.
            /// </summary>
            /// <param name="profile">
            /// A pointer to the MLIdentity.Profile that was allocated by the library in which
            /// none one or more of the attributes have had their IsRequested field set to true.
            /// Attributes for which the is_requested field is true, that are still available in the cloud and
            /// which the user has approved to make available for the calling service will have their
            /// values filled.
            /// The IsGranted field will be set by the library to true in those attributes that were filled
            /// and false in the others.
            /// The profile must not be released (using MLIdentityReleaseUserProfile()) until
            /// MLIdentityRequestAttributeValuesWait() returns a value other than MLResult.Code.Pending, because
            /// it will be written to asynchronously by MLIdentityRequestAttributeValuesAsync().
            /// </param>
            /// <param name="outFuture">
            /// A pointer to an MLInvokeFuture which provides the means to poll for completion and
            /// to retrieve the profile returned by MLIdentityRequestAttributeValues().
            /// This pointer will be freed by the library before returning from the first(and last) call
            /// to MLIdentityRequestAttributeValuesWait() after the asynchronous call completed, that is
            /// after MLIdentityRequestAttributeValuesWait() returns any value that is not MLResult.Code.Pending.
            /// </param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully and
            /// out_future points to an allocated MLInvokeFuture. If any other value is returned, the location pointed to
            /// by out_future is set to 0 (null)
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToLocalService</c> if the local service is not running, or it cannot be accessed
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToCloudService</c> if there is no IP connection or the cloud service is not available.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCloudAuthentication</c> if the user does not have the required privileges to use the requesting service.or the refresh token used by the service is invalid.
            /// MLResult.Result will be <c>MLResult.Code.IdentityInvalidInformationFromCloud</c> if the signature verification failed on the information returned by the cloud or a parsing error occurred.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNotLoggedIn</c> if the user is not logged in to the cloud.
            /// MLResult.Result will be <c>MLResult.Code.IdentityExpiredCredentials</c> if the user's credentials have expired.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToGetUserProfile</c> if failed to retrieve attributes of the user's profile.
            /// MLResult.Result will be <c>MLResult.Code.IdentityUnauthorized</c> if the user is not authorized to execute the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCertificateError</c> if the device failed to authenticate the server.
            /// MLResult.Result will be <c>MLResult.Code.IdentityRejectedByCloud</c> if the cloud rejected the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityAlreadyLoggedIn</c> if the user is already logged in.
            /// MLResult.Result will be <c>MLResult.Code.IdentityModifyIsNotSupported</c> if the cloud does not support modification of an attribute value.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNetworkError</c> if the device is not connected to a network.
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLIdentityRequestAttributeValuesAsync(IntPtr profile, ref IntPtr outFuture);

            /// <summary>
            /// Having made a call to MLIdentityRequestAttributeValuesAsync(), the user can call
            /// MLIdentityRequestAttributeValuesWait() to detect whether the asynchronous call completed.
            /// The call to MLIdentityRequestAttributeValuesWait() blocks until either <c>msecTimeout</c> elapses
            /// or the asynchronous function completes.
            /// To poll without blocking use <c>msecTimeout</c> = 0.
            /// After the function returns a result other than MLResult.Code.Pending, future is freed by the
            /// library and must not be used again in a call to MLIdentityRequestAttributeValuesWait().
            /// </summary>
            /// <param name="future"> The pointer returned by the MLIdentityRequestAttributeValuesAsync() function.</param>
            /// <param name="msecTimeout">The timeout in milliseconds.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully before the timeout elapsed.
            /// The profile provided in the MLIdentityRequestAttributeValuesAsync call was updated as follows:
            /// Attributes for which the IsRequested field is true, that are still available in the cloud and
            /// which the user has approved to make available for the calling service will have their values filled.
            /// The IsGranted field will be set by the library to true in those attributes that were filled and false in the others.
            /// If any other value is returned, the profile provided in the
            /// MLIdentityRequestAttributeValuesAsync() call was not updated.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the timeout elapsed before the asynchronous call completed.
            /// MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the operation failed to allocate memory.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToLocalService</c> if the local service is not running, or it cannot be accessed
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToConnectToCloudService</c> if there is no IP connection or the cloud service is not available.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCloudAuthentication</c> if the user does not have the required privileges to use the requesting service.or the refresh token used by the service is invalid.
            /// MLResult.Result will be <c>MLResult.Code.IdentityInvalidInformationFromCloud</c> if the signature verification failed on the information returned by the cloud or a parsing error occurred.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNotLoggedIn</c> if the user is not logged in to the cloud.
            /// MLResult.Result will be <c>MLResult.Code.IdentityExpiredCredentials</c> if the user's credentials have expired.
            /// MLResult.Result will be <c>MLResult.Code.IdentityFailedToGetUserProfile</c> if failed to retrieve attributes of the user's profile.
            /// MLResult.Result will be <c>MLResult.Code.IdentityUnauthorized</c> if the user is not authorized to execute the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityCertificateError</c> if the device failed to authenticate the server.
            /// MLResult.Result will be <c>MLResult.Code.IdentityRejectedByCloud</c> if the cloud rejected the operation.
            /// MLResult.Result will be <c>MLResult.Code.IdentityAlreadyLoggedIn</c> if the user is already logged in.
            /// MLResult.Result will be <c>MLResult.Code.IdentityModifyIsNotSupported</c> if the cloud does not support modification of an attribute value.
            /// MLResult.Result will be <c>MLResult.Code.IdentityNetworkError</c> if the device is not connected to a network.
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLIdentityRequestAttributeValuesWait(IntPtr future, uint msecTimeout);

            /// <summary>
            /// MLIdentityReleaseUserProfile() releases all resources associated with the
            /// MLIdentity.Profile structure that was returned by the library.
            /// </summary>
            /// <param name="profile">
            /// A pointer to a library allocated structure received using one of:
            /// MLIdentityGetAttributeNames(), MLIdentityGetAttributeNamesWait() or MLIdentityGetKnownAttributeNames().
            /// </param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if there was an invalid parameter.
            /// </returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLIdentityReleaseUserProfile(IntPtr profile);

            /// <summary>
            /// Gets the result string for an MLIdentity related MLResult.Code.
            /// Developers should use MLResult.CodeToString(MLResult.Code).
            /// </summary>
            /// <param name="result">The MLResult.Code to be requested.</param>
            /// <returns>A pointer to the result string.</returns>
            [DllImport(MLIdentityDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MLIdentityGetResultString(MLResult.Code result);

            /// <summary>
            /// Internal raw representation of the platform's MLIdentity.Profile.
            /// ProfileNative represents a set of attributes of a user's profile.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ProfileNative
            {
                /// <summary>
                /// AttributeCount is the number of MLIdentity.Attribute structures pointed
                /// to by <c>AttributePtrs</c>.
                /// </summary>
                public uint AttributeCount;

                /// <summary>
                /// An array of MLIdentity.Attribute structures containing user profile.
                /// </summary>
                public IntPtr AttributePtrs;
            }
        }
    }
}

#endif
