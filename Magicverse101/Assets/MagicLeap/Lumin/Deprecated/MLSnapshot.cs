// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLSnapshot.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// Identifier of Magic Leap API results
    /// </summary>
    [Obsolete("Please use MLResult.Code instead.", false)]
    public enum MLResultCode : int
    {
        ///
        /// MLResultGlobal
        ///
        /// <summary/>
        Ok = (0 << 16),
        /// <summary/>
        Pending,
        /// <summary/>
        Timeout,
        /// <summary/>
        Locked,
        /// <summary/>
        UnspecifiedFailure,
        /// <summary/>
        InvalidParam,
        /// <summary/>
        AllocFailed,
        /// <summary/>
        PrivilegeDenied,
        /// <summary/>
        NotImplemented,
        /// <summary/>
        NotCompatible,
        ///
        /// MLAudioResult
        ///
        /// <summary/>
        AudioNotImplemented = (0x9e11 << 16),
        /// <summary/>
        AudioHandleNotFound,
        /// <summary/>
        AudioInvalidSampleRate,
        /// <summary/>
        AudioInvalidBitsPerSample,
        /// <summary/>
        AudioInvalidValidBits,
        /// <summary/>
        AudioInvalidSampleFormat,
        /// <summary/>
        AudioInvalidChannelCount,
        /// <summary/>
        AudioInvalidBufferSize,
        /// <summary/>
        AudioBufferNotReady,
        /// <summary/>
        AudioFileNotFound,
        /// <summary/>
        AudioFileNotRecognized,
        /// <summary/>
        AudioResourceNotFound,
        /// <summary/>
        AudioResourceDiscarded,
        /// <summary/>
        AudioOperationUnavailable,
        /// <summary/>
        AudioInternalConfigError,
        ///
        /// MLMediaPlayerResult
        ///
        /// <summary/>
        MediaPlayerServerDied = (0xc435 << 16),
        /// <summary/>
        MediaPlayerNotValidForProgressivePlayback,
        ///
        /// MLMediaResult
        ///
        /// <summary/>
        MediaNotConnected = (0x4184 << 16),
        /// <summary/>
        MediaUnknownHost,
        /// <summary/>
        MediaCannotConnect,
        /// <summary/>
        MediaIO,
        /// <summary/>
        MediaConnectionLost,
        /// <summary/>
        MediaLegacy1,
        /// <summary/>
        MediaMalformed,
        /// <summary/>
        MediaOutOfRange,
        /// <summary/>
        MediaBufferTooSmall,
        /// <summary/>
        MediaUnsupported,
        /// <summary/>
        MediaEndOfStream,
        /// <summary/>
        MediaFormatChanged,
        /// <summary/>
        MediaDiscontinuity,
        /// <summary/>
        MediaOutputBuffersChanged,
        /// <summary/>
        MediaPermissionRevoked,
        /// <summary/>
        MediaUnsupportedAudioFormat,
        /// <summary/>
        MediaHeartbeatTerminateRequested,
        ///
        /// MLMediaDRMResult
        ///
        /// <summary/>
        MediaDRMUnknown = (0x62ce << 16),
        /// <summary/>
        MediaDRMNoLicense,
        /// <summary/>
        MediaDRMLicenseExpired,
        /// <summary/>
        MediaDRMSessionNotOpened,
        /// <summary/>
        MediaDRMDecryptUnitNotInitialized,
        /// <summary/>
        MediaDRMDecrypt,
        /// <summary/>
        MediaDRMCannotHandle,
        /// <summary/>
        MediaDRMTamperDetect,
        /// <summary/>
        MediaDRMNotProvisioned,
        /// <summary/>
        MediaDRMDeviceRevoked,
        /// <summary/>
        MediaDRMResourceBusy,
        /// <summary/>
        MediaDRMInsufficientOutputProtection,
        /// <summary/>
        MediaDRMLastUsedErrorCode = MediaDRMInsufficientOutputProtection,
        /// <summary/>
        MediaDRMVendorMin = (0x62ce << 16) + 500,
        /// <summary/>
        MediaDRMVendorMax = (0x62ce << 16) + 999,
        ///
        /// MLMediaGenericResult
        ///
        /// <summary/>
        MediaGenericInvalidOperation = (0xbf3b << 16),
        /// <summary/>
        MediaGenericBadType,
        /// <summary/>
        MediaGenericNameNotFound,
        /// <summary/>
        MediaGenericHandleNotFound,
        /// <summary/>
        MediaGenericNoInit,
        /// <summary/>
        MediaGenericAlreadyExists,
        /// <summary/>
        MediaGenericDeadObject,
        /// <summary/>
        MediaGenericFailedTransaction,
        /// <summary/>
        MediaGenericBadIndex,
        /// <summary/>
        MediaGenericNotEnoughData,
        /// <summary/>
        MediaGenericWouldBlock,
        /// <summary/>
        MediaGenericUnknownTransaction,
        /// <summary/>
        MediaGenericFDSNotAllowed,
        /// <summary/>
        MediaGenericUnexpectedNull,
        ///
        /// MLDispatchResult
        ///
        /// <summary>
        /// Cannot start app.
        /// </summary>
        DispatchCannotStartApp = (0xBBE0 << 16),
        /// <summary>
        /// Invalid packet.
        /// </summary>
        DispatchInvalidPacket,
        /// <summary>
        /// No app found.
        /// </summary>
        DispatchNoAppFound,
        /// <summary>
        /// App packet dialog failure.
        /// </summary>
        DispatchAppPickerDialogFailure,
        /// <summary>
        /// Invalid Url.
        /// </summary>
        DispatchInvalidUrl,
        /// <summary>
        /// Schema Already Registered.
        /// </summary>
        DispatchSchemaAlreadyRegistered,
        ///
        /// MLIdentityResult
        ///
        /// <summary>
        /// The local service is not running, or it cannot be accessed.
        /// </summary>
        IdentityFailedToConnectToLocalService = (0x7d4d << 16),
        /// <summary>
        /// The service failed to access the cloud service.
        /// Either there is no IP connection or the cloud service is not available.
        /// </summary>
        IdentityFailedToConnectToCloudService,
        /// <summary>
        /// The user does not have the required privileges to use the requesting service
        /// or the refresh token used by the service is invalid.
        /// </summary>
        IdentityCloudAuthentication,
        /// <summary>
        /// Signature verification failed on the information returned by the cloud or a
        /// parsing error occurred.
        /// </summary>
        IdentityInvalidInformationFromCloud,
        /// <summary>
        /// The operation failed because the user is not logged in to the cloud.
        /// </summary>
        IdentityNotLoggedIn,
        /// <summary>
        /// The user's credentials have expired.
        /// </summary>
        IdentityExpiredCredentials,
        /// <summary>
        /// Failed to retrieve attributes of the user's profile.
        /// </summary>
        IdentityFailedToGetUserProfile,
        /// <summary>
        /// The cloud rejected the operation because the user is not authorized to execute it.
        /// </summary>
        IdentityUnauthorized,
        /// <summary>
        /// The device failed to authenticate the server.
        /// </summary>
        IdentityCertificateError,
        /// <summary>
        /// The cloud rejected the operation.
        /// </summary>
        IdentityRejectedByCloud,
        /// <summary>
        /// The user is already logged in.
        /// </summary>
        IdentityAlreadyLoggedIn,
        /// <summary>
        /// The cloud does not support modification of an attribute value.
        /// </summary>
        IdentityModifyIsNotSupported,
        /// <summary>
        /// The device is not connected to a network.
        /// </summary>
        IdentityNetworkError,
        ///
        /// MLPassableWorldResult
        ///
        /// <summary/>
        PassableWorldLowMapQuality = (0x41c7 << 16),
        /// <summary/>
        PassableWorldUnableToLocalize,
        /// <summary/>
        PassableWorldServerUnavailable,
        ///
        /// MLScreensResult
        ///
        /// <summary/>
        [Obsolete("Deprecated and scheduled for removal.", true)]
        ScreensServiceNotAvailable = (0xFB4E << 16),
        /// <summary/>
        [Obsolete("Deprecated and scheduled for removal.", true)]
        PermissionDenied,
        /// <summary/>
        [Obsolete("Deprecated and scheduled for removal.", true)]
        InvalidScreenId,
        ///
        /// MLTokenAgentResult
        ///
        /// <summary>
        /// The local binder service was not found.
        /// </summary>
        TokenAgentFailedToConnectToLocalService = (0x37ee << 16),
        /// <summary>
        /// The local binder service is running but has not been registered by the login service.
        /// </summary>
        TokenAgentServiceNotStarted,
        /// <summary>
        /// The local binder service failed to connect to the cloud service.
        /// </summary>
        TokenAgentFailedToConnectToCloud,
        /// <summary>
        /// The cloud service rejected the request due to inappropriate credentials.
        /// </summary>
        TokenAgentCloudAuthentication,
        /// <summary>
        /// The local server failed to log in with the cloud.
        /// </summary>
        TokenAgentFailedToLogin,
        /// <summary>
        /// An attempt to complete the login was begun without starting the login.
        /// </summary>
        TokenAgentLoginNotBegun,
        /// <summary>
        /// The operation was rejected because the local server was not logged in with the cloud.
        /// </summary>
        TokenAgentNotLoggedIn,
        /// <summary>
        /// The login request failed because the local server is already logged in with the cloud.
        /// </summary>
        TokenAgentAlreadyLoggedIn,
        /// <summary>
        /// The login request request failed because a login is already in progress.
        /// </summary>
        TokenAgentLoginInProgress,
        /// <summary>
        /// Having completed the login process with the cloud successfully, the service failed to
        /// start the local identity service.
        /// </summary>
        TokenAgentFailedToStartIdentityService,
        /// <summary>
        /// The serial number of the device is not recognized by the cloud so login failed.
        /// </summary>
        TokenAgentDeviceNotRegistered,
        /// <summary>
        /// The device is not yet connected to the cloud so login failed.
        /// </summary>
        TokenAgentDeviceNotConnected,
        ///
        /// MLSnapshotResult
        ///
        /// <summary/>
        SnapshotPoseNotFound = (0x87b8 << 16),
        ///
        /// MLPurchaseResult
        ///
        /// <summary/>
        PurchaseUserCancelled = (0xdf1d << 16),
        /// <summary/>
        PurchaseItemAlreadyPurchased,
        /// <summary/>
        PurchasePaymentSourceError,
        /// <summary/>
        PurchaseInvalidPurchaseToken,
        /// <summary/>
        PurchaseItemUnavailable,
        /// <summary/>
        PurchaseNoDefaultPayment,
        ///
        /// MLCloudResult
        ///
        /// <summary/>
        CloudNotFound = (0xc4e3 << 16),
        /// <summary/>
        CloudServerError,
        /// <summary/>
        CloudNetworkError,
        /// <summary/>
        CloudSystemError,
        /// <summary/>
        CloudInvalidHandle,
        /// <summary/>
        CloudBadHandleState,
        ///
        /// MLPrivilegesResult
        ///
        /// <summary>
        /// Used for MLPrivileges check and request privilege
        /// functions. Indicates if the user has decided to
        /// grant the privilege to the application.
        /// </summary>
        PrivilegeGranted = (0xcbcd << 16),
        /// <summary>
        /// Used for MLPrivileges check and request privilege
        /// functions. Indicates whether a privilege has not
        /// yet been requested or if the user has decided not
        /// to grant the privilege to the application.
        /// </summary>
        PrivilegeNotGranted,
        ///
        /// MLContactsResult
        ///
        /// <summary/>
        ContactsHandleNotFound = (0x94a0 << 16),
        /// <summary/>
        ContactsCompleted,
        /// <summary/>
        ContactsCancelled,
        /// <summary/>
        ContactsIllegalState,
        ///
        /// MLLocationResult
        ///
        /// <summary>
        /// Unknown location error.
        /// </summary>
        LocationUnknown = (0xda19 << 16),
        /// <summary>
        /// No connection to server.
        /// </summary>
        LocationNetworkConnection,
        /// <summary>
        /// No location data received.
        /// </summary>
        LocationNoLocation,
        /// <summary>
        /// Location provider is not found.
        /// </summary>
        LocationProviderNotFound,
        ///
        /// MLNetworkingResult
        ///
        /// <summary>
        /// The corresponding service is not available.
        /// </summary>
        NetworkingServiceNotAvailable = (0x4c62 << 16),
        /// <summary>
        /// The corresponding service returned with error.
        /// </summary>
        NetworkingServiceError,
        /// <summary>
        /// The version number in MLNetworkingWiFiData is not recognized.
        /// </summary>
        NetworkingWiFiDataStructureVersionError,
        /// <summary>
        /// WiFi service is not in the right state.
        /// </summary>
        NetworkingWiFiServiceInvalidState,
        ///
        /// MLMovementResult
        ///
        /// <summary>
        /// Not a valid MLHandle for movement session.
        /// </summary>
        MovementInvalidMovementHandle = (0xdffe << 16),
        /// <summary>
        /// Not a valid MLHandle for a collision session.
        /// </summary>
        MovementInvalidCollisionHandle,
        ///
        /// MLConnectionsResult
        ///
        /// <summary>
        /// This MLHandle is not recognized.
        /// </summary>
        ConnectionsInvalidHandle = (0xbfae << 16),
        /// <summary>
        /// Indicates number of invitees is invalid.
        /// </summary>
        ConnectionsInvalidInviteeCount,
        /// <summary>
        /// Indicates number of selectees is invalid.
        /// </summary>
        ConnectionsInvalidSelecteeCount = ConnectionsInvalidInviteeCount,
        /// <summary>
        /// Indicates invite request has been found and the system is attempting to cancel the process.
        /// </summary>
        ConnectionsCancellationPending,
        /// <summary>
        /// Request failed due to system being in an illegal state,
        /// e.g. user has not successfully logged-in.
        /// </summary>
        ConnectionsIllegalState,
        /// <summary>
        /// MLConnectionsRegisterForInvite failed because the system had an error
        /// with network connectivity, or the servers could not be reached.
        /// </summary>
        ConnectionsNetworkFailure,
        /// <summary>
        /// MLConnectionsRegisterForInvite failed because the application is already registered to handle invite requests.
        /// </summary>
        ConnectionsAlreadyRegistered,
        /// <summary>
        /// Indicates a general system failure
        /// </summary>
        ConnectionsSystemFailure
    }

    #if PLATFORM_LUMIN

    /// <summary>
    /// Magic Leap utility perception snapshot functions.
    /// </summary>
    [Obsolete("Please use MLResult.CodeToString(MLResult.Code) instead.", true)]
    public struct MLSnapshot
    {
        /// <summary>
        /// Gets a readable version of the result code from snapshot internal calls as an ASCII string.
        /// </summary>
        /// <param name="result">The MLResult that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        [Obsolete("Please use MLResult.CodeToString(MLResult.Code) instead.", true)]
        public static string GetResultString(MLResultCode result)
        {
            return "This function is deprecated. Use MLResult.CodeToString(MLResult.Code) instead.";
        }
    }
    #endif
}
