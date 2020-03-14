// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLBluetoothLE.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Magic Leap Bluetooth Low Energy implementation for Unity
    /// </summary>
    public sealed partial class MLBluetoothLE : MLAPISingleton<MLBluetoothLE>
    {
#if PLATFORM_LUMIN
        /// <summary>
        /// Max Bluetooth buffer size, as defined by Android OS
        /// </summary>
        private const int MaxBufferSize = 600;

        /// <summary>
        /// Prevents a default instance of the <see cref="MLBluetoothLE"/> class from being created. Use MLBluetooth.Start()
        /// </summary>
        private MLBluetoothLE()
        {
        }

        /// <summary>
        /// Delegate describing the callback necessary to scan for devices
        /// </summary>
        /// <param name="device">Information about the device scanned</param>
        public delegate void BLEScanResultDelegate(Device device);

        /// <summary>
        /// Delegate describing the callback necessary monitor adapter state changes
        /// </summary>
        /// <param name="newState">The new adapter state</param>
        public delegate void BLEAdapterStateDelegate(AdapterState newState);

        /// <summary>
        /// Delegate describing the callback necessary to monitor <c>acl</c> state changes
        /// </summary>
        /// <param name="device">Information about the device being updated</param>
        /// <param name="newState">The new state of the device</param>
        public delegate void BLEAclStateDelegate(Device device, AclState newState);

        /// <summary>
        /// Delegate describing the callback necessary to monitor bond state changes
        /// </summary>
        /// <param name="device">Information about the device being changed</param>
        /// <param name="newState">New bond state for the device</param>
        public delegate void BLEBondStateDelegate(Device device, BondState newState);

        /// <summary>
        /// Delegate that describes the callback for connection state changes
        /// </summary>
        /// <param name="status">New status</param>
        /// <param name="state">New connection state</param>
        public delegate void BLEConnectionStateChangedDelegate(Status status, GattConnectionState state);

        /// <summary>
        /// Delegate that describes the callback for new services
        /// </summary>
        /// <param name="status">Status values of GATT operation</param>
        public delegate void BLEServicesDiscoveredDelegate(Status status);

        /// <summary>
        /// Delegate that describes the callback invoked when the RSSI value of remote has been read
        /// </summary>
        /// <param name="rssi">Indicates the current received signal strength indicator</param>
        /// <param name="status">Status values of GATT operation</param>
        public delegate void BLEReadRemoteRssiDelegate(int rssi, Status status);

        /// <summary>
        /// Delegate that describes the callback invoked when the MTU value has changed
        /// </summary>
        /// <param name="mtu">Indicates the current MTU size</param>
        /// <param name="status">Status values of GATT operation</param>
        public delegate void BLEMTUSizeChanged(int mtu, Status status);

        /// <summary>
        /// Delegate that describes the callback  invoked when the remote characteristic has been read
        /// </summary>
        /// <param name="characteristic">The characteristic that was read</param>
        /// <param name="status">Status values of GATT operation</param>
        public delegate void BLECharacteristicReadDelegate(Characteristic characteristic, Status status);

        /// <summary>
        /// Delegate that describes the callback  invoked when the characteristic has been written to the remote device
        /// </summary>
        /// <param name="characteristic">The characteristic written</param>
        /// <param name="status">Status values of GATT operation</param>
        public delegate void BLECharacteristicWriteDelegate(Characteristic characteristic, Status status);

        /// <summary>
        /// Delegate that describes the callback invoked when the remote descriptor has been read 
        /// </summary>
        /// <param name="descriptor">Descriptor being read</param>
        /// <param name="status">Status values of GATT operation</param>
        public delegate void BLEGattDescriptorReadDelegate(Descriptor descriptor, Status status);

        /// <summary>
        /// Delegate that describes the callback invoked when the descriptor has been written to the remote device
        /// </summary>
        /// <param name="descriptor">The descriptor written</param>
        /// <param name="status">Status values of GATT operation</param>
        public delegate void BLEGattDescriptorWriteDelegate(Descriptor descriptor, Status status);

        /// <summary>
        /// Delegate that describes the callback invoked when a characteristic changes
        /// </summary>
        /// <param name="characteristic">Remote characteristic changed</param>
        public delegate void BLEGattRemoteCharacteristicChangedDelegate(Characteristic characteristic);

        /// <summary>
        /// Delegate that describes the callback invoked when the connection interval changes
        /// </summary>
        /// <param name="interval">the new interval</param>
        public delegate void BLEGattConnectionIntervalUpdatedDelegate(int interval);

        /// <summary>
        /// Gets or sets the user define callback invoked when scanning for devices.
        /// </summary>
        public static event BLEScanResultDelegate OnScanResult;

        /// <summary>
        /// Gets or sets the user define callback invoked when <c>acl</c> states change
        /// </summary>
        public static event BLEAclStateDelegate OnAclStateChanged;

        /// <summary>
        /// Gets or sets the user define callback invoked when bond states change
        /// </summary>
        public static event BLEBondStateDelegate OnBondStateChanged;

        /// <summary>
        /// Gets or sets the user define callback invoked when adapter states change
        /// </summary>
        public static event BLEAdapterStateDelegate OnAdapterStateChanged;

        /// <summary>
        /// Gets or sets the callback invoked when the connection state changes
        /// </summary>
        public static event BLEConnectionStateChangedDelegate OnBluetoothConnectionStateChanged = delegate { };

        /// <summary>
        /// Gets or sets the callback invoked when new services are discovered
        /// </summary>
        public static event BLEServicesDiscoveredDelegate OnBluetoothServicesDiscovered = delegate { };

        /// <summary>
        /// Gets or sets the callback invoked when there is new RSSI information
        /// </summary>
        public static event BLEReadRemoteRssiDelegate OnBluetoothReadRemoteRssi = delegate { };

        /// <summary>
        /// Gets or sets the callback invoked when there is new RSSI information
        /// </summary>
        public static event BLEMTUSizeChanged OnBluetoothMTUSizeChanged = delegate { };

        /// <summary>
        /// Gets or sets the callback invoked when a characteristic has been read
        /// </summary>
        public static event BLECharacteristicReadDelegate OnBluetoothCharacteristicRead = delegate { };

        /// <summary>
        /// Gets or sets the callback invoked after a characteristic has been written
        /// </summary>
        public static event BLECharacteristicWriteDelegate OnBluetoothCharacteristicWrite = delegate { };

        /// <summary>
        /// Gets or sets the callback invoked when the <c>Gatt</c> descriptor is updated.
        /// </summary>
        public static event BLEGattDescriptorReadDelegate OnBluetoothGattDescriptorRead = delegate { };

        /// <summary>
        /// Gets or sets the callback invoked when the <c>Gatt</c> descriptor has been written
        /// </summary>
        public static event BLEGattDescriptorWriteDelegate OnBluetoothGattDescriptorWrite = delegate { };

        /// <summary>
        /// Gets or sets the callback invoked when the remote device characteristics change
        /// </summary>
        public static event BLEGattRemoteCharacteristicChangedDelegate OnBluetoothGattRemoteCharacteristicChanged = delegate { };

        /// <summary>
        /// Gets or sets the callback invoked when the <c>Gatt</c> connection interval changes.
        /// </summary>
        public static event BLEGattConnectionIntervalUpdatedDelegate OnBluetoothGattConnectionIntervalUpdated = delegate { };

        /// <summary>
        /// Bluetooth adapter states
        /// </summary>
        public enum AdapterState : int
        {
            /// <summary>
            /// The adapter is off
            /// </summary>
            Off,

            /// <summary>
            /// The adapter is on
            /// </summary>
            On
        }

        /// <summary>
        /// Bond States
        /// </summary>
        public enum BondState : int
        {
            /// <summary>
            /// Not Bonded
            /// </summary>
            None = 0,

            /// <summary>
            /// Bonding has started but not complete
            /// </summary>
            Bonding,

            /// <summary>
            /// Bonding successful
            /// </summary>
            Bonded
        }

        /// <summary>
        /// Bond types
        /// </summary>
        public enum BondType : int
        {
            /// <summary>
            /// User must provide a PIN
            /// </summary>
            ClassicPin = 0,

            /// <summary>
            /// Secure Simple Pairing, it just works. No user interaction required
            /// </summary>
            SspJustWorks,

            /// <summary>
            /// Secure Simple Pairing after accepting numerical comparison prompt
            /// </summary>
            SspNumericalComparison,

            /// <summary>
            /// Secure Simple Pairing after entering a key
            /// </summary>
            SspPasskeyEntry,

            /// <summary>
            /// Secure Simple Pairing after accepting yes/no.
            /// </summary>
            SspPasskeyNotification,
        }

        /// <summary>
        /// Asynchronous connection-less state
        /// </summary>
        public enum AclState : int
        {
            /// <summary>
            /// An <c>Acl</c> connection was successful
            /// </summary>
            Connected = 0,

            /// <summary>
            /// Not connected
            /// </summary>
            Disconnected
        }

        /// <summary>
        /// Type of Bluetooth device
        /// </summary>
        public enum DeviceType : uint
        {
            /// <summary>
            /// Unknown. Only Low Energy devices are supported.
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Confirmed it is a LE device.
            /// </summary>
            LowEnergy = 2
        }

        /// <summary>
        /// Attribute permission for a <c>Gatt</c> client
        /// </summary>
        [Flags]
        public enum AttributePermissions
        {
            /// <summary>
            /// No permissions
            /// </summary>
            None = 0,

            /// <summary>
            /// Read permission
            /// </summary>
            Read = (1 << 0),

            /// <summary>
            /// Read encrypted permission
            /// </summary>
            ReadEncrypted = (1 << 1),

            /// <summary>
            /// Read encryption with Man-In-The-Middle protection
            /// </summary>
            ReadEncryptedMITM = (1 << 2),

            /// <summary>
            /// Write permission
            /// </summary>
            Write = (1 << 4),

            /// <summary>
            /// Write encrypted permission
            /// </summary>
            WriteEncrypted = (1 << 5),

            /// <summary>
            /// Write encryption with Man-In-The-Middle protection
            /// </summary>
            WriteEncryptedMITM = (1 << 6),

            /// <summary>
            /// Write signed data
            /// </summary>
            WriteSigned = (1 << 7),

            /// <summary>
            /// Write signed data with Man-In-The-Middle protection
            /// </summary>
            WriteSignedMITM = (1 << 8)
        }

        /// <summary>
        /// Characteristic Properties for the <c>Gatt</c>
        /// </summary>
        [Flags]
        public enum CharacteristicProperties
        {
            /// <summary>
            /// No properties
            /// </summary>
            None = 0,

            /// <summary>
            /// Allows this characteristic value to be placed in advertising packets, using the Service Data AD Type
            /// </summary>
            Broadcast = (1 << 0),

            /// <summary>
            /// Allows clients to read this characteristic using any of the ATT read operations
            /// </summary>
            Read = (1 << 1),

            /// <summary>
            /// Allows clients to use the Write Command ATT operation on this characteristic
            /// </summary>
            WriteNoRes = (1 << 2),

            /// <summary>
            /// Allows clients to use the Write Request/Response ATT operation on this characteristic
            /// </summary>
            Write = (1 << 3),

            /// <summary>
            /// Allows the server to use the Handle Value Notification ATT operation on this characteristic
            /// </summary>
            Notify = (1 << 4),

            /// <summary>
            /// Allows the server to use the Handle Value Indication/Confirmation ATT operation on this characteristic 
            /// </summary>
            Indicate = (1 << 5),

            /// <summary>
            /// Allows clients to use the Signed Write Command ATT operation on this characteristic
            /// </summary>
            SignedWrite = (1 << 6),

            /// <summary>
            /// Extended properties
            /// </summary>
            ExtProps = (1 << 7)
        }

        /// <summary>
        /// Definition of write types
        /// </summary>
        [Flags]
        public enum WriteType
        {
            /// <summary>
            /// Write characteristic without requiring a response by the remote device
            /// </summary>
            NoResponse = (1 << 0),

            /// <summary>
            /// Write characteristic, requesting acknowledgement by the remote device
            /// </summary>
            Default = (1 << 1),

            /// <summary>
            /// Write characteristic including authentication signature
            /// </summary>
            Signed = (1 << 2)
        }

        /// <summary>
        /// Status values of GATT operation
        /// </summary>
        [Flags]
        public enum Status : int
        {
            /// <summary>
            /// An operation is completed successfully.
            /// </summary>
            Success = 0,

            /// <summary>
            /// GATT read operation is not permitted.
            /// </summary>
            Read_Not_Permitted = 0x02,

            /// <summary>
            /// GATT write operation is not permitted.
            /// </summary>
            Write_Not_Permitted = 0x03,

            /// <summary>
            /// Insufficient authentication for a given operation.
            /// </summary>
            Insufficient_Authentication = 0x05,

            /// <summary>
            /// The given request is not supported.
            /// </summary>
            Request_Not_Supported = 0x06,

            /// <summary>
            /// A read or write operation was requested with an invalid offset.
            /// </summary>
            Invalid_Offset = 0x07,

            /// <summary>
            /// A write operation exceeds the maximum length of the attribute.
            /// </summary>
            Invalid_Attribute_Length = 0x0d,

            /// <summary>
            /// Insufficient encryption for a given operation.
            /// </summary>
            Insufficient_Encryption = 0x0f,

            /// <summary>
            /// A remote device connection is congested.
            /// </summary>
            Connection_Congested = 0x8f,

            /// <summary>
            /// Generic error.
            /// </summary>
            Error = 0x85,

            /// <summary>
            /// An operation failed.
            /// </summary>
            Failure = 0x101
        }

        /// <summary>
        /// GATT Connection states
        /// </summary>
        [Flags]
        public enum GattConnectionState : int
        {
            /// <summary>
            /// Disconnected from the device
            /// </summary>
            Disconnected = 0,

            /// <summary>
            /// Connected to the device
            /// </summary>
            Connected
        }

        /// <summary>
        /// <c>Gatt</c> Connection Priority
        /// </summary>
        public enum ConnectionPriority : int
        {
            /// <summary>
            /// Use the connection parameters recommended by the Bluetooth SIG
            /// </summary>
            Balanced = 0,

            /// <summary>
            /// Request a high priority, low latency connection
            /// </summary>
            High,

            /// <summary>
            /// Request low power, reduced data rate connection parameters
            /// </summary>
            Low_Power,
        }

        /// <summary>
        /// Start up the instance of the BluetoothLE API
        /// </summary>
        /// <returns>
        /// MLResult.Code will be MLResultCode.Ok if Bluetooth started up successfully, or an error if it was not successful.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLBluetoothLE.BaseStart();
        }

        /// <summary>
        /// Get the name of the adapter
        /// </summary>
        /// <param name="name">A string representation of the adapter name</param>
        /// <returns>MLResultCode.Ok on success, or failure</returns>
        public static MLResult GetAdapterName(out string name)
        {
            return NativeBindings.GetAdapterName(out name);
        }

        /// <summary>
        /// Get the current adapter state
        /// </summary>
        /// <param name="state">Current Adapter State of the device</param>
        /// <returns>MLResultCode.Ok on success, or failure</returns>
        public static MLResult GetAdapterState(out AdapterState state)
        {
            // Provide a default value
            state = AdapterState.Off;

            return MLResult.Create(NativeBindings.MLBluetoothAdapterGetState(ref state));
        }

        /// <summary>
        /// Create a bond with the bluetooth device at the address.
        /// </summary>
        /// <param name="address">The address to create a bond with.</param>
        /// <returns>MLResultCode.Ok on success, or failure</returns>
        public static MLResult CreateBond(string address)
        {
            NativeBindings.BluetoothAddress bleAddr = new NativeBindings.BluetoothAddress();

            bleAddr.SetAddress(address);

            return MLResult.Create(NativeBindings.MLBluetoothAdapterCreateBond(ref bleAddr));
        }

        /// <summary>
        /// Begin scanning for Bluetooth LE devices
        /// </summary>
        /// <returns>MLResultCode.Ok on success, or failure</returns>
        public static MLResult StartScan()
        {
            return MLResult.Create(NativeBindings.MLBluetoothLeStartScan());
        }

        /// <summary>
        /// Stop scanning for Bluetooth LE devices
        /// </summary>
        /// <returns>MLResultCode.Ok on success, or failure</returns>
        public static MLResult StopScan()
        {
            return MLResult.Create(NativeBindings.MLBluetoothLeStopScan());
        }

        /// <summary>
        /// Create a Generic Attribute Profile client to exchange profile and user data with a specific device.
        /// </summary>
        /// <param name="bluetoothAddress">Address of the Bluetooth device to connect to</param>
        /// <returns>Returns the <c>Gatt</c> Client</returns>
        public static MLResult GattConnect(string bluetoothAddress)
        {
            if (!MLBluetoothLE.IsValidInstance())
            {
                return MLResult.Create(MLResult.Code.UnspecifiedFailure);
            }

            if (!NativeBindings.GattConnect(bluetoothAddress).IsOk)
            {
                // Failed to connect. Exit.
                return MLResult.Create(MLResult.Code.UnspecifiedFailure);
            }

            return MLResult.Create(MLResult.Code.Ok);
        }

        /// <summary>
        /// Disconnect the <c>Gatt</c> device.
        /// </summary>
        /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
        public static MLResult GattDisconnect()
        {
            return MLResult.Create(NativeBindings.MLBluetoothGattDisconnect());
        }

        /// <summary>
        /// Discovers GATT services offered by a remote device.
        /// </summary>
        /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
        public static MLResult DiscoverServices()
        {
            return MLResult.Create(NativeBindings.MLBluetoothGattDiscoverServices());
        }

        /// <summary>
        /// Reads the RSSI for a connected remote device. The <c>on_gatt_read_remote_rssi</c> callback will 
        /// be invoked when the RSSI value has been read.
        /// </summary>
        /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
        public static MLResult ReadRemoteRssi()
        {
            return MLResult.Create(NativeBindings.MLBluetoothGattReadRemoteRssi());
        }

        /// <summary>
        /// Gets a list of GATT services offered by the remote devices
        /// </summary>
        /// <param name="serviceList">Returns the list of services</param>
        /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
        public static MLResult GetServiceRecord(out Service[] serviceList)
        {
            return NativeBindings.GetServiceRecord(out serviceList);
        }

        /// <summary>
        /// Reads the requested characteristic from the connected remote device
        /// </summary>
        /// <param name="characteristic">The characteristic to read from the remote device</param>
        /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
        public static MLResult ReadCharacteristic(ref Characteristic characteristic)
        {
            return MLResult.Create(NativeBindings.ReadCharacteristic(ref characteristic));
        }

        /// <summary>
        /// Writes a given characteristic and its value to the connected remote device
        /// </summary>
        /// <param name="characteristic">The characteristic to write on the remote device</param>
        /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
        public static MLResult WriteCharacteristic(Characteristic characteristic)
        {
            return MLResult.Create(NativeBindings.WriteCharacteristic(characteristic));
        }

        /// <summary>
        /// Reads the requested descriptor from the connected remote device
        /// </summary>
        /// <param name="descriptor">The descriptor to read from the remote device</param>
        /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
        public static MLResult ReadDescriptor(ref Descriptor descriptor)
        {
            return MLResult.Create(NativeBindings.ReadDescriptor(ref descriptor));
        }

        /// <summary>
        /// Writes the value of a given descriptor to the connected device
        /// </summary>
        /// <param name="descriptor">The descriptor to write to the remote device</param>
        /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
        public static MLResult WriteDescriptor(Descriptor descriptor)
        {
            return MLResult.Create(NativeBindings.WriteDescriptor(descriptor));
        }

        /// <summary>
        /// Enables or disables notifications/indications for a given characteristic
        /// </summary>
        /// <param name="characteristic">Characteristic to be notified of</param>
        /// <param name="enabled">Enabled state</param>
        /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
        public static MLResult EnableCharacteristicNotification(Characteristic characteristic, bool enabled)
        {
            return MLResult.Create(NativeBindings.EnableCharacteristicNotification(characteristic, enabled));
        }

        /// <summary>
        /// Requests a connection parameter update.
        /// </summary>
        /// <param name="priority">A specific connection priority</param>
        /// <returns>Returns <c>MLResult.Code.Ok</c> on success or an error on failure.</returns>
        public MLResult RequestConnectionPriority(ConnectionPriority priority)
        {
            return MLResult.Create(NativeBindings.MLBluetoothGattRequestConnectionPriority(priority));
        }

        /// <summary>
        /// Receives a time slice for monitoring, not used.
        /// </summary>
        protected override void Update()
        {
        }

#if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Start Contacts API and set up callbacks.
        /// </summary>
        /// <returns>
        /// Returns MLResult.Ok if the bluetooth code was started up ok.
        /// </returns>
        protected override MLResult StartAPI()
        {
            Debug.Log("Bluetooth::StartAPI()");
            return NativeBindings.StartupBluetooth();
        }
#endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Called by MLAPISingleton on destruction
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Indicates whether or not is should be considered safe to access managed objects</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
        }

        /// <summary>
        /// static instance of the MLBluetoothLE class
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLBluetoothLE.IsValidInstance())
            {
                MLBluetoothLE._instance = new MLBluetoothLE();
            }
        }

        /// <summary>
        /// A Device structure holds characteristics of the device.
        /// </summary>
        public struct Device
        {
            /// <summary>
            /// The address of the device, used to connect
            /// </summary>
            public string Address;

            /// <summary>
            /// A human readable device name
            /// </summary>
            public string Name;

            /// <summary>
            /// Received Signal Strength Indicator
            /// </summary>
            public byte Rssi;

            /// <summary>
            /// Type of Bluetooth device, Low Energy or not.
            /// </summary>
            public DeviceType DeviceType;
        }

        /// <summary>
        /// A structure containing the contents of a GATT descriptor.
        /// </summary>
        public struct Descriptor
        {
            /// <summary>
            /// UUID for the descriptor
            /// </summary>
            public string Uuid;

            /// <summary>
            /// Instance ID
            /// </summary>
            public int InstanceId;

            /// <summary>
            /// Permissions for the descriptor
            /// </summary>
            public AttributePermissions Permissions;

            /// <summary>
            /// Value for the descriptor
            /// </summary>
            public byte[] Buffer;

            /// <summary>
            /// Creates a new descriptor and initializes its contents
            /// </summary>
            /// <returns>Returns the newly created descriptor</returns>
            public Descriptor Create()
            {
                Descriptor desc = new Descriptor();

                desc.Uuid = string.Empty;
                this.InstanceId = 0;
                this.Permissions = AttributePermissions.None;
                this.Buffer = new byte[MaxBufferSize];

                return desc;
            }
        }

        /// <summary>
        /// A structure for a node in the list of GATT characteristics
        /// </summary>
        public struct Characteristic
        {
            /// <summary>
            /// UUID for the characteristic
            /// </summary>
            public string Uuid;

            /// <summary>
            /// Instance ID
            /// </summary>
            public int InstanceId;

            /// <summary>
            /// Permissions for the characteristics
            /// </summary>
            public AttributePermissions Permissions;

            /// <summary>
            /// Properties of the characteristic
            /// </summary>
            public CharacteristicProperties Properties;

            /// <summary>
            /// The writing type
            /// </summary>
            public WriteType WriteType;

            /// <summary>
            /// Value of the characteristic
            /// </summary>
            public byte[] Buffer;

            /// <summary>
            /// Descriptors for the characteristic
            /// </summary>
            public Descriptor[] Descriptors;

            /// <summary>
            /// Create a characteristic and initialize its contents
            /// </summary>
            /// <returns>Returns the newly created Characteristic</returns>
            public static Characteristic Create()
            {
                Characteristic character = new Characteristic();

                character.Uuid = string.Empty;
                character.InstanceId = 0;
                character.Permissions = AttributePermissions.None;
                character.Properties = CharacteristicProperties.None;
                character.Buffer = new byte[MaxBufferSize];

                return character;
            }
        }

        /// <summary>
        /// A structure containing the contents of a GATT include
        /// </summary>
        public struct IncludedService
        {
            /// <summary>
            /// UUID for the service
            /// </summary>
            public string Uuid;

            /// <summary>
            /// Instance ID
            /// </summary>
            public int InstanceId;

            /// <summary>
            /// Service type
            /// </summary>
            public int ServiceType;
        }

        /// <summary>
        /// A structure containing the contents of a GATT service
        /// </summary>
        public struct Service
        {
            /// <summary>
            /// Instance ID
            /// </summary>
            public int InstanceId;

            /// <summary>
            /// The service type
            /// </summary>
            public int ServiceType;

            /// <summary>
            /// UUID for the service
            /// </summary>
            public string Uuid;

            /// <summary>
            /// Included services
            /// </summary>
            public IncludedService[] IncludedServices;

            /// <summary>
            /// Included characteristics
            /// </summary>
            public Characteristic[] Characteristics;
        }
#endif
    }
}
