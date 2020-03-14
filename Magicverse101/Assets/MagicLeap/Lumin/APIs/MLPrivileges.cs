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

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Functionality to validate or query privileges from the system.
    /// </summary>
    public sealed partial class MLPrivileges : MLAPISingleton<MLPrivileges>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// A dictionary of pending privilege requests.
        /// </summary>
        private Dictionary<MLPrivileges.Id, RequestPrivilegeQuery> currentRequests = new Dictionary<MLPrivileges.Id, RequestPrivilegeQuery>();

        /// <summary>
        /// Prevents a default instance of the <see cref="MLPrivileges"/> class from being created.
        /// </summary>
        private MLPrivileges()
        {
            this.DllNotFoundError = "MLPrivileges API is currently available only on device.";
        }

        /// <summary>
        /// The callback delegate for privilege requests.
        /// </summary>
        /// <param name="result">The result code of the privilege callback request.</param>
        /// <param name="id">The privilege id that was requested.</param>
        public delegate void CallbackDelegate(MLResult result, MLPrivileges.Id id);
        #endif

        /// <summary>
        /// Privilege Types:
        /// AutoGranted - Once it is included in the manifest the application is granted the privilege.
        /// Sensitive - Must be requested at runtime, as well as in the manifest, because it requires user consent on first use.
        /// Reality - Must be requested at runtime and every time the application gains focus, as well as in the manifest, because it requires user consent every active session.
        /// </summary>
        public enum Id : uint
        {
            /// <summary>
            /// An invalid Id.
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
            /// Provision and use DRM certificates.
            /// Type: AutoGranted
            /// </summary>
            DrmCertificates = 51,

            /// <summary>
            /// Access Low Latency data from the <c>Lightwear</c>.
            /// Type: AutoGranted
            /// </summary>
            LowLatencyLightwear = 59,

            /// <summary>
            /// Access the internet.
            /// Type: AutoGranted
            /// </summary>
            Internet = 96,

            /// <summary>
            /// Bluetooth Adapter User
            /// Type: AutoGranted
            /// </summary>
            BluetoothAdapterUser = 106,

            /// <summary>
            /// Bluetooth Gatt Client Write
            /// Type: AutoGranted
            /// </summary>
            BluetoothGattWrite = 108,

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
            PcfRead = 201,

            /// <summary>
            /// Read found objects from Passable World.
            /// Type: AutoGranted
            /// </summary>
            [System.Obsolete("Deprecated and scheduled for removal.", true)]
            PwFoundObjRead = 201,

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
            /// SDK access CV related info from <c>graph_pss</c>.
            /// Type: Reality
            /// </summary>
            ComputerVision = 343,

            /// <summary>
            /// Get <c>Wifi</c> status to application.
            /// Type: AutoGranted
            /// </summary>
            WifiStatusRead = 344,

            /// <summary>
            /// Access ACP connection APIs.
            /// Type: Reality
            /// </summary>
            ConnectionAccess = 350,

            /// <summary>
            /// Access ACP connection audio API.
            /// Type: Reality
            /// </summary>
            ConnectionAudioCaptureStreaming = 351,

            /// <summary>
            /// Access ACP connection video API.
            /// Type: Reality
            /// </summary>
            ConnectionVideoCaptureStreaming = 352,

            /// <summary>
            /// Request a secure browser window.
            /// Type: AutoGranted
            /// </summary>
            SecureBrowserWindow = 357,

            /// <summary>
            /// Access to Bluetooth adapter from external app
            /// Type: AutoGranted
            /// </summary>
            BluetoothAdapterExternalApp = 362,

            /// <summary>
            /// Get fine location of the device.
            /// Type: Reality
            /// </summary>
            FineLocation = 367,

            /// <summary>
            /// Select access to social connections.
            /// Type: AutoGranted
            /// </summary>
            SocialConnectionsSelectAccess = 372,

            /// <summary>
            /// Access found object data from object-recognition pipeline.
            /// Type: Sensitive
            /// </summary>
            ObjectData = 394
        }

        /// <summary>
        /// Privilege ids that need to be requested at runtime in addition
        /// to being specified in the app manifest. Descriptions for each
        /// value can be found in MLPrivilege.Id enum. This is the subset of
        /// runtime privileges applicable to Unity apps.
        /// </summary>
        public enum RuntimeRequestId : uint
        {
            /// <summary>
            /// The Audio Capture Microphone privilege.
            /// </summary>
            AudioCaptureMic = MLPrivileges.Id.AudioCaptureMic,

            /// <summary>
            /// The Camera Capture privilege.
            /// </summary>
            CameraCapture = MLPrivileges.Id.CameraCapture,

            /// <summary>
            /// The Local Area Network privilege.
            /// </summary>
            LocalAreaNetwork = MLPrivileges.Id.LocalAreaNetwork,

            /// <summary>
            /// The obsolete PCF Read privilege.
            /// </summary>
            [System.Obsolete("PcfRead/PwFoundObjRead are now AutoGranted and do not need to be requested at runtime.", true)]
            PcfRead = MLPrivileges.Id.PcfRead,

            /// <summary>
            /// The Address Book Read privilege.
            /// </summary>
            AddressBookRead = MLPrivileges.Id.AddressBookRead,

            /// <summary>
            /// The Address Book Write privilege.
            /// </summary>
            AddressBookWrite = MLPrivileges.Id.AddressBookWrite,

            /// <summary>
            /// The Coarse Location privilege.
            /// </summary>
            CoarseLocation = MLPrivileges.Id.CoarseLocation,

            /// <summary>
            /// The Computer Vision privilege.
            /// </summary>
            ComputerVision = MLPrivileges.Id.ComputerVision,

            /// <summary>
            /// The Fine Location privilege.
            /// </summary>
            FineLocation = MLPrivileges.Id.FineLocation,

            /// <summary>
            /// Access ACP connection audio API.
            /// Type: Reality
            /// </summary>
            ConnectionAudioCaptureStreaming = MLPrivileges.Id.ConnectionAudioCaptureStreaming,

            /// <summary>
            /// Access ACP connection video API.
            /// Type: Reality
            /// </summary>
            ConnectionVideoCaptureStreaming = MLPrivileges.Id.ConnectionVideoCaptureStreaming,

            /// <summary>
            /// Access found object data from object-recognition pipeline.
            /// Type: Sensitive
            /// </summary>
            ObjectData = MLPrivileges.Id.ObjectData
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Privileges API, must be called
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the privilege system startup succeeded.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the privilege system failed to startup.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLPrivileges.BaseStart();
        }

        /// <summary>
        /// Checks whether the application has the specified privileges.
        /// This does not solicit consent from the end-user.
        /// </summary>
        /// <param name="privilegeId">The privilege to check for access.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeGranted</c> if the privilege is granted.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeNotGranted</c> if the privilege is denied.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the privilege system was not started.
        /// </returns>
        public static MLResult CheckPrivilege(MLPrivileges.Id privilegeId)
        {
            try
            {
                if (MLPrivileges.IsValidInstance())
                {
                    MLResult.Code checkPrivilegeResult = NativeBindings.MLPrivilegesCheckPrivilege(privilegeId);
                    return (checkPrivilegeResult == MLResult.Code.PrivilegeNotGranted) ? MLResult.Create(checkPrivilegeResult, "Privilege Denied or Not Yet Requested.") : MLResult.Create(checkPrivilegeResult);
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLPrivileges.CheckPrivilege failed. Reason: No Instance for MLPrivileges.");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPrivileges.CheckPrivilege failed. Reason: No Instance for MLPrivileges.");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLPrivileges.CheckPrivilege failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPrivileges.CheckPrivilege failed. Reason: API symbols not found.");
            }
        }

        /// <summary>
        /// Requests the specified privileges. This may possibly solicit consent from the end-user.
        /// </summary>
        /// <param name="privilegeId">The privilege to request.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeGranted</c> if the privilege is granted.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeNotGranted</c> if the privilege is denied.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the privilege system was not started.
        /// </returns>
        public static MLResult RequestPrivilege(MLPrivileges.Id privilegeId)
        {
            try
            {
                if (MLPrivileges.IsValidInstance())
                {
                    MLResult.Code requestPrivilegeResult = NativeBindings.MLPrivilegesRequestPrivilege(privilegeId);

                    MLResult result = MLResult.Create(requestPrivilegeResult);

                    if (result.Result != MLResult.Code.PrivilegeGranted)
                    {
                        MLPluginLog.ErrorFormat("MLPrivileges.RequestPrivilege failed to request {0}. Reason: {1}", privilegeId, result);
                    }

                    return result;
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLPrivileges.RequestPrivilege failed. Reason: No Instance for MLPrivileges.");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPrivileges.RequestPrivilege failed. Reason: No Instance for MLPrivileges.");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLPrivileges.RequestPrivilege failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPrivileges.RequestPrivilege failed. Reason: API symbols not found.");
            }
        }

        /// <summary>
        /// Request the specified privileges. This may solicit consent from the end-user.
        /// Note: The asynchronous callback occurs within the main thread.
        /// </summary>
        /// <param name="privilegeId">The privilege to request.</param>
        /// <param name="callback">Callback to be executed when the privilege request has completed.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the privilege request is in progress.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the callback is null.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the privilege system was not started.
        /// Callback MLResult.Result will be <c>MLResult.Code.PrivilegeGranted</c> if the privilege is granted.
        /// Callback MLResult.Result will be <c>MLResult.Code.PrivilegeNotGranted</c> if the privilege is denied.
        /// </returns>
        public static MLResult RequestPrivilegeAsync(MLPrivileges.Id privilegeId, CallbackDelegate callback)
        {
            try
            {
                if (MLPrivileges.IsValidInstance())
                {
                    if (callback == null)
                    {
                        return MLResult.Create(MLResult.Code.InvalidParam, "MLPrivileges.RequestPrivilegeAsync failed. Reason: Must send a valid callback.");
                    }

                    if (!_instance.currentRequests.ContainsKey(privilegeId))
                    {
                        IntPtr newRequest = IntPtr.Zero;

                        MLResult.Code resultCode = NativeBindings.MLPrivilegesRequestPrivilegeAsync(privilegeId, ref newRequest);
                        if (resultCode == MLResult.Code.Ok)
                        {
                            RequestPrivilegeQuery newQuery = new RequestPrivilegeQuery(callback, newRequest, privilegeId);
                            _instance.currentRequests.Add(privilegeId, newQuery);
                        }

                        return MLResult.Create(resultCode);
                    }
                    else
                    {
                        return MLResult.Create(MLResult.Code.Ok);
                    }
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLPrivileges.RequestPrivilegeAsync failed. Reason: No Instance for MLPrivileges.");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPrivileges.RequestPrivilegeAsync failed. Reason: No Instance for MLPrivileges.");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLPrivileges.RequestPrivilegeAsync failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLPrivileges.RequestPrivilegeAsync failed. Reason: API symbols not found.");
            }
        }

        /// <summary>
        /// Gets the result string for a MLResult.Code.
        /// </summary>
        /// <param name="result">The MLResult.Code to be requested.</param>
        /// <returns>A pointer to the result string.</returns>
        internal static IntPtr GetResultString(MLResult.Code result)
        {
            try
            {
                if (MLPrivileges.IsValidInstance())
                {
                    return NativeBindings.MLPrivilegesGetResultString(result);
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLPrivileges.GetResultString failed. Reason: No Instance for MLPrivileges.");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLPrivileges.GetResultString failed. Reason: API symbols not found");
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Starts the Privileges, Must be called to start checking for privileges at runtime.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the privilege system startup succeeded.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the privilege system failed to startup.
        /// </returns>
        protected override MLResult StartAPI()
        {
            return MLResult.Create(NativeBindings.MLPrivilegesStartup());
        }

        /// <summary>
        /// Cleans up unmanaged memory.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Boolean that tells if it is safe to clear managed memory</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            try
            {
                if (isSafeToAccessManagedObjects)
                {
                    _instance.currentRequests.Clear();
                }

                MLResult.Code resultCode = NativeBindings.MLPrivilegesShutdown();
                if (resultCode != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLPrivileges.CleanupAPI failed to shutdown. Reason: {0}", MLResult.CodeToString(resultCode));
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLPrivileges.CleanupAPI failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Polls for the result of pending privileges requests.
        /// </summary>
        protected override void Update()
        {
            this.ProcessPendingQueries();
        }

        /// <summary>
        /// static instance of the MLPrivileges class
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLPrivileges.IsValidInstance())
            {
                MLPrivileges._instance = new MLPrivileges();
            }
        }

        /// <summary>
        /// Process pending requests and call the callback specified in the startup config.
        /// </summary>
        private void ProcessPendingQueries()
        {
            try
            {
                foreach (var pending in this.currentRequests.OrderByDescending(x => x.Key))
                {
                    MLResult.Code result = NativeBindings.MLPrivilegesRequestPrivilegeTryGet(pending.Value.Request);
                    if (result != MLResult.Code.Pending)
                    {
                        pending.Value.Result = MLResult.Create(result);
                        pending.Value.Callback(pending.Value.Result, pending.Key);

                        this.currentRequests.Remove(pending.Key);
                    }
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLPrivileges.ProcessPendingQueries failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Wrapper for the Async Request
        /// </summary>
        private class RequestPrivilegeQuery
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RequestPrivilegeQuery"/> class.
            /// </summary>
            /// <param name="callback">The callback that should receive the notification.</param>
            /// <param name="request">A pointer to the request.</param>
            /// <param name="privilege">The privilege Id.</param>
            public RequestPrivilegeQuery(CallbackDelegate callback, IntPtr request, MLPrivileges.Id privilege)
            {
                this.Callback = callback;
                this.Result = MLResult.Create(MLResult.Code.Pending);
                this.Request = request;
                this.PrivilegeId = privilege;
            }

            /// <summary>
            /// Gets or sets the query result callback.
            /// </summary>
            public CallbackDelegate Callback { get; set; }

            /// <summary>
            /// Gets or sets the requested privilege id.
            /// </summary>
            public MLPrivileges.Id PrivilegeId { get; set; }

            /// <summary>
            /// Gets or sets The result.
            /// </summary>
            public MLResult Result { get; set; }

            /// <summary>
            /// Gets or sets the Async request <c>IntPtr</c>.
            /// </summary>
            public IntPtr Request { get; set; }
        }
        #endif
    }
}
