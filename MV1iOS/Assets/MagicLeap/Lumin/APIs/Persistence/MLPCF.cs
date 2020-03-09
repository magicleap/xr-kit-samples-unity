// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLPCF.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// The Persistent Coordinate Frames API.
    /// </summary>
    public partial class MLPersistentCoordinateFrames
    {
        /// <summary>
        /// PCF class is an abstraction for representing anchor points, called Persistent
        /// Coordinate Frames or PCFs, in the real world. PCFs cannot be created, modified or
        /// destroyed from the app level. Rather, we query the OS for any PCFs it has stored
        /// and query again to determine the PCF location, if it is within the vicinity.
        /// </summary>
        [Serializable]
        public partial class PCF
        {
            #if PLATFORM_LUMIN
            /// <summary>
            /// The list of bindings associated with the PCF.
            /// </summary>
            private List<IBinding> bindings = new List<IBinding>();

            /// <summary>
            /// The pose of the PCF that contains all of it's transform information.
            /// </summary>
            private Pose pose;

            /// <summary>
            /// The state of the PCF that contains confidence values regarding how accurate it's current pose is.
            /// </summary>
            private State state;

            /// <summary>
            /// Used to determine when the PCF's pose has changed after it was updated.
            /// </summary>
            private bool poseChanged = false;

            /// <summary>
            /// Used to determine when the PCF's state has changed after it was updated.
            /// </summary>
            private bool stateChanged = false;

            /// <summary>
            /// Caches the most recent MLResult value from getting the PCF's pose.
            /// Pending means we only have a CFUID and that Position/Orientation and State are invalid.
            /// </summary>
            [NonSerialized]
            private MLResult.Code currentResultCode = MLResult.Code.Pending;

            /// <summary>
            /// The unique coordinate frame id of the PCF.
            /// </summary>
            [SerializeField, HideInInspector]
            private MagicLeapNativeBindings.MLCoordinateFrameUID cfuid;

            /// <summary>
            /// Initializes a new instance of the <see cref="PCF" /> class.
            /// </summary>
            /// <param name="id">The CFUID to give to the PCF.</param>
            public PCF(MagicLeapNativeBindings.MLCoordinateFrameUID id)
            {
                this.cfuid = id;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PCF" /> class.
            /// Used for when JSON deserializes a PCF and we need it's currentResultCode to be Pending.
            /// </summary>
            protected PCF()
            {
                this.currentResultCode = MLResult.Code.Pending;
            }

            /// <summary>
            /// Delegate for the OnStatusChange event, an event that can be used to hook into all PCF status changes.
            /// </summary>
            /// <param name="pcfStatus">The current status of the PCF.</param>
            /// <param name="pcf">The PCF that had it's status changed.</param>
            public delegate void OnStatusChangeHandle(PCF.Status pcfStatus, PCF pcf);

            /// <summary>
            /// OnPCFStatusChange event gets fired when a PCF is created, updated, lost, or regained.
            /// </summary>
            public static event OnStatusChangeHandle OnStatusChange;
            #endif

            /// <summary>
            /// Enumeration specifying the type of a persistent coordinate frame (PCF).
            /// Type is not fixed. PCF can vary in its type between multiple head pose sessions.
            /// A new head pose session is created when the device reboots or loses tracking.
            /// </summary>
            [Flags]
            public enum Types
            {
                /// <summary>
                /// PCF is available only in the current head pose session.
                /// This is PCF type is only available on the local device. It cannot be shared
                /// with other users and will not persist across device reboots.
                /// A SingleUserSingleSession type PCF can later be promoted to a SingleUserMultiSession type PCF.
                /// </summary>
                SingleUserSingleSession = 1,

                /// <summary>
                /// PCF is available across multiple head pose sessions.
                /// This PCF type is only available on the local device. It cannot be shared
                /// with other users but will persist across device reboots.
                /// </summary>
                SingleUserMultiSession = 2,

                /// <summary>
                /// PCF is available across multiple users and head pose sessions.
                /// This PCF type can be shared with other users in the same physical
                /// environment and will persist across device reboots. This PCF type requires
                /// that the user should enable the Shared World feature in the Settings app.
                /// </summary>
                MultiUserMultiSession = 4
            }

            /// <summary>
            /// The tracking status of the PCF.
            /// </summary>
            public enum Status
            {
                /// <summary>
                /// The PCF is lost and unreliable.
                /// </summary>
                Lost,

                /// <summary>
                /// The PCF has just been created/found for this map.
                /// </summary>
                Created,

                /// <summary>
                /// The PCF's pose has changed.
                /// </summary>
                Updated,

                /// <summary>
                /// The PCF has been found again after being Lost.
                /// </summary>
                Regained,

                /// <summary>
                /// The PCF is stable, it's pose has been queried for multiple times and has not changed.
                /// </summary>
                Stable
            }

            #if PLATFORM_LUMIN
            /// <summary>
            /// Gets the current tracking status of the PCF.
            /// </summary>
            public Status CurrentStatus
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the CFUID of the PCF.
            /// </summary>
            public MagicLeapNativeBindings.MLCoordinateFrameUID CFUID
            {
                get
                {
                    return this.cfuid;
                }
            }

            /// <summary>
            /// Gets a confidence value (from [0, 1]) representing the confidence in the
            /// Persistent Coordinate Frame error within the valid radius.
            /// </summary>
            public Vector3 Position
            {
                get
                {
                    return this.pose.position;
                }
            }

            /// <summary>
            /// Gets a confidence value (from [0, 1]) representing the confidence in the
            /// Persistent Coordinate Frame error within the valid radius.
            /// </summary>
            public Quaternion Rotation
            {
                get
                {
                    return this.pose.rotation;
                }
            }

            /// <summary>
            /// Gets a confidence value (from [0, 1]) representing the confidence in the
            /// Persistent Coordinate Frame error within the valid radius.
            /// </summary>
            public float Confidence
            {
                get
                {
                    return this.state.Confidence;
                }
            }

            /// <summary>
            /// Gets the radius (in meters) within which the confidence is valid.
            /// </summary>
            public float ValidRadiusM
            {
                get
                {
                    return this.state.ValidRadiusM;
                }
            }

            /// <summary>
            /// Gets the rotational error (in degrees).
            /// </summary>
            public float RotationErrDeg
            {
                get
                {
                    return this.state.RotationErrDeg;
                }
            }

            /// <summary>
            /// Gets the translation error (in meters).
            /// </summary>
            public float TranslationErrM
            {
                get
                {
                    return this.state.TranslationErrM;
                }
            }

            /// <summary>
            /// Gets the PCF type.
            /// PCFs can vary in their types between multiple head pose sessions.
            /// </summary>
            public Types Type
            {
                get
                {
                    return this.state.Type;
                }
            }

            /// <summary>
            /// Gets the MLResult from the last query for the PCF's pose. It could be one of the following:
            /// <c>MLResult.Code.Pending</c> - Position/Orientation does not exist.
            /// <c>MLResult.Code.Ok</c> - Position/Orientation is reliable.
            /// Otherwise - Position/Orientation is unreliable.
            /// </summary>
            public MLResult.Code CurrentResultCode
            {
                get
                {
                    return this.currentResultCode;
                }

                private set
                {
                    MLResult.Code oldCode = this.currentResultCode;
                    this.currentResultCode = value;
                    this.OnCurrentResultChanged(oldCode, value);
                }
            }

            /// <summary>
            /// Gets or sets the position of the PCF for the current session.
            /// </summary>
            private Pose Pose
            {
                get
                {
                    return this.pose;
                }

                set
                {
                    if (this.pose != value)
                    {
                        this.poseChanged = true;
                    }

                    this.pose = value;
                }
            }

            /// <summary>
            /// Gets or sets the position of the PCF for the current session.
            /// </summary>
            private State FrameState
            {
                get
                {
                    return this.state;
                }

                set
                {
                    if (this.state != value)
                    {
                        this.stateChanged = true;
                    }

                    this.state = value;
                }
            }

            /// <summary>
            /// Adds a binding to the PCF.
            /// You can listen to the global PCF.OnStatusChange event to react to PCF changes as well, but bindings will be called directly by the changed PCF.
            /// </summary>
            /// <param name="binding">The binding to add.</param>
            public void AddBinding(IBinding binding)
            {
                if (binding.PCF == this && !this.bindings.Contains(binding))
                {
                    this.bindings.Add(binding);
                }
            }

            /// <summary>
            /// Removes a binding from the PCF.
            /// </summary>
            /// <param name="binding">The binding to remove.</param>
            public void RemoveBinding(IBinding binding)
            {
                if (binding.PCF == this && this.bindings.Contains(binding))
                {
                    this.bindings.Remove(binding);
                }
            }

            /// <summary>
            /// Returns the string representation of the PCF. Use it only for debugging.
            /// </summary>
            /// <returns>Returns the string representation of the PCF.</returns>
            public override string ToString()
            {
                if (this.currentResultCode == MLResult.Code.Pending)
                {
                    return string.Format("Id: {0} - Invalid PCF", this.CFUID.ToString());
                }

                return string.Format(
                    "Id: {0} - ({1}, {2}, {3}, {4}, {5}, {6}) " +
                    "Confidence {7}, ValidRadiusM {8}, RotationErrDeg {9}, TranslationErrM {10}",
                     this.CFUID.ToString(),
                     this.pose.position.x,
                     this.pose.position.y,
                     this.pose.position.z,
                     this.pose.rotation.eulerAngles.x,
                     this.pose.rotation.eulerAngles.y,
                     this.pose.rotation.eulerAngles.z,
                     this.state.Confidence,
                     this.state.ValidRadiusM,
                     this.state.RotationErrDeg,
                     this.state.TranslationErrM);
            }

            /// <summary>
            /// Updates the PCF's pose and state.
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if operation was successful.
            /// MLResult.Result will be <c>MLResult.Code.SnapshotPoseNotFound</c> if the PCF could not be found in the current map.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldLowMapQuality</c> if map quality is too low for content persistence. Continue building the map.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldNotFound</c> if the passed CFUID is not available.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldUnableToLocalize</c> if currently unable to localize into any map. Continue building the map.
            /// </returns>
            public MLResult Update()
            {
                if (MLPersistentCoordinateFrames.IsValidInstance())
                {
                    if (!MLPersistentCoordinateFrames._instance.mapUpdatedPCFsThisFrame.ContainsKey(this.cfuid))
                    {
                        MLPersistentCoordinateFrames._instance.mapUpdatedPCFsThisFrame.Add(this.cfuid, this);

                        try
                        {
                            MLResult result = MLResult.Create(MLResult.Code.Ok);

                            if (MLPersistentCoordinateFrames.IsLocalized)
                            {
                                if (MagicLeapNativeBindings.UnityMagicLeap_TryGetPose(this.CFUID, out this.pose))
                                {
                                    result = this.UpdateState();

                                    if (!result.IsOk)
                                    {
                                        MLPluginLog.ErrorFormat("PCF.Update failed because PCF.UpdateState failed. Reason: {0}", result);
                                        return MLResult.Create(MLResult.Code.UnspecifiedFailure, string.Format("PCF.Update failed because PCF.UpdateState failed. Reason: {0}", result));
                                    }

                                    this.CurrentResultCode = MLResult.Code.Ok;
                                }
                                else
                                {
                                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure, string.Format("PCF.Update failed. Reason: No pose could be found for the CFUID {0}", this.cfuid));
                                    this.CurrentResultCode = MLResult.Code.SnapshotPoseNotFound;
                                }
                            }
                            else
                            {
                                this.CurrentResultCode = MLResult.Code.SnapshotPoseNotFound;
                            }

                            return result;
                        }
                        catch (EntryPointNotFoundException)
                        {
                            MLPluginLog.Error("PCF.Update failed. Reason: API symbols not found.");
                            return MLResult.Create(MLResult.Code.UnspecifiedFailure, "PCF.Update failed. Reason: API symbols not found.");
                        }
                    }
                    else
                    {
                        return MLResult.Create(MLResult.Code.Ok);
                    }
                }
                else
                {
                    MLPluginLog.ErrorFormat("PCF.Update failed. Reason: No Instance for MLPersistentCoordinateFrames.");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "PCF.Update failed. Reason: No Instance for MLPersistentCoordinateFrames.");
                }
            }

            /// <summary>
            /// Updates the state of the PCF.
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if failed due to an invalid input parameter.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if there was a lack of privileges.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldLowMapQuality</c> if map quality is too low for content persistence. Continue building the map.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldNotFound</c> if the passed CFUID is not available.
            /// MLResult.Result will be <c>MLResult.Code.PassableWorldUnableToLocalize</c> if currently unable to localize into any map. Continue building the map.
            /// </returns>
            private MLResult UpdateState()
            {
                if (MLPersistentCoordinateFrames.IsValidInstance())
                {
                    try
                    {
                        NativeBindings.FrameStateNative nativeState = NativeBindings.FrameStateNative.Create();
                        MLResult.Code resultCode = NativeBindings.MLPersistentCoordinateFramesGetFrameState(MLPersistentCoordinateFrames._instance.nativeTracker, in this.cfuid, ref nativeState);
                        if (!MLResult.IsOK(resultCode))
                        {
                            MLPluginLog.ErrorFormat("PCF.UpdateState failed to get frame state. Reason: {0}", MLResult.CodeToString(resultCode));
                            return MLResult.Create(resultCode, string.Format("PCF.UpdateState failed to get frame state. Reason: {0}", MLResult.CodeToString(resultCode)));
                        }

                        this.FrameState = nativeState.Data();

                        return MLResult.Create(resultCode);
                    }
                    catch (EntryPointNotFoundException)
                    {
                        MLPluginLog.Error("PCF.UpdateState failed. Reason: API symbols not found.");
                        return MLResult.Create(MLResult.Code.UnspecifiedFailure, "PCF.UpdateState failed. Reason: API symbols not found.");
                    }
                }
                else
                {
                    MLPluginLog.ErrorFormat("PCF.UpdateState failed. Reason: No Instance for MLPersistentCoordinateFrames.");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "PCF.UpdateState failed. Reason: No Instance for MLPersistentCoordinateFrames.");
                }
            }

            /// <summary>
            /// Updates the tracking status of the PCF.
            /// </summary>
            /// <param name="newStatus">The new status to update the PCF with.</param>
            private void UpdateStatus(Status newStatus)
            {
                Status oldStatus = this.CurrentStatus;
                this.CurrentStatus = newStatus;

                if ((newStatus != oldStatus && newStatus != PCF.Status.Stable) || newStatus == PCF.Status.Updated)
                {
                    OnStatusChange?.Invoke(newStatus, this);
                }
            }

            /// <summary>
            /// Updates the tracking status of the PCF based on the current and last MLResult
            /// received when querying for the PCF's pose.
            /// </summary>
            /// <param name="oldCode">The MLResult from the previous pose query.</param>
            /// <param name="newCode">The MLResult from the current pose query.</param>
            /* -----------------------------------------
            * OldCode                                   | NewCode | Event
            * ------------------------------------------|---------|---------------
            * Pending                                   | Ok      | Create
            * !Ok & !Pending                            | Ok      | Regain
            * Ok & (poseChanged || stateChanged)        | Ok      | Update
            * Ok & !(poseChanged || stateChanged)       | Ok      | Stable
            * Ok                                        | !Ok     | Lost
            * ----------------------------------------- */
            private void OnCurrentResultChanged(MLResult.Code oldCode, MLResult.Code newCode)
            {
                if (MLResult.IsOK(newCode))
                {
                    if (oldCode == MLResult.Code.Pending)
                    {
                        this.UpdateStatus(Status.Created);
                    }
                    else if (!MLResult.IsOK(oldCode))
                    {
                        foreach (IBinding binding in this.bindings)
                        {
                            binding.Regain();
                        }

                        this.UpdateStatus(Status.Regained);
                    }
                    else if (this.poseChanged || this.stateChanged)
                    {
                        foreach (IBinding binding in this.bindings)
                        {
                            binding.Update();
                        }

                        this.UpdateStatus(Status.Updated);

                        this.poseChanged = false;
                        this.stateChanged = false;
                    }
                    else
                    {
                        this.UpdateStatus(Status.Stable);
                    }
                }
                else
                {
                    foreach (IBinding binding in this.bindings)
                    {
                        binding.Lost();
                    }

                    this.UpdateStatus(Status.Lost);
                }
            }

            /// <summary>
            /// The structure for tracking the Persistent Coordinate Frame's state.
            /// </summary>
            public struct State
            {
                /// <summary>
                /// A confidence value (from [0, 1]) representing the confidence in the
                /// Persistent Coordinate Frame error within the valid radius.
                /// </summary>
                public float Confidence;

                /// <summary>
                /// The radius (in meters) within which the confidence is valid.
                /// </summary>
                public float ValidRadiusM;

                /// <summary>
                /// The rotational error (in degrees).
                /// </summary>
                public float RotationErrDeg;

                /// <summary>
                /// The translation error (in meters).
                /// </summary>
                public float TranslationErrM;

                /// <summary>
                /// The PCF type.
                /// PCFs can vary in their types between multiple head pose sessions.
                /// </summary>
                public PCF.Types Type;

                /// <summary>
                /// The inequality check to be used for comparing two PCF.State structs.
                /// </summary>
                /// <param name="one">The first struct to compare with the second struct.</param>
                /// <param name="two">The second struct to compare with the first struct.</param>
                /// <returns>True if the two provided structs do not have the same field values.</returns>
                public static bool operator !=(State one, State two)
                {
                    return !(one.Confidence == two.Confidence && one.ValidRadiusM == two.ValidRadiusM && one.RotationErrDeg == two.RotationErrDeg
                            && one.TranslationErrM == two.TranslationErrM && one.Type == two.Type);
                }

                /// <summary>
                /// The inequality check to be used for comparing two PCF.State structs.
                /// </summary>
                /// <param name="one">The first struct to compare with the second struct.</param>
                /// <param name="two">The second struct to compare with the first struct.</param>
                /// <returns>True if the two provided structs do not have the same field values.</returns>
                public static bool operator ==(State one, State two)
                {
                    return one.Confidence == two.Confidence && one.ValidRadiusM == two.ValidRadiusM && one.RotationErrDeg == two.RotationErrDeg
                            && one.TranslationErrM == two.TranslationErrM && one.Type == two.Type;
                }

                /// <summary>
                /// The equality check to be used for comparing another object to this one.
                /// </summary>
                /// <param name="obj">The object to compare to this one with. </param>
                /// <returns>True if the the provided object is of the PCF.State type and has the same field values as this one.</returns>
                public override bool Equals(object obj)
                {
                    //// Check for null values and compare run-time types.
                    if (obj == null || GetType() != obj.GetType())
                    {
                        return false;
                    }

                    State p = (State)obj;
                    return this == p;
                }

                /// <summary>
                /// Gets the hash code to use from all the fields of this struct.
                /// </summary>
                /// <returns>The hash code returned by all the fields of this struct.</returns>
                public override int GetHashCode()
                {
                    return this.Confidence.GetHashCode() ^ this.ValidRadiusM.GetHashCode() ^ this.RotationErrDeg.GetHashCode() ^ this.TranslationErrM.GetHashCode() ^ this.Type.GetHashCode();
                }
            }
            #endif
        }
    }
}
