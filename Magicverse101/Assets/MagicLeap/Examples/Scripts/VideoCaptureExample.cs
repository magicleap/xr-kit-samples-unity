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

using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.UI;
using System;
using System.Text;

namespace MagicLeap
{
    /// <summary>
    /// This class handles video recording and loading based on controller
    /// input.
    /// </summary>
    public class VideoCaptureExample : MonoBehaviour
    {
        [SerializeField, Tooltip("The maximum amount of time the camera can be recording for (in seconds.)")]
        private float _maxRecordingTime = 10.0f;

        [SerializeField, Tooltip("The text used to display status information for the example.")]
        private Text _statusText = null;

        [Space, SerializeField, Tooltip("MLControllerConnectionHandlerBehavior reference.")]
        private MLControllerConnectionHandlerBehavior _controllerConnectionHandler = null;

        [SerializeField, Tooltip("Refrence to the Privilege requester Prefab")]
        private MLPrivilegeRequesterBehavior _privilegeRequester = null;

        [SerializeField, Tooltip("Refrence to the Video Capture Visualizer gameobject")]
        private VideoCaptureVisualizer _videoCaptureVisualizer = null;

        [SerializeField, Tooltip("Refrence to the Raw Video Capture Visualizer gameobject")]
        private RawVideoCaptureVisualizer _rawVideoCaptureVisualizer = null;

        private const string _validFileFormat = ".mp4";

        private string _intrinsicValuesText = null;

        private const float _minRecordingTime = 1.0f;

        // Is the camera currently recording
        private bool _isCapturing = false;

        // The file path to the active capture
        private string _captureFilePath;

        private bool _isCameraConnected = false;

        private float _captureStartTime = 0.0f;

        private bool _hasStarted = false;

        #pragma warning disable 414
        private bool _appPaused = false;
        #pragma warning restore 414

        #if PLATFORM_LUMIN
        private event Action<MLCamera.ResultExtras, MLCamera.YUVFrameInfo, MLCamera.FrameMetadata> OnRawVideoDataReceived = null;
        #endif

        #pragma warning disable 414
        private event Action OnVideoCaptureStarted = null;

        private event Action<string> OnVideoCaptureEnded = null;

        private event Action OnRawVideoCaptureStarted = null;

        private event Action OnRawVideoCaptureEnded = null;
        #pragma warning restore 414

        private bool _rawVideoCaptureMode = false;

        /// <summary>
        /// Validate that _maxRecordingTime is not less than minimum possible.
        /// </summary>
        void OnValidate()
        {
            if (_maxRecordingTime < _minRecordingTime)
            {
                Debug.LogWarning(string.Format("You can not have a MaxRecordingTime less than {0}, setting back to minimum allowed!", _minRecordingTime));
                _maxRecordingTime = _minRecordingTime;
            }
        }

        // Using Awake so that Privileges is set before MLPrivilegeRequesterBehavior Start
        void Awake()
        {
            if (_controllerConnectionHandler == null)
            {
                Debug.LogError("Error: VideoCamptureExample._controllerConnectionHandler is not set, disabling script.");
                enabled = false;
                return;
            }

            if(_privilegeRequester == null)
            {
                Debug.LogError("Error: VideoCaptureExample._privilegeRequester is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_statusText == null)
            {
                Debug.LogError("Error: VideoCaptureExample._statusText is not set, disabling script.");
                enabled = false;
                return;
            }

            #if PLATFORM_LUMIN
            // Before enabling the Camera, the scene must wait until the privileges have been granted.
            _privilegeRequester.OnPrivilegesDone += HandlePrivilegesDone;
            #endif

            OnVideoCaptureStarted += _videoCaptureVisualizer.OnCaptureStarted;
            OnVideoCaptureEnded += _videoCaptureVisualizer.OnCaptureEnded;
            OnRawVideoCaptureStarted += _rawVideoCaptureVisualizer.OnCaptureStarted;
            OnRawVideoCaptureEnded += _rawVideoCaptureVisualizer.OnCaptureEnded;

            #if PLATFORM_LUMIN
            OnRawVideoDataReceived += _rawVideoCaptureVisualizer.OnRawCaptureDataReceived;
            #endif
        }

        void Update()
        {
           if (_isCapturing)
           {
                // If the recording has gone longer than the max time
                if (Time.time - _captureStartTime > _maxRecordingTime)
                {
                    EndCapture();
                }
            }

            UpdateStatusText();
        }

        /// <summary>
        /// Stop the camera, unregister callbacks, and stop input and privileges APIs.
        /// </summary>
        void OnDisable()
        {
            #if PLATFORM_LUMIN
            MLInput.OnControllerButtonDown -= OnButtonDown;
            MLInput.OnTriggerDown -= OnTriggerDown;
            #endif

            if (_isCameraConnected)
            {
                DisableMLCamera();
            }
        }

        /// <summary>
        /// Cannot make the assumption that a privilege is still granted after
        /// returning from pause. Return the application to the state where it
        /// requests privileges needed and clear out the list of already granted
        /// privileges. Also, disable the camera and unregister callbacks.
        /// </summary>
        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                _appPaused = true;

                #if PLATFORM_LUMIN
                if (_isCameraConnected && MLCamera.IsStarted)
                {
                    MLResult result = MLCamera.ApplicationPause(_appPaused);
                    if(!result.IsOk)
                    {
                        Debug.LogErrorFormat("Error: VideoCaptureExample failed to pause MLCamera, disabling script. Reason: {0}", result);
                        enabled = false;
                        return;
                    }

                    // If we did not record long enough make sure our path is marked as invalid to avoid trying to load invalid file.
                    if (Time.time - _captureStartTime < _minRecordingTime)
                    {
                        _captureFilePath = null;
                    }

                    if (_isCapturing)
                    {
                        if (_rawVideoCaptureMode)
                        {
                            OnRawVideoCaptureEnded.Invoke();
                        }
                        else
                        {
                            OnVideoCaptureEnded.Invoke(_captureFilePath);
                        }
                    }

                    _isCapturing = false;
                    _captureStartTime = 0;
                    _captureFilePath = null;
                    _isCameraConnected = false;
                }

                MLInput.OnControllerButtonDown -= OnButtonDown;
                MLInput.OnTriggerDown -= OnTriggerDown;
                #endif
            }
        }

        void OnDestroy()
        {
            if (_privilegeRequester != null)
            {
                #if PLATFORM_LUMIN
                _privilegeRequester.OnPrivilegesDone -= HandlePrivilegesDone;
                #endif
            }

            OnVideoCaptureStarted -= _videoCaptureVisualizer.OnCaptureStarted;
            OnVideoCaptureEnded -= _videoCaptureVisualizer.OnCaptureEnded;
            OnRawVideoCaptureStarted -= _rawVideoCaptureVisualizer.OnCaptureStarted;
            OnRawVideoCaptureEnded -= _rawVideoCaptureVisualizer.OnCaptureEnded;

            #if PLATFORM_LUMIN
            OnRawVideoDataReceived -= _rawVideoCaptureVisualizer.OnRawCaptureDataReceived;
            #endif
        }

        /// <summary>
        /// Start capturing video.
        /// </summary>
        public void StartCapture()
        {
            if(_rawVideoCaptureMode)
            {
                StartRawCapture();
                return;
            }

            string fileName = System.DateTime.Now.ToString("MM_dd_yyyy__HH_mm_ss") + _validFileFormat;
            StartCapture(fileName);
        }

        /// <summary>
        /// Start capturing video to input filename.
        /// </summary>
        /// <param name="fileName">File path to write the video to.</param>
        public void StartCapture(string fileName)
        {
            #if PLATFORM_LUMIN
            if(!_isCapturing && MLCamera.IsStarted && _isCameraConnected)
            {
                // Check file fileName extensions
                string extension = System.IO.Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(extension) || !extension.Equals(_validFileFormat, System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogErrorFormat("Invalid fileName extension '{0}' passed into Capture({1}).\n" +
                        "Videos must be saved in {2} format.", extension, fileName, _validFileFormat);
                    return;
                }

                string pathName = System.IO.Path.Combine(Application.persistentDataPath, fileName);

                MLResult result = MLCamera.StartVideoCapture(pathName);
                if (result.IsOk)
                {
                    _isCapturing = true;
                    _captureStartTime = Time.time;
                    _captureFilePath = pathName;
                    OnVideoCaptureStarted.Invoke();
                }
                else
                {
                    Debug.LogErrorFormat("Error: VideoCaptureExample failed to start video capture for {0}. Reason: {1}", fileName, MLCamera.GetErrorCode().ToString());
                }
            }
            else
            {
                Debug.LogErrorFormat("Error: VideoCaptureExample failed to start video capture for {0} because '{1}' is already recording!",
                    fileName, _captureFilePath);
            }
            #endif
        }

        /// <summary>
        /// Start capturing raw video.
        /// </summary>
        public void StartRawCapture()
        {
            #if PLATFORM_LUMIN
            if (!_isCapturing && MLCamera.IsStarted && _isCameraConnected)
            {
                MLResult result = MLCamera.StartRawVideoCapture();
                if (result.IsOk)
                {
                    _isCapturing = true;
                    _captureStartTime = Time.time;
                    OnRawVideoCaptureStarted.Invoke();
                    SetupCameraIntrinsics();
                }
                else
                {
                    Debug.LogErrorFormat("Error: VideoCaptureExample failed to start raw video capture. Reason: {1}", MLCamera.GetErrorCode().ToString());
                }
            }
            else
            {
                Debug.LogErrorFormat("Error: VideoCaptureExample failed to start raw video capture.");
            }
            #endif
        }

        /// <summary>
        /// Stop capturing video.
        /// </summary>
        public void EndCapture()
        {
            if (_isCapturing)
            {
                #if PLATFORM_LUMIN
                MLResult result = MLCamera.StopVideoCapture();
                if (result.IsOk)
                {
                    // If we did not record long enough make sure our path is marked as invalid to avoid trying to load invalid file.
                    if (Time.time - _captureStartTime < _minRecordingTime)
                    {
                        _captureFilePath = null;
                    }

                    if (_rawVideoCaptureMode)
                    {
                        OnRawVideoCaptureEnded.Invoke();
                    }
                    else
                    {
                        OnVideoCaptureEnded.Invoke(_captureFilePath);
                    }

                    _isCapturing = false;
                    _captureStartTime = 0;
                    _captureFilePath = null;
                }
                else
                {
                    Debug.LogErrorFormat("Error: VideoCaptureExample failed to end video capture. Error Code: {0}", MLCamera.GetErrorCode().ToString());
                }
                #endif
            }
            else
            {
                Debug.LogError("Error: VideoCaptureExample failed to end video capture because the camera is not recording.");
            }
        }

           /// <summary>
        /// Update Status Tabin Ui.
        /// </summary>
        private void UpdateStatusText()
        {
            _statusText.text = string.Format("<color=#dbfb76><b>{0} </b></color>\n{1}: {2}\n",
                 LocalizeManager.GetString("ControllerData"),
                 LocalizeManager.GetString("Status"),
                 LocalizeManager.GetString(ControllerStatus.Text));

            _statusText.text += string.Format("\n<color=#dbfb76><b>{0}</b></color>:\n", LocalizeManager.GetString("VideoData"));

            if (_rawVideoCaptureMode)
            {
                _statusText.text += string.Format("{0}: {1}\n",
                    LocalizeManager.GetString("Mode"),
                    LocalizeManager.GetString("RawVideoCapture"));

                _statusText.text += _intrinsicValuesText;
            }
            else
            {
                _statusText.text += string.Format("{0}: {1}\n",
                    LocalizeManager.GetString("Mode"),
                    LocalizeManager.GetString("VideoCapture"));
            }
        }

        /// <summary>
        /// Connects the MLCamera component and instantiates a new instance
        /// if it was never created.
        /// </summary>
        private void EnableMLCamera()
        {
            #if PLATFORM_LUMIN
            MLResult result = MLCamera.Start();
            if (result.IsOk)
            {
                result = MLCamera.Connect();
                if(_rawVideoCaptureMode)
                {
                    MLCamera.OnRawVideoFrameAvailableYUV += OnRawCaptureDataReceived;
                }
                else
                {
                    _rawVideoCaptureVisualizer.OnRawCaptureEnded();
                }
                _isCameraConnected = true;
            }
            else
            {
                Debug.LogErrorFormat("Error: VideoCaptureExample failed starting MLCamera, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }
            #endif
        }

        /// <summary>
        /// Disconnects the MLCamera if it was ever created or connected.
        /// Also stops any video recording if active.
        /// </summary>
        private void DisableMLCamera()
        {
            #if PLATFORM_LUMIN
            if (MLCamera.IsStarted)
            {
                if (_isCapturing)
                {
                    EndCapture();
                }
                MLCamera.Disconnect();
                _isCameraConnected = false;
                MLCamera.Stop();

                if(_rawVideoCaptureMode)
                {
                    MLCamera.OnRawVideoFrameAvailableYUV -= OnRawCaptureDataReceived;
                }
            }
            #endif
        }

        /// <summary>
        /// Enable the camera and callbacks. Called once privileges have been granted.
        /// </summary>
        private void EnableCapture()
        {
            if (!_hasStarted)
            {
                EnableMLCamera();
                #if PLATFORM_LUMIN
                MLInput.OnControllerButtonDown += OnButtonDown;
                MLInput.OnTriggerDown += OnTriggerDown;
                #endif
                _hasStarted = true;
            }
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Responds to privilege requester result.
        /// </summary>
        /// <param name="result"/>
        private void HandlePrivilegesDone(MLResult result)
        {
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: VideoCaptureExample failed to get all requested privileges, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }

            Debug.Log("Succeeded in requesting all privileges");

            // Called here because it needs privileges to be granted first on resume by MLPrivilegeRequesterBehavior.
            if (_appPaused)
            {
                _appPaused = false;

                result = MLCamera.ApplicationPause(_appPaused);
                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("Error: VideoCaptureExample failed to resume MLCamera, disabling script. Reason: {0}", result);
                    enabled = false;
                    return;
                }

                _isCameraConnected = true;

                MLInput.OnControllerButtonDown += OnButtonDown;
                MLInput.OnTriggerDown += OnTriggerDown;
            }
            else
            {
                EnableCapture();
            }
        }

        /// <summary>
        /// Handles the event for raw capture data recieved, and forwards it to any listeners.
        /// </summary>
        /// <param name="extras">Contains timestamp to use with GetFramePose, also forwarded to listeners.</param>
        /// <param name="frameData">Forwarded to listeners.</param>
        /// <param name="frameMetadata">Forwarded to listeners.</param>
        private void OnRawCaptureDataReceived(MLCamera.ResultExtras extras, MLCamera.YUVFrameInfo frameData, MLCamera.FrameMetadata frameMetadata)
        {
            OnRawVideoDataReceived?.Invoke(extras, frameData, frameMetadata);
        }
        #endif

         /// <summary>
        /// Handles the event for button down. Starts or stops recording.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            if (_controllerConnectionHandler.IsControllerValid(controllerId) && MLInput.Controller.Button.Bumper == button)
            {
                if (!_isCapturing)
                {
                    StartCapture();
                }
                else
                {
                    EndCapture();
                }
            }
        }

        private void OnTriggerDown(byte controllerId, float value)
        {
            if (_controllerConnectionHandler.IsControllerValid(controllerId))
            {
                if (_isCapturing)
                {
                    EndCapture();
                }

                if (_rawVideoCaptureMode)
                {
                    #if PLATFORM_LUMIN
                    MLCamera.OnRawVideoFrameAvailableYUV -= OnRawCaptureDataReceived;
                    _rawVideoCaptureVisualizer.OnRawCaptureEnded();
                    #endif
                }
                else
                {
                    #if PLATFORM_LUMIN
                    MLCamera.OnRawVideoFrameAvailableYUV += OnRawCaptureDataReceived;
                    _videoCaptureVisualizer.DisablePreview();
                    #endif
                }
                _rawVideoCaptureMode = !_rawVideoCaptureMode;
            }
        }

        /// <summary>
        /// Setup the text field for camera intrinsic values.
        /// Precondition: MLCamera must be successfully started.
        /// </summary>
        void SetupCameraIntrinsics()
        {
            #if PLATFORM_LUMIN
            MLCamera.IntrinsicCalibrationParameters parameters;
            MLResult result = MLCamera.GetIntrinsicCalibrationParameters(out parameters);
            if (result.IsOk)
            {
                _intrinsicValuesText = CalibrationParametersToString(parameters);
            }
            else
            {
                Debug.LogErrorFormat("Error: VideoCaptureExample failed to GetIntrinsicCalibrationParameters. Reason: {0}", result);
            }
            #endif
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Convert camera calibration parameters to a string.
        /// </summary>
        /// <param name="parameters">The camera calibration values to pull from.</param>
        static string CalibrationParametersToString(MLCamera.IntrinsicCalibrationParameters parameters)
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("\n <color=#dbfb76><b>{0}  {1}:</b></color>", LocalizeManager.GetString("Camera"), LocalizeManager.GetString("IntrinsicValues"))
                .AppendFormat("\n   {0}: {1}", LocalizeManager.GetString("Width"), parameters.Width)
                .AppendFormat("\n   {0}: {1}", LocalizeManager.GetString("Height"), parameters.Height)
                .AppendFormat("\n   {0}: {1}", LocalizeManager.GetString("FocalLength"), parameters.FocalLength)
                .AppendFormat("\n   {0}: {1}", LocalizeManager.GetString("PrincipalPoint"), parameters.PrincipalPoint)
                .AppendFormat("\n   {0}: {1}", LocalizeManager.GetString("FOV"), parameters.FOV)
                .AppendFormat("\n   {0}:", LocalizeManager.GetString("DistortionCoeff"));
            for (int i = 0; i < parameters.Distortion.Length; ++i)
            {
                b.AppendFormat("\n   [{0}]: {1}", i, parameters.Distortion[i]);
            }
            return b.ToString();
        }
         #endif

}
}
