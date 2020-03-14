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
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Defines possible status values for sending invites with MLConnectionsRequestInvite. Links to MLConnectionsInviteStatus in ml_connections.h.
    /// </summary>
    [Obsolete("Please use MLConnections.InviteStatus instead.", true)]
    public enum MLConnectionsInviteStatus : uint
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
    /// Defines default filter for Magic Leap connections when displaying invite dialog to the user in MLConnectionsRequestInvite. Links to MLConnectionsInviteeFilter in ml_connections.h.
    /// </summary>
    [Obsolete("Please use MLConnections.InviteeFilter instead.", false)]
    public enum MLConnectionsInviteeFilter : uint
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

    #if PLATFORM_LUMIN
    /// <summary>
    /// MLConnections class is the entry point for the Connections API
    /// </summary>
    public sealed partial class MLConnections : MLAPISingleton<MLConnections>
    {
        /// <summary>
        /// Callback which is fired when a request sent is completed.
        /// </summary>
        /// <param name="completedStatus">Contains the status of the sending process once it has been completed.</param>
        [Obsolete("Please use MLConnections.OnInvitationResultDelegate instead.", true)]
        public delegate void RequestStatusDelegate(MLConnections.InviteStatus completedStatus);

        /// <summary>
        /// This is the callback that will be called once SendInvite() completes. The possible completedStatus are: Dispatched, DispatchFailed, Cancelled, or InvalidHandle.
        /// </summary>
        [Obsolete("Please use MLConnections.OnInvitationResult instead.", true)]
        public static event RequestStatusDelegate OnRequestComplete = delegate { };

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
        /// <para>MLResult.Result will be <c>MLResult.Code.Ok</c> If successfully submitted to at least one follower.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> Indicates permissions haven't been granted to make this API call.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.Timeout</c> Indicates the request to request sending an invite timed out.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error (no device, <c>dll</c> not found, no API symbols).</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsIllegalState</c> Indicates an issue with the connections system, e.g. service is not available for any reason.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.ConnectionsInvalidInviteeCount</c> Indicates number of invitees is invalid.</para>
        /// <para>MLResult.Result will be <c>MLResult.Code.InvalidParam</c> Indicates the given arguments are invalid.</para>
        /// </returns>
        [Obsolete("Please use MLConnections.SendInvite(uint, MLConnections.InviteeFilter, string, string) instead.", false)]
        public static MLResult SendInvite(uint inviteeCount, MLConnectionsInviteeFilter filterLevel, string textPrompt, string payload)
        {
            return SendInvite(inviteeCount, (InviteeFilter)filterLevel, textPrompt, payload);
        }

        /// <summary>
        /// Gets a readable version of the result code as an string.
        /// </summary>
        /// <param name="resultCode">The MLResult that should be converted.</param>
        /// <returns>String containing a readable version of the result code.</returns>
        [Obsolete("Please use MLResult.CodeToString(resultCode) instead.", true)]
        public static string GetResultString(MLResultCode resultCode)
        {
            return "This function is deprecated. Use MLResult.CodeToString(resultCode) instead.";
        }
    }
    #endif
}
