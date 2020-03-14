// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLAppConnectNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

// Disable warnings about missing documentation for native interop.
#pragma warning disable 1591
namespace UnityEngine.XR.MagicLeap.Native
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine.XR.MagicLeap;

    /// <summary>
    /// See ml_app_connect.h for additional comments.
    /// </summary>
    public class MLAppConnectNativeBindings : MagicLeapNativeBindings
    {
        /// <summary>
        /// String containing the header file name.
        /// </summary>
        private const string MLAppConnectDLL = "ml_app_connect";

        /// <summary>
        /// Connection callback function.
        /// </summary>
        /// <param name="result">The result status of the connection.</param>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="userData">User provided data.</param>
        private delegate void OnConnectionCallbackPrivate(MLResult.Code result, [MarshalAs(UnmanagedType.LPStr)] string connectionName, IntPtr userData);

        /// <summary>
        /// Connection Invite callback function.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="arguments">A list of arguments.</param>
        /// <param name="argumentsCount">Number of arguments in the list.</param>
        /// <param name="userData">User provided data.</param>PipeProperties
        private delegate void OnInviteCallbackPrivate([MarshalAs(UnmanagedType.LPStr)] string connectionName, MLAppConnect.KeyValue[] arguments, uint argumentsCount, IntPtr userData);

        /// <summary>
        /// Connection event callback function.
        /// </summary>
        /// <param name="eventInfo">eventInfo Information related to the event.</param>
        /// <param name="userData"> User provided data.</param>
        private delegate void OnEventCallbackPrivate(IntPtr eventInfo, IntPtr userData);

        /// <summary>
        /// Event triggered when a connection state is updated.
        /// </summary>
        public static event MLAppConnect.OnConnectionDelegate OnConnection;

        /// <summary>
        /// Connection invite event.
        /// </summary>
        public static event MLAppConnect.OnInviteDelegate OnInvite;

        /// <summary>
        ///  This Event will be triggered when there is an update on a connection, user, microphone or pipe.
        /// </summary>
        public static event MLAppConnect.OnEventDelegate OnEvent;

        /// <summary>
        /// This Event will be triggered when there is an update on the connection status.
        /// </summary>
        public static event MLAppConnect.OnPipeEventDelegate OnPipeEvent;

        /// <summary>
        /// This Event will be triggered when there is an update on to any users connection status.
        /// </summary>
        public static event MLAppConnect.OnUsersEventDelegate OnUsersEvent;

        /// <summary>
        /// This Event will be triggered when there is an update on the microphone state.
        /// </summary>
        public static event MLAppConnect.OnMicrophoneEventDelegate OnMicrophoneEvent;

        /// <summary>
        /// Register invite callback.
        /// </summary>
        /// <param name="inviteCallback">The callback function.</param>
        /// <param name="outCallbackHandle">a callback handle, used in #MLAppConnectUnregisterInviteCallback()</param>
        /// <returns>
        /// MLResult_Ok if successfully registered, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectRegisterInviteCallback(ref InviteCallback inviteCallback, ref ulong outCallbackHandle);

        /// <summary>
        /// Unregister connection callback.
        /// </summary>
        /// <param name="callbackHandle">The callback handle obtained by #MLAppConnectRegisterInviteCallback().</param>
        /// <returns>
        /// MLResult_Ok if successfully unregistered, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectUnregisterInviteCallback(ulong callbackHandle);

        /// <summary>
        /// Register connection callback.
        /// </summary>
        /// <param name="connection_callback">The callback function.</param>
        /// <param name="outCallbackHandle">The callback handle obtained by #MLAppConnectRegisterInviteCallback().</param>
        /// <returns>
        /// MLResult_Ok if successfully registered, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectRegisterConnectionCallback(ref ConnectionCallback connection_callback, ref ulong outCallbackHandle);

        /// <summary>
        /// Unregister connection callback.
        /// </summary>
        /// <param name="callbackHandle">The callback handle obtained by #MLAppConnectRegisterInviteCallback().</param>
        /// <returns>
        /// MLResult_Ok if successfully unregistered, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectUnregisterConnectionCallback(ulong callbackHandle);

        /// <summary>
        /// Create connection.
        /// Initial step to create a new connection.
        /// Invoking this function will start by displaying the "invite dialog" interface in the "initiator device".
        /// The user will be able to invite one or more of their users to the connection.Each one of the invited
        /// user will then receive a system notification to be accepted or declined.
        /// A connection callback (registered by #MLAppConnectRegisterConnectionCallback()) should be
        /// set up before calling this function.
        /// </summary>
        /// <param name="connectionName">
        /// The name of the connection to create (used for identification while
        /// managing multiple connections).
        /// </param>
        /// <param name="connectionProperties">The properties of the connection to create from #MLAppConnectConnectionProperties.</param>
        /// <returns>
        /// MLResult_Ok if successfully created a connection, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectCreateConnection([MarshalAs(UnmanagedType.LPStr)] string connectionName, ref MLAppConnect.ConnectionProperties connectionProperties);

        /// <summary>
        /// Co-join connection.
        /// Once a user is invited to join some specific connection, a system notification will be displayed
        /// and the user will be able to accept or decline.
        /// If the invite is accepted, the corresponding app will load and be ready to join the connection.
        /// The callback #MLAppConnectInviteCallback will then be triggered and bring any information related
        /// to the connection. In special, the "connection name" used by <c>#MLAppConnectCojoinConnection()</c>.
        /// A connection callback (registered by <c>#MLAppConnectRegisterConnectionCallback()</c>) should be
        /// set up before calling this function.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <returns>
        /// MLResult_Ok if successfully created a connection, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectCojoinConnection([MarshalAs(UnmanagedType.LPStr)] string connectionName);

        /// <summary>
        /// Invite users.
        /// This function is similar #MLAppConnectCreateConnection(), but works with an ongoing connection.
        /// It will also display the "invite dialog" and allow the user to invite more contacts.
        /// A connection event callback (registered by #MLAppConnectRegisterEventCallback()) should be
        /// set up before calling this function.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <returns>
        /// MLResult_Ok if successful, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectInviteUsers([MarshalAs(UnmanagedType.LPStr)] string connectionName);

        /// <summary>
        /// Delete connection.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <returns>
        /// MLResult_Ok if successfully deleted a connection, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectDeleteConnection([MarshalAs(UnmanagedType.LPStr)] string connectionName);

        /// <summary>
        /// Register connection event callback.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="eventCallback">Event callback function.</param>
        /// <param name="outCallbackHandle">A callback handle, used in #MLAppConnectUnregisterEventCallback().</param>
        /// <returns>
        /// MLResult_Ok if successfully registered, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectRegisterEventCallback([MarshalAs(UnmanagedType.LPStr)] string connectionName, ref EventCallback eventCallback, ref ulong outCallbackHandle);

        /// <summary>
        /// Unregister connection event callback.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="outCallbackHandle">A callback handle, used in #MLAppConnectUnregisterEventCallback().</param>
        /// <returns>
        /// MLResult_Ok if successfully registered, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectUnregisterEventCallback([MarshalAs(UnmanagedType.LPStr)] string connectionName, ulong outCallbackHandle);

        /// <summary>
        /// Create data pipe.
        /// Creates a "general purpose" data pipe.The functions #MLAppConnectSendData() and #MLAppConnectReadData() should
        /// be used to write to and read from this pipe.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="pipeName">The name of the pipe to create.</param>
        /// <param name="pipeProperties">The data pipe properties for this pipe.</param>
        /// <returns>
        /// MLResult_Ok if successfully created a pipe, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectCreateDataPipe([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string pipeName, ref MLAppConnect.DataPipeProperties pipeProperties);

        /// <summary>
        /// Create video pipe.
        /// Creates a "video specialized" data pipe.The functions #MLAppConnectSendVideoFrame() and
        /// #MLAppConnectReadVideoFrame() should be used to write to and read from this pipe.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="pipeName">pipeName The name of the pipe to create. </param>
        /// <param name="pipeProperties">The data pipe properties for this pipe.</param>
        /// <returns>
        ///  MLResult_Ok if successfully created a pipe, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectCreateVideoPipe([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string pipeName, ref MLAppConnect.VideoPipeProperties pipeProperties);

        /// <summary>
        /// This function should be called to delete a "data pipe" or a "video pipe" or an "audio pipe".
        /// This function should NOT be used to delete "video capture pipe" or "microphone audio pipe".
        /// For these, use <c>#MLAppConnectDeleteVideoCapturePipe()</c> and <c>#MLAppConnectDeleteMicAudioPipe()</c> instead.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="pipeName">The name of the pipe to delete.</param>
        /// <returns>
        /// MLResult_Ok if successfully deleted the pipe, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectDeletePipe([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string pipeName);

        /// <summary>
        /// Create video capture pipe.
        /// Creates a "video capture specialized" data pipe.
        /// No function should be used to write to this pipe.It is managed and fed automatically with frames.
        /// from the device's camera.
        /// The other peers of the connection should use #MLAppConnectReadVideoFrame() to read the video frames
        /// from this pipe.From the receiver side, a "video capture pipe" is no different than a "regular video pipe".
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="pipeProperties">The name of the pipe to delete.</param>
        /// <returns>
        /// MLResult_Ok if successfully created a pipe, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectCreateVideoCapturePipe([MarshalAs(UnmanagedType.LPStr)] string connectionName, ref MLAppConnect.VideoPipeProperties pipeProperties);

        /// <summary>
        /// Delete video capture pipe.
        /// </summary>
        /// <param name="connectionName">The name of the connection to delete the pipe from.</param>
        /// <returns>
        /// MLResult_Ok if successfully deleted a pipe, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectDeleteVideoCapturePipe([MarshalAs(UnmanagedType.LPStr)] string connectionName);

        /// <summary>
        /// Create microphone audio pipe.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <returns>
        /// MLResult_Ok if successfully created a pipe, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectCreateMicAudioPipe([MarshalAs(UnmanagedType.LPStr)] string connectionName);

        /// <summary>
        /// Delete the audio pipe for the (internally managed) microphone from the connection..
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <returns>
        /// MLResult_Ok if successfully deleted a pipe, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectDeleteMicAudioPipe([MarshalAs(UnmanagedType.LPStr)] string connectionName);

        /// <summary>
        /// Mute the microphone for the connection.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <returns>
        /// LResult_Ok if successfully muted a pipe, error code from #MLAppConnectResult otherwise
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectMuteMic([MarshalAs(UnmanagedType.LPStr)] string connectionName);

        /// <summary>
        /// Unmute the microphone for the connection.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <returns>
        /// MLResult_Ok if successfully unmuted a pipe, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectUnmuteMic([MarshalAs(UnmanagedType.LPStr)] string connectionName);

        /// <summary>
        /// Check if the microphone is muted for the the connection.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="outMicMuted">A flag that indicates if microphone is muted.</param>
        /// <returns>
        /// MLResult_Ok if successfully returned the value, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectIsMicMuted([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.I1)] ref bool outMicMuted);

        /// <summary>
        /// Mute user.
        /// Mute audio from another user.
        /// </summary>
        /// <param name="connectionName">connectionName The name of the connection.</param>
        /// <param name="user">The user name.</param>
        /// <returns>
        /// MLResult_Ok if successfully returned the value, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectMuteUser([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string user);

        /// <summary>
        /// Mute user.
        /// Mute audio from another user.
        /// </summary>
        /// <param name="connectionName">connectionName The name of the connection.</param>
        /// <param name="user">The user name.</param>
        /// <returns>
        /// MLResult_Ok if successfully returned the value, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectUnmuteUser([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string user);

        /// <summary>
        /// Get a list of strings with the pipe names.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="pipeNames">The list of pipe names.</param>
        /// <param name="count">Number of elements in the array.</param>
        /// <returns>
        /// MLResult_Ok if successfully returned the value, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectGetPipeNames([MarshalAs(UnmanagedType.LPStr)] string connectionName, out IntPtr pipeNames, out uint count);

        /// <summary>
        /// Free a list of pipe names.
        /// <c>Deallocates</c> a list of pipe names returned by #MLAppConnectGetPipeNames().
        /// </summary>
        /// <param name="pipeNames">The list of pipe names.</param>
        /// <param name="count">Number of elements in the array.</param>
        /// <returns>
        /// MLResult_Ok if successful in getting the value, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectFreePipeNames(IntPtr pipeNames, uint count);

        /// <summary>
        /// Send data over a pipe.
        /// This function should be called to send data over a pipe.
        /// </summary>
        /// <param name="connectionName">The name of the connection to send the data.</param>
        /// <param name="pipeName">The name of the pipe to send the data.</param>
        /// <param name="data">The data to be sent.</param>
        /// <param name="dataSize">he size of the data to be sent.</param>
        /// <returns>
        /// MLResult_Ok if successfully sent data, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectSendData([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string pipeName, IntPtr data, ulong dataSize);

        /// <summary>
        /// Send Large data over a pipe.
        /// Send Large data(>IP MTU) to a pipe of this connection.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="pipeName">The name of the pipe to send the data.</param>
        /// <param name="data">The data to be sent..</param>
        /// <param name="dataSize">The video frame.</param>
        /// <returns>
        /// MLResult_Ok if successfully sent data, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectSendLargeData([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string pipeName, IntPtr data, ulong dataSize);

        /// <summary>
        /// Send video frame.
        /// This function should be called to send video data over a pipe.
        /// </summary>
        /// <param name="connectionName">The name of the connection to send the data.</param>
        /// <param name="pipeName">The name of the pipe to send the data.</param>
        /// <param name="video_frame">The video frame.</param>
        /// <returns>
        /// MLResult_Ok if successfully sent data, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectSendVideoFrame([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string pipeName, ref MLImageNativeBindings.MLImageNative videoframe);

        /// <summary>
        /// Read data.
        /// This function will block the execution until at least #min_bytes are read from the pipe.
        /// #min_bytes can also be set to 0 and this function will return immediately with or without
        /// any incoming data.
        /// The receiving data must be freed using #MLAppConnectFreeData().
        /// </summary>
        /// <param name="connectionName">The name of the connection to read the data.</param>
        /// <param name="pipeName">The name of the pipe to read the data.</param>
        /// <param name="readDataParameters">Information about the received data and the amount of bytes to read.</param>
        /// <returns>
        /// MLResult_Ok if successfully read data, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectReadData([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string pipeName, ref ReadDataParameters readDataParameters);

        /// <summary>
        /// Free data.
        /// Free data buffers allocated by MLAppConnectReadData()
        /// </summary>
        /// <param name="data">Address of a data buffer.</param>
        /// <returns>
        /// MLResult_Ok if successfully <c>deallocates</c> the data, MLResult_UnspecifiedFailure otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectFreeData(IntPtr data);

        /// <summary>
        /// Read Large.
        /// [non-blocking; ownership of memory is transferred to the client]
        /// Read (large) data from a pipe of this connection. Due to fragmentation and reassembly
        /// of large datagrams when calling this API there is no guarantee that this datagram is received
        /// in order with data that is sent using the Send API, MLAppConnectSendLarge().
        /// The receiving data must be freed using #MLAppConnectFreeLargeData().
        /// </summary>
        /// <param name="connectionName">The name of the connection to read the data.</param>
        /// <param name="pipeName">The name of the pipe to read the data.</param>
        /// <param name="readLargeDataParameters">Information about the received data</param>
        /// <returns>
        /// MLResult_Ok if successfully read data, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectReadLargeData([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string pipeName, ref ReadLargeDataParameters readLargeDataParameters);

        /// <summary>
        /// Free large data
        /// Free data buffers allocated by MLAppConnectReadLargeData()
        /// </summary>
        /// <param name="data">Address of a data buffer.</param>
        /// <returns>
        /// MLResult_Ok if successfully <c>deallocates</c> the data, MLResult_UnspecifiedFailure otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectFreeLargeData(IntPtr data);

        /// <summary>
        /// Read a video frame from a video pipe.
        /// This will fill up outVideoFrame structure with data related to the video frame.
        /// The data can be cleaned by #MLAppConnectFreeVideoFrame() after already used.
        /// </summary>
        /// <param name="connectionName">The name of the connection to read the data.</param>
        /// <param name="pipeName">he name of the pipe to read the data.</param>
        /// <param name="waitForNextfFame">Number of frames to read.</param>
        /// <param name="outVideoFrame">Reference to an existing
        /// structure.</param>
        /// <returns>
        /// MLResult_Ok if successfully read data, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectReadVideoFrame([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string pipeName, [MarshalAs(UnmanagedType.I1)] bool waitForNextfFame, out MLImageNativeBindings.MLImageNative outVideoFrame);

        /// <summary>
        /// Free data buffers allocated by #MLAppConnectReadVideoFrame().
        /// </summary>
        /// <param name="outVideoFrame">Address of a data buffer.</param>
        /// <returns>MLResult_Ok if successfully <c>deallocates</c> the data, MLResult_UnspecifiedFailure otherwise.</returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectFreeVideoFrame(ref MLImageNativeBindings.MLImageNative outVideoFrame);

        /// <summary>
        /// Unblocks a blocked pipe.
        /// See #MLAppConnectReadData(), #MLAppConnectReadLargeData() and #MLAppConnectReadVideoFrame() for more details
        /// on why and how a pipe can get blocked.
        /// </summary>
        /// <param name="connectionName">The connection name.</param>
        /// <param name="pipeName">The pipe name.</param>
        /// <returns>
        /// MLResult_Ok if successfully unblocks read data, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectUnblockRead([MarshalAs(UnmanagedType.LPStr)] string connectionName, [MarshalAs(UnmanagedType.LPStr)] string pipeName);

        /// <summary>
        /// Get Participants.
        /// Get a list of participants in an ongoing connection.
        /// </summary>
        /// <param name="connectionName">The name of the connection.</param>
        /// <param name="outParticipants">The list of participants in this connection.</param>
        /// <param name="count">Number of entries in the outParticipants list.</param>
        /// <returns>
        /// MLResult_Ok if successfully able to get the participants
        /// error code from #MLAppConnectResult otherwise
        /// outParticipants is allocated by this function. Call free function MLAppConnectFreeParticipants() after usage.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectGetParticipants([MarshalAs(UnmanagedType.LPStr)] string connectionName, out IntPtr outParticipants, out uint count);

        /// <summary>
        /// <c>Deallocates</c> a list of participants returned by #MLAppConnectGetParticipants().
        /// </summary>
        /// <param name="participantsList">An array of C-Strings containing the list of participants.</param>
        /// <param name="count">Number of elements in the array.</param>
        /// <returns>
        /// MLResult_Ok if successful in getting the value, error code from #MLAppConnectResult otherwise.
        /// </returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern MLResult.Code MLAppConnectFreeParticipants(IntPtr participantsList, uint count);

        /// <summary>
        /// Returns an ASCII string for MLAppConnectResult/MLResultGlobal codes.
        /// </summary>
        /// <param name="resultCode">The input MLAppConnectResult/MLResultGlobal result code.</param>
        /// <returns>ASCII string containing readable version of result code.</returns>
        [DllImport(MLAppConnectDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MLAppConnectGetResultString(MLResult.Code resultCode);

        /// <summary>
        /// Internal Invite callback function.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="arguments">A list of arguments.</param>
        /// <param name="argumentsCount">Number of arguments in the list.</param>
        /// <param name="userData">User provided data.</param>
        [AOT.MonoPInvokeCallback(typeof(OnInviteCallbackPrivate))]
        public static void OnInviteCallbackInternal([MarshalAs(UnmanagedType.LPStr)] string connectionName, MLAppConnect.KeyValue[] arguments, uint argumentsCount, IntPtr userData)
        {
            MLAppConnect.ConnectionName = connectionName;
            MLThreadDispatch.Call(connectionName, OnInvite);
        }

        /// <summary>
        /// Connection callback function.
        /// </summary>
        /// <param name="result">The result status of the connection.</param>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="userData">User provided data.</param>
        [AOT.MonoPInvokeCallback(typeof(OnConnectionCallbackPrivate))]
        public static void OnConnectionCallbackInternal(MLResult.Code result, [MarshalAs(UnmanagedType.LPStr)] string connectionName, IntPtr userData)
        {
            if (MLResult.IsOK(result))
            {
                MLAppConnect.RegisterEventCallback(connectionName);
            }

            MLThreadDispatch.Call(result, connectionName, OnConnection);
        }

        /// <summary>
        /// Connection event callback function.
        /// All resources in eventInfo will be cleaned up once the callback returns.
        /// </summary>
        /// <param name="unmanagedEventInfo">Information related to the event.</param>
        /// <param name="userData">User provided data.</param>
        [AOT.MonoPInvokeCallback(typeof(OnEventCallbackPrivate))]
        public static void OnEventCallbackInternal(IntPtr unmanagedEventInfo, IntPtr userData)
        {
            EventInfoInternal internalEventInfo = (EventInfoInternal)Marshal.PtrToStructure(unmanagedEventInfo, typeof(EventInfoInternal));
            MLAppConnect.EventInfo eventInfo = new MLAppConnect.EventInfo();

            eventInfo.Version = internalEventInfo.Version;
            eventInfo.EventName = internalEventInfo.EventName;
            eventInfo.ConnectionName = internalEventInfo.ConnectionName;

            switch (eventInfo.EventName)
            {
                case MLAppConnect.EventType.UserExited:
                case MLAppConnect.EventType.UserJoined:
                case MLAppConnect.EventType.UserInvite:
                case MLAppConnect.EventType.UsersInviteAbort:
                    eventInfo.UserInfo = (MLAppConnect.UserEventInfo)Marshal.PtrToStructure(internalEventInfo.UserInfo, typeof(MLAppConnect.UserEventInfo));
                    MLThreadDispatch.Call(eventInfo.EventName, eventInfo.UserInfo, OnUsersEvent);
                    break;
                case MLAppConnect.EventType.MicMuted:
                case MLAppConnect.EventType.MicUnmuted:
                    eventInfo.MicrophoneInfo = (MLAppConnect.MicrophoneEventInfo)Marshal.PtrToStructure(internalEventInfo.MicrophoneInfo, typeof(MLAppConnect.MicrophoneEventInfo));
                    MLThreadDispatch.Call(eventInfo.EventName, eventInfo.MicrophoneInfo, OnMicrophoneEvent);
                    break;
                case MLAppConnect.EventType.PipeCreated:
                case MLAppConnect.EventType.PipeDeleted:
                case MLAppConnect.EventType.PipeFailed:
                case MLAppConnect.EventType.PipeLargeData:
                    eventInfo.PipeInfo = (MLAppConnect.PipeEventInfo)Marshal.PtrToStructure(internalEventInfo.PipeInfo, typeof(MLAppConnect.PipeEventInfo));
                    MLThreadDispatch.Call(eventInfo.EventName, eventInfo.PipeInfo, OnPipeEvent);
                    break;
                default:
                    return;
            }

            MLThreadDispatch.Call(eventInfo.EventName, OnEvent);
        }

        /// <summary>
        /// Event info.
        /// Top level structure with information from the connection event callback (#MLAppConnectEventCallback()).
        /// It contains child structures with specific information for each type of event.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct EventInfoInternal
        {
            /// <summary>
            /// Version of this structure.
            /// </summary>
            public uint Version;

            /// <summary>
            /// Event type.
            /// </summary>
            public MLAppConnect.EventType EventName;

            /// <summary>
            /// Connection name.
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string ConnectionName;

            /// <summary>
            /// Pipe Information.
            /// </summary>
            public IntPtr PipeInfo;

            /// <summary>
            /// User Information.
            /// </summary>
            public IntPtr UserInfo;

            /// <summary>
            /// Microphone Information.
            /// </summary>
            public IntPtr MicrophoneInfo;
        }

        /// <summary>
        /// Connection callback.
        /// Parameters to be used by #MLAppConnectRegisterConnectionCallback().
        /// It contains a callback function and a custom user data to be returned as argument in the callback itself.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ConnectionCallback
        {
            /// <summary>
            /// Version of this structure.
            /// </summary>
            public uint Version;

            /// <summary>
            /// Connection callback function.
            /// </summary>
            public OnConnectionCallbackDelegate OnConnectionCallback;

            /// <summary>
            /// User provided data.
            /// </summary>
            public IntPtr UserData;

            /// <summary>
            /// Connection callback function.
            /// </summary>
            /// <param name="result">The result status of the connection.</param>
            /// <param name="connectionName">Name of the connection.</param>
            /// <param name="userData">User provided data.</param>
            public delegate void OnConnectionCallbackDelegate(MLResult.Code result, [MarshalAs(UnmanagedType.LPStr)] string connectionName, IntPtr userData);

            /// <summary>
            /// Initializes the structure with default values.
            /// </summary>
            /// <returns>Initialized structure.</returns>
            public static ConnectionCallback Create()
            {
                return new ConnectionCallback()
                {
                    Version = 1,
                    OnConnectionCallback = OnConnectionCallbackInternal,
                    UserData = IntPtr.Zero
                };
            }
        }

        /// <summary>
        /// Invite callback.
        /// Parameters to be used by #MLAppConnectRegisterInviteCallback().
        /// It contains a callback function and a custom user data to be returned as argument in the callback itself.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct InviteCallback
        {
            /// <summary>
            /// Version of this structure.
            /// </summary>
            public uint Version;

            /// <summary>
            /// Invite event callback function.
            /// </summary>
            public OnInviteCalllbackDelegate OnInvite;

            /// <summary>
            /// User provided data.
            /// </summary>
            public IntPtr UserData;

            /// <summary>
            /// Connection callback function.
            /// </summary>
            /// <param name="connectionName">Name of the connection.</param>
            /// <param name="arguments">A list of arguments.</param>
            /// <param name="argumentsCount">Number of arguments in the list.</param>
            /// <param name="userData">User provided data.</param>PipeProperties
            public delegate void OnInviteCalllbackDelegate([MarshalAs(UnmanagedType.LPStr)] string connectionName, MLAppConnect.KeyValue[] arguments, uint argumentsCount, IntPtr userData);

            /// <summary>
            /// Initializes the structure with default values.
            /// </summary>
            /// <returns>Initialized structure.</returns>
            public static InviteCallback Create()
            {
                return new InviteCallback()
                {
                    Version = 1,
                    OnInvite = OnInviteCallbackInternal,
                    UserData = IntPtr.Zero
                };
            }
        }

        /// <summary>
        /// Connection event callback.
        /// Parameters to be used by #MLAppConnectRegisterEventCallback().
        /// It contains a callback function and a custom user data to be returned as argument in the callback itself.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct EventCallback
        {
            /// <summary>
            /// Version of this structure.
            /// </summary>
            public uint Version;

            /// <summary>
            /// Event callback function.
            /// </summary>
            public OnEventCallbackDelegate OnEvent;

            /// <summary>
            /// User provided data.
            /// </summary>
            public IntPtr UserData;

            /// <summary>
            /// Event callback function.
            /// All resources in eventInfo will be cleaned up once the callback returns.
            /// </summary>
            /// <param name="eventInfo">Information related to the event.</param>
            /// <param name="userData">User provided data.</param>
            public delegate void OnEventCallbackDelegate(IntPtr eventInfo, IntPtr userData);

            /// <summary>
            /// Initializes the structure with default values.
            /// </summary>
            /// <returns>Initialized structure.</returns>
            public static EventCallback Create()
            {
                return new EventCallback()
                {
                    Version = 1,
                    OnEvent = OnEventCallbackInternal,
                    UserData = IntPtr.Zero
                };
            }
        }

        /// <summary>
        /// Used as a parameter for ReadData
        /// Information about the received data from #MLAppConnect.ReadData() and the amount of bytes to read.
        /// </summary>
        public struct ReadDataParameters
        {
            /// <summary>
            /// Version of this structure.
            /// </summary>
            public uint Version;

            /// <summary>
            /// The minimum number of bytes to read before returning.
            /// </summary>
            public uint MinBytes;

            /// <summary>
            /// The size of the data that was read.
            /// </summary>
            public ulong DataSize;

            /// <summary>
            /// The number of bytes to read from the pipe.
            /// </summary>
            public IntPtr Data;

            /// <summary>
            /// <summary>
            /// Initializes the structure with default values.
            /// </summary>
            /// <returns>Initialized structure.</returns>
            public static ReadDataParameters Create()
            {
                return new ReadDataParameters()
                {
                    Version = 1,
                    MinBytes = 0,
                    DataSize = 0,
                    Data = IntPtr.Zero
                };
            }
        }

        /// <summary>
        /// Used as a parameter for ReadLargeData.
        /// Information about the received data from #MLAppConnect.ReadLargeDataParameters().
        /// </summary>
        public struct ReadLargeDataParameters
        {
            /// <summary>
            /// Version of this structure.
            /// </summary>
            public uint Version;

            /// <summary>
            /// The size of the data that was read.
            /// </summary>
            public ulong DataSize;

            /// <summary>
            /// The number of bytes to read from the pipe.
            /// </summary>
            public IntPtr Data;

            /// <summary>
            ///  The number of packets remaining.
            /// </summary>
            public ulong DataPacketsRemaining;

            /// <summary>
            /// <summary>
            /// Initializes the structure with default values.
            /// </summary>
            /// <returns>Initialized structure.</returns>
            public static ReadLargeDataParameters Create()
            {
                return new ReadLargeDataParameters()
                {
                    Version = 1,
                    DataSize = 0,
                    Data = IntPtr.Zero,
                    DataPacketsRemaining = 0,

                };
            }
        }

    }
}
#endif
