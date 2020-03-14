// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHandTrackingKeyPoseManager.cs" company="Magic Leap, Inc">
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
    using UnityEngine.XR.InteractionSubsystems;

    /// <summary>
    /// MLHandTracking is the entry point for all the hand tracking data
    /// including gestures, hand centers and key points for both hands.
    /// </summary>
    public partial class MLHandTracking
    {
        /// <summary>
        /// Manages what key poses are enabled and exposes the events.
        /// </summary>
        public partial class KeyposeManager
        {
            /// <summary>
            /// The array of hands to account for (just two).
            /// </summary>
            private Hand[] hands = new Hand[2];

            /// <summary>
            /// The native configuration indicating which key poses to watch for and the fidelity of tracking once for key poses and key points.
            /// </summary>
            private NativeBindings.ConfigurationNative config = new NativeBindings.ConfigurationNative();

            /// <summary>
            /// Initializes a new instance of the <see cref="KeyposeManager" /> class.
            /// </summary>
            /// <param name="leftHand">Left hand to which KeyPoseManager will subscribe for events.</param>
            /// <param name="rightHand">Right hand to which KeyPoseManager will subscribe for events.</param>
            public KeyposeManager(Hand leftHand, Hand rightHand)
            {
                // Array length excludes [NoHand], since we do not allow it to be disabled.
                this.config.KeyposeConfig = new byte[(int)MLHandTracking.HandKeyPose.NoHand];

                this.hands[(int)MLHandTracking.HandType.Left] = leftHand;
                this.hands[(int)MLHandTracking.HandType.Right] = rightHand;

                // Start the Unity gesture subsystem.s
                MLDevice.RegisterGestureSubsystem();
                if (MLDevice.GestureSubsystem != null)
                {
                    MLDevice.GestureSubsystem.onKeyPoseGestureChanged += this.HandleOnKeyPoseChanged;
                }
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="KeyposeManager" /> class.
            /// </summary>
            ~KeyposeManager()
            {
                this.DisableAllKeyPoses();

                if (MLDevice.GestureSubsystem != null)
                {
                    MLDevice.GestureSubsystem.onKeyPoseGestureChanged -= this.HandleOnKeyPoseChanged;
                    MLDevice.UnregisterGestureSubsystem();
                }
            }

            /// <summary>
            /// The delegate for when a key pose has been first detected by some hand.
            /// </summary>
            /// <param name="pose">The key pose that has been detected.</param>
            /// <param name="type">The hand that has performed the pose.</param>
            public delegate void OnHandKeyPoseBeginDelegate(MLHandTracking.HandKeyPose pose, MLHandTracking.HandType type);

            /// <summary>
            /// The delegate for when a previously detected key pose is no longer detected.
            /// </summary>
            /// <param name="pose">The key pose that has been detected.</param>
            /// <param name="type">The hand that has performed the key pose.</param>
            public delegate void OnHandKeyPoseEndDelegate(MLHandTracking.HandKeyPose pose, MLHandTracking.HandType type);

            /// <summary>
            /// Event is raised whenever a key pose starts being recognized.
            /// </summary>
            public event OnHandKeyPoseBeginDelegate OnKeyPoseBegin = delegate { };

            /// <summary>
            /// Event is raised whenever a key pose stops being recognized.
            /// </summary>
            public event OnHandKeyPoseEndDelegate OnKeyPoseEnd = delegate { };

            /// <summary>
            /// Gets the currently enabled key poses.
            /// </summary>
            public List<MLHandTracking.HandKeyPose> EnabledKeyPoses
            {
                get
                {
                    var enabledHandKeyPoses = new List<MLHandTracking.HandKeyPose>();
                    for (var i = 0; i < this.config.KeyposeConfig.Length; ++i)
                    {
                        if (Convert.ToBoolean(this.config.KeyposeConfig[i]))
                        {
                            enabledHandKeyPoses.Add((MLHandTracking.HandKeyPose)i);
                        }
                    }

                    return enabledHandKeyPoses;
                }
            }

            /// <summary>
            /// Gets the key points filter level.
            /// </summary>
            /// <returns>The filter level.</returns>
            public MLHandTracking.KeyPointFilterLevel KeyPointsFilterLevel
            {
                get { return this.config.KeyPointsFilterLevel; }
            }

            /// <summary>
            /// Gets the pose filter level.
            /// </summary>
            /// <returns>The filter level.</returns>
            public MLHandTracking.PoseFilterLevel PoseFilterLevel
            {
                get { return this.config.PoseFilterLevel; }
            }

            /// <summary>
            /// Disables all the key poses.
            /// </summary>
            /// <returns>True if all key poses were disabled and the config was updated successfully</returns>
            public bool DisableAllKeyPoses()
            {
                this.SetKeyPoseConfig(0);
                //// Disabling hand tracking pipeline as turning off all key poses will internally
                //// disable the pipeline if we don't.
                this.config.HandTrackingPipelineEnabled = false;
                return this.ApplyConfig();
            }

            /// <summary>
            /// Enables or disables an array of key poses.
            /// Enabling too many key poses will currently lead to decreased sensitivity to each
            /// individual key pose.
            /// </summary>
            /// <param name="keyPoses">The list of key poses to affect.</param>
            /// <param name="enable">Enable or disable key poses.</param>
            /// <param name="exclusive">
            /// When enabling and this is true, only the list of provided key poses
            /// are enabled, all other previously-enabled key poses get disabled. No effect if
            /// parameter enable is false.
            /// </param>
            /// <returns>
            /// True if the chosen key poses were successfully enabled/disabled and applied to the key pose config.
            /// </returns>
            public bool EnableKeyPoses(MLHandTracking.HandKeyPose[] keyPoses, bool enable, bool exclusive = false)
            {
                if (keyPoses == null || keyPoses.Length <= 0)
                {
                    MLPluginLog.Error("KeyPoseManager.EnableKeyPoses passed key poses array is null or empty.");
                    return false;
                }

                if (enable)
                {
                    // Enable the hand tracking pipeline.
                    this.config.HandTrackingPipelineEnabled = true;
                    if (exclusive)
                    {
                        // Disable all other previous key poses.
                        this.SetKeyPoseConfig(0);
                    }
                }

                foreach (var keyPoseType in keyPoses)
                {
                    SetKeyPoseConfig(this.config, keyPoseType, enable);
                }

                return this.ApplyConfig();
            }

            /// <summary>
            /// Sets the key points filter level.
            /// </summary>
            /// <param name="filterLevel">The desired filter level.</param>
            /// <returns>true if the filter level was successfully applied and false otherwise.</returns>
            public bool SetKeyPointsFilterLevel(MLHandTracking.KeyPointFilterLevel filterLevel)
            {
                this.config.KeyPointsFilterLevel = filterLevel;
                return this.ApplyConfig();
            }

            /// <summary>
            /// Sets the pose filter level.
            /// </summary>
            /// <param name="filterLevel">The desired filter level.</param>
            /// <returns>true if the filter level was successfully applied and false otherwise.</returns>
            public bool SetPoseFilterLevel(MLHandTracking.PoseFilterLevel filterLevel)
            {
                this.config.PoseFilterLevel = filterLevel;
                return this.ApplyConfig();
            }

            /// <summary>
            /// Sets the values of key poses given by a raw byte array representing a list of key poses.
            /// </summary>
            /// <param name="keyPoses">The raw byte array of key poses to set.</param>
            /// <param name="value">The raw byte value to set all the key pose with.</param>
            private static void SetKeyPoseConfig(byte[] keyPoses, byte value)
            {
                for (var i = 0; i < keyPoses.Length; ++i)
                {
                    keyPoses[i] = value;
                }
            }

            /// <summary>
            /// Sets the values of a specific key pose.
            /// </summary>
            /// <param name="config">The native config object of key poses to use.</param>
            /// <param name="handKeyPose">The key pose to set.</param>
            /// <param name="enable">The value to set the key pose with.</param>
            private static void SetKeyPoseConfig(NativeBindings.ConfigurationNative config, MLHandTracking.HandKeyPose handKeyPose, bool enable)
            {
                if (handKeyPose > MLHandTracking.HandKeyPose.NoPose)
                {
                    MLPluginLog.WarningFormat("KeyPoseManager.SetKeyPoseConfig trying to set {0}. Ignoring.", handKeyPose);
                    return;
                }

                config.KeyposeConfig[(uint)handKeyPose] = Convert.ToByte(enable);
            }

            /// <summary>
            /// Sets the configuration of all key poses to enabled or disabled.
            /// </summary>
            /// <param name="value">The raw byte representation that determines whether to enable or disable all key poses.</param>
            private void SetKeyPoseConfig(byte value)
            {
                SetKeyPoseConfig(this.config.KeyposeConfig, value);
            }

            /// <summary>
            /// Caches the current status of the this.config object.
            /// </summary>
            /// <returns>True if the this.config object was updated successfully.</returns>
            private bool ApplyConfig()
            {
                try
                {
                    NativeBindings.UpdateConfiguration(ref this.config);
                }
                catch (EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLHandTracking.ApplyConfig failed. Reason: API symbols not found");
                }

                return true;
            }

            /// <summary>
            /// Method that gets called when a key pose has been changed or was just detected.
            /// </summary>
            /// <param name="gestureEvent">The object containing info on the detected key pose and hand.</param>
            private void HandleOnKeyPoseChanged(MagicLeapKeyPoseGestureEvent gestureEvent)
            {
                if (gestureEvent.state == GestureState.Started)
                {
                    // Notify the hand of the key pose update.
                    this.hands[(int)gestureEvent.hand].BeginKeyPose((MLHandTracking.HandKeyPose)gestureEvent.keyPose);

                    // Invoke the event from KeyPoseManager.
                    this.OnKeyPoseBegin?.Invoke((MLHandTracking.HandKeyPose)gestureEvent.keyPose, (MLHandTracking.HandType)gestureEvent.hand);
                }
                else if (gestureEvent.state == GestureState.Completed)
                {
                    // Notify the hand of the key pose update.
                    this.hands[(int)gestureEvent.hand].EndKeyPose((MLHandTracking.HandKeyPose)gestureEvent.keyPose);

                    // Invoke the event from KeyPoseManager.
                    this.OnKeyPoseEnd?.Invoke((MLHandTracking.HandKeyPose)gestureEvent.keyPose, (MLHandTracking.HandType)gestureEvent.hand);
                }
            }
        }
    }
}
#endif
