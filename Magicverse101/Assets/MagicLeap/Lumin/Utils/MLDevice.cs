// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLDevice.cs" company="Magic Leap, Inc">
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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine.SceneManagement;
    using UnityEngine.XR.MagicLeap.Internal;
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// MagicLeap device class responsible for updating all trackers when they register and are enabled.
    /// </summary>
    public class MLDevice : UnityEngine.SpatialTracking.TrackedPoseDriver
    {
        /// <summary>
        /// Hardcoded value approximating minimum near clip plane distance allowed by the platform.
        /// </summary>
        public const float MinimumNearClipDistance = 0.37f;

        /// <summary>
        /// Hardcoded value with the default near clip plane distance to use.
        /// </summary>
        public const float DefaultNearClipDistance = 0.37f;

        /// <summary>
        /// MagicLeap platform Unity name.
        /// </summary>
        public const string MagicLeapDeviceName = "Lumin";

        /// <summary>
        /// Reference to the active MLDevice instance.
        /// </summary>
        private static MLDevice instance = null;

        /// <summary>
        /// World scale value.
        /// </summary>
        private static float worldScale = 1.0f;

        /// <summary>
        /// Value that determines if the world scale has been yet set.
        /// </summary>
        private static bool worldScaleInitialized = false;

        /// <summary>
        /// Reference to the active XR MagicLeap gestures subsystem.
        /// </summary>
        private MagicLeapGestures gestureSubsystem = null;

        /// <summary>
        /// Contains the value that specifies if the underlying Unity XR MagicLeap subsystem is initialized.
        /// </summary>
        private bool isReady = false;

        /// <summary>
        /// Count of XR MagicLeap gestures subsystem start calls.
        /// </summary>
        private uint gestureSubsystemStartCount = 0;

        /// <summary>
        /// Platform API level.
        /// </summary>
        private uint? platformLevel = null;

        /// <summary>
        /// List of callbacks to trigger at the end of the frame.
        /// </summary>
        private List<Action> endOfFrameCallbacks = new List<Action>();

        /// <summary>
        /// <c>Coroutine</c> used to get the end of frame.
        /// </summary>
        private Coroutine endOfFrameCoroutine = null;

        /// <summary>
        /// Delegate to handle Update calls.
        /// </summary>
        public delegate void OnUpdateActionsDelegate();

        /// <summary>
        /// Delegate to handle application pause events.
        /// </summary>
        /// <param name="paused">Whether app is paused or resumed.</param>
        public delegate void OnPauseEventDelegate(bool paused);

        /// <summary>
        /// Event triggered on application update.
        /// </summary>
        private event OnUpdateActionsDelegate OnUpdateActions = delegate { };

        /// <summary>
        /// Event triggered on application pause and resume.
        /// </summary>
        private event OnPauseEventDelegate OnPauseEvent = delegate { };

        /// <summary>
        /// Gets the last scale assigned from the main camera's parent
        /// </summary>
        public static float WorldScale
        {
            get
            {
                if (!worldScaleInitialized)
                {
                    UpdateWorldScale();
                    worldScaleInitialized = true;
                }

                return worldScale;
            }
        }

        /// <summary>
        /// Gets the platform API level that the OS supports.
        /// </summary>
        public static uint PlatformLevel
        {
            get
            {
                if (Instance.platformLevel == null)
                {
                    Instance.GetPlatformLevel();
                }

                return Instance.platformLevel.Value;
            }
        }

        /// <summary>
        /// Gets the active XR MagicLeap gestures subsystem.
        /// </summary>
        public static MagicLeapGestures GestureSubsystem
        {
            get
            {
                if (instance == null)
                {
                    return null;
                }

                return instance.gestureSubsystem;
            }
        }

        /// <summary>
        /// Gets the MLDevice singleton instance.
        /// </summary>
        private static MLDevice Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<MLDevice>();
                    instance.name = "(MLDevice Singleton) ";
                    #if DEBUG
                    MLPluginLog.Debug("Creating MLDevice");
                    #endif
                    UnityEngine.Object.DontDestroyOnLoad(go);
                }

                return instance;
            }
        }

        /// <summary>
        /// Check if the underlying Unity XR MagicLeap subsystem is initialized.
        /// </summary>
        /// <returns>Value indicating whether the XR MagicLeap subsystem is initialized.</returns>
        public static bool IsReady()
        {
            return UnityEngine.XR.XRSettings.enabled && (UnityEngine.XR.Management.XRGeneralSettings.Instance?.Manager?.ActiveLoaderAs<MagicLeapLoader>() != null);
        }

        /// <summary>
        /// Register a MagicLeap API Update callback to be called on Update of this behavior.
        /// </summary>
        /// <param name="callback">Callback to register.</param>
        public static void Register(OnUpdateActionsDelegate callback)
        {
            Instance.OnUpdateActions += callback;
        }

        /// <summary>
        /// Register a MagicLeap API application pause callback to be called OnApplicationPause of this behavior.
        /// </summary>
        /// <param name="callback">Callback to register.</param>
        public static void RegisterOnApplicationPause(OnPauseEventDelegate callback)
        {
            Instance.OnPauseEvent += callback;
        }

        /// <summary>
        /// Unregister a previously registered MagicLeap API Update callback.
        /// </summary>
        /// <param name="callback">Callback to unregister.</param>
        public static void Unregister(OnUpdateActionsDelegate callback)
        {
            // Check instance instead of the Instance property to prevent
            // creating an instance to unregister something that won't be there.
            if (instance != null)
            {
                instance.OnUpdateActions -= callback;
            }
        }

        /// <summary>
        /// Unregister a previously registered MagicLeap API application pause callback.
        /// </summary>
        /// <param name="callback">Callback to unregister.</param>
        public static void UnregisterOnApplicationPause(OnPauseEventDelegate callback)
        {
            if (instance != null)
            {
                instance.OnPauseEvent -= callback;
            }
        }

        /// <summary>
        /// Apply the world scale set on the current main camera's parent.
        /// </summary>
        /// <returns>True if the scale was set false if the main camera or main camera parent was not found.</returns>
        public static bool UpdateWorldScale()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.transform.parent != null)
            {
                Vector3 cameraLossyScale = mainCamera.transform.parent.lossyScale;
                worldScale = (cameraLossyScale.x + cameraLossyScale.y + cameraLossyScale.z) / 3.0f;
                return true;
            }

            worldScale = 1.0f;
            return false;
        }

        /// <summary>
        /// Attempts to register the Unity GestureSubsystem.
        /// </summary>
        public static void RegisterGestureSubsystem()
        {
            // Instance is used on register, to ensure we have a valid instance.
            Instance.gestureSubsystemStartCount++;

            if (instance != null && instance.gestureSubsystem == null)
            {
                // Attempt to find or add the MagicLeapGestures component.
                instance.gestureSubsystem = instance.gameObject.GetComponent<MagicLeapGestures>();
                if (instance.gestureSubsystem == null)
                {
                    instance.gestureSubsystem = instance.gameObject.AddComponent<MagicLeapGestures>();
                }
            }
        }

        /// <summary>
        /// Attempts to unregister the GestureSubsystem.
        /// </summary>
        public static void UnregisterGestureSubsystem()
        {
            if (instance == null)
            {
                return;
            }

            if (instance.gestureSubsystemStartCount > 0)
            {
                instance.gestureSubsystemStartCount--;
            }

            // Only destroy the GameObject, when all instances have been removed.
            if (instance.gestureSubsystemStartCount == 0 && instance.gestureSubsystem != null)
            {
                // Remove the Gesture Subsystem component.
                GameObject.Destroy(instance.gestureSubsystem);
            }
        }

        /// <summary>
        /// Register a function to be executed per frame at the end of every frame, after all cameras and GUI is rendered
        /// but before displaying the frame on screen
        /// </summary>
        /// <param name="endOfFrameFunction">The function.</param>
        public static void RegisterEndOfFrameUpdate(Action endOfFrameFunction)
        {
            if (endOfFrameFunction != null)
            {
                Instance.endOfFrameCallbacks.Add(endOfFrameFunction);
            }
        }

        /// <summary>
        /// Unregister a function to no longer be executed at the end of the frame
        /// </summary>
        /// <param name="endOfFrameFunction">The function.</param>
        public static void UnregisterEndOfFrameUpdate(Action endOfFrameFunction)
        {
            if (instance != null && endOfFrameFunction != null)
            {
                instance.endOfFrameCallbacks.Remove(endOfFrameFunction);
            }
        }

        /// <summary>
        /// Initializes the Magic Leap device state.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            this.SetPoseSource(DeviceType.GenericXRDevice, TrackedPose.Head);
            this.trackingType = TrackingType.RotationAndPosition;
            this.updateType = UpdateType.Update;
        }

        /// <summary>
        /// Subscribes to Unity events necessary for the device's operation.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            SceneManager.sceneLoaded += this.CheckMainCameraTransform;
        }

        /// <summary>
        /// Cleans up event subscriptions that are no longer necessary.
        /// </summary>
        protected override void OnDisable()
        {
            SceneManager.sceneLoaded -= this.CheckMainCameraTransform;
            base.OnDisable();
        }

        /// <summary>
        /// Cleans up any resources the object has open.
        /// </summary>
        protected override void OnDestroy()
        {
            if (this.endOfFrameCoroutine != null)
            {
                this.StopCoroutine(this.endOfFrameCoroutine);
            }
        }

        /// <summary>
        /// Calls OnUpdateActions event and dispatches all queued callbacks.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            if (!this.isReady && !(this.isReady = MLDevice.IsReady()))
            {
                return;
            }

            this.OnUpdateActions?.Invoke();

            MLThreadDispatch.DispatchAll();
        }

        /// <summary>
        /// Starts the EndOfFrameUpdate coroutine.
        /// </summary>
        private void Start()
        {
            this.endOfFrameCoroutine = this.StartCoroutine(this.EndOfFrameUpdate());
        }

        /// <summary>
        /// Callback sent to all game objects when the player pauses.
        /// </summary>
        /// <param name="pauseStatus">The pause state of the application.</param>
        private void OnApplicationPause(bool pauseStatus)
        {
            this.OnPauseEvent?.Invoke(pauseStatus);
        }

        /// <summary>
        /// Ensures the camera transform is at the origin.
        /// </summary>
        /// <param name="scene">Current scene.</param>
        /// <param name="mode">Scene loading mode.</param>
        private void CheckMainCameraTransform(Scene scene, LoadSceneMode mode)
        {
            Camera mainCamera = Camera.main;

            // Return if there is no camera or if we are loading an additive scene.
            if (mainCamera == null || mode == LoadSceneMode.Additive)
            {
                return;
            }

            Transform cameraTransform = mainCamera.transform;

            if (cameraTransform.localPosition != Vector3.zero ||
                cameraTransform.localRotation != Quaternion.identity ||
                cameraTransform.localScale != Vector3.one)
            {
                MLPluginLog.WarningFormat(
                                       "The main camera's transform is not set to identity in scene \"{0}\": " +
                                       "position : {1}, rotation : {2}, scale : {3}. " +
                                       "Those changes can cause undesired effects.",
                                       scene.name,
                                       cameraTransform.localPosition,
                                       cameraTransform.localRotation.eulerAngles,
                                       cameraTransform.localScale);
            }
        }

        /// <summary>
        /// Retrieves the platform API level that the OS supports.
        /// </summary>
        private void GetPlatformLevel()
        {
            try
            {
                uint level = 0;
                MLResult.Code resultCode = MagicLeapNativeBindings.MLPlatformGetAPILevel(ref level);
                if (resultCode != MLResult.Code.Ok)
                {
                    MLPluginLog.ErrorFormat("MLDevice.GetPlatformLevel failed to get platform level. Reason: {0}", resultCode);
                }
                else
                {
                    this.platformLevel = level;
                }
            }
            catch (DllNotFoundException)
            {
                MLPluginLog.ErrorFormat("MLDevice.GetPlatformLevel failed. Reason: {0} library not found", MagicLeapNativeBindings.MLPlatformDll);
            }

            if (this.platformLevel == null)
            {
                this.platformLevel = 0;
            }
        }

        /// <summary>
        /// Update function that gets called at the end of frame.
        /// </summary>
        /// <returns>End of frame <c>IEnumetaor</c></returns>
        private IEnumerator EndOfFrameUpdate()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                foreach (Action callback in this.endOfFrameCallbacks)
                {
                    callback?.Invoke();
                }
            }
        }
    }
}

#endif
