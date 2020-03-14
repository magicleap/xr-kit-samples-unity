// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLConnectionsNativeBindings.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// MLConnections class is the entry point for the Connections API
    /// </summary>
    public sealed partial class MLConnections
    {
        /// <summary>
        /// See ml_connections.h for additional comments
        /// </summary>
        private class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// This is the recommended number of invitees from the CAPI. This is overwritten in our implementation.
            /// </summary>
            public const uint RecommendedInviteeCount = 5u;

            /// <summary>
            /// MLConnections library name
            /// </summary>
            private const string MLConnectionsDLL = "ml_connections";

            /// <summary>
            /// Prevents a default instance of the <see cref="NativeBindings"/> class from being created.
            /// </summary>
            private NativeBindings()
            {
            }

            /// <summary>
            /// <para>This is the callback that will be called when Registration has completed,
            /// either successfully or not. It will be called only be called once.</para>
            /// </summary>
            /// <param name="result">This is the result of the Registration. It will be <c>MLResult.Code.Ok</c> in case of success, and an error otherwise.</param>
            /// <param name="data">This is the context that was passed to MLConnectionsRegisterForInvite.</param>
            private delegate void OnRegistrationCompleteCallbackPrivate(MLResult.Code result, IntPtr data);

            /// <summary>
            /// <para>This is the callback that will be called every time an invitation comes in.
            /// If user_accepted is true, this means the user was shown a notification and accepted
            /// the invitation to launch the application.In this case, you should take the user
            /// directly to the invitation context. If it is false, you should display the notification
            /// yourself in a way that is most in-line with your application's style and design. The
            /// payload parameter will be the data sent from the inviting application on another device.</para>
            /// </summary>
            /// <param name="userAccepted">This is a boolean value that informs you whether the user has confirmed the invite notification.</param>
            /// <param name="payload">This is the payload that was sent from the inviting application.</param>
            /// <param name="data">This is the context that was passed to MLConnectionsRegisterForInvite.</param>
            private delegate void OnInvitationCallbackPrivate([MarshalAs(UnmanagedType.I1)] bool userAccepted, IntPtr payload, IntPtr data);

            /// <summary>
            /// <para>This is the callback that will be called when invitation has been confirmed by the user.</para>
            /// </summary>
            /// <param name="result">The invitation results.</param>
            /// <param name="context">This is the context field that was passed to MLConnections.SendInvite</param>
            private delegate void OnInvitationResultCallbackPrivate(ref InvitationResultNative result, IntPtr context);

            /// <summary>
            /// <para>This is the callback that will be called when invitee statuses become available.</para>
            /// <para>When sending invitation using join code this callback is not expected to be triggered, this may change in the future.</para>
            /// </summary>
            /// <param name="status">The invitee status and details.</param>
            /// <param name="context">This is the context field that was passed to MLConnections.SendInvite</param>
            private delegate void OnInviteeStatusCallbackPrivate(ref InviteeStatusNative status, IntPtr context);

            /// <summary>
            /// <para>This is the callback that will be called when selection has completed by the user.</para>
            /// <para>If this callback is supplied the handle is automatically cleaned up when selection is canceled, failed or confirmed.</para>
            /// </summary>
            /// <param name="result">This is a boolean value that informs you whether the user has confirmed the invite notification.</param>
            /// <param name="context">This is the payload that was sent from the inviting application.</param>
            private delegate void OnSelectionCallbackPrivate(ref SelectionResult result, IntPtr context);

            /// <summary>
            /// This is the callback that will be called every time an invitation comes in. If userConfirmation is true, this means the user was shown a
            /// notification and accepted the invitation to launch the application. If false, the application was running already when the request was received
            /// and the application is in charge of gaining user consent.
            /// </summary>
            public static event InvitationDelegate OnInvitation = delegate { };

            /// <summary>
            /// This is the callback that will be called when Registration has completed, either successfully or not. Expect it to only be called once.
            /// </summary>
            public static event RegistrationCompleteDelegate OnRegistrationComplete = delegate { };

            /// <summary>
            /// This is the callback that will be called when invitation has been confirmed by the user.
            /// </summary>
            public static event OnInvitationResultDelegate OnInvitationResult = delegate { };

            /// <summary>
            /// <para>This is the callback that will be called when invitee statuses become available.</para>
            /// <para>When sending invitation using join code this callback is not expected to be triggered, this may change in the future.</para>
            /// </summary>
            public static event OnInviteeStatusDelegate OnInviteeStatus = delegate { };

            /// <summary>
            /// This is the callback that will be called when selection has completed by the user.
            /// </summary>
            public static event OnSelectionDelegate OnSelection = delegate { };

            /// <summary>
            /// <para>Initialize all necessary resources for sending an invite using MLConnections API.</para>
            /// <para>This function should be called before any connection functions are called.
            /// This function should be called along with MLConnectionsRegistrationStartup and
            /// MLConnectionsRegisterForInvite for any invitation to be delivered.</para>
            /// </summary>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if successfully initialized.
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsStartup();

            /// <summary>
            /// <para>De-initialize all resources used for sending an invite using MLConnections API.</para>
            /// <para>This function should be called prior to exiting the program
            /// if a call to MLConnectionsStartup was called.</para>
            /// <para>This function should be called along with MLConnectionsRegistrationShutdown if
            /// MLConnectionsRegistrationStartup was called along with MLConnectionsStartup.</para>
            /// </summary>
            /// <returns>
            /// <c>MLResult.Code.Ok</c> if successfully shutdown.
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsShutdown();

            /// <summary>
            /// <para>Initialize all necessary resources for receiving an invite using MLConnectionsRegisterForInvite API.</para>
            /// <para>This function should be called before any invite receiving functions are called.
            /// This function should be called along with MLConnectionsStartup and
            /// MLConnectionsRegisterForInvite for any invitation to be delivered.</para>
            /// </summary>
            /// <param name="out_register_handle">The handle that will be used for managing MLConnectionsRegisterForInvite.</param>
            /// <returns>
            /// <para><c>MLResult.Code.AllocFailed</c> if the registration resource allocation failed.</para>
            /// <para><c>MLResult.Code.InvalidParam</c> if a given argument is invalid.</para>
            /// <para><c>MLResult.Code.Ok</c> if successfully initialized.</para>
            /// <para><c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsRegistrationStartup(ref ulong out_register_handle);

            /// <summary>
            /// <para>De-initialize all resources for receiving an invite using MLConnectionsRegisterForInvite API.</para>
            /// <para>This should be called when you no longer want the MLConnections Invitation
            /// callback to be activated, and prior to exiting the program if a call to
            /// MLConnectionsRegistrationStartup was called.</para>
            /// </summary>
            /// <param name="register_handle">The handle that will be de-initialized.</param>
            /// <returns>
            /// <para><c>MLResult.Code.InvalidParam</c> if a given argument is invalid.</para>
            /// <para><c>MLResult.Code.Ok</c> if successfully initialized.</para>
            /// <para><c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsRegistrationShutdown(ulong register_handle);

            /// <summary>
            /// <para>Register your application for receiving invites from other devices.</para>
            /// <para>This function should be called along with MLConnectionsStartup and
            /// MLConnectionsRegistrationStartup for any invitation to be delivered.</para>
            /// </summary>
            /// <param name="register_handle">The handle that was allocated with MLConnectionsRegistrationStartup.</param>
            /// <param name="callbacks">This is the callbacks struct that contains the callbacks that will be called.</param>
            /// <param name="data">This is the value that will be passed to both callbacks above.</param>
            /// <returns>
            /// <para><c>MLResult.Code.InvalidParam</c> if a given argument is invalid.</para>
            /// <para><c>MLResult.Code.Ok</c> if successfully initiated registration.</para>
            /// <para><c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
            /// <para><c>MLResult.Code.ConnectionsAlreadyRegistered</c> if this is a duplicate registration.</para>
            /// <para><c>MLResult.Code.ConnectionsNetworkFailure</c> if communication to the network failed.</para>
            /// <para><c>MLResult.Code.ConnectionsSystemFailure</c> if there was system failure.</para>
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsRegisterForInvite(ulong register_handle, InviteCallbacks callbacks, IntPtr data);

            /// <summary>
            /// <para>Submits a request to start the invite sending process.</para>
            /// <para>Request an invite to be sent for other connections to join a multi-user
            /// experience. This call will trigger a connections invite dialog to start the
            /// invitation process.There are two ways to invite users:</para>
            /// <para>[1] Invite users from your social graph.</para>
            /// <para>[2] Invite users using a join code.</para>
            /// <para>The system will then initiate a push notification to other online devices, start
            /// a copy of the application requesting the invite and deliver the given payload.</para>
            /// <para>The payload should contain the session context.</para>
            /// <para>When inviting users in the social graph the initiator selects the invitees from
            /// invite picker.The payload is delivered to the remote users via the on_invitation
            /// callback.This can be used to create a session.  You cannot invite users in
            /// your social graph who are not following the current user.</para>
            /// <para>When inviting users using a join code the initiator first creates the room by
            /// calling this API.The remote user has to join the room before the payload can
            /// be delivered via the on_invitation callback. In order to join the room the
            /// remote user has to call this API.</para>
            /// </summary>
            /// <param name="args">Sender invite arguments.</param>
            /// <param name="out_request_handle">This is the handle used to check the status of the invite sending process.</param>
            /// <returns>
            /// <para><c>MLResult.Code.InvalidParam</c> if a given argument is invalid.</para>
            /// <para><c>MLResult.Code.Ok</c> if successfully requested the start of the invite dialog.</para>
            /// <para><c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
            /// <para><c>MLResult.Code.PrivilegeDenied</c> if permissions haven't been granted to make this API call.</para>
            /// <para><c>MLResult.Code.Timeout</c> if the request to request sending an invite timed out.</para>
            /// <para><c>MLResult.Code.ConnectionsIllegalState</c> if there was an issue with the connections system, e.g. service is not available for any reason.</para>
            /// <para><c>MLResult.Code.ConnectionsInvalidInviteeCount</c> if number of invitees is invalid.</para>
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsRequestInvite(ref InviteArgs args, ref ulong out_request_handle);

            /// <summary>
            /// <para>Checks the status of an invite sending process handle.</para>
            /// <para>This API call may be repeated if it returns MLResult_Pending for a request handle.</para>
            /// </summary>
            /// <param name="request_handle">This is the handle created by MLConnectionsRequestInvite.</param>
            /// <param name="out_status">Given that <c>MLResult.Code.Ok</c> is returned, this will contain the status of the sending process for the invite.</param>
            /// <returns>
            /// <para><c>MLResult.Code.Ok</c> if polling for the invite status was successful.</para>
            /// <para><c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure in getting the invite status.</para>
            /// <para><c>MLResult.Code.ConnectionsInvalidHandle</c> if input handle is invalid.</para>
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsTryGetInviteStatus(ulong request_handle, ref MLConnections.InviteStatus out_status);

            /// <summary>
            /// <para>Attempts to cancel a previously requested invite sending process.</para>
            /// <para>If invite dialog has not yet been completed by the user, this request will
            /// dismiss the dialog and cancel the invite sending process. Otherwise this
            /// operation will return an error.</para>
            /// </summary>
            /// <param name="request_handle">This is the handle created by MLConnectionsRequestInvite.</param>
            /// <returns>
            /// <para><c>MLResult.Code.Ok</c> if invite was successfully cancelled.</para>
            /// <para><c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
            /// <para><c>MLResult.Code.PrivilegeDenied</c> if permissions haven't been granted to make this API call.</para>
            /// <para><c>MLResult.Code.ConnectionsInvalidHandle</c> if input handle is invalid.</para>
            /// <para><c>MLResult.Code.ConnectionsCancellationPending</c> if the invite request has been found and the system is attempting to cancel the process.</para>
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsCancelInvite(ulong request_handle);

            /// <summary>
            /// <para>Free all resources for a request corresponding to the handle.</para>
            /// </summary>
            /// <param name="request_handle">This is the handle created by MLConnectionsRequestInvite.</param>
            /// <returns>
            /// <para><c>MLResult.Code.Ok</c> if invite was successfully cancelled.</para>
            /// <para><c>MLResult.Code.ConnectionsInvalidHandle</c> if the request corresponding to the handle was not found.</para>
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsReleaseRequestResources(ulong request_handle);

            /// <summary>
            /// <para>Returns an ASCII string for MLConnectionsResult and MLResult codes.</para>
            /// </summary>
            /// <param name="result">The input MLResult.Code enum from MLConnections functions.</param>
            /// <returns>
            /// <para>ASCII string containing readable version of result code.</para>
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MLConnectionsGetResultString(MLResult.Code result);

            /// <summary>
            /// <para>Request a subset of connections as manually selected by the user.</para>
            /// <para>The selection is done using a separate system application. The results are delivered via a callback specified in args.</para>
            /// </summary>
            /// <param name="args">Reference to the SelectionArgs.</param>
            /// <param name="out_selection_request_handle">A pointer to a handle which will contain the handle to the request. If this operation fails this handle will be invalid.</param>
            /// <returns>
            /// <para><c>MLResult.Code.Ok</c> if successfully submitted.</para>
            /// <para><c>MLResult.Code.PrivilegeDenied</c> if permissions haven't been granted to make this call.</para>
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsRequestSelection(ref SelectionArgs args, ref ulong out_selection_request_handle);

            /// <summary>
            /// <para>Try to get result of a selection request.</para>
            /// <para>This call may be repeated if it returns Pending for a request handle.</para>
            /// </summary>
            /// <param name="selection_request_handle">This is the handle created by MLConnectionsRequestSelection.</param>
            /// <param name="out_result">MLConnectionsSelectionResult. The content will be freed by MLConnectionsReleaseRequestResources.</param>
            /// <returns>
            /// <para><c>MLResult.Code.InvalidParam</c> if the request handle doesn't correspond to this operation.</para>
            /// <para><c>MLResult.Code.Pending</c> if the request is still pending.</para>
            /// <para><c>MLResult.Code.ConnectionsInvalidHandle</c> if </para>
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsTryGetSelectionResult(ulong selection_request_handle, ref IntPtr out_result);

            /// <summary>
            /// <para>Attempts to cancel a previously requested selection process.</para>
            /// <para>If selection dialog has not yet been completed by the user, this request will
            /// dismiss the dialog and cancel the selection process.Otherwise this
            /// operation will return an error.</para>
            /// </summary>
            /// <param name="selection_request_handle">This is the handle created by MLConnectionsRequestSelection.</param>
            /// <returns>
            /// <para><c>MLResult.Code.Ok</c> if selection was successfully cancelled.</para>
            /// <para><c>MLResult.Code.PrivilegeDenied</c> if permissions haven't been granted to make this call.</para>
            /// <para><c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
            /// <para><c>MLResult.Code.ConnectionsCancellationPending</c> if the selection request has been found and the system is attempting to cancel the process.</para>
            /// <para><c>MLResult.Code.ConnectionsInvalidHandle</c> if input handle is invalid.</para>
            /// </returns>
            [DllImport(MLConnectionsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLConnectionsCancelSelection(ulong selection_request_handle);

            /// <summary>
            /// <para>This is the callback that will be called when Registration has completed,
            /// either successfully or not. It will be called only be called once.</para>
            /// </summary>
            /// <param name="result">This is the result of the Registration. It will be <c>MLResult.Code.Ok</c> in case of success, and an error otherwise.</param>
            /// <param name="data">This is the context that was passed to MLConnectionsRegisterForInvite.</param>
            [AOT.MonoPInvokeCallback(typeof(OnRegistrationCompleteCallbackPrivate))]
            private static void OnRegistrationCompleteNative(MLResult.Code result, IntPtr data)
            {
                Native.MLThreadDispatch.Call(MLResult.Create(result), OnRegistrationComplete);
            }

            /// <summary>
            /// <para>This is the callback that will be called every time an invitation comes in.
            /// If user_accepted is true, this means the user was shown a notification and accepted
            /// the invitation to launch the application.In this case, you should take the user
            /// directly to the invitation context. If it is false, you should display the notification
            /// yourself in a way that is most in-line with your application's style and design. The
            /// payload parameter will be the data sent from the inviting application on another device.</para>
            /// </summary>
            /// <param name="userAccepted">This is a boolean value that informs you whether the user has confirmed the invite notification.</param>
            /// <param name="payload">This is the payload that was sent from the inviting application.</param>
            /// <param name="data">This is the context that was passed to MLConnectionsRegisterForInvite.</param>
            [AOT.MonoPInvokeCallback(typeof(OnInvitationCallbackPrivate))]
            private static void OnInvitationNative([MarshalAs(UnmanagedType.I1)] bool userAccepted, IntPtr payload, IntPtr data)
            {
                string decodedPayload = Native.MLConvert.DecodeUTF8(payload);
                Native.MLThreadDispatch.Call(userAccepted, decodedPayload, OnInvitation);
            }

            /// <summary>
            /// <para>This is the callback that will be called when invitation has been confirmed by the user.</para>
            /// </summary>
            /// <param name="result">The invitation results.</param>
            /// <param name="context">This is the context field that was passed to MLConnections.SendInvite</param>
            [AOT.MonoPInvokeCallback(typeof(OnInvitationResultCallbackPrivate))]
            private static void OnInvitationResultNative(ref InvitationResultNative result, IntPtr context)
            {
                Native.MLThreadDispatch.Call(result.Data, OnInvitationResult);
            }

            /// <summary>
            /// <para>This is the callback that will be called when invitee statuses become available.</para>
            /// <para>When sending invitation using join code this callback is not expected to be triggered, this may change in the future.</para>
            /// </summary>
            /// <param name="status">The invitee status and details.</param>
            /// <param name="context">This is the context field that was passed to MLConnections.SendInvite</param>
            [AOT.MonoPInvokeCallback(typeof(OnInviteeStatusCallbackPrivate))]
            private static void OnInviteeStatusNative(ref InviteeStatusNative status, IntPtr context)
            {
                Native.MLThreadDispatch.Call(status.Data, OnInviteeStatus);
            }

            /// <summary>
            /// <para>This is the callback that will be called when invitee statuses become available.</para>
            /// <para>When sending invitation using join code this callback is not expected to be triggered, this may change in the future.</para>
            /// </summary>
            /// <param name="result">The invitee status and details.</param>
            /// <param name="context">This is the context field that was passed to MLConnections.SendInvite</param>
            [AOT.MonoPInvokeCallback(typeof(OnSelectionCallbackPrivate))]
            private static void OnSelectionNative(ref SelectionResult result, IntPtr context)
            {
                int numConnections = (int)result.ResultList.Count;

                Connection[] externalList = new Connection[numConnections];

                IntPtr walkPtr = result.ResultList.Connections;

                for (int i = 0; i < numConnections; ++i)
                {
                    ConnectionNative conn = (ConnectionNative)Marshal.PtrToStructure(Marshal.ReadIntPtr(walkPtr), typeof(ConnectionNative));
                    externalList[i] = conn.Data;
                    walkPtr = new IntPtr(walkPtr.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));
                }

                Native.MLThreadDispatch.Call(result.SelectionStatus, externalList, OnSelection);
            }

            /// <summary>
            /// Stores arguments for the sending invite process with MLConnections.SendInvite. Links to MLConnectionsInviteArgs in ml_connections.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct InviteArgs
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Max number of connections to be invited. Min limit is 1.
                /// </summary>
                public uint InviteeCount;

                /// <summary>
                /// Text prompt to be displayed to the user with invitee selection dialog. UTF8. The max length for the prompt is 40 characters.
                /// </summary>
                public IntPtr InviteUserPrompt;

                /// <summary>
                /// Payload message to be delivered to remote copy of the application with invite in serialized text form. UTF8. The max length for the prompt is 1200 bytes.
                /// </summary>
                public IntPtr InvitePayload;

                /// <summary>
                /// Type of filter applied by default to ML connections list in invitee selection dialog.
                /// </summary>
                public MLConnections.InviteeFilter DefaultInviteeFilter;

                /// <summary>
                /// The context for callbacks to be registered for this call.
                /// </summary>
                public IntPtr Context;

                /// <summary>
                /// <para>This is the callback that will be called when invitation has been confirmed by the user.</para>
                /// </summary>
                public OnInvitationResultCallback OnInvitationResult;

                /// <summary>
                /// <para>This is the callback that will be called when invitee statuses become available.</para>
                /// <para>When sending invitation using join code this callback is not expected to be triggered, this may change in the future.</para>
                /// </summary>
                public OnInviteeStatusCallback OnInviteeStatus;

                /// <summary>
                /// <para>This is the callback that will be called when invitation has been confirmed by the user.</para>
                /// </summary>
                /// <param name="result">The invitation results.</param>
                /// <param name="context">This is the context field that was passed to MLConnections.SendInvite</param>
                public delegate void OnInvitationResultCallback(ref InvitationResultNative result, IntPtr context);

                /// <summary>
                /// <para>This is the callback that will be called when invitee statuses become available.</para>
                /// <para>When sending invitation using join code this callback is not expected to be triggered, this may change in the future.</para>
                /// </summary>
                /// <param name="status">The invitee status and details.</param>
                /// <param name="context">This is the context field that was passed to MLConnections.SendInvite</param>
                public delegate void OnInviteeStatusCallback(ref InviteeStatusNative status, IntPtr context);

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>
                /// An initialized version of this struct.
                /// </returns>
                public static InviteArgs Create()
                {
                    return new InviteArgs()
                    {
                        Version = 2u,
                        InviteeCount = RecommendedInviteeCount,
                        InviteUserPrompt = IntPtr.Zero,
                        InvitePayload = IntPtr.Zero,
                        DefaultInviteeFilter = MLConnections.InviteeFilter.Followers,
                        Context = IntPtr.Zero,
                        OnInvitationResult = OnInvitationResultNative,
                        OnInviteeStatus = OnInviteeStatusNative
                    };
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
                /// <param name="out_request_handle">This is the handle used to check the status of the invite sending process.</param>
                /// <returns>
                /// <para><c>MLResult.Code.InvalidParam</c> if a given argument is invalid.</para>
                /// <para><c>MLResult.Code.Ok</c> if successfully requested the start of the invite dialog.</para>
                /// <para><c>MLResult.Code.UnspecifiedFailure</c> if there was an unexpected failure.</para>
                /// <para><c>MLResult.Code.PrivilegeDenied</c> if permissions haven't been granted to make this API call.</para>
                /// <para><c>MLResult.Code.Timeout</c> if the request to request sending an invite timed out.</para>
                /// <para><c>MLResult.Code.ConnectionsIllegalState</c> if there was an issue with the connections system, e.g. service is not available for any reason.</para>
                /// <para><c>MLResult.Code.ConnectionsInvalidInviteeCount</c> if number of invitees is invalid.</para>
                /// </returns>
                public static MLResult SendInviteHelper(uint inviteeCount, MLConnections.InviteeFilter filterLevel, string textPrompt, string payload, ref ulong out_request_handle)
                {
                    InviteArgs requestArgs = Create();
                    requestArgs.InviteeCount = inviteeCount;
                    requestArgs.InviteUserPrompt = Native.MLConvert.EncodeToUnmanagedUTF8(textPrompt);
                    requestArgs.InvitePayload = Native.MLConvert.EncodeToUnmanagedUTF8(payload);
                    requestArgs.DefaultInviteeFilter = filterLevel;

                    MLResult.Code requestResultCode = MLConnectionsRequestInvite(ref requestArgs, ref out_request_handle);

                    Marshal.FreeHGlobal(requestArgs.InviteUserPrompt);
                    Marshal.FreeHGlobal(requestArgs.InvitePayload);

                    return MLResult.Create(requestResultCode);
                }
            }

            /// <summary>
            /// Links to MLConnectionsInviteCallbacks in ml_input.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct InviteCallbacks
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// <para>This is the callback that will be called when Registration has completed,
                /// either successfully or not. It will be called only be called once.</para>
                /// </summary>
                public OnRegistrationCompleteCallback OnRegistrationComplete;

                /// <summary>
                /// <para>This is the callback that will be called every time an invitation comes in.
                /// If user_accepted is true, this means the user was shown a notification and accepted
                /// the invitation to launch the application.In this case, you should take the user
                /// directly to the invitation context. If it is false, you should display the notification
                /// yourself in a way that is most in-line with your application's style and design. The
                /// payload parameter will be the data sent from the inviting application on another device.</para>
                /// <para>The payload will contain the required session context.</para>
                /// </summary>
                public OnInvitationCallback OnInvitation;

                /// <summary>
                /// <para>This is the callback that will be called when Registration has completed,
                /// either successfully or not. It will be called only be called once.</para>
                /// </summary>
                /// <param name="result">This is the result of the Registration. It will be <c>MLResult.Code.Ok</c> in case of success, and an error otherwise.</param>
                /// <param name="data">This is the context that was passed to MLConnectionsRegisterForInvite.</param>
                public delegate void OnRegistrationCompleteCallback(MLResult.Code result, IntPtr data);

                /// <summary>
                /// <para>This is the callback that will be called every time an invitation comes in.
                /// If user_accepted is true, this means the user was shown a notification and accepted
                /// the invitation to launch the application.In this case, you should take the user
                /// directly to the invitation context. If it is false, you should display the notification
                /// yourself in a way that is most in-line with your application's style and design. The
                /// payload parameter will be the data sent from the inviting application on another device.</para>
                /// </summary>
                /// <param name="userAccepted">This is a boolean value that informs you whether the user has confirmed the invite notification.</param>
                /// <param name="payload">This is the payload that was sent from the inviting application.</param>
                /// <param name="data">This is the context that was passed to MLConnectionsRegisterForInvite.</param>
                public delegate void OnInvitationCallback([MarshalAs(UnmanagedType.I1)] bool userAccepted, IntPtr payload, IntPtr data);

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>
                /// An initialized version of this struct.
                /// </returns>
                public static InviteCallbacks Create()
                {
                    return new InviteCallbacks()
                    {
                        Version = 1u,
                        OnRegistrationComplete = OnRegistrationCompleteNative,
                        OnInvitation = OnInvitationNative
                    };
                }
            }

            /// <summary>
            /// Representation of available information for a single connection in address book. Links to MLConnectionsConnection in ml_connections.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ConnectionNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Locally-unique connection identifier. Generated by the system. May change across reboots.
                /// </summary>
                public IntPtr Id;

                /// <summary>
                /// Connection's username.
                /// </summary>
                public IntPtr Username;

                /// <summary>
                /// Connection's avatar personalization.
                /// </summary>
                public IntPtr AvatarPersonalization;

                /// <summary>
                /// Gets the native structures from the user facing properties.
                /// </summary>
                public MLConnections.Connection Data
                {
                    get
                    {
                        return new MLConnections.Connection()
                        {
                            Id = Native.MLConvert.DecodeUTF8(this.Id),
                            Username = Native.MLConvert.DecodeUTF8(this.Username),
                            AvatarPersonalization = Native.MLConvert.DecodeUTF8(this.AvatarPersonalization),
                        };
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>
                /// An initialized version of this struct.
                /// </returns>
                public static ConnectionNative Create()
                {
                    return new ConnectionNative()
                    {
                        Version = 1u,
                        Id = IntPtr.Zero,
                        Username = IntPtr.Zero,
                        AvatarPersonalization = IntPtr.Zero
                    };
                }
            }

            /// <summary>
            /// Stores a list of ConnectionNative. Links to MLConnectionsConnectionList in ml_connections.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ConnectionList
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Number of connections.
                /// </summary>
                public ulong Count;

                /// <summary>
                /// Pointer referring to array of ConnectionNative.
                /// </summary>
                public IntPtr Connections;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>
                /// An initialized version of this struct.
                /// </returns>
                public static ConnectionList Create()
                {
                    return new ConnectionList()
                    {
                        Version = 1u,
                        Count = 0,
                        Connections = IntPtr.Zero
                    };
                }
            }

            /// <summary>
            /// Defines invitee status to be delivered in the invitee status callbacks. Links to MLConnectionsInviteeStatus in ml_connections.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct InviteeStatusNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Locally-unique connection identifier. Generated by the system. May change across reboots.
                /// </summary>
                public IntPtr Id;

                /// <summary>
                /// Connection's username.
                /// </summary>
                public IntPtr Username;

                /// <summary>
                /// Connection's avatar personalization.
                /// </summary>
                public IntPtr AvatarPersonalization;

                /// <summary>
                /// The invitation delivery status for the invitee.
                /// </summary>
                public MLConnections.InvitationDeliveryStatus InvitationDeliveryStatus;

                /// <summary>
                /// Gets the native structures from the user facing properties.
                /// </summary>
                public MLConnections.InviteeStatus Data
                {
                    get
                    {
                        return new MLConnections.InviteeStatus()
                        {
                            Id = Native.MLConvert.DecodeUTF8(this.Id),
                            Username = Native.MLConvert.DecodeUTF8(this.Username),
                            AvatarPersonalization = Native.MLConvert.DecodeUTF8(this.AvatarPersonalization),
                            InvitationDeliveryStatus = this.InvitationDeliveryStatus
                        };
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>
                /// An initialized version of this struct.
                /// </returns>
                public static InviteeStatusNative Create()
                {
                    return new InviteeStatusNative()
                    {
                        Id = IntPtr.Zero,
                        Username = IntPtr.Zero,
                        AvatarPersonalization = IntPtr.Zero,
                        InvitationDeliveryStatus = MLConnections.InvitationDeliveryStatus.Processing
                    };
                }
            }

            /// <summary>
            /// Defines invitation result delivered by the callback registered with MLConnections.SendInvite. Links to MLConnectionsInvitationResult in ml_connections.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct InvitationResultNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// General status of invitation request.
                /// </summary>
                public MLConnections.InviteStatus InvitationStatus;

                /// <summary>
                /// The join code to display in app.
                /// </summary>
                public IntPtr JoinCode;

                /// <summary>
                /// Gets the native structures from the user facing properties.
                /// </summary>
                public MLConnections.InvitationResult Data
                {
                    get
                    {
                        return new MLConnections.InvitationResult()
                        {
                            InvitationStatus = this.InvitationStatus,
                            JoinCode = Native.MLConvert.DecodeUTF8(this.JoinCode)
                        };
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>
                /// An initialized version of this struct.
                /// </returns>
                public static InvitationResultNative Create()
                {
                    return new InvitationResultNative()
                    {
                        Version = 1u,
                        InvitationStatus = MLConnections.InviteStatus.SubmittingRequest,
                        JoinCode = IntPtr.Zero
                    };
                }
            }

            /// <summary>
            /// Stores result of a selection. Links to MLConnectionsSelectionResult in ml_connections.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SelectionResult
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Status of operation.
                /// </summary>
                public MLConnections.SelectionStatus SelectionStatus;

                /// <summary>
                /// Resultant list of connections.
                /// </summary>
                public ConnectionList ResultList;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>
                /// An initialized version of this struct.
                /// </returns>
                public static SelectionResult Create()
                {
                    return new SelectionResult()
                    {
                        Version = 1u,
                        SelectionStatus = MLConnections.SelectionStatus.Pending,
                        ResultList = new ConnectionList()
                    };
                }
            }

            /// <summary>
            /// Stores arguments for the sending invite process. Links to MLConnectionsSelectionArgs in ml_connections.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SelectionArgs
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Max number of connections to be selected. Min limit is 1.
                /// </summary>
                public uint MaxSelectionCount;

                /// <summary>
                /// Text prompt to be displayed to the user with selection dialog.
                /// <para>Caller should allocate memory for this. Encoding should be in UTF8. This will be copied internally.</para>
                /// <para>The max length for the prompt is 40 characters.</para>
                /// </summary>
                public IntPtr UserPrompt;

                /// <summary>
                /// Type of filter applied by default to MLConnections list in selection dialog.
                /// </summary>
                public MLConnections.SelectionFilter DefaultFilter;

                /// <summary>
                /// The context for OnSelection callback.
                /// </summary>
                public IntPtr Context;

                /// <summary>
                /// <para>This is the callback that will be called when selection has completed by the user.</para>
                /// </summary>
                public OnSelectionCallback OnSelection;

                /// <summary>
                /// <para>This is the callback that will be called when selection has completed by the user.</para>
                /// <para>If this callback is supplied the handle is automatically cleaned up when selection is canceled, failed or confirmed.</para>
                /// </summary>
                /// <param name="result">This is a boolean value that informs you whether the user has confirmed the invite notification.</param>
                /// <param name="context">This is the payload that was sent from the inviting application.</param>
                public delegate void OnSelectionCallback(ref SelectionResult result, IntPtr context);

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>
                /// An initialized version of this struct.
                /// </returns>
                public static SelectionArgs Create()
                {
                    return new SelectionArgs()
                    {
                        Version = 1u,
                        MaxSelectionCount = RecommendedInviteeCount,
                        UserPrompt = IntPtr.Zero,
                        DefaultFilter = MLConnections.SelectionFilter.Following,
                        Context = IntPtr.Zero,
                        OnSelection = OnSelectionNative
                    };
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <param name="count">Max number of connections to be selected. Min limit is 1.</param>
                /// <param name="userPrompt">Text prompt to be displayed to the user with selection dialog. The max length for the prompt is 40 characters.</param>
                /// <param name="defaultFilter">Type of filter applied by default to MLConnections list in selection dialog.</param>
                /// <returns>
                /// An initialized version of this struct.
                /// </returns>
                public static SelectionArgs Create(uint count, string userPrompt, MLConnections.SelectionFilter defaultFilter)
                {
                    return new SelectionArgs()
                    {
                        Version = 1u,
                        MaxSelectionCount = count,
                        UserPrompt = Native.MLConvert.EncodeToUnmanagedUTF8(userPrompt),
                        DefaultFilter = defaultFilter,
                        Context = IntPtr.Zero,
                        OnSelection = OnSelectionNative
                    };
                }
            }
        }
    }
}

#endif
