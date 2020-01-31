// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;
using System.Collections.Generic;
using System;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// MLImageTrackerBehavior encapsulates the functionality to track images
    /// Avoid making references to this behavior from OnApplicationPause
    /// since the underlying system will likely be disabled.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/MLImageTrackerBehavior")]
    public class MLImageTrackerBehavior : MonoBehaviour
    {
        #region Public Variables
        /// <summary>
        /// Image that needs to be tracked.
        /// Do not resize the image, the aspect ratio of the image provided here
        /// and the printed image should be the same. Set the "Non Power of 2"
        /// property of Texture2D to none.
        /// </summary>
        [Tooltip("Texture2D  of image that needs to be tracked. Do not change the aspect ratio of the image, it should be the same as the printed image. Set the \"Non Power of 2\" property of Texture2D to \"none\".")]
        public Texture2D Image;

        /// <summary>
        /// Set this to true if the position of this image target in the physical
        /// world is fixed and its surroundings are planar (ex: walls, floors, tables, etc).
        /// </summary>
        [Tooltip("Set this to true if the position of this image target in the physical world is fixed and its surroundings are planar (ex: walls, floors, tables, etc).")]
        public bool IsStationary;

        /// <summary>
        /// Set this to true if the behavior should automatically move the attached game object.
        /// </summary>
        [Tooltip("Set this to true if the behavior should automatically move the attached game object.")]
        public bool AutoUpdate;

        /// <summary>
        /// Longer dimension of the printed image target in scene units.
        /// If width is greater than height, it is the width, height otherwise.
        /// </summary>
        [Tooltip("Longer dimension of the printed image target in scene units. If width is greater than height, it is the width, height otherwise.")]
        public float LongerDimensionInSceneUnits;
        #endregion

        #region Public Properties
        /// <summary>
        /// Whether or not this object is currently being tracked
        /// </summary>
        public bool IsTracking
        {
            get
            {
                return (_trackerResult.Status == MLImageTargetTrackingStatus.Tracked || _trackerResult.Status == MLImageTargetTrackingStatus.Unreliable);
            }
        }

        /// <summary>
        /// The current status of the tracking state
        /// </summary>
        public MLImageTargetTrackingStatus TrackingStatus
        {
            get
            {
                return _trackerResult.Status;
            }
        }
        #endregion

        #region Public Events
        /// <summary>
        /// Occurs when an existing image target is found. Bool value indicates
        /// whether the tracking result is good or unreliable.
        /// </summary>
        public event Action<bool> OnTargetFound;

        /// <summary>
        /// Occurs when the image target is lost.
        /// </summary>
        public event Action OnTargetLost;

        /// <summary>
        /// Occurs when the result gets updated for the image target and happens
        /// once every frame.  This provides the target position, orientation, and
        /// tracking status.
        /// </summary>
        public event Action<MLImageTargetResult> OnTargetUpdated;
        #endregion

        #region Private Variables
        private MLImageTarget _imageTarget;
        private MLImageTargetResult _trackerResult;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Start the image tracker and the image target to the tracking system.
        /// </summary>
        private void Awake()
        {
            MLResult result = MLImageTracker.Start();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLImageTrackerBehavior failed to start image tracker. Reason: {0}", result);
                return;
            }

            _imageTarget = MLImageTracker.AddTarget(gameObject.GetInstanceID().ToString(), Image, LongerDimensionInSceneUnits, HandleTargetResult, IsStationary);
            if (_imageTarget == null)
            {
                Debug.LogErrorFormat("MLImageTrackerBehavior failed to add target {0} to the image tracker.", gameObject.name);
            }

            _trackerResult.Status = MLImageTargetTrackingStatus.NotTracked;
        }

        private void OnDestroy()
        {
            MLImageTracker.RemoveTarget(gameObject.GetInstanceID().ToString());
            MLImageTracker.Stop();
        }
        #endregion

        #region Private Methods
        private void UpdateTransform(MLImageTargetResult newResult)
        {
            transform.position = newResult.Position;
            transform.rotation = newResult.Rotation;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the longer dimension of the Image Target.
        /// This should not be called before the image target is added to the tracker system.
        /// </summary>
        /// <param name="longerDimension">longer dimension of the image target in scene units.</param>
        /// <returns> true if the dimension was successfully fetched, false otherwise.</returns>
        public bool GetTargetLongerDimension(out float longerDimension)
        {
            if (_imageTarget == null)
            {
                Debug.LogError("MLImageTrackerBehavior failed to get the longer dimension of the image target. Reason: Invalid image target.");
                longerDimension = 0;
                return false;
            }
            longerDimension = _imageTarget.GetTargetLongerDimension();
            return true;
        }

        /// <summary>
        /// Set the longer dimension of the Image Target.
        /// This method can be used to change the dimension of the image target at runtime.
        /// This should not be called before the image target is added to the tracker system.
        /// </summary>
        /// <param name="longerDimension">longer dimension of the image target in scene units.</param>
        /// <returns/>
        public MLResult SetTargetLongerDimension(float longerDimension)
        {
            MLResult result;
            if (_imageTarget == null)
            {
                result = new MLResult(MLResultCode.InvalidParam, "Invalid image target");
                Debug.LogErrorFormat("MLImageTrackerBehavior failed to set the longer dimension of the image target. Reason: {0}", result);
                return result;
            }

            result = _imageTarget.SetTargetLongerDimension(longerDimension);
            if (result.IsOk)
            {
                LongerDimensionInSceneUnits = longerDimension;
            }
            return result;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handle the image target result callback. This is called every frame.
        /// </summary>
        private void HandleTargetResult(MLImageTarget imageTarget, MLImageTargetResult newResult)
        {
            if (AutoUpdate)
            {
                UpdateTransform(newResult);
            }

            HandleCallbacks(newResult);
        }

        private void HandleCallbacks(MLImageTargetResult newResult)
        {
            if (newResult.Status != _trackerResult.Status)
            {
                if (newResult.Status == MLImageTargetTrackingStatus.Tracked || newResult.Status == MLImageTargetTrackingStatus.Unreliable)
                {
                    if (OnTargetFound != null)
                    {
                        OnTargetFound(newResult.Status == MLImageTargetTrackingStatus.Tracked);
                    }
                }
                else
                {
                    if (OnTargetLost != null)
                    {
                        OnTargetLost();
                    }
                }
                _trackerResult = newResult;
            }

            // This additional update is for someone wants to listen to ImageTarget updates.
            if (OnTargetUpdated != null)
            {
                OnTargetUpdated(newResult);
            }
        }
        #endregion
    }
}
