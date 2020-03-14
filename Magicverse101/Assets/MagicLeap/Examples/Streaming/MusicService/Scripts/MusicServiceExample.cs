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
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using System;
using System.Collections;

namespace MagicLeap
{
    public class MusicServiceExample : MonoBehaviour
    {
        private const float CANVAS_DISTANCE_FORWARD = 1.8f;
        private const float CANVAS_DISTANCE_DOWNWARD = 0.3f;

        [SerializeField, Tooltip("Reference to the MLMusicServiceBehavior component.")]
        private MLMusicServiceBehavior musicService = null;

        [SerializeField, Tooltip("MusicServiceVisualizer object transform.")]
        private Transform musicServiceVisualizer = null;

        [SerializeField, Space, Tooltip("MLControllerConnectionHandlerBehavior reference.")]
        private MLControllerConnectionHandlerBehavior _controllerConnectionHandler = null;

        [SerializeField, Tooltip("The text to place runtime data on.")]
        private Text statusText = null;

        /// <summary>
        /// Check required variables, register callbacks and setup player in front of the camera.
        /// </summary>
        void Start()
        {
            if (musicService == null)
            {
                Debug.LogError("Error: MusicServiceExample.musicService is not set, disabling script.");
                enabled = false;
                return;
            }

            if (musicServiceVisualizer == null)
            {
                Debug.LogError("Error: MusicServiceExample.musicServiceVisualizer is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_controllerConnectionHandler == null)
            {
                Debug.LogError("Error: MusicServiceExample._controllerConnectionHandler is not set, disabling script.");
                enabled = false;
                return;
            }

            if (statusText == null)
            {
                Debug.LogError("Error: MusicServiceExample.statusText is not set, disabling script.");
                enabled = false;
                return;
            }

            #if PLATFORM_LUMIN
            MLInput.OnControllerButtonDown += HandleOnButtonDown;
            musicService.OnError += HandleError;
            #endif

            StartCoroutine("PlaceMusicServiceVisualizer");
        }

        /// <summary>
        /// Update the UI status text with the latest data.
        /// </summary>
        void Update()
        {
            statusText.text = string.Format("<color=#dbfb76><b>{0}</b></color>\n{1}: {2}\n",
                LocalizeManager.GetString("ControllerData"),
                LocalizeManager.GetString("Status"),
                LocalizeManager.GetString(ControllerStatus.Text));

            #if PLATFORM_LUMIN
            MLMusicService.Metadata metaData = musicService.CurrentTrackMetadata;
            statusText.text += string.Format("\n<color=#dbfb76><b>{0}</b></color>\n{1}: {2}\n{3}: {4}\n{5}: {6}\n{7}: {8}\n{9}: {10}\n{11}: {12}\n{13}: {14}\n",
                LocalizeManager.GetString("MusicServiceData"),
                LocalizeManager.GetString("Status"),
                LocalizeManager.GetString("Ok"),
                LocalizeManager.GetString("TrackTitle"),
                metaData.TrackTitle,
                LocalizeManager.GetString("AlbumName"),
                metaData.AlbumInfoName,
                LocalizeManager.GetString("AlbumURL"),
                metaData.AlbumInfoUrl,
                LocalizeManager.GetString("AlbumCoverURL"),
                metaData.AlbumInfoCoverUrl,
                LocalizeManager.GetString("ArtistName"),
                metaData.ArtistInfoName,
                LocalizeManager.GetString("ArtistURL"),
                metaData.ArtistInfoUrl);
            #endif
        }

        /// <summary>
        /// Unregister music service callbacks.
        /// </summary>
        void OnDestroy()
        {
            #if PLATFORM_LUMIN
            musicService.OnError -= HandleError;
            MLInput.OnControllerButtonDown -= HandleOnButtonDown;
            #endif
        }

        /// <summary>
        /// Give time for the Camera to start and setup the visualizer in front of the camera.
        /// </summary>
        private IEnumerator PlaceMusicServiceVisualizer()
        {
            yield return new WaitForSeconds(1);
            PlaceMusicServiceVisualizerFromCamera();
        }

        /// <summary>
        /// Sets the music service visualizer to the correct position and orientation.
        /// </summary>
        private void PlaceMusicServiceVisualizerFromCamera()
        {
            Camera mainCamera = Camera.main;
            musicServiceVisualizer.position = mainCamera.transform.position + mainCamera.transform.forward * CANVAS_DISTANCE_FORWARD - mainCamera.transform.up * CANVAS_DISTANCE_DOWNWARD;
            musicServiceVisualizer.rotation = Quaternion.LookRotation(musicServiceVisualizer.position - mainCamera.transform.position);
        }

        /// <summary>
        /// Handles the event for button down.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void HandleOnButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            if (_controllerConnectionHandler.IsControllerValid(controllerId) && button == MLInput.Controller.Button.Bumper)
            {
                PlaceMusicServiceVisualizerFromCamera();
            }
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Handles an error from the MLMusicService.
        /// </summary>
        /// <param name="error">The error that ocurred.</param>
        private void HandleError(MLMusicService.Error error)
        {
            statusText.text = string.Format("<color=#dbfb76><b>{0}</b></color>\n{1}: {2}\n",
                LocalizeManager.GetString("ControllerData"),
                LocalizeManager.GetString("Status"),
                LocalizeManager.GetString(ControllerStatus.Text));

            MLMusicService.Metadata metaData = musicService.CurrentTrackMetadata;
            statusText.text += string.Format("<color=#dbfb76><b>{0}</b></color>\n{1}: {2} - {3}: {4}, {5}: {6}",
                LocalizeManager.GetString("MusicServiceData"),
                LocalizeManager.GetString("Status"),
                LocalizeManager.GetString("Error"),
                LocalizeManager.GetString("Type"),
                error.Type,
                LocalizeManager.GetString("Code"),
                error.Code);

            enabled = false;
            return;
        }
        #endif
    }
}
