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

using System;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Core.StarterKit
{
    /// <summary>
    /// Starter kit class for practical use of MLHandTracking.
    /// </summary>
    public static class MLHandTrackingStarterKit
    {
        /// <summary>
        /// Array of all trackable keyposes that MLHandTracking offers.
        /// </summary>
        private static MLHandTracking.HandKeyPose[] _allPoses = (MLHandTracking.HandKeyPose[])Enum.GetValues(typeof(MLHandTracking.HandKeyPose));

        /// <summary>
        /// Configured level for filtering of keypoints and hand centers.
        /// KeyPoints are like the detected joints of your fingers.
        /// </summary>
        public static MLHandTracking.KeyPointFilterLevel KeyPointFilterLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// Configured level for filtering of keyposes.
        /// key poses are the detected poses of your hand.
        /// </summary>
        public static MLHandTracking.PoseFilterLevel KeyPoseFilterLevel
        {
            get;
            private set;
        }

        #if PLATFORM_LUMIN

        public static MLHandTracking.Hand Right
        {
            get
            {
                return MLHandTracking.Right;
            }
        }

        public static MLHandTracking.Hand Left
        {
            get
            {
                return MLHandTracking.Left;
            }
        }
        #endif

        /// <summary>
        /// Starts up MLHandTracking.
        /// </summary>
        /// <param name="initializeValues">Bool that determines if MLHandTracking should automatically run with all poses and high filter levels.</param>
        public static MLResult Start(bool initializeValues = false)
        {
            #if PLATFORM_LUMIN

            MLResult _result = MLHandTracking.Start();
            if (!_result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLHandTrackingStarterKit failed starting MLHandTracking. Reason: {0}", _result);
            }

            if (initializeValues)
            {
                bool success = false;

                success = EnableKeyPoses();
                if (!success)
                {
                    MLHandTracking.Stop();
                    _result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLHandTrackingStarterKit failed to start correctly because MLHandTrackingStarterKit.EnablePoses failed because MLHandTracking.KeyPoseManager.EnableKeyPoses failed.");
                }

                success = SetKeyPointsFilterLevel(MLHandTracking.KeyPointFilterLevel.ExtraSmoothed);
                if (!success)
                {
                    MLHandTracking.Stop();
                    _result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLHandTrackingStarterKit failed to start correctly because MLHandTrackingStarterKit.SetKeyPointsFilterLevel failed because MLHandTracking.KeyPoseManager.SetKeyPointsFilterLevel failed.");
                }

                success = SetPoseFilterLevel(MLHandTracking.PoseFilterLevel.ExtraRobust);
                if (!success)
                {
                    MLHandTracking.Stop();
                    _result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLHandTrackingStarterKit failed to start correctly because MLHandTrackingStarterKit.SetPoseFilterLevel failed because MLHandTracking.KeyPoseManager.SetPoseFilterLevel failed.");
                }
            }

            return _result;
            #else
            return new MLResult();
            #endif
        }

        /// <summary>
        /// Sets the filter level for detecting hand key points.
        /// </summary>
        /// <param name="filterLevel">The filter level to set for detecting keypoints.</param>
        public static bool SetKeyPointsFilterLevel(MLHandTracking.KeyPointFilterLevel filterLevel)
        {
            bool success = false;

            #if PLATFORM_LUMIN

            if (MLHandTracking.IsStarted)
            {
                KeyPointFilterLevel = filterLevel;
                success = MLHandTracking.KeyPoseManager.SetKeyPointsFilterLevel(KeyPointFilterLevel);
                if (!success)
                {
                    Debug.LogErrorFormat("Error: MLHandTrackingStarterKit.SetKeyPointsFilterLevel failed because MLHandTracking.KeyPoseManager.SetKeyPointsFilterLevel failed.");
                }
            }

            else
            {
                Debug.LogErrorFormat("Error: MLHandTrackingStarterKit.SetKeyPointsFilterLevel failed because MLHandTracking was not started.");
            }

            #endif

            return success;
        }

        /// <summary>
        /// Sets the filter level for detecting keyposes.
        /// </summary>
        /// <param name="filterLevel">The filter level to set for detecting keyposes.</param>
        public static bool SetPoseFilterLevel(MLHandTracking.PoseFilterLevel filterLevel)
        {
            bool success = false;

            #if PLATFORM_LUMIN

            if (MLHandTracking.IsStarted)
            {
                KeyPoseFilterLevel = filterLevel;
                success = MLHandTracking.KeyPoseManager.SetPoseFilterLevel(KeyPoseFilterLevel);
                if (!success)
                {
                    Debug.LogErrorFormat("Error: MLHandTrackingStarterKit.SetPoseFilterLevel failed because MLHandTracking.KeyPoseManager.SetPoseFilterLevel failed.");
                }
            }

            else
            {
                Debug.LogErrorFormat("Error: MLHandTrackingStarterKit.SetPoseFilterLevel failed because MLHandTracking was not started.");
            }

            #endif

            return success;
        }

        /// <summary>
        /// Enables which keyposes will be looked for by MLHandTracking.
        /// <param name="exclusive">Bool that determines if all poses not included should be disabled.</param>
        /// <param name="poses">Array of poses to enable. An empty array will assume all keyposes.</param>
        /// </summary>
        public static bool EnableKeyPoses(bool exclusive = false, params MLHandTracking.HandKeyPose[] poses)
        {
            bool success = false;

            #if PLATFORM_LUMIN

            if (MLHandTracking.IsStarted)
            {
                if (poses.Length == 0)
                {
                    poses = _allPoses;
                }
                success = MLHandTracking.KeyPoseManager.EnableKeyPoses(poses, true, exclusive);
                if (!success)
                {
                    Debug.LogErrorFormat("Error: MLHandTrackingStarterKit.EnablePoses failed because MLHandTracking.KeyPoseManager.EnableKeyPoses failed.");
                }
            }

            else
            {
                Debug.LogErrorFormat("Error: MLHandTrackingStarterKit.SetPoseFilterLevel failed because MLHandTracking was not started.");
            }

            #endif

            return success;
        }


        /// <summary>
        /// Disables which keyposes will be looked for by MLHandTracking.
        /// <param name="poses">Array of poses to disable. An empty array will assume all keyposes.</param>
        /// </summary>
        public static bool DisableKeyPoses(params MLHandTracking.HandKeyPose[] poses)
        {
            bool success = false;

            #if PLATFORM_LUMIN

            if (MLHandTracking.IsStarted)
            {
                if (poses.Length == 0)
                {
                    success = MLHandTracking.KeyPoseManager.DisableAllKeyPoses();
                    if (!success)
                    {
                        Debug.LogErrorFormat("Error: MLHandTrackingStarterKit.DisablePoses failed because MLHandTracking.KeyPoseManager.DisableAllKeyPoses failed.");
                    }
                    return success;
                }

                success = MLHandTracking.KeyPoseManager.EnableKeyPoses(poses, false);
                if (!success)
                {
                    Debug.LogErrorFormat("Error: MLHandTrackingStarterKit.DisablePoses failed because MLHandTracking.KeyPoseManager.EnableKeyPoses failed.");
                }
            }

            else
            {
                Debug.LogErrorFormat("Error: MLHandTrackingStarterKit.DisablePoses failed because MLHandTracking was not started.");
            }

            #endif

            return success;
        }

        /// <summary>
        /// Stops MLHandTracking if it has been started.
        /// </summary>
        public static void Stop()
        {
            #if PLATFORM_LUMIN
            if (MLHandTracking.IsStarted)
            {
                MLHandTracking.Stop();
            }
            #endif
        }
    }
}

