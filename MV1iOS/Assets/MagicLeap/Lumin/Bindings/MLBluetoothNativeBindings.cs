// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLBluetoothNativeBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// --------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.XR.MagicLeap;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// Magic Leap Bluetooth Low Energy implementation for Unity
    /// </summary>
    public sealed partial class MLBluetoothLE : MLAPISingleton<MLBluetoothLE>
    {
#if PLATFORM_LUMIN
        /// <summary>
        /// Native bindings helper class for Magic Leap Bluetooth LE support 
        /// </summary>
        private class NativeBindings
        {
            /// <summary>
            /// The name of the Adapter DLL to retrieve adapter API
            /// </summary>
            private const string MLBluetoothAdapterDll = "ml_bluetooth_adapter";

            /// <summary>
            /// The name of the <c>Gatt</c> DLL to retrieve <c>Gatt</c> API
            /// </summary>
            private const string MLBluetoothGattDll = "ml_bluetooth_gatt";

            /// <summary>
            /// The name of the DLL to look for the core BLE API
            /// </summary>
            private const string MLBluetoothLEDll = "ml_bluetooth_le";

            /// <summary>
            /// A native mode helper API
            /// </summary>
            private const string MLBluetoothNativeHelper = "ml_bluetoothle_plugin";

            /// <summary>
            /// The constant length of a Bluetooth Address
            /// </summary>
            private const int AddressLength = 17;

            /// <summary>
            /// The constant length of a string UUID
            /// </summary>
            private const int UUIDLENGTH = 36;

            /// <summary>
            /// The defined constant length of a device name
            /// </summary>
            private const int NameLength = 249;

            /// <summary>
            /// The maximum length of a characteristic/descriptor type buffer.
            /// </summary>
            private const int BufferLength = 600;

            /// <summary>
            /// A delegate that describes the requirements of the scan result callback
            /// </summary>
            /// <param name="device">Device being returned</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ScanResultDelegate(ref ScanResult device);

            /// <summary>
            /// Delegate describing the callback necessary to monitor <c>Gatt</c> connection states
            /// </summary>
            /// <param name="status"><c>Gatt</c> Status</param>
            /// <param name="state"><c>Gatt</c> connection state</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GattConnectionStateChangedDelegate(MLBluetoothLE.Status status, MLBluetoothLE.GattConnectionState state);

            /// <summary>
            /// Delegate describing the callback necessary to monitor <c>Gatt</c> status
            /// </summary>
            /// <param name="status"><c>Gatt</c> client status</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GattServicesDiscoveredDelegate(MLBluetoothLE.Status status);

            /// <summary>
            /// Delegate describing the callback used to monitor Received Single Strength Indicators
            /// </summary>
            /// <param name="rssi">Remote Signal Strength Indicator</param>
            /// <param name="status">Client status</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GattReadRemoteRssiDelegate(int rssi, MLBluetoothLE.Status status);

            /// <summary>
            /// Delegate describing the callback used to monitor the MTU size
            /// </summary>
            /// <param name="mtu">Remote Signal Strength Indicator</param>
            /// <param name="status">Client status</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GattMTUChangedDelegate(int mtu, MLBluetoothLE.Status status);

            /// <summary>
            /// Delegate describing the callback used to read <c>Gatt</c> characteristics
            /// </summary>
            /// <param name="characteristic">Characteristic being read</param>
            /// <param name="status">Client status</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GattCharacteristicReadDelegate(CharacteristicInternal characteristic, MLBluetoothLE.Status status);

            /// <summary>
            /// Delegate describing the callback used to monitor <c>Gatt</c> interval updates
            /// </summary>
            /// <param name="interval">New connection interval</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GattConnectionIntervalUpdatedDelegate(int interval);

            /// <summary>
            /// Delegate describing the callback used to monitor remote <c>Gatt</c> characteristics changes
            /// </summary>
            /// <param name="characteristic">Characteristic that changed</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GattRemoteCharacteristicChangedDelegate(CharacteristicInternal characteristic);

            /// <summary>
            /// Delegate describing the callback used to monitor <c>Gatt</c> descriptor writes
            /// </summary>
            /// <param name="descriptor">Descriptor that was written</param>
            /// <param name="status">Write status</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GattDescriptorWriteDelegate(DescriptorInternal descriptor, MLBluetoothLE.Status status);

            /// <summary>
            /// Delegate describing the callback necessary to monitor <c>Gatt</c> descriptor changes
            /// </summary>
            /// <param name="descriptor">Descriptor that changed</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GattNotifyDelegate(DescriptorInternal descriptor);

            /// <summary>
            /// Delegate describing the callback indicating a <c>Gatt</c> characteristic write change
            /// </summary>
            /// <param name="characteristic">Characteristic that was written</param>
            /// <param name="status">Write status</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GattCharacteristicWriteDelegate(CharacteristicInternal characteristic, MLBluetoothLE.Status status);

            /// <summary>
            /// Delegate describing the callback indicating a <c>Gatt</c> Descriptor read has been successful
            /// </summary>
            /// <param name="descriptor">Descriptor being read</param>
            /// <param name="status">Read status</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GattDescriptorReadDelegate(DescriptorInternal descriptor, MLBluetoothLE.Status status);

            /// <summary>
            /// A delegate describing the requirements for the state change callback
            /// </summary>
            /// <param name="adapterState">Adapter that changed</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void AdapterStateChangedDelegate([MarshalAs(UnmanagedType.I1)] MLBluetoothLE.AdapterState adapterState);

            /// <summary>
            /// A delegate describing the requirements for the bond state callback
            /// </summary>
            /// <param name="device">Device that changed</param>
            /// <param name="state">New state</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void BondStateChangedDelegate(ref DeviceInternal device, MLBluetoothLE.BondState state);

            /// <summary>
            /// A delegate describing the requirements for the <c>Acl</c> State change callback
            /// </summary>
            /// <param name="device">Device that changed</param>
            /// <param name="state">New state</param>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void AclStateChangedDelegate(ref DeviceInternal device, MLBluetoothLE.AclState state);

            /// <summary>
            /// Start up the Bluetooth services and register all static callbacks to call user facing events.
            /// </summary>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            public static MLResult StartupBluetooth()
            {
                // Register scanner callbacks
                RegisterBluetoothScannerCallback(ScanInternalCallback);

                // Register adapter callbacks
                RegisterBluetoothAdapterAclCallback(ACLInternalCallback);
                RegisterBluetoothAdapterChangedCallback(StateChangedInternalCallback);
                RegisterBluetoothBondChangedCallback(BondStateChangedInternalCallback);

                // Register Gatt callbacks
                RegisterBluetoothGattConnectionParametersUpdatedCallback(GattConnectionIntervalUpdatedCallback);
                RegisterBluetoothGattConnectionStateChangedCallback(GattConnectionStateChangedCallback);
                RegisterBluetoothGattMTUChangedCallback(GattMTUSizeChangedCallback);
                RegisterBluetoothGattNotifyCallback(GattNotifyCallback);
                RegisterBluetoothGattReadCharacteristicCallback(GattCharacteristicReadCallback);
                RegisterBluetoothGattReadDescriptorCallback(GattDescriptorReadCallback);
                RegisterBluetoothGattWriteCharacteristicCallback(GattCharacteristicWriteCallback);
                RegisterBluetoothGattWriteDescriptorCallback(GattDescriptorWriteCallback);
                RegisterBluetoothGattReadRemoteRssi(GattReadRemoteRssiCallback);
                RegisterBluetoothGattServicesDiscoveredCallback(GattServicesDiscoveredCallback);

                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Associate a <c>Gatt</c> profile with a MLBluetoothLE client
            /// </summary>
            /// <param name="bluetoothAddress">Address to associate, returned from the scan</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            public static MLResult GattConnect(string bluetoothAddress)
            {
                BluetoothAddress address = new BluetoothAddress();

                address.SetAddress(bluetoothAddress);

                // Register the local Gatt callbacks
                MLResult result = MLResult.Create(MLBluetoothGattConnect(ref address));

                return result;
            }

            /// <summary>
            /// Gets a list of GATT services offered by the remote devices
            /// </summary>
            /// <param name="serviceList">Returns the service list</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            public static MLResult GetServiceRecord(out MLBluetoothLE.Service[] serviceList)
            {
                // TODO: Need to construct C API to take the internal linked list and return an array.
                List<MLBluetoothLE.Service> list = new List<Service>();

                ServiceListInternal returnedList = ServiceListInternal.Create();

                MLResult.Code result = MLBluetoothGattGetServiceRecord(ref returnedList);

                if (!MLResult.IsOK(result))
                {
                    serviceList = list.ToArray();
                    return MLResult.Create(result);
                }

                IntPtr serviceArray = returnedList.Services;

                for (long n = 0; n < returnedList.Count; ++n)
                {
                    GattServiceInternal nativeService = Marshal.PtrToStructure<GattServiceInternal>(serviceArray);

                    list.Add(nativeService.Data);

                    serviceArray += System.Runtime.InteropServices.Marshal.SizeOf<GattServiceInternal>();
                }

                MLBluetoothGattReleaseServiceList(ref returnedList);

                serviceList = list.ToArray();

                return MLResult.Create(result);
            }

            /// <summary>
            /// Reads the requested characteristic from the connected remote device
            /// </summary>
            /// <param name="characteristic">The characteristic to read from the remote device</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            public static MLResult.Code ReadCharacteristic(ref MLBluetoothLE.Characteristic characteristic)
            {
                CharacteristicInternal characteristicInternal = new CharacteristicInternal();

                characteristicInternal.Data = characteristic;

                MLResult.Code result = MLBluetoothGattReadCharacteristic(ref characteristicInternal);

                characteristic = characteristicInternal.Data;

                return result;
            }

            /// <summary>
            /// Writes a given characteristic and its value to the connected remote device
            /// </summary>
            /// <param name="characteristic">The characteristic to write on the remote device</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            public static MLResult.Code WriteCharacteristic(MLBluetoothLE.Characteristic characteristic)
            {
                CharacteristicInternal characteristicInternal = new CharacteristicInternal
                {
                    Data = characteristic
                };

                return MLBluetoothGattWriteCharacteristic(ref characteristicInternal);
            }

            /// <summary>
            /// Reads the requested descriptor from the connected remote device
            /// </summary>
            /// <param name="descriptor">The descriptor to read from the remote device</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            public static MLResult.Code ReadDescriptor(ref MLBluetoothLE.Descriptor descriptor)
            {
                DescriptorInternal descriptorInternal = new DescriptorInternal();

                descriptorInternal.Data = descriptor;

                MLResult.Code result = MLBluetoothGattReadDescriptor(ref descriptorInternal);

                descriptor = descriptorInternal.Data;

                return result;
            }

            /// <summary>
            /// Writes the value of a given descriptor to the connected device
            /// </summary>
            /// <param name="descriptor">The descriptor to write to the remote device</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            public static MLResult.Code WriteDescriptor(MLBluetoothLE.Descriptor descriptor)
            {
                DescriptorInternal descriptorInternal = new DescriptorInternal
                {
                    Data = descriptor
                };

                return MLBluetoothGattWriteDescriptor(ref descriptorInternal);
            }

            /// <summary>
            /// Enables or disables notifications/indications for a given characteristic
            /// </summary>
            /// <param name="characteristic">characteristic The descriptor for which to enable notification</param>
            /// <param name="enabled">enable Set to true to enable notification or indications</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            public static MLResult.Code EnableCharacteristicNotification(MLBluetoothLE.Characteristic characteristic, bool enabled)
            {
                CharacteristicInternal characteristicInternal = new CharacteristicInternal
                {
                    Data = characteristic
                };

                return MLBluetoothGattSetCharacteristicNotification(ref characteristicInternal, enabled);
            }

            /// <summary>
            /// Gets the adapter name
            /// </summary>
            /// <param name="name">Returns the adapter name</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            public static MLResult GetAdapterName(out string name)
            {
                BluetoothName bleName = new BluetoothName();

                MLResult.Code result = MLBluetoothAdapterGetName(ref bleName);

                name = bleName.ToString();

                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Start scanning for devices
            /// </summary>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothLEDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothLeStartScan();

            /// <summary>
            /// Stop scanning for devices
            /// </summary>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothLEDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothLeStopScan();

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code RegisterBluetoothScannerCallback(ScanResultDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code RegisterBluetoothAdapterChangedCallback(AdapterStateChangedDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code RegisterBluetoothAdapterAclCallback(AclStateChangedDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code RegisterBluetoothBondChangedCallback(BondStateChangedDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern void RegisterBluetoothGattConnectionStateChangedCallback(GattConnectionStateChangedDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern void RegisterBluetoothGattServicesDiscoveredCallback(GattServicesDiscoveredDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern void RegisterBluetoothGattReadRemoteRssi(GattReadRemoteRssiDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern void RegisterBluetoothGattReadCharacteristicCallback(GattCharacteristicReadDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern void RegisterBluetoothGattWriteCharacteristicCallback(GattCharacteristicWriteDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern void RegisterBluetoothGattReadDescriptorCallback(GattDescriptorReadDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern void RegisterBluetoothGattWriteDescriptorCallback(GattDescriptorWriteDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern void RegisterBluetoothGattNotifyCallback(GattNotifyDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern void RegisterBluetoothGattConnectionParametersUpdatedCallback(GattConnectionIntervalUpdatedDelegate callback);

            [DllImport(MLBluetoothNativeHelper, CallingConvention = CallingConvention.StdCall)]
            public static extern void RegisterBluetoothGattMTUChangedCallback(GattMTUChangedDelegate callback);

            [DllImport(MLBluetoothAdapterDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothAdapterGetName(ref BluetoothName name);

            /// <summary>
            /// Get the state of local Bluetooth adapter
            /// </summary>
            /// <param name="state">The state of local Bluetooth adapter</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothAdapterDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothAdapterGetState(ref MLBluetoothLE.AdapterState state);

            /// <summary>
            /// Initiates bonding request with remote device
            /// </summary>
            /// <param name="bluetoothAddress">The address of remote device to bond with</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothAdapterDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothAdapterCreateBond(ref BluetoothAddress bluetoothAddress);

            /// <summary>
            /// Enables or disables notifications/indications for a given characteristic
            /// </summary>
            /// <param name="characteristic">The descriptor for which to enable notification</param>
            /// <param name="enable">Set to true to enable notification or indications</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothGattSetCharacteristicNotification(ref CharacteristicInternal characteristic, bool enable);

            /// <summary>
            /// Reads the requested characteristic from the connected remote device
            /// </summary>
            /// <param name="descriptor">The characteristic to read from the remote device</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothGattReadCharacteristic(ref CharacteristicInternal descriptor);

            /// <summary>
            /// Writes a given characteristic and its value to the connected remote device
            /// </summary>
            /// <param name="descriptor">The characteristic to write on the remote device</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothGattWriteCharacteristic(ref CharacteristicInternal descriptor);

            /// <summary>
            /// Writes the value of a given descriptor to the connected device
            /// </summary>
            /// <param name="descriptor">The descriptor to write to the remote device</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothGattWriteDescriptor(ref DescriptorInternal descriptor);

            /// <summary>
            /// Reads the requested descriptor from the connected remote device
            /// </summary>
            /// <param name="descriptor">The descriptor to read from the remote device</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothGattReadDescriptor(ref DescriptorInternal descriptor);

            /// <summary>
            /// Requests a connection parameter update
            /// </summary>
            /// <param name="priority">A specific connection priority</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothGattRequestConnectionPriority(MLBluetoothLE.ConnectionPriority priority);

            /// <summary>
            /// Initiates a connection to a Bluetooth GATT capable device
            /// </summary>
            /// <param name="bluetoothAddress">The Bluetooth address of device to connect to</param>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothGattConnect(ref BluetoothAddress bluetoothAddress);

            /// <summary>
            /// Disconnects an established connection, or cancel a connection attempt
            /// </summary>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothGattDisconnect();

            /// <summary>
            /// Discovers GATT services offered by a remote device
            /// </summary>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothGattDiscoverServices();

            /// <summary>
            /// Reads the RSSI for a connected remote device. The <c>on_gatt_read_remote_rssi</c> 
            /// callback will be invoked when the RSSI value has been read.
            /// </summary>
            /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            public static extern MLResult.Code MLBluetoothGattReadRemoteRssi();

            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            private static extern MLResult.Code MLBluetoothGattGetServiceRecord(ref ServiceListInternal serviceList);

            [DllImport(MLBluetoothGattDll, CallingConvention = CallingConvention.StdCall)]
            private static extern MLResult.Code MLBluetoothGattReleaseServiceList(ref ServiceListInternal serviceList);

            /// <summary>
            /// Callback that is invoked when a device has been scanned
            /// </summary>
            /// <param name="inDevice">The native device information</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.ScanResultDelegate))]
            private static void ScanInternalCallback(ref ScanResult inDevice)
            {
                // Now dispatch the event
                MLThreadDispatch.Call<MLBluetoothLE.Device>(inDevice.Device.Data, MLBluetoothLE.OnScanResult);
            }

            /// <summary>
            /// Callback from native code to indicate the adapter state has changed
            /// </summary>
            /// <param name="inState">New adapter state</param>
            [AOT.MonoPInvokeCallback(typeof(AdapterStateChangedDelegate))]
            private static void StateChangedInternalCallback([MarshalAs(UnmanagedType.I1)] MLBluetoothLE.AdapterState inState)
            {
                MLThreadDispatch.Call<MLBluetoothLE.AdapterState>(inState, MLBluetoothLE.OnAdapterStateChanged);
            }

            /// <summary>
            /// Callback from the native code to indicate the <c>Acl</c> State has changed
            /// </summary>
            /// <param name="inDevice">Device that has changed</param>
            /// <param name="inState">New <c>Acl</c> state</param>
            [AOT.MonoPInvokeCallback(typeof(AclStateChangedDelegate))]
            private static void ACLInternalCallback(ref DeviceInternal inDevice, MLBluetoothLE.AclState inState)
            {
                MLThreadDispatch.Call<MLBluetoothLE.Device, MLBluetoothLE.AclState>(inDevice.Data, inState, MLBluetoothLE.OnAclStateChanged);
            }

            /// <summary>
            /// Callback from the native code to indicate the bond state has changed.
            /// </summary>
            /// <param name="inDevice">Device that has changed</param>
            /// <param name="inState">The new bond state</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.BondStateChangedDelegate))]
            private static void BondStateChangedInternalCallback(ref DeviceInternal inDevice, MLBluetoothLE.BondState inState)
            {
                MLThreadDispatch.Call<MLBluetoothLE.Device, MLBluetoothLE.BondState>(inDevice.Data, inState, MLBluetoothLE.OnBondStateChanged);
            }

            /// <summary>
            /// Callback from the native code to indicate the connection state has changed
            /// </summary>
            /// <param name="status">New connection status</param>
            /// <param name="state">New connection state</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.GattConnectionStateChangedDelegate))]
            private static void GattConnectionStateChangedCallback(MLBluetoothLE.Status status, MLBluetoothLE.GattConnectionState state)
            {
                MLThreadDispatch.Call<MLBluetoothLE.Status, MLBluetoothLE.GattConnectionState>(status, state, MLBluetoothLE.OnBluetoothConnectionStateChanged);
            }

            /// <summary>
            /// Callback from native code describing the callback necessary to monitor <c>Gatt</c> service discovery
            /// </summary>
            /// <param name="status"><c>Gatt</c> client status</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.GattServicesDiscoveredDelegate))]
            private static void GattServicesDiscoveredCallback(MLBluetoothLE.Status status)
            {
                MLThreadDispatch.Call<MLBluetoothLE.Status>(status, MLBluetoothLE.OnBluetoothServicesDiscovered);
            }

            /// <summary>
            /// Callback used to monitor Received Single Strength Indicators
            /// </summary>
            /// <param name="rssi">Remote Signal Strength Indicator</param>
            /// <param name="status">Client status</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.GattReadRemoteRssiDelegate))]
            private static void GattReadRemoteRssiCallback(int rssi, MLBluetoothLE.Status status)
            {
                MLThreadDispatch.Call<int, MLBluetoothLE.Status>(rssi, status, MLBluetoothLE.OnBluetoothReadRemoteRssi);
            }

            /// <summary>
            /// Callback used to monitor the MTU size
            /// </summary>
            /// <param name="mtu">new MTU size</param>
            /// <param name="status">Client status</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.GattReadRemoteRssiDelegate))]
            private static void GattMTUSizeChangedCallback(int mtu, MLBluetoothLE.Status status)
            {
                MLThreadDispatch.Call<int, MLBluetoothLE.Status>(mtu, status, MLBluetoothLE.OnBluetoothMTUSizeChanged);
            }

            /// <summary>
            /// Callback used to read <c>Gatt</c> characteristics
            /// </summary>
            /// <param name="characteristic">Characteristic being read</param>
            /// <param name="status">Client status</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.GattCharacteristicReadDelegate))]
            private static void GattCharacteristicReadCallback(CharacteristicInternal characteristic, MLBluetoothLE.Status status)
            {
                MLThreadDispatch.Call<Characteristic, MLBluetoothLE.Status>(characteristic.Data, status, MLBluetoothLE.OnBluetoothCharacteristicRead);
            }

            /// <summary>
            /// Callback indicating a <c>Gatt</c> characteristic write change
            /// </summary>
            /// <param name="characteristic">Characteristic that was written</param>
            /// <param name="status">Write status</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.GattCharacteristicWriteDelegate))]
            private static void GattCharacteristicWriteCallback(CharacteristicInternal characteristic, MLBluetoothLE.Status status)
            {
                MLThreadDispatch.Call<Characteristic, MLBluetoothLE.Status>(characteristic.Data, status, MLBluetoothLE.OnBluetoothCharacteristicWrite);
            }

            /// <summary>
            /// Delegate describing the callback used to monitor <c>Gatt</c> interval updates
            /// </summary>
            /// <param name="interval">New connection interval</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.GattConnectionIntervalUpdatedDelegate))]
            private static void GattConnectionIntervalUpdatedCallback(int interval)
            {
                MLThreadDispatch.Call<int>(interval, MLBluetoothLE.OnBluetoothGattConnectionIntervalUpdated);
            }

            /// <summary>
            /// Delegate describing the callback used to monitor remote <c>Gatt</c> characteristics changes
            /// </summary>
            /// <param name="characteristic">Characteristic that changed</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.GattRemoteCharacteristicChangedDelegate))]
            private static void GattRemoteCharacteristicChangedCallback(CharacteristicInternal characteristic)
            {
                MLThreadDispatch.Call<Characteristic>(characteristic.Data, MLBluetoothLE.OnBluetoothGattRemoteCharacteristicChanged);
            }

            /// <summary>
            /// Delegate describing the callback necessary to monitor <c>Gatt</c> descriptor changes
            /// </summary>
            /// <param name="descriptor">Descriptor that changed</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.GattNotifyDelegate))]
            private static void GattNotifyCallback(DescriptorInternal descriptor)
            {
                // MLThreadDispatch.Call<Descriptor>(descriptor.Data, MLBluetoothLE.OnBluetoothGattNotify);
            }

            /// <summary>
            /// Callback used to monitor <c>Gatt</c> descriptor writes
            /// </summary>
            /// <param name="descriptor">Descriptor that was written</param>
            /// <param name="status">Write status</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.GattDescriptorWriteDelegate))]
            private static void GattDescriptorWriteCallback(DescriptorInternal descriptor, MLBluetoothLE.Status status)
            {
                MLThreadDispatch.Call<Descriptor, MLBluetoothLE.Status>(descriptor.Data, status, MLBluetoothLE.OnBluetoothGattDescriptorWrite);
            }

            /// <summary>
            /// Delegate describing the callback indicating a <c>Gatt</c> Descriptor read has been successful
            /// </summary>
            /// <param name="descriptor">Descriptor being read</param>
            /// <param name="status">Read status</param>
            [AOT.MonoPInvokeCallback(typeof(NativeBindings.GattDescriptorReadDelegate))]
            private static void GattDescriptorReadCallback(DescriptorInternal descriptor, MLBluetoothLE.Status status)
            {
                MLThreadDispatch.Call<Descriptor, MLBluetoothLE.Status>(descriptor.Data, status, MLBluetoothLE.OnBluetoothGattDescriptorRead);
            }

            /// <summary>
            /// The internal representation of a device
            /// </summary>
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct DeviceInternal
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// The address of the device
                /// </summary>
                public BluetoothAddress Address;

                /// <summary>
                /// The name of the device
                /// </summary>
                public BluetoothName Name;

                /// <summary>
                /// The Remote Signal Strength Indicator
                /// </summary>
                public byte Rssi;

                /// <summary>
                /// The type of Bluetooth Device
                /// </summary>
                public MLBluetoothLE.DeviceType DeviceType;

                /// <summary>
                /// Gets or sets MLBluetoothLE.Device data, doing data conversions if necessary for the C API to consume.
                /// </summary>
                public MLBluetoothLE.Device Data
                {
                    get
                    {
                        MLBluetoothLE.Device result = new MLBluetoothLE.Device();
                        result.Rssi = this.Rssi;
                        result.DeviceType = this.DeviceType;
                        result.Address = this.Address.ToString();
                        result.Name = this.Name.ToString();

                        return result;
                    }

                    set
                    {
                        this.Version = 1;
                        this.Name.SetName(value.Name);
                        this.Address.SetAddress(value.Address);
                        this.Rssi = value.Rssi;
                        this.DeviceType = value.DeviceType;
                    }
                }
            }

            /// <summary>
            /// A structure containing the contents of a GATT characteristic
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct CharacteristicInternal
            {
                /// <summary>
                /// Version number of this struct
                /// </summary>
                public int Version;

                /// <summary>
                /// UUID of the characteristic
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = UUIDLENGTH + 1)]
                public byte[] Uuid;

                /// <summary>
                /// The Instance ID
                /// </summary>
                public int InstanceId;

                /// <summary>
                /// Permissions of the characteristics
                /// </summary>
                public MLBluetoothLE.AttributePermissions Permissions;

                /// <summary>
                /// Properties of the characteristic
                /// </summary>
                public MLBluetoothLE.CharacteristicProperties Properties;

                /// <summary>
                /// The write type of the characteristic
                /// </summary>
                public MLBluetoothLE.WriteType WriteType;

                /// <summary>
                /// The value associated with the characteristic
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = BufferLength)]
                public byte[] Buffer;

                /// <summary>
                /// size of the value
                /// </summary>
                public int BufferSize;

                /// <summary>
                /// Pointer to a descriptor list included
                /// </summary>
                public IntPtr Descriptors;

                /// <summary>
                /// Gets or sets the internal characteristics from the public facing characteristics
                /// </summary>
                public MLBluetoothLE.Characteristic Data
                {
                    get
                    {
                        MLBluetoothLE.Characteristic result = MLBluetoothLE.Characteristic.Create();

                        result.InstanceId = this.InstanceId;
                        result.Uuid = System.Text.Encoding.ASCII.GetString(this.Uuid).TrimEnd('\0');
                        result.Permissions = this.Permissions;
                        result.Properties = this.Properties;
                        result.WriteType = this.WriteType;
                        result.Buffer = new byte[this.BufferSize];
                        Array.Copy(this.Buffer, result.Buffer, this.BufferSize);

                        List<Descriptor> descList = new List<Descriptor>();
                        IntPtr includeWalk = this.Descriptors;

                        while (includeWalk != IntPtr.Zero)
                        {
                            DescriptorNodeInternal walk = Marshal.PtrToStructure<DescriptorNodeInternal>(includeWalk);

                            descList.Add(walk.Descriptor.Data);

                            includeWalk = walk.Next;
                        }

                        result.Descriptors = descList.ToArray();

                        return result;
                    }

                    set
                    {
                        this.Version = 1;

                        Uuid = new byte[UUIDLENGTH + 1];
                        System.Text.Encoding.ASCII.GetBytes(value.Uuid).CopyTo(this.Uuid, 0);
                        this.InstanceId = value.InstanceId;
                        this.Permissions = value.Permissions;
                        this.Properties = value.Properties;
                        this.WriteType = value.WriteType;
                        this.Descriptors = IntPtr.Zero;
                        this.BufferSize = 0;

                        if (this.Buffer == null)
                        {
                            this.BufferSize = value.Buffer.Length;
                            this.Buffer = new byte[BufferLength];
                        }

                        if (value.Buffer != null)
                        {
                            Array.Copy(value.Buffer, this.Buffer, this.BufferSize);
                        }
                    }
                }
            }

            /// <summary>
            /// Internal descriptor
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct DescriptorInternal
            {
                /// <summary>
                /// version of this structure
                /// </summary>
                public uint Version;

                /// <summary>
                /// UUID of the descriptor
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = UUIDLENGTH + 1)]
                public byte[] Uuid;

                /// <summary>
                /// Instance ID
                /// </summary>
                public int InstanceId;

                /// <summary>
                /// Permissions of the descriptor
                /// </summary>
                public MLBluetoothLE.AttributePermissions Permissions;

                /// <summary>
                /// Value of the descriptor
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = BufferLength)]
                public byte[] Buffer;

                /// <summary>
                /// size of the value buffer
                /// </summary>
                public int BufferSize;

                /// <summary>
                /// Gets or sets the internal descriptor data from the public facing descriptor data
                /// </summary>
                public MLBluetoothLE.Descriptor Data
                {
                    get
                    {
                        MLBluetoothLE.Descriptor result = new MLBluetoothLE.Descriptor();

                        result.Uuid = System.Text.Encoding.ASCII.GetString(this.Uuid).TrimEnd('\0');
                        result.InstanceId = this.InstanceId;
                        result.Permissions = this.Permissions;
                        result.Buffer = new byte[this.BufferSize];
                        Array.Copy(this.Buffer, result.Buffer, this.BufferSize);

                        return result;
                    }

                    set
                    {
                        this.Version = 1;
                        Uuid = new byte[UUIDLENGTH + 1];
                        System.Text.Encoding.ASCII.GetBytes(value.Uuid).CopyTo(this.Uuid, 0);
                        this.InstanceId = value.InstanceId;
                        this.Permissions = value.Permissions;
                        this.BufferSize = value.Buffer.Length;

                        if (this.Buffer == null)
                        {
                            this.Buffer = new byte[BufferLength];
                        }

                        if (value.Buffer != null)
                        {
                            Array.Copy(value.Buffer, this.Buffer, this.BufferSize);
                        }
                    }
                }
            }

            /// <summary>
            /// A structure holding the scan result.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct ScanResult
            {
                /// <summary>
                /// The device returned during the scan
                /// </summary>
                public DeviceInternal Device;
            }

            /// <summary>
            /// A structure holding a Bluetooth address
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct BluetoothAddress
            {
                /// <summary>
                /// The address of the Bluetooth device
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = AddressLength + 1)]
                public byte[] Address;

                /// <summary>
                /// Sets the Bluetooth address from a string
                /// </summary>
                /// <param name="address">The Bluetooth address</param>
                public void SetAddress(string address)
                {
                    this.Address = new byte[AddressLength + 1];

                    System.Text.Encoding.ASCII.GetBytes(address).CopyTo(this.Address, 0);
                }

                /// <summary>
                /// Given a Bluetooth address from native code, convert it to a string
                /// </summary>
                /// <returns>Returns the string containing the address</returns>
                public override string ToString()
                {
                    if (this.Address.Length == 0)
                    {
                        return string.Empty;
                    }

                    return System.Text.Encoding.ASCII.GetString(this.Address).TrimEnd('\0');
                }
            }

            /// <summary>
            /// A structure holding a Bluetooth name
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct BluetoothName
            {
                /// <summary>
                /// The name of the Bluetooth device
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = NameLength + 1)]
                public byte[] Name;

                /// <summary>
                /// Set the name of the Bluetooth device from a string
                /// </summary>
                /// <param name="name">The name of the Bluetooth device</param>
                public void SetName(string name)
                {
                    this.Name = new byte[NameLength + 1];

                    System.Text.Encoding.ASCII.GetBytes(name).CopyTo(this.Name, 0);
                }

                /// <summary>
                /// Given a native buffer holding the device name, convert it to a string.
                /// </summary>
                /// <returns>Returns the string of the device name</returns>
                public override string ToString()
                {
                    if (this.Name.Length == 0)
                    {
                        return string.Empty;
                    }

                    return System.Text.Encoding.ASCII.GetString(this.Name).TrimEnd('\0');
                }
            }

            /// <summary>
            /// The native representation of the descriptor node
            /// </summary>
            private struct DescriptorNodeInternal
            {
                /// <summary>
                /// The internal descriptor referenced by this linked list
                /// </summary>
                public DescriptorInternal Descriptor;

#pragma warning disable 649
                /// <summary>
                /// Pointer to the next item in the linked list. This is assigned
                /// to through marshaling
                /// </summary>
                public IntPtr Next;
#pragma warning restore 649
            }

            /// <summary>
            /// Internal representation of the characteristic node (linked list)
            /// </summary>
            private struct CharacteristicNodeInternal
            {
                /// <summary>
                /// the internal characteristic referenced by this linked list
                /// </summary>
                public CharacteristicInternal Characteristic;

#pragma warning disable 649
                /// <summary>
                /// Pointer to the next item in the linked list. This is assigned to
                /// through marshaling
                /// </summary>
                public IntPtr Next;
#pragma warning restore 649
            }

            /// <summary>
            /// Internal representation of an included service
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            private struct IncludedServiceInternal
            {
                /// <summary>
                /// UUID for the service
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = UUIDLENGTH + 1)]
                public byte[] Uuid;

                /// <summary>
                /// Instance ID
                /// </summary>
                public int InstanceId;

                /// <summary>
                /// Service Type
                /// </summary>
                public int ServiceType;

                /// <summary>
                /// Gets the public facing included service
                /// </summary>
                public IncludedService Data
                {
                    get
                    {
                        IncludedService result = new IncludedService();

                        result.InstanceId = this.InstanceId;
                        result.ServiceType = this.ServiceType;
                        result.Uuid = System.Text.Encoding.ASCII.GetString(this.Uuid).TrimEnd('\0');

                        return result;
                    }
                }
            }

            /// <summary>
            /// Internal representation of the included service list (linked list)
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            private struct IncludedServiceNodeInternal
            {
                /// <summary>
                /// Service included in this linked list
                /// </summary>
                public IncludedServiceInternal IncludedService;

                /// <summary>
                /// The native pointer to the next included service in the list.
                /// </summary>
                public IntPtr Next;
            }

            /// <summary>
            /// The native representation of the <c>Gatt</c> Service
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            private struct GattServiceInternal
            {
                /// <summary>
                /// The instance ID
                /// </summary>
                public int InstanceId;

                /// <summary>
                /// The service type
                /// </summary>
                public int ServiceType;

                /// <summary>
                /// The UUID for the service.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = UUIDLENGTH + 1)]
                public byte[] Uuid;

                /// <summary>
                /// A native pointer to the included services
                /// </summary>
                public IntPtr IncludedServices;

                /// <summary>
                /// A native pointer to the characteristics
                /// </summary>
                public IntPtr Characteristics;

                /// <summary>
                /// Gets the public facing Service structure from the internal structure.
                /// </summary>
                public Service Data
                {
                    get
                    {
                        Service result = new Service();

                        result.InstanceId = this.InstanceId;
                        result.ServiceType = this.ServiceType;
                        result.Uuid = System.Text.Encoding.ASCII.GetString(this.Uuid).TrimEnd('\0');

                        List<IncludedService> includedServices = new List<IncludedService>();
                        List<Characteristic> chars = new List<Characteristic>();

                        IntPtr includeWalk = this.IncludedServices;

                        while (includeWalk != IntPtr.Zero)
                        {
                            IncludedServiceNodeInternal walk = Marshal.PtrToStructure<IncludedServiceNodeInternal>(includeWalk);

                            includedServices.Add(walk.IncludedService.Data);

                            includeWalk = walk.Next;
                        }

                        includeWalk = this.Characteristics;

                        while (includeWalk != IntPtr.Zero)
                        {
                            CharacteristicNodeInternal walk = Marshal.PtrToStructure<CharacteristicNodeInternal>(includeWalk);

                            chars.Add(walk.Characteristic.Data);

                            includeWalk = walk.Next;
                        }

                        result.IncludedServices = includedServices.ToArray();
                        result.Characteristics = chars.ToArray();

                        return result;
                    }
                }
            }

            /// <summary>
            /// A structure holding a Bluetooth name
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            private struct ServiceListInternal
            {
                /// <summary>
                /// version of this structure
                /// </summary>
                public uint Version;

                /// <summary>
                /// A native pointer to an array of <c>GattServiceInternal</c>
                /// </summary>
                public IntPtr Services;

                /// <summary>
                /// The number of elements in the services array (64 bits)
                /// </summary>
                public long Count;

                /// <summary>
                /// Create an internal representation of the service list
                /// </summary>
                /// <returns>Returns the newly created struct</returns>
                public static ServiceListInternal Create()
                {
                    ServiceListInternal result = new ServiceListInternal();

                    result.Version = 1;
                    result.Services = IntPtr.Zero;
                    result.Count = 0;

                    return result;
                }
            }
        }
#endif
    }
}
