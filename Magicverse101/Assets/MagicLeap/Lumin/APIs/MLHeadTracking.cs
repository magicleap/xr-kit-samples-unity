// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLHeadTracking.cs" company="Magic Leap, Inc">
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
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// The MLHeadTracking API used to get head tracking state and triggered map events.
    /// </summary>
    public sealed partial class MLHeadTracking : MLAPISingleton<MLHeadTracking>
    {
        #if PLATFORM_LUMIN

        /// <summary>
        /// The time limit used by the error timers.
        /// </summary>
        private const float ErrorTimeLimit = 3f;

        /// <summary>
        /// A handle to the head tracker.
        /// </summary>
        private ulong handle = MagicLeapNativeBindings.InvalidHandle;

        /// <summary>
        /// A handle to the map events bitmask.
        /// </summary>
        private ulong mapEvents = 0;

        /// <summary>
        /// Timer that dictates how often to produce an error log when <c>UpdateHeadposeState</c> fails.
        /// </summary>
        private Timer headTrackingErrorTimer = new Timer(ErrorTimeLimit);

        /// <summary>
        /// Timer that dictates how often to produce an error log when PollMapEvents fails.
        /// </summary>
        private Timer mapEventsErrorTimer = new Timer(ErrorTimeLimit);

        /// <summary>
        /// Contains the result code of the last head tracking state get call.
        /// </summary>
        private MLResult.Code lastHeadTrackingGetStateResult = MLResult.Code.Ok;

        /// <summary>
        /// Current head tracking state.
        /// </summary>
        private MLHeadTracking.State lastHeadTrackingState;

        /// <summary>
        /// Cached struct for passing around native state data.
        /// </summary>
        private NativeBindings.StateNative stateNative = new NativeBindings.StateNative();

        /// <summary>
        /// Cached struct for passing around native static data.
        /// </summary>
        private NativeBindings.StaticDataNative staticDataNative = new NativeBindings.StaticDataNative();

        /// <summary>
        /// Delegate to handle head tracking state changes.
        /// </summary>
        /// <param name="state">The new state.</param>
        public delegate void OnHeadTrackingModeChangedDelegate(MLHeadTracking.State state);

        /// <summary>
        /// Delegate to handle head tracking map events.
        /// </summary>
        /// <param name="mapEvent">The new map events.</param>
        public delegate void OnHeadTrackingMapEventDelegate(MLHeadTracking.MapEvents mapEvent);

        /// <summary>
        /// Event triggered on head tracking state changes.
        /// </summary>
        private event OnHeadTrackingModeChangedDelegate OnHeadTrackingModeChanged = delegate { };

        /// <summary>
        /// Event triggered on new head tracking map events.
        /// </summary>
        private event OnHeadTrackingMapEventDelegate OnHeadTrackingMapEvent = delegate { };

        #endif

        /// <summary>
        /// A set of possible error conditions that can cause Head Tracking to be less than ideal.
        /// </summary>
        public enum TrackingError
        {
            /// <summary>
            /// No error, tracking is nominal.
            /// </summary>
            None,

            /// <summary>
            /// There are not enough features in the environment.
            /// </summary>
            NotEnoughFeatures,

            /// <summary>
            /// Lighting in the environment is not sufficient to track accurately.
            /// </summary>
            LowLight,

            /// <summary>
            /// Head tracking failed for an unknown reason.
            /// </summary>
            Unknown
        }

        /// <summary>
        /// A set of possible tracking modes the Head Tracking system can be in.
        /// </summary>
        public enum TrackingMode
        {
            /// <summary>
            /// Full 6 degrees of freedom tracking (position and orientation).
            /// </summary>
            Mode6DO,

            /// <summary>
            /// Head tracking is unavailable.
            /// </summary>
            ModeUnavailable
        }

        /// <summary>
        /// A set of all types of map events that can occur that a developer
        /// may have to handle.
        /// </summary>
        [Flags]
        public enum MapEvents : uint
        {
            /// <summary>
            /// Map was lost. It could possibly recover.
            /// </summary>
            Lost = (1 << 0),

            /// <summary>
            /// Previous map was recovered.
            /// </summary>
            Recovered = (1 << 1),

            /// <summary>
            /// Failed to recover previous map.
            /// </summary>
            RecoveryFailed = (1 << 2),

            /// <summary>
            /// New map session created.
            /// </summary>
            NewSession = (1 << 3)
        }

        #if PLATFORM_LUMIN

        /// <summary>
        /// Gets the latest head tracking state.
        /// </summary>
        public static MLHeadTracking.State TrackingState
        {
            get
            {
                if (MLHeadTracking.IsValidInstance())
                {
                    return _instance.lastHeadTrackingState;
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLHeadTracking.CurrentState failed to return the latest tracking state. Reason: No Instance for MLHeadTracking.");
                    return new State();
                }
            }
        }

        /// <summary>
        /// Starts the HeadTracking API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to other internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLHeadTracking.BaseStart();
        }

        /// <summary>
        /// Registers a callback to be called when only rotation head pose is available,
        /// or when full head pose is available again.
        /// </summary>
        /// <param name="callback">Callback to register.</param>
        public static void RegisterHeadTrackingModeChanged(OnHeadTrackingModeChangedDelegate callback)
        {
            if (MLHeadTracking.IsValidInstance())
            {
                _instance.OnHeadTrackingModeChanged += callback;
            }
            else
            {
                MLPluginLog.ErrorFormat("MLHeadTracking.RegisterHeadTrackingModeChanged failed. Reason: No Instance for MLHeadTracking.");
            }
        }

        /// <summary>
        /// Unregisters a previously registered head tracking mode change callback.
        /// </summary>
        /// <param name="callback">Callback to unregister.</param>
        public static void UnregisterHeadTrackingModeChanged(OnHeadTrackingModeChangedDelegate callback)
        {
            if (MLHeadTracking.IsValidInstance())
            {
                _instance.OnHeadTrackingModeChanged -= callback;
            }
            else
            {
                MLPluginLog.ErrorFormat("MLHeadTracking.UnregisterHeadTrackingModeChanged failed. Reason: No Instance for MLHeadTracking.");
            }
        }

        /// <summary>
        /// Registers a callback to be called when map events occur.
        /// This functionality is only supported on platform API level 2 and onwards.
        /// </summary>
        /// <param name="callback">Callback to register.</param>
        public static void RegisterOnHeadTrackingMapEvent(OnHeadTrackingMapEventDelegate callback)
        {
            if (MLHeadTracking.IsValidInstance())
            {
                _instance.OnHeadTrackingMapEvent += callback;
            }
            else
            {
                MLPluginLog.ErrorFormat("MLHeadTracking.RegisterOnHeadTrackingMapEvent failed. Reason: No Instance for MLHeadTracking.");
            }
        }

        /// <summary>
        /// Unregisters a previously registered map event callback.
        /// This functionality is only supported on platform API level 2 and onwards.
        /// </summary>
        /// <param name="callback">Callback to unregister.</param>
        public static void UnregisterOnHeadTrackingMapEvent(OnHeadTrackingMapEventDelegate callback)
        {
            if (MLHeadTracking.IsValidInstance())
            {
                _instance.OnHeadTrackingMapEvent -= callback;
            }
            else
            {
                MLPluginLog.ErrorFormat("MLHeadTracking.UnregisterOnHeadTrackingMapEvent failed. Reason: No Instance for MLHeadTracking.");
            }
        }

        /// <summary>
        /// Gets the most recent Head Tracking state.
        /// </summary>
        /// <param name="state">Some MLHeadTracking.State object to be filled with current state information.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the head tracking state was successfully received.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if the outState parameter was not valid (null).
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed to receive head tracking state.
        /// </returns>
        public static MLResult GetState(out State state)
        {
            state = new State();

            if (MLHeadTracking.IsValidInstance())
            {
                try
                {
                    MLResult.Code resultCode = NativeBindings.MLHeadTrackingGetState(_instance.handle, ref _instance.stateNative);

                    state.Mode = (TrackingMode)_instance.stateNative.Mode;
                    state.Confidence = _instance.stateNative.Confidence;
                    state.Error = (TrackingError)_instance.stateNative.Error;
                    state.Handle = _instance.handle;

                    return MLResult.Create(resultCode);
                }
                catch (EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLHeadTracking.GetState failed. Reason: API symbols not found.");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure);
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLHeadTracking.GetState failed. Reason: No Instance for MLHeadTracking.");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLHeadTracking.GetState failed. Reason: No Instance for MLHeadTracking.");
            }
        }

        #if !DOXYGENSHOULDSKIPTHIS
        /// <summary>
        /// Starts the HeadTracking API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if connected to MLContacts successfully.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        protected override MLResult StartAPI()
        {
            MLResult.Code resultCode = MLResult.Code.UnspecifiedFailure;

            try
            {
                resultCode = NativeBindings.MLHeadTrackingCreate(ref _instance.handle);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLHeadTracking.StartAPI failed to create native head tracker.");
                }

                resultCode = NativeBindings.MLHeadTrackingGetStaticData(_instance.handle, ref _instance.staticDataNative);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLHeadTracking.StartAPI failed to get static date from the native head tracker.");
                }
            }
            catch (EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLHeadTracking.StartAPI failed. Reason: API symbols not found.");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure);
            }

            return MLResult.Create(resultCode);
        }

        /// <summary>
        /// Cleans up API and unmanaged memory.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Allow complete cleanup of the API.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            //// Head Tracker is destroyed in the XR package, it should not be destroyed outside of it.
        }
        #endif // DOXYGENSHOULDSKIPTHIS

        /// <summary>
        /// Updates the key pose state based on the provided snapshot.
        /// </summary>
        protected override void Update()
        {
            if (this.OnHeadTrackingModeChanged != null)
            {
                this.UpdateHeadposeState();
            }

            if (this.OnHeadTrackingMapEvent != null)
            {
                this.PollMapEvents();
            }
        }

        /// <summary>
        /// Static instance of the MLHeadTracking class.
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLHeadTracking.IsValidInstance())
            {
                MLHeadTracking._instance = new MLHeadTracking();
            }
        }

        /// <summary>
        /// Retrieves the latest head-pose state.
        /// </summary>
        private void UpdateHeadposeState()
        {
            MLResult result = MLHeadTracking.GetState(out State headTrackingState);

            if (!result.IsOk)
            {
                if (this.headTrackingErrorTimer.LimitPassed)
                {
                    MLPluginLog.ErrorFormat("MLHeadTracking.UpdateHeadposeState failed to get head pose state. Reason: {0}", result);
                    this.headTrackingErrorTimer.Reset();
                }

                return;
            }
            else if (result.IsOk && this.lastHeadTrackingGetStateResult != MLResult.Code.Ok)
            {
                MLPluginLog.Warning("MLHeadTracking.UpdateHeadposeState is able to get head pose state again");
            }

            this.lastHeadTrackingGetStateResult = result.Result;

            bool headTrackingModeChanged = this.lastHeadTrackingState.Mode != headTrackingState.Mode;
            this.lastHeadTrackingState = headTrackingState;

            if (headTrackingModeChanged)
            {
                this.OnHeadTrackingModeChanged?.Invoke(headTrackingState);
            }
        }

        /// <summary>
        /// Polls the map events that occurred within the last frame.
        /// </summary>
        private void PollMapEvents()
        {
            MLResult result;
            if (MLHeadTracking.IsValidInstance())
            {
                try
                {
                    MLResult.Code resultCode = NativeBindings.MLHeadTrackingGetMapEvents(_instance.handle, ref _instance.mapEvents);
                    result = MLResult.Create(resultCode);

                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLHeadTracking.PollMapEvents failed to get latest map events. Reason: {0}", resultCode);
                    }
                }
                catch (EntryPointNotFoundException)
                {
                    MLPluginLog.Error("MLHeadTracking.PollMapEvents failed. Reason: API symbols not found");
                    result = MLResult.Create(MLResult.Code.UnspecifiedFailure);
                }
            }
            else
            {
                MLPluginLog.ErrorFormat("MLHeadTracking.PollMapEvents failed. Reason: No Instance for MLHeadTracking.");
                result = MLResult.Create(MLResult.Code.UnspecifiedFailure);
            }

            if (!result.IsOk)
            {
                if (this.mapEventsErrorTimer.LimitPassed)
                {
                    MLPluginLog.ErrorFormat("MLHeadTracking.PollMapEvents failed to get latest map events. Reason: {0}", result);
                    this.mapEventsErrorTimer.Reset();
                }

                return;
            }

            if (_instance.mapEvents != 0)
            {
                this.OnHeadTrackingMapEvent?.Invoke((MapEvents)_instance.mapEvents);
            }
        }

        #endif

        /// <summary>
        /// A structure containing information on the current state of the
        /// Head Tracking system.
        /// </summary>
        public struct State
        {
            /// <summary>
            /// What tracking mode the Head Tracking system is currently in.
            /// </summary>
            public TrackingMode Mode;

            /// <summary>
            /// A confidence value (from 0..1) representing the confidence in the
            /// current pose estimation.
            /// </summary>
            public float Confidence;

            /// <summary>
            /// Represents what tracking error (if any) is present.
            /// </summary>
            public TrackingError Error;

            /// <summary>
            /// The handle to the native head tracker.
            /// </summary>
            public ulong Handle;
        }
    }
}
