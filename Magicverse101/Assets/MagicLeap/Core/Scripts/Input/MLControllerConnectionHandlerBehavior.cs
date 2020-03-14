// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://auth.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// Class to automatically handle connection/disconnection events of an input device. By default,
    /// all device types are allowed but it could be modified through the inspector to limit which types to
    /// allow. This class automatically handles the disconnection/reconnection of Control and MLA devices.
    /// This class keeps track of all connected devices matching allowed types. If more than one allowed
    /// device is connected, the first one connected is returned.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/Input/MLControllerConnectionHandlerBehavior")]
    public sealed class MLControllerConnectionHandlerBehavior : MonoBehaviour
    {
        /// <summary>
        /// Flags to determine which input devices to allow
        /// </summary>
        [Flags]
        public enum DeviceTypesAllowed : int
        {
            MobileApp = 1 << 0,
            ControllerLeft = 1 << 1,
            ControllerRight = 1 << 2,
        }

        [SerializeField, MLBitMask(typeof(DeviceTypesAllowed)), Tooltip("Bitmask on which devices to allow.")]
        private DeviceTypesAllowed _devicesAllowed = (DeviceTypesAllowed)~0;

        private List<MLInput.Controller> _allowedConnectedDevices = new List<MLInput.Controller>();

        /// <summary>
        /// Getter for the first allowed connected device, could return null.
        /// </summary>
        public MLInput.Controller ConnectedController
        {
            get
            {
                return (_allowedConnectedDevices.Count == 0) ? null : _allowedConnectedDevices[0];
            }
        }

        /// <summary>
        /// Getter for devices allowed bitmask
        /// </summary>
        public DeviceTypesAllowed DevicesAllowed
        {
            get
            {
                return _devicesAllowed;
            }
        }

        /// <summary>
        /// Invoked when a valid controller has connected.
        /// </summary>
        public event Action<byte> OnControllerConnected = delegate { };

        /// <summary>
        /// Invoked when an invalid controller has disconnected.
        /// </summary>
        public event Action<byte> OnControllerDisconnected = delegate { };

        /// <summary>
        /// Starts the MLInput, initializes the first controller, and registers the connection handlers
        /// </summary>
        void Start()
        {
            if (_devicesAllowed == 0)
            {
                Debug.LogErrorFormat("Error: ControllerConnectionHandler._devicesAllowed is invalid, disabling script.");
                enabled = false;
                return;

            }
            bool requestCFUID = DevicesAllowed.HasFlag(DeviceTypesAllowed.ControllerLeft) ||
                DevicesAllowed.HasFlag(DeviceTypesAllowed.ControllerRight);

            #if PLATFORM_LUMIN
            if (!MLInput.IsStarted)
            {
                MLInput.Configuration config = new MLInput.Configuration(requestCFUID,
                                                            MLInput.Configuration.DefaultTriggerDownThreshold,
                                                            MLInput.Configuration. DefaultTriggerUpThreshold);
                MLResult result = MLInput.Start(config);
                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("Error: ControllerConnectionHandler failed starting MLInput, disabling script. Reason: {0}", result);
                    enabled = false;
                    return;
                }
            }

            MLInput.OnControllerConnected += HandleOnControllerConnected;
            MLInput.OnControllerDisconnected += HandleOnControllerDisconnected;
            #endif

            GetAllowedInput();
        }

        /// <summary>
        /// Unregisters the connection handlers and stops the MLInput
        /// </summary>
        void OnDestroy()
        {
            #if PLATFORM_LUMIN
            if (MLInput.IsStarted)
            {
                MLInput.OnControllerDisconnected -= HandleOnControllerDisconnected;
                MLInput.OnControllerConnected -= HandleOnControllerConnected;

                MLInput.Stop();
            }
            #endif
        }

        /// <summary>
        /// Fills allowed connected devices list with all the connected controllers matching
        /// types set in _devicesAllowed.
        /// </summary>
        private void GetAllowedInput()
        {
            _allowedConnectedDevices.Clear();

            #if PLATFORM_LUMIN
            for (int i = 0; i < 2; ++i)
            {
                MLInput.Controller controller = MLInput.GetController(i);
                if (IsDeviceAllowed(controller) && !_allowedConnectedDevices.Exists((device) => device.Id == controller.Id))
                {
                    _allowedConnectedDevices.Add(controller);
                }
            }
            #endif
        }

        /// <summary>
        /// Checks if a controller exists, is connected, and is allowed.
        /// </summary>
        /// <param name="controller">The controller to be checked for</param>
        /// <returns>True if the controller exists, is connected, and is allowed</returns>
        private bool IsDeviceAllowed(MLInput.Controller controller)
        {
            #if PLATFORM_LUMIN
            if (controller == null || !controller.Connected)
            {
                return false;
            }

            return (((_devicesAllowed & DeviceTypesAllowed.MobileApp) != 0 && controller.Type == MLInput.Controller.ControlType.MobileApp) ||
                ((_devicesAllowed & DeviceTypesAllowed.ControllerLeft) != 0 && controller.Type == MLInput.Controller.ControlType.Control && controller.Hand == MLInput.Hand.Left) ||
                ((_devicesAllowed & DeviceTypesAllowed.ControllerRight) != 0 && controller.Type == MLInput.Controller.ControlType.Control && controller.Hand == MLInput.Hand.Right));
            #else
            return false;
            #endif
        }

        /// <summary>
        /// Checks if there is a controller in the list. This method
        /// does not check if the controller is of the allowed device type
        /// since that's handled by the connection/disconnection handlers.
        /// Should not be called from Awake() or OnEnable().
        /// </summary>
        /// <returns>True if the controller is ready for use, false otherwise</returns>
        public bool IsControllerValid()
        {
            return (ConnectedController != null);
        }

        /// <summary>
        /// Checks if controller list contains controller with input id.
        /// This method does not check if the controller is of the allowed device
        /// type since that's handled by the connection/disconnection handlers.
        /// Should not be called from Awake() or OnEnable().
        /// </summary>
        /// <param name="controllerId"> Controller id to check against </param>
        /// <returns>True if a controller is found, false otherwise</returns>
        public bool IsControllerValid(byte controllerId)
        {
            #if PLATFORM_LUMIN
            return _allowedConnectedDevices.Exists((device) => device.Id == controllerId);
            #else
            return false;
            #endif
        }

        /// <summary>
        /// Handles the event when a controller connects. If the connected controller
        /// is valid, we add it to the _allowedConnectedDevices list.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        private void HandleOnControllerConnected(byte controllerId)
        {
            #if PLATFORM_LUMIN
            MLInput.Controller newController = MLInput.GetController(controllerId);
            if (IsDeviceAllowed(newController))
            {
                if(_allowedConnectedDevices.Exists((device) => device.Id == controllerId))
                {
                    Debug.LogWarning(string.Format("Connected controller with id {0} already connected.", controllerId));
                    return;
                }

                _allowedConnectedDevices.Add(newController);

                // Notify Listeners
                if (OnControllerConnected != null)
                {
                    OnControllerConnected.Invoke(controllerId);
                }
            }
            #endif
        }

        /// <summary>
        /// Handles the event when a controller disconnects. If the disconnected
        /// controller happens to be in the _allowedConnectedDevices list, we
        /// remove it from the list.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        private void HandleOnControllerDisconnected(byte controllerId)
        {
            #if PLATFORM_LUMIN
            // Remove from the list of allowed devices.
            int devicesRemoved = _allowedConnectedDevices.RemoveAll((device) => device.Id == controllerId);

            // Notify Listeners of the disconnected device.
            if (devicesRemoved > 0)
            {
                if (OnControllerDisconnected != null)
                {
                    OnControllerDisconnected.Invoke(controllerId);
                }
            }
            #endif
        }
    }
}
