// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLPrivilegeIds.cs" company="Magic Leap, Inc">
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
    /// Privilege Types:
    /// AutoGranted - Once it is included in the manifest the application is granted the privilege.
    /// Sensitive - Must be requested at runtime, as well as in the manifest, because it requires user consent on first use.
    /// Reality - Must be requested at runtime and every time the application gains focus, as well as in the manifest, because it requires user consent every active session.
    /// </summary>
    [System.Obsolete("Please use MLPrivileges.Id instead.", true)]
    public enum MLPrivilegeId : uint
    {
        /// <summary>
        /// Invalid privilege
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Full read and search access to address book contacts.
        /// Type: Sensitive
        /// </summary>
        AddressBookRead = 1,

        /// <summary>
        /// Ability to add, modify and delete address book contacts.
        /// Type: Sensitive
        /// </summary>
        AddressBookWrite = 2,

        /// <summary>
        /// Deprecated and scheduled for removal.
        /// Type: Reality
        /// </summary>
        [System.Obsolete("Deprecated and scheduled for removal.", true)]
        AudioRecognizer = 13,

        /// <summary>
        /// Query battery status/percentage.
        /// Type: AutoGranted
        /// </summary>
        BatteryInfo = 18,

        /// <summary>
        /// Take pictures and videos using camera.
        /// Type: Reality
        /// </summary>
        CameraCapture = 26,

        /// <summary>
        /// Access dense map.
        /// Type: AutoGranted
        /// </summary>
        WorldReconstruction = 33,

        /// <summary>
        /// Use the in-app purchase mechanism.
        /// Type: AutoGranted
        /// </summary>
        InAppPurchase = 42,

        /// <summary>
        /// Open a microphone stream of the users voice or the ambient surroundings.
        /// Type: Reality
        /// </summary>
        AudioCaptureMic = 49,

        /// <summary>
        /// Provision and use DRM certficates.
        /// Type: AutoGranted
        /// </summary>
        DrmCertificates = 51,

        /// <summary>
        /// Deprecated and scheduled for removal.
        /// Type: AutoGranted
        /// </summary>
        [System.Obsolete("Deprecated and scheduled for removal.", true)]
        Occlusion = 52,

        /// <summary>
        /// Access Low Latency data from the Lightwear.
        /// Type: AutoGranted
        /// </summary>
        LowLatencyLightwear = 59,

        /// <summary>
        /// Access the internet.
        /// Type: AutoGranted
        /// </summary>
        Internet = 96,

        /// <summary>
        /// Read user profile attributes.
        /// Type: AutoGranted
        /// </summary>
        IdentityRead = 113,

        /// <summary>
        /// Download in the background.
        /// Type: AutoGranted
        /// </summary>
        BackgroundDownload = 120,

        /// <summary>
        /// Upload in the background.
        /// Type: AutoGranted
        /// </summary>
        BackgroundUpload = 121,

        /// <summary>
        /// Get power information.
        /// Type: AutoGranted
        /// </summary>
        PowerInfo = 150,

        /// <summary>
        /// Access other entities on the local network.
        /// Type: Sensitive
        /// </summary>
        LocalAreaNetwork = 171,

        /// <summary>
        /// Receive voice input.
        /// Type: Reality
        /// </summary>
        VoiceInput = 173,

        /// <summary>
        /// Connect to Background Music Service.
        /// Type: AutoGranted
        /// </summary>
        ConnectBackgroundMusicService = 192,

        /// <summary>
        /// Register with Background Music Service.
        /// Type: AutoGranted
        /// </summary>
        RegisterBackgroundMusicService = 193,

        /// <summary>
        /// Read found objects from Passable World.
        /// Type: AutoGranted
        /// </summary>
        [System.Obsolete("Deprecated and scheduled for removal.", true)]
        PwFoundObjRead = 201,

        /// <summary>
        /// Read found objects from Passable World.
        /// Type: AutoGranted
        /// </summary>
        PcfRead = 201,

        /// <summary>
        /// Post notifications for users to see, dismiss own notifications, listen for own notification events.
        /// Type: AutoGranted
        /// </summary>
        NormalNotificationsUsage = 208,

        /// <summary>
        /// Access Music Service functionality.
        /// Type: AutoGranted
        /// </summary>
        MusicService = 218,

        /// <summary>
        /// Access controller pose data.
        /// Type: AutoGranted
        /// </summary>
        ControllerPose = 263,

        /// <summary>
        /// Create channels in the screens framework'.
        /// Type: AutoGranted
        /// </summary>
        [System.Obsolete("Deprecated and scheduled for removal.", true)]
        ScreensProvider = 264,

        /// <summary>
        /// Subscribe to gesture hand mask and config data.
        /// Type: AutoGranted
        /// </summary>
        GesturesSubscribe = 268,

        /// <summary>
        /// Set/Get gesture configuration.
        /// Type: AutoGranted
        /// </summary>
        GesturesConfig = 269,

        /// <summary>
        /// Access a manually selected subset of contacts from address book.
        /// Type: AutoGranted
        /// </summary>
        AddressBookBasicAccess = 305,

        /// <summary>
        /// Access hand mesh features.
        /// Type: AutoGranted
        /// </summary>
        HandMesh = 315,

        /// <summary>
        /// Get coarse location of the device.
        /// Type: Sensitive
        /// </summary>
        CoarseLocation = 323,

        /// <summary>
        /// Ability to initiate invites to Social connections.
        /// Type: AutoGranted
        /// </summary>
        SocialConnectionsInvitesAccess = 329,

        /// <summary>
        /// SDK access CV related info from graph_pss.
        /// Type: Reality
        /// </summary>
        ComputerVision = 343,

        /// <summary>
        /// Get Wifi status to application.
        /// Type: AutoGranted
        /// </summary>
        WifiStatusRead = 344,

        /// <summary>
        /// Request a secure browser window.
        /// Type: AutoGranted
        /// </summary>
        SecureBrowserWindow = 357,

        /// <summary>
        /// Get fine location of the device.
        /// Type: Reality
        /// </summary>
        FineLocation = 367
    }

    /// <summary>
    /// Privilege ids that need to be requested at runtime in addition
    /// to being specified in the app manifest. Descriptions for each
    /// value can be found in MLPrivilegeId enum. This is the subset of
    /// runtime privileges applicable to Unity apps.
    /// </summary>
    [System.Obsolete("Please use MLPrivileges.RuntimeRequestId instead.", true)]
    public enum MLRuntimeRequestPrivilegeId : uint
    {
        /// <summary/>
        AudioCaptureMic = MLPrivilegeId.AudioCaptureMic,

        /// <summary/>
        CameraCapture = MLPrivilegeId.CameraCapture,

        /// <summary/>
        LocalAreaNetwork = MLPrivilegeId.LocalAreaNetwork,

        /// <summary/>
        PcfRead = MLPrivilegeId.PcfRead,

        /// <summary/>
        AddressBookRead = MLPrivilegeId.AddressBookRead,

        /// <summary/>
        AddressBookWrite = MLPrivilegeId.AddressBookWrite,

        /// <summary/>
        CoarseLocation = MLPrivilegeId.CoarseLocation,

        /// <summary/>
        ComputerVision = MLPrivilegeId.ComputerVision,

        /// <summary/>
        FineLocation = MLPrivilegeId.FineLocation
    }
}
