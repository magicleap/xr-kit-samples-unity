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
using MagicLeap.Core.StarterKit;

namespace MagicLeap.Core
{
    /// <summary>
    /// Base raycast class containing the some common variables and functionality.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/MLRaycastBehavior")]
    public class MLRaycastBehavior : MonoBehaviour
    {
        public enum RaycastDirection
        {
            Forward,
            EyesFixationPoint
        }

        public enum Mode
        {
            World,
            Virtual,
            Combination
        }

        // Certain properties to set _raycastParams with before raycasting into the world
        [Serializable]
        public class WorldRayProperties
        {
            [Tooltip("The number of horizontal rays to cast. Single raycasts set to 1.")]
            public uint width = 1;

            [Tooltip("The number of vertical rays to cast. Single raycasts set to 1.")]
            public uint height = 1;

            [Tooltip("The horizontal field of view in degrees to determine density of Width/Height raycasts.")]
            public float horizontalFovDegrees;

            [Tooltip("If true the ray will terminate when encountering an unobserved area. Useful for determining unmapped areas.")]
            public bool collideWithUnobserved = false;

            // Note: Generated mesh may include noise (bumps). This bias is meant to cover
            // the possible deltas between that and the perception stack results.
            public const float bias = 0.04f;
        }

        // Certain properties we would like the virtual raycast to have
        [Serializable]
        public class VirtualRayProperties
        {
            public float distance = 9f;

            [Tooltip("The layer(s) that will be used for hit detection.")]
            public LayerMask hitLayerMask = new LayerMask();
        }

        public delegate void OnRaycastResultDelegate(MLRaycast.ResultState state, Mode mode, Ray ray, RaycastHit hit, float confidence);
        public event OnRaycastResultDelegate OnRaycastResult;

        [Tooltip("Direction to determine if raycast come from this object's forward vector or use MLEyes fixation point")]
        public RaycastDirection direction;

        [Tooltip("Mode to determine if raycast should be world, virtual, or combinaton.")]
        public Mode mode;

        [Tooltip("Properties to use for a world raycast.")]
        public WorldRayProperties worldRayProperties;

        [Tooltip("Properties to use for a virutal raycast.")]
        public VirtualRayProperties virtualRayProperties;

        #if PLATFORM_LUMIN
        // The parameters used for querying raycasts
        protected MLRaycast.QueryParams _raycastParams = new MLRaycast.QueryParams();
        #endif

        // Used to send to the OnRaycastResult callback
        protected Ray _ray = new Ray();

        // Used to send to the OnRaycastResult callback
        protected RaycastHit _raycastResult;

        // Used inside HandleOnReceiveRaycast to keep track of the mode we were in when raycasting
        protected Mode _modeOnWorldRaycast;

        // Used to wait on the world raycast callback to finish before attempting another world raycast
        protected bool _isReady = true;

        // Used to determine if a callback should be fired or not.
        private bool _isLastResultHit = false;

        /// <summary>
        /// Returns raycast position.
        /// </summary>
        protected Vector3 Position
        {
            get
            {
                return transform.position;
            }
        }

        /// <summary>
        /// Returns raycast direction.
        /// </summary>
        protected Vector3 Direction
        {
            get
            {
                switch (direction)
                {
                    case RaycastDirection.EyesFixationPoint:
                        return (MLEyesStarterKit.FixationPoint - Position).normalized;

                    case RaycastDirection.Forward:
                    default:
                        return transform.forward;
                }
            }
        }

        /// <summary>
        /// Returns raycast up.
        /// </summary>
        protected Vector3 Up
        {
            get
            {
                return transform.up;
            }
        }

        /// <summary>
        /// Starts up component.
        /// </summary>
        protected void Start()
        {
            MLRaycastStarterKit.Start();
            MLEyesStarterKit.Start();
            #if PLATFORM_LUMIN
            #endif
        }

        /// <summary>
        /// Cleans up component.
        /// </summary>
        protected void OnDestroy()
        {
            MLRaycastStarterKit.Stop();
            MLEyesStarterKit.Stop();
        }

        /// <summary>
        /// Deals with what to do when the application is paused
        /// </summary>
        protected void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                _isReady = true;
            }
        }

        /// <summary>
        /// Continuously casts rays
        /// </summary>
        protected void Update()
        {
            if(mode == Mode.Virtual)
            {
                #if PLATFORM_LUMIN
                _raycastParams.Position = Position;
                _raycastParams.Direction = Direction;
                #endif

                CastVirtualRay(virtualRayProperties.distance);
            }
            else
            {
                CastWorldRay();
            }
        }

        /// <summary>
        /// Casts a world ray using _raycastParams
        /// </summary>
        protected void CastWorldRay()
        {
            if (_isReady)
            {
                _isReady = false;

                #if PLATFORM_LUMIN
                _raycastParams.Position = Position;
                _raycastParams.Direction = Direction;
                _raycastParams.UpVector = Up;
                _raycastParams.Width = worldRayProperties.width;
                _raycastParams.Height = worldRayProperties.height;
                _raycastParams.HorizontalFovDegrees = worldRayProperties.horizontalFovDegrees;
                _raycastParams.CollideWithUnobserved = worldRayProperties.collideWithUnobserved;

                _ray.origin = _raycastParams.Position;
                _ray.direction = _raycastParams.Direction;
                #endif

                _modeOnWorldRaycast = mode;

                #if PLATFORM_LUMIN
                if(!MLRaycastStarterKit.Raycast(_raycastParams, HandleOnReceiveRaycast).IsOk)
                {
                    _isReady = true;
                }
                #endif
            }
        }

        /// <summary>
        /// Casts a virtual ray using _raycastParams for origin and direction
        /// </summary>
        protected void CastVirtualRay(float distance)
        {
            #if PLATFORM_LUMIN
            if (Physics.Raycast(_raycastParams.Position, _raycastParams.Direction, out _raycastResult, distance, virtualRayProperties.hitLayerMask))
            {
                _ray.origin = _raycastParams.Position;
                _ray.direction = _raycastParams.Direction;

                OnRaycastResult?.Invoke(MLRaycast.ResultState.HitObserved, Mode.Virtual, _ray, _raycastResult, 1.0f);
            }
            #endif
        }

        /// <summary>
        /// Sets _raycastResult based on results from callback function HandleOnReceiveRaycast.
        /// </summary>
        /// <param name="state"> The state of the raycast result.</param>
        /// <param name="point"> Position of the hit.</param>
        /// <param name="normal"> Normal of the surface hit.</param>
        /// <returns></returns>
        protected void SetWorldRaycastResult(MLRaycast.ResultState state, Vector3 point, Vector3 normal)
        {
            _raycastResult = new RaycastHit();

            if (state != MLRaycast.ResultState.RequestFailed && state != MLRaycast.ResultState.NoCollision)
            {
                _raycastResult.point = point;
                _raycastResult.normal = normal;

                #if PLATFORM_LUMIN
                _raycastResult.distance = Vector3.Distance(_raycastParams.Position, point);
                #endif
            }
        }

        /// <summary>
        /// Callback handler called when raycast call has a result.
        /// </summary>
        /// <param name="state"> The state of the raycast result.</param>
        /// <param name="point"> Position of the hit.</param>
        /// <param name="normal"> Normal of the surface hit.</param>
        /// <param name="confidence"> Confidence value on hit.</param>
        protected void HandleOnReceiveRaycast(MLRaycast.ResultState state, Vector3 point, Vector3 normal, float confidence)
        {
            if((state == MLRaycast.ResultState.HitObserved || state == MLRaycast.ResultState.HitUnobserved) || (state == MLRaycast.ResultState.NoCollision && _isLastResultHit))
            {
                if (_modeOnWorldRaycast == Mode.World)
                {
                    SetWorldRaycastResult(state, point, normal);
                    OnRaycastResult?.Invoke(state, Mode.World, _ray, _raycastResult, confidence);
                }

                if (_modeOnWorldRaycast == Mode.Combination)
                {
                    // If there was a hit on world raycast, change max distance to the hitpoint distance
                    float maxDist = (_raycastResult.distance > 0.0f) ? (_raycastResult.distance + WorldRayProperties.bias) : virtualRayProperties.distance;
                    CastVirtualRay(maxDist);
                }
            }

            _isLastResultHit = (state == MLRaycast.ResultState.HitObserved || state == MLRaycast.ResultState.HitUnobserved);
            _isReady = true;
        }
    }
}
