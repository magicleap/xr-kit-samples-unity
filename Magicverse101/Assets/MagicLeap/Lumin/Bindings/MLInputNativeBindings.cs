// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLInputNativeBindings.cs" company="Magic Leap, Inc">
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
    using System.Linq;
    using System.Runtime.InteropServices;

    public sealed partial class MLInput : MLAPISingleton<MLInput>
    {
        /// <summary>
        /// See ml_input.h for additional comments.
        /// </summary>
        private partial class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// The maximum number of controllers that are supported.
            /// </summary>
            public const uint MaxControllers = 2;

            /// <summary>
            /// The maximum number of controller touchpad touches that are supported.
            /// </summary>
            public const uint MaxControllerTouchpadTouches = 2;

            /// <summary>
            /// Tablet additional pen data size.
            /// </summary>
            public const int TabletAdditionalPenDataSize = 3;

            /// <summary>
            /// The number of controller touchpad gestures.
            /// </summary>
            internal const uint ControllerTouchpadGestureTypeCount = (uint)MLInput.Controller.TouchpadGesture.GestureType.Pinch + 1;

            /// <summary>
            /// The number of controller touchpad gesture directions.
            /// </summary>
            internal const uint ControllerTouchpadGestureDirectionCount = (uint)MLInput.Controller.TouchpadGesture.GestureDirection.CounterClockwise + 1;

            /// <summary>
            /// The number of controller buttons.
            /// </summary>
            internal const uint ButtonCount = (uint)MLInput.Controller.Button.HomeTap + 1;

            /// <summary>
            /// MLInput library name.
            /// </summary>
            private const string MLInputDll = "ml_input";

            /// <summary>
            /// Create an input tracker.
            /// </summary>
            /// <param name="config">The desired configuration. Pass NULL for default configuration.</param>
            /// <param name="handle">A handle to the created input tracker.</param>
            /// <returns>
            ///  MLResult.Result will be <c>MLResult.Code.Ok</c> if input tracker was created successfully.
            ///  MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if input tracker was not created due to invalid out_handle.
            ///  MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if input tracker was not created due to an unknown error.
            /// </returns>
            [DllImport(MLInputDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLInputCreate(IntPtr config, ref ulong handle);

            /// <summary>
            /// Destroy an input tracker.
            /// </summary>
            /// <param name="handle">A handle to an input tracker.</param>
            /// <returns>
            ///  MLResult.Result will be <c>MLResult.Code.Ok</c> if the input tracker was destroyed.
            ///  MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the input tracker failed to destroy due to an invalid handle.
            ///  MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLInputDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLInputDestroy(ulong handle);

            /// <summary>
            /// Gets the device IDs of all connected devices.
            /// </summary>
            /// <param name="handle">A handle to the input tracker.</param>
            /// <param name="out_devices">Pointer to MLInputConnectedDevicesList structure that will be populated.</param>
            /// <returns>
            ///  MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully fetched the IDs of the connected devices.
            ///  MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to get the IDs of all connected devices.
            ///  MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLInputDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLInputGetConnectedDevices(ulong handle, ref ConnectedDevicesListNative out_devices);

            /// <summary>
            /// Release the contents of MLInputConnectedDevicesList populated by MLInputGetConnectedDevices.
            /// </summary>
            /// <param name="handle">A handle to the input tracker.</param>
            /// <param name="devices">A pointer to MLInputConnectedDevicesList struct. Its contents will be released.</param>
            /// <returns>
            ///  MLResult.Result will be <c>MLResult.Code.Ok</c> if operation was successful.
            ///  MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if releasing the contents failed due to an invalid parameter.
            ///  MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLInputDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLInputReleaseConnectedDevicesList(ulong handle, ref ConnectedDevicesListNative devices);

            /// <summary>
            /// Sets the callbacks for tablet device input events.
            /// </summary>
            /// <param name="handle">A handle to the input tracker.</param>
            /// <param name="callbacks">A pointer to MLInputTabletDeviceCallbacks structure (can be NULL).</param>
            /// <param name="data">A A pointer to user payload data (can be NULL).</param>
            /// <returns>
            ///  MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully set the callbacks for tablet device input events.
            ///  MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to set the callback for tablet device input events due to an invalid parameter.
            ///  MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLInputDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLInputSetTabletDeviceCallbacks(ulong handle, ref TabletDeviceCallbacksNative callbacks, IntPtr data);

            /// <summary>
            /// Sets the callbacks for tablet device input events.
            /// Overloaded method to allow for easily clearing native callbacks
            /// </summary>
            /// <param name="handle">A handle to the input tracker.</param>
            /// <param name="callbacks">A pointer to MLInputTabletDeviceCallbacks structure (can be NULL).</param>
            /// <param name="data">A A pointer to user payload data (can be NULL).</param>
            /// <returns>
            ///  MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully set the callbacks for tablet device input events.
            ///  MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed to set the callback for tablet device input events due to an invalid parameter.
            ///  MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLInputDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLInputSetTabletDeviceCallbacks(ulong handle, IntPtr callbacks, IntPtr data);

            /// <summary>
            /// Return a list of states of the tablet device.
            ///  This API will return all the states of tablet device since the last query up to a maximum of 20 states.
            ///  The memory used to store the list is maintained by the library.User should call
            ///  MLInputReleaseTabletDeviceStates() to release it.
            /// </summary>
            /// <param name="handle">A handle to the input tracker.</param>
            /// <param name="tablet_device_id">A pointer to MLInputTabletDeviceStatesList structure. Its contents will be released.</param>
            /// <param name="out_tablet_device_states">A pointer to MLInputTabletDeviceStatesList structure that will be populated.
            /// The app should call MLInputReleaseTabletDeviceStates to release the contents after use.</param>
            /// <returns>
            ///  MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully fetched the tablet device state.
            ///  MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if fetching tablet device states failed due to an invalid parameter.
            ///  MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLInputDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLInputGetTabletDeviceStates(ulong handle, byte tablet_device_id, ref TabletDeviceStatesListNative out_tablet_device_states);

            /// <summary>
            /// Release the contents of MLInputTabletDeviceStatesList populated by MLInputGetTabletDeviceStates.
            /// </summary>
            /// <param name="handle">A handle to the input tracker.</param>
            /// <param name="tablet_device_states">A pointer to MLInputTabletDeviceStatesList structure. Its contents will be released.</param>
            /// <returns>
            ///  MLResult.Result will be <c>MLResult.Code.Ok</c> if operation was successful.
            ///  MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if releasing the contents failed due to an invalid parameter.
            ///  MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
            /// </returns>
            [DllImport(MLInputDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLInputReleaseTabletDeviceStates(ulong handle, ref TabletDeviceStatesListNative tablet_device_states);

            /// <summary>
            /// Converts a ConnectedDevicesListNative into a byte array of connected device id values.
            /// </summary>
            /// <param name="deviceListNative">The structure of connected devices.</param>
            /// <returns>Returns an array of connected device id values.</returns>
            public static byte[] GetConnectedTabletIds(ConnectedDevicesListNative deviceListNative)
            {
                byte[] deviceList = new byte[deviceListNative.TabletDeviceCount];
                for (int i = 0; i < deviceListNative.TabletDeviceCount; i++)
                {
                    deviceList[i] = Marshal.ReadByte(deviceListNative.TabletDeviceIds, i * Marshal.SizeOf(typeof(int)));
                }

                return deviceList;
            }

            /// <summary>
            /// Converts the native TabletDeviceStatesListNative structure into an array of TabletState structures.
            /// </summary>
            /// <param name="deviceStatesNative">The retrieved TabletDeviceStatesListNative structure.</param>
            /// <returns>Returns an array of TabletState structures.</returns>
            public static MLInput.TabletState[] DeviceStatesToArray(TabletDeviceStatesListNative deviceStatesNative)
            {
                MLInput.TabletState[] deviceStates = new MLInput.TabletState[deviceStatesNative.Count];
                for (int i = 0; i < (int)deviceStatesNative.Count; i++)
                {
                    deviceStates[i] = Marshal.PtrToStructure<NativeBindings.TabletDeviceStateNative>(deviceStatesNative.Data + (i * Marshal.SizeOf(typeof(NativeBindings.TabletDeviceStateNative)))).Data;
                }

                return deviceStates;
            }

            /// <summary>
            /// Links to MLInputTabletDeviceState in ml_input.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TabletDeviceStateNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Type of this tablet device.
                /// </summary>
                public MLInput.TabletDeviceType Type;

                /// <summary>
                /// Type of tool used with the tablet.
                /// </summary>
                public MLInput.TabletDeviceToolType ToolType;

                /// <summary>
                /// Current touch position (X,Y) and force (Z).
                /// Position is in the [-1.0,1.0] range and force is in the [0.0,1.0] range.
                /// </summary>
                public MLVec3f PenTouchPosAndForce;

                /// <summary>
                /// Additional coordinate values (X,Y,Z)
                /// It could contain data specific to the device type.
                /// Example: It could hold tilt values while using pen
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = TabletAdditionalPenDataSize)]
                public int[] AdditionalPenTouchData;

                /// <summary>
                /// Is touch active.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool IsPenTouchActive;

                /// <summary>
                /// If this tablet is connected.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool IsConnected;

                /// <summary>
                /// Distance between pen and tablet.
                /// </summary>
                public float PenDistance;

                /// <summary>
                /// Time stamp of the event.
                /// </summary>
                public ulong TimeStamp;

                /// <summary>
                /// Flags to denote which of the above fields are valid.
                /// </summary>
                public uint ValidFieldsFlag;

                /// <summary>
                /// Gets the tablet state from the internal format.
                /// </summary>
                public MLInput.TabletState Data
                {
                    get
                    {
                        MLInput.TabletState state = new MLInput.TabletState();

                        state.Type = this.Type;
                        state.ToolType = this.ToolType;
                        state.PenTouchPosAndForce = new Vector3(this.PenTouchPosAndForce.X, this.PenTouchPosAndForce.Y, this.PenTouchPosAndForce.Z);

                        state.AdditionalPenTouchData = new int[NativeBindings.TabletAdditionalPenDataSize];
                        if (this.AdditionalPenTouchData != null && this.AdditionalPenTouchData.Length > 0)
                        {
                            for (int i = 0; i < NativeBindings.TabletAdditionalPenDataSize; i++)
                            {
                                state.AdditionalPenTouchData[i] = this.AdditionalPenTouchData[i];
                            }
                        }

                        state.IsPenTouchActive = this.IsPenTouchActive;
                        state.IsConnected = this.IsConnected;
                        state.PenDistance = this.PenDistance;
                        state.TimeStamp = this.TimeStamp;
                        state.ValidityCheck = (MLInput.TabletDeviceStateMask)this.ValidFieldsFlag;
                        return state;
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>Returns a new instance of the MLInputTabletDeviceStateNative structure.</returns>
                public static TabletDeviceStateNative Create()
                {
                    return new TabletDeviceStateNative
                    {
                        Version = 1u,
                        Type = MLInput.TabletDeviceType.Unknown,
                        ToolType = MLInput.TabletDeviceToolType.Unknown,
                        PenTouchPosAndForce = new MLVec3f() { X = 0.0f, Y = 0.0f, Z = 0.0f },
                        AdditionalPenTouchData = Enumerable.Repeat<int>(0, TabletAdditionalPenDataSize).ToArray(),
                        IsPenTouchActive = false,
                        IsConnected = false,
                        PenDistance = 0.0f,
                        TimeStamp = 0u,
                        ValidFieldsFlag = 0u
                    };
                }
            }

            /// <summary>
            /// Links to MLInputTabletDeviceStatesList in ml_input.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TabletDeviceStatesListNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Number of tablet device states in this list.
                /// </summary>
                public uint Count;

                /// <summary>
                /// Pointer referring to the array of MLInputTabletDeviceState.
                /// </summary>
                public IntPtr Data;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>Returns a new instance of the TabletDeviceStatesListNative structure.</returns>
                public static TabletDeviceStatesListNative Create()
                {
                    return new TabletDeviceStatesListNative
                    {
                        Version = 1u,
                        Count = 0u,
                        Data = IntPtr.Zero
                    };
                }
            }

            /// <summary>
            /// Links to MLInputConnectedDevicesList in ml_input.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ConnectedDevicesListNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Number of connected controllers.
                /// </summary>
                public uint ControllerCount;

                /// <summary>
                /// Pointer to the array of connected controller IDs(byte).
                /// </summary>
                public IntPtr ControllerIds;

                /// <summary>
                /// Number of connected tablet devices.
                /// </summary>
                public uint TabletDeviceCount;

                /// <summary>
                /// Pointer to the array of connected tablet device IDs(byte).
                /// </summary>
                public IntPtr TabletDeviceIds;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>Returns a new instance of the MLInputConnectedDevicesListNative structure.</returns>
                public static ConnectedDevicesListNative Create()
                {
                    return new ConnectedDevicesListNative
                    {
                        Version = 1u,
                        ControllerCount = 0u,
                        ControllerIds = IntPtr.Zero,
                        TabletDeviceCount = 0u,
                        TabletDeviceIds = IntPtr.Zero
                    };
                }
            }

            /// <summary>
            /// Links to MLInputTabletDeviceCallbacks in ml_input.h.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TabletDeviceCallbacksNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// This callback occurs when the pen touches the tablet.
                /// </summary>
                public OnPenTouchEventCallback OnPenTouchEvent;

                /// <summary>
                /// This callback occurs when the tablet ring is touched.
                /// </summary>
                public OnTouchRingEventCallback OnTouchRingEvent;

                /// <summary>
                /// This callback occurs when buttons on the tablet are pressed.
                /// </summary>
                public OnButtonDownCallback OnButtonDown;

                /// <summary>
                /// This callback occurs when buttons on the tablet are released.
                /// </summary>
                public OnButtonUpCallback OnButtonUp;

                /// <summary>
                /// This callback occurs when the tablet connects.
                /// </summary>
                public OnConnectCallback OnConnect;

                /// <summary>
                /// This callback occurs when the tablet disconnects.
                /// </summary>
                public OnDisconnectCallback OnDisconnect;

                /// <summary>
                /// A delegate for when a pen touches the tablet.
                /// </summary>
                /// <param name="tablet_device_id">The id of the tablet.</param>
                /// <param name="tablet_device_state">The state of the tablet.</param>
                /// <param name="data">A pointer to user payload data (can be NULL).</param>
                public delegate void OnPenTouchEventCallback(byte tablet_device_id, ref TabletDeviceStateNative tablet_device_state, IntPtr data);

                /// <summary>
                /// A delegate for when the tablet ring is touched.
                /// </summary>
                /// <param name="tablet_device_id">The id of the tablet.</param>
                /// <param name="touch_ring_value">The value of the tablet touch ring.</param>
                /// <param name="timestamp">The time the callback occurred.</param>
                /// <param name="data">A pointer to user payload data (can be NULL).</param>
                public delegate void OnTouchRingEventCallback(byte tablet_device_id, int touch_ring_value, ulong timestamp, IntPtr data);

                /// <summary>
                /// A delegate for when a tablet button is pressed.
                /// </summary>
                /// <param name="tablet_device_id">The id of the tablet.</param>
                /// <param name="tablet_device_button">The tablet button that was pressed.</param>
                /// <param name="timestamp">The time the callback occurred.</param>
                /// <param name="data">A pointer to user payload data (can be NULL).</param>
                public delegate void OnButtonDownCallback(byte tablet_device_id, MLInput.TabletDeviceButton tablet_device_button, ulong timestamp, IntPtr data);

                /// <summary>
                /// A delegate for when a tablet button is released.
                /// </summary>
                /// <param name="tablet_device_id">The id of the tablet.</param>
                /// <param name="tablet_device_button">The tablet button that was released.</param>
                /// <param name="timestamp">The time the callback occurred.</param>
                /// <param name="data">A pointer to user payload data (can be NULL).</param>
                public delegate void OnButtonUpCallback(byte tablet_device_id, MLInput.TabletDeviceButton tablet_device_button, ulong timestamp, IntPtr data);

                /// <summary>
                /// A delegate for when a tablet connects.
                /// </summary>
                /// <param name="tablet_device_id">The id of the tablet.</param>
                /// <param name="data">A pointer to user payload data (can be NULL).</param>
                public delegate void OnConnectCallback(byte tablet_device_id, IntPtr data);

                /// <summary>
                /// A delegate for when a tablet disconnects.
                /// </summary>
                /// <param name="tablet_device_id">The id of the tablet.</param>
                /// <param name="data">A pointer to user payload data (can be NULL).</param>
                public delegate void OnDisconnectCallback(byte tablet_device_id, IntPtr data);

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>Returns a new instance of the MLInputTabletDeviceCallbacksNative structure.</returns>
                public static TabletDeviceCallbacksNative Create()
                {
                    return new TabletDeviceCallbacksNative
                    {
                        Version = 1u,
                        OnPenTouchEvent = null,
                        OnTouchRingEvent = null,
                        OnButtonDown = null,
                        OnButtonUp = null,
                        OnConnect = null,
                        OnDisconnect = null
                    };
                }
            }
        }
    }
}

#endif
