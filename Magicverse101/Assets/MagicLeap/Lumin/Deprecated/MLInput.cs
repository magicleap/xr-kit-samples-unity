// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLInput.cs" company="Magic Leap, Inc">
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
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// Manages the input state for controllers, MCA and tablet devices.
    /// </summary>
    public partial class MLInput : MLAPISingleton<MLInput>
    {
        /// <summary>
        /// List of MLInput.TabletStates since last query up to a max of 20 MLInput.TabletStates
        /// as specified in the CAPI. Will be empty if device has just been connected and no
        /// updates have happened yet or if a Tablet Pen is not actively touching or hovering above the touchpad.
        /// </summary>
        /// <param name="tabletId">The id of the tablet.</param>
        /// <param name="queueOfStates">An array of TabletState structures.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.NotImplemented</c> this method has been deprecated.
        /// </returns>
        [Obsolete("Please use MLInput.GetTabletStates that uses the MLInput.TabletState array instead.", true)]
        public static MLResult GetTabletStates(byte tabletId, out List<MLInput.TabletState> queueOfStates)
        {
            queueOfStates = null;

            return new MLResult(MLResultCode.NotImplemented);
        }
    }

    /// <summary>
    /// A structure containing information about the state of the tablet device.
    /// </summary>
    [Obsolete("Please use MLInput.TabletState instead.", true)]
    public struct MLTabletState
    {
        /// <summary>
        /// Type of this tablet device.
        /// </summary>
        public MLInput.TabletDeviceType Type;

        /// <summary>
        /// Type of tool used with the tablet.
        /// </summary>
        public MLInput.TabletDeviceToolType ToolType;

        /// <summary>
        /// Current touch position (x,y) and force (z).
        /// Position is in the [-1.0,1.0] range and force is in the [0.0,1.0] range.
        /// </summary>
        public Vector3 PenTouchPosAndForce;

        /// <summary>
        /// Additional coordinate values (x, y, z)
        /// It could contain data specific to the device type.
        /// AdditionalPenTouchData for Wacom holds pen tilt data (x, y), in degrees from -64 to 64. Straight up an down is 0.
        /// </summary>
        public int[] AdditionalPenTouchData;

        /// <summary>
        /// Is touch active.
        /// </summary>
        public bool IsPenTouchActive;

        /// <summary>
        /// If this tablet is connected.
        /// </summary>
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
        /// Used to determine what data in this structure is valid.
        /// Example: Before using AdditionalPenTouchData check if that variable is valid in the ValidityCheck.
        /// </summary>
        public MLInput.TabletDeviceStateMask ValidityCheck;

        /// <summary>
        /// Override the ToString. Does not print the ValidityCheck.
        /// </summary>
        /// <returns>A string with the tablet information.</returns>
        public override string ToString()
        {
            string penTouch = string.Format("({0},{1},{2})", this.PenTouchPosAndForce.x, this.PenTouchPosAndForce.y, this.PenTouchPosAndForce.z);
            string penData = string.Format("({0},{1},{2})", this.AdditionalPenTouchData[0], this.AdditionalPenTouchData[1], this.AdditionalPenTouchData[2]);
            return string.Format("Tablet Device Type: {0} \nTool Type: {1} \nPen Position and Force: {2} \nAdditional Pen Touch Data: {3} \nIs Pen Touching: {4} \nIs Tablet Connected: {5} \nPen Distance: {6} \nTimeStamp: {7}", this.Type, this.ToolType, penTouch, penData, this.IsPenTouchActive, this.IsConnected, this.PenDistance, this.TimeStamp);
        }
    }

    /// <summary>
    /// The configuration class for MLInput.
    /// Updating settings requires a stop/start of the input system, which is done automatically.
    /// </summary>
    [Obsolete("Please use MLInput.Configuration instead.", true)]
    public class MLInputConfiguration
    {
        /// <summary>
        /// The default value for CFUID tracking if not specified.
        /// </summary>
        public const bool DEFAULT_CFUID_TRACKING_ENABLED = true;

        /// <summary>
        /// The default trigger reading threshold for emitting OnTriggerDown.
        /// </summary>
        public const float DEFAULT_TRIGGER_DOWN_THRESHOLD = 0.8f;

        /// <summary>
        /// The default trigger reading threshold for emitting OnTriggerUp.
        /// </summary>
        public const float DEFAULT_TRIGGER_UP_THRESHOLD = 0.2f;

        /// <summary>
        /// Controls if CFUID (Coordinate Frame Unique ID) based tracking should be initialized or not.
        /// </summary>
        public bool EnableCFUIDTracking = DEFAULT_CFUID_TRACKING_ENABLED;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLInputConfiguration"/> class.
        /// </summary>
        /// <param name="enableCFUIDTracking">A flag that indicates if 6DOF will be used.</param>
        /// <param name="triggerDownThreshold">The threshold before the trigger down event occurs.</param>
        /// <param name="triggerUpThreshold">The threshold before the trigger up event occurs.</param>
        public MLInputConfiguration(
            bool enableCFUIDTracking,
            float triggerDownThreshold = DEFAULT_TRIGGER_DOWN_THRESHOLD,
            float triggerUpThreshold = DEFAULT_TRIGGER_UP_THRESHOLD)
        {
            this.Dof = new MLInput.Controller.ControlDof[MLInputNativeBindings.MaxControllers];

            this.EnableCFUIDTracking = enableCFUIDTracking;
            this.TriggerDownThreshold = triggerDownThreshold;
            this.TriggerUpThreshold = triggerUpThreshold;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MLInputConfiguration"/> class.
        /// </summary>
        /// <param name="triggerDownThreshold">The threshold before the trigger down event occurs.</param>
        /// <param name="triggerUpThreshold">The threshold before the trigger up event occurs.</param>
        /// <param name="enableCFUIDTracking">A flag that indicates if 6DOF will be used.</param>
        [Obsolete("Please use MLInput.Configuration(enableCFUIDTracking, triggerDownThreshold, triggerUpThreshold) instead.", true)]
        public MLInputConfiguration(
            float triggerDownThreshold = DEFAULT_TRIGGER_DOWN_THRESHOLD,
            float triggerUpThreshold = DEFAULT_TRIGGER_UP_THRESHOLD,
            bool enableCFUIDTracking = DEFAULT_CFUID_TRACKING_ENABLED)
        {
            this.Dof = new MLInput.Controller.ControlDof[MLInputNativeBindings.MaxControllers];

            this.TriggerDownThreshold = triggerDownThreshold;
            this.TriggerUpThreshold = triggerUpThreshold;
            this.EnableCFUIDTracking = enableCFUIDTracking;
        }

        /// <summary>
        /// Gets the degrees-of-freedom mode for the control.
        /// </summary>
        public MLInput.Controller.ControlDof[] Dof { get; private set; }

        /// <summary>
        /// Gets or sets the trigger reading threshold for emitting OnTriggerUp.
        /// </summary>
        public float TriggerDownThreshold { get; set; }

        /// <summary>
        /// Gets or sets the trigger reading threshold for emitting OnTriggerUp.
        /// </summary>
        public float TriggerUpThreshold { get; set; }

        /// <summary>
        /// Casts data to the native configuration structure.
        /// </summary>
        /// <param name="self">The configuration to cast.</param>
        [Obsolete("Please use MLInputNativeBindings.MLInputConfigurationNative.Data to cast betwen this structs instead.")]
        public static explicit operator MLInputNativeBindings.MLInputConfigurationNative(MLInputConfiguration self)
        {
            MLInputNativeBindings.MLInputConfigurationNative config;

            config.Dof = self.Dof;

            return config;
        }
    }
}
#endif
