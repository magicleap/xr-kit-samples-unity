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
using MagicLeap.Core.StarterKit;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Core
{
    /// <summary>
    /// MLImageTrackerBehavior encapsulates the functionality to track images.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/MLImageTrackerBehavior")]
    public class MLImageTrackerBehavior : MonoBehaviour
    {
        /// <summary>
        /// Image that needs to be tracked.
        /// Do not resize the image, the aspect ratio of the image provided here
        /// and the printed image should be the same. Set the "Non Power of 2"
        /// property of Texture2D to none.
        /// </summary>
        [Tooltip("Texture2D  of image that needs to be tracked. Do not change the aspect ratio of the image, it should be the same as the printed image. Set the \"Non Power of 2\" property of Texture2D to \"none\".")]
        public Texture2D image;

        /// <summary>
        /// Set this to true if the position of this image target in the physical
        /// world is fixed and its surroundings are planar (ex: walls, floors, tables, etc).
        /// </summary>
        [Tooltip("Set this to true if the position of this image target in the physical world is fixed and its surroundings are planar (ex: walls, floors, tables, etc).")]
        public bool isStationary;

        /// <summary>
        /// Set this to true if the behavior should automatically move the attached game object.
        /// </summary>
        [Tooltip("Set this to true if the behavior should automatically move the attached game object.")]
        public bool autoUpdate;

        /// <summary>
        /// Longer dimension of the printed image target in scene units.
        /// If width is greater than height, it is the width, height otherwise.
        /// </summary>
        [Tooltip("Longer dimension of the printed image target in scene units. If width is greater than height, it is the width, height otherwise.")]
        public float longerDimensionInSceneUnits;

        /// <summary>
        /// Whether or not this object is currently being tracked.
        /// </summary>
        public bool IsTracking
        {
            get
            {
                if(_imageTarget == null)
                {
                    return false;
                }

                return (_imageTarget.Status == MLImageTracker.Target.TrackingStatus.Tracked || _imageTarget.Status == MLImageTracker.Target.TrackingStatus.Unreliable);
            }
        }

        /// <summary>
        /// The current status of the tracking state.
        /// </summary>
        public MLImageTracker.Target.TrackingStatus TrackingStatus
        {
            get
            {
                if (_imageTarget == null)
                {
                    return MLImageTracker.Target.TrackingStatus.NotTracked;
                }

                return _imageTarget.Status;
            }
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Occurs when an existing image target is found.
        /// The status of the MLImageTracker.Target.Result will indicate if tracking is unreliable.
        /// </summary>
        public event MLImageTrackerStarterKit.MLImageTargetStarterKit.StatusUpdate OnTargetFound;

        /// <summary>
        /// Occurs when the image target is lost.
        /// </summary>
        public event MLImageTrackerStarterKit.MLImageTargetStarterKit.StatusUpdate OnTargetLost;

        /// <summary>
        /// Occurs when the result gets updated for the image target and happens once every frame.
        /// </summary>
        public event MLImageTrackerStarterKit.MLImageTargetStarterKit.StatusUpdate OnTargetUpdated;
        #endif

        /// <summary>
        /// Holds reference to the image target inside MLImageTrackerStarterKit.MLImageTargetStarterKit.
        /// </summary>
        private MLImageTrackerStarterKit.MLImageTargetStarterKit _imageTarget = null;

        /// <summary>
        /// Starts the image tracker and adds the image target to the tracking system.
        /// </summary>
        void Start()
        {
            #if PLATFORM_LUMIN
            MLResult result = MLImageTrackerStarterKit.Start();
            if (result.IsOk)
            {
                AddTarget();
            }

            else
            {
                Debug.LogErrorFormat("MLImageTrackerBehavior failed on MLImageTrackerStarterKit.Start. Reason: {0}", result);
            }
            #endif
        }

        /// <summary>
        /// Cannot make the assumption that a privilege is still granted after
        /// returning from pause. Return the application to the state where it
        /// requests privileges needed.
        /// </summary>
        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                MLImageTrackerStarterKit.Stop();
            }

            #if PLATFORM_LUMIN
            else if(MLDevice.IsReady() && _imageTarget != null)
            {
                MLImageTrackerStarterKit.Start();
            }
            #endif
        }

        /// <summary>
        /// Removes the image target from the tracking system and then stops the starter kit.
        /// </summary>
        void OnDestroy()
        {
            MLImageTrackerStarterKit.RemoveTarget(gameObject.GetInstanceID().ToString());
            MLImageTrackerStarterKit.Stop();
        }

        /// <summary>
        /// Adds a new image target to be tracked.
        /// </summary>
        private void AddTarget()
        {
            #if PLATFORM_LUMIN
            _imageTarget = MLImageTrackerStarterKit.AddTarget(gameObject.GetInstanceID().ToString(), image, longerDimensionInSceneUnits, HandleAllTargetStatuses, isStationary);

            if (_imageTarget == null)
            {
                Debug.LogErrorFormat("MLImageTrackerBehavior.AddTarget failed to add target {0} to the image tracker.", gameObject.name);
                return;
            }

            _imageTarget.OnFound += (MLImageTracker.Target target, MLImageTracker.Target.Result result) => { OnTargetFound?.Invoke(target, result); };
            _imageTarget.OnLost += (MLImageTracker.Target target, MLImageTracker.Target.Result result) => { OnTargetLost?.Invoke(target, result); };
            _imageTarget.OnUpdated += (MLImageTracker.Target target, MLImageTracker.Target.Result result) => { OnTargetUpdated?.Invoke(target, result); };
            #endif
        }

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
                Debug.LogError("MLImageTrackerBehavior.GetTargetLongerDimension failed to get the longer dimension of the image target. Reason: Invalid image target.");
                longerDimension = 0;
                return false;
            }

            #if PLATFORM_LUMIN
            longerDimension = _imageTarget.Target.GetTargetLongerDimension();
            #else
            longerDimension = 0.0f;
            #endif

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

            #if PLATFORM_LUMIN
            if (_imageTarget == null)
            {
                result = MLResult.Create(MLResult.Code.InvalidParam, "Invalid image target");
                Debug.LogErrorFormat("MLImageTrackerBehavior.SetTargetLongerDimension failed to set the longer dimension of the image target. Reason: {0}", result);
                return result;
            }

            result = _imageTarget.Target.SetTargetLongerDimension(longerDimension);
            if (result.IsOk)
            {
                longerDimensionInSceneUnits = longerDimension;
            }
            #endif

            return result;
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Handles all the image target's status updates. This is called every frame.
        /// </summary>
        private void HandleAllTargetStatuses(MLImageTracker.Target imageTarget, MLImageTracker.Target.Result newResult)
        {
            if (autoUpdate)
            {
                transform.position = newResult.Position;
                transform.rotation = newResult.Rotation;
            }
        }
        #endif
    }
}
