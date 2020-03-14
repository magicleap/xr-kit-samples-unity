// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLAudio.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Manages Audio.
    /// </summary>
    public sealed partial class MLAudio : MLAPISingleton<MLAudio>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// The current audio device.
        /// </summary>
        private Device audioDevice = Device.Wearable;

        /// <summary>
        /// The last audio device.
        /// </summary>
        private Device lastAudioDevice = Device.Wearable;

        /// <summary>
        /// The value of the master volume.
        /// </summary>
        private float masterVolume;

        /// <summary>
        /// The mute state of the microphone.
        /// </summary>
        private bool isMicrophoneMuted;

        /// <summary>
        /// Prevents a default instance of the <see cref="MLAudio" /> class from being created.
        /// </summary>
        private MLAudio()
        {
            this.DllNotFoundError = "MLAudio API is currently available only on device.";
        }

        /// <summary>
        /// The delegate for the master volume changed event.
        /// </summary>
        /// <param name="volume">The new master volume value.</param>
        public delegate void OnMasterVolumeChangedDelegate(float volume);

        /// <summary>
        /// The delegate for the microphone mute changed event.
        /// </summary>
        /// <param name="muted">The new mute state of the microphone.</param>
        public delegate void OnMicrophoneMuteChangedDelegate(bool muted);

        /// <summary>
        /// The delegate for audio output device changed event.
        /// </summary>
        /// <param name="device">The new audio output device.</param>
        public delegate void OnAudioOutputDeviceChangedDelegate(Device device);

        /// <summary>
        /// Raised whenever the master volume gets changed.
        /// </summary>
        public static event OnMasterVolumeChangedDelegate OnMasterVolumeChanged = delegate { };

        /// <summary>
        /// Raised whenever the global microphone mute gets changed.
        /// </summary>
        public static event OnMicrophoneMuteChangedDelegate OnMicrophoneMuteChanged = delegate { };

        /// <summary>
        /// Raised whenever the audio output device gets changed.
        /// </summary>
        public static event OnAudioOutputDeviceChangedDelegate OnAudioOutputDeviceChanged = delegate { };
        #endif

        /// <summary>
        /// The currently active output device.
        /// </summary>
        public enum Device : uint
        {
            /// <summary>
            /// Built-in speakers in the wearable.
            /// </summary>
            Wearable,

            /// <summary>
            /// 3.5mm jack on the belt pack.
            /// </summary>
            AnalogJack
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Gets the audio output device.
        /// </summary>
        public static Device AudioOutputDevice
        {
            get
            {
                return Instance.InternalGetOutputDevice();
            }
        }

        /// <summary>
        /// Gets the master volume for the device.
        /// </summary>
        public static float MasterVolume
        {
            get { return Instance.masterVolume; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the microphone is muted.
        /// </summary>
        public static bool MicrophoneMuted
        {
            get
            {
                return Instance.isMicrophoneMuted;
            }

            set
            {
                Instance.InternalSetMicMute(value);
            }
        }

        /// <summary>
        /// Starts the Audio API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if <c>AudioCaptureMic</c> privilege is denied.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLAudio.BaseStart();
        }

        /// <summary>
        /// Gets the result string for a MLResult.Code.
        /// </summary>
        /// <param name="result">The MLResult.Code to be requested.</param>
        /// <returns>A pointer to the result string.</returns>
        internal static IntPtr GetResultString(MLResult.Code result)
        {
            return NativeBindings.MLAudioGetResultString(result);
        }

#if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Called by MLAPISingleton to start the API
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a parameter is invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if <c>AudioCaptureMic</c> privilege is denied.
        /// MLResult.Result will be <c>MLResult.Code.AudioNotImplemented</c> if the function is not implemented.
        /// </returns>
        protected override MLResult StartAPI()
        {
            // Set the initial audio device.
            Instance.lastAudioDevice = Instance.InternalGetOutputDevice();

            MLResult.Code result;

            // Get the initial MasterVolume value.
            result = Instance.GetMasterVolume(out this.masterVolume);
            if (result != MLResult.Code.Ok)
            {
                return MLResult.Create(result);
            }

            // Master Volume Callback
            result = this.RegisterOnVolumeChangeCallback();
            if (result != MLResult.Code.Ok)
            {
                return MLResult.Create(result);
            }

            // Attempt to register Microphone Muted Callback
            result = this.RegisterOnMicrophoneMuteCallback();
            if (result == MLResult.Code.PrivilegeDenied)
            {
                MLPluginLog.WarningFormat("MLAudio.StartAPI missing AudioCaptureMic privilege, microphone specific features disabled.");
                result = MLResult.Code.Ok;
            }
            else if (result == MLResult.Code.Ok)
            {
                // Get the initial IsMicrophoneMuted value.
                result = Instance.GetMicrophoneMuted(out this.isMicrophoneMuted);
                if (result != MLResult.Code.Ok)
                {
                    this.UnregisterOnVolumeChangeCallback();
                    this.UnregisterOnMicrophoneMuteCallback();
                    return MLResult.Create(result);
                }
            }
            else
            {
                this.UnregisterOnVolumeChangeCallback();
            }

            return MLResult.Create(result);
        }
#endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Called by MLAPISingleton on destruction
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">Not Implemented</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            this.UnregisterOnVolumeChangeCallback();
            this.UnregisterOnMicrophoneMuteCallback();
        }

        /// <summary>
        /// Called every device frame
        /// </summary>
        protected override void Update()
        {
            if (this.lastAudioDevice != Instance.InternalGetOutputDevice())
            {
                Instance.lastAudioDevice = Instance.audioDevice;

                // Notify event listeners.
                // Callback is not needed to be in the queue because it is in Update.
                OnAudioOutputDeviceChanged?.Invoke(Instance.lastAudioDevice);
            }
        }

        /// <summary>
        /// Handles the callback for MLAudioSetMasterVolume.
        /// </summary>
        /// <param name="volume">The volume value.</param>
        /// <param name="callback">A pointer to the callback.</param>
        [AOT.MonoPInvokeCallback(typeof(NativeBindings.MLAudioMasterVolumeChangedCallback))]
        private static void HandleOnMLAudioSetMasterVolumeCallback(float volume, IntPtr callback)
        {
            Instance.masterVolume = volume;

            MLThreadDispatch.Call(volume, OnMasterVolumeChanged);
        }

        /// <summary>
        /// Handles the callback for <c>MLAudioSetMicMute</c>.
        /// </summary>
        /// <param name="isMuted">The mute state of the microphone.</param>
        /// <param name="callback">A pointer to the callback.</param>
        [AOT.MonoPInvokeCallback(typeof(NativeBindings.MLAudioMicMuteCallback))]
        private static void HandleOnMLAudioSetMicMuteCallback([MarshalAs(UnmanagedType.I1)] bool isMuted, IntPtr callback)
        {
            Instance.isMicrophoneMuted = isMuted;

            MLThreadDispatch.Call(isMuted, OnMicrophoneMuteChanged);
        }

        /// <summary>
        /// static instance of the MLAudio class
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLAudio.IsValidInstance())
            {
                MLAudio._instance = new MLAudio();
            }
        }

        /// <summary>
        /// Returns the master volume for the audio system.
        /// The range of the volume is 0-10, with 0 being silent and 10 being full volume.
        /// </summary>
        /// <param name="volume">The current volume value.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a parameter is invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if <c>AudioCaptureMic</c> privilege is denied.
        /// MLResult.Result will be <c>MLResult.Code.AudioNotImplemented</c> if the function is not implemented.
        /// </returns>
        private MLResult.Code GetMasterVolume(out float volume)
        {
            MLResult.Code result;

            try
            {
                result = NativeBindings.MLAudioGetMasterVolume(out volume);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLAudio.GetMasterVolume failed to get the volume. Reason: {0}", result);
                }
            }
            catch (System.DllNotFoundException)
            {
                // Exception is caught in the Singleton BaseStart().
                throw;
            }

            return result;
        }

        /// <summary>
        /// Returns the mute state of the microphone.
        /// </summary>
        /// <param name="isMuted">The mute state of the microphone.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a parameter is invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if <c>AudioCaptureMic</c> privilege is denied.
        /// MLResult.Result will be <c>MLResult.Code.AudioNotImplemented</c> if the function is not implemented.
        /// </returns>
        private MLResult.Code GetMicrophoneMuted(out bool isMuted)
        {
            MLResult.Code result;

            try
            {
                result = NativeBindings.MLAudioIsMicMuted(out isMuted);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLAudio.GetMicrophoneMuted failed to get the value. Reason: {0}", result);
                }
            }
            catch (System.DllNotFoundException)
            {
                // Exception is caught in the Singleton BaseStart().
                throw;
            }

            return result;
        }

        /// <summary>
        /// Registers a callback for the device volume change event.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a parameter is invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if <c>AudioCaptureMic</c> privilege is denied.
        /// MLResult.Result will be <c>MLResult.Code.AudioNotImplemented</c> if the function is not implemented.
        /// </returns>
        private MLResult.Code RegisterOnVolumeChangeCallback()
        {
            MLResult.Code result;

            try
            {
                // Attempt to register the native callback for the volume change event.
                result = NativeBindings.MLAudioSetMasterVolumeCallback(HandleOnMLAudioSetMasterVolumeCallback, IntPtr.Zero);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLAudio.RegisterOnVolumeChangeCallback failed to register callback. Reason: {0}", result);
                }
            }
            catch (System.DllNotFoundException)
            {
                // Exception is caught in the Singleton BaseStart().
                throw;
            }

            return result;
        }

        /// <summary>
        /// Unregisters a previously registered callback for the device volume change event.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a parameter is invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if <c>AudioCaptureMic</c> privilege is denied.
        /// MLResult.Result will be <c>MLResult.Code.AudioNotImplemented</c> if the function is not implemented.
        /// </returns>
        private MLResult.Code UnregisterOnVolumeChangeCallback()
        {
            MLResult.Code result;

            try
            {
                // Unregister the native callback for the volume change event.
                result = NativeBindings.MLAudioSetMasterVolumeCallback(null, IntPtr.Zero);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLAudio.UnregisterOnVolumeChangeCallback failed to register callback. Reason: {0}", result);
                }
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(this.DllNotFoundError);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Registers a callback for the device microphone mute change event.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a parameter is invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if <c>AudioCaptureMic</c> privilege is denied.
        /// MLResult.Result will be <c>MLResult.Code.AudioNotImplemented</c> if the function is not implemented.
        /// </returns>
        private MLResult.Code RegisterOnMicrophoneMuteCallback()
        {
            MLResult.Code result;

            try
            {
                // Attempt to register the native callback for the volume change event.
                result = NativeBindings.MLAudioSetMicMuteCallback(HandleOnMLAudioSetMicMuteCallback, IntPtr.Zero);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLAudio.RegisterOnAudioSetMicMuteCallback failed to register callback. Reason: {0}", result);
                }
            }
            catch (System.DllNotFoundException)
            {
                // Exception is caught in the Singleton BaseStart().
                throw;
            }

            return result;
        }

        /// <summary>
        /// Unregisters a previously registered callback for the device microphone mute change event.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if failed due to internal error.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if a parameter is invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if <c>AudioCaptureMic</c> privilege is denied.
        /// MLResult.Result will be <c>MLResult.Code.AudioNotImplemented</c> if the function is not implemented.
        /// </returns>
        private MLResult.Code UnregisterOnMicrophoneMuteCallback()
        {
            MLResult.Code result;

            try
            {
                // Unregister the native callback for the microphone mute change event.
                result = NativeBindings.MLAudioSetMicMuteCallback(null, IntPtr.Zero);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLAudio.UnregisterOnMicrophoneMuteCallback failed to register callback. Reason: {0}", result);
                }
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(this.DllNotFoundError);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Get the current audio output device.
        /// </summary>
        /// <returns>The audio output device.</returns>
        private MLAudio.Device InternalGetOutputDevice()
        {
            try
            {
                MLResult.Code result = NativeBindings.MLAudioGetOutputDevice(out Instance.audioDevice);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLAudio.InternalGetOutputDevice failed to get the audio output device. Reason: {0}", result);
                }
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(this.DllNotFoundError);
                throw;
            }

            return Instance.audioDevice;
        }

        /// <summary>
        /// Sets the mute state of the microphone.
        /// </summary>
        /// <param name="mute">The microphone mute state.</param>
        private void InternalSetMicMute(bool mute)
        {
            try
            {
                MLResult.Code result = NativeBindings.MLAudioSetMicMute(mute);
                if (result != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLAudio.InternalSetMicMute failed to set the value. Reason: {0}", result);
                }

                Instance.isMicrophoneMuted = mute;
            }
            catch (System.DllNotFoundException)
            {
                MLPluginLog.Error(this.DllNotFoundError);
                throw;
            }
        }
        #endif
    }
}
