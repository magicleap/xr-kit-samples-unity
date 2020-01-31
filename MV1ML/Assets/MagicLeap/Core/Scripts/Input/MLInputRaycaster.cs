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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// The raycaster used for the UI canvas interactions.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/Input/MLInputRaycaster")]
    [RequireComponent(typeof(Canvas))]
    public class MLInputRaycaster : BaseRaycaster
    {
        private struct RaycastHitData
        {
            public RaycastHitData(Graphic graphic, Vector3 worldHitPosition, Vector3 worldHitNormal, float distance)
            {
                Graphic = graphic;
                WorldHitPosition = worldHitPosition;
                WorldHitNormal = worldHitNormal;
                Distance = distance;
            }

            public Graphic Graphic { get; set; }
            public Vector3 WorldHitPosition { get; set; }
            public Vector3 WorldHitNormal { get; set; }
            public float Distance { get; set; }
        }

        #region Private Variables
        static readonly List<RaycastHitData> _sortedGraphics = new List<RaycastHitData>();

        [SerializeField]
        private bool _ignoreReversedGraphics = true;

        [SerializeField]
        private GraphicRaycaster.BlockingObjects _blockingObjects = GraphicRaycaster.BlockingObjects.None;

        [SerializeField]
        private LayerMask _blockingMask = -1;

        private Canvas _canvas;

        [NonSerialized]
        private List<RaycastHitData> _raycastResultsCache = new List<RaycastHitData>();
        #endregion

        #region Private Properties
        private Canvas Canvas
        {
            get
            {
                if (_canvas != null)
                    return _canvas;

                _canvas = GetComponent<Canvas>();
                return _canvas;
            }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// When enabled ignores hits on reversed images.
        /// </summary>
        public bool IgnoreReversedGraphics
        {
            get
            {
                return _ignoreReversedGraphics;
            }
            set
            {
                _ignoreReversedGraphics = value;
            }
        }

        /// <summary>
        /// Type of objects that will block graphics raycasts.
        /// </summary>
        public GraphicRaycaster.BlockingObjects BlockingObjects
        {
            get
            {
                return _blockingObjects;
            }
            set
            {
                _blockingObjects = value;
            }
        }

        /// <summary>
        /// Type of objects mask that will block graphic raycasts.
        /// </summary>
        public LayerMask BlockingMask
        {
            get
            {
                return _blockingMask;
            }
            set
            {
                _blockingMask = value;
            }
        }

        /// <summary>
        /// The camera attached to the Canvas, which receives the events.
        /// </summary>
        public override Camera eventCamera
        {
            get
            {
                var myCanvas = Canvas;
                return myCanvas != null ? myCanvas.worldCamera : null;
            }
        }
        #endregion

        #region Private Methods
        private void PerformRaycast(MLInputDeviceEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (Canvas == null)
                return;

            var ray = eventData.Ray;

            Debug.DrawRay(ray.origin, ray.direction, Color.red);

            var hitDistance = eventData.MaxDistance;
            if (BlockingObjects == GraphicRaycaster.BlockingObjects.All || BlockingObjects == GraphicRaycaster.BlockingObjects.ThreeD)
            {
                var hits = Physics.RaycastAll(ray, hitDistance, _blockingMask);

                if (hits.Length > 0 && hits[0].distance < hitDistance)
                {
                    hitDistance = hits[0].distance;
                }
            }

            if (BlockingObjects == GraphicRaycaster.BlockingObjects.All || BlockingObjects == GraphicRaycaster.BlockingObjects.TwoD)
            {
                var raycastDistance = hitDistance;
                var hits = Physics2D.GetRayIntersectionAll(ray, raycastDistance, _blockingMask);

                if (hits.Length > 0 && hits[0].fraction * raycastDistance < hitDistance)
                {
                    hitDistance = hits[0].fraction * raycastDistance;
                }
            }

            _raycastResultsCache.Clear();
            SortedRaycastGraphics(Canvas, ray, _raycastResultsCache);

            // Now that we have a list of sorted hits, process any extra settings and filters.
            for (var i = 0; i < _raycastResultsCache.Count; i++)
            {
                var validHit = true;

                var hitData = _raycastResultsCache[i];

                var go = hitData.Graphic.gameObject;
                if (IgnoreReversedGraphics)
                {
                    var forward = ray.direction;
                    var goDirection = go.transform.rotation * Vector3.forward;
                    validHit = Vector3.Dot(forward, goDirection) > 0;
                }

                validHit &= hitData.Distance < hitDistance;

                if (validHit)
                {
                    var castResult = new RaycastResult
                    {
                        gameObject = go,
                        module = this,
                        distance = hitData.Distance,
                        index = resultAppendList.Count,
                        depth = hitData.Graphic.depth,
                        worldNormal = hitData.WorldHitNormal,
                        worldPosition = hitData.WorldHitPosition
                    };
                    resultAppendList.Add(castResult);
                }
            }
        }

        private void SortedRaycastGraphics(Canvas canvas, Ray ray, List<RaycastHitData> results)
        {
            var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);

            _sortedGraphics.Clear();
            for (int i = 0; i < graphics.Count; ++i)
            {
                Graphic graphic = graphics[i];

                if (graphic.depth == -1)
                {
                    continue;
                }

                Vector3 worldPos;
                Vector3 worldNormal;
                float distance;
                if (RayIntersectsRectTransform(graphic.rectTransform, ray, out worldPos, out worldNormal, out distance))
                {
                    _sortedGraphics.Add(new RaycastHitData(graphic, worldPos, worldNormal, distance));
                }
            }

            _sortedGraphics.Sort((g1, g2) => g2.Graphic.depth.CompareTo(g1.Graphic.depth));

            results.AddRange(_sortedGraphics);
        }

        private bool RayIntersectsRectTransform(RectTransform transform, Ray ray, out Vector3 worldPosition, out Vector3 worldNormal, out float distance)
        {
            var corners = new Vector3[4];
            transform.GetWorldCorners(corners);
            var plane = new Plane(corners[0], corners[1], corners[2]);

            float enter;
            if (plane.Raycast(ray, out enter))
            {
                var intersection = ray.GetPoint(enter);

                var bottomEdge = corners[3] - corners[0];
                var leftEdge = corners[1] - corners[0];
                var bottomDot = Vector3.Dot(intersection - corners[0], bottomEdge);
                var leftDot = Vector3.Dot(intersection - corners[0], leftEdge);

                // If the intersection is right of the left edge and above the bottom edge.
                if (leftDot >= 0 && bottomDot >= 0)
                {
                    var topEdge = corners[1] - corners[2];
                    var rightEdge = corners[3] - corners[2];
                    var topDot = Vector3.Dot(intersection - corners[2], topEdge);
                    var rightDot = Vector3.Dot(intersection - corners[2], rightEdge);

                    //If the intersection is left of the right edge, and below the top edge
                    if (topDot >= 0 && rightDot >= 0)
                    {
                        worldNormal = plane.normal;
                        worldPosition = intersection;
                        distance = enter;

                        return true;
                    }
                }
            }

            worldNormal = Vector3.zero;
            worldPosition = Vector3.zero;
            distance = 0;

            return false;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Raycast from a specific PointerEventData.
        /// </summary>
        /// <param name="eventData">Pointer data to execute raycast from.</param>
        /// <param name="resultAppendList">List to append raycast results to.</param>
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            var trackedEventData = eventData as MLInputDeviceEventData;
            if (trackedEventData != null)
            {
                PerformRaycast(trackedEventData, resultAppendList);
            }
        }
        #endregion
    }
}
