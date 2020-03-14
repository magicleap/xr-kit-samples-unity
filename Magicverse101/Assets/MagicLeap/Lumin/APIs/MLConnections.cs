// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLConnections.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// MLConnections class is the entry point for the Connections API
    /// </summary>
    public sealed partial class MLConnections : MLAPISingleton<MLConnections>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// The internal handle attached to this instance of MLConnections invite request.
        /// </summary>
        private ulong requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// The internal handle attached to this instance of MLConnections invite receiving registration.
        /// </summary>
        private ulong registerHandle = Native.MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// The internal handle attached to this instance of MLConnections selection request.
        /// </summary>
        private ulong selectionHandle = Native.MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// Requesting and sending invite registration has completed properly.
        /// </summary>
        private bool sendInviteHasStarted = false;

        /// <summary>
        /// Currently an invite request is being processed.
        /// </summary>
        private bool activeInvite = false;

        /// <summary>
        /// Precautionary check for multiple registrations.
        /// </summary>
        private bool activeRequest = false;

        /// <summary>
        /// Prevents a default instance of the <see cref="MLConnections" /> class from being created.
        /// </summary>
        private MLConnections()
        {
            this.DllNotFoundError = "MLConnections API is currently available only on device.";
        }

        /// <summary>
        /// Callback which is fired when registration is complete.
        /// </summary>
        /// <param name="result">This is the result of the Registration. It will be MLResult.Code.Ok in case of success, and an error otherwise.</param>
        public delegate void RegistrationCompleteDelegate(MLResult result);

        /// <summary>
        /// Callback which is fired every time an invitation comes in.
        /// </summary>
        /// <param name="userConfirmation">This is a boolean value that informs you whether the user has confirmed the invite notification.</param>
        /// <param name="payload">This is the data sent from the inviting application on another device.</param>
        public delegate void InvitationDelegate(bool userConfirmation, string payload);

        /// <summary>
        /// <para>This is the callback that will be called when invitation has been confirmed by the user.</para>
        /// </summary>
        /// <param name="result">The invitation results.</param>
        public delegate void OnInvitationResultDelegate(InvitationResult result);

        /// <summary>
        /// <para>This is the callback that will be called when invitee statuses become available.</para>
        /// <para>When sending invitation using join code this callback is not expected to be triggered, this may change in the future.</para>
        /// </summary>
        /// <param name="status">The invitee status and details.</param>
        public delegate void OnInviteeStatusDelegate(InviteeStatus status);

        /// <summary>
        /// <para>This is the callback that will be called when selection has completed by the user.</para>
        /// </summary>
        /// <param name="status">Status of operation.</param>
        /// <param name="connections">Resultant array of connections.</param>
        public delegate void OnSelectionDelegate(SelectionStatus status, Connection[] connections);

        /// <summary>
        /// This is the callback that will be called when Registration has completed, either successfully or not. Expect it to only be called once.
        /// </summary>
        public static event RegistrationCompleteDelegate OnRegistrationComplete
        {
            add
            {
                NativeBindings.OnRegistrationComplete += value;
            }

            remove
            {
                NativeBindings.OnRegistrationComplete -= value;
            }
        }

        /// <summary>
        /// This is the callback that will be called every time an invitation comes in. If userConfirmation is true, this means the user was shown a
        /// notification and accepted the invitation to launch the application. If false, the application was running already when the request was received
        /// and the application is in charge of gaining user consent.
        /// </summary>
        public static event InvitationDelegate OnInvitation
        {
            add
            {
                NativeBindings.OnInvitation += value;
            }

            remove
            {
                NativeBindings.OnInvitation -= value;
            }
        }

        /// <summary>
        /// This is the callback that will be called when invitation has been confirmed by the user.
        /// </summary>
        public static event OnInvitationResultDelegate OnInvitationResult
        {
            add
            {
                NativeBindings.OnInvitationResult += value;
            }

            remove
            {
                NativeBindings.OnInvitationResult -= value;
            }
        }

        /// <summary>
        /// <para>This is the callback that will be called when invitee statuses become available.</para>
        /// <para>When sending invitation using join code this callback is not expected to be triggered, this may change in the future.</para>
        /// </summary>
        public static event OnInviteeStatusDelegate OnInviteeStatus
        {
            add
            {
                NativeBindings.OnInviteeStatus += value;
            }

            remove
            {
                NativeBindings.OnInviteeStatus -= value;
            }
        }

        /// <summary>
        /// <para>This is the callback that will be called when selection has completed by the user.</para>
        /// </summary>
        public static event OnSelectionDelegate OnSelection
        {
            add
            {
                NativeBindings.OnSelection += value;
            }

            remove
            {
                NativeBindings.OnSelection -= value;
            }
        }
        #endif

        /// <summary>
        /// Defines possible status values for sending invites with MLConnections.SendInvite. Links to MLConnectionsInviteStatus in ml_connections.h.
        /// </summary>
        public enum InviteStatus : uint
        {
            /// <summary>
            /// The request to start the sending process is being submitted to the system.
            /// </summary>
            SubmittingRequest,

            /// <summary>
            /// The sending process has successfully initiated and invite dialog is being displayed to the user.
            /// </summary>
            Pending,

            /// <summary>
            /// The invite dialog has been completed by the user and the invite was successfully sent.
            /// </summary>
            Dispatched,

            /// <summary>
            /// The user has completed the invite dialog but the system was unable to send the invite.
            /// </summary>
            DispatchFailed,

            /// <summary>
            /// The sending process was cancelled either by user, system or by CancelSentInvite
            /// </summary>
            Cancelled,

            /// <summary>
            /// Unable to determine invite status, because provided handle is invalid.
            /// </summary>
            InvalidHandle
        }

        /// <summary>
        /// Defines default filter for Magic Leap connections when displaying invite dialog to the user in MLConnections.SendInvite. Links to MLConnectionsInviteeFilter in ml_connections.h.
        /// </summary>
        public enum InviteeFilter : uint
        {
            /// <summary>
            /// Show Magic Leap connections who are online and follow current user.
            /// </summary>
            Followers,

            /// <summary>
            /// Show Magic Leap connections who are online and are mutual followers for current user.
            /// </summary>
            Mutual
        }

        /// <summary>
        /// Defines default filter for Magic Leap connections when displaying invite dialog to the user MLConnectionsRequestSelection. Links to MLConnectionsSelectionFilter in ml_connections.h.
        /// </summary>
        public enum SelectionFilter : uint
        {
            /// <summary>
            /// Show Magic Leap connections who are online and followed by current user.
            /// </summary>
            Following,

            /// <summary>
            /// Show Magic Leap connections who are online and follow current user.
            /// </summary>
            Followers,

            /// <summary>
            /// Show Magic Leap connections who are online and are mutual followers for current user.
            /// </summary>
            Mutual
        }

        /// <summary>
        /// Defines possible status values for selecting connections using MLConnections.. Links to MLConnectionsSelectionStatus in ml_connections.h.
        /// </summary>
        public enum SelectionStatus : uint
        {
            /// <summary>
            /// Indicates the request to start the sending process has been submitted to the system.
            /// </summary>
            Pending,

            /// <summary>
            /// Indicates that the selection is ready.
            /// </summary>
            Confirmed,

            /// <summary>
            /// Indicates that the selection was cancelled by user action.
            /// </summary>
            Cancelled,

            /// <summary>
            /// Indicates that the selection failed unexpectedly.
            /// </summary>
            Failed
        }

        /// <summary>
        /// Defines invitation status types. Links to MLConnectionsInvitationDeliveryStatus in ml_connections.h.
        /// </summary>
        public enum InvitationDeliveryStatus : uint
        {
            /// <summary>
            /// The invitation request is being processed.
            /// </summary>
            Processing,

            /// <summary>
            /// The invitee was found and the invitation was dispatched.
            /// </summary>
            Dispatched,

            /// <summary>
            /// The invitee is online and the invitation was delivered.
            /// </summary>
            Delivered,

            /// <summary>
            /// A new participant joined.
            /// </summary>
            Joined,

            /// <summary>
            /// A system level failure prevented the delivery of the invitation to this invitee.
            /// </summary>
            Failed
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Start the MLConnections API. This will register and set callbacks for receiving an invite. Once this is called,
        /// the user can receive invites after closing the application.
        /// </summary>
        /// <returns>
        /// <para>MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the registration resource allocation failed.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a given argument is invalid.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully initialized.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsAlreadyRegistered</c> if this is a duplicate registration.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsNetworkFailure</c> if communication to the network failed.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsSystemFailure</c> if there was system failure.</para>
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLConnections.BaseStart();
        }

        /// <summary>
        /// <para>Submit a request to start the invite sending process. Request an invite to be sent for other connections to join a multi-user
        /// experience. This call will trigger a connections invite dialog requesting the user to select up to the specified number of online
        /// users to be invited. The system will then initiate a push notification to other online devices, start a copy of the application
        /// requesting the invite and deliver the given payload.</para>
        /// <para>Only one invite can be processed at a time. Will return an error if the first invite request is incomplete/pending in the service
        /// when a second request is sent.</para>
        /// <para>Will receive a DispatchFailed status if the invitee is not registered to receive invites for the specific application. If multiple invitees
        /// are selected, SendInvite will dispatch successfully if at least one of them is registered to receive invites for the specific application.</para>
        /// <para>Note: You cannot invite users who are not following the Sender.</para>
        /// </summary>
        /// <param name="inviteeCount">Max number of connections to be invited. Min limit is 1.</param>
        /// <param name="filterLevel">Type of filter applied by default to ML connections list in invitee selection dialog.</param>
        /// <param name="textPrompt">Text prompt to be displayed to the user with invitee selection dialog.</param>
        /// <param name="payload">Payload message to be delivered to remote copy of the application with invite.</param>
        /// <returns>
        /// <para>MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a given argument is invalid.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully requested the start of the invite dialog.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if permissions haven't been granted to make this API call.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.Timeout</c> if the request to request sending an invite timed out.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsIllegalState</c> if there was an issue with the connections system, e.g. service is not available for any reason.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsInvalidInviteeCount</c> if number of invitees is invalid.</para>
        /// </returns>
        public static MLResult SendInvite(uint inviteeCount, InviteeFilter filterLevel, string textPrompt, string payload)
        {
            if (MLConnections.IsValidInstance())
            {
                MLResult requestResult = NativeBindings.InviteArgs.SendInviteHelper(inviteeCount, filterLevel, textPrompt, payload, ref _instance.requestHandle);

                if (!requestResult.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLConnections.SendInvite failed to initialize resources for sending an invite. Reason: {0}", requestResult);
                }
                else
                {
                    if (_instance.activeInvite)
                    {
                        MLPluginLog.WarningFormat("MLConnections.SendInvite allowed multiple active invites.");
                    }

                    _instance.activeInvite = true;
                }

                return requestResult;
            }
            else
            {
                MLPluginLog.ErrorFormat("MLConnections.SendInvite failed. Reason: No Instance for MLConnections");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLConnections.SendInvite failed. Reason: No Instance for MLConnections");
            }
        }

        /// <summary>
        /// <para>Submit a request to start the invite sending process with the intention of creating/joining a session by code.</para>
        /// <para>Same functionality as MLConnections.SendInvite but it defaults to 5 max invitee count, the recommended amount,
        /// and Followers invitee filter as these are not needed for joining by code.</para>
        /// <para>When sending an invitation using join code OnInviteeStatus callback is not expected to be triggered.</para>
        /// </summary>
        /// <param name="textPrompt">Text prompt to be displayed to the user with invitee selection dialog.</param>
        /// <param name="payload">Payload message to be delivered to remote copy of the application with invite.</param>
        /// <returns>
        /// <para>MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a given argument is invalid.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully requested the start of the invite dialog.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if permissions haven't been granted to make this API call.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.Timeout</c> if the request to request sending an invite timed out.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsIllegalState</c> if there was an issue with the connections system, e.g. service is not available for any reason.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsInvalidInviteeCount</c> if number of invitees is invalid.</para>
        /// </returns>
        public static MLResult JoinByCode(string textPrompt, string payload)
        {
            if (MLConnections.IsValidInstance())
            {
                return SendInvite(NativeBindings.RecommendedInviteeCount, InviteeFilter.Followers, textPrompt, payload);
            }
            else
            {
                MLPluginLog.ErrorFormat("MLConnections.JoinByCode failed. Reason: No Instance for MLConnections");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLConnections.JoinByCode failed. Reason: No Instance for MLConnections");
            }
        }

        /// <summary>
        /// Attempts to cancel a previously requested invite sending process. If invite dialog has not yet been completed by the user, this request will
        /// dismiss the dialog and cancel the invite sending process. Otherwise this operation will return an error.
        /// </summary>
        /// <returns>
        /// <para>MLResult.Result will be <c>MLResult.Code.Ok</c> if invite was successfully cancelled.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if permissions haven't been granted to make this API call.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsInvalidHandle</c> if input handle is invalid.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsCancellationPending</c> if the invite request has been found and the system is attempting to cancel the process.</para>
        /// </returns>
        public static MLResult CancelSentInvite()
        {
            if (MLConnections.IsValidInstance())
            {
                MLResult.Code resultCode = NativeBindings.MLConnectionsCancelInvite(_instance.requestHandle);
                MLResult result = MLResult.Create(resultCode);

                if (!result.IsOk && (result.Result != MLResult.Code.ConnectionsCancellationPending))
                {
                    MLPluginLog.ErrorFormat("MLConnections.CancelSentInvite failed to cancel the sent invite. Reason: {0}", result);
                }

                return result;
            }
            else
            {
                MLPluginLog.ErrorFormat("MLConnections.CancelSentInvite failed. Reason: No Instance for MLConnections");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLConnections.CancelSentInvite failed. Reason: No Instance for MLConnections");
            }
        }

        /// <summary>
        /// Request a subset of connections as manually selected by the user. The results are delivered via a callback
        /// </summary>
        /// <param name="maxSelectionAmount">Max number of connections to be selected. Min limit is 1.</param>
        /// <param name="userPrompt">Text prompt to be displayed to the user with selection dialog. The max length for the prompt is 40 characters.</param>
        /// <param name="defaultFilter">Type of filter applied by default to MLConnections list in selection dialog.</param>
        /// <returns>
        /// <para>MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully submitted.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if permissions haven't been granted to make this call.</para>
        /// </returns>
        public static MLResult GetSelection(uint maxSelectionAmount, string userPrompt, SelectionFilter defaultFilter)
        {
            if (MLConnections.IsValidInstance())
            {
                NativeBindings.SelectionArgs argsToSend = NativeBindings.SelectionArgs.Create(maxSelectionAmount, userPrompt, defaultFilter);

                MLResult.Code resultCode = NativeBindings.MLConnectionsRequestSelection(ref argsToSend, ref _instance.selectionHandle);
                MLResult result = MLResult.Create(resultCode);

                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLConnections.GetSelection failed to request a selection. Reason: {0}", result);
                }

                return result;
            }
            else
            {
                MLPluginLog.ErrorFormat("MLConnections.GetSelection failed. Reason: No Instance for MLConnections");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLConnections.GetSelection failed. Reason: No Instance for MLConnections");
            }
        }

        /// <summary>
        /// <para>Attempts to cancel a previously requested selection process.</para>
        /// <para>If selection dialog has not yet been completed by the user, this request will
        /// dismiss the dialog and cancel the selection process.Otherwise this
        /// operation will return an error.</para>
        /// </summary>
        /// <returns>
        /// <para>MLResult.Result will be <c>MLResult.Code.Ok</c> if selection was successfully cancelled.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if permissions haven't been granted to make this call.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsCancellationPending</c> if the selection request has been found and the system is attempting to cancel the process.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsInvalidHandle</c> if input handle is invalid.</para>
        /// </returns>
        public static MLResult CancelSelection()
        {
            if (MLConnections.IsValidInstance())
            {
                MLResult.Code resultCode = NativeBindings.MLConnectionsCancelSelection(_instance.selectionHandle);
                MLResult result = MLResult.Create(resultCode);

                if (!result.IsOk)
                {
                    MLPluginLog.ErrorFormat("MLConnections.CancelSelection failed to cancel the selection requested. Reason: {0}", result);
                }

                return result;
            }
            else
            {
                MLPluginLog.ErrorFormat("MLConnections.CancelSelection failed. Reason: No Instance for MLConnections");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLConnections.CancelSelection failed. Reason: No Instance for MLConnections");
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
                return NativeBindings.MLConnectionsGetResultString(result);
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLConnections.GetResultString failed. Reason: API symbols not found");
            }

            return IntPtr.Zero;
        }

#if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Called by MLAPISingleton to start the API
        /// </summary>
        /// <returns>
        /// <para>MLResult.Result will be <c>MLResult.Code.AllocFailed</c> if the registration resource allocation failed.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a given argument is invalid.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully initialized.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsAlreadyRegistered</c> if this is a duplicate registration.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsNetworkFailure</c> if communication to the network failed.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsSystemFailure</c> if there was system failure.</para>
        /// </returns>
        protected override MLResult StartAPI()
        {
            MLResult.Code resultCode = NativeBindings.MLConnectionsRegistrationStartup(ref _instance.registerHandle);

            if (!MLResult.IsOK(resultCode) || !Native.MagicLeapNativeBindings.MLHandleIsValid(_instance.registerHandle))
            {
                MLResult resultError = MLResult.Create(resultCode);

                MLPluginLog.ErrorFormat("MLConnections.Start failed in StartAPI() to register to receive invites. Handle is invalid or Reason: {0}", resultError);

                return resultError;
            }

            NativeBindings.InviteCallbacks connectionCallbacks = NativeBindings.InviteCallbacks.Create();

            resultCode = NativeBindings.MLConnectionsRegisterForInvite(_instance.registerHandle, connectionCallbacks, IntPtr.Zero);

            if (!MLResult.IsOK(resultCode))
            {
                MLResult resultError = MLResult.Create(resultCode);

                MLPluginLog.ErrorFormat("MLConnections.Start failed in StartAPI() to set callbacks. Reason: {0}", resultError);

                return resultError;
            }
            else
            {
                if (_instance.activeRequest)
                {
                    // This check is just precautionary, but it should never happen.
                    MLPluginLog.WarningFormat("MLConnections.Start allowed multiple active registrations.");
                }

                Instance.activeRequest = true;
            }

            resultCode = NativeBindings.MLConnectionsStartup();

            if (!MLResult.IsOK(resultCode))
            {
                MLPluginLog.ErrorFormat("MLConnections.Start failed in StartAPI() to initialize resources for sending an invite. Reason: {0}", MLResult.CodeToString(resultCode));
            }
            else
            {
                _instance.sendInviteHasStarted = true;
            }

            return MLResult.Create(resultCode);
        }
        #endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Called by MLAPISingleton on destruction
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Used for cleanup</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            if (Native.MagicLeapNativeBindings.MLHandleIsValid(this.registerHandle))
            {
                MLResult.Code resultCode = NativeBindings.MLConnectionsRegistrationShutdown(this.registerHandle);

                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLConnections.Stop failed in CleapupAPI() to shutdown registration to receive invites. Reason: {0}", MLResult.CodeToString(resultCode));
                }
            }

            // Shutdown should only be called if Startup was successful.
            if (this.sendInviteHasStarted)
            {
                MLResult.Code shutdownResultCode = NativeBindings.MLConnectionsShutdown();

                if (!MLResult.IsOK(shutdownResultCode))
                {
                    MLPluginLog.ErrorFormat("MLConnections.Stop failed in CleapupAPI() to deinitialize all resources used for sending an invite. Reason: {0}", MLResult.CodeToString(shutdownResultCode));
                }
            }

            this.registerHandle = Native.MagicLeapNativeBindings.InvalidHandle;
            this.activeRequest = false;
            this.sendInviteHasStarted = false;
        }

        /// <summary>
        /// Called every device frame
        /// </summary>
        protected override void Update()
        {
        }

        /// <summary>
        /// static instance of the MLConnections class
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLConnections.IsValidInstance())
            {
                MLConnections._instance = new MLConnections();
            }
        }

        /// <summary>
        /// Defines invitation result delivered by the callback registered with MLConnections.SendInvite.
        /// </summary>
        public struct InvitationResult
        {
            /// <summary>
            /// General status of invitation request.
            /// </summary>
            public InviteStatus InvitationStatus;

            /// <summary>
            /// The join code to display in app.
            /// </summary>
            public string JoinCode;
        }

        /// <summary>
        /// Defines invitee status to be delivered in the invitee status callbacks. Links to MLConnectionsInviteeStatus in ml_connections.h.
        /// </summary>
        public struct InviteeStatus
        {
            /// <summary>
            /// Locally-unique connection identifier. Generated by the system. May change across reboots.
            /// </summary>
            public string Id;

            /// <summary>
            /// Connection's username.
            /// </summary>
            public string Username;

            /// <summary>
            /// Connection's avatar personalization.
            /// </summary>
            public string AvatarPersonalization;

            /// <summary>
            /// The invitation delivery status for the invitee.
            /// </summary>
            public InvitationDeliveryStatus InvitationDeliveryStatus;
        }

        /// <summary>
        /// Representation of available information for a single connection in address book.
        /// </summary>
        public struct Connection
        {
            /// <summary>
            /// Locally-unique connection identifier. Generated by the system. May change across reboots.
            /// </summary>
            public string Id;

            /// <summary>
            /// Connection's username.
            /// </summary>
            public string Username;

            /// <summary>
            /// Connection's avatar personalization.
            /// </summary>
            public string AvatarPersonalization;
        }
        #endif
    }
}
